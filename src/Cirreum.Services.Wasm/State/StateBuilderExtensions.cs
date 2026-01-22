namespace Cirreum.State;

using Cirreum.State.DataStores;

public static class StateBuilderExtensions {

	extension(IStateBuilder builder) {

		/// <summary>
		/// Creates a builder for configuring data stores within the State infrastructure.
		/// </summary>
		/// <returns>
		/// A <see cref="DataStoresBuilder"/> for registering data stores and configuring
		/// initialization behavior.
		/// </returns>
		/// <remarks>
		/// <para>
		/// Data stores provide in-memory caching for domain data fetched from backend services.
		/// Use the returned builder to register stores and optionally enable automatic
		/// initialization during application startup.
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
		/// </code>
		/// </example>
		/// <exception cref="InvalidOperationException">
		/// Thrown when the <see cref="IStateBuilder"/> is not a <see cref="StateBuilder"/> instance.
		/// </exception>
		/// <seealso cref="DataStoresBuilder"/>
		/// <seealso cref="IDataStore"/>
		public DataStoresBuilder AddDataStores() {
			if (builder is IStateBuilderWithDataStores dataStoreBuilder) {
				return new DataStoresBuilder(dataStoreBuilder);
			}
			throw new InvalidOperationException(
				$"IStateBuilder implementation must also implement {nameof(IStateBuilderWithDataStores)} to support data stores.");
		}

	}

}