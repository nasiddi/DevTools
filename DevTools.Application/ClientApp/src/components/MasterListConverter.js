import React, { useState } from 'react'
import Button from '@material-ui/core/Button'
import { postForm } from '../BackendClient'
import { Alert } from '@material-ui/lab'
import { Grid } from '@material-ui/core'

export const MasterListConverter = () => {
	const [state, setState] = useState({
		showProgress: false,
		files: [],
		alert: {
			level: 'success',
			message: '',
		},
	})

	const convert = async (e) => {
		setState({ ...state, showProgress: true })
		const file = Array.from(e.target.files)[0]

		if (file.type !== 'text/csv') {
			setState({
				...state,
				showProgress: false,
				files: [],
				alert: {
					level: 'error',
					message: 'File should be a csv',
				},
			})

			return
		}

		const formData = new FormData()

		formData.append('file', file)

		try {
			const res = await postForm('ListConversion', formData)
			if (res.status !== 200) {
				throw new Error('Status not ok')
			}
			const url = window.URL.createObjectURL(new Blob([res.data]))
			const link = document.createElement('a')
			link.href = url
			link.setAttribute('download', 'masterlist.csv')
			document.body.appendChild(link)
			link.click()
			link.remove()
			setState({
				...state,
				showProgress: false,
				files: [],
				alert: {
					level: 'success',
					message:
						'Conversion successful. File downloaded as masterlist.csv',
				},
			})
		} catch (ex) {
			setState({
				...state,
				showProgress: false,
				files: [],
				alert: {
					level: 'error',
					message: 'Conversion failed',
				},
			})
		}
	}

	return (
		<Grid
			container
			direction="column"
			justifyContent="center"
			alignItems="center"
			spacing={4}
		>
			<Grid item xs={12}>
				<input
					id="contained-button-file"
					type="file"
					onChange={convert}
					multiple
					style={{ display: 'none' }}
					value={state.files}
				/>
				<label htmlFor="contained-button-file">
					<Button
						htmlFor="contained-button-file"
						variant="contained"
						color="primary"
						component="span"
						fullWidth
						disabled={state.showProgress}
					>
						Select exported csv
					</Button>
				</label>
			</Grid>
			<Grid item xs={12}>
				{state.alert.message.length > 0 ? (
					<Alert severity={state.alert.level}>
						{state.alert.message}
					</Alert>
				) : (
					<></>
				)}
			</Grid>
		</Grid>
	)
}
