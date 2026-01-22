namespace Cirreum.State.DataStores;

/// <summary>
/// Defines a data store that participates in application startup initialization.
/// </summary>
/// <remarks>
/// <para>
/// Stores implementing this interface are automatically discovered and initialized
/// during application startup by the <see cref="AutoInitializeStores"/> when
/// auto-initialization is enabled via <see cref="DataStoresBuilder.WithAutoInitialization()"/>.
/// </para>
/// <para>
/// The <see cref="DisplayName"/> and <see cref="InitializationMessage"/> properties
/// provide user-friendly status updates for splash screens, while <see cref="Order"/>
/// controls the initialization sequence.
/// </para>
/// </remarks>
/// <seealso cref="InitializableStore"/>
/// <seealso cref="AutoInitializeStores"/>
/// <seealso cref="IInitializationState"/>
public interface IInitializableStore : IDataStore {
	/// <summary>
	/// Gets the display name associated with the object.
	/// </summary>
	string DisplayName { get; }
	/// <summary>
	/// Gets the message that describes the initialization process. Sample: "Loading Events..."
	/// </summary>
	string InitializationMessage { get; }
	/// <summary>
	/// Gets the order in which the item appears relative to others.
	/// </summary>
	int Order { get; }
}