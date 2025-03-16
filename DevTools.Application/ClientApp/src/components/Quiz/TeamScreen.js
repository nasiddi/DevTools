import React, { useEffect, useState } from 'react'
import { Alert, Box, Button, CircularProgress, Grid, TextField, Typography } from '@mui/material'
import { get, post } from '../../BackendClient'
import { jokerIcons, letters } from './MainScreen'

function generateGUID() {
	return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
		const r = (Math.random() * 16) | 0
		const v = c === 'x' ? r : (r & 0x3) | 0x8

		return v.toString(16)
	})
}

function Question({ halfJokerIsActive, question, team, canSubmitAnswer }) {
	if (!question) {
		return <></>
	}

	const teamAnswer = team.answers.find((answer) => answer.questionIndex === question.index)

	const [selectedAnswerId, setSelectedAnswerId] = useState(0)

	function onClickAnswer(answer) {
		if (question.isLockedIn || !!teamAnswer) {
			return
		}

		setSelectedAnswerId(answer.id)
	}

	async function onConfirm() {
		await post(`quiz/teams/${team.id}/answer/${selectedAnswerId}`)
	}

	function getButtonColor(answer) {
		let color = 'primary'

		const answerId = teamAnswer?.answerId || selectedAnswerId

		if (!answerId) {
			return color
		}

		if (answer.id === answerId) {
			color = 'warning'
		}

		if (question.isLockedIn) {
			if (answer.id === answerId) {
				color = 'error'
			}

			if (answer.isCorrect) {
				color = 'success'
			}
		}

		return color
	}

	function getDisabled(answer) {
		if (!canSubmitAnswer) {
			return true
		}

		if (!!teamAnswer) {
			return answer.id !== selectedAnswerId
		}

		return !halfJokerIsActive ? false : !answer.isInHalfJoker
	}

	return (
		<Grid
			container
			spacing={3}
			sx={{
				justifyContent: 'center',
				alignItems: 'center',
				paddingTop: '20px',
			}}
		>
			<Grid item xs={12}>
				<Typography
					variant="h6"
					sx={{
						fontFamily: 'Verdana, Arial, sans-serif',
						width: '100%',
						backgroundColor: 'primary.main', // Use theme's primary color
						textAlign: 'center', // Center the text
						padding: '20px', // Optionally add padding for better appearance
						color: 'white', // Ensure text is readable on primary background
						boxSizing: 'border-box',
					}}
				>
					{question.questionText}
				</Typography>
			</Grid>
			{question.answers.map((answer, index) => (
				<Grid item xs={12} key={answer.answerText}>
					<Button
						variant="contained"
						color={getButtonColor(answer)}
						disabled={getDisabled(answer)}
						fullWidth
						sx={{
							fontFamily: 'Verdana, Arial, sans-serif',
							width: '100%',
							display: 'flex',
							justifyContent: 'center',
							alignItems: 'center',
						}}
						onClick={() => onClickAnswer(answer)}
					>
						<span style={{ marginRight: 'auto', fontWeight: 'bold' }}>
							{letters[index]}:
						</span>{' '}
						{/* Align 'A:' to the left */}
						<div style={{ flexGrow: 1, textAlign: 'center' }}>
							{answer.answerText}
						</div>{' '}
						{/* Keep answer text centered */}
					</Button>
				</Grid>
			))}
			<Grid
				item
				xs={12}
				sx={{
					display: 'flex',
					justifyContent: 'center',
					alignItems: 'center',
				}}
			>
				<Button
					variant="contained"
					color="success"
					disabled={!selectedAnswerId || !!teamAnswer || !canSubmitAnswer}
					sx={{
						fontFamily: 'Verdana, Arial, sans-serif',
						padding: '0 16px', // Add padding for better spacing
					}}
					onClick={onConfirm}
				>
					Bestätigen
				</Button>
			</Grid>
		</Grid>
	)
}

function renderJoker(joker, onClickJoker) {
	return (
		<Grid item key={joker.jokerType}>
			<Button
				key={joker.jokerType}
				variant="contained"
				disabled={!!joker.questionIndex}
				sx={{
					borderRadius: '50%',
				}}
				onClick={() => onClickJoker(joker)}
			>
				{jokerIcons(2)[joker.jokerType.toLowerCase()]}
			</Button>
		</Grid>
	)
}

