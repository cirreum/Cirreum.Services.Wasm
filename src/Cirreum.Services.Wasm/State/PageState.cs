namespace Cirreum.State;

/// <summary>
/// Default implementation of the <see cref="IPageState"/> state section.
/// </summary>
sealed class PageState(
	IStateManager stateManager
) : ScopedNotificationState, IPageState {

	/// <inheritdoc/>
	public string AppName { get; private set; } = string.Empty;

	/// <inheritdoc/>
	public void SetAppName(string value) {
		using var _ = this.CreateNotificationScope();
		this.AppName = value;
	}

	/// <inheritdoc/>
	public string PageTitlePrefix { get; private set; } = string.Empty;

	/// <inheritdoc/>
	public void SetPageTitlePrefix(string value) {
		using var _ = this.CreateNotificationScope();
		this.PageTitlePrefix = value;
	}

	/// <inheritdoc/>
	public string PageTitleSuffix { get; private set; } = string.Empty;

	/// <inheritdoc/>
	public void SetPageTitleSuffix(string value) {
		using var _ = this.CreateNotificationScope();
		this.PageTitleSuffix = value;
	}

	/// <inheritdoc/>
	public string PageTitleSeparator { get; private set; } = "|";

	/// <inheritdoc/>
	public void SetPageTitleSeparator(string value) {
		using var _ = this.CreateNotificationScope();
		this.PageTitleSeparator = value;
	}

	/// <inheritdoc/>
	public bool IsStandAlone { get; private set; }

	/// <inheritdoc/>
	public void SetIsStandAlone(bool value) {
		using var _ = this.CreateNotificationScope();
		this.IsStandAlone = value;
	}

	protected override void OnStateHasChanged() {
		stateManager.NotifySubscribers<IPageState>(this);
	}

}