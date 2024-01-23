# Configuring LubeLogger
In order to provide the best possible user experience, we have provided ample amount of flexibility when it comes to user settings.
Upon initial launch, you are using the Root User by default without any authentication, so you will have access to all of the settings.

![image](https://github.com/hargata/lubelog/assets/155338622/cf73ff68-e79b-468d-84fb-375e481cbadd)

Most of the settings are relatively straightforward and self-explanatory.

**Note:** If you are a user in the UK and you wish be able to input Fuel Purchases in Liters but display Fuel Mileage as Miles Per UK Gallons, you will need to enable "Use Imperial Calculation" and "Use UK MPG Calculation"

## Enable Authentication
It is highly recommended that you secure your LubeLogger instance by enabling authentication.
To do so, simply check "Enable Authentication" and you will be prompted to enter a Username and Password

![image](https://github.com/hargata/lubelog/assets/155338622/4d8b1855-e437-4ade-999b-3dd6ea19e55f)

The credentials that you set up here are the credentials for the Root User, aka the Super Admin, and shouldn't be shared with anyone else.

Once you have entered the credentials, you will then be redirected to a Login page

![image](https://github.com/hargata/lubelog/assets/155338622/26181116-5a03-48cf-972d-89a4b1050cce)

Simply enter the credentials you have just set up and you will be logged right in

![image](https://github.com/hargata/lubelog/assets/155338622/20d9a3b9-80de-4a32-8cb6-1100dd237dbd)

## Setting Up Multiple Users
To set up multiple users, all you have to do is click on the dropdown that has your username on it and select "Admin Panel"

![image](https://github.com/hargata/lubelog/assets/155338622/75f32408-b9f3-4e2c-bec6-ea1c196c3438)

If you have SMTP configured correctly, the "Auto Notify(via Email) switch will be enabled and checked, otherwise it will be disabled/grayed out.
Without SMTP Configured:

![image](https://github.com/hargata/lubelog/assets/155338622/9d75ab55-afb7-4d3e-b830-3191de23695c)

With SMTP Configured:

![image](https://github.com/hargata/lubelog/assets/155338622/d9995c30-dd47-44d1-a73d-39f0f129450b)

To generate a new user token, simply click on the "Generate User Token" button and you will be prompted with the user's email address

![image](https://github.com/hargata/lubelog/assets/155338622/f870d5cd-4d4b-480c-94ea-b1161757793f)
