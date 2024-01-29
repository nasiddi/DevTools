import React, { useState } from 'react'
import { Button, Grid, TextField } from '@mui/material'
import CryptoJS from 'crypto-js/'
import { userIsLoggedIn } from '../auth/AuthUtlis'

const Login = () => {
	const initialState = {
		username: '',
		hashedPassword: '',
	}
	const [state, setState] = useState(initialState)

	async function onLogin() {
		console.log(state.username)
		console.log(state.hashedPassword)
		await userIsLoggedIn()

		localStorage.setItem('username', state.username)
		localStorage.setItem('passwordHash', state.hashedPassword)
		window.location.reload(false)
	}

	function onUsernameChange(event) {
		setState({ ...state, username: event.target.value })
	}

	function onPasswordChange(event) {
		const password = CryptoJS.enc.Base64.stringify(CryptoJS.enc.Utf8.parse(event.target.value))
		setState({ ...state, hashedPassword: password })
	}

	function renderLogin() {
		return (
			<Grid container spacing={3}>
				<Grid item xs={12} sm={4}>
					<TextField
						variant={'outlined'}
						label={'UserName'}
						value={state.username}
						name={'username'}
						onChange={onUsernameChange}
						fullWidth={true}
					/>
				</Grid>
				<Grid item xs={12} sm={4}>
					<TextField
						type="password"
						variant={'outlined'}
						label={'Password'}
						name={'password'}
						onChange={onPasswordChange}
						fullWidth={true}
					/>
				</Grid>
				<Grid item xs={12} sm={4}>
					<Button
						variant="contained"
						color="primary"
						onClick={onLogin}
						fullWidth={true}
						size={'large'}
						disableElevation
					>
						Login
					</Button>
				</Grid>
			</Grid>
		)
	}

	function render() {
		const username = localStorage.getItem('username')

		if (username !== null) {
			return <></>
		}

		return renderLogin()
	}

	return render()
}

export default Login
