namespace Cirreum.Clock;

using Cirreum.Components.Interop;

sealed class DateTimeService(
	TimeProvider timeProvider,
	IJSAppInterop appInterop
) : IDateTimeClock {

	public TimeProvider TimeProvider => timeProvider;

	private string? _cachedIanaTimeZoneId = null;
	public string LocalTimeZoneId {
		get {

			if (_cachedIanaTimeZoneId != null) {
				return _cachedIanaTimeZoneId;
			}

			try {
				// First attempt: Try getting directly from browser via JS interop
				// This is the most reliable method in browser environments
				var formats = appInterop.GetInternationalFormats();
				if (!string.IsNullOrEmpty(formats.TimeZone)) {
					_cachedIanaTimeZoneId = formats.TimeZone;
					return _cachedIanaTimeZoneId;
				}
			} catch {
				// JS interop might fail in certain scenarios, continue to fallback
			}

			// Second attempt: Use TimeZoneInfo.Local.Id which is already IANA format in browsers
			var localId = TimeZoneInfo.Local.Id;

			// Verify it looks like an IANA ID
			if (localId.Contains('/')) {
				_cachedIanaTimeZoneId = localId;
				return _cachedIanaTimeZoneId;
			}

			// Ultimate fallback - should rarely hit this in browser environments
			_cachedIanaTimeZoneId = "Etc/UTC";
			return _cachedIanaTimeZoneId;
		}
	}

	public TimeZoneInfo GetTimeZoneByIanaId(string ianaTimeZoneId) {
		// In browser environments, IANA IDs can be directly used with TimeZoneInfo.FindSystemTimeZoneById
		try {
			return TimeZoneInfo.FindSystemTimeZoneById(ianaTimeZoneId);
		} catch {
			// Fallback to UTC if the IANA ID isn't found
			return TimeZoneInfo.Utc;
		}
	}

}