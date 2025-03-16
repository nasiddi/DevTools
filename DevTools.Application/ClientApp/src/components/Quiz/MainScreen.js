import React, { useEffect, useState } from 'react'
import { Box, Button, Chip, Grid, LinearProgress, List, ListItem, Typography } from '@mui/material'
import TaskAltIcon from '@mui/icons-material/TaskAlt'
import { get } from '../../BackendClient'
import RuleIcon from '@mui/icons-material/Rule'
import PhoneEnabledIcon from '@mui/icons-material/PhoneEnabled'
import GroupsIcon from '@mui/icons-material/Groups'
import { GetTeamAnswers } from './HelperFunctions'

export const letters = ['A', 'B', 'C', 'D']

export const jokerIcons = (rem) => ({
	half: <RuleIcon sx={{ fontSize: `${rem}rem` }} />,
	phone: <PhoneEnabledIcon sx={{ fontSize: `${rem}rem` }} />,
	poll: <GroupsIcon sx={{ fontSize: `${rem}rem` }} />,
})

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

function AnswerButton({ answer, index, halfJokerIsActive, pollResult, isLockedIn }) {
	function getButtonColor(answer) {
		let color = 'primary'

		if (answer.isSelectedByContestant) {
			color = 'warning'
		}

		if (isLockedIn) {
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
			direction="column"
			spacing={2}
			sx={{
				alignItems: 'center',
				width: '100%',
			}}
		>
			<Grid item xs={12} sx={{ width: '100%' }}>
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
				>
					<span style={{ marginRight: 'auto', fontWeight: 'bold' }}>
						{letters[index]}:
					</span>
					<div style={{ flexGrow: 1, textAlign: 'center' }}>{answer.answerText}</div>
				</Button>
			</Grid>
			<Grid
				item
				xs={12}
				sx={{ width: '100%', visibility: !!pollResult ? 'visible' : 'hidden' }}
			>
				<LinearProgress
					color={pollResult?.isWinner ? 'warning' : 'inherit'}
					variant="determinate"
					value={pollResult?.percent || 0}
					sx={{ height: '20px' }}
				/>
			</Grid>
		</Grid>
	)
}

function Question({ halfJokerIsActive, questionData, teamAnswers }) {
	if (!questionData) {
		return (
			<Grid
				container
				spacing={3}
				sx={{
					justifyContent: 'center',
					alignItems: 'flex-end',
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
			</Grid>
		)
	}

	const [question, setQuestion] = useState(questionData)

	useEffect(() => {
		setQuestion(questionData)
	}, [questionData])

	return (
		<Grid
			container
			spacing={3}
			sx={{
				justifyContent: 'center',
				alignItems: 'flex-end',
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
					<AnswerButton
						key={index}
						answer={answer}
						index={index}
						halfJokerIsActive={halfJokerIsActive}
						pollResult={!!teamAnswers && teamAnswers[answer.id]}
						isLockedIn={question.isLockedIn}
					/>
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
			icon={jokerIcons(4)[joker.jokerType.toLowerCase()]}
			sx={{
				width: '200px', // Increase width
				height: '120px', // Increase height
				fontSize: '4rem', // Increase font size
				borderRadius: '50%',
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

	const teamAnswers =
		quizShow.jokers.some(
			(e) => e.questionIndex === quizShow.questionIndex && e.jokerType === 'Poll'
		) && GetTeamAnswers(quizShow)

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
						(e) => e.questionIndex === quizShow.questionIndex && e.jokerType === 'Half'
					)}
					teamAnswers={teamAnswers}
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
