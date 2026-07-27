namespace Cirreum;

using Cirreum.State;


/// <summary>
/// Default implementation of the <see cref="IBrowserDocumentState"/> state section — the browser
/// document hosting the application: its title, application name, and progressive-web-app display
/// mode.
/// </summary>
sealed class BrowserDocumentState(
	IStateManager stateManager
) : ScopedNotificationState, IBrowserDocumentState {

	/// <inheritdoc/>
	public string AppName { get; private set; } = string.Empty;

	/// <inheritdoc/>
	public void SetAppName(string value) {
		using var _ = this.CreateNotificationScope();
		this.AppName = value;
	}

	/// <inheritdoc/>
	public string DocumentTitlePrefix { get; private set; } = string.Empty;

	/// <inheritdoc/>
	public void SetDocumentTitlePrefix(string value) {
		using var _ = this.CreateNotificationScope();
		this.DocumentTitlePrefix = value;
	}

	/// <inheritdoc/>
	public string DocumentTitleSuffix { get; private set; } = string.Empty;

	/// <inheritdoc/>
	public void SetDocumentTitleSuffix(string value) {
		using var _ = this.CreateNotificationScope();
		this.DocumentTitleSuffix = value;
	}

	/// <inheritdoc/>
	public string DocumentTitleSeparator { get; private set; } = "|";

	/// <inheritdoc/>
	public void SetDocumentTitleSeparator(string value) {
		using var _ = this.CreateNotificationScope();
		this.DocumentTitleSeparator = value;
	}

	/// <inheritdoc/>
	public bool IsStandAlone { get; private set; }

	/// <inheritdoc/>
	public void SetIsStandAlone(bool value) {
		using var _ = this.CreateNotificationScope();
		this.IsStandAlone = value;
	}

	protected override void OnStateHasChanged() {
		stateManager.NotifySubscribers<IBrowserDocumentState>(this);
	}

}