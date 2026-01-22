namespace Cirreum.Components.Interop;

/// <summary>
/// Browser culture interop interface for retrieving internationalization and timezone information.
/// </summary>
public interface IBrowserCultureInterop {

	/// <summary>
	/// Gets the browser's internationalization format information.
	/// </summary>
	/// <returns>An object containing the browser's locale, timezone, and formatting preferences.</returns>
	DateTimeFormatOptions GetInternationalFormats();

	/// <summary>
	/// Gets the current local time as a string.
	/// </summary>
	/// <returns>A string representation of the current local time.</returns>
	string GetCurrentLocalTime();

	/// <summary>
	/// Gets the current UTC time as a string.
	/// </summary>
	/// <returns>A string representation of the current UTC time.</returns>
	string GetCurrentUtcTime();

	/// <summary>
	/// Determines if daylight saving time is currently in effect.
	/// </summary>
	/// <returns>'Yes' if DST is in effect, 'No' otherwise.</returns>
	string IsDaylightSavingTime();

	/// <summary>
	/// Checks if the browser supports the Intl.DateTimeFormat timeZone feature.
	/// </summary>
	/// <returns>True if timeZone is supported, false otherwise.</returns>
	bool HasTimeZoneSupport();

	/// <summary>
	/// Checks if the browser supports the Date.getTimezoneOffset method.
	/// </summary>
	/// <returns>True if getTimezoneOffset is supported, false otherwise.</returns>
	bool HasOffsetSupport();
}