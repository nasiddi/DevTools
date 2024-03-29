export function humanFileSize(bytes, dp = 1) {
	const thresh = 1024

	if (Math.abs(bytes) < thresh) {
		return `${bytes} B`
	}

	const units = ['kB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB']
	let u = -1
	const r = 10 ** dp

	do {
		bytes /= thresh
		// eslint-disable-next-line no-plusplus
		++u
	} while (Math.round(Math.abs(bytes) * r) / r >= thresh && u < units.length - 1)

	return `${bytes.toFixed(dp)} ${units[u]}`
}
