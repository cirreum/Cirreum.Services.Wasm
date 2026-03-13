namespace Cirreum.State;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

/// <summary>
/// Implementation of the state manager with logging support.
/// Manages subscriber registration, notification broadcasting, and state caching.
/// </summary>
sealed partial class StateManager : IStateManager {

	private readonly Dictionary<Type, List<Delegate>> _subscribers = [];
	private readonly Lock _lock = new();
	private readonly ILogger<StateManager> _logger;
	private readonly IServiceProvider _serviceProvider;

	// Cache for subscriber lists with version tracking
	private readonly ConcurrentDictionary<Type, (List<Delegate> Subscribers, int Version)> _subscriberCache = new();
	private readonly ConcurrentDictionary<Type, int> _subscriberVersions = new();
	private readonly ConcurrentDictionary<Type, object> _stateCache = new();

	private readonly Guid StateId = Guid.NewGuid();

	/// <summary>
	/// Construct a new instance.
	/// </summary>
	/// <param name="logger">The <see cref="ILogger{TCategoryName}"/> to use.</param>
	/// <param name="serviceProvider">The active <see cref="IServiceProvider"/> instance.</param>
	/// <exception cref="ArgumentNullException"></exception>
	public StateManager(
		ILogger<StateManager> logger,
		IServiceProvider serviceProvider) {
		this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
		this._serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		Log.Initialized(this._logger, StateId);
	}

	/// <inheritdoc/>
	public TState Get<TState>() where TState : IApplicationState {
		var cacheKey = ResolveCacheKey<TState>();
		Log.RetrievingState(this._logger, cacheKey.Name);
		return this.GetFromCacheKey<TState>(cacheKey);
	}

	// -------------------------------------------------------------------------
	// Subscribe
	// -------------------------------------------------------------------------

	/// <inheritdoc/>
	public IDisposable Subscribe<TState>(Action handler) where TState : IApplicationState {
		ArgumentNullException.ThrowIfNull(handler);
		return this.Subscribe<TState>(_ => handler());
	}

	/// <inheritdoc/>
	public IDisposable Subscribe<TState>(Action<TState> handler) where TState : IApplicationState {
		ArgumentNullException.ThrowIfNull(handler);

		var cacheKey = ResolveCacheKey<TState>();
		_ = this.GetFromCacheKey<TState>(cacheKey);

		Log.AddingSubscriber(this._logger, cacheKey.Name);

		lock (this._lock) {
			if (!this._subscribers.TryGetValue(cacheKey, out var value)) {
				value = [];
				this._subscribers[cacheKey] = value;
				this._subscriberVersions.TryAdd(cacheKey, 0);
				Log.CreatedSubscriberList(this._logger, cacheKey.Name);
			}
			value.Add(handler);
			this.IncrementVersion(cacheKey);
		}

		Log.AddedSubscriber(this._logger, cacheKey.Name);

		return new SubscriptionToken(() => this.RemoveSubscriber(cacheKey, handler), this._logger);
	}

	// -------------------------------------------------------------------------
	// Notify
	// -------------------------------------------------------------------------

	/// <inheritdoc/>
	public void NotifySubscribers<TState>() where TState : class, IApplicationState {
		var cacheKey = ResolveCacheKey<TState>();
		var diInstance = this.GetFromCacheKey<TState>(cacheKey);
		this.NotifySubscribersInternal(cacheKey, diInstance);
	}

	/// <inheritdoc/>
	public void NotifySubscribers<TState>(TState state) where TState : class, IApplicationState {
		ArgumentNullException.ThrowIfNull(state);
		var cacheKey = ResolveCacheKey<TState>();
		this.NotifySubscribersInternal(cacheKey, state);
	}

	// -------------------------------------------------------------------------
	// Internal Helpers
	// -------------------------------------------------------------------------

	private static Type ResolveCacheKey<TState>() where TState : IApplicationState {
		var type = typeof(TState);

		if (type.IsInterface) {
			return type;
		}

		if (type.IsClass) {
			var stateInterfaces = type.GetInterfaces()
				.Where(i => i.IsAssignableTo(typeof(IApplicationState)))
				.ToList();

			if (stateInterfaces.Count > 0) {
				return stateInterfaces
					.OrderByDescending(i => stateInterfaces
						.Count(other => i != other && i.IsAssignableTo(other)))
					.First();
			}
		}

		return type;
	}

