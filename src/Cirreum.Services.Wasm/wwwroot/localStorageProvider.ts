
export function clear(): void {
	localStorage.clear();
}

export function getItem(key: string): string | null {
	return localStorage.getItem(key);
}

export function key(index: number): string | null {
	return localStorage.key(index);
}

export function containsKey(key: string): boolean {
	return localStorage.hasOwnProperty(key);
}

export function length(): number {
	return localStorage.length;
}

export function removeItem(key: string): void {
	localStorage.removeItem(key);
}

export function removeItems(keys: Array<string>): void {
	if (keys) {
		keys.forEach((value) => {
			localStorage.removeItem(value);
		});
	}
}

export function setItem(key: string, data: string): void {
	localStorage.setItem(key, data);
}

export function keys(): string[] {
	return Object.keys(localStorage);
}