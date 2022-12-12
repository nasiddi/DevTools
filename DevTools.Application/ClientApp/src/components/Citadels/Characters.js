import React, { useEffect, useState } from 'react'
import Button from '@material-ui/core/Button'
import {
	Table,
	TableBody,
	TableCell,
	TableContainer,
	TableHead,
	TableRow,
} from '@material-ui/core'
import DeleteIcon from '@material-ui/icons/Delete'
import { get } from '../../BackendClient'

export const Characters = () => {
	const [state, setState] = useState({})

	const loadData = async () => {
		await get('citadels/characters')
	}

	// useEffect(() => {
	// 	loadData().then()
	// }, [])

	return <DeleteIcon onClick={loadData} />
}
