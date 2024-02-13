# Troubleshooting
Common issues and steps you can take to fix them.

## General Issues

### Button doesn't work / feature stopped working.
Your browser might have cached an older version of a JavaScript(JS) file which is no longer compatible with the current version of LubeLogger. Clear your browser's cache and retry.

### Can't Send Email via SMTP
Note that for most email providers, you can no longer use your account password to authenticate and must instead generate an app password for LubeLogger to be able to authenticate on your behalf to your email provider's SMTP server.

If you've downloaded the .env file from the GitHub repo, there is an issue with how the file gets formatted when it is downloaded, you will have to copy the contents and re-create one manually on your machine.

### Console shows Authentication Errors
Those are purely informational, add a line in your environment variables to prevent information logs from showing up in the console.

## Locale Issues

### Can't input values in "," format / shows up as 0.
Ensure that your locale environment variables are configured correctly, note that if running via docker, both environment variables LANG and LC_ALL have to be identical.

### Can't change locale.
Environment variables are injected on deployment. You will need to re-deploy.

## Server Issues

### NGINX / Cloudflare 
LubeLogger is a web app that runs on Kestrel, it literally doesn't matter if it's deployed behind a reverse proxy or Cloudflare tunnel. As long as the app can receive traffic on the port it's configured on, it will run.

Here's a sample Nginx reverse proxy configuration courtesy of [thehijacker](https://github.com/thehijacker)
```
server
{
    listen 443 ssl http2;
    server_name lubelogger.domain.com;

    ssl_certificate /etc/nginx/ssl/acme/domain.com/fullchain.pem;
    ssl_certificate_key /etc/nginx/ssl/acme/domain.com/key.pem;
    ssl_dhparam /etc/nginx/ssl/acme/domain.com/dhparams.pem;
    ssl_trusted_certificate /etc/nginx/ssl/acme/domain.com/fullchain.pem;

    location /
    {
        proxy_pass http://192.168.28.53:8289;
        client_max_body_size               50000M;
        proxy_set_header Host              $http_host;
        proxy_set_header X-Real-IP         $remote_addr;
        proxy_set_header X-Forwarded-For   $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_http_version 1.1;
        proxy_set_header   Upgrade    $http_upgrade;
        proxy_set_header   Connection "upgrade";
        proxy_redirect off;
    }
}
```
