# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Updated

- Updated NuGet packages (Cirreum spine 4.2.0 wave: `Cirreum.Contracts` 4.2.0 / `Cirreum.Domain` 4.2.0 and current patch releases).

## [1.1.2] - 2026-07-31

### Updated

- Updated NuGet packages (Cirreum spine 4.0.1 wave: `Cirreum.Contracts` 4.0.1 / `Cirreum.Domain` 4.0.1 / `Cirreum.Kernel` 2.0.1 / `Cirreum.AuthenticationProvider` 2.0.3).

## [1.1.1] - 2026-07-30

### Updated

- Re-pinned `Cirreum.Domain` `2.0.0` → `3.0.0` — restores operation-authorization enforcement
  (the fail-open intercept fix shipped in Domain 2.0.1/3.0.0) and adopts the `IPolicyAuthorizer`
  vocabulary; see Cirreum.Domain `MIGRATION-v3.md`.

## [1.1.0] - 2026-07-27

### Changed

- **`PageState` → `BrowserDocumentState`**, following the `IPageState` → `IBrowserDocumentState`
  rename in `Cirreum.Contracts` 2.0.0, along with its `PageTitle*` members → `DocumentTitle*`. The
  type governs the browser document hosting the application — title, application name, PWA display
  mode — none of which are properties of a Blazor *page*, and "page" already means a routable
  component. The registration description was wrong for the same reason ("Page navigation and routing
  state") and now says what it does. The implementation is internal, so this is not a public API
  change.

- **The feature state implementations move to the root `Cirreum` namespace**, following their
  contracts: `ActivityState`, `BrowserDocumentState`, `NotificationState`, `ThemeState`. The
  machinery stays in `Cirreum.State` — `StateManager`, `StateContainer`,
  `PersistableStateContainer`, `RemoteState`, `StateBuilder`, the persistence helpers — which is
  the same split `Cirreum.Contracts` applied to the interfaces. All four are internal, so no public
  API changes.

### Updated

- Re-pinned to the `2.0.0` foundations.

### Fixed

- `AuthenticationLibraryType`'s documentation referenced `IdentityProviderType`, removed in
  `Cirreum.Kernel` 2.0.0, which would leave a dangling `cref` once this package re-pins. Its
  remarks now state the distinction directly: this names the client-side *library*, not the
  identity provider behind it, since several providers are reached through the same library. For
  the provider itself, `UserProfile.Issuer` identifies it exactly.

## [1.0.35] - 2026-07-24

### Updated

- Updated NuGet packages.

## [1.0.34] - 2026-07-24

### Updated

- Updated NuGet packages.

## [1.0.33] - 2026-07-20

### Updated

- Updated NuGet packages.

## [1.0.32] - 2026-07-19

### Updated

- Updated NuGet packages.

## [1.0.28] - 2026-07-04

### Updated

- Updated NuGet packages.

## [1.0.27] - 2026-07-04

### Updated

- Updated NuGet packages.

## [1.0.26] - 2026-07-04

### Updated

- Updated NuGet packages.

## [1.0.25] - 2026-05-10

### Updated

- Updated NuGet packages.

## [1.0.24] - 2026-05-07

### Updated

- Updated NuGet packages.

## [1.0.23] - 2026-05-01

### Updated

- Updated NuGet packages.
