namespace Cirreum.SystemInitializers;

using Cirreum.Startup;
using Cirreum.Storage;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

sealed class InitializeSessionStorageService : ISystemInitializer {
	public async ValueTask RunAsync(IServiceProvider serviceProvider) {
		var sessionStorage = serviceProvider.GetRequiredService<ISessionStorageService>();
		await sessionStorage.InitializeAsync();
	}
}