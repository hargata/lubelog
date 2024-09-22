function showExtraFieldModal() {
    $.get(`/Home/GetExtraFieldsModal?importMode=0`, function (data) {
        $("#extraFieldModalContent").html(data);
        $("#extraFieldModal").modal('show');
    });
}
function hideExtraFieldModal() {
    $("#extraFieldModal").modal('hide');
}
function getCheckedTabs() {
    var visibleTabs = $("#visibleTabs :checked").map(function () {
        return this.value;
    });
    return visibleTabs.toArray();
}
function deleteLanguage() {
    var languageFileLocation = `/translations/${$("#defaultLanguage").val()}.json`;
    $.post('/Files/DeleteFiles', { fileLocation: languageFileLocation }, function (data) {
        //reset user language back to en_US
        $("#defaultLanguage").val('en_US');
        updateSettings();
    });
}
function updateSettings() {
    var visibleTabs = getCheckedTabs();
    var defaultTab = $("#defaultTab").val();
    if (!visibleTabs.includes(defaultTab)) {
        defaultTab = "Dashboard"; //default to dashboard.
    }
    //Root User Only Settings that aren't rendered:
    var defaultReminderEmail = $("#inputDefaultEmail").length > 0 ? $("#inputDefaultEmail").val() : "";
    var disableRegistration = $("#disableRegistration").length > 0 ? $("#disableRegistration").is(":checked") : false;
    var enableRootUserOIDC = $("#enableRootUserOIDC").length > 0 ? $("#enableRootUserOIDC").is(":checked") : false;

    var userConfigObject = {
        useDarkMode: $("#enableDarkMode").is(':checked'),
        enableCsvImports: $("#enableCsvImports").is(':checked'),
        useMPG: $("#useMPG").is(':checked'),
        useDescending: $("#useDescending").is(':checked'),
        hideZero: $("#hideZero").is(":checked"),
        useUKMpg: $("#useUKMPG").is(":checked"),
        useThreeDecimalGasCost: $("#useThreeDecimal").is(":checked"),
        useMarkDownOnSavedNotes: $("#useMarkDownOnSavedNotes").is(":checked"),
        enableAutoReminderRefresh: $("#enableAutoReminderRefresh").is(":checked"),
        enableAutoOdometerInsert: $("#enableAutoOdometerInsert").is(":checked"),
        enableShopSupplies: $("#enableShopSupplies").is(":checked"),
        enableExtraFieldColumns: $("#enableExtraFieldColumns").is(":checked"),
        hideSoldVehicles: $("#hideSoldVehicles").is(":checked"),
        preferredGasUnit: $("#preferredGasUnit").val(),
        preferredGasMileageUnit: $("#preferredFuelMileageUnit").val(),
        userLanguage: $("#defaultLanguage").val(),
        visibleTabs: visibleTabs,
        defaultTab: defaultTab,
        disableRegistration: disableRegistration,
        defaultReminderEmail: defaultReminderEmail,
        enableRootUserOIDC: enableRootUserOIDC
    }
    sloader.show();
    $.post('/Home/WriteToSettings', { userConfig: userConfigObject }, function (data) {
        sloader.hide();
        if (data) {
            setTimeout(function () { window.location.href = '/Home/Index?tab=settings' }, 500);
        } else {
            errorToast(genericErrorMessage());
        }
    })
}
function makeBackup() {
    $.get('/Files/MakeBackup', function (data) {
        window.location.href = data;
    });
}
function openUploadLanguage() {
    $("#inputLanguage").click();
}
function openRestoreBackup() {
    $("#inputBackup").click();
}
function uploadLanguage(event) {
    let formData = new FormData();
    formData.append("file", event.files[0]);
    sloader.show();
    $.ajax({
        url: "/Files/HandleTranslationFileUpload",
        data: formData,
        cache: false,
        processData: false,
        contentType: false,
        type: 'POST',
        success: function (response) {
            sloader.hide();
            if (response.success) {
                setTimeout(function () { window.location.href = '/Home/Index?tab=settings' }, 500);
            } else {
                errorToast(response.message);
            }
        },
        error: function () {
            sloader.hide();
            errorToast("An error has occurred, please check the file size and try again later.");
        }
    });
}
function restoreBackup(event) {
    let formData = new FormData();
    formData.append("file", event.files[0]);
    console.log('LubeLogger - DB Restoration Started');
    sloader.show();
    $.ajax({
        url: "/Files/HandleFileUpload",
        data: formData,
        cache: false,
        processData: false,
        contentType: false,
        type: 'POST',
        success: function (response) {
            if (response.trim() != '') {
                $.post('/Files/RestoreBackup', { fileName: response }, function (data) {
                    sloader.hide();
                    if (data) {
                        console.log('LubeLogger - DB Restoration Completed');
                        successToast("Backup Restored");
                        setTimeout(function () { window.location.href = '/Home/Index' }, 500);
                    } else {
                        errorToast(genericErrorMessage());
                        console.log('LubeLogger - DB Restoration Failed - Failed to process backup file.');
                    }
                });
            } else {
                console.log('LubeLogger - DB Restoration Failed - Failed to upload backup file.');
            }
        },
        error: function () {
            sloader.hide();
            console.log('LubeLogger - DB Restoration Failed - Request failed to reach backend, please check file size.');
            errorToast("An error has occurred, please check the file size and try again later.");
        }
    });
}