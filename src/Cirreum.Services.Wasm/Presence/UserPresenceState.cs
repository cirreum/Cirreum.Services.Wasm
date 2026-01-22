namespace Cirreum.Presence;

using Cirreum.State;

sealed class UserPresenceState(
	IStateManager stateManager
) : ScopedNotificationState, IUserPresenceState {

	private UserPresence _presence = new(PresenceStatus.Unknown, "", "");

	/// <inheritdoc/>
	public UserPresence Presence => this._presence;

	/// <inheritdoc/>
	public void SetPresence(UserPresence presence) {
		using var _ = this.CreateNotificationScope();
		this._presence = presence;
	}

	protected override void OnStateHasChanged() {
		stateManager.NotifySubscribers<IUserPresenceState>(this);
	}

}