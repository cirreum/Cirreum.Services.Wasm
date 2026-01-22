namespace Cirreum.State;

/// <summary>
/// The default data store initialization policy that allows immediate initialization.
/// </summary>
/// <remarks>
/// <para>
/// This policy permits data store initialization to proceed immediately when the
/// application starts, without waiting for any preconditions.
/// </para>
/// <para>
/// For applications that require conditions to be met before initialization (such as
/// user authentication), implement a custom <see cref="IStartupGate"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Use the default policy
/// services.AddClientState(state => {
///     state.AddDataStores()
///         .WithAutoInitialization()
///         .AddStore&lt;IEventsStore, EventsStore&gt;();
/// });
/// 
/// // Or provide a custom policy
/// services.AddClientState(state => {
///     state.AddDataStores()
///         .WithAutoInitialization&lt;AuthenticatedInitializationPolicy&gt;()
///         .AddStore&lt;IEventsStore, EventsStore&gt;();
/// });
/// </code>
/// </example>
/// <seealso cref="IStartupGate"/>
public sealed class ImmediateStartupGate : IStartupGate {

	/// <inheritdoc />
	public IDisposable? WhenReady(Func<CancellationToken, Task> callback) {
		var cts = new CancellationTokenSource();
		_ = callback(cts.Token);
		return cts;
	}

}