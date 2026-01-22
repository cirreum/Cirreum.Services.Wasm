namespace Cirreum.Authorization;

/// <summary>
/// Configuration for a session stage, defining behavior during specific periods of a session.
/// </summary>
public class SessionStage {

	/// <summary>
	/// Gets or sets the name of this stage for logging and identification purposes.
	/// </summary>
	public string Name { get; set; } = "";

	/// <summary>
	/// Gets or sets the start percentage of the session when this stage begins (0.0 to 1.0).
	/// </summary>
	public double StartPercentage { get; set; }

	/// <summary>
	/// Gets or sets the end percentage of the session when this stage ends (0.0 to 1.0).
	/// </summary>
	public double EndPercentage { get; set; }

	/// <summary>
	/// Gets or sets the debounce delay for activity monitoring during this stage.
	/// </summary>
	public TimeSpan ActivityDebounce { get; set; } = TimeSpan.FromSeconds(2);

	/// <summary>
	/// Gets or sets whether user activity should be monitored during this stage.
	/// </summary>
	public bool MonitorActivity { get; set; } = true;

	/// <summary>
	/// Gets or sets additional metadata for this stage that can be used by external components.
	/// </summary>
	/// <remarks>
	/// This can be used to pass stage-specific configuration to DOM activity monitors,
	/// UI components, or other parts of the application.
	/// </remarks>
	public Dictionary<string, object> Metadata { get; set; } = [];

	/// <summary>
	/// Well-known metadata keys for common stage configurations.
	/// </summary>
	public static class MetadataKeys {
		/// <summary>
		/// DOM event throttling multiplier. Value should be an integer representing 
		/// the multiplier to apply to base throttling intervals.
		/// </summary>
		public const string DomThrottleMultiplier = "DomThrottleMultiplier";

		/// <summary>
		/// UI warning threshold percentage. Value should be a double between 0.0 and 1.0.
		/// </summary>
		public const string WarningThreshold = "WarningThreshold";

		/// <summary>
		/// Background task frequency adjustment. Value should be a TimeSpan.
		/// </summary>
		public const string BackgroundTaskInterval = "BackgroundTaskInterval";
	}

}