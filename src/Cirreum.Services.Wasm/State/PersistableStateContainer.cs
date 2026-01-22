namespace Cirreum.State;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

/// <summary>
/// State container with serialization support
/// </summary>
public abstract class PersistableStateContainer(
	IServiceProvider serviceProvider
) : StateContainer(serviceProvider)
  , IPersistableStateContainer {

	private readonly Dictionary<string, JsonElement> _values = [];

	/// <inheritdoc/>
	public override void Initialize() {
		this._values.Clear();
		base.Initialize();
	}

	/// <inheritdoc/>
	public override IStateValueHandle<T> GetOrCreate<T>(string key, T initialValue) {
		SerializationHelper.ValidateType<T>();
		return base.GetOrCreate<T>(key, initialValue);
	}

	protected override T GetInitialValue<T>(string key, string storageKey, T defaultValue) {

		// Check for deserialized value
		if (this._values.TryGetValue(storageKey, out var jsonElement)) {
			try {
				this._values.Remove(key); // remove!
				var deserializedValueOrNull = JsonSerializer.Deserialize<T>(jsonElement);
				return deserializedValueOrNull ?? defaultValue;
			} catch (Exception ex) when (ex is not InvalidOperationException) {
				throw new InvalidOperationException($"Failed to deserialize value for key '{key}' as {typeof(T).Name}", ex);
			}
		}

		return defaultValue; // No persisted data found

	}

	/// <inheritdoc/>
	public override void Remove(string key) {
		this._values.Remove(key);
		base.Remove(key);
	}

	/// <inheritdoc/>
	public string SerializeToString() {
		var serializableValues = this._handles.ToDictionary(
			kvp => kvp.Key,
			kvp => ((ISerializableStateHandle)kvp.Value).GetSerializableValue()
		);
		var json = JsonSerializer.Serialize(serializableValues);
		return this.Encrypt(json); // Encrypt the entire JSON string
	}

	/// <inheritdoc/>
	public void DeserializeFromString(string value) {
		this._values.Clear();

		// Decrypt the entire blob first
		var decryptedJson = this.Decrypt(value);

		var deserializedValues = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(decryptedJson);
		if (deserializedValues == null) {
			return;
		}

		foreach (var (key, jsonElement) in deserializedValues) {
			this._values[key] = jsonElement;
		}
	}

	protected override string CreateStorageKey(string originalKey) {

		var hash = SHA256.HashData(Encoding.UTF8.GetBytes(originalKey));
		var base64 = Convert.ToBase64String(hash)[..12];

		// Split by colon and filter out empty segments
		var segments = originalKey.Split(':', StringSplitOptions.RemoveEmptyEntries);

		if (segments.Length >= 2) {
			// Take first two non-empty segments as prefix
			return $"{segments[0]}:{segments[1]}:k{base64}";
		} else if (segments.Length == 1) {
			// Single segment format
			return $"{segments[0]}:k{base64}";
		}

		return $"k{base64}"; // Fallback for edge cases

	}

}