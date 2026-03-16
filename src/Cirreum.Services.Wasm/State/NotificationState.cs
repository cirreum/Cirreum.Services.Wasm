namespace Cirreum.State;

sealed class NotificationState(
	IStateManager stateManager
) : ScopedNotificationState, INotificationState {

	private readonly List<Notification> _notifications = [];
	private List<Notification>? _cachedNotifications;

	// Only show non-dismissed notifications
	public IReadOnlyList<Notification> Notifications {
		get {
			this._cachedNotifications ??= [.. _notifications.Where(n => !n.IsDismissed)];
			return this._cachedNotifications.AsReadOnly();
		}
	}

	// Count unread, non-dismissed notifications
	public int UnreadCount => _notifications.Count(n => !n.IsRead && !n.IsDismissed);

	public void AddNotification(Notification notification) {
		_notifications.Insert(0, notification);
		this.NotifyStateChanged();
	}

	public void MarkAsRead(string notificationId) {
		var notification = _notifications.FirstOrDefault(n => n.Id == notificationId);
		if (notification != null && !notification.IsRead) {
			var index = _notifications.IndexOf(notification);
			_notifications[index] = notification with { IsRead = true };
			this.NotifyStateChanged();
		}
	}

	public void MarkAllAsRead() {
		var changed = false;
		for (var i = 0; i < _notifications.Count; i++) {
			if (!_notifications[i].IsRead && !_notifications[i].IsDismissed) {
				_notifications[i] = _notifications[i] with { IsRead = true };
				changed = true;
			}
		}
		if (changed) {
			this.NotifyStateChanged();
		}
	}

	public void Dismiss(string notificationId) {
		var notification = _notifications.FirstOrDefault(n => n.Id == notificationId);
		if (notification != null) {
			var index = _notifications.IndexOf(notification);
			// Mark as read AND dismissed
			_notifications[index] = notification with { IsRead = true, IsDismissed = true };
			this.NotifyStateChanged();
		}
	}

	public void DismissAll() {
		var changed = false;
		for (var i = 0; i < _notifications.Count; i++) {
			if (!_notifications[i].IsDismissed) {
				_notifications[i] = _notifications[i] with { IsRead = true, IsDismissed = true };
				changed = true;
			}
		}
		if (changed) {
			this.NotifyStateChanged();
		}
	}

	// Keep for hard delete if needed
	public void RemoveNotification(string notificationId) {
		_notifications.RemoveAll(n => n.Id == notificationId);
		this.NotifyStateChanged();
	}

	public void ClearAll() {
		_notifications.Clear();
		this.NotifyStateChanged();
	}

	public void Refresh() {
		this.NotifyStateChanged();
	}

	protected override void OnStateHasChanged() {
		this._cachedNotifications = null; // invalidate on any state change
		stateManager.NotifySubscribers<INotificationState>(this);
	}

}