import React, { Component } from 'react';
import { Container } from 'reactstrap';
import { NavMenu } from './NavMenu';

export const DefaultLayout = (props) => {
    return (
        <div>
            {/* eslint-disable-next-line no-restricted-globals */}
            {location.pathname === '/kids-number' ? null : <NavMenu />}
            <Container>{props.children}</Container>
        </div>
    )
}

export const NumberLayout = (props) => {
    return (
        <div>
            <Container fluid={true} style={{textAlignVertical: 'bottom', backgroundColor: 'red'}}>
                {props.children}
            </Container>
        </div>
    )
}