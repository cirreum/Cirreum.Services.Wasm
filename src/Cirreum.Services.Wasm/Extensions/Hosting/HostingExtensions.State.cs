namespace Cirreum;

using Cirreum.Presence;
using Cirreum.State;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

public static partial class HostingExtensions {

	/// <summary>
	/// Adds core client state services to the application with default configuration.
	/// </summary>
	/// <param name="builder">The client domain application builder.</param>
	/// <returns>The builder for method chaining.</returns>
	/// <remarks>
	/// <para>
	/// This method registers core client state services, including state management and predefined state
	/// sections such as theme, page, user presence, and memory, as well as state containers for memory,
	/// session and local backing stores.
	/// </para>
	/// <para>
	/// <strong>Built-in Decryption Support:</strong> The framework automatically registers 
	/// decryptors for <see cref="BuiltInEncryption.None"/> and <see cref="BuiltInEncryption.Base64Obfuscation"/> 
	/// to support migration from these common encryption schemes. Other built-in algorithms 
	/// (such as <see cref="BuiltInEncryption.XorObfuscation"/>) and custom encryption implementations 
	/// must be explicitly registered using <see cref="IStateBuilder.RegisterDecryptor"/> if migration 
	/// support is needed.
	/// </para>
	/// </remarks>
	public static IClientDomainApplicationBuilder AddClientState(
		this IClientDomainApplicationBuilder builder) {

		builder.Services.AddClientState();
		return builder;
	}

	/// <summary>
	/// Adds core client state services to the application with custom configuration.
	/// </summary>
	/// <param name="builder">The client domain application builder.</param>
	/// <param name="configureState">A delegate to configure additional application state.</param>
	/// <returns>The builder for method chaining.</returns>
	/// <remarks>
	/// <para>
	/// This method registers core client state services, including state management and predefined state
	/// sections such as theme, page, user presence, and memory, as well as state containers for memory,
	/// session and local backing stores. The <paramref name="configureState"/> parameter allows the caller
	/// to define and register additional application state as needed.
	/// </para>
	/// <para>
	/// <strong>Built-in Decryption Support:</strong> The framework automatically registers 
	/// decryptors for <see cref="BuiltInEncryption.None"/> and <see cref="BuiltInEncryption.Base64Obfuscation"/> 
	/// to support migration from these common encryption schemes. Other built-in algorithms 
	/// (such as <see cref="BuiltInEncryption.XorObfuscation"/>) and custom encryption implementations 
	/// must be explicitly registered using <see cref="IStateBuilder.RegisterDecryptor"/> if migration 
	/// support is needed.
	/// </para>
	/// <para>
	/// <strong>Example Usage:</strong>
	/// <code>
	/// builder.AddClientState(state => {
	///     state.AddState&lt;ICustomState, CustomState&gt;();
	///     state.RegisterDecryptor(StateEncryptionKinds.CUSTOM, new CustomDecryptor());
	/// });
	/// </code>
	/// </para>
	/// </remarks>
	public static IClientDomainApplicationBuilder AddClientState(
		this IClientDomainApplicationBuilder builder,
		Action<IStateBuilder> configureState) {

		builder.Services.AddClientState(configureState);
		return builder;
	}

	/// <summary>
	/// Adds core client state services to the service collection with default configuration.
	/// </summary>
	/// <param name="services">The service collection.</param>
	/// <returns>The service collection for method chaining.</returns>
	/// <remarks>
	/// <para>
	/// This is a convenience method that calls <see cref="AddClientState(IServiceCollection, Action{IStateBuilder})"/>
	/// with an empty configuration delegate, providing default settings for all state services.
	/// </para>
	/// </remarks>
	public static IServiceCollection AddClientState(
		this IServiceCollection services) {

		return services.AddClientState(_ => { /* no additional configuration */ });
	}

	/// <summary>
	/// Adds core client state services to the service collection with custom configuration.
	/// </summary>
	/// <param name="services">The service collection.</param>
	/// <param name="configureState">A delegate to configure additional application state.</param>
	/// <returns>The service collection for method chaining.</returns>
	/// <remarks>
	/// <para>
	/// This method registers core client state services, including state management and predefined state
	/// sections such as theme, page, user presence, and memory, as well as state containers for memory,
	/// session and local backing stores. The <paramref name="configureState"/> parameter allows the caller
	/// to define and register additional application state as needed.
	/// </para>
	/// <para>
	/// <strong>Built-in Services Registered:</strong>
	/// </para>
	/// <list type="bullet">
	///   <item><see cref="IStateManager"/> - Core state management service</item>
	///   <item><see cref="IThemeState"/> - Application theme state</item>
	///   <item><see cref="IPageState"/> - Page navigation and routing state</item>
	///   <item><see cref="IUserPresenceState"/> - User presence and activity state</item>
	///   <item><see cref="IMemoryState"/> - In-memory state container</item>
	///   <item><see cref="ISessionState"/> - Browser session storage state</item>
	///   <item><see cref="ILocalState"/> - Browser local storage state</item>
	/// </list>
	/// <para>
	/// <strong>Built-in Decryption Support:</strong> The framework automatically registers 
	/// decryptors for <see cref="BuiltInEncryption.None"/> and <see cref="BuiltInEncryption.Base64Obfuscation"/> 
	/// to support migration from these common encryption schemes. Other built-in algorithms 
	/// (such as <see cref="BuiltInEncryption.XorObfuscation"/>) and custom encryption implementations 
	/// must be explicitly registered using <see cref="IStateBuilder.RegisterDecryptor"/> if migration 
	/// support is needed.
	/// </para>
	/// <para>
	/// <strong>Thread Safety:</strong> All registered services use TryAdd* methods, making this method
	/// safe to call multiple times and allowing consumers to override default implementations.
	/// </para>
	/// <para>
	/// <strong>Example Usage:</strong>
	/// <code>
	/// services.AddClientState(state => {
	///     state.AddState&lt;ICustomState, CustomState&gt;();
	///     state.RegisterDecryptor(StateEncryptionKinds.CUSTOM, new CustomDecryptor());
	///     state.SetDefaultEncryption(BuiltInEncryption.Base64Obfuscation);
	/// });
	/// </code>
	/// </para>
	/// </remarks>
	public static IServiceCollection AddClientState(
		this IServiceCollection services,
		Action<IStateBuilder> configureState) {

		// Register built-in encryption providers that don't require keys
		services.TryAddKeyedSingleton(StateEncryptionKinds.NONE, BuiltInEncryption.None);
		services.TryAddKeyedSingleton(StateEncryptionKinds.BASE64, BuiltInEncryption.Base64Obfuscation);

		// Register default state container encryption (no encryption)
		services.TryAddSingleton(BuiltInEncryption.None);

		// Register initialization state service
		services.TryAddScoped<IInitializationState, InitializationState>();

		// Allow the user to register application state
		var stateBuilder = new StateBuilder(services);
		configureState(stateBuilder);

		// Register core state management service
		services.TryAddScoped<IStateManager, StateManager>();

		// Register foundational services		
		services.TryAddScoped<INotificationState, NotificationState>();
		services.TryAddScoped<IThemeState, ThemeState>();
		services.TryAddScoped<IPageState, PageState>();
		services.TryAddScoped<IUserPresenceState, UserPresenceState>();
		services.TryAddScoped<IMemoryState, MemoryState>();
		services.TryAddScoped<ISessionState, SessionState>();
		services.TryAddScoped<ILocalState, LocalState>();

		return services;

	}

}