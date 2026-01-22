namespace Cirreum.State.DataStores;

/// <summary>
/// Defines the core contract for a data store that manages in-memory domain data
/// fetched from backend services.
/// </summary>
/// <remarks>
/// <para>
/// Data stores are a specialized extension of the State infrastructure, designed for
/// managing domain data that is loaded from APIs and cached in memory for the lifetime
/// of the application. They leverage the State notification system to propagate changes
/// to subscribers.
/// </para>
/// <para>
/// While other State components like <see cref="ISessionState"/> and <see cref="ILocalState"/>
/// manage application state with browser storage persistence, data stores focus on
/// transient domain data with no persistence - the data is fetched fresh on each
/// application startup.
/// </para>
/// <para>
/// Data stores provide a caching layer for API responses, eliminating the need
/// for individual pages to manage their own loading states and data fetching logic.
/// Combined with <see cref="IInitializableStore"/>, they integrate with the startup
/// pipeline to pre-load data before the user interacts with the application.
/// </para>
/// </remarks>
/// <seealso cref="DataStore"/>
/// <seealso cref="IInitializableStore"/>
/// <seealso cref="IInitializationState"/>
public interface IDataStore : IApplicationState {

	/// <summary>
	/// Gets a value indicating whether the data has been successfully loaded.
	/// </summary>
	/// <remarks>
	/// This property is <c>true</c> after the initial load operation completes successfully,
	/// and remains <c>true</c> even during subsequent refresh operations.
	/// </remarks>
	bool IsLoaded { get; }

	/// <summary>
	/// Gets a value indicating whether the initial data loading operation is in progress.
	/// </summary>
	/// <remarks>
	/// This property is <c>true</c> only during the first load operation.
	/// For subsequent data fetches, see <see cref="IsRefreshing"/>.
	/// </remarks>
	bool IsLoading { get; }

	/// <summary>
	/// Gets a value indicating whether the data is being refreshed.
	/// </summary>
	/// <remarks>
	/// This property is <c>true</c> when fetching updated data after the initial load.
	/// Use this to show non-blocking refresh indicators while preserving existing data display.
	/// </remarks>
	bool IsRefreshing { get; }

	/// <summary>
	/// Loads data if not already loaded or loading.
	/// </summary>
	Task LoadAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Refreshes data if not already loading or refreshing.
	/// </summary>
	Task RefreshAsync(CancellationToken cancellationToken = default);

}