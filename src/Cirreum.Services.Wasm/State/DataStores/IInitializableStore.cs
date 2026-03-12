namespace Cirreum.State.DataStores;

/// <summary>
/// Defines a data store that participates in application startup initialization.
/// </summary>
/// <remarks>
/// <para>
/// Stores implementing this interface are automatically discovered and initialized
/// during application startup by the <see cref="IInitializationOrchestrator"/>.
/// </para>
/// <para>
/// This interface combines <see cref="IDataStore"/> (for data loading and state management)
/// with <see cref="IInitializable"/> (for ordered startup initialization with progress tracking).
/// The <see cref="IInitializable.InitializeAsync"/> implementation delegates to
/// <see cref="IDataStore.LoadAsync"/> on the base <see cref="InitializableStore"/> class.
/// </para>
/// </remarks>
/// <seealso cref="InitializableStore"/>
/// <seealso cref="IInitializationOrchestrator"/>
/// <seealso cref="IInitializationState"/>
public interface IInitializableStore : IDataStore, IInitializable;