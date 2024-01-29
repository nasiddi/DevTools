import React, { useEffect, useState } from 'react'
import {
	BarChart,
	Bar,
	XAxis,
	YAxis,
	CartesianGrid,
	Tooltip,
	Legend,
	LineChart,
	Line,
} from 'recharts'
import { get } from '../../BackendClient'

export const Statistics = () => {
	const [state, setState] = useState({
		statistics: {},
	})

	const loadData = async () => {
		const response = await get('citadels/statistic-data')

		setState({ statistics: await response.json() })
	}

	useEffect(() => {
		loadData().then()
	}, [])

	console.log(state)

	function mapCharacterDistributionData(characters) {
		const data = characters.map((c) => {
			const wonPlayed = c.characterPlayed.reduce(
				(obj, item) => ((obj[`${item.playerName} won`] = item.playedInWonGame), obj),
				{}
			)

			const lostPlayed = c.characterPlayed.reduce(
				(obj, item) => ((obj[`${item.playerName} lost`] = item.playedInLostGame), obj),
				{}
			)

			return {
				name: c.characterName,
				...wonPlayed,
				...lostPlayed,
			}
		})

		return data
	}

	function mapSuccessfulHitsData(characters) {
		const data = characters.map((c) => {
			const hits = c.characterAttacks.reduce(
				(obj, item) => ((obj[`${item.playerName} hit`] = item.successfulAttacks), obj),
				{}
			)

			const misses = c.characterAttacks.reduce(
				(obj, item) => ((obj[`${item.playerName} miss`] = item.unsuccessfulAttacks), obj),
				{}
			)

			return {
				name: c.characterName,
				...hits,
				...misses,
			}
		})

		return data
	}

	function mapGameData(games) {
		const data = games.map((g) => {
			return {
				date: new Date(g.startTime).toLocaleDateString(),
				...g.playerPoints.reduce(
					(obj, item) => ((obj[item.playerName] = item.points), obj),
					{}
				),
			}
		})

		return data
	}

	return (
		<div>
			<h2>Points per Game</h2>
			<LineChart
				width={1000}
				height={300}
				data={mapGameData(state.statistics?.statisticsGames ?? [])}
				margin={{
					top: 5,
					right: 30,
					left: 20,
					bottom: 5,
				}}
			>
				<CartesianGrid strokeDasharray="3 3" />
				<XAxis dataKey="date" />
				<YAxis />
				<Tooltip />
				<Legend />
				<Line type="monotone" dataKey="Nadina" stroke="#2803fc" />
				<Line type="monotone" dataKey="Pascal" stroke="#9403fc" />
			</LineChart>
			<h2>Character Distribution</h2>
			<BarChart
				width={1000}
				height={300}
				data={mapCharacterDistributionData(state.statistics?.statisticsCharacters ?? [])}
				margin={{
					top: 20,
					right: 30,
					left: 20,
					bottom: 5,
				}}
			>
				<CartesianGrid strokeDasharray="3 3" />
				<XAxis dataKey="name" />
				<YAxis />
				<Tooltip />
				<Legend />
				<Bar dataKey="Nadina won" stackId="b" fill="#2803fc" />
				<Bar dataKey="Nadina lost" stackId="b" fill="#036bfc" />
				<Bar dataKey="Pascal won" stackId="a" fill="#9403fc" />
				<Bar dataKey="Pascal lost" stackId="a" fill="#e703fc" />
			</BarChart>
			<h2>Successful Hits</h2>
			<BarChart
				width={1000}
				height={300}
				data={mapSuccessfulHitsData(state.statistics?.statisticsCharacters ?? [])}
				margin={{
					top: 20,
					right: 30,
					left: 20,
					bottom: 5,
				}}
			>
				<CartesianGrid strokeDasharray="3 3" />
				<XAxis dataKey="name" />
				<YAxis />
				<Tooltip />
				<Legend />
				<Bar dataKey="Nadina hit" stackId="b" fill="#2803fc" />
				<Bar dataKey="Nadina miss" stackId="b" fill="#036bfc" />
				<Bar dataKey="Pascal hit" stackId="a" fill="#9403fc" />
				<Bar dataKey="Pascal miss" stackId="a" fill="#e703fc" />
			</BarChart>
		</div>
	)
}
