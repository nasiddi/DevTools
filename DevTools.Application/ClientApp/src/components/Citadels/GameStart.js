/* eslint-disable react/jsx-key */
import React, { useEffect, useState } from 'react'
import Button from '@material-ui/core/Button'
import { Chip, Grid, TextField } from '@material-ui/core'
import { get, post } from '../../BackendClient'
import { Autocomplete } from '@material-ui/lab'

export const GameStart = () => {
	const [state, setState] = useState({
		players: [],
		selectedPlayers: [],
	})

	const loadData = async () => {
		const playerResponse = await get('citadels/players/all')
		const players = await playerResponse.json()

		setState({
			...state,
			players: players,
			selectedPlayers: players.filter((p) => p.isActive),
		})
	}

	useEffect(() => {
		loadData().then()
	}, [])

	function onPlayersChange(event) {
		if (event.currentTarget?.title === 'Clear') {
			state.players.forEach((p) => {
				p.isActive = false
			})

			setState({
				...state,
				selectedPlayers: [],
				players: [...state.players],
			})

			return
		}

		const value = event.target.value
		if (value?.length > 0) {
			if (state.selectedPlayers.some((p) => p.name === value)) {
				return
			}

			setState({
				...state,
				selectedPlayers: [
					...state.selectedPlayers,
					{ id: 0, name: value, isActive: true },
				],
			})

			return
		}

		const name = event.currentTarget?.childNodes[0]?.data
		if (name?.length > 0) {
			state.players.forEach((p) => {
				if (p.name === name) {
					p.isActive = true
				}
			})
			setState({
				...state,
				selectedPlayers: [
					...state.selectedPlayers,
					state.players.find(
						(p) => p.name === event.currentTarget.childNodes[0].data
					),
				],
				players: [...state.players],
			})
		}
	}

	function onRemovePlayer(event) {
		const id = event.currentTarget?.parentElement?.id

		const existingPlayer = state.players.find((p) => p.name === id)
		if (existingPlayer) {
			existingPlayer.isActive = false
		}

		if (state.selectedPlayers.some((p) => p.name === id)) {
			setState({
				...state,
				selectedPlayers: state.selectedPlayers.filter(
					(p) => p.name !== id
				),
				players: [...state.players],
			})
		}
	}

	async function startGame() {
		await post('citadels/game/start', [
			...state.selectedPlayers.filter((p) => p.id === 0),
			...state.players,
		])

		window.location.reload()
	}

	return (
		<Grid
			container
			direction="row"
			justifyContent="space-between"
			alignItems="flex-start"
			spacing={3}
		>
			<Grid item xs={12}>
				<Button
					fullWidth
					color="primary"
					variant="contained"
					onClick={startGame}
				>
					Start Game
				</Button>
			</Grid>
			<Grid item xs={12}>
				<Autocomplete
					multiple
					options={state.players
						.filter((p) => !p.isActive)
						.map((option) => option.name)}
					freeSolo
					onChange={onPlayersChange}
					value={state.selectedPlayers}
					renderTags={(value) =>
						value.map((option) => (
							<Chip
								style={{ marginRight: '5px' }}
								key={option.name}
								label={option.name}
								onDelete={onRemovePlayer}
								id={option.name}
								color={'primary'}
							/>
						))
					}
					renderInput={(params) => (
						<TextField
							{...params}
							variant="outlined"
							label="Players"
							placeholder="Type to add new Players"
						/>
					)}
				/>
			</Grid>
		</Grid>
	)
}
