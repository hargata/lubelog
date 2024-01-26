![image](https://github.com/hargata/lubelog/assets/155338622/545debcd-d80a-44da-b892-4c652ab0384a)

A self-hosted, open-source vehicle service records and maintainence tracker.

Visit our website: https://lubelogger.com

Support this project on Patreon: https://patreon.com/LubeLogger

## Why
Because nobody should have to deal with a homemade spreadsheet or a shoebox full of receipts when it comes to vehicle maintainence.

## Screenshots
<a href="/docs/screenshots.md">Screenshots</a>

## Demo
Try it out before you download it! The live demo resets every 20 minutes.

[Live Demo](https://demo.lubelogger.com) Login using username "test" and password "1234"

## Dependencies
- Bootstrap
- LiteDB
- Bootstrap-DatePicker
- SweetAlert2
- CsvHelper
- Chart.js

## Docker Setup (GHCR)
1. Install Docker
2. Run `docker pull ghcr.io/hargata/lubelogger:latest`
3. CHECK culture in .env file, default is en_US, this will change the currency and date formats. You can also setup SMTP Config here.
4. If using traefik, use docker-compose.traefik.yml
5. Run `docker-compose up`

## Docker Setup (Manual Build)
1. Install Docker
2. Clone this repo
3. CHECK culture in .env file, default is en_US, also setup SMTP for user management if you want that.
4. Run `docker build -t lubelogger -f Dockerfile .`
5. CHECK docker-compose.yml and make sure the mounting directories look correct.
6. If using traefik, use docker-compose.traefik.yml
7. Run `docker-compose up`

## Additional Docker Instructions

### manual

- build

```
docker build -t hargata/lubelog:latest .
```

- run

```
docker run -d hargata/lubelog:latest
```

add `-v` for persistent volumes as needed. Have a look at the docker-compose.yml for examples.

## docker-compose

- build image

```
docker compose build
```

- run

```
docker compose up

# or variant with traefik labels:

docker compose -f docker-compose.traefik.yml up
```
