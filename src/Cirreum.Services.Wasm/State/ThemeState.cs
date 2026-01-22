namespace Cirreum.State;

sealed class ThemeState(
	IStateManager stateManager
) : ScopedNotificationState, IThemeState {

	/// <inheritdoc/>
	public string Mode { get; private set; } = "auto";

	/// <inheritdoc/>
	public string AppliedMode { get; private set; } = "light";

	/// <inheritdoc/>
	public string Theme { get; private set; } = "default";



	/// <inheritdoc/>
	public void SetMode(string value) => this.Mode = value;

	/// <inheritdoc/>
	public void SetAppliedMode(string value) => this.AppliedMode = value;

	/// <inheritdoc/>
	public void SetTheme(string value) => this.Theme = value;

	/// <inheritdoc/>
	protected override void OnStateHasChanged() {
		stateManager.NotifySubscribers<IThemeState>(this);
	}

}