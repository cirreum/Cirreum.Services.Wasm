namespace Cirreum.State;

sealed class SessionState(
	IStateManager stateManager,
	IServiceProvider serviceProvider
) : PersistableStateContainer(serviceProvider)
  , ISessionState {
	protected override void OnStateHasChanged() {
		stateManager.NotifySubscribers<ISessionState>(this);
	}
}