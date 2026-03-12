namespace Cirreum.State;

/// <summary>
/// Default implementation of <see cref="IInitializationState"/> that tracks
/// application initialization progress during startup.
/// </summary>
/// <remarks>
/// <para>
/// This class tracks total and completed task counts to enable deterministic
/// progress reporting. State changes are propagated through the <see cref="IStateManager"/>
/// to enable reactive UI updates for splash screens and loading indicators.
/// </para>
/// </remarks>
public class InitializationState(
	IStateManager stateManager
) : ScopedNotificationState, IInitializationState {

	private int _totalTasks;
	private int _completedTasks;
	private readonly List<InitializationError> _errors = [];

	/// <inheritdoc />
	public bool IsInitializing => this._totalTasks > 0 && this._completedTasks < this._totalTasks;

	/// <inheritdoc />
	public string DisplayStatus { get; private set; } = string.Empty;

	/// <inheritdoc />
	public int TotalTasks => this._totalTasks;

	/// <inheritdoc />
	public int CompletedTasks => this._completedTasks;

	/// <inheritdoc />
	public void SetTotalTasks(int total) {
		this._totalTasks = total;
		this.NotifyStateChanged();
	}

	/// <inheritdoc />
	public void SetDisplayStatus(string status) {
		this.DisplayStatus = status;
		this.NotifyStateChanged();
	}

	/// <inheritdoc />
	public void CompleteTask() {
		if (this._completedTasks < this._totalTasks) {
			this._completedTasks++;
			if (this._completedTasks >= this._totalTasks) {
				this.DisplayStatus = string.Empty;
			}
			this.NotifyStateChanged();
		}
	}

	/// <inheritdoc />
	public IReadOnlyList<InitializationError> Errors => this._errors.AsReadOnly();

	/// <inheritdoc />
	public bool HasErrors => this._errors.Count > 0;

	/// <inheritdoc />
	public int ErrorCount => this._errors.Count;

	/// <inheritdoc />
	public void LogError(string storeName, Exception exception) {
		this._errors.Add(InitializationError.FromException(storeName, exception));
		this.NotifyStateChanged();
	}

	/// <inheritdoc />
	public void ClearErrors() {
		this._errors.Clear();
		this.NotifyStateChanged();
	}

	/// <inheritdoc />
	protected override void OnStateHasChanged() {
		stateManager.NotifySubscribers<IInitializationState>(this);
	}

}
