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
                <Section className='downloadSection'>
                    <BackNavigation to='/' text='Start' />

                    <Container>
                        <h2>Download the latest version (0.1.0).</h2>
                        <div className="buttonBar">
                            <DownloadButton />
                            <DocumentButton />
                        </div>
                    </Container>
                </Section>
            </>
        );
    }
}
