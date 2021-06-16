import React, { Component } from 'react';
import authService from './api-authorization/AuthorizeService'
import {Alert, Button, Col, Container, Row, Spinner} from "reactstrap";

export class Deploy extends Component {
  static displayName = Deploy.name;

  constructor(props) {
    super(props);
    this.deploy = this.deploy.bind(this)

    this.state = { commit: {}, loading: true, alert: { text: '', color: 'success', showSpinner: false } };
  }

  componentDidMount() {
    this.getLatestCommit().then();
  }

  renderContent() {
    const commit = this.state.commit;
    return (
        <div>
          <h4>Currently Deployed Commit</h4>
        <Container className={"commit-container"}>
          <Row>
            <Col><b>Hash</b></Col>
            <Col><b>Message</b></Col>
            <Col><b>Date</b></Col>
          </Row>
          <Row>
            <Col>{commit.hash}</Col>
            <Col>{commit.message}</Col>
            <Col>{commit.date}</Col>
          </Row>
          <Row>
            <Col/>
          </Row>
        </Container>
        <Button onClick={this.deploy} color={'primary'} block>
          Deploy
        </Button>
        </div>
    );
  }

  renderAlert() {
    const spinner = (this.state.alert.showSpinner) ? <span><Spinner size="sm" color="info" />{' '}</span> : <div/>
    return (<Alert color={this.state.alert.color}>{spinner}{this.state.alert.text}</Alert>)
  }

  render() {
    const contents = this.state.loading ? (
        <p>
          <em>Loading...</em>
        </p>
    ) : (
        this.renderContent()
    )

    const alert = this.state.alert.text.length === 0 ? (
        <div/>
    ) : (
        this.renderAlert()
    )

    return (
      <div className={"deploy"}>
        {contents}
        {alert}
      </div>
    );
  }

  async getLatestCommit() {
    const token = await authService.getAccessToken();
    const response = await fetch('deploy/commit', {
      headers: !token ? {} : { 'Authorization': `Bearer ${token}` }
    });
    if (response.status === 200){
      const data = await response.json();
      this.setState({ commit: data, loading: false });
      return
    }

    if (response.status === 500){
      response.text().then(t => this.setState({loading: false, alert: {text: `Server Error: ${t}`, color: 'danger'}}))
    }
  }

  async deploy() {
    this.setState({alert: {text: 'Uploading Files', color: 'info', showSpinner: true}})
    const token = await authService.getAccessToken();
    await fetch(
        `deploy/spa`,
        {
          method: 'POST',
          headers: !token ? {} : { 'Authorization': `Bearer ${token}` },
        }
    ).then(r => {
      if (r.status === 200){
        this.setState({alert: {text: 'Deployment completed.', color: 'success'}})
        this.getLatestCommit()
        return
      }

      if (r.status === 401){
        this.setState({alert: {text: 'Unauthorized: Try logging out and logging back in.', color: 'danger'}})
        return
      }

      if (r.status === 500){
        r.text().then(t => this.setState({alert: {text: `Server Error: ${t}`, color: 'danger'}}))
      }
    })
  }
}
