namespace Cirreum.Authorization;

using Microsoft.Extensions.Logging;

/// <summary>
/// Source-generated logging extensions for SessionManager.
/// </summary>
internal static partial class SessionManagerLogging {
	[LoggerMessage(
		EventId = 2001,
		Level = LogLevel.Trace,
		Message = "User state cleared. Stopping session management.")]
	public static partial void LogUserStateCleared(this ILogger logger);

	[LoggerMessage(
		EventId = 2002,
		Level = LogLevel.Trace,
		Message = "User '{UserName}' is authenticated. Starting session management.")]
	public static partial void LogUserAuthenticated(this ILogger logger, string userName);

	[LoggerMessage(
		EventId = 2003,
		Level = LogLevel.Trace,
		Message = "User '{UserName}' is no longer authenticated. Stopping session management.")]
	public static partial void LogUserNoLongerAuthenticated(this ILogger logger, string userName);

	[LoggerMessage(
		EventId = 2004,
		Level = LogLevel.Trace,
		Message = "All session stages completed for '{UserName}'. Session expired.")]
	public static partial void LogAllStagesCompleted(this ILogger logger, string? userName);

	[LoggerMessage(
		EventId = 2005,
		Level = LogLevel.Trace,
		Message = "Advanced to stage '{StageName}' for user '{UserName}' (duration: {Duration})")]
	public static partial void LogAdvancedToStage(
		this ILogger logger,
		string stageName,
		string? userName,
		TimeSpan duration);

	[LoggerMessage(
		EventId = 2006,
		Level = LogLevel.Trace,
		Message = "Session extended due to activity for '{UserName}'")]
	public static partial void LogSessionExtended(this ILogger logger, string userName);

	[LoggerMessage(
		EventId = 2007,
		Level = LogLevel.Trace,
		Message = "Debounce timer fired but user is no longer authenticated. Stopping session management.")]
	public static partial void LogDebounceTimerNoAuth(this ILogger logger);

	[LoggerMessage(
		EventId = 2008,
		Level = LogLevel.Trace,
		Message = "Session expired for '{UserName}'")]
	public static partial void LogSessionExpired(this ILogger logger, string userName);

	[LoggerMessage(
		EventId = 2009,
		Level = LogLevel.Error,
		Message = "Exception thrown by SessionStarted event subscriber")]
	public static partial void LogSessionStartedError(this ILogger logger, Exception ex);

	[LoggerMessage(
		EventId = 2010,
		Level = LogLevel.Error,
		Message = "Exception thrown by SessionExpired event subscriber")]
	public static partial void LogSessionExpiredError(this ILogger logger, Exception ex);

	[LoggerMessage(
		EventId = 2011,
		Level = LogLevel.Error,
		Message = "Exception thrown by SessionStageChanged event subscriber for stage '{StageName}'")]
	public static partial void LogStageChangedError(this ILogger logger, Exception ex, string stageName);
}