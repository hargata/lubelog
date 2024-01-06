![image](https://github.com/hargata/lubelog/assets/155338622/545debcd-d80a-44da-b892-4c652ab0384a)

A self-hosted, open-source vehicle service records and maintainence tracker.

## Why
Because nobody should have to deal with a homemade spreadsheet or a shoebox full of receipts when it comes to vehicle maintainence.

## Docker Setup (Recommended)
1. Install Docker
2. Clone this repo
3. Run `docker build -t lubelog .`
4. Run `docker run -d -p 80:5000 --name lubelog lubelog`
   1. Optionally, you can mount a volume to the container to persist data.  For example, `docker run -d -p 80:5000 -v /path/to/data:/app/data --name lubelog lubelog`

## Dependencies
- Bootstrap
- LiteDB
- Bootstrap-DatePicker
- SweetAlert2
- CsvHelper
- Chart.js