/* eslint-disable react/react-in-jsx-scope */
import { Container } from 'reactstrap'
import { NavMenu } from './NavMenu'

const urlsToHideTheMenu = ['/file-download', 'list-converter', 'quiz']

export const DefaultLayout = (props) => {
	return (
		<div>
			{/* eslint-disable-next-line no-restricted-globals */}
			{urlsToHideTheMenu.some((urlToHideTheMenu) =>
				location.pathname.includes(urlToHideTheMenu)
			) ? null : (
				<NavMenu />
			)}
			<Container>{props.children}</Container>
		</div>
	)
}

export const CleanLayout = (props) => {
	return (
		<div>
			<Container
				style={{
					display: 'flex',
					justifyContent: 'center',
					alignItems: 'center',
				}}
			>
				{props.children}
			</Container>
		</div>
	)
}
