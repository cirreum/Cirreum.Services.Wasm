
export function clear(): void {
	sessionStorage.clear();
}

export function getItem(key: string): string | null {
	return sessionStorage.getItem(key);
}

export function key(index: number): string | null {
	return sessionStorage.key(index);
}

export function containsKey(key: string): boolean {
	return sessionStorage.hasOwnProperty(key);
}

export function length(): number {
	return sessionStorage.length;
}

export function removeItem(key: string): void {
	sessionStorage.removeItem(key);
}

export function removeItems(keys: Array<string>): void {
	if (keys) {
		keys.forEach((value) => {
			sessionStorage.removeItem(value);
		});
	}
}

export function setItem(key: string, data: string): void {
	sessionStorage.setItem(key, data);
}

export function keys(): string[] {
	return Object.keys(sessionStorage);
}