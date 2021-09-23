import React, { Component } from 'react';
import { Route } from 'react-router';
import { Home } from './components/Home';
import { Deploy } from './components/Deploy';
import AuthorizeRoute from './components/api-authorization/AuthorizeRoute';
import ApiAuthorizationRoutes from './components/api-authorization/ApiAuthorizationRoutes';
import { ApplicationPaths } from './components/api-authorization/ApiAuthorizationConstants';

import './custom.css'
import {HueColors} from "./components/HueColors";
import {DisplayNumber, EnterNumber} from "./components/NumberDisplay";
import {DefaultLayout, NumberDisplayLayout} from "./components/Layout";

export default class App extends Component {
  static displayName = App.name;



    render () {
    return (
        <div>
          <DefaultLayout>
            <Route exact path='/' component={Home} />
            <AuthorizeRoute path='/deploy-spa' component={Deploy} />
            <AuthorizeRoute path='/hue-colors' component={HueColors} />
              <Route path='/number' component={EnterNumber}/>
              <Route path={ApplicationPaths.ApiAuthorizationPrefix} component={ApiAuthorizationRoutes} />
          </DefaultLayout>
          <NumberDisplayLayout>
              <Route path='/kids-number' component={DisplayNumber}/>
              {/*<Route exact path="/kids-number" render={() => {window.location.href="number.html"}} />*/}

          </NumberDisplayLayout>
        </div>
    );
  }
}
