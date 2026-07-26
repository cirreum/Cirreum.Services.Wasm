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
/// This names the client-side <em>library</em>, not the identity provider behind it — several
/// providers are reached through the same library. When an application needs to distinguish the
/// provider itself, the token's issuer (<c>UserProfile.Issuer</c>) identifies it exactly, and the
/// authenticated scheme is the authoritative per-request answer on the server.
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
