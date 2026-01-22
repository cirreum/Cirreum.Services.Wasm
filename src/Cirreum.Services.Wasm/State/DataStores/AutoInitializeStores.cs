namespace Cirreum.State.DataStores;

using Cirreum.Startup;
using Microsoft.Extensions.Logging;

/// <summary>
/// A startup task that orchestrates the initialization of all registered
/// <see cref="IInitializableStore"/> implementations.
/// </summary>
/// <remarks>
/// <para>
/// This task is automatically ran during application startup based on the configured <see cref="IStartupGate"/>.
/// </para>
/// <para>
/// Stores are initialized in order by their <see cref="IInitializableStore.Order"/> property.
/// Errors during individual store initialization are logged and recorded but do not prevent
/// other stores from initializing.
/// </para>
/// </remarks>
public class AutoInitializeStores(
	IEnumerable<IInitializableStore> stores,
	IInitializationState initState,
	IStartupGate gate,
	ILogger<AutoInitializeStores> logger
) : IStartupTask {

	/// <summary>
	/// Gets the execution order for this startup task.
	/// </summary>
	/// <remarks>
	/// Defaults to 10000 to run after most other startup tasks have completed.
	/// </remarks>
	public int Order => 10000;

	/// <inheritdoc />
	public ValueTask ExecuteAsync() {
		gate.WhenReady(this.InitializeStoresAsync);
		return ValueTask.CompletedTask;
	}

	private async Task InitializeStoresAsync(CancellationToken cancellationToken) {

		var storeList = stores.OrderBy(s => s.Order).ToList();

		if (storeList.Count == 0) {
			if (logger.IsEnabled(LogLevel.Debug)) {
				logger.LogDebug("No initializable stores registered");
			}
			return;
		}

		if (logger.IsEnabled(LogLevel.Information)) {
			logger.LogInformation(
				"Beginning data store initialization for {StoreCount} stores",
				storeList.Count);
		}

		initState.StartTask("Initializing application data");

		try {

			foreach (var store in storeList) {
				cancellationToken.ThrowIfCancellationRequested();

				var statusMessage = store.InitializationMessage;
				if (string.IsNullOrWhiteSpace(statusMessage)) {
					statusMessage = $"Loading {store.DisplayName}...";
				}
				initState.SetDisplayStatus(statusMessage);
				if (logger.IsEnabled(LogLevel.Debug)) {
					logger.LogDebug("Initializing store: {StoreName}", store.DisplayName);
				}

				try {
					await store.LoadAsync(cancellationToken);
					if (logger.IsEnabled(LogLevel.Debug)) {
						logger.LogDebug(
							"Successfully initialized store: {StoreName}",
							store.DisplayName);
					}
				} catch (Exception ex) {
					if (logger.IsEnabled(LogLevel.Error)) {
						logger.LogError(
							ex,
							"Failed to initialize store: {StoreName}",
							store.DisplayName);
					}
					initState.LogError(store.DisplayName, ex);
					// Continue with other stores - don't let one failure stop initialization
				}

			}

			if (logger.IsEnabled(LogLevel.Information)) {
				logger.LogInformation(
				"Data store initialization complete. Errors: {ErrorCount}",
				initState.ErrorCount);
			}

		} finally {
			initState.CompleteTask();
		}

	}

}