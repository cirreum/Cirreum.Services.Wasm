namespace Cirreum.Authorization;

/// <summary>
/// Configuration options for session timeout and activity tracking behavior.
/// </summary>
/// <remarks>
/// This class provides comprehensive configuration for managing user session idle timeout,
/// activity tracking, and custom event handling in Blazor applications.
/// </remarks>
public class SessionOptions {

	/// <summary>
	/// Gets or sets a value indicating whether the session timeout feature is enabled.
	/// </summary>
	/// <value>
	/// <see langword="true"/> to enable session timeout monitoring; <see langword="false"/> to disable.
	/// Default is <see langword="false"/>.
	/// </value>
	public bool Enabled { get; set; } = false;

	/// <summary>
	/// Gets or sets the session idle timeout duration in minutes.
	/// </summary>
	/// <remarks>
	/// This value determines when a session is considered stale based on user inactivity.
	/// </remarks>
	public int TimeoutMinutes { get; set; } = 30;

	/// <summary>
	/// Gets or sets a value indicating whether to track general user activity for session extension.
	/// </summary>
	/// <value>
	/// <see langword="true"/> to track user interactions (clicks, navigation, keyboard input);
	/// <see langword="false"/> to disable activity tracking. Default is <see langword="true"/>.
	/// </value>
	/// <remarks>
	/// When enabled, user interactions with the application UI could reset the session idle timeout timer.
	/// This could provide a seamless user experience by automatically extending sessions during active use.
	/// </remarks>
	public bool TrackUserActivity { get; set; } = true;

	/// <summary>
	/// Gets or sets a value indicating whether to track API calls for session extension.
	/// </summary>
	/// <value>
	/// <see langword="true"/> to track HTTP API calls as user activity;
	/// <see langword="false"/> to ignore API calls for session extension. Default is <see langword="true"/>.
	/// </value>
	/// <remarks>
	/// When enabled, authenticated API calls could reset the session idle timeout timer.
	/// This could be useful for applications that perform background operations or real-time data updates.
	/// </remarks>
	public bool TrackApiCalls { get; set; } = true;

	/// <summary>
	/// Gets or sets a value indicating whether the session should require explicit user action to extend.
	/// </summary>
	/// <remarks>
	/// <para>
	/// When set to <see langword="false"/> (default), allows the session idle timeout to behave in a
	/// user-friendly manner where an optional user activity monitor could reset the inactivity timer
	/// by calling <see cref="ISessionManager.ExtendSession"/>.
	/// </para>
	/// <para>
	/// When set to <see langword="true"/>, this essentially disables <see cref="TrackUserActivity"/> and 
	/// <see cref="TrackApiCalls"/>, preventing the session idle timer from being quietly extended.
	/// When the idle timer expires, the user should be notified by the consuming UI/UX component
	/// listening to <see cref="ISessionManager.SessionExpired"/>, displaying options to
	/// continue (<see cref="ISessionManager.ExtendSession"/>) or logout.
	/// </para>
	/// </remarks>
	public bool RequireExplicitKeepAlive { get; set; } = false;

	/// <summary>
	/// Gets or sets the message displayed to the user when their session has timed out.
	/// </summary>
	public string SessionTimeoutMessage { get; set; } = "Your session has expired. Continue working?";

	/// <summary>
	/// Gets or sets the logout URL to redirect to when the user chooses to logout or auto-logout occurs.
	/// </summary>
	/// <value>
	/// The logout URL path. Default is "authentication/logout" which works with standard 
	/// ASP.NET Core authentication. Can be relative or absolute URL.
	/// </value>
	/// <remarks>
	/// This URL is used by <c>SessionExpirationDialog</c> when the user clicks logout or when 
	/// auto-logout occurs after session expiration. The URL should handle the logout 
	/// process for your authentication system.
	/// </remarks>
	public string LogoutUrl { get; set; } = DefaultLogoutUrl;
	public const string DefaultLogoutUrl = "authentication/logout";

	/// <summary>
	/// Gets or sets the session stages that define monitoring behavior during different periods of the session.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Session stages allow fine-grained control over when and how activity monitoring occurs during a session.
	/// Each stage defines a percentage range of the session duration and specific monitoring behaviors.
	/// </para>
	/// <para>
	/// If not specified, default stages will be used:
	/// <list type="bullet">
	/// <item><description>SafeZone (0-90%): No activity monitoring, 10x DOM throttling for optimal performance</description></item>
	/// <item><description>WatchZone (90-100%): Active monitoring with 2-second debounce, 1x DOM throttling</description></item>
	/// </list>
	/// </para>
	/// <para>
	/// Custom stages must cover the complete session duration (0% to 100%) with no gaps or overlaps.
	/// Stages are processed sequentially and each stage must start where the previous stage ends.
	/// </para>
	/// </remarks>
	public List<SessionStage>? Stages { get; set; }

}