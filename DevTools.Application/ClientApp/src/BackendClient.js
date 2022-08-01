export async function post(endpoint, body) {
	return await fetch(endpoint, {
		method: 'POST',
		headers: {
			'Content-Type': 'application/json',
			ApiKey: localStorage.getItem('apiKey'),
		},
		body: JSON.stringify(body),
	})
}

export async function postForm(endpoint, body) {
	return await fetch(endpoint, {
		method: 'POST',
		headers: {
			ApiKey: localStorage.getItem('apiKey'),
		},
		body: body,
	})
}

export async function get(endpoint) {
	return await fetch(endpoint, {
		method: 'GET',
		headers: {
			ApiKey: localStorage.getItem('apiKey'),
		},
	})
}
