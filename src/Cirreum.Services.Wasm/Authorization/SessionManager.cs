namespace Cirreum.Authorization;

using Cirreum.Security;
using Cirreum.Startup;
using Cirreum.State;
using Microsoft.Extensions.Logging;

/// <summary>
/// Manages user session lifecycle with configurable stage-based monitoring.
/// </summary>
sealed class SessionManager(
	SessionOptions options,
	IStateManager stateManager,
	ILogger<SessionManager> logger
) : ISessionManager, IAutoInitialize, IDisposable {

	private bool _isDisposed;
	private IUserState? _currentUser;
	private IDisposable? _userSubscription;

	private readonly TimeSpan _sessionTimeout = TimeSpan.FromMinutes(options.TimeoutMinutes);
	private readonly List<SessionStage> _stages = options.Stages ?? CreateDefaultStages();

	// Stage management
	private int _currentStageIndex = -1;
	private Timer? _stageTransitionTimer;
	private Timer? _activityDebounceTimer;
	private DateTimeOffset _sessionStartTime;

	#region Default Stage Configuration

	private static List<SessionStage> CreateDefaultStages() {
		return [
			new() {
				Name = "SafeZone",
				StartPercentage = 0.0,
				EndPercentage = 0.9,
				MonitorActivity = false,
				Metadata = { [SessionStage.MetadataKeys.DomThrottleMultiplier] = 10 }
			},
			new() {
				Name = "WatchZone",
				StartPercentage = 0.9,
				EndPercentage = 1.0,
				ActivityDebounce = TimeSpan.FromSeconds(2),
				MonitorActivity = true,
				Metadata = { [SessionStage.MetadataKeys.DomThrottleMultiplier] = 1 }
			}
		];
	}

	#endregion

	#region IAutoInitialize

	public ValueTask InitializeAsync() {
		this.Initialize();
		return default;
	}

	void Initialize() {

#if DEBUG
		Console.WriteLine($"[SESSION:{this.SessionId}] Initialized");
#endif

		if (options.Enabled is false) {
			return;
		}

		this.ValidateStageConfiguration();

		this._userSubscription = stateManager.Subscribe<IUserState>(currentUser => {
			if (currentUser is null) {
				logger.LogUserStateCleared();
				this.StopSessionManagement();
				this._currentUser = null;
				return;
			}

			this._currentUser = currentUser;

			if (currentUser.IsAuthenticated) {
				logger.LogUserAuthenticated(currentUser.Name);
				currentUser.UpdateActivity();
				this.StartSessionStages();
				this.RaiseSessionStarted();
			} else {
				logger.LogUserNoLongerAuthenticated(currentUser.Name);
				this.StopSessionManagement();
				this._currentUser = null;
			}
		});

		return;
	}

	private void ValidateStageConfiguration() {
		if (this._stages.Count == 0) {
			throw new InvalidOperationException("At least one session stage must be configured.");
		}

		for (var i = 0; i < this._stages.Count; i++) {
			var stage = this._stages[i];

			if (stage.StartPercentage < 0 || stage.StartPercentage > 1) {
				throw new ArgumentException($"Stage '{stage.Name}' StartPercentage must be between 0 and 1");
			}

			if (stage.EndPercentage < 0 || stage.EndPercentage > 1) {
				throw new ArgumentException($"Stage '{stage.Name}' EndPercentage must be between 0 and 1");
			}

			if (stage.StartPercentage >= stage.EndPercentage) {
				throw new ArgumentException($"Stage '{stage.Name}' StartPercentage must be less than EndPercentage");
			}

			if (i > 0 && Math.Abs(this._stages[i - 1].EndPercentage - stage.StartPercentage) > 0.001) {
				throw new ArgumentException($"Stage '{stage.Name}' must start where previous stage ends");
			}
		}

		if (Math.Abs(this._stages.First().StartPercentage) > 0.001) {
			throw new ArgumentException("First stage must start at 0%");
		}

		if (Math.Abs(this._stages.Last().EndPercentage - 1.0) > 0.001) {
			throw new ArgumentException("Last stage must end at 100%");
		}
	}

	#endregion

	#region ISessionManager

	public event Action? SessionExpired;
	public event Action? SessionStarted;
	public event Action<SessionStage>? SessionStageChanged;

	public SessionStage? CurrentStage =>
		this._currentStageIndex >= 0 && this._currentStageIndex < this._stages.Count
			? this._stages[this._currentStageIndex]
			: null;

	public TimeSpan TimeRemaining => this.GetTimeRemaining();

	public string SessionId { get; } = Guid.NewGuid().ToString();

	public void ExtendSession() {
#if DEBUG
		Console.WriteLine($"[SESSION:{this.SessionId}] ExtendSession called - Disposed: {_isDisposed}, User authenticated: {_currentUser?.IsAuthenticated}");
#endif
		if (this._isDisposed || this._currentUser?.IsAuthenticated != true) {
#if DEBUG
			Console.WriteLine($"[SESSION:{this.SessionId}] ExtendSession early return - disposed or not authenticated");
#endif
			return;
		}

		var currentStage = this.CurrentStage;
#if DEBUG
		Console.WriteLine($"[SESSION:{this.SessionId}] Current stage: {currentStage?.Name}, Monitor activity: {currentStage?.MonitorActivity}");
#endif

		// Restart session if: explicit mode OR no current stage (expired session)
		if (options.RequireExplicitKeepAlive || currentStage == null) {
#if DEBUG
			var reason = options.RequireExplicitKeepAlive ? "Explicit mode" : "No current stage (expired session)";
			Console.WriteLine($"[SESSION:{this.SessionId}] {reason} - restarting session stages");
#endif
			this._currentUser.UpdateActivity();
			this.RestartSessionStages();
			return;
		}

		if (!currentStage.MonitorActivity) {
#if DEBUG
			Console.WriteLine($"[SESSION:{this.SessionId}] ExtendSession early return - stage doesn't monitor activity");
#endif
			return;
		}

#if DEBUG
		Console.WriteLine($"[SESSION:{this.SessionId}] Auto-extend mode - updating activity and debouncing");
#endif
		this._currentUser.UpdateActivity();
		this.DebounceSessionExtension(currentStage);
	}

	#endregion

	#region Stage Management

	private void StartSessionStages() {

		// Extend session on Http activity
		if (options.TrackApiCalls) {
			SessionHttpHandler.HttpActivityDetected += this.ExtendSession;
		}

		this._sessionStartTime = DateTimeOffset.UtcNow;
		this._currentStageIndex = -1;
		this.AdvanceToNextStage();
	}

	private void AdvanceToNextStage() {
		var nextStageIndex = this._currentStageIndex + 1;

		if (nextStageIndex >= this._stages.Count) {
			// No more stages - session expires
			logger.LogAllStagesCompleted(this._currentUser?.Name);
			this.HandleSessionExpiration();
			return;
		}

		var nextStage = this._stages[nextStageIndex];
		this._currentStageIndex = nextStageIndex;

		// Calculate when this stage should end
		var stageDuration = this.CalculateStageDuration(nextStage);

		this._stageTransitionTimer?.Dispose();
		this._stageTransitionTimer = new Timer(
			(_) => this.AdvanceToNextStage(),
			null,
			stageDuration,
			Timeout.InfiniteTimeSpan
		);

		logger.LogAdvancedToStage(nextStage.Name, this._currentUser?.Name, stageDuration);

		this.RaiseStageChanged(nextStage);
	}

	private TimeSpan CalculateStageDuration(SessionStage stage) {
		var totalDuration = this._sessionTimeout.TotalMilliseconds;
		var stageDurationMs = totalDuration * (stage.EndPercentage - stage.StartPercentage);
		return TimeSpan.FromMilliseconds(stageDurationMs);
	}

	private void DebounceSessionExtension(SessionStage stage) {
		this._activityDebounceTimer?.Dispose();
		this._activityDebounceTimer = new Timer(
			(_) => this.HandleDebouncedSessionExtension(),
			null,
			stage.ActivityDebounce,
			Timeout.InfiniteTimeSpan
		);

#if DEBUG
		Console.WriteLine($"Activity debounce started for stage '{stage.Name}' with {stage.ActivityDebounce}ms delay");
#endif
	}

	private void HandleDebouncedSessionExtension() {
		this._activityDebounceTimer?.Dispose();
		this._activityDebounceTimer = null;

		if (this._currentUser?.IsAuthenticated == true) {
			logger.LogSessionExtended(this._currentUser.Name);
			this.RestartSessionStages();
		} else {
			logger.LogDebounceTimerNoAuth();
			this.StopSessionManagement();
		}
	}

	private void RestartSessionStages() {
		this.StopAllTimers();
		this.StartSessionStages();
	}

	#endregion

	#region Session Lifecycle

	private void HandleSessionExpiration() {
		logger.LogSessionExpired(this._currentUser?.Name ?? "unknown");
		this.StopSessionManagement();
		this.RaiseSessionExpired();
	}

	private void StopSessionManagement() {
		this.StopAllTimers();
		this._currentStageIndex = -1;
	}

	private void StopAllTimers() {

		SessionHttpHandler.HttpActivityDetected -= this.ExtendSession;

		this._stageTransitionTimer?.Dispose();
		this._stageTransitionTimer = null;

		this._activityDebounceTimer?.Dispose();
		this._activityDebounceTimer = null;
	}

	#endregion

	#region Time Calculations

	private TimeSpan GetTimeRemaining() {
		if (this._currentUser?.IsAuthenticated != true || this._currentStageIndex < 0) {
			return TimeSpan.Zero;
		}

		var elapsed = DateTimeOffset.UtcNow - this._sessionStartTime;
		var remaining = this._sessionTimeout - elapsed;

		return remaining <= TimeSpan.Zero
			? TimeSpan.Zero
			: TimeSpan.FromSeconds(Math.Ceiling(remaining.TotalSeconds));
	}

	#endregion

	#region Event Handling

	private void RaiseSessionStarted() {
		try {
			SessionStarted?.Invoke();
		} catch (Exception ex) {
			logger.LogSessionStartedError(ex);
		}
	}

	private void RaiseSessionExpired() {
		try {
			SessionExpired?.Invoke();
		} catch (Exception ex) {
			logger.LogSessionExpiredError(ex);
		}
	}

	private void RaiseStageChanged(SessionStage stage) {
		try {
			SessionStageChanged?.Invoke(stage);
		} catch (Exception ex) {
			logger.LogStageChangedError(ex, stage.Name);
		}
	}

	#endregion

	#region IDisposable

	public void Dispose() {
		if (this._isDisposed) {
			return;
		}
		this._isDisposed = true;

		this.StopSessionManagement();
		this._userSubscription?.Dispose();
		this._userSubscription = null;
		this._currentUser = null;
	}

	#endregion
}