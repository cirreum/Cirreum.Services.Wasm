namespace Cirreum.FileSystem;

using Microsoft.JSInterop;
using System.Threading.Tasks;

sealed class NewWasmFileSystem : IWasmFileSystem, IDisposable {

	private static readonly string InteropUrl = "./_content/Cirreum.Services.Wasm/wasmFileSystem.js";
	private readonly IJSRuntime JSA;
	private IJSInProcessObjectReference? module;

	public NewWasmFileSystem(IJSRuntime js) {
		ArgumentNullException.ThrowIfNull(js);
		this.JSA = js;
	}

	public async ValueTask InitializeAsync() {
		this.module = await this.JSA.InvokeAsync<IJSInProcessObjectReference>("import", InteropUrl);
	}
	public void Dispose() {
		this.module?.Dispose();
	}


	public async Task DownloadFileAsync(byte[] data, string fileName, string contentType = "application/octet-stream") =>
		await this.module!.InvokeVoidAsync("downloadFile", data, fileName, contentType);

}