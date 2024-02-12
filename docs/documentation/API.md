# API

LubeLogger provides API endpoints to retrieve and add records, full documentation of these endpoints can be found at `/api`.

## Authentication
If authentication is enabled, it implements Basic Auth based on RFC2617, which stipulates that the "token" is passed in as a Base64-encoded string comprising of a username and password separated by a colon(":"). Because of this, neither the username nor password can contain a colon(":") character.

### Testing
You can utilize any REST API testing tool to test your use-case.

## Example Use Cases
- Send Email Reminders, see [[Reminders|Records/Reminders#reminder-emails]]
- Insert Odometer Records, see [[Odometer|Records/Odometer#api-integration]]
- Create DB Backups
