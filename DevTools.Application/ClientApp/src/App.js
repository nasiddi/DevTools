import React from 'react'
import { Route, Routes } from 'react-router-dom'
import { CleanLayout, DefaultLayout } from './components/Layout'
import { Home } from './components/Home'

import './custom.css'
import { Deploy } from './components/Deploy'
import { HueColors } from './components/HueColors'
import AuthRoute from './auth/AuthRoute'
import { FileUpload } from './components/FileUpload'
import { FileDownload } from './components/FileDownload'
import { Files } from './components/Files'
import { Characters } from './components/Citadels/Characters'
import { Game } from './components/Citadels/Game'
import { Statistics } from './components/Citadels/Statistics'

export default function App() {
	return (
		<>
			<DefaultLayout>
				<Routes>
					<Route exact path="/" element={<Home />} />
					<Route exact path="/deploy-spa" element={<AuthRoute />}>
						<Route exact path="/deploy-spa" element={<Deploy />} />
					</Route>
					<Route exact path="/hue-colors" element={<AuthRoute />}>
						<Route
							exact
							path="/hue-colors"
							element={<HueColors />}
						/>
					</Route>
					<Route exact path="/file-upload" element={<AuthRoute />}>
						<Route
							exact
							path="/file-upload"
							element={<FileUpload />}
						/>
					</Route>
					<Route exact path="/files" element={<AuthRoute />}>
						<Route exact path="/files" element={<Files />} />
					</Route>
					<Route exact path="/citadels/game" element={<AuthRoute />}>
						<Route exact path="/citadels/game" element={<Game />} />
					</Route>
					<Route
						exact
						path="/citadels/statistics"
						element={<AuthRoute />}
					>
						<Route
							exact
							path="/citadels/statistics"
							element={<Statistics />}
						/>
					</Route>
					<Route
						exact
						path="/citadels/character-configuration"
						element={<AuthRoute />}
					>
						<Route
							exact
							path="/citadels/character-configuration"
							element={<Characters />}
						/>
					</Route>
				</Routes>
			</DefaultLayout>
			<CleanLayout>
				<Routes>
					<Route
						exact
						path="/file-download/:id"
						element={<FileDownload />}
					/>
				</Routes>
			</CleanLayout>
		</>
	)
}
