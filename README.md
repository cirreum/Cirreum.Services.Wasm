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

A synchronous notification system designed around Blazor WASM's unique runtime characteristics. In WASM, JavaScript runs on the same thread as .NET, enabling direct synchronous JS interop with zero task scheduling overhead.

All state types implement `IApplicationState`. Subscribe to changes and notify subscribers through `IStateManager`:

```csharp
// Subscribe to state changes
stateManager.Subscribe<IThemeState>(state => jsModule.ApplyTheme(state.Current));

// Notify subscribers after mutation
stateManager.NotifySubscribers<IThemeState>();
```

**State hierarchy:**

```
ScopedNotificationState           ← batched notification base class
    RemoteState                   ← async data loading with load/refresh lifecycle
    StateContainer                ← key/value storage with encryption support
        PersistableStateContainer ← adds JSON serialization/deserialization
            LocalState            ← browser localStorage backed
            SessionState          ← browser sessionStorage backed
```

### 🔄 Remote State & Initialization

`RemoteState` is the base class for client-side state that loads data from backend services. It manages loading/refreshing lifecycle with guard checks and integrates with the notification system.

```csharp
public class ProductsState(
    IProductApi api,
    IStateManager stateManager
) : RemoteState, IProductsState {

    public IReadOnlyList<Product> Products { get; private set; } = [];

    protected override async Task LoadCoreAsync(CancellationToken cancellationToken) {
        Products = await api.GetAllAsync(cancellationToken);
    }

    protected override void OnStateHasChanged() {
        stateManager.NotifySubscribers<IProductsState>(this);
    }
}
```

Registration uses `RegisterRemoteState` on the state builder, which automatically wires up `IInitializable` discovery for startup initialization:

```csharp
services.AddClientState(state => {
    state.RegisterRemoteState<IProductsState, ProductsState>();
});
```

The `IInitializationOrchestrator` coordinates startup, running all `IInitializable` services in order and reporting progress through `IInitializationState` for splash screens and loading indicators.

### 📋 Initialization State

`IInitializationState` tracks application startup progress with deterministic task counting and error collection. It enables splash screens and loading indicators to display meaningful progress.

```csharp
// Subscribe to initialization progress
stateManager.Subscribe<IInitializationState>(state => {
    progressBar.Value = state.CompletedTasks;
    progressBar.Max = state.TotalTasks;
    statusLabel.Text = state.DisplayStatus;
});
```

Key properties: `IsInitializing`, `TotalTasks`, `CompletedTasks`, `DisplayStatus`, `HasErrors`, `Errors`. Errors are captured as `InitializationError` records with store name, exception details, and timestamp — allowing the UI to surface partial failures without blocking the entire startup pipeline.

### 🔔 Notification State

`INotificationState` provides in-app notification management with read/dismiss semantics. Notifications are stored newest-first and support soft dismissal (hidden but retained) and hard removal.

```csharp
// Add a notification
notificationState.AddNotification(
    Notification.Create("Deployment complete", "v2.1 deployed to production", NotificationType.Success)
);

// React to notification changes
stateManager.Subscribe<INotificationState>(state => {
    badge.Count = state.UnreadCount;
});
```

Operations: `AddNotification`, `MarkAsRead` / `MarkAllAsRead`, `Dismiss` / `DismissAll`, `RemoveNotification`, `ClearAll`. The `Notifications` property automatically filters dismissed items, while `UnreadCount` reflects only unread, non-dismissed notifications.

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

Activity detection through DOM events and HTTP call interception with configurable throttling. Drives `IUserPresenceState` notifications via the JS interop path.

### 🔐 Security

Claims-based user context management and authentication state integration for WASM clients.

---

## Registration

```csharp
// Register all core client state services with defaults
builder.AddClientState();

// Or with custom configuration
builder.AddClientState(state => {
    state.RegisterRemoteState<IProductsState, ProductsState>();
    state.RegisterDecryptor(StateEncryptionKinds.CUSTOM, new CustomDecryptor());
});
```

**Built-in services registered by `AddClientState`:**

| Service | Purpose |
|---------|---------|
| `IStateManager` | Core state management and subscriber notification |
| `IInitializationState` | Startup progress tracking and error collection |
| `INotificationState` | In-app notification management |
| `IThemeState` | Application theme state |
| `IPageState` | Page title and navigation metadata |
| `IUserPresenceState` | User activity detection |
| `IMemoryState` | In-memory state container |
| `ISessionState` | Browser `sessionStorage` backed container |
| `ILocalState` | Browser `localStorage` backed container |

---

## Architecture

### State Notification Design

The `StateManager` maintains a subscriber dictionary of `Action<TState>` delegates, with version-tracked caching for efficient subscriber list retrieval. Source-generated logging (`[LoggerMessage]`) is used throughout for zero-overhead log filtering.

```
Subscribers → _subscribers dict → notified by NotifySubscribers
```

### ScopedNotificationState

`ScopedNotificationState` is the base class for all state that batches notifications. Two mechanisms are provided:

| Mechanism | Use When |
|-----------|----------|
| `NotifyStateChanged()` | Single mutation — one property changed |
| `CreateNotificationScope()` | Multiple mutations — batch into one notification |

Never wrap single-mutation methods in `CreateNotificationScope`. Callers use scopes to batch multiple method calls — internal scopes break that pattern.

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

## Documentation

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
