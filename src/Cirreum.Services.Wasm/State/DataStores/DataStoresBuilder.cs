namespace Cirreum.State.DataStores;

using Cirreum.State;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary>
/// A fluent builder for configuring data stores within the State infrastructure.
/// </summary>
/// <remarks>
/// <para>
/// This builder provides a fluent API for registering data stores and configuring
/// their initialization behavior. It is accessed via the <c>AddDataStores()</c>
/// extension method on <see cref="IStateBuilder"/>.
/// </para>
/// <para>
/// Data stores registered through this builder are automatically integrated with
/// the State notification system. Stores implementing <see cref="IInitializableStore"/>
/// are discovered for startup initialization when <see cref="WithAutoInitialization()"/>
/// is enabled.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// services.AddClientState(state => {
///     state.AddDataStores()
///         .WithAutoInitialization()
///         .AddStore&lt;IEventsStore, EventsStore&gt;()
///         .AddStore&lt;IProductsStore, ProductsStore&gt;();
/// });
/// 
/// // With a custom startup gate
/// services.AddClientState(state => {
///     state.AddDataStores()
///         .WithAutoInitialization&lt;AuthenticatedStartupGate&gt;()
///         .AddStore&lt;IEventsStore, EventsStore&gt;();
/// });
/// 
/// // Without auto-initialization (manual loading)
/// services.AddClientState(state => {
///     state.AddDataStores()
///         .AddStore&lt;IEventsStore, EventsStore&gt;();
/// });
/// </code>
/// </example>
/// <seealso cref="IDataStore"/>
/// <seealso cref="IInitializableStore"/>
/// <seealso cref="IStartupGate"/>
public class DataStoresBuilder {

	private readonly IServiceCollection _services;
	private readonly IStateBuilderWithDataStores _stateBuilder;

	internal DataStoresBuilder(IStateBuilderWithDataStores stateBuilder) {
		this._stateBuilder = stateBuilder;
		this._services = stateBuilder.Services;
	}

	/// <summary>
	/// Enables automatic initialization of data stores during application startup
	/// using the <see cref="ImmediateStartupGate"/>.
	/// </summary>
	/// <returns>The builder for method chaining.</returns>
	/// <remarks>
	/// <para>
	/// When enabled, all registered stores implementing <see cref="IInitializableStore"/>
	/// are automatically loaded during application startup. The initialization integrates
	/// with <see cref="IInitializationState"/> to provide progress updates
	/// for splash screens or loading indicators.
	/// </para>
	/// <para>
	/// The default gate opens immediately when the application starts.
	/// For applications requiring preconditions (such as user authentication), use
	/// <see cref="WithAutoInitialization{TGate}"/> with a custom gate.
	/// </para>
	/// </remarks>
	/// <seealso cref="WithAutoInitialization{TGate}"/>
	/// <seealso cref="ImmediateStartupGate"/>
	public DataStoresBuilder WithAutoInitialization() {
		return this.WithAutoInitialization<ImmediateStartupGate>();
	}

	/// <summary>
	/// Enables automatic initialization of data stores during application startup
	/// using a custom startup gate.
	/// </summary>
	/// <typeparam name="TGate">
	/// The type of <see cref="IStartupGate"/> that controls
	/// when initialization can proceed.
	/// </typeparam>
	/// <returns>The builder for method chaining.</returns>
	/// <remarks>
	/// <para>
	/// Use this overload to provide a custom gate that controls when data store
	/// initialization should occur. Common scenarios include waiting for user
	/// authentication before loading protected data.
	/// </para>
	/// </remarks>
	/// <example>
	/// <code>
	/// public class AuthenticatedStartupGate(
	///     IUserState userState
	/// ) : IStartupGate {
	///
	///     public IDisposable? WhenReady(Func&lt;CancellationToken, Task&gt; callback) {
	///         if (userState.IsAuthenticated) {
	///             _ = callback(CancellationToken.None);
	///             return null;
	///         }
	///         return userState.Subscribe(state => {
	///             if (state.IsAuthenticated) {
	///                 _ = callback(CancellationToken.None);
	///             }
	///         });
	///     }
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="IStartupGate"/>
	public DataStoresBuilder WithAutoInitialization<TGate>()
		where TGate : class, IStartupGate {
		this._services.TryAddTransient<IStartupGate, TGate>();
		return this;
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
	/// If the implementation type also implements <see cref="IInitializableStore"/>,
	/// it is automatically registered for discovery by the initialization system.
	/// When <see cref="WithAutoInitialization()"/> is enabled, these stores are
	/// loaded during application startup in the order specified by
	/// <see cref="IInitializableStore.Order"/>.
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
		if (typeof(IInitializableStore).IsAssignableFrom(typeof(TImplementation))) {
			this._services.AddScoped(sp =>
				(IInitializableStore)sp.GetRequiredService<TInterface>());
		}
		return this;
	}

}