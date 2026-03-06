namespace Cirreum.Presence;

sealed class UserPresenceState(
	IStateManager stateManager
) : ScopedNotificationState, IUserPresenceState {

	private UserPresence _presence = new(PresenceStatus.Unknown, "", "");

	/// <inheritdoc/>
	public UserPresence Presence => this._presence;

	/// <inheritdoc/>
	public void SetPresence(UserPresence presence) {
		this._presence = presence;
		this.NotifyStateChanged();
	}

	protected override void OnStateHasChanged() {
		stateManager.NotifySubscribers<IUserPresenceState>(this);
	}

}