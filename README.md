# NodeSentinel

## Project Overview
NodeSentinel is a dedicated server hosting and monitoring tool. Users can create servers with ease and start playing their favorite games without needing knowledge of server setup. Users can monitor and manage servers inside the web UI. NodeSentinel supports Terraria, tModLoader, Minecraft and Valheim server hosting.

## Hosting

Servers are hosted on Azure. Virtual Machines use Ubuntu Server. I used one B1ms VM for the web server and a larger B2s VM for server hosting. Docker containers are used to deploy servers. Caddy is used as a reverse proxy. Both the API and web server are started using Docker Compose.

## UI design

The main color palette is Tailwind's mist palette. Blue is used as the brand color. The design also includes obvious semantic colors for destructive actions and so on. The project uses icons from Heroicons and Lucide. The UI is built with HTML, Tailwind CSS and JavaScript. The project only supports dark theme.

## Database

The database for this project is MongoDB. I chose MongoDB for its simplicity — I didn't need a SQL database. NoSQL is also a great fit because I didn't know the exact database structure upfront. Different collections store different data; for example, env data for servers varies depending on the server type.

## API

The API allows the web server and server manager to communicate with each other. It is a REST API built on an ASP.NET MVC project. I used OpenAPI to create the API schema and the Kiota library to generate C# classes from it. Everything related to servers — sending commands and retrieving server data — goes through the Docker API. Only the web server knows the API key, so users cannot call the API directly. The API key is generated using the openssl command and is validated at the middleware level, so every request requires a valid key. Including when i am trying to retreive openapi schema, so i need to disable it :D.

## Security
Argon2 hashing with salt is used to store user passwords. A private API key is used for communication between the web server and the server API. Users can only see their own servers.

## Servers 
Servers are created using Docker and the Docker.DotNet library, which interacts with the Docker API. I used pre-made open source server images to make server creation easier.

## Backups
World saves are backed up by copying them to a separate directory. When restoring a backup, the server is stopped and the save file is overwritten with the selected backup.

## Libraries & Dependencies

- ASP.NET MVC
- Docker.DotNet
- HTMX
- Openapi
- Kiota
- Swagger UI
- Humanizer
- Argon2
- Chart.js
- MongoDB

## Architecture diagram

Visual structure of the project
![diagram](Diagrams/Systemdiagram.png)

## Database Schema

![diagram](Diagrams/NodeSentinelDB.png)

## Known limitations

- One VM cannot support many dedicated servers due to the single-server infrastructure.
- Minecraft backups don't work, probably due to how Minecraft servers handle saving.
- Valheim backups are untested because I don't own Valheim :D
- Valheim containers do not support manual saving from the web UI.
- Mods are fetched once with no logic to refresh them. I already had the data, so why bother :D
- The UI doesn't render correctly on Chromium-based browsers :D
- Only works on Linux
