import React, { Component } from 'react';
import { DownloadButton } from '../DownloadButton';
import { DocumentButton } from '../DocumentButton';

export class StartSection extends Component {
    static displayName = StartSection.name;

    constructor(props) {
        super(props);
    }

    render() {
        return (
            <>
                <h1>TypeO</h1>
                <h2>A modular .net Core game engine</h2>
                <div className="buttonBar">
                    <DownloadButton/>
                    <DocumentButton/>
                </div>
            </>
        );
    }
}