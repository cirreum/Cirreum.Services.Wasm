namespace Cirreum.Security;

/// <summary>
/// Defines a contract for loading the application user from identity information.
/// </summary>
public interface IApplicationUserFactory {
	/// <summary>
	/// Asynchronously attempts to load an application user for the specified identity.
	/// </summary>
	/// <param name="userState">The current <see cref="IUserState"/>.</param>
	/// <param name="cancellationToken">
	/// A token to monitor for cancellation requests.
	/// </param>
	/// <returns>
	/// A task that represents the asynchronous load operation. The task result contains
	/// the resolved application user when successful.
	/// </returns>
	/// <exception cref="ArgumentException">
	/// Thrown when <paramref name="userState"/> is null.
	/// </exception>
	Task<Result<IApplicationUser>> CreateUserAsync(
		IUserState userState,
		CancellationToken cancellationToken = default);
}