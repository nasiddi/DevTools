import { post } from '../BackendClient'

export async function userIsLoggedIn() {
	const userLastVerifiedString = localStorage.getItem('userLastVerified')
	const apiKey = localStorage.getItem('apiKey')

	if (userLastVerifiedString !== undefined && apiKey?.length > 0) {
		const userLastVerified = new Date(userLastVerifiedString)
		if (new Date() - userLastVerified < 3600000) {
			return true
		}
	}

	const username = localStorage.getItem('username')
	const passwordHash = localStorage.getItem('passwordHash')

	const response = await post('user/verify', {
		username,
		passwordHash,
	})

	if (response.status === 200) {
		const key = await response.text()
		localStorage.setItem('apiKey', key)
		localStorage.setItem('userLastVerified', new Date().toString())
	} else {
		localStorage.removeItem('username')
		localStorage.removeItem('passwordHash')
		localStorage.removeItem('apiKey')
		localStorage.removeItem('userLastVerified')
	}

	return response.status === 200
}
