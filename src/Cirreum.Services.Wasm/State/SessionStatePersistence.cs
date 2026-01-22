namespace Cirreum.State;

using Cirreum.Startup;
using Cirreum.Storage;
using Microsoft.Extensions.Logging;

sealed class SessionStatePersistence(
	IStateManager manager,
	ISessionStorageService storage,
	ISessionState state,
	ILogger<SessionStatePersistence> logger
) : ISessionStatePersistence
  , IAutoInitialize {

	private const string containerIdKey = "_cirreum-f2d9ac85-a131-4f3c-ab62-c1b94ca8fe0a";
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