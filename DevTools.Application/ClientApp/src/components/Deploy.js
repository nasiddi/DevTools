import React, { useEffect, useState } from 'react'
import { Alert, Button, Col, Container, Row, Spinner } from 'reactstrap'
import { Checkbox, FormControlLabel } from '@material-ui/core'
import { get, post } from '../BackendClient'

export function Deploy() {
	const manualStates = {
		isRunning: false,
		isSuccess: false,
		isUpToDate: false,
		isUnAuth: false,
		isFailed: false,
	}
	const defaultAlert = { text: '', color: 'success', showSpinner: false }
	const [state, setState] = useState({
		commit: {},
		taskStates: manualStates,
		alert: defaultAlert,
		deployOnChange: false,
	})

	function getAlert(status) {
		if (status.isEnabled) {
			return status.isRunning
				? { text: 'Uploading Files', color: 'info', showSpinner: true }
				: defaultAlert
		}

		if (state.taskStates.isSuccess) {
			return { text: 'Deployment completed.', color: 'success' }
		}

		if (status.isRunning) {
			return { text: 'Uploading Files', color: 'info', showSpinner: true }
		}

		if (state.taskStates.isUpToDate) {
			return { text: 'Already up to date.', color: 'warning' }
		}

		if (state.taskStates.isUnAuth) {
			return {
				text: 'Unauthorized: Try logging out and logging back in.',
				color: 'danger',
			}
		}

		if (state.taskStates.isFailed) {
			return { text: 'Server Error', color: 'danger' }
		}

		return defaultAlert
	}

	async function getLatestCommit() {
		const response = await get('deploy/commit')
		if (response.status === 200) {
			const backgroundTask = await get('deploy/background-task')

			if (backgroundTask.status === 200) {
				const data = await response.json()
				const status = await backgroundTask.json()

				const alert = getAlert(status)

				setState({
					...state,
					commit: data,
					loading: false,
					deployOnChange: status.isEnabled,
					taskStates: status.isEnabled
						? manualStates
						: state.taskStates,
					alert: alert,
				})

				return
			}

			response.text().then((t) =>
				setState({
					...state,
					loading: false,
					alert: { text: `Server Error: ${t}`, color: 'danger' },
				})
			)
		}
	}

	useEffect(() => {
		getLatestCommit(false).then()
	}, [])

	async function deploy() {
		const taskStates = manualStates
		taskStates.isRunning = true
		setState({ ...state, taskStates: taskStates })
		await post('deploy/spa').then(async (r) => {
			if (r.status === 200) {
				r.text().then((t) => {
					if (t === 'true') {
						const states = manualStates
						states.isSuccess = true
						setState({ ...state, taskStates: states })
					} else {
						const states = manualStates
						states.isUpToDate = true
						setState({ ...state, taskStates: states })
					}
				})

				return
			}

			if (r.status === 401) {
				const states = manualStates
				states.isUnAuth = true
				setState({ ...state, taskStates: states })

				return
			}

			if (r.status === 500) {
				r.text().then((t) => {
					const states = manualStates
					states.isFailed = true
					setState({ ...state, taskStates: states })
				})
			}
		})
	}

	async function onCheckBoxChange(event) {
		const checked = event.target.checked
		const name = event.target.name
		await post(`deploy/on-change?deployOnChange=${checked}`)
		setState({ ...state, [name]: checked })
	}

	function renderContent() {
		const commit = state.commit

		return (
			<div>
				<h4>Currently Deployed Commit</h4>
				<FormControlLabel
					control={
						<Checkbox
							checked={state.deployOnChange}
							onChange={onCheckBoxChange}
							name="deployOnChange"
							color="primary"
						/>
					}
					label="Deploy on change"
				/>{' '}
				<Container className={'commit-container'}>
					<Row>
						<Col>
							<b>Hash</b>
						</Col>
						<Col>
							<b>Message</b>
						</Col>
						<Col>
							<b>Date</b>
						</Col>
					</Row>
					<Row>
						<Col>{commit.hash}</Col>
						<Col>{commit.message}</Col>
						<Col>{commit.date}</Col>
					</Row>
					<Row>
						<Col />
					</Row>
				</Container>
				<Button
					onClick={deploy}
					color={'primary'}
					block
					disabled={state.deployOnChange}
				>
					Deploy
				</Button>
			</div>
		)
	}

	function renderAlert(alert) {
		const spinner = alert.showSpinner ? (
			<span>
				<Spinner size="sm" color="info" />{' '}
			</span>
		) : (
			<div />
		)

		return (
			<Alert color={alert.color}>
				{spinner}
				{alert.text}
			</Alert>
		)
	}

	const contents = state.loading ? (
		<p>
			<em>Loading...</em>
		</p>
	) : (
		renderContent()
	)

	const alert =
		state.alert.text.length === 0 ? <div /> : renderAlert(state.alert)

	return (
		<div className={'deploy'}>
			{contents}
			{alert}
		</div>
	)
}
