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
            link = <Link id="downloadButton" to={"/download/" + this.props.download + (this.props.path ? "?path=" + this.props.path : "")} onClick={() => this.sendDownloadAnalytics((this.props.path ? this.props.path + "/" : "") + this.props.download)} target="_blank" download><h3>DOWNLOAD</h3></Link>;
        }

        return (
            <>
                {link}
            </>
        );
    }
}
