import React, { Component } from 'react';
import { UpNavigation } from './UpNavigation';
import { DownNavigation } from './DownNavigation';
import { Section } from './Section';
import { Container } from 'reactstrap';

import { StartSection } from './sections/StartSection'
import { GettingStartedSection } from './sections/GettingStartedSection'
import { DownloadSection } from './sections/DownloadSection'
import { DocumentationSection } from './sections/DocumentationSection'

import './homePage.css'

export class HomePage extends Component {
    static displayName = HomePage.name;

    constructor(props) {
        super(props);
        this.StartSection = React.createRef();
        this.GettingStartedSection = React.createRef();
        this.DownloadSection = React.createRef();
        this.DocumentationSection = React.createRef();
    }

    render() {
        return (
            <>
                <Section className='startSection' ref={this.StartSection}>
                    <Container>
                        <StartSection/>
                    </Container>

                    <DownNavigation onClick={() => this.GettingStartedSection.current.scrollTo()} text='GET STARTED' />
                </Section>

                <Section className='gettingStartedSection' ref={this.GettingStartedSection}>
                    <UpNavigation onClick={() => this.StartSection.current.scrollTo()} text='START' />

                    <Container>
                        <GettingStartedSection/>
                    </Container>

                    <DownNavigation onClick={() => this.DownloadSection.current.scrollTo()} text='DOWNLOADS' />
                </Section>

                <Section className='downloadSection' ref={this.DownloadSection}>
                    <UpNavigation onClick={() => this.GettingStartedSection.current.scrollTo()} text='GETTING STARTED' />

                    <Container>
                        <DownloadSection/>
                    </Container>

                    <DownNavigation onClick={() => this.DocumentationSection.current.scrollTo()} text='DOCUMENTATION' />
                </Section>

                <Section className='documentationSection' ref={this.DocumentationSection}>
                    <UpNavigation onClick={() => this.DownloadSection.current.scrollTo()} text='DOWNLOAD' />

                    <Container>
                        <DocumentationSection/>
                    </Container>
                </Section>
            </>
        );
    }
}
