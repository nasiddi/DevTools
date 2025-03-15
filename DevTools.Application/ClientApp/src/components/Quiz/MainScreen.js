import React, { useEffect, useState } from 'react'
import { Box, Button, Chip, Grid, List, ListItem, Typography } from '@mui/material'
import TaskAltIcon from '@mui/icons-material/TaskAlt'
import { get } from '../../BackendClient'
import Grid2 from '@mui/material/Unstable_Grid2'
import RuleIcon from '@mui/icons-material/Rule'
import PhoneEnabledIcon from '@mui/icons-material/PhoneEnabled'
import GroupsIcon from '@mui/icons-material/Groups'

export const letters = ['A', 'B', 'C', 'D']

export const jokerIcons = {
	half: <RuleIcon sx={{ fontSize: '4rem' }} />,
	phone: <PhoneEnabledIcon sx={{ fontSize: '4rem' }} />,
	poll: <GroupsIcon sx={{ fontSize: '4rem' }} />,
}

export function QuestionList({ currentIndex, questions }) {
	const maxIndexWidth = Math.max(...questions.map((q) => `${q.index}`.length)) * 20 // Adjust multiplier as needed
	const maxAmountWidth = Math.max(...questions.map((q) => `CHF ${q.amount}`.length)) * 20 // Adjust multiplier as needed

	return (
		<List
			sx={{
				paddingLeft: '200px',
				paddingRight: '200px',
				paddingTop: '50px',
				boxSizing: 'border-box',
			}}
		>
			{questions
				.sort((a, b) => b.index - a.index)
				.map((question) => (
					<ListItem
						key={question.index}
						sx={{
							display: 'flex',
							alignItems: 'center',
							paddingTop: '4px',
							paddingBottom: '4px',
							backgroundColor:
								question.index === currentIndex ? 'orange' : 'transparent',
						}}
					>
						<Typography
							sx={{
								fontFamily: 'Verdana, Arial, sans-serif',
								fontSize: '2rem',
								width: `${maxIndexWidth}px`,
								textAlign: 'right', // Align text to the right
							}}
							variant="h6"
						>
							{question.index}
						</Typography>
						<TaskAltIcon
							sx={{
								fontSize: '2rem',
								margin: '0 16px',
								visibility: currentIndex > question.index ? 'visible' : 'hidden',
							}}
						/>
						<Typography
							sx={{ fontFamily: 'Verdana, Arial, sans-serif', fontSize: '2rem' }}
							variant="h6"
							style={{ width: `${maxAmountWidth}px`, textAlign: 'left' }}
						>
							CHF {question.amount}
						</Typography>
					</ListItem>
				))}
		</List>
	)
}

function Question({ halfJokerIsActive, questionData }) {
	const [question, setQuestion] = useState(questionData)

	useEffect(() => {
		setQuestion(questionData)
	}, [questionData])

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
			}}
		>
			<Grid2 xs={12} sx={{ display: 'flex', justifyContent: 'center', paddingTop: '100px' }}>
				<img
					src="https://cloud.skyship.space/s/LN5eeqQ8rH5xqYs/download/logo.png"
					alt={'logo'}
					style={{ maxWidth: '80%', height: 'auto' }}
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
							padding: '0 16px', // Add padding for better spacing
						}}
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

function renderChip(joker) {
	return (
		<Chip
			key={joker.jokerType}
			color={!joker.questionIndex ? 'primary' : 'default'}
			icon={jokerIcons[joker.jokerType.toLowerCase()]}
			sx={{
				width: '200px', // Increase width
				height: '120px', // Increase height
				fontSize: '4rem', // Increase font size
				borderRadius: '60px', // Make chip oval
				margin: '100px',
			}}
		/>
	)
}

function JokerList({ jokers }) {
	return (
		<Box
			sx={{
				display: 'flex', // Use flexbox for layout
				flexDirection: 'column', // Arrange children in a column
				justifyContent: 'center', // Center content vertically
				alignItems: 'center', // Center content horizontally
				height: '1080px', // Optional: Full viewport height for centering
			}}
		>
			{jokers.map((joker) => renderChip(joker))}
		</Box>
	)
}

function MainScreen() {
	const [quizShow, setQuizShow] = useState(undefined)

	useEffect(() => {
		document.body.style.backgroundColor = 'lightblue'
	}, [])

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

	if (!quizShow) {
		return <></>
	}

	return (
		<Box
			sx={{
				display: 'flex', // Use flexbox layout
				width: '5760px', // Ensure full viewport width
				height: '1080px', // Full viewport height
			}}
		>
			<Box
				sx={{
					width: '960px', // Set width to 1920px
					border: '1px solid black',
				}}
			>
				<QuestionList
					currentIndex={quizShow.questionIndex}
					questions={quizShow.questions}
				/>
			</Box>
			<Box
				sx={{
					width: '960px', // Ensure the width is specified in pixels
					border: '1px solid black', // Add a black border
				}}
			>
				<JokerList jokers={quizShow.jokers} />
			</Box>
			<Box
				sx={{
					width: '1920px', // Set width to 1920px
					border: '1px solid black',
				}}
			>
				<Question
					questionData={quizShow.questions.find(
						(question) => question.index === quizShow.questionIndex
					)}
					halfJokerIsActive={quizShow.jokers.some(
						(e) => e.questionIndex === quizShow.questionIndex
					)}
				/>
			</Box>
			<Box
				sx={{
					width: '1920px', // Set width to 1920px
					border: '1px solid black',
				}}
			/>
		</Box>
	)
}

export default MainScreen
