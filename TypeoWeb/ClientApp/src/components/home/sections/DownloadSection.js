import React, { Component } from 'react';
import { DownloadButton } from '../DownloadButton';
import { DocumentButton } from '../DocumentButton';

export class DownloadSection extends Component {
    static displayName = DownloadSection.name;

    constructor(props) {
        super(props);
    }

    render() {
        return (
            <>
                <h2>Download the latest version (0.1.0).</h2>
                <div className="buttonBar">
                    <DownloadButton/>
                    <DocumentButton/>
                </div>
            </>
        );
    }
}