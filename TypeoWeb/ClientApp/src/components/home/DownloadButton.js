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

    constructor(props) {
        super(props);
        this.state = { downloadLink: ""};
    }

    componentDidMount() {
        this.fetchLatestDownload(this.props.download);
    }

    render() {
        let link;
        if (!this.props.download) {
            link = <Link id="downloadButton" to="/download"><h3>DOWNLOAD</h3></Link>;
        } else {
            if (this.state.downloadLink) {
                link = <Link id="downloadButton" to={"/download/" + this.state.downloadLink} onClick={() => this.sendDownloadAnalytics(this.state.downloadLink)} target="_blank" download><h3>DOWNLOAD</h3></Link>;
            }
        }

        return (
            <>
                {link}
            </>
        );
    }

    async fetchLatestDownload(downloadPath) {
        const response = await fetch("download/latest?query=" + downloadPath);
        const data = await response.text();
        this.setState({ downloadLink: data });
    }
}
