# Odometer

The Odometer tab is where you can log your current odometer reading without having to insert any Service/Repair/Upgrade/Fuel records. This odometer readings entered in this tab allows Reminder urgencies to be calculated as accurately as possible since it uses the maximum mileage reported in each of the tabs to determine the last reported mileage.

The Odometer tab is hidden by default and must be enabled by checking the "Odometer" switch under "Visible Tabs" in the Settings tab.

## API Integration
As with the other tabs, odometer readings can be retrieved via a GET endpoint and inserted via a POST API endpoint.

Example use cases:
- An app to integrate with OBDII and insert odometer reading from the vehicle's computer onto LubeLogger.
- An app to keep track of distance traveled via GPS and incrementing the last reported odometer reading.

These are not functionalities provided out of the box by LubeLogger, and are just examples of the possibilities achievable via the API endpoints.
