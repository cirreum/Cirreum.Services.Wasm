namespace Cirreum.State;

/// <summary>
/// Default implementation of <see cref="IActivityState"/> that tracks
/// application activity and progress.
/// </summary>
/// <remarks>
/// <para>
/// This class tracks total and completed task counts to support both
/// indeterminate and deterministic activity reporting.
/// </para>
/// <para>
/// State changes are propagated through the <see cref="IStateManager"/> so that
/// UI consumers can react to global activity changes such as splash screens,
/// loading overlays, and progress indicators.
/// </para>
/// </remarks>
sealed class ActivityState(
	IStateManager stateManager
) : ScopedNotificationState, IActivityState {

	private int _totalTasks;
	private int _completedTasks;
	private ActivityMode _mode = ActivityMode.Indeterminate;
	private readonly List<ActivityError> _errors = [];

	/// <inheritdoc />
	public bool IsActive => this._totalTasks > this._completedTasks;

	/// <inheritdoc />
	public ActivityMode Mode => this._mode;

	/// <inheritdoc />
	public bool IsDeterministic => this._mode == ActivityMode.Deterministic;

	/// <inheritdoc />
	public string DisplayStatus { get; private set; } = string.Empty;

	/// <inheritdoc />
	public int TotalTasks => this._totalTasks;

	/// <inheritdoc />
	public int CompletedTasks => this._completedTasks;

	/// <inheritdoc />
	public double? ProgressPercent =>
		this._mode == ActivityMode.Deterministic && this._totalTasks > 0
			? (double)this._completedTasks / this._totalTasks
			: null;

	/// <inheritdoc />
	public IReadOnlyList<ActivityError> Errors => this._errors.AsReadOnly();

	/// <inheritdoc />
	public void StartTask(string? status = null) {
		this._totalTasks++;

		if (!string.IsNullOrWhiteSpace(status)) {
			this.DisplayStatus = status;
		}

		this.NotifyStateChanged();
	}

	/// <inheritdoc />
	public void BeginTasks(int count, string? status = null) {
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

		this._totalTasks += count;

		if (!string.IsNullOrWhiteSpace(status)) {
			this.DisplayStatus = status;
		}

		this.NotifyStateChanged();
	}

	/// <inheritdoc />
	public void CompleteTask() {
		if (this._completedTasks >= this._totalTasks) {
			return;
		}

		this._completedTasks++;

		if (this._completedTasks >= this._totalTasks) {
			this.ResetTasks();
			return;
		}

		this.NotifyStateChanged();
	}

	/// <inheritdoc />
	public void SetDisplayStatus(string status) {
		ArgumentNullException.ThrowIfNull(status);

		this.DisplayStatus = status;
		this.NotifyStateChanged();
	}

	/// <inheritdoc />
	public void SetMode(ActivityMode mode) {
		if (mode == ActivityMode.Deterministic && this._totalTasks == 0) {
			throw new InvalidOperationException(
				"Deterministic mode requires one or more tracked tasks.");
		}

		this._mode = mode;
		this.NotifyStateChanged();
	}

	/// <inheritdoc />
	public void ResetTasks() {
		this._totalTasks = 0;
		this._completedTasks = 0;
		this._mode = ActivityMode.Indeterminate;
		this.DisplayStatus = string.Empty;

		this.NotifyStateChanged();
	}

	/// <inheritdoc />
	public void LogError(ActivityError error) {
		ArgumentNullException.ThrowIfNull(error);

		this._errors.Add(error);
		this.NotifyStateChanged();
	}

	/// <inheritdoc />
	public void LogError(
		string sourceName,
		Exception exception,
		string? displayMessage = null,
		ActivityErrorSeverity severity = ActivityErrorSeverity.Error) {

		ArgumentException.ThrowIfNullOrWhiteSpace(sourceName);
		ArgumentNullException.ThrowIfNull(exception);

		this._errors.Add(ActivityError.FromException(
			sourceName,
			exception,
			displayMessage,
			severity));

		this.NotifyStateChanged();
	}

	/// <inheritdoc />
	public void ClearErrors() {
		this._errors.Clear();
		this.NotifyStateChanged();
	}

	/// <inheritdoc />
	protected override void OnStateHasChanged() {
		stateManager.NotifySubscribers<IActivityState>(this);
	}

}