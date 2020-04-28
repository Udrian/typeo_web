import React, { Component } from 'react';
import { Container } from 'reactstrap';
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
                            <DownloadButton download="^samples/Particles.zip"/>
                        </div>
                        <h2>Breakout</h2>
                        <div className="buttonBar">
                            <DownloadButton download="^samples/Breakout.zip"/>
                        </div>
                        <h2>Space invader</h2>
                        <div className="buttonBar">
                            <DownloadButton download="^samples/SpaceInvader.zip"/>
                        </div>
                        <h1>TypeO</h1>
                        <h2>Download the latest version (0.1.1)</h2>
                        <div className="buttonBar">
                            <DownloadButton download="^TypeO-"/>
                            <DocumentButton />
                        </div>
                        <h2>Download the latest development build (0.1.1)</h2>
                        <div className="buttonBar">
                            <DownloadButton download="^dev/TypeO-" />
                            <DocumentButton />
                        </div>
                        <h1>Individual modules</h1>
                        <h2>TypeOCore (0.1.1)</h2>
                        <div className="buttonBar">
                            <DownloadButton download="^TypeOCore-" />
                            <DocumentButton />
                        </div>
                        <h2>TypeODesktop (0.1.1)</h2>
                        <div className="buttonBar">
                            <DownloadButton download="^TypeODesktop-" />
                            <DocumentButton />
                        </div>
                        <h2>TypeOSDL (0.1.1)</h2>
                        <div className="buttonBar">
                            <DownloadButton download="^TypeOSDL-" />
                            <DocumentButton />
                        </div>
                    </Container>
                </div>
            </>
        );
    }
}
