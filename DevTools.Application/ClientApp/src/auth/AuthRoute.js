import React, { useEffect, useState } from 'react'
import { Navigate, Outlet } from 'react-router'
import { userIsLoggedIn } from './AuthUtlis'

const AuthRoute = () => {
	const [state, setState] = useState({
		userIsAuthenticated: false,
		loading: true,
	})

	useEffect(() => {
		async function verifyUser() {
			setState({
				...state,
				userIsAuthenticated: await userIsLoggedIn(),
				loading: false,
			})
		}

		verifyUser().then()
	}, [])

	function render() {
		if (state.loading) {
			return <></>
		}

		return state.userIsAuthenticated ? <Outlet /> : <Navigate to="/" />
	}

	return render()
}

export default AuthRoute
