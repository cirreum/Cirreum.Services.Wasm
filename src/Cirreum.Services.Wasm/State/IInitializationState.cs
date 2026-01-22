namespace Cirreum.State;
/// <summary>
/// Tracks the state of initialization during application startup.
/// </summary>
/// <remarks>
/// <para>
/// This interface provides observable state for displaying initialization progress
/// to users, typically on a splash screen. It tracks the current status message,
/// active task count, and any errors that occurred during initialization.
/// </para>
/// <para>
/// The implementation notifies subscribers when state changes, enabling reactive
/// UI updates during the initialization process.
/// </para>
/// </remarks>
public interface IInitializationState : IScopedNotificationState {

	/// <summary>
	/// Gets a value indicating whether initialization is currently in progress.
	/// </summary>
	/// <remarks>
	/// This property is <c>true</c> when at least one initialization task is active.
	/// </remarks>
	bool IsInitializing { get; }

	/// <summary>
	/// Gets the current status message to display to users.
	/// </summary>
	/// <remarks>
	/// This message is updated as each store begins initialization,
	/// typically in the format "Loading Events..." or similar.
	/// </remarks>
	string DisplayStatus { get; }

	/// <summary>
	/// Signals the start of an initialization task.
	/// </summary>
	/// <param name="status">The status message to display.</param>
	void StartTask(string status);

	/// <summary>
	/// Updates the current display status without changing the task count.
	/// </summary>
	/// <param name="status">The new status message to display.</param>
	void SetDisplayStatus(string status);

	/// <summary>
	/// Signals the completion of an initialization task.
	/// </summary>
	/// <remarks>
	/// When all tasks complete (task count reaches zero), the display status is cleared.
	/// </remarks>
	void CompleteTask();

	/// <summary>
	/// Gets the current number of active initialization tasks.
	/// </summary>
	/// <returns>The count of active tasks.</returns>
	int GetTaskCount();

	/// <summary>
	/// Gets the collection of errors that occurred during initialization.
	/// </summary>
	IReadOnlyList<InitializationError> Errors { get; }

	/// <summary>
	/// Gets a value indicating whether any errors occurred during initialization.
	/// </summary>
	bool HasErrors { get; }

	/// <summary>
	/// Gets the count of errors that occurred during initialization.
	/// </summary>
	int ErrorCount { get; }

	/// <summary>
	/// Logs an error that occurred during store initialization.
	/// </summary>
	/// <param name="storeName">The display name of the store that failed.</param>
	/// <param name="exception">The exception that occurred.</param>
	void LogError(string storeName, Exception exception);

	/// <summary>
	/// Clears all recorded initialization errors.
	/// </summary>
	void ClearErrors();

}