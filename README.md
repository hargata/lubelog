![image](https://github.com/hargata/lubelog/assets/155338622/545debcd-d80a-44da-b892-4c652ab0384a)

A self-hosted, open-source vehicle service records and maintenance tracker.

Visit our website: https://lubelogger.com

Support this project by [Subscribing on Patreon](https://patreon.com/LubeLogger) or [Making a Donation](https://buy.stripe.com/aEU9Egc8DdMc9bO144)

Note: Commercial users are required to maintain an active Patreon subscripton to be compliant with our licensing model.

## Why
Because nobody should have to deal with a homemade spreadsheet or a shoebox full of receipts when it comes to vehicle maintenance.

## Showcase
[Promotional Brochure](https://lubelogger.com/brochure.pdf)

[Screenshots](/docs/screenshots.md)

## Demo
Try it out before you download it! The live demo resets every 20 minutes.

[Live Demo](https://demo.lubelogger.com) Login using username "test" and password "1234"

## Download
LubeLogger is available as both a Docker image and a Windows Standalone Executable.

Read this [Getting Started Guide](https://docs.lubelogger.com/Getting%20Started) on how to download either of them

### Docker Setup (Manual Build)
1. Install Docker
2. Clone this repo
3. CHECK culture in .env file, default is en_US, also setup SMTP for user management if you want that.
4. Run `docker build -t lubelogger -f Dockerfile .`
5. CHECK docker-compose.yml and make sure the mounting directories look correct.
6. If using traefik, use docker-compose.traefik.yml
7. Run `docker-compose up`

## Dependencies
- Bootstrap
- LiteDB
- Npgsql
- Bootstrap-DatePicker
- SweetAlert2
- CsvHelper
- Chart.js
- Drawdown
