namespace Cirreum.State;

using Cirreum.Startup;
using Cirreum.Storage;
using Microsoft.Extensions.Logging;

sealed class LocalStatePersistence(
	IStateManager manager,
	ILocalStorageService storage,
	ILocalState state,
	ILogger<LocalStatePersistence> logger
) : ILocalStatePersistence
  , IAutoInitialize {

	private const string containerIdKey = "_cirreum-348f8115-b981-4783-b5ab-54d36da0d714";
	private string? containerId;
	private bool _initialized;

	public async ValueTask InitializeAsync() {
		if (this._initialized) {
			logger.LogWarning("InitializeAsync was called more than once. Ignoring subsequent calls.");
			return;
		}
		this._initialized = true;

		this.containerId =
			await PersistenceHelper.InitializeAsync(
			containerIdKey,
			manager,
			storage,
			state,
			logger);
	}

	public async Task SaveStateAsync() {
		if (containerId != null) {
			await PersistenceHelper.SaveState(
				containerId,
				storage,
				state,
				logger);
		}
	}

	public async Task RestoreStateAsync() {
		if (containerId != null) {
			await PersistenceHelper.RestoreState(
				containerId,
				storage,
				state,
				logger);
		}
	}

}