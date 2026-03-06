namespace Cirreum.Presence;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Provider-agnostic monitor for user presence, compatible with Blazor WASM.
/// </summary>
sealed class DefaultUserPresenceMonitor(
	IOptions<UserPresenceMonitorOptions> userPresenceMonitorOptions,
	IUserPresenceService presenceService,
	IUserPresenceState presenceState,
	ILogger<DefaultUserPresenceMonitor>? logger = null
) : IUserPresenceMonitor, IAsyncDisposable {

	private readonly IUserPresenceService _presenceService = presenceService ?? throw new ArgumentNullException(nameof(presenceService));
	private readonly IUserPresenceState _presenceState = presenceState ?? throw new ArgumentNullException(nameof(presenceState));
	private readonly UserPresenceMonitorOptions _monitorOptions = userPresenceMonitorOptions?.Value ?? throw new ArgumentNullException(nameof(userPresenceMonitorOptions));
	private readonly ILogger<DefaultUserPresenceMonitor> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
	private CancellationTokenSource? _cancellationTokenSource;
	private Task? _monitoringTask;
	private bool _isDisposed;
	private int _monitoringInterval;
	private const int MinimumValidInterval = 6;
	private const int MaxRetryDelayMs = 30_000; // 30 seconds

	/// <inheritdoc/>
	public bool IsMonitoring { get; private set; }

	/// <inheritdoc/>
	public async Task StartMonitoringPresence() {

		if (this.IsMonitoring) {
			return;
		}

		try {

			this._monitoringInterval = this._monitorOptions.RefreshInterval;
			if (this._monitoringInterval == 0) {
				// disabled...
				this._logger.LogDebug("Presence monitoring disabled (interval = 0)");
				return;
			}

			if (this._monitoringInterval < MinimumValidInterval &&
				this._monitoringInterval > 0) {
				// fallback to default value
				this._logger.LogWarning(
						"Configured refresh interval {ConfiguredInterval} is below minimum threshold of {MinThreshold}. Using default interval of {DefaultInterval}",
						this._monitoringInterval,
						MinimumValidInterval,
						UserPresenceMonitorOptions.DefaultRefreshInterval
					);
				this._monitoringInterval = UserPresenceMonitorOptions.DefaultRefreshInterval;
			}

			this._cancellationTokenSource = new CancellationTokenSource();
			var token = this._cancellationTokenSource.Token;

			// Start the monitoring loop
			if (this._logger.IsEnabled(LogLevel.Debug)) {
				this._logger.LogDebug("Starting presence monitoring with interval: {Interval}ms", this._monitoringInterval);
			}
			this._monitoringTask = this.MonitorPresenceLoop(token);
			this.IsMonitoring = true;

		} catch (Exception ex) {
			this._logger.LogError(ex, "Failed to start presence monitoring");
			await this.Shutdown();
			throw;
		}

	}

	private async Task MonitorPresenceLoop(CancellationToken cancellationToken) {

		// Do initial update
		await this.UpdatePresenceAsync();

		var retryCount = 0;
		while (!cancellationToken.IsCancellationRequested) {
			try {
				await Task.Delay(this._monitoringInterval, cancellationToken);
				await this.UpdatePresenceAsync();
				retryCount = 0;
			} catch (OperationCanceledException) {
				break;
			} catch (Exception ex) {
				this._logger.LogError(ex, "Error in presence monitoring loop");
				try {
					var nextRetry = ++retryCount;
					if (this._logger.IsEnabled(LogLevel.Debug)) {
						this._logger.LogDebug("Backing off retry attempt {RetryCount}", nextRetry);
					}
					await DelayWithBackoff(nextRetry, MaxRetryDelayMs, cancellationToken);
				} catch (OperationCanceledException) {
					break;
				}
			}
		}
	}

	private static async Task DelayWithBackoff(int retryCount, int maxDelay, CancellationToken cancellationToken) {
		var delay = Math.Min(1000 * Math.Pow(2, retryCount), maxDelay);
		await Task.Delay((int)delay, cancellationToken);
	}

	/// <inheritdoc/>
	public async Task StopMonitoringPresence() {

		if (!this.IsMonitoring) {
			return;
		}

		this._logger.LogDebug("Stopping presence monitoring");

		try {
			await this.Shutdown();
		} finally {
			this.IsMonitoring = false;
		}

	}

	private async Task Shutdown() {

		if (this._cancellationTokenSource != null) {
			await this._cancellationTokenSource.CancelAsync();
			this._cancellationTokenSource.Dispose();
			this._cancellationTokenSource = null;
		}

		if (this._monitoringTask != null) {
			try {
				using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
				await this._monitoringTask.WaitAsync(timeoutCts.Token);
			} catch (OperationCanceledException) {
				this._logger.LogWarning("Timeout while waiting for monitoring task to complete");
			}
			this._monitoringTask = null;
		}
	}

	private async Task UpdatePresenceAsync() {

		if (this._isDisposed || (this._cancellationTokenSource?.Token.IsCancellationRequested ?? true)) {
			return;
		}

		try {
			await this._presenceService.UpdateUserPresence();
		} catch (Exception ex) {
			this._logger.LogError(ex, "Error updating user presence.");
			this._presenceState.SetPresence(new UserPresence(
				Status: PresenceStatus.Unknown,
				Activity: null,
				Message: null
			));
		}
	}

	public async ValueTask DisposeAsync() {
		if (this._isDisposed) {
			return;
		}

		await this.StopMonitoringPresence();
		this._isDisposed = true;

	}

}