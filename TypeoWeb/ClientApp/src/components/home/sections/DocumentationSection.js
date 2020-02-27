import React, { Component } from 'react';

export class DocumentationSection extends Component {
    static displayName = DocumentationSection.name;

    constructor(props) {
        super(props);
    }

    render() {
        return (
            <>
                <h2 style={{ marginBottom: '20vw' }}>Up to date documentation in some sort of shape or form.</h2>
            </>
        );
    }
}