import React, { Component } from 'react';
import { Link } from "react-router-dom";

export class DownloadButton extends Component {
    static displayName = DownloadButton.name;

    render() {
        let link;
        if (!this.props.download) {
            link = <Link id="downloadButton" to="/download"><h3>DOWNLOAD</h3></Link>;
        } else {
            link = <Link id="downloadButton" to={"/download/" + this.props.download} target="_blank" download><h3>DOWNLOAD</h3></Link>;
        }

        return (
            <>
                {link}
            </>
        );
    }
}
