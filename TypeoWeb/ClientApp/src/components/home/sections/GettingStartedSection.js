import React, { Component } from 'react';

export class GettingStartedSection extends Component {
    static displayName = GettingStartedSection.name;

    constructor(props) {
        super(props);
    }

    render() {
        return (
            <>
                <h1>GETTING STARTED</h1>
                <div className="buttonBar">
                    <h2 id="step1">Step 1: Download</h2>
                    <h2 id="step2">Step 2: Install</h2>
                </div>
            </>
        );
    }
}