	private TState GetFromCacheKey<TState>(Type cacheKey) where TState : IApplicationState {
		Log.RetrievingStateFromCache(this._logger, typeof(TState).Name);

		var section = (TState)_stateCache.GetOrAdd(cacheKey, key => {
			var instance = _serviceProvider.GetService<TState>();
			if (instance != null) {
				return instance;
			}
			if (key != typeof(TState)) {
				var interfaceInstance = _serviceProvider.GetService(key);
				if (interfaceInstance is TState typedInstance) {
					return typedInstance;
				}
			}
			throw new InvalidOperationException($"Section of type {typeof(TState).Name} is not registered.");
		});

		Log.RetrievedState(this._logger, typeof(TState).Name);
		return section;
	}

	private void NotifySubscribersInternal<TState>(Type cacheKey, TState stateSection)
		where TState : class, IApplicationState {
		ArgumentNullException.ThrowIfNull(stateSection);

		Log.BeginNotify(this._logger, cacheKey.Name);

		var subscribersCopy = this.GetCachedSubscribers(cacheKey);
		if (subscribersCopy.Count == 0) {
			Log.NoSubscribers(this._logger, cacheKey.Name);
			return;
		}

		var notificationCount = 0;
		var mismatchCount = 0;
		var errorCount = 0;

		foreach (var subscriber in subscribersCopy) {
			try {
				if (subscriber is Action<TState> action) {
					action.Invoke(stateSection);
					notificationCount++;
				} else {
					Log.SubscriberMismatch(this._logger, cacheKey.Name, typeof(TState).Name, subscriber.GetType().FullName ?? "unknown");
					mismatchCount++;
				}
			} catch (Exception ex) {
				errorCount++;
				Log.SubscriberError(this._logger, ex, notificationCount + errorCount, cacheKey.Name);
			}
		}

		Log.NotifyComplete(this._logger, cacheKey.Name, notificationCount, mismatchCount, errorCount);
	}

	private void RemoveSubscriber(Type cacheKey, Delegate handler) {
		lock (this._lock) {
			if (this._subscribers.TryGetValue(cacheKey, out var value)) {
				if (value.Remove(handler)) {
					Log.RemovedSubscriber(this._logger, cacheKey.Name);
					this.IncrementVersion(cacheKey);
					if (value.Count == 0) {
						this._subscribers.Remove(cacheKey);
						this._subscriberVersions.TryRemove(cacheKey, out _);
						this._subscriberCache.TryRemove(cacheKey, out _);
						Log.RemovedEmptySubscriberList(this._logger, cacheKey.Name);
					}
				} else {
					Log.RemoveSubscriberNotFound(this._logger, cacheKey.Name);
				}
			} else {
				Log.RemoveSubscriberListNotFound(this._logger, cacheKey.Name);
			}
		}
	}

	private void IncrementVersion(Type type) {
		this._subscriberVersions.AddOrUpdate(type, 1, (_, current) => current + 1);
		this._subscriberCache.TryRemove(type, out _);
	}

	private List<Delegate> GetCachedSubscribers(Type type) {
		if (!this._subscriberVersions.TryGetValue(type, out var currentVersion)) {
			return [];
		}

		if (this._subscriberCache.TryGetValue(type, out var cached) && cached.Version == currentVersion) {
			Log.UsingCachedSubscribers(this._logger, type.Name);
			return cached.Subscribers;
		}

		lock (this._lock) {
			if (!this._subscriberVersions.TryGetValue(type, out currentVersion)) {
				return [];
			}
			if (!this._subscribers.TryGetValue(type, out var list)) {
				return [];
			}

			List<Delegate> copy = [.. list];
			this._subscriberCache.AddOrUpdate(type, (copy, currentVersion), (_, _) => (copy, currentVersion));
			Log.CreatedCachedSubscribers(this._logger, type.Name);
			return copy;
		}
	}

	// -------------------------------------------------------------------------
	// Source-Generated Logging
	// -------------------------------------------------------------------------

	private static partial class Log {
		[LoggerMessage(Level = LogLevel.Debug, Message = "StateManager initialized: {StateId}")]
		public static partial void Initialized(ILogger logger, Guid stateId);

