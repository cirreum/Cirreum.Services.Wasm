namespace Cirreum.Presence;
sealed class DefaultUserPresenceService : IUserPresenceService {
	public bool IsEnabled { get; } = false;
	public Task UpdateUserPresence() {
		return Task.CompletedTask;
	}
}