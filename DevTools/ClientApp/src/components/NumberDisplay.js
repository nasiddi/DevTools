import React, {useEffect, useState} from 'react'
import Grid from '@material-ui/core/Grid'
import {Button, TextField} from "@material-ui/core";

export function EnterNumber(){
    const [number, setNumber] = useState('')
    const [displayed, setDisplayed] = useState('')
    
    async function onButtonClick() {
        await fetch(`/kidsnumber?number=${number}`, {method: 'POST'})
        window.location.reload()
    }

    function onNumberChange(event) {
        setNumber(event.target.value)
    }

    useEffect(() => {
        async function load(){
            const response = await fetch('/kidsnumber', {method: 'GET'})
            setDisplayed(await response.json())
        }
        load().then()
    }, [])

    return <Grid container spacing={3} style={{marginTop: 50}}>
        <Grid item xs={12} sm={6} md={4}>
            <TextField label={'Ausrufsnummer'} variant={'outlined'}  type="number" value={number} onChange={onNumberChange} fullWidth/>
        </Grid>
        <Grid item xs={12} sm={6} md={4}>
            <Button variant={'contained'} color={'primary'} onClick={onButtonClick} fullWidth size={"large"} style={{height: 56}}>Send</Button>
        </Grid>
        <Grid item xs={12} md={4}>
            <h3>Aktuelle Anzeige: {displayed.number}</h3>
        </Grid>
    </Grid>
} 

export function DisplayNumber(){
    const [number, setNumber] = useState('')

    useEffect(() => {
        async function load(){
            const response = await fetch('/kidsnumber', {method: 'GET'})
            setNumber(await response.json())
        }
        load().then()
    }, [])
    return <h1 className={'number'}>{number.number}</h1>
}