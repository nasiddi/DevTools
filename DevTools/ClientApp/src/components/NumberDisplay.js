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

    return <Grid container spacing={3}>
        <Grid item>
            <TextField label={'Ausrufsnummer'} variant={'filled'} type="number" value={number} onChange={onNumberChange}/>
        </Grid>
        <Grid item>
            <Button variant={'contained'} onClick={onButtonClick}>Enter</Button>
        </Grid>
        <Grid item>
            <h3>Aktuelle Anzeige: {displayed.number}</h3>
        </Grid>
    </Grid>
} 

export function DisplayNumber(props){
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