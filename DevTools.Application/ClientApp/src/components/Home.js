import React from 'react'
import Login from './Login'
import { Container } from 'reactstrap'

export function Home() {
	return (
		<div>
			<Container fluid>
				<h1 className="display-3">DevTools</h1>
			</Container>
			<Login />
		</div>
	)
}
