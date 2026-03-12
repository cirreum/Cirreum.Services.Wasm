namespace Cirreum.State.DataStores;

using Cirreum.Security;

/// <summary>
/// Abstract base class for data stores that participate in application startup initialization.
/// </summary>
/// <remarks>
/// <para>
/// This class extends <see cref="DataStore"/> with metadata required for startup initialization.
/// Stores inheriting from this class are automatically discovered and initialized during
/// application startup by the <see cref="IInitializationOrchestrator"/>.
/// </para>
/// <para>
/// The <see cref="DisplayName"/> property provides user-friendly status messages during
/// initialization, while <see cref="Order"/> controls the sequence in which stores are loaded.
/// </para>
/// <para>
/// The <see cref="IInitializable.InitializeAsync"/> implementation delegates to
/// <see cref="DataStore.LoadAsync"/>, so derived classes only need to implement
/// <see cref="DataStore.LoadCoreAsync"/> as they do for any data store.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class EventsStore(
///     IDispatcher dispatcher,
///     IStateManager stateManager
/// ) : InitializableStore, IEventsStore {
///
///     public override string DisplayName => "Events";
///     public override string InitializationMessage => "Loading events...";
///     public override int Order => 100;
///
///     public IReadOnlyList&lt;Event&gt; Events { get; private set; } = [];
///
///     protected override async Task LoadCoreAsync(CancellationToken cancellationToken) {
///         var results = await dispatcher.DispatchAsync(GetAllEvents.Query, cancellationToken);
///         this.Events = results.IsSuccess ? results.Value : [];
///     }
///
///     protected override void OnStateHasChanged() {
///         stateManager.NotifySubscribers&lt;IEventsStore&gt;(this);
///     }
/// }
/// </code>
/// </example>
/// <seealso cref="DataStore"/>
/// <seealso cref="IInitializableStore"/>
/// <seealso cref="IInitializationOrchestrator"/>
public abstract class InitializableStore : DataStore, IInitializableStore {

	/// <inheritdoc />
	public abstract string DisplayName { get; }

	/// <inheritdoc />
	public abstract string InitializationMessage { get; }

	/// <inheritdoc />
	public virtual int Order => 1000;

	/// <summary>
	/// Data stores always participate in initialization.
	/// </summary>
	bool IInitializable.ShouldInitialize(IUserState userState) => true;

	/// <summary>
	/// Delegates to <see cref="DataStore.LoadAsync"/> for initialization.
	/// </summary>
	Task IInitializable.InitializeAsync(Action<string> updateStatus, CancellationToken cancellationToken) =>
		this.LoadAsync(cancellationToken);

}
