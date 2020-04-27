import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { HomePage } from './components/home/HomePage';
import { DocumentPage } from './components/document/DocumentPage';
import { DownloadPage } from './components/download/DownloadPage';
import { ReleaseNotesPage } from './components/ReleaseNotesPage';

import './custom.css'

export default class App extends Component {
    static displayName = App.name;

    render () {
        return (
            <Layout>
                <Route exact path='/' component={HomePage} />
                <Route exact path='/doc' component={DocumentPage} />
                <Route exact path='/download' component={DownloadPage} />
                <Route exact path='/releasenotes' component={ReleaseNotesPage} />
            </Layout>
        );
    }
}
