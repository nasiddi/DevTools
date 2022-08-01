import React, { useEffect, useState } from 'react'
import { useParams } from 'react-router'
import { get } from '../BackendClient'
import { Button, Grid } from '@material-ui/core'
import { humanFileSize } from '../Utlis/Common'

export const FileDownload = () => {
	const [state, setState] = useState({
		metaInfo: {
			fileName: '',
			size: 0,
		},
	})

	const params = useParams()

	const loadData = async () => {
		const response = await get(`file/${params.id}/meta-info`)
		const info = await response.json()
		setState({ ...state, metaInfo: info })
	}

	useEffect(() => {
		loadData().then()
	}, [])

	return (
		<Grid
			container
			spacing={2}
			direction="column"
			justifyContent="center"
			alignItems="center"
		>
			<Grid item xs={12}>
				<span>{state.metaInfo.filename}</span>
			</Grid>
			<Grid item xs={12}>
				<span>{humanFileSize(state.metaInfo.size)}</span>
			</Grid>
			<Grid item xs={12}>
				<Button
					variant={'contained'}
					color={'primary'}
					href={`/file/${params.id}`}
					download
					disabled={state.metaInfo.size === 0}
				>
					Download
				</Button>
			</Grid>
		</Grid>
	)
}
