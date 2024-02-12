# Replacing The LubeLogger Logo

You can overwrite the LubeLogger Logo that is displayed in the Login and Home/Garage page.

To do so, simply inject an environment variable with the key `LUBELOGGER_LOGO_URL` into your lubelogger instance either via the .env file or the appsettings.json file.

## .env
```
LUBELOGGER_LOGO_URL=<URL to your Logo>
```

## appsettings.json
```
LUBELOGGER_LOGO_URL:<URL to your Logo>
```

## Non-replaceable Locations
- Logo in the About section in the Settings tab
- Logo that shows up in the top left of the Vehicle Maintenance History Report
