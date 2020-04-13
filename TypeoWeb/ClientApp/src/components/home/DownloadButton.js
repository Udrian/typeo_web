import React, { Component } from 'react';
import { Link } from "react-router-dom";
import ReactGA from 'react-ga';

export class DownloadButton extends Component {
    static displayName = DownloadButton.name;

    sendDownloadAnalytics(download) {
        ReactGA.event({
            category: "Download",
            action: download
        });
    }

    render() {
        let link;
        if (!this.props.download) {
            link = <Link id="downloadButton" to="/download"><h3>DOWNLOAD</h3></Link>;
        } else {
            link = <Link id="downloadButton" to={"/download/" + this.props.download} onClick={() => this.sendDownloadAnalytics(this.props.download)} target="_blank" download><h3>DOWNLOAD</h3></Link>;
        }

        return (
            <>
                {link}
            </>
        );
    }
}
