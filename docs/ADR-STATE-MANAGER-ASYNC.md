# ADR: Dual-Path State Notification System

**Status:** Accepted  
**Date:** 2026  
**Packages Affected:** `Cirreum.Core`, `Cirreum.Services.Wasm`, `Cirreum.Runtime.Wasm`

---

## Context

Cirreum's `IStateManager` originally supported only synchronous subscriber notification via `Action<TState>` delegates. As the framework matured, state types began requiring async work in their notification handlers — persistence to browser storage, app user hydration, navigation after authentication. The sync path was papered over with `Task.Run` in `CommonClaimsPrincipalFactory`, which moved notification off the Blazor synchronization context and caused subscribers to need `InvokeAsync` wrapping to safely update UI state.

Two problems needed solving:

1. **Threading** — `Task.Run` breaks the Blazor sync context, pushing the problem onto every subscriber.
2. **Alignment** — no compile-time signal telling consumers which notification path a state type uses, leading to silent misses where subscribers registered on the wrong path and never received notifications.

---

## Decision

### 1. Two Separate Notification Lists — Not One Unified Async Path

Sync and async subscribers are maintained in completely separate dictionaries. `NotifySubscribers` fires only sync subscribers. `NotifySubscribersAsync` fires only async subscribers. They never interleave.

**Why not collapse to a single async path?**

In Blazor WASM, JavaScript runs on the same thread as .NET. This enables direct synchronous JS interop calls with zero task scheduling overhead — no marshaling, no `ValueTask`, no thread switches. This is a fundamental runtime capability, not a convenience. Many state types in Cirreum (`ThemeState`, `PageState`, `UserPresenceState`, `DynamicAuthInterop`) drive synchronous JS calls that depend on this characteristic.

Collapsing to a single async path would force those sync JS calls through unnecessary task machinery, introducing latency in UI interactions and eliminating a key performance advantage of the WASM hosting model. The two lists are load-bearing for WASM's performance model.

### 2. Compile-Time Enforcement via Marker Interfaces

```
IApplicationState          ← all state types
IAsyncApplicationState     ← state types that notify asynchronously
    extends IApplicationState
```

`SubscribeAsync` and `NotifySubscribersAsync` are constrained to `IAsyncApplicationState`. `Subscribe` and `NotifySubscribers` are constrained to `IApplicationState`. The compiler enforces alignment — wrong marker, wrong path, won't build.

This eliminates the silent miss problem entirely. A consumer cannot accidentally subscribe async to a sync state or vice versa.

### 3. IClientUserState for WASM-Specific Async User State

`IUserState` is shared across all three hosting environments (WASM, Server, Serverless). Server and Serverless resolve user state per-request via `IUserStateAccessor` — no notification concern at all. Making `IUserState` implement `IAsyncApplicationState` would be a misleading contract for those environments.

The async notification contract for user state is WASM-specific and lives in a WASM-specific interface:

```csharp
// Cirreum.Runtime.Wasm
public interface IClientUserState : IUserState, IAsyncApplicationState { }
```

`ClientUser` implements `IClientUserState`. WASM consumers subscribe via `SubscribeAsync<IClientUserState>`. Server and Serverless never reference `IClientUserState`.

### 4. Deferred Notification in CommonClaimsPrincipalFactory

`CreateUserAsync` is Blazor's entry point for building a `ClaimsPrincipal`. The framework awaits it before proceeding. Work inside it blocks the auth pipeline.

The factory splits into two phases:

**Phase 1 — inline, blocking, critical:**
```csharp
await MapIdentityAsync(identity, account);    // builds ClaimsPrincipal
await ExtendClaimsAsync(identity, account);   // enriches claims
UpdatePrincipalTracking(userPrincipal);
// CreateUserAsync returns here — Blazor unblocked
```

