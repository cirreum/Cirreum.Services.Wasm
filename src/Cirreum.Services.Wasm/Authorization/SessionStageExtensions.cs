namespace Cirreum.Authorization;

/// <summary>
/// Extension methods for working with SessionStage metadata in a type-safe manner.
/// </summary>
public static class SessionStageExtensions {

	/// <summary>
	/// Gets the DOM throttle multiplier for this stage.
	/// </summary>
	/// <param name="stage">The session stage.</param>
	/// <param name="defaultValue">Default value if not specified.</param>
	/// <returns>The throttle multiplier.</returns>
	public static int GetDomThrottleMultiplier(this SessionStage stage, int defaultValue = 1) {
		return stage.Metadata.TryGetValue(SessionStage.MetadataKeys.DomThrottleMultiplier, out var value) && value is int multiplier
			? multiplier
			: defaultValue;
	}

	/// <summary>
	/// Sets the DOM throttle multiplier for this stage.
	/// </summary>
	/// <param name="stage">The session stage.</param>
	/// <param name="multiplier">The throttle multiplier.</param>
	/// <returns>The same stage instance for fluent chaining.</returns>
	public static SessionStage WithDomThrottleMultiplier(this SessionStage stage, int multiplier) {
		stage.Metadata[SessionStage.MetadataKeys.DomThrottleMultiplier] = multiplier;
		return stage;
	}

	/// <summary>
	/// Gets the warning threshold percentage for this stage.
	/// </summary>
	/// <param name="stage">The session stage.</param>
	/// <param name="defaultValue">Default value if not specified.</param>
	/// <returns>The warning threshold as a percentage (0.0 to 1.0).</returns>
	public static double GetWarningThreshold(this SessionStage stage, double defaultValue = 0.0) {
		return stage.Metadata.TryGetValue(SessionStage.MetadataKeys.WarningThreshold, out var value) && value is double threshold
			? threshold
			: defaultValue;
	}

	/// <summary>
	/// Sets the warning threshold percentage for this stage.
	/// </summary>
	/// <param name="stage">The session stage.</param>
	/// <param name="threshold">The warning threshold (0.0 to 1.0).</param>
	/// <returns>The same stage instance for fluent chaining.</returns>
	public static SessionStage WithWarningThreshold(this SessionStage stage, double threshold) {
		stage.Metadata[SessionStage.MetadataKeys.WarningThreshold] = threshold;
		return stage;
	}

	/// <summary>
	/// Gets the background task interval for this stage.
	/// </summary>
	/// <param name="stage">The session stage.</param>
	/// <param name="defaultValue">Default value if not specified.</param>
	/// <returns>The background task interval.</returns>
	public static TimeSpan GetBackgroundTaskInterval(this SessionStage stage, TimeSpan? defaultValue = null) {
		return stage.Metadata.TryGetValue(SessionStage.MetadataKeys.BackgroundTaskInterval, out var value) && value is TimeSpan interval
			? interval
			: defaultValue ?? TimeSpan.FromMinutes(1);
	}

	/// <summary>
	/// Sets the background task interval for this stage.
	/// </summary>
	/// <param name="stage">The session stage.</param>
	/// <param name="interval">The background task interval.</param>
	/// <returns>The same stage instance for fluent chaining.</returns>
	public static SessionStage WithBackgroundTaskInterval(this SessionStage stage, TimeSpan interval) {
		stage.Metadata[SessionStage.MetadataKeys.BackgroundTaskInterval] = interval;
		return stage;
	}

}