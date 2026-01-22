namespace Cirreum.Authorization;

/// <summary>
/// Provides functionality for managing user session lifecycle and timeout monitoring.
/// </summary>
public interface ISessionManager {

	/// <summary>
	/// Gets the unique identifier for the current session.
	/// </summary>
	string SessionId { get; }

	/// <summary>
	/// Occurs when the session timeout period has elapsed.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This event is triggered when the configured session timeout duration has elapsed.
	/// Event handlers should decide how to respond to session expiration, such as:
	/// <list type="bullet">
	/// <item><description>Showing a "Continue Working?" dialog</description></item>
	/// <item><description>Providing a grace period with countdown</description></item>
	/// <item><description>Immediately logging out the user</description></item>
	/// <item><description>Redirecting to login pages</description></item>
	/// </list>
	/// </para>
	/// <para>
	/// The application can call <see cref="ExtendSession"/> to restart the session timer
	/// for another timeout period, or handle logout/cleanup as appropriate.
	/// </para>
	/// </remarks>
	event Action? SessionExpired;

	/// <summary>
	/// Occurs when a session begins.
	/// </summary>
	/// <remarks>
	/// Subscribe to this event to perform actions when a user session starts. The event is triggered
	/// at the beginning of a session and can be used to initialize resources or perform setup tasks.
	/// </remarks>
	event Action? SessionStarted;

	/// <summary>
	/// Occurs when the session transitions to a different monitoring stage.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This event is triggered whenever the session moves from one configured stage to another.
	/// Subscribers can use this to adjust their behavior based on the current session stage.
	/// </para>
	/// <para>
	/// Common use cases include:
	/// <list type="bullet">
	/// <item><description>Adjusting DOM activity monitoring throttling (relaxed vs tight)</description></item>
	/// <item><description>Displaying different UI indicators (safe zone vs warning zone)</description></item>
	/// <item><description>Modifying background task frequencies</description></item>
	/// <item><description>Implementing progressive session warning systems</description></item>
	/// </list>
	/// </para>
	/// <para>
	/// Stage metadata can be accessed via <see cref="SessionStage.Metadata"/> to configure
	/// external components dynamically based on the current stage requirements.
	/// </para>
	/// </remarks>
	event Action<SessionStage>? SessionStageChanged;

	/// <summary>
	/// Gets the current session stage configuration, if any.
	/// </summary>
	/// <value>
	/// The <see cref="SessionStage"/> for the current stage, or <see langword="null"/> 
	/// if no session is active or no stages are configured.
	/// </value>
	/// <remarks>
	/// This property provides access to the current stage's configuration, including its name,
	/// activity monitoring settings, debounce timing, and metadata. External components can
	/// use this information to adapt their behavior to the current session phase.
	/// </remarks>
	SessionStage? CurrentStage { get; }

	/// <summary>
	/// Gets the amount of time remaining for the current session.
	/// </summary>
	/// <value>
	/// A <see cref="TimeSpan"/> representing the time until the next session expiration event, 
	/// or <see cref="TimeSpan.Zero"/> if no session monitoring is active.
	/// </value>
	/// <remarks>
	/// This value represents the total time remaining until session expiration, not the time
	/// remaining in the current stage. The calculation is based on the session start time
	/// and the configured timeout duration.
	/// </remarks>
	TimeSpan TimeRemaining { get; }

	/// <summary>
	/// Extends the current session, resetting its expiration timer.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This method is typically used to keep a session active by extending its duration.
	/// The behavior depends on the session configuration:
	/// </para>
	/// <list type="bullet">
	/// <item><description><strong>Explicit mode</strong>: Always restarts the entire session from the beginning</description></item>
	/// <item><description><strong>Auto-extend mode</strong>: Only extends if called during a monitoring stage</description></item>
	/// </list>
	/// <para>
	/// When a session is extended, it restarts from the first configured stage and all
	/// stage timers are reset. Ensure that the session is valid before calling this method.
	/// </para>
	/// </remarks>
	void ExtendSession();

}