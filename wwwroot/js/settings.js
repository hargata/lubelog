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
function updateColorModeSettings(e) {
    var colorMode = $(e).prop("id");
    switch (colorMode) {
        case "enableDarkMode":
            //uncheck system prefernce
            $("#useSystemColorMode").prop('checked', false);
            updateSettings();
            break;
        case "useSystemColorMode":
            $("#enableDarkMode").prop('checked', false);
            updateSettings();
            break;
    }
}
function updateSettings() {
    var visibleTabs = getCheckedTabs();
    var defaultTab = $("#defaultTab").val();
    if (!visibleTabs.includes(defaultTab)) {
        defaultTab = "Dashboard"; //default to dashboard.
    }
    var tabOrder = getTabOrder();

    var userConfigObject = {
        useDarkMode: $("#enableDarkMode").is(':checked'),
        useSystemColorMode: $("#useSystemColorMode").is(':checked'),
        enableCsvImports: $("#enableCsvImports").is(':checked'),
        useMPG: $("#useMPG").is(':checked'),
        useDescending: $("#useDescending").is(':checked'),
        hideZero: $("#hideZero").is(":checked"),
        automaticDecimalFormat: $("#automaticDecimalFormat").is(":checked"),
        useUKMpg: $("#useUKMPG").is(":checked"),
        useThreeDecimalGasCost: $("#useThreeDecimal").is(":checked"),
        useThreeDecimalGasConsumption: $("#useThreeDecimalGasConsumption").is(":checked"),
        useMarkDownOnSavedNotes: $("#useMarkDownOnSavedNotes").is(":checked"),
        enableAutoReminderRefresh: $("#enableAutoReminderRefresh").is(":checked"),
        enableAutoOdometerInsert: $("#enableAutoOdometerInsert").is(":checked"),
        enableShopSupplies: $("#enableShopSupplies").is(":checked"),
        showCalendar: $("#showCalendar").is(":checked"),
        showVehicleThumbnail: $("#showVehicleThumbnail").is(":checked"),
        showSearch: $("#showGarageSearch").is(":checked"),
        enableExtraFieldColumns: $("#enableExtraFieldColumns").is(":checked"),
        hideSoldVehicles: $("#hideSoldVehicles").is(":checked"),
        preferredGasUnit: $("#preferredGasUnit").val(),
        preferredGasMileageUnit: $("#preferredFuelMileageUnit").val(),
        userLanguage: $("#defaultLanguage").val(),
        useUnitForFuelCost: $("#useUnitForFuelCost").is(":checked"),
        visibleTabs: visibleTabs,
        defaultTab: defaultTab,
        tabOrder: tabOrder
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
    $("#inputLanguage").trigger('click');
}
function openRestoreBackup() {
    $("#inputBackup").trigger('click');
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

function loadSponsors() {
    $.get('/Home/Sponsors', function (data) {
        $("#sponsorsContainer").html(data);
    })
}

function showTranslationEditor() {
    $.get(`/Home/GetTranslatorEditor?userLanguage=${$("#defaultLanguage").val()}`, function (data) {
        $('#translationEditorModalContent').html(data);
        $('#translationEditorModal').modal('show');
    })
}
function hideTranslationEditor() {
    $('#translationEditorModal').modal('hide');
}
function createAndUploadTranslation(translationName, translationData) {
    let jsonData = JSON.stringify(translationData);
    let translationBlob = new Blob([jsonData], { type: "application/json" });
    let translationFile = new File([translationBlob], `${translationName}.json`, { type: "application/json" });
    let formData = new FormData();
    formData.append("file", translationFile);
    sloader.show();
    $.ajax({
        url: "/Home/SaveTranslation",
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
function createAndExportTranslation(translationData) {
    let jsonData = JSON.stringify(translationData);
    let translationBlob = new Blob([jsonData], { type: "application/json" });
    let translationFile = new File([translationBlob], `translationexport.json`, { type: "application/json" });
    let formData = new FormData();
    formData.append("file", translationFile);
    sloader.show();
    $.ajax({
        url: "/Home/ExportTranslation",
        data: formData,
        cache: false,
        processData: false,
        contentType: false,
        type: 'POST',
        success: function (response) {
            sloader.hide();
            if (!response) {
                errorToast(genericErrorMessage());
            } else {
                window.location.href = response;
            }
        },
        error: function () {
            sloader.hide();
            errorToast("An error has occurred, please check the file size and try again later.");
        }
    });
}
function saveTranslation() {
    var currentLanguage = $("#defaultLanguage").val();
    var translationData = {};
    $(".translation-keyvalue").map((index, elem) => {
        var translationKey = $(elem).find('.translation-key');
        var translationValue = $(elem).find('.translation-value textarea');
        translationData[translationKey.text().replaceAll(' ', '_').trim()] = translationValue.val().trim();
    });
    if (translationData.length == 0) {
        errorToast(genericErrorMessage());
        return;
    }
    var userCanDelete = $(".translation-delete").length > 0;
    Swal.fire({
        title: 'Save Translation',
        html: `
                                    <input type="text" id="translationFileName" class="swal2-input" placeholder="Translation Name" value="${currentLanguage}" onkeydown="handleSwalEnter(event)">
                                    `,
        confirmButtonText: 'Save',
        focusConfirm: false,
        preConfirm: () => {
            const translationFileName = $("#translationFileName").val();
            if (!translationFileName || translationFileName.trim() == '') {
                Swal.showValidationMessage(`Please enter a valid file name`);
            } else if (translationFileName.trim() == 'en_US' && !userCanDelete) {
                Swal.showValidationMessage(`en_US is reserved, please enter a different name`);
            }
            return { translationFileName }
        },
    }).then(function (result) {
        if (result.isConfirmed) {
            createAndUploadTranslation(result.value.translationFileName, translationData);
        }
    });
}
function exportTranslation(){
    var translationData = {};
    $(".translation-keyvalue").map((index, elem) => {
        var translationKey = $(elem).find('.translation-key');
        var translationValue = $(elem).find('.translation-value textarea');
        translationData[translationKey.text().replaceAll(' ', '_').trim()] = translationValue.val().trim();
    });
    if (translationData.length == 0) {
        errorToast(genericErrorMessage());
        return;
    }
    createAndExportTranslation(translationData);
}
function showTranslationDownloader() {
    $.get('/Home/GetAvailableTranslations', function(data){
        $('#translationDownloadModalContent').html(data);
        $('#translationDownloadModal').modal('show');
    })
}
function hideTranslationDownloader() {
    $('#translationDownloadModal').modal('hide');
}
function downloadTranslation(continent, name) {
    sloader.show();
    $.get(`/Home/DownloadTranslation?continent=${continent}&name=${name}`, function (data) {
        sloader.hide();
        if (data) {
            successToast("Translation Downloaded");
            updateSettings();
        } else {
            errorToast(genericErrorMessage());
        }
    })
}
function downloadAllTranslations() {
    sloader.show();
    $.get('/Home/DownloadAllTranslations', function (data) {
        sloader.hide();
        if (data.success) {
            successToast(data.message);
            updateSettings();
        } else {
            errorToast(data.message);
        }
    })
}
function deleteTranslationKey(e) {
    $(e).parent().parent().remove();
}
//tabs reorder
function showTabReorderModal() {
    //reorder the list items based on the CSS attribute
    var sortedOrderedTabs = $(".lubelog-tab-groups > li").toArray().sort((a, b) => {
        var currentVal = $(a).css("order");
        var nextVal = $(b).css("order");
        return currentVal - nextVal;
    });
    $(".lubelog-tab-groups").html(sortedOrderedTabs);
    $("#tabReorderModal").modal('show');
    bindTabReorderEvents();
}
function hideTabReorderModal() {
    $("#tabReorderModal").modal('hide');
}
var tabDraggedToReorder = undefined;
function handleTabDragStart(e) {
    tabDraggedToReorder = $(e.target).closest('.list-group-item');
    //clear out order attribute.
    $(".lubelog-tab-groups > li").map((index, elem) => {
        $(elem).css('order', 0);
    })
}
function handleTabDragOver(e) {
    if (tabDraggedToReorder == undefined || tabDraggedToReorder == "") {
        return;
    }
    var potentialDropTarget = $(e.target).closest('.list-group-item').attr("data-tab");
    var draggedTarget = tabDraggedToReorder.closest('.list-group-item').attr("data-tab");
    if (draggedTarget != potentialDropTarget) {
        var targetObj = $(e.target).closest('.list-group-item');
        var draggedOrder = tabDraggedToReorder.index();
        var targetOrder = targetObj.index();
        if (draggedOrder < targetOrder) {
            tabDraggedToReorder.insertAfter(targetObj);
        } else {
            tabDraggedToReorder.insertBefore(targetObj);
        }
    }
    else {
        event.preventDefault();
    }
}
function bindTabReorderEvents() {
    $(".lubelog-tab-groups > li").on('dragstart', event => {
        handleTabDragStart(event);
    });
    $(".lubelog-tab-groups > li").on('dragover', event => {
        handleTabDragOver(event);
    });
    $(".lubelog-tab-groups > li").on('dragend', event => {
        //reset order attribute
        $(".lubelog-tab-groups > li").map((index, elem) => {
            $(elem).css('order', $(elem).index());
        })
    });
}
function getTabOrder() {
    var tabOrderArray = [];
    //check if any tabs have -1 order
    var resetTabs = $(".lubelog-tab-groups > li").filter((index, elem) => $(elem).css('order') == -1).length > 0;
    if (resetTabs) {
        return tabOrderArray; //return empty array.
    }
    var sortedOrderedTabs = $(".lubelog-tab-groups > li").toArray().sort((a, b) => {
        var currentVal = $(a).css("order");
        var nextVal = $(b).css("order");
        return currentVal - nextVal;
    });
    sortedOrderedTabs.map(elem => {
        var elemName = $(elem).attr("data-tab");
        tabOrderArray.push(elemName);
    });
    return tabOrderArray;
}
function resetTabOrder() {
    //set all orders to -1
    $(".lubelog-tab-groups > li").map((index, elem) => {
        $(elem).css('order', -1);
    })
    updateSettings();
}

function hideCustomWidgets() {
    $("#customWidgetModal").modal('hide');
}
function saveCustomWidgets() {
    $.post('/Home/SaveCustomWidgets', { widgetsData: $("#widgetEditor").val() }, function (data) {
        if (data) {
            successToast("Custom Widgets Saved!");
            updateSettings();
        } else {
            errorToast(genericErrorMessage());
        }
    })
}
function deleteCustomWidgets() {
    $.post('/Home/DeleteCustomWidgets', function (data) {
        if (data) {
            successToast("Custom Widgets Deleted!");
            updateSettings();
        } else {
            errorToast(genericErrorMessage());
        }
    })
}
function saveCustomWidgetsAcknowledgement() {
    sessionStorage.setItem('customWidgetsAcknowledged', true);
}
function getCustomWidgetsAcknowledgement() {
    let storedItem = sessionStorage.getItem('customWidgetsAcknowledged');
    if (storedItem == null || storedItem == undefined) {
        return false;
    } else {
        return storedItem;
    }
}
function showCustomWidgets() {
    let acknowledged = getCustomWidgetsAcknowledgement();
    if (acknowledged) {
        $.get('/Home/GetCustomWidgetEditor', function (data) {
            if (data.trim() != '') {
                $("#customWidgetModalContent").html(data);
                $("#customWidgetModal").modal('show');
            } else {
                errorToast("Custom Widgets Not Enabled");
            }
        });
        return;
    }
    Swal.fire({
        title: 'Warning',
        icon: "warning",
        html: `
               <span>
               You are about to use the Custom Widgets Editor, this is a developer-focused feature that can lead to security vulnerabilities if you don't understand what you're doing.
               <br />Zero support will be provided from the developer(s) of LubeLogger regarding Custom Widgets, Read the Documentation.
               <br />By proceeding, you acknowledge that you are solely responsible for all consequences from utilizing the Custom Widgets Editor.
               <br />To proceed, enter 'acknowledge' into the text field below.
               </span>
               <input type="text" id="inputAcknowledge" class="swal2-input" placeholder="acknowledge" onkeydown="handleSwalEnter(event)">
              `,
        confirmButtonText: 'Proceed',
        focusConfirm: false,
        preConfirm: () => {
            const userAcknowledge = $("#inputAcknowledge").val();
            if (!userAcknowledge || userAcknowledge != 'acknowledge') {
                Swal.showValidationMessage(`Please acknowledge before proceeding.`)
            }
            return { userAcknowledge }
        },
    }).then(function (result) {
        if (result.isConfirmed) {
            saveCustomWidgetsAcknowledgement();
            $.get('/Home/GetCustomWidgetEditor', function (data) {
                if (data.trim() != '') {
                    $("#customWidgetModalContent").html(data);
                    $("#customWidgetModal").modal('show');
                } else {
                    errorToast("Custom Widgets Not Enabled");
                }
            });
        }
    });
}