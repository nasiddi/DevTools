import React, { useEffect, useState } from 'react'
import { Box, Button, Grid, List, ListItem, Typography } from '@mui/material'
import TaskAltIcon from '@mui/icons-material/TaskAlt'
import { get, post } from '../../BackendClient'
import Grid2 from '@mui/material/Unstable_Grid2'
import { QuestionList, shuffleArray } from './MainScreen'

function Question({ questionData, onNextQuestion }) {
	const [question, setQuestion] = useState(() => {
		const shuffledAnswers = shuffleArray(questionData.answers)

		return { ...questionData, answers: shuffledAnswers }
	})

	useEffect(() => {
		const shuffledAnswers = shuffleArray(questionData.answers)
		setQuestion({ ...questionData, answers: shuffledAnswers })
	}, [questionData])

	async function onClickAnswer(answer) {
		if (question.answers.some((e) => e.isLockedIn)) {
			return
		}

		const currentData = JSON.parse(JSON.stringify(question))

		currentData.answers.forEach((a) => {
			if (a.isSelectedByContestant && a.answerText === answer.answerText) {
				a.isLockedIn = true
			} else a.isSelectedByContestant = a.answerText === answer.answerText
		})

		setQuestion(currentData)

		if (currentData.answers.some((e) => e.isLockedIn)) {
			return
		}

		const selectedAnswer = currentData.answers.find((answer) => answer.isSelectedByContestant)
		await post(`quiz/answers/${selectedAnswer.id}/current`)
	}

	function getButtonColor(answer) {
		if (answer.isLockedIn) {
			return answer.isCorrect ? 'success' : 'error'
		}

		return answer.isSelectedByContestant ? 'warning' : 'primary'
	}

	return (
		<Grid
			container
			spacing={3}
			sx={{
				justifyContent: 'center',
				alignItems: 'flex-end',
			}}
		>
			<Grid2 xs={12} sx={{ display: 'flex', justifyContent: 'center', paddingTop: '100px' }}>
				<img
					src="https://cloud.skyship.space/s/LN5eeqQ8rH5xqYs/download/logo.png"
					alt={'logo'}
					style={{ maxWidth: '80%', height: 'auto' }}
					onClick={onNextQuestion}
				/>
			</Grid2>
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
			{question.answers.map((answer) => (
				<Grid item xs={6} key={answer.answerText}>
					<Button
						variant="contained"
						color={getButtonColor(answer)}
						fullwidth
						sx={{ fontFamily: 'Verdana, Arial, sans-serif', width: '100%' }}
						onClick={() => onClickAnswer(answer)}
					>
						{answer.answerText}
					</Button>
				</Grid>
			))}
		</Grid>
	)
}

function JokerList() {
	return (
		<Box
			sx={{
				height: '100vh', // Fill the vertical space
				width: '100vw', // Fill the horizontal space
				display: 'flex', // Optional: Use flexbox for content alignment
				justifyContent: 'center', // Center content horizontally
				alignItems: 'center', // Center content vertically
			}}
		>
			{/* Content inside the Box */}
			<h1>Centered Content</h1>
		</Box>
	)
}

function MainScreen() {
	const [questions, setQuestions] = useState([])
	const [currentIndex, setCurrentIndex] = useState(1)

	useEffect(() => {
		document.body.style.overflow = 'hidden'
		document.body.style.backgroundColor = 'lightblue'

		async function load() {
			const data = await fetchQuestions()
			const quizShow = await fetchQuizShow()
			setQuestions(data)
			setCurrentIndex(quizShow.questionIndex)
		}

		load().then()

		return () => {
			document.body.style.overflow = '' // Cleanup on unmount
		}
	}, [])

	async function fetchQuestions() {
		return await get('quiz/questions')
			.then((r) => r.json())
			.then((j) => {
				return j
			})
	}

	async function fetchQuizShow() {
		return await get('quiz/questions/current')
			.then((r) => r.json())
			.then((j) => {
				return j
			})
	}

	async function onNextQuestion() {
		setCurrentIndex(currentIndex + 1)
		await post(`quiz/questions/current?questionIndex=${currentIndex + 1}`)
	}

	if (questions.length === 0) {
		return <></>
	}

	return (
		<Box
			sx={{
				display: 'flex', // Use flexbox for layout
				width: '100vw', // Ensure full viewport width
				height: '100vh', // Full viewport height
			}}
		>
			<Box
				sx={{
					flex: 1, // Takes up one-third of the horizontal space
				}}
			>
				<QuestionList currentIndex={currentIndex} questions={questions} />
			</Box>
			<Box
				sx={{
					flex: 1, // Takes up one-third of the horizontal space
				}}
			>
				<Question
					questionData={questions.find((question) => question.index === currentIndex)}
					onNextQuestion={onNextQuestion}
				/>
			</Box>
			<Box
				sx={{
					flex: 1, // Takes up one-third of the horizontal space
				}}
			/>
		</Box>
	)
}

export default MainScreen
