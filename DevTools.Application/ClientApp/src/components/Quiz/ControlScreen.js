import React, { useEffect, useState } from 'react'
import { Box, Button, Grid, Typography } from '@mui/material'
import { get, post } from '../../BackendClient'
import { jokerIcons, letters } from './MainScreen'

function Question({ halfJokerIsActive, questionData }) {
	if (!questionData) {
		return <></>
	}

	const [question, setQuestion] = useState(questionData)

	useEffect(() => {
		setQuestion(questionData)
	}, [questionData])

	async function onClickAnswer(answer) {
		if (question.isLockedIn) {
			return
		}

		const currentData = JSON.parse(JSON.stringify(question))

		for (const a of currentData.answers) {
			if (a.isSelectedByContestant && a.answerText === answer.answerText) {
				currentData.isLockedIn = true
				await post('quiz/questions/current/locked-in')
			} else a.isSelectedByContestant = a.answerText === answer.answerText
		}

		const selectedAnswer = currentData.answers.find((answer) => answer.isSelectedByContestant)
		const previouslySelectedAnswer = question.answers.find(
			(answer) => answer.isSelectedByContestant
		)

		if (selectedAnswer !== previouslySelectedAnswer) {
			await post(`quiz/answers/${selectedAnswer.id}/current`)
		}

		setQuestion(currentData)
	}

	function getButtonColor(answer) {
		let color = 'primary'

		if (answer.isSelectedByContestant) {
			color = 'warning'
		}

		if (question.isLockedIn) {
			if (answer.isSelectedByContestant) {
				color = 'error'
			}

			if (answer.isCorrect) {
				color = 'success'
			}
		}

		return color
	}

	return (
		<Grid
			container
			spacing={3}
			sx={{
				justifyContent: 'center',
				alignItems: 'flex-end',
				paddingTop: '100px',
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
						fontSize: '2.2rem',
					}}
				>
					{question.questionText}
				</Typography>
			</Grid>
			{question.answers.map((answer, index) => (
				<Grid item xs={6} key={answer.answerText}>
					<Button
						variant="contained"
						color={getButtonColor(answer)}
						disabled={!halfJokerIsActive ? false : !answer.isInHalfJoker}
						fullWidth
						sx={{
							fontFamily: 'Verdana, Arial, sans-serif',
							width: '100%',
							fontSize: '2rem',
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
		</Grid>
	)
}

function renderJoker(joker, onClickJoker) {
	return (
		<Button
			key={joker.jokerType}
			variant="contained"
			disabled={!!joker.questionIndex}
			sx={{
				fontSize: '4rem', // Increase font size
				margin: '100px',
			}}
			onClick={() => onClickJoker(joker)}
		>
			{jokerIcons(4)[joker.jokerType.toLowerCase()]}
		</Button>
	)
}

function JokerList({ jokers, onClickJoker }) {
	return (
		<Box
			sx={{
				display: 'flex', // Use flexbox for layout
				justifyContent: 'center', // Center content vertically
				alignItems: 'center', // Center content horizontally
			}}
		>
			{jokers.map((joker) => renderJoker(joker, onClickJoker))}
		</Box>
	)
}

function ControlScreen() {
	const [quizShow, setQuizShow] = useState(undefined)

	useEffect(() => {
		document.body.style.backgroundColor = 'lightblue'

		async function load() {
			const quizShow = await fetchQuizShow()
			setQuizShow(quizShow)
		}

		load().then()
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

	async function onResetQuiz() {
		await post('quiz/reset')
		await reloadQuiz()
	}

	async function onClickJoker(joker) {
		await post(`quiz/jokers/${joker.id}/use`)
		await reloadQuiz()
	}

	async function onNextQuestion() {
		const questionIndex = quizShow.questionIndex + 1
		await post(`quiz/questions/current?questionIndex=${questionIndex}`)
		setQuizShow({ ...quizShow, questionIndex: questionIndex })
	}

	if (!quizShow) {
		return <></>
	}

	return (
		<Box
			sx={{
				display: 'flex', // Use flexbox for layout
				flexDirection: 'column',
				width: '100vw', // Ensure full viewport width
				height: '100vh', // Full viewport height
				padding: '100px',
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
				<Grid item xs={12}>
					<JokerList jokers={quizShow.jokers} onClickJoker={onClickJoker} />
				</Grid>
				<Grid item xs={6}>
					<Button
						variant="contained"
						color="primary"
						fullWidth
						sx={{
							fontFamily: 'Verdana, Arial, sans-serif',
							width: '100%',
							fontSize: '2rem',
							display: 'flex',
							justifyContent: 'center',
							alignItems: 'center',
							padding: '0 16px', // Add padding for better spacing
						}}
						onClick={onNextQuestion}
					>
						Next Question
					</Button>
				</Grid>
				<Grid item xs={6}>
					<Button
						variant="contained"
						color="secondary"
						fullWidth
						sx={{
							fontFamily: 'Verdana, Arial, sans-serif',
							width: '100%',
							fontSize: '2rem',
							display: 'flex',
							justifyContent: 'center',
							alignItems: 'center',
							padding: '0 16px', // Add padding for better spacing
						}}
						onClick={onResetQuiz}
					>
						Reset Quiz
					</Button>
				</Grid>
			</Grid>
			<Box
				sx={{
					flex: 1, // Takes up one-third of the horizontal space
				}}
			>
				<Question
					halfJokerIsActive={quizShow.jokers.some(
						(e) => e.questionIndex === quizShow.questionIndex && e.jokerType === 'Half'
					)}
					questionData={quizShow.questions.find(
						(question) => question.index === quizShow.questionIndex
					)}
				/>
			</Box>
		</Box>
	)
}

export default ControlScreen
