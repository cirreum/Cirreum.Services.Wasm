namespace Cirreum.Authorization;

public class SessionHttpHandler : DelegatingHandler {

	public static event Action? HttpActivityDetected;

	protected override async Task<HttpResponseMessage> SendAsync(
		HttpRequestMessage request,
		CancellationToken cancellationToken) {

		// Just emit the event
		HttpActivityDetected?.Invoke();

		return await base.SendAsync(request, cancellationToken);

	}

}