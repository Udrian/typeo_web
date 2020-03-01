import React, { Component } from 'react';
import { Link } from "react-router-dom";

export class DownloadButton extends Component {
    static displayName = DownloadButton.name;

    render() {
        return (
            <Link id="downloadButton" to="/download"><h3>DOWNLOAD</h3></Link>
        );
    }
}
