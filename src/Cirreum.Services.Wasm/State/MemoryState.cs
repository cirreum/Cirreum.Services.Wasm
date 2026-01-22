namespace Cirreum.State;

sealed class MemoryState(
	IStateManager stateManager,
	IServiceProvider serviceProvider
) : StateContainer(serviceProvider)
  , IMemoryState {
	protected override void OnStateHasChanged() {
		stateManager.NotifySubscribers<IMemoryState>(this);
	}
}