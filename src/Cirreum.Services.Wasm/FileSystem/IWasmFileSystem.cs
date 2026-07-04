namespace Cirreum.FileSystem;

/// <summary>
/// A browser-specific file system for Blazor WebAssembly — JS-interop initialization plus client-side
/// file download. Distinct from the general <see cref="IFileSystem"/> abstraction; this is Wasm-only and
/// was previously provided by the pre-reset Cirreum.Core monolith.
/// </summary>
public interface IWasmFileSystem {

	/// <summary>Loads the JS-interop module backing the file system. Call once during startup.</summary>
	ValueTask InitializeAsync();

	/// <summary>Triggers a client-side (browser) download of the given bytes as a file.</summary>
	Task DownloadFileAsync(byte[] data, string fileName, string contentType = "application/octet-stream");
}
