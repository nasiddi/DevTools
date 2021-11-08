import React from 'react'
import { Route, Routes } from 'react-router-dom'
import { DefaultLayout } from './components/Layout'
import { Home } from './components/Home'

import './custom.css'
import { Deploy } from './components/Deploy'
import { HueColors } from './components/HueColors'
import AuthRoute from './auth/AuthRoute'

export default function App() {
	return (
		<DefaultLayout>
			<Routes>
				<Route exact path="/" element={<Home />} />
				<Route exact path="/deploy-spa" element={<AuthRoute />}>
					<Route exact path="/deploy-spa" element={<Deploy />} />
				</Route>
				<Route exact path="/hue-colors" element={<AuthRoute />}>
					<Route exact path="/hue-colors" element={<HueColors />} />
				</Route>
			</Routes>
		</DefaultLayout>
	)
}
