import React, { Component } from 'react';

export class StartSection extends Component {
    static displayName = StartSection.name;

    constructor(props) {
        super(props);
    }

    render() {
        return (
            <>
                <h1>TypeO</h1>
                <h2>The best open source game engine. Ever.</h2>
                <h2>Or something else provocative.</h2>
                <div className="buttonBar">
                    <div id="downloadButton"><h3>DOWNLOAD</h3></div>
                    <div id="documentationButton"><h3>Documentation</h3></div>
                </div>
            </>
        );
    }
}