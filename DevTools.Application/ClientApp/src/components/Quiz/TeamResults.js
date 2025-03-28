import React from 'react'
import { Grid, Paper, Typography } from '@mui/material'
import CancelIcon from '@mui/icons-material/Cancel'
import DoNotDisturbOnIcon from '@mui/icons-material/DoNotDisturbOn'
import CheckCircleIcon from '@mui/icons-material/CheckCircle'
import CircleIcon from '@mui/icons-material/Circle'
import RuleIcon from '@mui/icons-material/Rule'
import GroupsIcon from '@mui/icons-material/Groups'
import LooksOneIcon from '@mui/icons-material/LooksOne'
import LooksTwoIcon from '@mui/icons-material/LooksTwo'
import Looks3Icon from '@mui/icons-material/Looks3'

const teamIconSx = {
	fontSize: '2rem',
	margin: '0 10px',
	backgroundColor: 'white',
	borderRadius: '50%',
}

const rankings = [
	<LooksOneIcon
		key={1}
		sx={{
			fontSize: '3rem',
		}}
	/>,
	<LooksTwoIcon
		key={2}
		sx={{
			fontSize: '3rem',
		}}
	/>,
	<Looks3Icon
		key={3}
		sx={{
			fontSize: '3rem',
		}}
	/>,
]

function TeamAnswer({ answers, questionIndex, lastLockedInQuestionIndex }) {
	const answer = answers.find((e) => e.questionIndex === questionIndex)

	if (questionIndex > lastLockedInQuestionIndex) {
		return (
			<CircleIcon
				sx={{
					fontSize: '2rem',
					margin: '0 10px',
					backgroundColor: 'white',
					borderRadius: '50%',
					color: !answer ? '#303030' : 'primary.main',
				}}
			/>
		)
	}

	if (!answer) {
		return <DoNotDisturbOnIcon color="warning" sx={teamIconSx} />
	}

	return answer.isCorrect ? (
		<CheckCircleIcon color="success" sx={teamIconSx} />
	) : (
		<CancelIcon color="error" sx={teamIconSx} />
	)
}

function TeamResults({ quizShow }) {
	const maxNameWidth = Math.max(...quizShow.teams.map((q) => `${q.name}`.length)) * 27 // Adjust multiplier as needed

	const lastLockedInQuestionIndex = quizShow.questions
		.filter((obj) => obj.isLockedIn)
		.reduce((max, obj) => (obj.index > max.index ? obj : max), { index: -1 }).index

	function renderHalfJoker(team) {
		const joker = team.jokers.find((joker) => joker.jokerType === 'Half')
		const backgroundColor = !joker?.questionIndex
			? 'primary.main'
			: joker.questionIndex === quizShow.questionIndex
				? 'success.main'
				: 'grey'

		return (
			<RuleIcon
				sx={{
					...teamIconSx,
					color: 'white',
					backgroundColor: backgroundColor,
					width: '4rem',
					marginLeft: '50px',
				}}
			/>
		)
	}

	function renderPollJoker(team) {
		const joker = team.jokers.find((joker) => joker.jokerType === 'Poll')
		const backgroundColor = !joker?.questionIndex ? 'primary.main' : 'grey'

		return (
			<GroupsIcon
				sx={{
					...teamIconSx,
					color: 'white',
					backgroundColor: backgroundColor,
					width: '4rem',
					marginRight: '50px',
				}}
			/>
		)
	}

	function sortTeams(teams) {
		return teams.sort((teamA, teamB) => {
			// Calculate correct answers count
			const correctAnswersA = teamA.answers.filter(
				(answer) => answer.isCorrect && answer.questionIndex <= lastLockedInQuestionIndex
			)
			const correctAnswersB = teamB.answers.filter(
				(answer) => answer.isCorrect && answer.questionIndex <= lastLockedInQuestionIndex
			)

			// Calculate total answer time
			const totalTimeA = correctAnswersA.reduce(
				(total, answer) => total + answer.answerTimeMilliseconds,
				0
			)
			const totalTimeB = correctAnswersB.reduce(
				(total, answer) => total + answer.answerTimeMilliseconds,
				0
			)

			// Sort by number of correct answers descending, then by total answer time ascending
			if (correctAnswersA.length !== correctAnswersB.length) {
				return correctAnswersB.length - correctAnswersA.length // More correct answers first
			} else {
				return totalTimeA - totalTimeB // Shorter total answer time first
			}
		})
	}

	const teams = sortTeams(quizShow.teams)

	return (
		<Grid container spacing={2} sx={{ padding: '50px' }}>
			{teams.map((team, index) => (
				<Grid item xs={12} key={team.id}>
					<Paper
						sx={{
							display: 'flex',
							alignItems: 'center',
							paddingLeft: '20px',
							paddingRight: '20px',
							backgroundColor: '#303030',
							color: 'white',
						}}
					>
						<Grid
							container
							direction="row"
							sx={{
								justifyContent: 'space-between',
								alignItems: 'center',
							}}
						>
							<Grid item>
								<Typography
									sx={{
										fontFamily: 'Verdana, Arial, sans-serif',
										fontSize: '2rem',
										width: `${maxNameWidth}px`,
									}}
									variant="h6"
								>
									{team.name}
								</Typography>
							</Grid>
							<Grid item>
								{renderHalfJoker(team)}
								{renderPollJoker(team)}
							</Grid>
							<Grid item>
								{Array.from({ length: 15 }, (_, i) => (
									<TeamAnswer
										key={i + 1}
										answers={team.answers}
										questionIndex={i + 1}
										lastLockedInQuestionIndex={lastLockedInQuestionIndex}
									/>
								))}
							</Grid>
							<Grid
								item
								sx={{
									visibility:
										index > 2 || lastLockedInQuestionIndex === -1
											? 'hidden'
											: 'visible',
								}}
							>
								{index <= 2 && rankings[index]}
								{index > 2 && (
									<LooksOneIcon
										key={1}
										sx={{
											fontSize: '3rem',
											visibility: 'hidden',
										}}
									/>
								)}
							</Grid>
						</Grid>
					</Paper>
				</Grid>
			))}
		</Grid>
	)
}

export default TeamResults
