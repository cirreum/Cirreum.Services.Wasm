namespace Cirreum.Storage;

using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

sealed class LocalStorageService : ILocalStorageService, IDisposable {

	private static readonly string InteropUrl = "./_content/Cirreum.Services.Wasm/localStorageProvider.js";
	private readonly IJSRuntime JSA;
	private IJSInProcessObjectReference? module;

	public LocalStorageService(
		IJSRuntime js,
		IStorageSerializer serializer) {
		this._serializer = serializer;
		ArgumentNullException.ThrowIfNull(js);
		this.JSA = js;
	}

	public async ValueTask InitializeAsync() {
		this.module = await this.JSA.InvokeAsync<IJSInProcessObjectReference>("import", InteropUrl);
	}
	public void Dispose() {
		this.module?.Dispose();
	}


	public event EventHandler<StorageChangedEventArgs> Changed = default!;
	public event EventHandler<StorageChangingEventArgs> Changing = default!;

	private readonly IStorageSerializer _serializer;

	public async Task<IEnumerable<string>> KeysAsync() {
		return await this.module!.InvokeAsync<IEnumerable<string>>("keys");
	}

	public async Task SetItemAsync<T>(string key, T data) {
		if (string.IsNullOrWhiteSpace(key)) {
			throw new ArgumentNullException(nameof(key));
		}

		ArgumentNullException.ThrowIfNull(data);

		var oldValue = await this.GetItemInternalAsync(key);
		var e = this.RaiseOnChanging(key, data, oldValue);

		if (e.Cancel) {
			return;
		}

		var serialisedData = this._serializer.Serialize(data);
		await this.module!.InvokeVoidAsync("setItem", key, serialisedData);

		this.RaiseOnChanged(key, e.OldValue, data);
	}

	public async Task SetItemAsync<T>(string key, T data, JsonTypeInfo<T> typeInfo) {
		if (string.IsNullOrWhiteSpace(key)) {
			throw new ArgumentNullException(nameof(key));
		}

		ArgumentNullException.ThrowIfNull(data);

		var oldValue = await this.GetItemInternalAsync(key);
		var e = this.RaiseOnChanging(key, data, oldValue);

		if (e.Cancel) {
			return;
		}

		var serialisedData = this._serializer.Serialize(data, typeInfo);
		await this.module!.InvokeVoidAsync("setItem", key, serialisedData);

		this.RaiseOnChanged(key, e.OldValue, data);
	}

	public async Task SetItemAsStringAsync(string key, string data) {
		if (string.IsNullOrWhiteSpace(key)) {
			throw new ArgumentNullException(nameof(key));
		}

		ArgumentNullException.ThrowIfNull(data);

		var oldValue = await this.GetItemInternalAsync(key);
		var e = this.RaiseOnChanging(key, data, oldValue);

		if (e.Cancel) {
			return;
		}

		await this.module!.InvokeVoidAsync("setItem", key, data);

		this.RaiseOnChanged(key, e.OldValue, data);
	}

	public async Task<T?> GetItemAsync<T>(string key) {
		if (string.IsNullOrWhiteSpace(key)) {
			throw new ArgumentNullException(nameof(key));
		}

		var serialisedData = await this.module!.InvokeAsync<string>("getItem", key);

		if (string.IsNullOrWhiteSpace(serialisedData)) {
			return default;
		}

		try {
			return this._serializer.Deserialize<T>(serialisedData);
		} catch (JsonException e) when (e.Path == "$" && typeof(T) == typeof(string)) {
			return (T)(object)serialisedData;
		}
	}

	public async Task<T?> GetItemAsync<T>(string key, JsonTypeInfo<T> typeInfo) {
		if (string.IsNullOrWhiteSpace(key)) {
			throw new ArgumentNullException(nameof(key));
		}

		var serialisedData = await this.module!.InvokeAsync<string>("getItem", key);

		if (string.IsNullOrWhiteSpace(serialisedData)) {
			return default;
		}

		try {
			return this._serializer.Deserialize<T>(serialisedData, typeInfo);
		} catch (JsonException e) when (e.Path == "$" && typeof(T) == typeof(string)) {
			return (T)(object)serialisedData;
		}
	}

	public async Task<string?> GetItemAsStringAsync(string key) {
		if (string.IsNullOrWhiteSpace(key)) {
			throw new ArgumentNullException(nameof(key));
		}

		return await this.module!.InvokeAsync<string?>("getItem", key);
	}

	public async Task RemoveItemAsync(string key) {
		if (string.IsNullOrWhiteSpace(key)) {
			throw new ArgumentNullException(nameof(key));
		}

		await this.module!.InvokeVoidAsync("removeItem", key);
	}

	public async Task RemoveItemsAsync(IEnumerable<string> keys) {
		ArgumentNullException.ThrowIfNull(keys);
		if (keys.Any()) {
			await this.module!.InvokeVoidAsync("removeItems", keys);
		}
	}

	public async Task ClearAsync() =>
		await this.module!.InvokeVoidAsync("clear");

	public async Task<int> LengthAsync() =>
		await this.module!.InvokeAsync<int>("length");

	public async Task<string?> KeyAsync(int index) =>
		await this.module!.InvokeAsync<string?>("key", index);

	public async Task<bool> ContainsKeyAsync(string key) {
		ArgumentNullException.ThrowIfNull(key);
		return await this.module!.InvokeAsync<bool>("containsKey", key);
	}

	private StorageChangingEventArgs RaiseOnChanging(string key, object data, object? oldValue) {
		var e = new StorageChangingEventArgs {
			Key = key,
			OldValue = oldValue,
			NewValue = data
		};

		Changing?.Invoke(this, e);

		return e;
	}

	private async Task<object?> GetItemInternalAsync(string key) {
		if (string.IsNullOrEmpty(key)) {
			throw new ArgumentNullException(nameof(key));
		}

		var serialisedData = await this.module!.InvokeAsync<string>("getItem", key);

		if (string.IsNullOrWhiteSpace(serialisedData)) {
			return default;
		}

		try {
			return this._serializer.Deserialize<object>(serialisedData);
		} catch (JsonException) {
			return serialisedData;
		}
	}

	private void RaiseOnChanged(string key, object? oldValue, object data) {
		var e = new StorageChangedEventArgs {
			Key = key,
			OldValue = oldValue,
			NewValue = data
		};

		Changed?.Invoke(this, e);
	}

}