import React, { Component } from 'react';

export class DownloadButton extends Component {
    static displayName = DownloadButton.name;

    render() {
        return (
            <div id="downloadButton" onClick={() => window.open('/download', "_self")}><h3>DOWNLOAD</h3></div>
        );
    }
}
