interface ActivityMonitorConfig {
	completionThrottleMs?: number;
	engagementThrottleMs?: number;
}

interface EventHandlerInfo {
	handler: EventListener;
	throttleMs: number;
}

class SessionActivityMonitor {
	private dotNetRef: any;
	private eventHandlers: Map<string, EventHandlerInfo> = new Map();
	private lastActivityUpdate: number = 0;

	// Current throttle settings
	private completionThrottleMs: number;
	private engagementThrottleMs: number;

	// Event categorization
	private readonly completionEvents = ['keyup', 'pointerup'];
	private readonly engagementEvents = ['scroll', 'pointermove'];

	constructor(dotNetRef: any, config: ActivityMonitorConfig = {}) {
		this.dotNetRef = dotNetRef;
		this.completionThrottleMs = config.completionThrottleMs ?? 1000;
		this.engagementThrottleMs = config.engagementThrottleMs ?? 2000;
	}

	public start(): void {
		this.bindActivityEvents();
	}

	public stop(): void {
		this.unbindActivityEvents();
	}

	/**
	 * Updates the throttling intervals and rebinds event handlers if monitor is active.
	 * @param config New throttling configuration
	 */
	public updateThrottling(config: ActivityMonitorConfig): void {
		const wasActive = this.eventHandlers.size > 0;

		// Update settings
		if (config.completionThrottleMs !== undefined) {
			this.completionThrottleMs = config.completionThrottleMs;
		}
		if (config.engagementThrottleMs !== undefined) {
			this.engagementThrottleMs = config.engagementThrottleMs;
		}

		// If monitor was active, rebind with new throttling
		if (wasActive) {
			this.unbindActivityEvents();
			this.bindActivityEvents();
		}
	}

	/**
	 * Updates throttling based on a multiplier (useful for stage changes).
	 * @param multiplier Multiplier to apply to base throttling values
	 */
	public updateThrottlingMultiplier(multiplier: number): void {
		this.updateThrottling({
			completionThrottleMs: this.getBaseCompletionThrottle() * multiplier,
			engagementThrottleMs: this.getBaseEngagementThrottle() * multiplier
		});
	}

	/**
	 * Gets current throttling configuration.
	 */
	public getCurrentThrottling(): ActivityMonitorConfig {
		return {
			completionThrottleMs: this.completionThrottleMs,
			engagementThrottleMs: this.engagementThrottleMs
		};
	}

	private bindActivityEvents(): void {
		// Bind completion events
		this.completionEvents.forEach(event => {
			const handler = this.createActivityHandler(this.completionThrottleMs);
			const handlerInfo: EventHandlerInfo = {
				handler,
				throttleMs: this.completionThrottleMs
			};
			this.eventHandlers.set(event, handlerInfo);
			document.addEventListener(event, handler, { passive: true });
		});

		// Bind engagement events
		this.engagementEvents.forEach(event => {
			const handler = this.createActivityHandler(this.engagementThrottleMs);
			const handlerInfo: EventHandlerInfo = {
				handler,
				throttleMs: this.engagementThrottleMs
			};
			this.eventHandlers.set(event, handlerInfo);
			document.addEventListener(event, handler, { passive: true });
		});
	}

	private unbindActivityEvents(): void {
		this.eventHandlers.forEach((handlerInfo, event) => {
			document.removeEventListener(event, handlerInfo.handler);
		});
		this.eventHandlers.clear();
	}

	private createActivityHandler(throttleMs: number): EventListener {
		return () => {
			const now = Date.now();
			if (now - this.lastActivityUpdate > throttleMs) {
				this.lastActivityUpdate = now;
				this.dotNetRef.invokeMethodAsync('RecordActivity');
			}
		};
	}

	// Base throttle values for multiplier calculations
	private getBaseCompletionThrottle(): number {
		return 1000; // 1 second base
	}

	private getBaseEngagementThrottle(): number {
		return 2000; // 2 second base
	}
}

// Global instance
let activityMonitor: SessionActivityMonitor | null = null;

// Public API
export const ActivityMonitor = {
	/**
	 * Initializes the activity monitor with optional configuration.
	 * @param dotNetRef .NET object reference for callbacks
	 * @param config Optional initial throttling configuration
	 */
	init(dotNetRef: any, config?: ActivityMonitorConfig): void {
		if (activityMonitor) {
			activityMonitor.stop();
		}
		activityMonitor = new SessionActivityMonitor(dotNetRef, config);
	},

	/**
	 * Starts monitoring user activity.
	 */
	start(): void {
		if (activityMonitor) {
			activityMonitor.start();
		}
	},

	/**
	 * Stops monitoring user activity.
	 */
	stop(): void {
		if (activityMonitor) {
			activityMonitor.stop();
		}
	},

	/**
	 * Updates throttling configuration.
	 * @param config New throttling settings
	 */
	updateThrottling(config: ActivityMonitorConfig): void {
		if (activityMonitor) {
			activityMonitor.updateThrottling(config);
		}
	},

	/**
	 * Updates throttling using a multiplier.
	 * @param multiplier Multiplier to apply to base throttling values
	 */
	updateThrottlingMultiplier(multiplier: number): void {
		if (activityMonitor) {
			activityMonitor.updateThrottlingMultiplier(multiplier);
		}
	},

	/**
	 * Gets current throttling configuration.
	 */
	getCurrentThrottling(): ActivityMonitorConfig | null {
		return activityMonitor ? activityMonitor.getCurrentThrottling() : null;
	},

	/**
	 * Destroys the activity monitor instance.
	 */
	destroy(): void {
		if (activityMonitor) {
			activityMonitor.stop();
			activityMonitor = null;
		}
	}
};