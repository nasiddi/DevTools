/* eslint-disable react/jsx-key */
import React, { useEffect, useState } from 'react'
import { get, post } from '../../BackendClient'
import { GameStart } from './GameStart'
import {
	Box,
	Button,
	ButtonGroup,
	Dialog,
	DialogActions,
	DialogContent,
	DialogTitle,
	Grid,
	TextField,
} from '@material-ui/core'
import ArrowForwardIcon from '@material-ui/icons/ArrowForward'
import EmojiEventsIcon from '@material-ui/icons/EmojiEvents'
import EmojiEventsOutlinedIcon from '@material-ui/icons/EmojiEventsOutlined'

export const Game = () => {
	const [state, setState] = useState({
		characters: [],
		allCharacters: [],
		players: [],
		game: null,
		turns: [],
		overlayIsOpen: false,
	})

	const overlay = React.useRef(null)

	const handleEntering = () => {
		if (overlay.current !== null) {
			overlay.current.focus()
		}
	}

	function handleCancel() {
		overlay.current = null

		state.players.forEach((p) => (p.hasWon = false))

		setState({
			...state,
			overlayIsOpen: false,
			players: [...state.players],
		})
	}

	const loadData = async () => {
		const gameResponse = await get('citadels/game/active')

		if (gameResponse.status === 204) {
			return
		}

		const response = await get('citadels/characters/active')
		const characterResponse = await get('citadels/characters/all')
		const playerResponse = await get('citadels/players/active')
		const players = await playerResponse.json()

		setState({
			...state,
			allCharacters: await characterResponse.json(),
			characters: await response.json(),
			players: players,
			game: await gameResponse.json(),
		})
	}

	useEffect(() => {
		loadData().then()
	}, [])

	if (state.game === null) {
		return <GameStart />
	}

	function selectPlayer(event) {
		const characterNumber = parseInt(event.currentTarget.name, 10)
		const playerId = parseInt(event.currentTarget.id, 10)
		console.log(playerId, characterNumber)
		const turn = state.turns.find(
			(t) => t.characterNumber === characterNumber
		)

		if (turn) {
			if (playerId === turn.playerId) {
				setState({
					...state,
					turns: [
						...state.turns.filter(
							(t) =>
								t.playerId !== playerId ||
								t.characterNumber !== characterNumber
						),
					],
				})

				return
			}

			turn.playerId = playerId
			setState({ ...state, turns: [...state.turns] })

			return
		}

		setState({
			...state,
			turns: [
				...state.turns,
				{
					playerId: playerId,
					characterNumber: characterNumber,
					handId: 0,
					hand: {},
					player: {},
				},
			],
		})
	}

	function onSelectTarget(event) {
		const turnCharacterNumber = parseInt(event.currentTarget.name, 10)
		const targetCharacterNumber = parseInt(event.currentTarget.id, 10)

		const turn = state.turns.find(
			(t) => t.characterNumber === turnCharacterNumber
		)

		if (!turn) {
			return
		}

		if (turn.targetCharacterNumber === targetCharacterNumber) {
			turn.targetCharacterNumber = null
		} else {
			turn.targetCharacterNumber = targetCharacterNumber
		}

		setState({
			...state,
			turns: [...state.turns],
		})
	}

	function RenderTurn(character) {
		function renderPlayers(p) {
			const turn = state.turns.find(
				(t) => t.characterNumber === character.characterNumber
			)

			return (
				<Button
					color={'primary'}
					variant={turn?.playerId === p.id ? 'contained' : 'outlined'}
					id={p.id}
					name={character.characterNumber}
					onClick={selectPlayer}
					key={p.id}
				>
					{p.name}
				</Button>
			)
		}

		function renderTargets(characterType, characterNumber) {
			let targets = []
			if (characterType === 'Assassin' || characterType === 'Witch') {
				targets = [2, 3, 4, 5, 6, 7, 8]
			}

			if (characterType === 'Thief') {
				targets = [3, 4, 5, 6, 7, 8]

				if (
					state.characters.some((c) => c.characterType === 'Assassin')
				) {
					const assassinTurn = state.turns.find(
						(t) =>
							t.characterNumber === 1 && t.targetCharacterNumber
					)

					if (assassinTurn) {
						targets = targets.filter(
							(t) => t !== assassinTurn.targetCharacterNumber
						)
					}
				}
			}

			if (characterType === 'Magician' || characterType === 'Wizard') {
				targets = [1, 2, 4, 5, 6, 7, 8]
			}

			if (characterType === 'Warlord') {
				targets = [1, 2, 3, 4, 6, 7]
			}

			if (characterType === 'Diplomat') {
				targets = [1, 2, 3, 4, 5, 6, 7]
			}

			const buttonGroups = []

			const turn = state.turns.find(
				(t) => t.characterNumber === characterNumber
			)

			if (targets.length === 0) {
				return
			}

			buttonGroups.push(
				<Grid item>
					<ButtonGroup>
						{targets.map((c) => (
							<Button
								id={c}
								name={characterNumber}
								onClick={onSelectTarget}
								color={'primary'}
								variant={
									turn?.targetCharacterNumber === c
										? 'contained'
										: 'outlined'
								}
								key={c}
							>
								{c}
							</Button>
						))}
					</ButtonGroup>
				</Grid>
			)

			return [
				<Grid item>
					<h4>
						<ArrowForwardIcon />
					</h4>
				</Grid>,
				...buttonGroups,
			]
		}

		return (
			<Grid item xs={12} key={character.characterNumber}>
				<Box
					sx={{
						padding: '13px',
						borderColor: 'lightgray',
						borderBottomColor: 'lightgray',
						borderBottom: '1px solid',
						...(character.characterNumber === 1 && {
							borderTop: '1px solid',
							borderTopColor: 'lightgray',
						}),
					}}
				>
					<Grid
						container
						direction="row"
						justifyContent="space-between"
						alignItems="flex-start"
						spacing={1}
					>
						<Grid item xs={1}>
							<h3>{character.characterNumber}</h3>
						</Grid>
						<Grid item xs={5}>
							<h3>{character.characterType}</h3>
						</Grid>
						<Grid item xs={6}>
							<ButtonGroup
								variant="outlined"
								aria-label="outlined button group"
							>
								{state.players.map((p) => renderPlayers(p))}
							</ButtonGroup>
						</Grid>

						{renderTargets(
							character.characterType,
							character.characterNumber
						)}
					</Grid>
				</Box>
			</Grid>
		)
	}

	async function onSubmitTurns() {
		const response = await post('citadels/turns/submit', state.turns)

		if (response.status === 200) {
			setState({ ...state, turns: [] })
		}
	}

	function onEndGame() {
		setState({ ...state, overlayIsOpen: true })
	}

	function onSelectWinner(event) {
		const playerId = parseInt(event.currentTarget.id, 10)
		const player = state.players.find((p) => p.id === playerId)
		player.hasWon = true

		setState({ ...state, players: [...state.players] })
	}

	function onDeselectWinner(event) {
		const playerId = parseInt(event.currentTarget.id, 10)
		const player = state.players.find((p) => p.id === playerId)
		player.hasWon = false

		setState({ ...state, players: [...state.players] })
	}

	async function onScoreOk() {
		const response = await post(
			'citadels/game/end',
			state.players.map((p) => ({
				playerId: p.id,
				points: p.points,
				hasWon: p.hasWon,
			}))
		)

		if (response.status === 200) {
			window.location.reload()
		}
	}

	function onScoreChange(event) {
		const playerId = parseInt(event.currentTarget.id, 10)
		const player = state.players.find((p) => p.id === playerId)
		player.points = parseInt(event.currentTarget.value, 10)

		setState({ ...state, players: [...state.players] })
	}

	return (
		<>
			<Grid
				container
				direction="row"
				justifyContent="space-between"
				alignItems="flex-start"
				spacing={0}
			>
				{state.characters
					.sort(function (a, b) {
						return a.characterNumber - b.characterNumber
					})
					.map((c) => RenderTurn(c))}
				<Grid item xs={12} style={{ paddingTop: '13px' }}>
					<Button
						fullWidth
						variant={'contained'}
						color={'primary'}
						onClick={onSubmitTurns}
					>
						Submit Turns
					</Button>
				</Grid>
				<Grid item xs={12} style={{ paddingTop: '13px' }}>
					<Button
						fullWidth
						variant={'contained'}
						color={'secondary'}
						onClick={onEndGame}
					>
						End Game
					</Button>
				</Grid>
			</Grid>
			<Dialog
				maxWidth="xs"
				onEntering={handleEntering}
				aria-labelledby="confirmation-dialog-title"
				open={state.overlayIsOpen}
			>
				<DialogTitle id="confirmation-dialog-title">
					Enter Scores
				</DialogTitle>
				<DialogContent dividers>
					<Grid
						container
						direction="row"
						justifyContent="space-between"
						alignItems="center"
						spacing={2}
					>
						{state.players.map((p) => (
							<>
								<Grid item xs={8} key={p.id}>
									<TextField
										type="number"
										label={p.name}
										value={p.points ?? ''}
										id={p.id}
										onChange={onScoreChange}
									/>
								</Grid>
								<Grid item xs={4}>
									{p.hasWon ? (
										<EmojiEventsIcon
											color={'primary'}
											onClick={onDeselectWinner}
											id={p.id}
										/>
									) : (
										<EmojiEventsOutlinedIcon
											onClick={onSelectWinner}
											id={p.id}
										/>
									)}
								</Grid>
							</>
						))}
					</Grid>
				</DialogContent>
				<DialogActions>
					<Button autoFocus color="primary" onClick={handleCancel}>
						Cancel
					</Button>
					<Button color="primary" onClick={onScoreOk}>
						Ok
					</Button>
				</DialogActions>
			</Dialog>
		</>
	)
}
