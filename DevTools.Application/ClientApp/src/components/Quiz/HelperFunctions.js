export function GetTeamAnswers(quizShow) {
	const questionAnswers = quizShow.questions.find(
		(e) => e.index === quizShow.questionIndex
	).answers

	const answers = quizShow.teams
		.filter(
			(e) =>
				e.jokers.find((j) => j.jokerType === 'Poll')?.questionIndex !==
				quizShow.questionIndex
		)
		.map((e) => e.answers.find((a) => a.questionIndex === quizShow.questionIndex))
		.filter((e) => !!e)

	const totalAnswers = answers.length

	// Group answers by answerId
	const groupedByAnswerId = answers.reduce((acc, answer) => {
		if (!acc[answer.answerId]) {
			acc[answer.answerId] = []
		}
		acc[answer.answerId].push(answer)

		return acc
	}, {})

	// Find the maximum group size
	const maxGroupSize = Math.max(...Object.values(groupedByAnswerId).map((group) => group.length))

	// Initialize result with all possible answerIds from questionAnswers
	const result = questionAnswers.reduce((acc, questionAnswer) => {
		const answerId = questionAnswer.id
		const group = groupedByAnswerId[answerId] || []
		const groupSize = group.length

		acc[answerId] = {
			isWinner: groupSize === maxGroupSize && maxGroupSize > 0,
			percent: totalAnswers > 0 ? Math.round((groupSize / totalAnswers) * 100) : 0,
		}

		return acc
	}, {})

	return result
}