function JokerList({ jokers, onClickJoker, showJokers }) {
	return (
		<Grid
			container
			spacing={3}
			sx={{
				justifyContent: 'center',
				paddingTop: '0',
				visibility: showJokers ? 'visible' : 'hidden', // Use visibility to hide/show content
			}}
		>
			{jokers.map((joker) => renderJoker(joker, onClickJoker))}
		</Grid>
	)
}

function Register({ onSignUp, teams }) {
	const [name, setName] = useState('')
	const [nameAlreadyUsed, setNameAlreadyUsed] = useState(false)
	async function onClickSignUp() {
		if (teams.some((team) => team.name === name)) {
			setNameAlreadyUsed(true)

			return
		}

		const teamId = generateGUID()
		const response = await post(`quiz/teams/register?name=${name}&teamId=${teamId}`)

		if (response.status === 400) {
			setNameAlreadyUsed(true)

			return
		}

		localStorage.setItem('teamId', teamId)
		onSignUp()
	}

	function setTeamName(e) {
		setName(e.target.value)

		if (nameAlreadyUsed) {
			setNameAlreadyUsed(false)
		}
	}

	return (
		<Box
			sx={{
				display: 'flex', // Use flexbox for layout
				flexDirection: 'column',
				width: '100vw', // Ensure full viewport width
				height: '100vh', // Full viewport height
				padding: '20px',
			}}
		>
			<Grid
				container
				spacing={3}
				sx={{
					justifyContent: 'center',
					alignItems: 'flex-end',
					paddingTop: '100px',
				}}
			>
				<Grid
					item
					xs={12}
					sx={{ display: 'flex', justifyContent: 'center', paddingTop: '100px' }}
				>
					<img
						src="https://cloud.skyship.space/s/LN5eeqQ8rH5xqYs/download/logo.png"
						alt={'logo'}
						style={{ maxWidth: '80%', height: 'auto' }}
					/>
				</Grid>
				<Grid item xs={12}>
					<TextField
						label="Team Name"
						variant="outlined"
						value={name}
						onChange={(e) => setTeamName(e)}
						fullWidth
						helperText="Maximal 20 Zeichen"
					/>
				</Grid>
				<Grid item xs={12}>
					<Button
						variant="contained"
						color="primary"
						fullWidth
						onClick={onClickSignUp}
						disabled={name.length === 0}
					>
						Anmelden
					</Button>
				</Grid>
				{nameAlreadyUsed && (
					<Grid item xs={12}>
						<Alert color="error">Name wird bereits verwendet</Alert>
					</Grid>
				)}
			</Grid>
		</Box>
	)
}

