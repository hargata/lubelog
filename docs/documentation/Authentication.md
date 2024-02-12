# Authentication

LubeLogger does not require authentication by default; however, it is highly recommend that you set up authentication if your LubeLogger instance is accessible vvia the Internet or if you wish to invite other users to your instance.

## Enabling Authentication
To enable authentication, all you have to do is navigate to the "Settings" tab and check "Enable Authentication".

A dialog will then prompt you to enter a username and password. These are the credentials for the Root/Super User.

Once you have entered the credentials, click the "Setup" button and you will be redirected to a login screen, enter the credentials of the Root/Super User here to login.

## Creating / Inviting New Users
LubeLogger relies on an invitation-only model for creating/registering new users. It is highly recommended that LubeLogger is configured with SMTP in order to make the user registration process as smooth as possible, see [[Getting Started]] for more information regarding SMTP configuration.

To Create/Invite New Users, you first need to enable authentication and set up the Root User credentials. Once that is done, upon login, you will see that there is now a dropdown to the right of the "Settings" tab that has your root username on it. Click on that dropdown and select "Admin Panel"
![](/Authentication/a/image-1706398957700.png)

You will now be taken to a new page. There are two sections in this page, Tokens and Users. 
![](/Authentication/a/image-1706398972637.png)

Tokens are used for invitees to register their user account or existing users to reset their password. These tokens are single use and are validated against the email address they are issued for.

Users, as the name suggests, is a list of users in the system. You can also mark or unmark existing users as Admins here, see below to understand what permissions Admin users have.

To invite a user, simply click on the "Generate User Token" button and type in their email address, note that this is case-sensitive. 
![](/Authentication/a/image-1706398926360.png)

If SMTP is configured and the Auto Notify(via Email) switch is checked, the user will receive an email that looks like this:
![](/Authentication/a/image-1706398865186.png)

## Root/Super User
You might be tempted to use your root credentials as your main credentials, and there is nothing wrong that, but you should know that there are a few caveats associated with the root user.
1. Root/Super Users can view and edit all vehicles, this might seem like a great advantage initially, but it can be problematic if there are sufficient users and you have to sift through dozens of vehicles to get to yours.
2. Any setting you enable/disable will become the default setting inherited by new users.

For the reasons above, it is highly recommended that you create a second user for personal use and mark it as an Admin. See below for a breakdown of permissions across user tiers.

| Permissions            | User                                      | Admin                                     | Root/Super User        |
| ---------------------- | ----------------------------------------- | ----------------------------------------- | ---------------------- |
| View/Edit Vehicles     | Only vehicles which they are collaborator | Only vehicles which they are collaborator | View/Edit All Vehicles |
| Access API             | Yes                                       | Yes                                       | Yes                    |
|  Personalized Settings                      |      Yes                                     |   Yes                                        |    No, settings will be set as server default                    |
| Add/Remove Users       | No                                        | Yes                                       | Yes                    |
| Make/Restore Backups   | No                                        | No                                        | Yes                    |
| Disable Authentication | No                                        | No                                        | Yes                    |
