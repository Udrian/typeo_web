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
                <h2>Documentation comming soon...</h2>
                <div className="buttonBar">
                    <DocumentButton />
                </div>
            </>
        );
    }
}