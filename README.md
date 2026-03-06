# Cirreum.Services.Wasm

[![NuGet Version](https://img.shields.io/nuget/v/Cirreum.Services.Wasm.svg?style=flat-square&labelColor=1F1F1F&color=003D8F)](https://www.nuget.org/packages/Cirreum.Services.Wasm/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Cirreum.Services.Wasm.svg?style=flat-square&labelColor=1F1F1F&color=003D8F)](https://www.nuget.org/packages/Cirreum.Services.Wasm/)
[![GitHub Release](https://img.shields.io/github/v/release/cirreum/Cirreum.Services.Wasm?style=flat-square&labelColor=1F1F1F&color=FF3B2E)](https://github.com/cirreum/Cirreum.Services.Wasm/releases)
[![License](https://img.shields.io/github/license/cirreum/Cirreum.Services.Wasm?style=flat-square&labelColor=1F1F1F&color=F2F2F2)](https://github.com/cirreum/Cirreum.Services.Wasm/blob/main/LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-003D8F?style=flat-square&labelColor=1F1F1F)](https://dotnet.microsoft.com/)

**Foundational services infrastructure for WebAssembly client applications**

## Overview

**Cirreum.Services.Wasm** provides the standard browser/WebAssembly runtime implementations of core Cirreum infrastructure services. It delivers WebAssembly-specific implementations for state management, session handling, browser storage integration, and user activity monitoring designed specifically for Blazor WebAssembly applications.

This package sits in the **Infrastructure** layer of the Cirreum stack:

```
Base → Common → Core → Infrastructure → Runtime → Runtime Extensions
                            ↑
                  Cirreum.Services.Wasm
```

## Key Features

### 📡 State Management

A dual-path notification system designed around Blazor WASM's unique runtime characteristics.

In WASM, JavaScript runs on the same thread as .NET, enabling direct synchronous JS interop with zero task scheduling overhead. This makes the sync notification path essential — not optional — for state that drives JS calls, theme updates, or in-memory UI state.

**Two paths, compile-time enforced:**

```csharp
// Sync — JS interop, in-memory UI, theme, page state
// State interface must implement IApplicationState
stateManager.Subscribe<IThemeState>(state => jsModule.ApplyTheme(state.Current));
stateManager.NotifySubscribers<IThemeState>();

// Async — persistence, navigation, app user hydration
// State interface must implement IAsyncApplicationState
stateManager.SubscribeAsync<IUserState>(async state => {
    await storage.SetAsync("userId", state.Id);
    navigation.NavigateTo(Routes.Dashboard);
});
await stateManager.NotifySubscribersAsync<IUserState>();
```

Registering on the wrong path results in a **compile error**, not a silent miss. See `IStateManager` XML docs and `docs/ADR-STATE-MANAGER-ASYNC.md` for full guidance.

**State container hierarchy:**

```
ScopedNotificationState           ← batched notification base class
    StateContainer                ← key/value storage with encryption support
        PersistableStateContainer ← adds JSON serialization/deserialization
            LocalState            ← browser localStorage backed
            SessionState          ← browser sessionStorage backed
```

### 🔒 Session Management

Sophisticated lifecycle management with configurable timeout stages and activity monitoring:

- **SafeZone** (0–90% of timeout) — minimal monitoring, low overhead
- **WatchZone** (90–100%) — active monitoring with debounced session extension

```csharp
var sessionManager = serviceProvider.GetRequiredService<ISessionManager>();
sessionManager.SessionStageChanged += stage => {
    if (stage == SessionStage.WatchZone) {
        ShowSessionExpiryWarning();
    }
};
```

### 🗄️ Browser Storage

Local and session storage abstractions with WebAssembly file system support:

- `ILocalStorageService` — browser `localStorage`
- `ISessionStorageService` — browser `sessionStorage`
- WebAssembly file system integration for client-side file operations

### 👤 User Presence

Activity detection through DOM events and HTTP call interception with configurable throttling. Drives `IUserPresenceState` sync notifications via the JS interop path.

### 🔐 Security

Content Security Policy builder and role-based authorization support for WASM clients.

---

## Registration

```csharp
// Register all core services
builder.Services.AddCoreServices(storage => {
    // Optional storage configuration
});
```

---

## Architecture

### State Notification Design

The `StateManager` implementation maintains two separate subscriber dictionaries — one for sync (`Action<TState>`) and one for async (`Func<TState, Task>`) delegates. They are never mixed or interleaved.

```
Sync subscribers  → _subscribers dict      → notified by NotifySubscribers
Async subscribers → _asyncSubscribers dict → notified by NotifySubscribersAsync
```

Version-tracked caching is applied to both lists independently. Source-generated logging (`[LoggerMessage]`) is used throughout for zero-overhead log filtering.

### ScopedNotificationState

`ScopedNotificationState` is the base class for all state that batches notifications. Two mechanisms are provided:

| Mechanism | Use When |
|-----------|----------|
| `NotifyStateChanged()` | Single mutation — one property changed |
| `CreateNotificationScope()` | Multiple mutations — batch into one notification |

Never wrap single-mutation methods in `CreateNotificationScope`. Callers use scopes to batch multiple method calls — internal scopes break that pattern. See `docs/ADR-STATE-MANAGER-ASYNC.md` for details.

### State Container Encryption

`StateContainer` supports pluggable encryption for persisted values. Encryption is applied to the entire serialized JSON blob before storage. Algorithm IDs are appended to ciphertext to support migration between encryption schemes via keyed DI services.

---

## Contribution Guidelines

1. **Be conservative with new abstractions**  
   The API surface must remain stable and meaningful.

2. **Limit dependency expansion**  
   Only add foundational, version-stable dependencies.

3. **Favor additive, non-breaking changes**  
   Breaking changes ripple through the entire ecosystem.

4. **Include thorough unit tests**  
   All primitives and patterns should be independently testable.

5. **Document architectural decisions**  
   Context and reasoning should be clear for future maintainers.

6. **Follow .NET conventions**  
   Use established patterns from Microsoft.Extensions.* libraries.

7. **Respect the two-path notification model**  
   Do not merge sync and async subscriber lists. The separation is load-bearing
   for WASM performance. See `docs/ADR-STATE-MANAGER-ASYNC.md`.

## Documentation

- [State Manager ADR](docs/ADR-STATE-MANAGER-ASYNC.md) — full rationale for dual-path notification design
- [CLAUDE.md](CLAUDE.md) — AI-assisted development context

## Versioning

Cirreum.Services.Wasm follows [Semantic Versioning](https://semver.org/):

- **Major** - Breaking API changes
- **Minor** - New features, backward compatible
- **Patch** - Bug fixes, backward compatible

Given its foundational role, major version bumps are rare and carefully considered.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

**Cirreum Foundation Framework**  
*Layered simplicity for modern .NET*
