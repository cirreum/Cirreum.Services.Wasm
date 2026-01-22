namespace Cirreum;

using Cirreum.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;

public static partial class HostingExtensions {

	/// <summary>
	/// Adds session monitoring services.
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection"/> to which the session monitoring services will be added.</param>
	/// <param name="configure">An optional delegate to configure the <see cref="SessionOptions"/>. If not provided, default options will be used.</param>
	/// <remarks>
	/// This method registers the necessary services for monitoring session activity, including HTTP
	/// monitoring and session-specific configuration. By default, session monitoring is enabled, but the
	/// behavior can be customized using the <paramref name="configure"/> delegate. HTTP monitoring is only
	/// registered when <see cref="SessionOptions.TrackApiCalls"/> is enabled to avoid unnecessary overhead.
	/// </remarks>
	/// <returns>The <see cref="IServiceCollection"/> for method chaining.</returns>
	public static IServiceCollection AddSessionMonitoring(
		this IServiceCollection services,
		Action<SessionOptions>? configure = null) {

		// Ensure we have ClientState
		services.AddClientState();

		// Configure options
		var options = new SessionOptions {
			Enabled = true
		};
		configure?.Invoke(options);

		// Validate configuration
		ValidateOptions(options);

		// Register the configured options as a Singleton
		services.Replace(ServiceDescriptor.Singleton(options));

		// Register Session Management scoped
		services.AddScoped<ISessionManager, SessionManager>();

		// Register HTTP monitoring if enabled
		if (options.TrackApiCalls) {
			services.AddScoped<SessionHttpHandler>();
			services.ConfigureAll<HttpClientFactoryOptions>(options => {
				options.HttpMessageHandlerBuilderActions.Add(builder => {
					builder.AdditionalHandlers.Add(
						builder.Services.GetRequiredService<SessionHttpHandler>()
					);
				});
			});
		}

		return services;

	}

	/// <summary>
	/// Adds session monitoring services with strongly-typed configuration.
	/// </summary>
	/// <param name="services">The service collection.</param>
	/// <param name="configurationSection">The configuration section to bind to SessionOptions.</param>
	/// <returns>The <see cref="IServiceCollection"/> for method chaining.</returns>
	public static IServiceCollection AddSessionMonitoring(
		this IServiceCollection services,
		IConfigurationSection configurationSection) {

		return services.AddSessionMonitoring(options => {
			configurationSection.Bind(options);
		});
	}

	private static void ValidateOptions(SessionOptions options) {
		if (options.TimeoutMinutes <= 0) {
			throw new ArgumentException("TimeoutMinutes must be greater than 0", nameof(options));
		}

		if (options.Stages != null) {
			ValidateStages(options.Stages);
		}

		if (string.IsNullOrWhiteSpace(options.LogoutUrl)) {
			throw new ArgumentException("LogoutUrl cannot be null or empty", nameof(options));
		}

		if (string.IsNullOrWhiteSpace(options.SessionTimeoutMessage)) {
			throw new ArgumentException("SessionTimeoutMessage cannot be null or empty", nameof(options));
		}
	}

	private static void ValidateStages(List<SessionStage> stages) {
		if (stages.Count == 0) {
			throw new ArgumentException("At least one session stage must be defined");
		}

		// Validate stage sequencing and percentages
		for (var i = 0; i < stages.Count; i++) {
			var stage = stages[i];

			if (string.IsNullOrWhiteSpace(stage.Name)) {
				throw new ArgumentException($"Stage at index {i} must have a name");
			}

			if (stage.StartPercentage < 0 || stage.StartPercentage > 1) {
				throw new ArgumentException($"Stage '{stage.Name}' StartPercentage must be between 0 and 1");
			}

			if (stage.EndPercentage < 0 || stage.EndPercentage > 1) {
				throw new ArgumentException($"Stage '{stage.Name}' EndPercentage must be between 0 and 1");
			}

			if (stage.StartPercentage >= stage.EndPercentage) {
				throw new ArgumentException($"Stage '{stage.Name}' StartPercentage must be less than EndPercentage");
			}

			if (stage.ActivityDebounce < TimeSpan.Zero) {
				throw new ArgumentException($"Stage '{stage.Name}' ActivityDebounce cannot be negative");
			}

			// Check sequencing with previous stage
			if (i > 0 && Math.Abs(stages[i - 1].EndPercentage - stage.StartPercentage) > 0.001) {
				throw new ArgumentException($"Stage '{stage.Name}' must start where previous stage '{stages[i - 1].Name}' ends");
			}
		}

		// First stage should start at 0, last should end at 1
		if (Math.Abs(stages.First().StartPercentage) > 0.001) {
			throw new ArgumentException("First stage must start at 0%");
		}

		if (Math.Abs(stages.Last().EndPercentage - 1.0) > 0.001) {
			throw new ArgumentException("Last stage must end at 100%");
		}
	}

}