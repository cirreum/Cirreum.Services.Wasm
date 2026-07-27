# Cirreum.Services.Wasm 1.1.0

A minor, and the interesting part is what it *isn't*: despite renaming a type and moving four others
between namespaces, **no public API changed here.** All of them are internal — applications bind to
the interfaces in `Cirreum.Contracts`, not to these implementations.

## What an upgrade actually brings

**The 2.0.0 foundations.** This release re-pins to `Cirreum.Contracts` 2.0.0 and the rest of the
2.0.0 foundation packages. If your application referenced `IPageState`, that is where the change is —
Contracts renamed it to `IBrowserDocumentState`, along with its `PageTitle*` members to
`DocumentTitle*`. See that package's migration guide; nothing in this one needs your attention.

## What changed inside

`PageState` became `BrowserDocumentState`, following its contract. The type governs the browser
*document* hosting the application — title, application name, PWA display mode — none of which are
properties of a Blazor *page*, and "page" already means a routable component in that vocabulary. Its
service-registration description said "Page navigation and routing state", which described something
the type does not do, and now says what it is.

The four feature-state implementations — `ActivityState`, `BrowserDocumentState`,
`NotificationState`, `ThemeState` — moved to the root `Cirreum` namespace, following their contracts.
The machinery stays in `Cirreum.State`: `StateManager`, `StateContainer`, `PersistableStateContainer`,
`RemoteState`, `StateBuilder` and the persistence helpers. That is the same split `Cirreum.Contracts`
applied to the interfaces — features where an application looks for them, plumbing kept separate.

## Fixed

`AuthenticationLibraryType`'s documentation referenced `IdentityProviderType`, removed in
`Cirreum.Kernel` 2.0.0, which would have left a dangling `cref` once this package re-pinned. Its
remarks now state the distinction directly: the type names the client-side *library*, not the identity
provider behind it, since several providers are reached through the same library. For the provider
itself, `UserProfile.Issuer` identifies it exactly.

## Compatibility

- No public type, method, or signature in this package changed
- Requires the 2.0.0 foundation packages, pulled transitively
- Registration, state persistence, and runtime behaviour are unchanged

## See also

- [`docs/CHANGELOG.md`](CHANGELOG.md)
- `Cirreum.Contracts` 2.0.0 migration guide — for `IPageState` → `IBrowserDocumentState`
