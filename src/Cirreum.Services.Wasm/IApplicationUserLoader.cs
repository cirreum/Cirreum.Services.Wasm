namespace Cirreum;

/// <summary>
/// Defines a contract for loading application user data based on identity information.
/// </summary>
/// <typeparam name="T">The type of user entity to load. Must be a reference type that
/// implements <see cref="IApplicationUser"/>.</typeparam>
/// <remarks>
/// <para>
/// This interface is designed to be used within the authentication pipeline where normal 
/// dependency injection patterns can cause circular dependencies. Implementations should 
/// use the provided <see cref="IServiceProvider"/> in the <see cref="TryLoadUserAsync"/>
/// method for dependency resolution rather than constructor injection.
/// </para>
/// <para>
/// <strong>Important:</strong> HttpClient-based services (such as API clients) 
/// via constructor parameters are examples of what will cause circular dependency
/// exceptions during authentication.
/// </para>
/// </remarks>
public interface IApplicationUserLoader<T> where T : class, IApplicationUser {
	/// <summary>
	/// Asynchronously attempts to load a user of type <typeparamref name="T"/> based on the
	/// provided identity identifier.
	/// </summary>
	/// <param name="serviceProvider">
	/// The service provider for resolving dependencies. Use this instead of constructor 
	/// injection to avoid circular dependencies during authentication.
	/// </param>
	/// <param name="identityId">The unique identifier for the user's identity.</param>
	/// <param name="cancellationToken">
	/// A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.
	/// </param>
	/// <returns>
	/// A task that represents the asynchronous load operation. The task result contains a <see cref="Result{T}"/>
	/// indicating success with the user entity if found, or failure with the associated exception if the operation fails.
	/// </returns>
	/// <exception cref="ArgumentException">Thrown when <paramref name="identityId"/> is null, empty, or whitespace.</exception>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceProvider"/> is null.</exception>
	Task<Result<T>> TryLoadUserAsync(IServiceProvider serviceProvider, string identityId, CancellationToken cancellationToken = default);
}