![image](https://github.com/hargata/lubelog/assets/155338622/545debcd-d80a-44da-b892-4c652ab0384a)

A self-hosted, open-source vehicle service records and maintainence tracker.

## Why
Because nobody should have to deal with a homemade spreadsheet or a shoebox full of receipts when it comes to vehicle maintainence.

## Docker Setup (Recommended)
1. Install Docker
2. Clone this repo
3. Run `docker build -t lubelog .`
4. CHECK docker-compose.yaml and make sure the mounting directories look correct.
5. Run `docker-compose up`

## Dependencies
- Bootstrap
- LiteDB
- Bootstrap-DatePicker
- SweetAlert2
- CsvHelper
- Chart.js