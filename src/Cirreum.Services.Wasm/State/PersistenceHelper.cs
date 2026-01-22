namespace Cirreum.State;

using Cirreum.Storage;
using Microsoft.Extensions.Logging;

/// <summary>
/// Shared persistence logic for state containers
/// </summary>
internal static class PersistenceHelper {

	public static async Task<string?> InitializeAsync<TState, TStorage>(
		string containerIdKey,
		IStateManager stateManager,
		TStorage storageService,
		TState state,
		ILogger logger)
		where TState : IPersistableStateContainer
		where TStorage : IAsyncStorageService {

		var containerId = await EnsureStorageContainerKeyId(containerIdKey, storageService, logger);

		// Allow the underlying state to initialize
		state.Initialize();

		// Do not proceed if we could not get a containerId
		if (containerId.IsEmpty()) {
			logger.LogWarning("Failed to ensure storage container - state will not be persisted");
			return null;
		}

		// If we have a containerId, we can restore the state from storage
		await RestoreState(containerId, storageService, state, logger);

		// Subscribe to state changes to persist them
		stateManager.Subscribe<TState>(async stateInstance => {
			await SaveState(containerId, storageService, stateInstance, logger);
		});

		return containerId;

	}

	public static async Task SaveState<TState, TStorage>(
		string containerId,
		TStorage storageService,
		TState state,
		ILogger logger)
		where TState : IPersistableStateContainer
		where TStorage : IAsyncStorageService {

		logger.LogDebug("SaveStateAsync");
		logger.LogDebug("\tHas storage container id, serializing");
		var stateStr = state.SerializeToString();
		if (stateStr.HasValue()) {
			logger.LogDebug("\tHas serialized storage data, writing to storage...");
			await storageService.SetItemAsStringAsync(containerId, stateStr);
		}
	}

	public static async Task RestoreState<TState, TStorage>(
		string containerId,
		TStorage storageService,
		TState state,
		ILogger logger)
		where TState : IPersistableStateContainer
		where TStorage : IAsyncStorageService {

		logger.LogDebug("RestoreStateAsync");
		logger.LogDebug("\tFound existing storage container id, reading from storage");
		var stateStr = await storageService.GetItemAsStringAsync(containerId);
		if (stateStr.HasValue()) {
			logger.LogDebug("\tFound existing storage data, deserializing");
			state.DeserializeFromString(stateStr);
		}
	}

	private static async Task<string?> EnsureStorageContainerKeyId<TStorage>(
		string containerKeyId,
		TStorage storageService,
		ILogger logger)
		where TStorage : IAsyncStorageService {

		var containerId = await storageService.GetItemAsStringAsync(containerKeyId);
		if (containerId.HasValue()) {
			return containerId;
		}

		try {
			var newContainerId = Guid.NewGuid().ToString();
			await storageService.SetItemAsStringAsync(containerKeyId, newContainerId);
			return newContainerId;
		} catch (Exception ex) {
			logger.LogError(ex, "Unable to initialize state storage.");
			return null;
		}

	}

}