namespace Cirreum.State;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Base implementation of state container functionality
/// </summary>
public abstract class StateContainer(
	IServiceProvider serviceProvider
) : ScopedNotificationState, IStateContainer {

	protected readonly Dictionary<string, IStateValueHandle> _handles = [];

	private IStateContainerEncryption? encryption;

	/// <summary>
	/// Encrypts/obfuscates a value before storage. Override to provide custom encryption.
	/// </summary>
	/// <param name="plaintext">The value to encrypt</param>
	/// <returns>The encrypted/obfuscated value</returns>
	protected virtual string Encrypt(string plaintext) {
		this.encryption ??= serviceProvider.GetRequiredService<IStateContainerEncryption>();
		var encrypted = this.encryption.Encrypt(plaintext);
		return encrypted + this.encryption.AlgorithmId;
	}

	/// <summary>
	/// Decrypts/de-obfuscates a value after retrieval. Override to provide custom decryption.
	/// </summary>
	/// <param name="ciphertext">The encrypted value</param>
	/// <returns>The decrypted/plain value</returns>
	protected virtual string Decrypt(string ciphertext) {

		if (ciphertext.Length < 1) {
			return ciphertext;
		}

		// Extract the algorithm ID from the end
		// Use spans for parsing to avoid substring allocations
		var ciphertextSpan = ciphertext.AsSpan();
		var algorithmId = ExtractAlgorithmId(ciphertextSpan);
		var encrypted = ciphertextSpan[..^algorithmId.Length];

		// Try to find a registered decryptor for this algorithm
		// First try as char (single character algorithm IDs)
		var decryptor = serviceProvider.GetKeyedService<IStateContainerEncryption>(algorithmId[0]);
		if (decryptor != null) {
			return decryptor.Decrypt(encrypted.ToString());
		}

		// Then try as string (multi-character algorithm IDs)  
		decryptor = serviceProvider.GetKeyedService<IStateContainerEncryption>(algorithmId.ToString());
		if (decryptor != null) {
			return decryptor.Decrypt(encrypted.ToString());
		}

		// Clear error message about missing migration
		throw new InvalidOperationException($"No migration path for algorithm '{algorithmId}'");

	}
	private static ReadOnlySpan<char> ExtractAlgorithmId(ReadOnlySpan<char> ciphertext) {
		var separatorIndex = ciphertext.LastIndexOf(IStateContainerEncryption.KindKeySeparator);
		if (separatorIndex > 0) {
			var kindIndex = separatorIndex - 1;
			return ciphertext[kindIndex..];
		}
		return ciphertext[^1..];
	}

	/// <summary>
	/// Creates a storage key from the provided key. Override to customize key formatting/hashing.
	/// </summary>
	/// <param name="originalKey">The original key provided by the caller</param>
	/// <returns>The key to use for actual storage operations</returns>
	protected virtual string CreateStorageKey(string originalKey) {
		// Default implementation: just return as-is (no hashing)
		return originalKey;
	}

	/// <inheritdoc/>
	public virtual void Initialize() {
		this._handles.Clear();
	}

	/// <inheritdoc/>
	public IStateValueHandle<T> GetOrCreate<T>(T initialValue) where T : notnull {
		var key = typeof(T).Name;
		return this.GetOrCreate<T>(key, initialValue);
	}
	/// <inheritdoc/>
	public virtual IStateValueHandle<T> GetOrCreate<T>(string key, T initialValue) where T : notnull {
		ArgumentException.ThrowIfNullOrEmpty(key);
		var storageKey = this.CreateStorageKey(key);
		if (!this._handles.TryGetValue(storageKey, out var existingHandle)) {
			var newHandle = new StateHandle<T>(
				this.GetInitialValue(key, storageKey, initialValue), // We have to do this since Get<T> is virtual
				this.NotifyHandleChanged
			);

			this._handles[storageKey] = newHandle;
			return newHandle;
		}

		return (IStateValueHandle<T>)existingHandle;
	}

	protected virtual T GetInitialValue<T>(string key, string storageKey, T defaultValue) where T : notnull {
		return defaultValue; // Base implementation: just return the default
	}

	/// <inheritdoc/>
	public T Get<T>(T defaultValue) where T : notnull {
		var key = typeof(T).Name;
		return this.Get<T>(key, defaultValue);
	}
	/// <inheritdoc/>
	public virtual T Get<T>(string key, T defaultValue) where T : notnull {

		var storageKey = this.CreateStorageKey(key);
		if (this._handles.TryGetValue(storageKey, out var existingHandle)) {
			if (existingHandle is IStateValueHandle<T> typedHandle) {
				return typedHandle.Value;
			}
			throw new InvalidOperationException($"Existing handle for key '{key}' is not of type {typeof(T).Name}");
		}

		// If no handle found, then first time reading, get initial value.
		return this.GetInitialValue(key, storageKey, defaultValue);

	}

	/// <inheritdoc/>
	public void Remove<T>() {
		var key = typeof(T).Name;
		this.Remove(key);
	}
	/// <inheritdoc/>
	public virtual void Remove(string key) {
		var storageKey = this.CreateStorageKey(key);
		this._handles.Remove(storageKey);
		this.NotifyStateChanged();
	}
	/// <inheritdoc/>
	public void Remove(params IEnumerable<string> keys) {
		using var scope = this.CreateNotificationScope();
		foreach (var key in keys) {
			this.Remove(key);
		}
	}

	/// <inheritdoc/>
	public Task Reset<T>(string key, T defaultValue) {
		var storageKey = this.CreateStorageKey(key);
		if (_handles.TryGetValue(storageKey, out var handle)) {
			if (handle is IStateValueHandle<T> typedHandle) {
				typedHandle.ResetValue(defaultValue);
				return Task.CompletedTask;
			}
		}
		return Task.CompletedTask;
	}

	private void NotifyHandleChanged() {
		this.NotifyStateChanged();
	}

}