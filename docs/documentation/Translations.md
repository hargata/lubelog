# Translations

LubeLogger supports UI Translations for ~95% of UI elements.

The following are not covered by translations:
- Toasts(messages that pop up on the top right)
- Sweetalert prompts(confirm delete dialogs, etc)
- About section

## Where to get translations
Translations can be found at [this repository](https://github.com/hargata/lubelog_translations/)

1. To upload a translation file, login as the root user.
2. Navigate to "Settings"
3. Click "Upload" under the "Manage Languages" section
![](/Translations/a/image-1707068703210.png)

4. Select the language file you wish to upload
5. The page should refresh
6. Select the language file from the dropdown to set it as your default language.

## Creating your own translation
1. Download the [latest en_US.json](https://github.com/hargata/lubelog/blob/main/wwwroot/defaults/en_US.json) file from the GitHub Repository for LubeLogger.
2. Rename this file, en_US is a reserved name.
3. Use a JSON pretty-printer to make it human-readable
![](/Translations/a/image-1707068227706.png)

3. The objects to the left of the ":" are the translation keys, DO NOT modify these.
4. The objects to the right of the ":" are the translation values(shown in green), these are what you want to translate.
5. To test out your translation, simply upload it to your LubeLogger instance and test it out.

## Contribute
Follow the instructions outlined in the [official repository](https://github.com/hargata/lubelog_translations/)

Translation efforts are coordinated via [this thread](https://github.com/hargata/lubelog/discussions/240)
