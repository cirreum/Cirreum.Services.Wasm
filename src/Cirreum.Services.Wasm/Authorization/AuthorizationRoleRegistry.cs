namespace Cirreum.Authorization;

using Cirreum.Startup;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

sealed class AuthorizationRoleRegistry(
	ILogger<AuthorizationRoleRegistry> logger
) : AuthorizationRoleRegistryBase(logger)
  , IAutoInitialize {

	/// <inheritdoc/>
	public ValueTask InitializeAsync() {
		return this.DefaultInitializationAsync();
	}

}