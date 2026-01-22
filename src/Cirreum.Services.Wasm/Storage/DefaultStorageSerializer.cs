namespace Cirreum.Storage;

using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

sealed class DefaultStorageSerializer : IStorageSerializer {

	private readonly JsonSerializerOptions _options;

	public DefaultStorageSerializer(IOptions<StorageOptions> options) {
		_options = options.Value.JsonSerializerOptions;
	}

	public DefaultStorageSerializer(StorageOptions localStorageOptions) {
		_options = localStorageOptions.JsonSerializerOptions;
	}

	public T? Deserialize<T>(string data)
		=> JsonSerializer.Deserialize<T>(data, _options);

	public T? Deserialize<T>(string data, JsonTypeInfo<T> typeInfo)
		=> JsonSerializer.Deserialize<T>(data, typeInfo);

	public string Serialize<T>(T data)
		=> JsonSerializer.Serialize(data, _options);

	public string Serialize<T>(T data, JsonTypeInfo<T> typeInfo)
		=> JsonSerializer.Serialize(data, typeInfo);

}