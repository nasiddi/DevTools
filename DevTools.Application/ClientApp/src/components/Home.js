import React from 'react'
import { Container, Jumbotron } from 'reactstrap'
import Login from './Login'

export function Home() {
	return (
		<div>
			<Jumbotron fluid>
				<Container fluid>
					<h1 className="display-3">DevTools</h1>
				</Container>
			</Jumbotron>
			<Login />
		</div>
	)
}
