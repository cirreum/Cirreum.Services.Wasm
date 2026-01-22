namespace Cirreum.SystemInitializers;

using Cirreum.Startup;
using Cirreum.Storage;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

sealed class InitializeLocalStorageService : ISystemInitializer {
	public async ValueTask RunAsync(IServiceProvider serviceProvider) {
		var localStorage = serviceProvider.GetRequiredService<ILocalStorageService>();
		await localStorage.InitializeAsync();
	}
}