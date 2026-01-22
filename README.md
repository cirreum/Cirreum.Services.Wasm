# Cirreum.Services.Wasm

[![NuGet Version](https://img.shields.io/nuget/v/Cirreum.Services.Wasm.svg?style=flat-square&labelColor=1F1F1F&color=003D8F)](https://www.nuget.org/packages/Cirreum.Services.Wasm/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Cirreum.Services.Wasm.svg?style=flat-square&labelColor=1F1F1F&color=003D8F)](https://www.nuget.org/packages/Cirreum.Services.Wasm/)
[![GitHub Release](https://img.shields.io/github/v/release/cirreum/Cirreum.Services.Wasm?style=flat-square&labelColor=1F1F1F&color=FF3B2E)](https://github.com/cirreum/Cirreum.Services.Wasm/releases)
[![License](https://img.shields.io/github/license/cirreum/Cirreum.Services.Wasm?style=flat-square&labelColor=1F1F1F&color=F2F2F2)](https://github.com/cirreum/Cirreum.Services.Wasm/blob/main/LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-003D8F?style=flat-square&labelColor=1F1F1F)](https://dotnet.microsoft.com/)

**Foundational services infrastructure for WebAssembly client applications**

## Overview

**Cirreum.Services.Wasm** provides essential infrastructure services specifically designed for Blazor WebAssembly applications. It delivers state management, session handling, browser storage integration, and user activity monitoring with a focus on WebAssembly-specific requirements.

### Key Features

- **State Management**: Thread-safe application state with subscriber notifications and intelligent caching
- **Session Management**: Sophisticated lifecycle management with configurable timeout stages and activity monitoring
- **Browser Integration**: Local and session storage abstractions with WebAssembly file system support
- **User Presence**: Activity detection through DOM events and HTTP calls with configurable throttling
- **Security**: Content Security Policy builder and role-based authorization support

### Core Services

```csharp
// Register all core services
services.AddCoreServices(storage => {
    // Optional storage configuration
});

// State management
var stateManager = serviceProvider.GetRequiredService<IStateManager>();
var userState = stateManager.Get<IUserState>();

// Session management with configurable stages
var sessionManager = serviceProvider.GetRequiredService<ISessionManager>();
sessionManager.SessionStageChanged += (stage) => {
    // React to SafeZone/WatchZone transitions
};

// Browser storage
var localStorage = serviceProvider.GetRequiredService<ILocalStorageService>();
await localStorage.SetItemAsync("key", value);
```

## Architecture

The library follows the Cirreum Foundation Framework pattern with layered simplicity:

- **State Layer**: `StateManager`, `StateContainer`, specialized state types
- **Session Layer**: `SessionManager` with stage-based timeout handling
- **Storage Layer**: Browser storage abstractions and WebAssembly file system
- **Infrastructure Layer**: Clock services, CSV utilities, CSP management

Session management uses a two-stage approach: SafeZone (0-90% of timeout) for minimal monitoring, and WatchZone (90-100%) for active monitoring with debounced session extension.

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