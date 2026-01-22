namespace Cirreum.State;

sealed class LocalState(
	IStateManager stateManager,
	IServiceProvider serviceProvider
) : PersistableStateContainer(serviceProvider)
  , ILocalState {
	protected override void OnStateHasChanged() {
		stateManager.NotifySubscribers<ILocalState>(this);
	}
}