import React, { Component } from 'react';
import authService from './api-authorization/AuthorizeService'
import {Button} from "reactstrap";
import { HexColorPicker } from "react-colorful";
import {Checkbox, FormControlLabel} from "@material-ui/core";

export class HueColors extends Component {
    static displayName = HueColors.name;

    constructor(props) {
        super(props);

        this.resetToDefault = this.resetToDefault.bind(this)
        this.setNewDefault = this.setNewDefault.bind(this)
        this.handleIsEnabledChange = this.handleIsEnabledChange.bind(this)


        this.state = {
            hueColors: [],
            isEnabled: false,
        };
    }

    componentDidMount() {
        this.getStatus().then()
        this.getHueColors().then();
    }

    async handleIsEnabledChange(event) {
        const flag = event.target.checked
        const token = await authService.getAccessToken();
        await fetch(
            `huecolors/set-enabled?isEnabled=${flag}`,
            {
                method: 'POST',
                headers: !token ? {} : {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                },
                contentType: "application/json",
            }
        ).then()
        this.setState({ isEnabled: flag });
    };

    renderLight(light) {
        return (
            <div key={light.hueId}>
                <h4>{light.name}</h4>
                <section className={'responsive'}>
                    <HexColorPicker color={light.color} onChange={(c) => this.setColor(light, c)} />
                </section>
                <Button id={light.hueId} onClick={this.resetToDefault} color={'secondary'} block>
                    Reset to default
                </Button>
                <Button id={light.hueId} onClick={(c) => this.setNewDefault(light, c)} color={'primary'} block>
                    Set as default
                </Button>
                <br/>
            </div>
        );
    }

    render() {
        const contents = this.state.isEnabled ? this.state.hueColors.map(c => this.renderLight(c)) : <></>

        return (
            <div className={"colors"}>
                <h2>Hue Lights</h2>
                <FormControlLabel
                    control={<Checkbox color={"primary"} checked={this.state.isEnabled} onChange={this.handleIsEnabledChange} name="isEnabled" />}
                    label="Is Enabled"
                />
                {contents}
            </div>
        );
    }

    async setColor(light, color) {
        light.color = color.substring(1)
        await this.updateLight(light);
        const colors = this.state.hueColors
        const update = colors.find(c => c.hueId === light.hueId)
        update.color = light.color
        this.setState({hueColors: colors})
    }

    async resetToDefault(event) {
        const lightId = event.target.id
        const colors = this.state.hueColors
        const light = colors.find(c => c.hueId.toString() === lightId)
        light.color = light.defaultColor
        await this.updateLight(light)
        this.setState({hueColors: colors})
    }

    async setNewDefault(light) {
        const colors = this.state.hueColors
        const update = colors.find(c => c.hueId === light.hueId)
        update.defaultColor = light.color
        await this.updateLight(update)
        this.setState({hueColors: colors})
    }

    async updateLight(light) {
        const token = await authService.getAccessToken();
        await fetch(
            `huecolors/light`,
            {
                method: 'POST',
                headers: !token ? {} : {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                },
                contentType: "application/json",
                body: JSON.stringify(light)
            }
        ).then()
    }

    async getHueColors() {
        const token = await authService.getAccessToken();
        const response = await fetch('huecolors/lights', {
            headers: !token ? {} : { 'Authorization': `Bearer ${token}` }
        });
        if (response.redirected){
            window.location.href = response.url;
            return
        }

        if (response.status === 200){
            const data = await response.json();
            this.setState({ hueColors: data });
            return
        }

        if (response.status === 500){
            response.text().then(t => this.setState({alert: {text: `Server Error: ${t}`, color: 'danger'}}))
        }
    }

    async getStatus() {
        const token = await authService.getAccessToken();
        const response = await fetch('huecolors/status', {
            headers: !token ? {} : { 'Authorization': `Bearer ${token}` }
        });
        if (response.redirected){
            window.location.href = response.url;
            return
        }

        if (response.status === 200){
            const status = await response.json();
            this.setState({ isEnabled: status.isEnabled });
            return
        }

        if (response.status === 500){
            response.text().then(t => this.setState({alert: {text: `Server Error: ${t}`, color: 'danger'}}))
        }
    }
}
