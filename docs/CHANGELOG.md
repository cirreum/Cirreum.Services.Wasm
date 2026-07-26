# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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
