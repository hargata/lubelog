# Getting Started
## Docker
The Docker Container Repository is the most reliable and up-to-date distribution channel for LubeLogger.
You need to have Docker Windows installed and Virtualization enabled(typically a BIOS setting).
You will then clone the following files onto your computer from the repository _.env_ and _docker-compose.yml_ or _docker-compose-traefik.yml_ if you're using Traefik.
In the .env file you will find the following and here are the explanations for the variables.
```
LC_ALL=en_US.UTF-8 <- Locale and Language Settings, this will affect how numbers, currencies, and dates are formatted.
LANG=en_US.UTF-8 <- Same as above. Note that some languages don't have UTF-8 encodings.
MailConfig__EmailServer="" <- Email SMTP settings used only for configuring multiple users(to send their registration token and forgot password tokens)
MailConfig__EmailFrom="" <- Same as above.
MailConfig__UseSSL="false" <- Same as above.
MailConfig__Port=587 <- Same as above.
MailConfig__Username="" <- Same as above.
MailConfig__Password="" <- Same as above.
```

Once you're happy with the configuration, run the following commands to pull down the image and run container.
```
docker pull ghcr.io/hargata/lubelogger:latest
docker-compose up
```
By default the app will start listening at localhost:8080, this port can be configured in the docker-compose file.

## Windows Standalone Executable
Windows Standalone executables are provided on a request basis, and will usually be included with every other release.
To run the server, you just have to double click on CarCareTracker.exe
Occassionally you might run into an issue regarding a missing folder, to fix that, just create a "config" folder where CarCareTracker.exe is located.
When using this approach, the default port the app will be listening on is 5000, so you will navigate to localhost:5000

Whichever path you choose, once you get the app up and running, just navigate to the IP address and port the server is listening to and you should be able to see the app
![image](https://github.com/hargata/lubelog/assets/155338622/a32c40ce-271c-406b-a211-c1f2af138418)
![image](https://github.com/hargata/lubelog/assets/155338622/4e1c913c-001e-43fd-9b56-4ecffdfd4c2e)
