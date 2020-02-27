import React, { Component } from 'react';

export class DownloadSection extends Component {
    static displayName = DownloadSection.name;

    constructor(props) {
        super(props);
    }

    render() {
        return (
            <>
                <h2>Download the latest version (0.0.1b).</h2>
                <div className="buttonBar">
                    <div id="downloadButton"><h3>DOWNLOAD</h3></div>
                    <div id="documentationButton"><h3>Documentation</h3></div>
                </div>
                <h2>Download latest stable version (0.0.1a)</h2>
                <div className="buttonBar">
                    <div id="downloadButton"><h3>DOWNLOAD</h3></div>
                    <div id="documentationButton"><h3>Documentation</h3></div>
                </div>
            </>
        );
    }
}