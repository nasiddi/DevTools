import React, { useEffect, useState } from 'react'
import { get, post } from '../BackendClient'
import Button from '@material-ui/core/Button'
import {
	Table,
	TableBody,
	TableCell,
	TableContainer,
	TableHead,
	TableRow,
} from '@material-ui/core'
import { humanFileSize } from '../Utlis/Common'
import DeleteIcon from '@material-ui/icons/Delete'
import GetAppIcon from '@material-ui/icons/GetApp'

export const Files = () => {
	const [state, setState] = useState({
		files: [],
	})

	const loadData = async () => {
		const response = await get('file/meta-info')
		const files = await response.json()
		setState({ ...state, files: files })
	}

	useEffect(() => {
		loadData().then()
	}, [])

	const removeFile = async (e) => {
		await post(`file/${e.currentTarget.id}/remove`)
		await loadData()
	}

	return (
		<TableContainer>
			<Table sx={{ minWidth: 650 }} size="small">
				<TableHead>
					<TableRow>
						<TableCell>Name</TableCell>
						<TableCell align="right">Size</TableCell>
						<TableCell align="center">Download</TableCell>
						<TableCell align="center">Delete</TableCell>
					</TableRow>
				</TableHead>
				<TableBody>
					{state.files.map((f) => (
						<TableRow
							key={f.guid}
							sx={{
								'&:last-child td, &:last-child th': {
									border: 0,
								},
							}}
						>
							<TableCell component="th" scope="row">
								{f.filename}
							</TableCell>
							<TableCell align="right">
								{humanFileSize(f.size)}
							</TableCell>
							<TableCell align="center">
								<Button>
									<a href={`/file/${f.guid}`}>
										<GetAppIcon color={'primary'} />
									</a>
								</Button>
							</TableCell>
							<TableCell align="center">
								<Button>
									<DeleteIcon
										color={'error'}
										id={f.guid}
										onClick={removeFile}
									/>
								</Button>
							</TableCell>
						</TableRow>
					))}
				</TableBody>
			</Table>
		</TableContainer>
	)
}