		[LoggerMessage(Level = LogLevel.Trace, Message = "Retrieving state of type {StateType}")]
		public static partial void RetrievingState(ILogger logger, string stateType);

		[LoggerMessage(Level = LogLevel.Trace, Message = "Retrieving state from cache for type {StateType}")]
		public static partial void RetrievingStateFromCache(ILogger logger, string stateType);

		[LoggerMessage(Level = LogLevel.Debug, Message = "Successfully retrieved state of type {StateType}")]
		public static partial void RetrievedState(ILogger logger, string stateType);

		[LoggerMessage(Level = LogLevel.Debug, Message = "Adding subscriber for type {StateType}")]
		public static partial void AddingSubscriber(ILogger logger, string stateType);

		[LoggerMessage(Level = LogLevel.Debug, Message = "Created new subscriber list for type {StateType}")]
		public static partial void CreatedSubscriberList(ILogger logger, string stateType);

		[LoggerMessage(Level = LogLevel.Debug, Message = "Successfully added subscriber for type {StateType}")]
		public static partial void AddedSubscriber(ILogger logger, string stateType);

		[LoggerMessage(Level = LogLevel.Debug, Message = "Successfully removed subscriber for type {StateType}")]
		public static partial void RemovedSubscriber(ILogger logger, string stateType);

		[LoggerMessage(Level = LogLevel.Debug, Message = "Removed empty subscriber list for type {StateType}")]
		public static partial void RemovedEmptySubscriberList(ILogger logger, string stateType);

		[LoggerMessage(Level = LogLevel.Warning, Message = "Failed to remove subscriber for type {StateType}: Handler not found")]
		public static partial void RemoveSubscriberNotFound(ILogger logger, string stateType);

		[LoggerMessage(Level = LogLevel.Warning, Message = "Failed to remove subscriber for type {StateType}: No subscribers found")]
		public static partial void RemoveSubscriberListNotFound(ILogger logger, string stateType);

		[LoggerMessage(Level = LogLevel.Debug, Message = "Beginning notification for type {StateType}")]
		public static partial void BeginNotify(ILogger logger, string stateType);

		[LoggerMessage(Level = LogLevel.Debug, Message = "No subscribers found for type {StateType}")]
		public static partial void NoSubscribers(ILogger logger, string stateType);

		[LoggerMessage(Level = LogLevel.Warning, Message = "Subscriber type mismatch for {StateType}. Expected: Action<{ExpectedType}>, Found: {ActualType}")]
		public static partial void SubscriberMismatch(ILogger logger, string stateType, string expectedType, string actualType);

		[LoggerMessage(Level = LogLevel.Error, Message = "Error notifying subscriber #{SubscriberNumber} for type {StateType}")]
		public static partial void SubscriberError(ILogger logger, Exception ex, int subscriberNumber, string stateType);

		[LoggerMessage(Level = LogLevel.Debug, Message = "Completed notifications for {StateType}: {SuccessCount} successful, {MismatchCount} mismatches, {ErrorCount} failed")]
		public static partial void NotifyComplete(ILogger logger, string stateType, int successCount, int mismatchCount, int errorCount);

		[LoggerMessage(Level = LogLevel.Debug, Message = "Using cached subscriber list for type {StateType}")]
		public static partial void UsingCachedSubscribers(ILogger logger, string stateType);

		[LoggerMessage(Level = LogLevel.Debug, Message = "Created new cached subscriber list for type {StateType}")]
		public static partial void CreatedCachedSubscribers(ILogger logger, string stateType);
	}

	// -------------------------------------------------------------------------
	// Subscription Token
	// -------------------------------------------------------------------------

	/// <summary>
	/// Token returned when subscribing that can be used to unsubscribe.
	/// </summary>
	private sealed class SubscriptionToken(Action unsubscribeAction, ILogger logger) : IDisposable {

		private readonly Action _unsubscribeAction = unsubscribeAction ?? throw new ArgumentNullException(nameof(unsubscribeAction));
		private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
		private bool _disposed;

		public void Dispose() {
			if (!this._disposed) {
				try {
					this._unsubscribeAction();
					this._disposed = true;
				} catch (Exception ex) {
					this._logger.LogError(ex, "Error during subscription disposal");
					this._disposed = true;
				}
			}
		}
	}

}
