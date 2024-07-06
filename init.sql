DO $$
BEGIN
    IF NOT EXISTS (SELECT schema_name FROM information_schema.schemata WHERE schema_name = 'app') THEN
        CREATE SCHEMA app;
    END IF;
END $$;
