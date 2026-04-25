# NodeSentinel

## Project Overview

NodeSentinel is dedicated server hosting and monitoring tool. Users can create servers with ease and start playing their favorite games, without needing knowledge to setup server. Users can monitor and use server inside web-UI.

## Hosting

Servers will be hosted on Azure. Virtual Machines will use Ubuntu server. Docker container will be used to deploy servers. Nginx is used to create reverse-proxy.

## UI design

Main color palette for this project is tailwinds mist palette. Blue will be used as brand color. Design also includes obvious semantic colors for destructive action and etc.

## Database

The database that this project uses is MongoDB. I chose MongoDB for this project due to its simplicity and not needing a SQL database. NoSQL is also great because i dont know database structure exactly. There will be tables with different data. For example Env data for servers ,which will be different for every server type.

## API

API is used so web-server and server manager can communicate with each other. My API is REST type.

## Libraries & Dependencies

- ASP.NET MVC
- Docker.DotNet
- HTMX
- Chart.js
- MongoDB

## Architecture diagram

Visual structure of the project
![diagram](Diagrams/Systemdiagram.png)

## Database Schema

## Known limitations

- One server cant support many dedicated servers, due to one server infrastructure
