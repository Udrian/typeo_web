import React, { Component } from 'react';
import { Container } from 'reactstrap';
import { Section } from '../home/Section';
import { BackNavigation } from '../home/BackNavigation';

export class DocumentPage extends Component {
    static displayName = DocumentPage.name;

    constructor(props) {
        super(props);
    }

    render() {
        return (
            <>
                <Section className='documentationSection'>
                    <BackNavigation to='/' text='Start' />

                    <Container>
                        <h2 style={{ marginBottom: '20vw' }}>Documentation not yet available</h2>
                    </Container>
                </Section>
            </>
        );
    }
}
