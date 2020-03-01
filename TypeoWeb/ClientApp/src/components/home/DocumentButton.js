import React, { Component } from 'react';
import { Link } from "react-router-dom";

export class DocumentButton extends Component {
    static displayName = DocumentButton.name;

    render() {
        return (
            <Link id="documentationButton" to="/doc"><h3>Documentation</h3></Link>
        );
    }
}
