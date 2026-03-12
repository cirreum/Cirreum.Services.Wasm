namespace Cirreum;

using Cirreum.Authorization;
using Cirreum.Clock;
using Cirreum.FileSystem;
using Cirreum.Presence;
using Cirreum.Security;
using Cirreum.Storage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

public static partial class HostingExtensions {

	/// <summary>
	/// Registers and configures the core services.
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection"/>.</param>
	/// <param name="configureStorage">Optional Local Storage Options.</param>
	/// <returns>The <see cref="IServiceCollection"/>.</returns>
	/// <remarks>
	/// <para>
	/// Standard Security, User Access and Browser based storage.
	/// </para>
	/// <para>
	/// <see cref="IUserStateAccessor"/>, <see cref="IStorageSerializer"/>, 
	/// <see cref="ILocalStorageService"/>, <see cref="ISessionStorageService"/> and <see cref="StorageOptions"/>.
	/// </para>
	/// <para>
	/// Other services implemented: <see cref="IDateTimeClock"/>, <see cref="IWasmFileSystem"/>, <see cref="ICsvFileBuilder"/>, 
	/// <see cref="ICsvFileReader"/> and <see cref="ICspBuilder"/>.
	/// </para>
	/// </remarks>
	public static IServiceCollection AddCoreServices(this IServiceCollection services,
		Action<StorageOptions>? configureStorage = null) {

		//
		// Default SessionOptions
		//
		services
			.AddSessionOptions();

		//
		// User Access
		//
		services
			.AddUserAccessor();

		//
		// Browser Storage
		//
		services
			.AddClientStorage(configureStorage);

		//
		// User Presence
		//
		services
			.AddUserPresence();

		//
		// DateTime/Clock
		//
		services
			.AddDateTimeClock();

		//
		// WASM File System
		//
		services
			.AddFileSystem();

		//
		// Content Security Policy
		//
		services
			.AddContentSecurityPolicy();

		return services;

	}

	private static IServiceCollection AddSessionOptions(this IServiceCollection services) {

		services.TryAddSingleton(new SessionOptions());

		return services;

	}

	private static IServiceCollection AddUserAccessor(this IServiceCollection services) {

		if (services.Any(d => d.ServiceType == typeof(AuthenticationStateProvider))) {
			services.AddScoped<IUserStateAccessor, UserAccessor>();
		}

		return services;

	}

	private static IServiceCollection AddClientStorage(this IServiceCollection services, Action<StorageOptions>? configure) {

		services.TryAddScoped<IStorageSerializer, DefaultStorageSerializer>();
		services.TryAddScoped<ILocalStorageService, LocalStorageService>();
		services.TryAddScoped<ISessionStorageService, SessionStorageService>();
		services.Configure<StorageOptions>(configureOptions => {
			configure?.Invoke(configureOptions);
		});

		return services;

	}

	private static IServiceCollection AddUserPresence(this IServiceCollection services) {

		// We purposely set initial value to 0, so the monitor doesn't start.
		// A lib with a functional IUserPresenceService should expose a fluent
		// builder that leverages the Cirreum.Presence.UserPresenceBuilderExtensions
		// AddPresenceService which uses a Post-Configure for the desired value.
		services.Configure<UserPresenceMonitorOptions>(o => o.RefreshInterval = 0); // 0 == disabled
		services.TryAddScoped<IUserPresenceMonitor, DefaultUserPresenceMonitor>();

		// Add the default implementation which doesn't actually do anything.
		// A lib with a functional IUserPresenceService should expose a fluent
		// builder that leverages the Cirreum.Presence.UserPresenceBuilderExtensions
		// AddPresenceService which uses a Post-Configure for the desired value.
		services.TryAddScoped<IUserPresenceService, DefaultUserPresenceService>();

		return services;

	}

	private static IServiceCollection AddDateTimeClock(this IServiceCollection services) {

		services
			.TryAddSingleton(TimeProvider.System);
		services
			.TryAddScoped<IDateTimeClock, DateTimeService>();

		return services;

	}

	private static IServiceCollection AddFileSystem(this IServiceCollection services) {

		services.TryAddScoped<IWasmFileSystem, NewWasmFileSystem>();

		services.TryAddTransient<ICsvFileBuilder, CsvFileBuilder>();
		services.TryAddTransient<ICsvFileReader, CsvFileReader>();
		services.TryAddScoped<IFileSystem, NotImplementedFileSystem>();

		return services;

	}

	private static IServiceCollection AddContentSecurityPolicy(this IServiceCollection services) {

		services.TryAddScoped<ICspBuilder, CspBuilder>();

		return services;

	}

}