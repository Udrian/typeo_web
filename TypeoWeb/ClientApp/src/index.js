import 'bootstrap/dist/css/bootstrap.css';
import React from 'react';
import ReactDOM from 'react-dom';
import { Router } from 'react-router-dom';
import ReactGA from 'react-ga';
import { createBrowserHistory } from 'history';
import App from './App';
import registerServiceWorker from './registerServiceWorker';

const baseUrl = document.getElementsByTagName('base')[0].getAttribute('href');
const rootElement = document.getElementById('root');

const trackingId = process.env.REACT_APP_TRACKING_ID;
ReactGA.initialize(trackingId);

const history = createBrowserHistory();

history.listen(location => {
    ReactGA.set({ page: location.pathname });
    ReactGA.pageview(location.pathname);
});

ReactDOM.render(
    <Router history={history} basename={baseUrl}>
        <App />
    </Router>,
  rootElement);

registerServiceWorker();

