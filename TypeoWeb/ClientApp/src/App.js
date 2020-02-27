import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { Home } from './components/home/Home';
import { Document } from './components/document/Document';
import { Download } from './components/download/Download';

import './custom.css'

export default class App extends Component {
    static displayName = App.name;

    render () {
        return (
            <Layout>
                <Route exact path='/' component={Home} />
                <Route exact path='/doc' component={Document} />
                <Route exact path='/download' component={Download} />
            </Layout>
        );
    }
}
