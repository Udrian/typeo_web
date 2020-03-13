import React, { Component } from 'react';
import { Container } from 'reactstrap';
import { Section } from '../home/Section';
import { DownloadButton } from '../home/DownloadButton';
import { DocumentButton } from '../home/DocumentButton';
import { BackNavigation } from '../home/BackNavigation';

export class DownloadPage extends Component {
    static displayName = DownloadPage.name;

    constructor(props) {
        super(props);
    }

    render() {
        return (
            <>
                <div className="downloadPage">
                    <BackNavigation to='/' text='Start' />

                    <Container>
                        <h1>Samples</h1>
                        <h2>Particles</h2>
                        <div className="buttonBar">
                            <DownloadButton/>
                        </div>
                        <h2>Breakout</h2>
                        <div className="buttonBar">
                            <DownloadButton />
                        </div>
                        <h2>Space invader</h2>
                        <div className="buttonBar">
                            <DownloadButton />
                        </div>
                        <h1>TypeO</h1>
                        <h2>Download the latest version (0.1.0).</h2>
                        <div className="buttonBar">
                            <DownloadButton download="TypeO-0.1.0.0.zip"/>
                            <DocumentButton />
                        </div>
                        <h1>Individual modules</h1>
                        <h2>TypeOCore (0.1.0).</h2>
                        <div className="buttonBar">
                            <DownloadButton />
                            <DocumentButton />
                        </div>
                        <h2>TypeODesktop (0.1.0).</h2>
                        <div className="buttonBar">
                            <DownloadButton />
                            <DocumentButton />
                        </div>
                        <h2>TypeOSDL (0.1.0).</h2>
                        <div className="buttonBar">
                            <DownloadButton />
                            <DocumentButton />
                        </div>
                    </Container>
                </div>
            </>
        );
    }
}
