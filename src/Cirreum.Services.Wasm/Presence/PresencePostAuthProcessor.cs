namespace Cirreum.Presence;

using Cirreum.Security;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;

sealed class PresencePostAuthProcessor : IAuthenticationPostProcessor {

	public int Order { get; } = 300;

	public async Task ProcessAsync(IServiceProvider serviceProvider, IUserState userState, CancellationToken cancellationToken = default) {
		var presenceService = serviceProvider.GetService<IUserPresenceService>();
		if (presenceService != null && presenceService.IsEnabled) {
			await presenceService.UpdateUserPresence();
		}
	}

}