**Phase 2 — deferred, non-blocking, best effort:**
```csharp
_ = RunPostAuthAsync(serviceProvider, stateManager, userPrincipal);

private async Task RunPostAuthAsync(...) {
    await Task.Yield();                          // release CreateUserAsync first
    clientUser.SetAuthenticatedPrincipal(...);
    await RunPostProcessors(clientUser);         // DB call — app user load
    await stateManager.NotifySubscribersAsync(clientUser); // bridge to subscribers
}
```

`Task.Yield` is correct here — it releases `CreateUserAsync` to complete before post-auth work begins. Blazor gets its `ClaimsPrincipal` with zero additional latency. App user hydration and state notification happen on the next turn.

### 5. ScopedNotificationState — Scope vs NotifyStateChanged

`ScopedNotificationState` provides two mechanisms:

- `NotifyStateChanged()` / `NotifyStateChangedAsync()` — for single-mutation methods
- `CreateNotificationScope()` / `CreateNotificationScopeAsync()` — for methods that perform multiple mutations

**Rule:** Use `NotifyStateChanged` in methods that own a single mutation. Use `CreateNotificationScope` in methods that own multiple mutations and want them treated as a single notification.

```csharp
// Single mutation — NotifyStateChanged
public void SetPresence(UserPresence presence) {
    this._presence = presence;
    this.NotifyStateChanged();
}

// Multiple mutations — CreateNotificationScope
public void Reset() {
    using var _ = this.CreateNotificationScope();
    this._presence = new(PresenceStatus.Unknown, "", "");
    this._lastSeen = DateTimeOffset.MinValue;
}
```

**Do not** wrap single-mutation methods in `CreateNotificationScope` as a defensive measure. Doing so breaks callers who legitimately use `CreateNotificationScope` to batch multiple method calls — each method's internal scope completes and fires its own notification, defeating the batching entirely.

If a caller wants to suppress intermediate notifications across multiple method calls, that is **their** responsibility:

```csharp
using var _ = presenceState.CreateNotificationScope();
presenceState.SetPresence(newPresence);
presenceState.SetLastSeen(now);
// single notification fires here
```

### 6. NotifySubscribers — Parameterless vs Instance Overload

Both sync and async notify paths provide two overloads:

```csharp
void NotifySubscribers<TState>()              // pulls instance from DI
void NotifySubscribers<TState>(TState state)  // explicit instance

Task NotifySubscribersAsync<TState>(...)              // pulls from DI
Task NotifySubscribersAsync<TState>(TState state, ...) // explicit instance
```

**Prefer the parameterless overload** for convenience when you've mutated state in place and the DI-registered instance is the correct one to broadcast.

**Use the instance overload** when:
- You're inside the state class itself calling `OnStateHasChanged` — pass `this` to guarantee the mutated instance is what subscribers receive, not a potentially different DI-resolved instance
- Casting or derived type scenarios where the concrete type differs from the registered DI type

---

## Consequences

### Positive

- Compile-time enforcement eliminates silent miss bugs
- Blazor sync context is never broken — no `InvokeAsync` needed in subscribers
- `App.razor` and other subscribers are simpler — no threading ceremony
- Performance of sync JS interop path is preserved
- Clean separation of concerns — factory builds identity, state manager bridges to app logic

### Negative

- Two parallel code paths in `StateManager` to maintain
- Consumers must know which marker interface their state uses — mitigated by compiler errors and XML docs
- `IClientUserState` adds an extra interface in the WASM layer — minor overhead

### Neutral

- Existing sync subscribers are entirely unaffected — zero migration burden
- Server and Serverless hosting environments are unaffected — `IStateManager` is not registered there

---

## Reference

**Interfaces changed or added:**
- `Cirreum.Core` — `IAsyncApplicationState` (new), `IStateManager` (async overloads), `ScopedNotificationState` (async hooks)
- `Cirreum.Services.Wasm` — `StateManager` (async subscriber list + notify implementation)
- `Cirreum.Runtime.Wasm` — `CommonClaimsPrincipalFactory` (Phase 1/2 split, `Task.Yield` deferred notify), `IClientUserState` (new)
