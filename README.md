![image](https://github.com/hargata/lubelog/assets/155338622/545debcd-d80a-44da-b892-4c652ab0384a)

Self-Hosted, Open-Source, Web-Based Vehicle Maintenance and Fuel Mileage Tracker

Website: https://lubelogger.com

## About This Fork

This is a personal fork of [hargata/lubelog](https://github.com/hargata/lubelog) with the following changes on top of upstream:

- **Insurance record tracking** — a new "Insurance" tab (cloned from the existing Tax record feature) for logging vehicle insurance costs, including:
  - Provider and Policy Number fields
  - Recurring policy renewals (auto-generates the next entry once a renewal date passes)
  - Installment/partial payments — track a policy's Total Premium and log individual payments against it, with a live "Paid / Remaining" balance shown on both the record and the record list
  - Full integration with cost reports, charts, tag filtering, search, duplication, and the Settings → Visible Tabs / Default Tab / Tab Order screens
- **Fuel Economy labels** — the "Min Fuel Economy" / "Max Fuel Economy" badges on the Gas tab now read "Worst Fuel Economy" / "Best Fuel Economy", correctly reflecting which number is actually better regardless of whether you're using MPG or a consumption-based unit (e.g. L/100km).

> **Note:** The code changes in this fork were generated with the assistance of an AI coding assistant (Claude), based on and closely following the existing patterns already present in the upstream codebase (in particular, the Tax record feature was used as the template for Insurance tracking). It has been reviewed and built successfully, but has not been exhaustively tested against every edge case — use accordingly, and see [hargata/lubelog](https://github.com/hargata/lubelog) for the original, upstream-maintained project.

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

Read this [Getting Started Guide](https://docs.lubelogger.com/Installation/Getting%20Started) on how to download either of them

### Kubernetes Deployment
[Helm Chart](https://artifacthub.io/packages/helm/anza-labs/lubelogger) provided by [Anza-Labs](https://github.com/anza-labs)

### Need Help?
[Documentation](https://docs.lubelogger.com/)

[Troubleshooting Guide](https://docs.lubelogger.com/Installation/Troubleshooting)

[Search Existing Issues](https://github.com/hargata/lubelog/issues)

## Dependencies
- [Bootstrap](https://github.com/twbs/bootstrap)
- [LiteDB](https://github.com/mbdavid/litedb)
- [Npgsql](https://github.com/npgsql/npgsql)
- [Bootstrap-DatePicker](https://github.com/uxsolutions/bootstrap-datepicker)
- [SweetAlert2](https://github.com/sweetalert2/sweetalert2)
- [CsvHelper](https://github.com/JoshClose/CsvHelper)
- [Chart.js](https://github.com/chartjs/Chart.js)
- [Drawdown](https://github.com/adamvleggett/drawdown)
- [MailKit](https://github.com/jstedfast/MailKit)
- [Masonry](https://github.com/desandro/masonry)
- [QRCode-Generator](https://github.com/kazuhikoarase/qrcode-generator)

## License
MIT

## Support
To support this project, please see [Funding](https://docs.lubelogger.com/Misc/Funding)
