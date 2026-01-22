namespace Cirreum.State;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// Implementation of the state manager with logging support
/// </summary>
sealed class StateManager : IStateManager {

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
		this._logger.LogDebug("StateManager initialized: {StateId}", StateId);
	}

	/// <inheritdoc/>
	public TState Get<TState>() where TState : IApplicationState {
		var serviceType = typeof(TState);
		this._logger.LogTrace("Attempting to retrieve section of type {SectionType}", serviceType.Name);
		var cacheKey = ResolveCacheKey<TState>();
		return this.GetFromCacheKey<TState>(cacheKey);
	}

	/// <inheritdoc/>
	public IDisposable Subscribe<TState>(Action handler) where TState : IApplicationState {
		ArgumentNullException.ThrowIfNull(handler);
		return this.Subscribe<TState>(t => handler());
	}

	/// <inheritdoc/>
	public IDisposable Subscribe<TState>(Action<TState> handler) where TState : IApplicationState {
		ArgumentNullException.ThrowIfNull(handler);

		// Validate that TState is resolvable from DI
		var cacheKey = ResolveCacheKey<TState>();
		_ = this.GetFromCacheKey<TState>(cacheKey);

		this._logger.LogDebug("Attempting to add subscriber for type {SectionType}", cacheKey.Name);

		lock (this._lock) {
			if (this._subscribers.TryGetValue(cacheKey, out var value) is false) {
				value = [];
				this._subscribers[cacheKey] = value;
				this._subscriberVersions.TryAdd(cacheKey, 0);
				this._logger.LogDebug("Created new subscriber list for type {SectionType}", cacheKey.Name);
			}
			value.Add(handler);

			// Increment version to invalidate cache
			this.IncrementVersion(cacheKey);
		}

		this._logger.LogDebug("Successfully added subscriber for type {SectionType}", cacheKey.Name);

		return new SubscriptionToken(() => {
			lock (this._lock) {
				if (this._subscribers.TryGetValue(cacheKey, out var value)) {
					if (value.Remove(handler)) {
						this._logger.LogDebug("Successfully removed subscriber for type {SectionType}", cacheKey.Name);

						// Increment version to invalidate cache
						this.IncrementVersion(cacheKey);

						if (value.Count == 0) {
							this._subscribers.Remove(cacheKey);
							this._subscriberVersions.TryRemove(cacheKey, out _);
							this._subscriberCache.TryRemove(cacheKey, out _);
							this._logger.LogDebug("Removed empty subscriber list for type {SectionType}", cacheKey.Name);
						}
					} else {
						this._logger.LogWarning("Failed to remove subscriber for type {SectionType}: Handler not found", cacheKey.Name);
					}
				} else {
					this._logger.LogWarning("Failed to remove subscriber for type {SectionType}: No subscribers found", cacheKey.Name);
				}
			}
		}, this._logger);

	}

	/// <inheritdoc/>
	public void NotifySubscribers<TState>() where TState : class, IApplicationState {
		var cacheKey = ResolveCacheKey<TState>();
		var diInstance = this.GetFromCacheKey<TState>(cacheKey);
		this.NotifySubscribersInternal(cacheKey, diInstance);
	}

	/// <summary>
	/// Notify all subscribers of the specified state type with the provided state instance.
	/// </summary>	
	/// <inheritdoc/>
	public void NotifySubscribers<TState>(TState state) where TState : class, IApplicationState {
		ArgumentNullException.ThrowIfNull(state);

		var cacheKey = ResolveCacheKey<TState>();

		this.NotifySubscribersInternal(cacheKey, state);

	}

	private static Type ResolveCacheKey<TState>() where TState : IApplicationState {
		var type = typeof(TState);

		// If it's already an interface, use it
		if (type.IsInterface) {
			return type;
		}

		// If it's a class, find the primary interface
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

		// If we reach here, return the type itself (no interfaces found)
		return type;

	}

	private TState GetFromCacheKey<TState>(Type cacheKey) where TState : IApplicationState {
		var serviceType = typeof(TState);
		this._logger.LogTrace("Attempting to retrieve section of type {SectionType}", serviceType.Name);
		var section = (TState)_stateCache.GetOrAdd(cacheKey, key => {

			// Try the requested type first
			var instance = _serviceProvider.GetService<TState>();
			if (instance != null) {
				return instance;
			}

			// If that fails, try the resolved cache key type
			if (key != serviceType) {
				var interfaceInstance = _serviceProvider.GetService(key);
				if (interfaceInstance is TState typedInstance) {
					return typedInstance;
				}
			}

			// If still not found, throw an exception
			throw new InvalidOperationException($"Section of type {serviceType.Name} is not registered.");

		});

		this._logger.LogDebug("Successfully retrieved section of type {SectionType}", serviceType.Name);
		return section;
	}

	private void NotifySubscribersInternal<TState>(Type cacheKey, TState stateSection) where TState : class, IApplicationState {
		ArgumentNullException.ThrowIfNull(stateSection);

		this._logger.LogDebug("Beginning notification process for type {SectionType}", cacheKey.Name);

		var subscribersCopy = this.GetCachedSubscribers(cacheKey);
		if (subscribersCopy.Count == 0) {
			this._logger.LogDebug("No subscribers found for type {SectionType}", cacheKey.Name);
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
					this._logger.LogWarning(
						"Subscriber type mismatch for {StateType}. Expected: Action<{ExpectedType}>, Found: {ActualType}",
						cacheKey.Name,
						typeof(TState).Name,
						subscriber.GetType().FullName);
					mismatchCount++;
				}
			} catch (Exception ex) {
				errorCount++;
				this._logger.LogError(ex, "Error notifying subscriber {SubscriberNumber} for type {SectionType}",
					notificationCount + errorCount, cacheKey.Name);
			}
		}

		this._logger.LogDebug(
			"Completed notifications for type {SectionType}: {SuccessCount} successful, {MismatchCount} mismatches, {ErrorCount} failed",
			cacheKey.Name, notificationCount, mismatchCount, errorCount);
	}

	private void IncrementVersion(Type type) {
		this._subscriberVersions.AddOrUpdate(
			type,
			1, // Initialize version to 1 if not present
			(_, currentVersion) => currentVersion + 1
		);

		// Remove the cached version when the list changes
		this._subscriberCache.TryRemove(type, out _);

	}

	private List<Delegate> GetCachedSubscribers(Type type) {
		// Try to get the current version
		if (!this._subscriberVersions.TryGetValue(type, out var currentVersion)) {
			return [];
		}

		// Check if we have a valid cached version
		if (this._subscriberCache.TryGetValue(type, out var cached) && cached.Version == currentVersion) {
			this._logger.LogDebug("Using cached subscriber list for type {SectionType}", type.Name);
			return cached.Subscribers;
		}

		// If cache is invalid or missing, create new copy under lock
		lock (this._lock) {
			// Double-check version hasn't changed while we were waiting for lock
			if (!this._subscriberVersions.TryGetValue(type, out currentVersion)) {
				return [];
			}

			// Get fresh copy of subscribers
			if (!this._subscribers.TryGetValue(type, out var subscribers)) {
				return [];
			}

			// Cache the copy with current version
			List<Delegate> subscribersCopy = [.. subscribers];
			this._subscriberCache.AddOrUpdate(
				type,
				(subscribersCopy, currentVersion),
				(_, _) => (subscribersCopy, currentVersion)
			);

			this._logger.LogDebug("Created new cached subscriber list for type {SectionType}", type.Name);
			return subscribersCopy;
		}
	}

	/// <summary>
	/// Token returned when subscribing that can be used to unsubscribe
	/// </summary>
	private class SubscriptionToken(Action unsubscribeAction, ILogger logger) : IDisposable {

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
					// Error is logged but not rethrown, following .NET disposal pattern best practices
					this._disposed = true;
				}
			}
		}

	}

}