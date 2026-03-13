namespace Cirreum.State;

/// <summary>
/// Abstract base class for client-side remote state that manages in-memory domain data.
/// </summary>
/// <remarks>
/// <para>
/// This class provides the foundational infrastructure for managing loading states
/// and coordinating state change notifications. Derived classes should implement
/// domain-specific data loading and storage logic.
/// </para>
/// <para>
/// The class inherits from <see cref="ScopedNotificationState"/> to leverage the
/// framework's notification batching and state change propagation mechanisms.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class ProductsState(
///     IProductApi api,
///     IStateManager stateManager
/// ) : RemoteState, IProductsState {
///
///     public IReadOnlyList&lt;Product&gt; Products { get; private set; } = [];
///
///     protected override async Task LoadCoreAsync(CancellationToken cancellationToken) {
///         Products = await api.GetAllAsync(cancellationToken);
///     }
///
///     protected override void OnStateHasChanged() {
///         stateManager.NotifySubscribers&lt;IProductsState&gt;(this);
///     }
/// }
/// </code>
/// </example>
public abstract class RemoteState : ScopedNotificationState, IRemoteState {

	/// <inheritdoc />
	public bool IsLoaded { get; protected set; }

	/// <inheritdoc />
	public bool IsLoading { get; protected set; }

	/// <inheritdoc />
	public bool IsRefreshing { get; protected set; }

	/// <inheritdoc />
	public Task LoadAsync() {
		return this.LoadAsync(CancellationToken.None);
	}

	/// <inheritdoc />
	public async Task LoadAsync(CancellationToken cancellationToken) {
		if (this.IsLoading || this.IsLoaded) {
			return;
		}
		await this.ExecuteWithLoadingState(
			() => this.LoadCoreAsync(cancellationToken),
			isRefresh: false);
	}

	/// <inheritdoc />
	public Task RefreshAsync() {
		return this.RefreshAsync(CancellationToken.None);
	}

	/// <inheritdoc />
	public async Task RefreshAsync(CancellationToken cancellationToken) {
		if (this.IsLoading || this.IsRefreshing) {
			return;
		}
		await this.ExecuteWithLoadingState(
			() => this.LoadCoreAsync(cancellationToken),
			isRefresh: true);
	}

	/// <summary>
	/// When implemented in a derived class, performs the actual data loading operation.
	/// </summary>
	protected abstract Task LoadCoreAsync(CancellationToken cancellationToken);

	/// <summary>
	/// Executes the specified asynchronous operation while managing loading and refreshing state indicators.
	/// </summary>
	/// <remarks>During execution, the method updates loading or refreshing state properties and notifies listeners
	/// of state changes. The state is reset after the operation completes, regardless of success or failure.</remarks>
	/// <param name="operation">A delegate representing the asynchronous operation to execute. Cannot be null.</param>
	/// <param name="isRefresh">true to indicate the operation is a refresh action; otherwise, false to indicate a standard loading action. The
	/// appropriate state indicator will be set accordingly.</param>
	/// <returns>A task that represents the asynchronous execution of the operation.</returns>
	private async Task ExecuteWithLoadingState(Func<Task> operation, bool isRefresh = false) {
		if (isRefresh) {
			this.IsRefreshing = true;
		} else {
			this.IsLoading = true;
		}
		this.NotifyStateChanged();

		using var scope = this.CreateNotificationScope();
		try {
			await operation();
			this.IsLoaded = true;
		} finally {
			this.IsLoading = false;
			this.IsRefreshing = false;
		}
	}

}