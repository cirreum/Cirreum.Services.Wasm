namespace Cirreum.Security;

using System.ComponentModel;

/// <summary>
/// The supported client-side authentication library types — drives Blazor WebAssembly
/// JavaScript auth-library selection (MSAL vs. OIDC) at startup.
/// </summary>
/// <remarks>
/// <para>
/// A WASM client-configuration concept. It lives in <c>Cirreum.Services.Wasm</c> —
/// the lowest package that consumes it — and is read from JS interop at startup via
/// <c>DynamicAuthInterop.GetAuthLibraryType()</c>. It is deliberately NOT part of the
/// cross-host <see cref="IUserState"/> contract: the auth library is static application
/// configuration, not per-user state.
/// </para>
/// <para>
/// See <see cref="IdentityProviderType"/> for the IdP-level implementations.
/// </para>
/// </remarks>
public enum AuthenticationLibraryType {
	/// <summary>
	/// No authentication library configured (anonymous only)
	/// </summary>
	None = 0,
	/// <summary>
	/// Microsoft Authentication Library
	/// </summary>
	[Description("msal")]
	MSAL = 1,
	/// <summary>
	/// Standards based OpenID Connect Library
	/// </summary>
	[Description("oidc")]
	OIDC = 2
}
