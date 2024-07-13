import React, { useState } from 'react'
import { postForm } from '../BackendClient'
import { Alert, Button, Checkbox, FormControlLabel, FormGroup, Grid } from '@mui/material'

export const MasterListConverter = () => {
	const [state, setState] = useState({
		showProgress: false,
		files: [],
		filetypes: [
			{
				name: 'Feriendorf Masterlist',
				isSelected: false,
			},
			{
				name: 'FerienAmMeer Masterlist',
				isSelected: false,
			},
			{
				name: 'FerienAmMeer Ferry Vouchers',
				isSelected: false,
			},
			{
				name: 'Flight List',
				isSelected: false,
			},
			{
				name: 'Room List',
				isSelected: false,
			},
		],
		alert: {
			level: 'success',
			message: '',
		},
	})

	const convert = async (e) => {
		const fileTypes = state.filetypes
			.filter((e) => e.isSelected)
			.map((e) => e.name.replaceAll(' ', ''))

		if (fileTypes.length === 0) {
			setState({
				...state,
				alert: {
					level: 'error',
					message: 'At least one file type needs to be selected',
				},
			})

			return
		}

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
			const res = await postForm(
				`ListConversion?fileTypes=${fileTypes.join('&fileTypes=')}`,
				formData
			)
			if (res.status !== 200) {
				throw new Error('Status not ok')
			}

			const data = await res.blob()
			const url = window.URL.createObjectURL(data)
			const link = document.createElement('a')
			link.href = url

			const filename = res.headers
				.get('content-disposition')
				.split(';')[1]
				.replace('filename=', '')
				.trim()

			link.setAttribute('download', filename)
			document.body.appendChild(link)
			link.click()

			setState({
				...state,
				showProgress: false,
				files: [],
				alert: {
					level: 'success',
					message: `Conversion successful. File downloaded as ${filename}`,
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

	function handleCheckbox({ currentTarget }) {
		console.log(event)

		const fileTypes = state.filetypes
		const fileType = fileTypes.find((e) => e.name === currentTarget.name)

		fileType.isSelected = currentTarget.checked

		setState({ ...state, fileTypes })
	}

	return (
		<Grid container direction="column" justifyContent="center" alignItems="center" spacing={4}>
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
				<FormGroup>
					{state.filetypes.map((t) => (
						<FormControlLabel
							key={t.name}
							control={
								<Checkbox
									checked={t.isSelected}
									onChange={handleCheckbox}
									name={t.name}
									color="primary"
								/>
							}
							label={t.name}
						/>
					))}
				</FormGroup>
			</Grid>
			<Grid item xs={12}>
				{state.alert.message.length > 0 ? (
					<Alert severity={state.alert.level}>{state.alert.message}</Alert>
				) : (
					<></>
				)}
			</Grid>
		</Grid>
	)
}
