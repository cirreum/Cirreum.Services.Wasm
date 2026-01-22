namespace Cirreum.SystemInitializers;

using Cirreum.FileSystem;
using Cirreum.Startup;
using Microsoft.Extensions.DependencyInjection;

sealed class InitializeWasmFileSystem : ISystemInitializer {
	public async ValueTask RunAsync(IServiceProvider serviceProvider) {
		var wasmFileSystem = serviceProvider.GetRequiredService<IWasmFileSystem>();
		await wasmFileSystem.InitializeAsync();
	}
}