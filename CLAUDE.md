# CLAUDE.md — Cirreum.Services.Wasm

This file provides architectural context for AI-assisted development in this repository.
Read this before making changes to state management, notification, or session code.

---

## Repository Role

`Cirreum.Services.Wasm` provides the Blazor WASM implementations of core Cirreum
infrastructure services. It sits in the **Infrastructure** layer:

```
Base → Common → Core → Infrastructure → Runtime → Runtime Extensions
```

This package references `Cirreum.Domain` and below (plus `Cirreum.Startup` and
`Cirreum.Storage.Browser`). It cannot reference Runtime packages. When
`UseLocalComponents=true`, the `Cirreum.Domain` package reference switches to a
project reference at `Core/Cirreum.Domain`.

### Registration surface

- `AddCoreServices()` — the core WASM services (user accessor, clock, storage, file system)
- `AddClientState(...)` — state container / ViewModel registration
- `AddSessionMonitoring(...)` — session stage tracking + expiration handling

### What's here

- **State/** — `StateManager` (the `IStateManager` implementation), `StateContainer`,
  `PersistableStateContainer`, the Memory/Session/Local/Remote/Page/Theme/Activity
  state family, and persistence helpers
- **Authorization/** — `SessionManager` + session stages, `SessionHttpHandler`,
  `AuthorizationRoleRegistry`
- **Presence/** — user presence monitor/service/state
- **Storage/** — local/session storage services over `Cirreum.Storage.Browser`
- **Security/** — the WASM `UserAccessor`
- **FileSystem/**, **Clock/**, **Components.Interop/** — browser-appropriate
  implementations

---

## State Management — Critical Context

### Notification System

`StateManager` maintains a subscriber dictionary of `Action<TState>` delegates. All state types
implement `IApplicationState`. Subscribers are notified synchronously via `NotifySubscribers`.

In Blazor WASM, JavaScript runs on the same thread as .NET. This enables synchronous JS interop
calls with zero task scheduling overhead — the synchronous notification path is load-bearing
for WASM performance.

```
Subscribe → registers Action<TState> subscriber
NotifySubscribers → fires all subscribers for that state type
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

Always pass `this` inside `OnStateHasChanged`. DI may not resolve
the same instance that was mutated, especially in testing or non-singleton registrations.

---

## ScopedNotificationState — Scope vs NotifyStateChanged

(`ScopedNotificationState` itself lives in `Cirreum.Domain`; the state types in this
repo build on it, so the usage rules matter here.)

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

## StateManager Implementation Notes

- Single subscriber dictionary: `_subscribers` with `Action<TState>` delegates
- Version-tracked caching for efficient subscriber list retrieval
- Source-generated logging via `[LoggerMessage]` in nested `static partial class Log`

---

## What Lives Where

| Concern | Package |
|---------|---------|
| `IApplicationState` | `Cirreum.Kernel` |
| `IStateManager` | `Cirreum.Contracts` |
| `ScopedNotificationState` | `Cirreum.Domain` |
| `StateManager`, `StateContainer`, the state family | `Cirreum.Services.Wasm` (this repo) |
| `CommonClaimsPrincipalFactory` | `Cirreum.Runtime.Wasm` |
| `MsalClaimsPrincipalFactory` | `Cirreum.Runtime.Wasm.Msal` (Runtime Extensions) |
| `OidcClaimsPrincipalFactory` | `Cirreum.Runtime.Wasm.Oidc` (Runtime Extensions) |

---

## See Also

- `Cirreum.Contracts` — `IStateManager` XML docs (subscription and notification guidance)
- `Cirreum.Domain` — `ScopedNotificationState`
- `Cirreum.Runtime.Wasm` — the WASM runtime client that composes these services
