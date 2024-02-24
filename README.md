![image](https://github.com/hargata/lubelog/assets/155338622/545debcd-d80a-44da-b892-4c652ab0384a)

Self-Hosted, Open-Source, Web-Based Vehicle Maintenance and Fuel Mileage Tracker

Website: https://lubelogger.com

## Why
Because nobody should have to deal with a homemade spreadsheet or a shoebox full of receipts when it comes to vehicle maintenance.

## Showcase
[Promotional Brochure](https://lubelogger.com/brochure.pdf)

[Screenshots](/docs/screenshots.md)

## Demo
Try it out before you download it! The live demo resets every 20 minutes.

[Live Demo](https://demo.lubelogger.com) Login using username "test" and password "1234"

## Download
LubeLogger is available as both a Docker Image and a Windows Standalone Executable.

Read this [Getting Started Guide](https://docs.lubelogger.com/Getting%20Started) on how to download either of them

### Docker Setup (Manual Build for Advanced Users)
1. Install Docker
2. Clone this repo
3. CHECK culture in .env file, default is en_US, also setup SMTP for user management if you want that.
4. Run `docker build -t lubelogger -f Dockerfile .`
5. CHECK docker-compose.yml and make sure the mounting directories look correct.
6. If using traefik, use docker-compose.traefik.yml
7. Run `docker-compose up`

### Need Help?
[Documentation](https://docs.lubelogger.com/)

[Troubleshooting Guide](https://docs.lubelogger.com/Troubleshooting)

[Search Existing Issues](https://github.com/hargata/lubelog/issues)

## Dependencies
- Bootstrap
- LiteDB
- Npgsql
- Bootstrap-DatePicker
- SweetAlert2
- CsvHelper
- Chart.js
- Drawdown

## License
LubeLogger utilizes a dual-licensing model, see [License](/LICENSE) for more information

## Support
Support this project by [Subscribing on Patreon](https://patreon.com/LubeLogger) or [Making a Donation](https://buy.stripe.com/aEU9Egc8DdMc9bO144)

Note: Commercial users are required to maintain an active Patreon subscripton to be compliant with our licensing model.
