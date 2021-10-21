import React, {useEffect, useState} from 'react';
import authService from './api-authorization/AuthorizeService'
import {Alert, Button, Col, Container, Row, Spinner} from "reactstrap";
import {Checkbox, FormControlLabel} from "@material-ui/core";

export function Deploy(props) {
    const defaultAlert = { text: '', color: 'success', showSpinner: false }
    const [state, setState] = useState({ 
        commit: {}, 
        loading: true, 
        deployOnChange: false,
        alert: defaultAlert
    })

  async function getLatestCommit() {
    const token = await authService.getAccessToken();
    const response = await fetch('deploy/commit', {
      headers: !token ? {} : { 'Authorization': `Bearer ${token}` }
    });
    if (response.status === 200){
      const backgroundTask = await fetch('deploy/background-task', {
          headers: !token ? {} : { 'Authorization': `Bearer ${token}` }
      }) 
      
      if (backgroundTask.status === 200){
          const data = await response.json();
          const status = await backgroundTask.json();

          const alert = status.isRunning ? {text: 'Uploading Files', color: 'info', showSpinner: true} : defaultAlert
          
          setState({
              ...state, 
              alert: status.isEnabled ? alert : state.alert, 
              commit: data, 
              loading: false, 
              deployOnChange: 
              status.isEnabled
          });
          return
      }

      response.text().then(t => setState({...state, loading: false, alert: {text: `Server Error: ${t}`, color: 'danger'}}))
    }
  }

    useEffect(() => {
        const intervalId = setInterval(() => {
            getLatestCommit().then()
        },1000)
        return () => clearInterval(intervalId)
    }, [])

  async function deploy() {
    setState({...state, alert: {text: 'Uploading Files', color: 'info', showSpinner: true}})
    const token = await authService.getAccessToken();
    await fetch(
        `deploy/spa`,
        {
          method: 'POST',
          headers: !token ? {} : { 'Authorization': `Bearer ${token}` },
        }
    ).then(r => {
      if (r.status === 200){
        r.text().then(t => {
          if (t === 'true'){
            setState({...state, alert: {text: 'Deployment completed.', color: 'success'}})
          } else {
            setState({...state, alert: {text: 'Already up to date.', color: 'warning'}})
          }
        })

        getLatestCommit()
        return
      }

      if (r.status === 401){
        setState({...state, alert: {text: 'Unauthorized: Try logging out and logging back in.', color: 'danger'}})
        return
      }

      if (r.status === 500){
        r.text().then(t => setState({...state, alert: {text: `Server Error: ${t}`, color: 'danger'}}))
      }
    })
  }

  async function onCheckBoxChange(event) {
      const checked = event.target.checked  
      const name = event.target.name;
      const token = await authService.getAccessToken();
      await fetch(
          `deploy/on-change?deployOnChange=${checked}`,
          {
              method: 'POST',
              headers: !token ? {} : { 'Authorization': `Bearer ${token}` },
          }  )
      setState({...state, [name]: checked})
  }

  function renderContent() {
    const commit = state.commit;
    return (
        <div>
          <h4>Currently Deployed Commit</h4>
          <FormControlLabel
              control={
                <Checkbox
                    checked={state.deployOnChange}
                    onChange={onCheckBoxChange}
                    name="deployOnChange"
                    color="primary"
                />
              }
              label="Deploy on change"
          />        <Container className={"commit-container"}>
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
        <Button onClick={deploy} color={'primary'} block disabled={state.deployOnChange}>
          Deploy
        </Button>
        </div>
    );
  }

  function renderAlert() {
    const spinner = (state.alert.showSpinner) ? <span><Spinner size="sm" color="info" />{' '}</span> : <div/>
    return (<Alert color={state.alert.color}>{spinner}{state.alert.text}</Alert>)
  }
  
    const contents = state.loading ? (
        <p>
          <em>Loading...</em>
        </p>
    ) : (
        renderContent()
    )
    
    const alert = state.alert.text.length === 0 ? (
        <div/>
    ) : (
        renderAlert()
    )

    return (
      <div className={"deploy"}>
        {contents}
        {alert}
      </div>
    );
}
