import React, { useState } from 'react'
import { postForm } from '../BackendClient'
import { Button, Alert, AlertTitle, Grid, LinearProgress } from '@mui/material'
import OpenInNewIcon from '@mui/icons-material/OpenInNew'
import FileCopyIcon from '@mui/icons-material/FileCopy'

export const FileUpload = () => {
	const [state, setState] = useState({
		files: [],
		results: [],
		inputKey: Math.random().toString(36),
		showProgress: false,
	})

	const saveFileSelected = (e) => {
		const fileArray = Array.from(e.target.files)
		setState({ ...state, files: fileArray, results: [] })
	}

	const openInNewTab = (e) => {
		window.open(`/file-download/${e.currentTarget.id}`, '_blank', 'noopener,noreferrer')
	}

	const importFile = async () => {
		setState({ ...state, showProgress: true })
		const formData = new FormData()

		state.files.forEach(function (value, i) {
			formData.append(`file${i}`, value)
		})

		try {
			const res = await postForm('file', formData)
			const result = await res.json()
			setState({
				...state,
				files: [],
				results: result,
				showProgress: false,
				inputKey: Math.random().toString(36),
			})
		} catch (ex) {
			setState({
				...state,
				files: [],
				showProgress: false,
				inputKey: Math.random().toString(36),
				results: [
					{
						fileName: 'All Files',
						resultType: 'Failed',
						error: ex.toString(),
					},
				],
			})
		}
	}

	const fileList = state.files.map((f) => (
		<Grid item xs={12} key={f.name}>
			<Alert severity="info">
				<AlertTitle>Ready to Upload</AlertTitle>
				{f.name}
			</Alert>
		</Grid>
	))

	const resultList = state.results.map((r) => {
		if (r.resultType === 'Success') {
			return (
				<Grid item xs={12} key={r.fileName}>
					<Alert
						severity="success"
						action={
							<>
								<Button onClick={openInNewTab} id={r.guid}>
									<OpenInNewIcon color={'primary'} />
								</Button>
								<Button
									onClick={() => {
										navigator.clipboard.writeText(
											`${location.protocol}
												//${location.host}/file-download/${r.guid}`
										)
									}}
								>
									<FileCopyIcon />
								</Button>
							</>
						}
					>
						<AlertTitle>{r.resultType}</AlertTitle>
						{r.fileName}
					</Alert>
				</Grid>
			)
		}

		return (
			<Grid item xs={12} key={r.fileName}>
				<Alert severity="error">
					<AlertTitle>{r.resultType}</AlertTitle>
					{r.fileName} {r.error}
				</Alert>
			</Grid>
		)
	})

	fileList.push(resultList)

	const progressBar = state.showProgress ? (
		<LinearProgress />
	) : (
		<LinearProgress variant="determinate" value={0} />
	)

	return (
		<Grid container spacing={3}>
			<Grid item>
				<input
					id="contained-button-file"
					type="file"
					key={state.inputKey}
					onChange={saveFileSelected}
					multiple
					style={{ display: 'none' }}
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
						Select Files
					</Button>
				</label>
			</Grid>
			<Grid item>
				<Button
					variant={'contained'}
					color={'primary'}
					name="Upload"
					onClick={importFile}
					disabled={state.files.length === 0 || state.showProgress}
				>
					Upload
				</Button>
			</Grid>
			<Grid item xs={12}>
				{progressBar}
			</Grid>
			{fileList}
		</Grid>
	)
}
