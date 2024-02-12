# Postgres

For users that desire additional scalability for their backend, LubeLogger now supports a PostgreSQL backend.

## Configuration

To configure LubeLogger to use PostgreSQL, you must first create a database with a schema named "app" in it, in the screenshot below we created a DB named "lubelogger" and then a schema named "app".

![](/Postgres/a/image-1707454502212.png)

Once that is done, simply inject the environment variable `POSTGRES_CONNECTION` with your connection string, example:

```
Host=<yourserveraddress:port>;Username=<yourusername>;Password=<yourpassword>;Database=<databasename>;
```

LubeLogger will then automatically create the tables it needs, all records will then be saved and loaded from Postgres tables from now on.

## Backups

Once you have switched over to Postgres, LubeLogger's built in and backup function will only back up images, documents, and the server config. You are responsible for maintaining backups of the DB records.

## Database Migration

A tool is provided to ease the migration process between LiteDB and Postgres. This tool can be found at the `/migration` endpoint and is only accessible when a Postgres connection is provided.

![](/Postgres/a/image-1707516092170.png)

### Importing to Postgres

To transfer all your existing data from LiteDB to Postgres:
1. Create a backup using the "Make Backup" feature in the Settings tab.
2. Extract the zip file.
3. You should see a folder named "data" in the extracted folder
4. Inside the data folder will be a .db file named `cartracker.db`
5. Navigate to the Database Migration tool
6. Click "Import to Postgres"
7. Select the `cartracker.db` file
8. Your data will be imported into your Postgres DB, and you may double check that the DB has been imported successfully using a Postgres DB Administration Tool.

### Exporting from Postgres

In the event that you need to transfer all your data back onto a LiteDB database file from Postgres, you may do so using the Database Migration tool:
1. Navigate to the Database Migration Tool
2. Click "Export from Postgres"
3. Extract the downloaded zip file and you should find `cartracker.db` in it.
4. Create a backup using the "Make Backup" feature in the Settings tab.
5. Extract the zip file.
6. You should see a folder named "data" in the extracted folder, if not, create it.
7. Place `cartracker.db` inside the "data" folder
8. Re-zip the extracted folder
9. Restore the backup using the "Restore Backup" feature in the Settings tab.
10. Make sure you remove the PostgreSQL connection from the environment variables so that all future changes will be saved in LiteDB.
