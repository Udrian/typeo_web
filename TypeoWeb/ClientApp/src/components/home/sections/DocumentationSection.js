import React, { Component } from 'react';
import { DocumentButton } from '../DocumentButton';

export class DocumentationSection extends Component {
    static displayName = DocumentationSection.name;

    constructor(props) {
        super(props);
    }

    render() {
        return (
            <>
                <h2>Documentation for the latest version can be found through the button below</h2>
                <div className="buttonBar">
                    <DocumentButton />
                </div>
            </>
        );
    }
}