/*import React, { Component } from 'react';

export class ReleaseNotesPage extends Component {
    static displayName = ReleaseNotesPage.name;

    constructor(props) {
        super(props);
        this.state = { releasenotes: "" };
    }

    componentDidMount() {
        this.populateReleaseNotes();
    }

    render() {
        return (
            <>
                <div>
                    <div>Testing is here</div>
                </div>
            </>
        );
    }

    async populateReleaseNotes() {
        const response = await fetch('download/releasenotes');
        const data = await response.text + "tesat";
        this.setState({ releasenotes: data });
    }
}



*/
import React, { Component } from 'react';
import { Container } from 'reactstrap';
import { Section } from './home/Section';

export class ReleaseNotesPage extends Component {
    static displayName = ReleaseNotesPage.name;

    constructor(props) {
        super(props);
        this.state = { releasenotes: "Loading..." };
    }

    componentDidMount() {
        this.populateReleaseNotes();
    }

    render() {
        return (
            <>
                <Container>
                    <pre style={{ textAlign: 'left' }}>{this.state.releasenotes}</pre>
                </Container>
            </>
        );
    }

    async populateReleaseNotes() {
        const response = await fetch('download/releasenotes');
        const data = await response.text();
        this.setState({ releasenotes: data });
    }
}
