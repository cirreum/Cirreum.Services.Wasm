export function downloadFile(content: BlobPart, filename: string, contentType: string): void {

	const file = new File([content], filename, { type: contentType });
	const url = URL.createObjectURL(file);
	const a = document.createElement('a');

	a.href = url;
	a.download = filename ?? '';
	a.target = "_self";
	a.click();
	a.remove();
	URL.revokeObjectURL(url);
}