function TeamScreenInner({ teamId }) {
	const [quizShow, setQuizShow] = useState(undefined)
	const [secondsRemaining, setSecondsRemaining] = useState(0)

	useEffect(() => {
		const interval = setInterval(() => {
			if (!quizShow?.questionStartTime) {
				setSecondsRemaining(0)

				return
			}

			const team = quizShow.teams.find((team) => team.teamId === teamId)
			const teamAnswer = team.answers.find(
				(answer) => answer.questionIndex === quizShow.questionIndex
			)

			if (!!teamAnswer) {
				setSecondsRemaining(0)

				return
			}

			const now = new Date()
			const startTime = new Date(quizShow.questionStartTime)
			const targetTime = new Date(startTime.getTime() + 60 * 1000)
			const seconds = (targetTime - now) / 1000
			setSecondsRemaining(seconds)
		}, 200)

		return () => clearInterval(interval)
	}, [quizShow?.questionStartTime])

	useEffect(() => {
		const intervalId = setInterval(async () => {
			try {
				const fetchedQuizShow = await fetchQuizShow()

				setQuizShow(fetchedQuizShow)
			} catch (error) {
				console.error('Error fetching quiz show:', error)
			}
		}, 200)

		// Cleanup the interval on component unmount
		return () => clearInterval(intervalId)
	}, [])

	async function fetchQuizShow() {
		return await get('quiz')
			.then((r) => r.json())
			.then((j) => {
				return j
			})
	}

	async function onClickJoker(joker) {
		const team = quizShow.teams.find((team) => team.teamId === teamId)
		await post(`quiz/teams/${team.id}/jokers/${joker.id}/use`)
	}

	if (!quizShow) {
		return <></>
	}

	function getProcessColor() {
		if (secondsRemaining <= 10) {
			return 'error'
		}

		if (secondsRemaining <= 30) {
			return 'warning'
		}

		return 'primary'
	}

	const team = quizShow.teams.find((team) => team.teamId === teamId)
	const teamAnswer = team.answers.find(
		(answer) => answer.questionIndex === quizShow.questionIndex
	)

	const canSubmitAnswer = secondsRemaining > 0 && !teamAnswer

	return (
		<Grid
			container
			spacing={3}
			sx={{
				justifyContent: 'center', // Center grid items horizontally
				alignItems: 'flex-end',
				width: '100%',
			}}
		>
			<Grid
				item
				xs={12}
				sx={{
					display: 'flex',
					justifyContent: 'center',
					paddingTop: '20px',
					height: '25vh', // Set height to 25% of the viewport height
				}}
			>
				<img
					src="https://cloud.skyship.space/s/LN5eeqQ8rH5xqYs/download/logo.png"
					alt="logo"
					style={{
						maxWidth: '45%',
						display: !canSubmitAnswer ? 'block' : 'none',
						objectFit: 'contain',
					}}
				/>
				{canSubmitAnswer && (
					<Box
						sx={{
							width: '25vh', // Set width to 25% of viewport height
							height: '25vh', // Set height to 25% of viewport height
							display: 'flex',
							alignItems: 'center',
							justifyContent: 'center',
							position: 'relative',
						}}
					>
						<CircularProgress
							variant="determinate"
							value={(secondsRemaining / 60) * 100}
							size={120} // Use a fixed size and center it within the box
							thickness={4} // Optionally adjust the thickness
							color={getProcessColor()}
						/>
						<Typography
							variant="h6"
							sx={{
								position: 'absolute',
								top: '50%',
								left: '50%',
								transform: 'translate(-50%, -50%)', // Center the text inside the spinner
							}}
						>
							{Math.floor(secondsRemaining)}
						</Typography>
					</Box>
				)}
			</Grid>
			{!quizShow.questionIndex && (
				<Grid item xs={12}>
					<Alert color="info">Bitte warten</Alert>
				</Grid>
			)}
			<Grid item xs={12}>
				{!!quizShow.questionIndex && (
					<JokerList
						jokers={team.jokers}
						onClickJoker={onClickJoker}
						showJokers={canSubmitAnswer}
					/>
				)}
			</Grid>
			<Grid item xs={12}>
				<Question
					halfJokerIsActive={team.jokers.some(
						(e) => e.questionIndex === quizShow.questionIndex && e.jokerType === 'Half'
					)}
					question={quizShow.questions.find(
						(question) => question.index === quizShow.questionIndex
					)}
					canSubmitAnswer={canSubmitAnswer}
					team={team}
				/>
			</Grid>
		</Grid>
	)
}

function TeamScreen() {
	const [quizShow, setQuizShow] = useState(undefined)
	const [teamId, setTeamId] = useState(null)

	useEffect(() => {
		document.body.style.backgroundColor = 'lightblue'

		async function load() {
			const quizShow = await fetchQuizShow()
			setQuizShow(quizShow)
		}

		load().then()
	}, [])

	useEffect(() => {
		const localTeamId = localStorage.getItem('teamId')
		setTeamId(localTeamId)
	}, [])

	async function fetchQuizShow() {
		return await get('quiz')
			.then((r) => r.json())
			.then((j) => {
				return j
			})
	}

	async function reloadQuiz() {
		const quizShow = await fetchQuizShow()
		setQuizShow(quizShow)
	}

	async function onSignUp() {
		const localTeamId = localStorage.getItem('teamId')
		await reloadQuiz()
		setTeamId(localTeamId)
	}

	if (!quizShow) {
		return <></>
	}

	if (!teamId) {
		return <Register onSignUp={onSignUp} teams={quizShow.teams} />
	}

	const team = quizShow.teams.find((team) => team.teamId === teamId)

	if (!team) {
		localStorage.removeItem('teamId')
		setTeamId(null)
	}

	return <TeamScreenInner teamId={teamId} />
}

export default TeamScreen
