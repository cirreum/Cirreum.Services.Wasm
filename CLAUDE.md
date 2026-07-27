# CLAUDE.md — Cirreum.Services.Wasm

Context for editing this repository. Read it before touching state, notification, or session
code — it records the traps, not the API.

For what this package *is* and what it registers, see [README.md](README.md). For what changed
and why, see [docs/CHANGELOG.md](docs/CHANGELOG.md).

---

## Namespace split: feature states versus machinery

Mirrors the split `Cirreum.Contracts` applies to the interfaces. Put a new type on the right side
of it:

| | Namespace | Files |
|---|---|---|
| **Feature states** — what an application consumes | `Cirreum` (project root) | `ActivityState`, `BrowserDocumentState`, `NotificationState`, `ThemeState` |
| **Machinery** — what state types are built on | `Cirreum.State` (`State/`) | `StateManager`, `StateContainer`, `PersistableStateContainer`, `RemoteState`, `StateBuilder`, the Memory/Session/Local containers, persistence and serialization helpers |

Feature states keep `using Cirreum.State;` because `ScopedNotificationState` — their base, and
machinery — correctly stayed behind. That is expected, not an oversight.

The rule of thumb: if an application injects it by name, it belongs at the root. If an application
only ever derives from or configures it, it belongs in `Cirreum.State`.

---

## Notification traps

### Synchronous by design

`StateManager` notifies subscribers **synchronously**. In Blazor WebAssembly, JavaScript runs on
the same thread as .NET, so synchronous JS interop costs nothing in task scheduling — the
synchronous path is load-bearing for performance here, not an oversight to be "fixed" with async.

### `NotifySubscribers` — which overload

```csharp
// Parameterless — resolves from DI. For external callers.
stateManager.NotifySubscribers<IThemeState>();

// Instance overload — inside OnStateHasChanged, always.
protected override void OnStateHasChanged() {
    stateManager.NotifySubscribers<IMyState>(this);   // pass `this`
}
```

**Always pass `this` inside `OnStateHasChanged`.** DI may not resolve the same instance that was
just mutated — especially under testing or non-singleton registration — so the parameterless
overload can notify subscribers about a *different* object than the one that changed.

### `CreateNotificationScope` — do not apply defensively

`ScopedNotificationState` lives in `Cirreum.Domain`; these rules govern the state types here that
build on it.

**Single mutation** → `NotifyStateChanged()`:

```csharp
public void SetValue(string value) {
    this._value = value;
    this.NotifyStateChanged();
}
```

**Multiple mutations in one method** → `CreateNotificationScope()`, which coalesces to a single
notification on exit:

```csharp
public void Reset() {
    using var _ = this.CreateNotificationScope();
    this._value = "";
    this._timestamp = null;
}
```

**Never wrap a single-mutation method in a scope "just in case."** It breaks callers who batch:

```csharp
using var _ = state.CreateNotificationScope();
state.SetA(a);
state.SetB(b);   // fires TWICE if SetA and SetB each open their own scope
```

The caller owns the batching decision. A method that mutates once should notify once.

---

## StateManager implementation notes

- One subscriber dictionary, `_subscribers`, holding `Action<TState>` delegates
- Version-tracked caching for subscriber-list retrieval
- Source-generated logging via `[LoggerMessage]` in the nested `static partial class Log`

---

## Cross-package boundaries

| Concern | Lives in |
|---|---|
| `IApplicationState` | `Cirreum.Kernel` |
| `IStateManager`, the feature state contracts | `Cirreum.Contracts` |
| `ScopedNotificationState` | `Cirreum.Domain` |
| `CommonClaimsPrincipalFactory` | `Cirreum.Runtime.Wasm` |
| `MsalClaimsPrincipalFactory` / `OidcClaimsPrincipalFactory` | `Cirreum.Runtime.Wasm.Msal` / `.Oidc` |

This package is Infrastructure: it references `Cirreum.Domain` and below, plus `Cirreum.Startup`
and `Cirreum.Storage.Browser`. **It cannot reference Runtime packages.** Under
`UseLocalComponents=true`, the `Cirreum.Domain` package reference becomes a project reference at
`Core/Cirreum.Domain`.
