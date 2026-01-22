namespace Cirreum;

using Cirreum.Security;

/// <summary>
/// Foundation interface for processing after authentication completes.
/// </summary>
/// <remarks>
/// <para>
/// Processors are executed in ascending order based on the <see cref="Order"/> property.
/// Processors with the same order value may execute in any sequence.
/// </para>
/// <para>
/// This interface is designed to be used within the authentication pipeline where normal 
/// dependency injection patterns can cause circular dependencies. Implementations should 
/// use the provided <see cref="IServiceProvider"/> in the <see cref="ProcessAsync"/>
/// method for dependency resolution rather than constructor injection.
/// </para>
/// <para>
/// <strong>Important:</strong> HttpClient-based services (such as API clients) 
/// via constructor parameters are examples of what will cause circular dependency
/// exceptions during authentication.
/// </para>
/// </remarks>
public interface IAuthenticationPostProcessor {
	/// <summary>
	/// Gets the execution order for this processor. Processors are executed in ascending order.
	/// </summary>
	/// <value>
	/// The order value. Default implementations should use values like 100, 200, 300 to allow 
	/// for insertion of custom processors between framework processors.
	/// </value>
	int Order { get; }

	/// <summary>
	/// Process a newly authenticated user.
	/// </summary>
	/// <param name="serviceProvider">
	/// The service provider for resolving dependencies. Use this instead of constructor 
	/// injection to avoid circular dependencies during authentication.
	/// </param>
	/// <param name="userState">The current user state.</param>
	/// <param name="cancellationToken">
	/// A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.
	/// </param>
	/// <returns>A task that represents the asynchronous processing operation.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceProvider"/> or <paramref name="userState"/> is null.</exception>
	Task ProcessAsync(IServiceProvider serviceProvider, IUserState userState, CancellationToken cancellationToken = default);
}