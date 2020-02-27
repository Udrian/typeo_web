import React, { Component } from 'react';

export class DocumentButton extends Component {
    static displayName = DocumentButton.name;

    render() {
        return (
            <div id="documentationButton" onClick={() => window.open('/doc', "_self")}><h3>Documentation</h3></div>
        );
    }
}
