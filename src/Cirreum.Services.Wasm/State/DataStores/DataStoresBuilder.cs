namespace Cirreum.State.DataStores;

using Cirreum.State;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// A fluent builder for configuring data stores within the State infrastructure.
/// </summary>
/// <remarks>
/// <para>
/// This builder provides a fluent API for registering data stores. It is accessed
/// via the <c>AddDataStores()</c> extension method on <see cref="IStateBuilder"/>.
/// </para>
/// <para>
/// Data stores registered through this builder are automatically integrated with
/// the State notification system. Stores implementing <see cref="IInitializableStore"/>
/// (which extends <see cref="IInitializable"/>) are automatically registered for
/// discovery by the <see cref="IInitializationOrchestrator"/> and initialized
/// during application startup in the order specified by <see cref="IInitializable.Order"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// services.AddClientState(state => {
///     state.AddDataStores()
///         .AddStore&lt;IEventsStore, EventsStore&gt;()
///         .AddStore&lt;IProductsStore, ProductsStore&gt;();
/// });
/// </code>
/// </example>
/// <seealso cref="IDataStore"/>
/// <seealso cref="IInitializableStore"/>
/// <seealso cref="IInitializationOrchestrator"/>
public class DataStoresBuilder {

	private readonly IServiceCollection _services;
	private readonly IStateBuilderWithDataStores _stateBuilder;

	internal DataStoresBuilder(IStateBuilderWithDataStores stateBuilder) {
		this._stateBuilder = stateBuilder;
		this._services = stateBuilder.Services;
	}

	/// <summary>
	/// Registers a data store with the dependency injection container.
	/// </summary>
	/// <typeparam name="TInterface">The interface type for the data store.</typeparam>
	/// <typeparam name="TImplementation">The implementation type for the data store.</typeparam>
	/// <returns>The builder for method chaining.</returns>
	/// <remarks>
	/// <para>
	/// The data store is registered as a scoped service and integrated with the
	/// State notification system via <see cref="IStateBuilder.RegisterState{TInterface, TImplementation}"/>.
	/// </para>
	/// <para>
	/// If the implementation type also implements <see cref="IInitializable"/>
	/// (typically via <see cref="IInitializableStore"/> and <see cref="InitializableStore"/>),
	/// it is automatically registered for discovery by the <see cref="IInitializationOrchestrator"/>.
	/// These stores are loaded during application startup in the order specified by
	/// <see cref="IInitializable.Order"/>.
	/// </para>
	/// </remarks>
	/// <example>
	/// <code>
	/// state.AddDataStores()
	///     .AddStore&lt;IEventsStore, EventsStore&gt;()
	///     .AddStore&lt;IProductsStore, ProductsStore&gt;();
	/// </code>
	/// </example>
	/// <seealso cref="IDataStore"/>
	/// <seealso cref="IInitializableStore"/>
	public DataStoresBuilder AddStore<TInterface, TImplementation>()
		where TInterface : class, IDataStore
		where TImplementation : class, TInterface {
		this._stateBuilder.RegisterState<TInterface, TImplementation>();
		if (typeof(IInitializable).IsAssignableFrom(typeof(TImplementation))) {
			this._services.AddScoped(sp =>
				(IInitializable)sp.GetRequiredService<TInterface>());
		}
		return this;
	}

}
