# CLAUDE.md — Cirreum.Services.Wasm

This file provides architectural context for AI-assisted development in this repository.
Read this before making changes to state management, notification, or authentication code.

---

## Repository Role

`Cirreum.Services.Wasm` provides the Blazor WASM implementations of core Cirreum infrastructure
services. It sits in the **Infrastructure** layer:

```
Base → Common → Core → Infrastructure → Runtime → Runtime Extensions
```

This package can reference `Cirreum.Core` and below. It cannot reference Runtime packages.

---

## State Management — Critical Context

### Two Notification Paths

`StateManager` maintains **two separate subscriber lists** — sync and async. They are not
interchangeable. This is a deliberate design decision load-bearing for WASM performance.

**Why two paths exist:**
In Blazor WASM, JavaScript runs on the same thread as .NET. This enables synchronous JS interop
calls with zero task scheduling overhead. Many state types drive sync JS calls (theme, page state,
presence, auth interop). Collapsing to a single async path would force those calls through
unnecessary task machinery and eliminate a key WASM performance advantage.

```
Sync path  → NotifySubscribers    → fires Action<TState> subscribers
Async path → NotifySubscribersAsync → fires Func<TState,Task> subscribers
```

**They never interleave. Registering on the wrong path = silent miss.**

### Marker Interface Enforcement

```csharp
IApplicationState        // sync state — use Subscribe / NotifySubscribers
IAsyncApplicationState   // async state — use SubscribeAsync / NotifySubscribersAsync
```

The compiler enforces this via generic type constraints on `IStateManager`. Wrong marker =
compile error. Do not remove or relax these constraints.

### Subscribe vs SubscribeAsync

```csharp
// Sync — JS interop, in-memory UI, theme, page state
stateManager.Subscribe<IThemeState>(state => jsModule.ApplyTheme(state.Current));

// Async — persistence, navigation, app user hydration
// IUserState must implement IAsyncApplicationState
stateManager.SubscribeAsync<IUserState>(async state => {
    await storage.SetAsync("userId", state.Id);
    navigation.NavigateTo(Routes.Dashboard);
});
```

### NotifySubscribers — Which Overload

```csharp
// Parameterless — pulls instance from DI, use for external callers
stateManager.NotifySubscribers<IThemeState>();

// Instance overload — use inside OnStateHasChanged, casting scenarios
protected override void OnStateHasChanged() {
    stateManager.NotifySubscribers<IMyState>(this); // always pass `this` here
}
```

Always pass `this` inside `OnStateHasChanged` / `OnStateHasChangedAsync`. DI may not resolve
the same instance that was mutated, especially in testing or non-singleton registrations.

---

## ScopedNotificationState — Scope vs NotifyStateChanged

**Single mutation methods** → use `NotifyStateChanged()`:
```csharp
public void SetValue(string value) {
    this._value = value;
    this.NotifyStateChanged(); // correct
}
```

**Multi-mutation methods** → use `CreateNotificationScope()`:
```csharp
public void Reset() {
    using var _ = this.CreateNotificationScope();
    this._value = "";
    this._timestamp = null;
    // single notification on scope exit
}
```

**Never** wrap single-mutation methods in `CreateNotificationScope` defensively. It breaks
callers who legitimately batch multiple calls:

```csharp
// This breaks if SetA and SetB both use internal scopes
using var _ = state.CreateNotificationScope();
state.SetA(a);
state.SetB(b); // fires TWO notifications instead of one
```

---

## IClientUserState

`IUserState` is shared across WASM, Server, and Serverless. Server/Serverless resolve user
state per-request — no notification concern. Making `IUserState` implement
`IAsyncApplicationState` would be wrong for those environments.

The WASM-specific async notification contract lives in:

```csharp
// Cirreum.Runtime.Wasm
public interface IClientUserState : IUserState, IAsyncApplicationState { }
```

In WASM:
- Read user data → resolve `IUserState`
- Subscribe to auth changes → use `IClientUserState`
- Mutate internal state → cast to `ClientUser` (internal)
- Notify → `stateManager.NotifySubscribersAsync<IClientUserState>(clientUser)`

---

## StateManager Implementation Notes

- Two dictionaries: `_subscribers` (sync) and `_asyncSubscribers` (async)
- Two cache dictionaries mirroring the above with version tracking
- `GetCachedSubscribers(type, sync: bool)` routes to the correct list
- `IncrementVersion(type, sync: bool)` invalidates the correct cache
- Source-generated logging via `[LoggerMessage]` in nested `static partial class Log`
- `Interlocked` used for `_scopeCount` in `ScopedNotificationState` — thread-safe for Server

---

## What Lives Where

| Concern | Package |
|---------|---------|
| `IApplicationState`, `IAsyncApplicationState`, `IStateManager` | `Cirreum.Core` |
| `ScopedNotificationState`, `StateContainer`, `StateManager` | `Cirreum.Services.Wasm` (this repo) |
| `IClientUserState`, `CommonClaimsPrincipalFactory` | `Cirreum.Runtime.Wasm` |
| `MsalClaimsPrincipalFactory` | `Cirreum.Runtime.Wasm.Msal` |

---

## See Also

- `docs/ADR-STATE-MANAGER-ASYNC.md` — full decision record with rationale
- `Cirreum.Core` README — state management section
- `IStateManager` XML docs — inline guidance on path selection
