import React, { Component } from 'react';

import './navigation.css'

export class UpNavigation extends Component {
    static displayName = UpNavigation.name;

    render() {
        return (
            <div className='upNavigation' onClick={this.props.onClick}>
                <svg width="10vw" height="5vw" viewBox="0 0 256 97">
                    <path className="scrollButton" d="M122.564,4.119a9,9,0,0,1,10.872,0L234.658,80.827A9,9,0,0,1,229.222,97H26.778a9,9,0,0,1-5.436-16.173Z" />
                </svg>
                <h2>{this.props.text}</h2>
            </div>
        );
    }
}
