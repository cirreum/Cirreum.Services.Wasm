namespace Cirreum.State;

/// <summary>
/// Controls when startup tasks can begin their work.
/// </summary>
/// <remarks>
/// <para>
/// Applications implement this interface to define their own startup preconditions.
/// For example, an admin portal might require authentication before loading data,
/// while a public site might proceed immediately.
/// </para>
/// <para>
/// The framework provides <see cref="ImmediateStartupGate"/> as a default
/// implementation that opens immediately on application startup.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Wait for user authentication before proceeding
/// public class AuthenticatedStartupGate(
///     IUserState userState
/// ) : IStartupGate {
///
///     public IDisposable? OnReady(Func&lt;CancellationToken, Task&gt; callback) {
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
public interface IStartupGate {
	/// <summary>
	/// Registers a callback to be invoked when the gate opens.
	/// </summary>
	/// <param name="callback">The callback to invoke when work can proceed.</param>
	/// <returns>
	/// An <see cref="IDisposable"/> to unsubscribe from the ready notification,
	/// or <c>null</c> if the callback was invoked immediately.
	/// </returns>
	/// <remarks>
	/// If the gate is already open, implementations should invoke the callback
	/// synchronously and return <c>null</c>. Otherwise, implementations should
	/// subscribe to the appropriate state change and invoke the callback when
	/// the precondition is met.
	/// </remarks>
	IDisposable? WhenReady(Func<CancellationToken, Task> callback);
}