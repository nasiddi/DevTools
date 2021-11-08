import React, { Component } from 'react'
import { Container } from 'reactstrap'
import { NavMenu } from './NavMenu'

const urlsToHideTheMenu = ['/kids-number', '/number']

export const DefaultLayout = (props) => {
	return (
		<div>
			{/* eslint-disable-next-line no-restricted-globals */}
			{urlsToHideTheMenu.includes(location.pathname) ? null : <NavMenu />}
			<Container>{props.children}</Container>
		</div>
	)
}

export const NumberDisplayLayout = (props) => {
	return (
		<div>
			<Container
				fluid={true}
				style={{ textAlignVertical: 'bottom', backgroundColor: 'red' }}
			>
				{props.children}
			</Container>
		</div>
	)
}
