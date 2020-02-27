import React, { Component } from 'react';

export class Section extends Component {
    static displayName = Section.name;

    constructor(props) {
        super(props);
        this.myRef = React.createRef();
    }

    render() {
        let className = this.props.className;
        className += " section";

        return (
            <div className={className} ref={this.myRef}>
                {this.props.children}
            </div>
        );
    }

    scrollTo() {
        window.scrollTo(0, this.myRef.current.offsetTop);
    }
}
