import React, { useEffect, useState } from 'react'
import DeleteIcon from '@mui/icons-material/Delete'
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
