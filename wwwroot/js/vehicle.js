$(function () {
    var vehicleId = GetVehicleId().vehicleId;
    //bind tabs
    $('button[data-bs-toggle="tab"]').on('show.bs.tab', function (e) {
        let dataCallBack = getImportModeData(e.target.id).dataCallBack;
        dataCallBack(vehicleId);
        $(`.lubelogger-tab #${e.target.id}`).addClass('active');
        $(`.lubelogger-mobile-nav #${e.target.id}`).addClass('active');
        if (e.relatedTarget != null) {
            let prevTabPaneName = getImportModeData(e.relatedTarget.id).tabPaneName;
            $(`#${prevTabPaneName}`).html(""); //clear out previous tabs with grids in them to help with performance
            $(`.lubelogger-tab #${e.relatedTarget.id}`).removeClass('active');
            $(`.lubelogger-mobile-nav #${e.relatedTarget.id}`).removeClass('active');
        }
        setBrowserHistory('tab', getTabNameForURL(e.target.id));
    });
    loadDefaultTab();
});

function getVehicleNotes(vehicleId) {
    $.get(`/Vehicle/GetNotesByVehicleId?vehicleId=${vehicleId}`, function (data) {
        if (data) {
            $("#notes-tab-pane").html(data);
            restoreScrollPosition();
        }
    });
}
function getVehicleServiceRecords(vehicleId) {
    $.get(`/Vehicle/GetServiceRecordsByVehicleId?vehicleId=${vehicleId}`, function (data) {
        if (data) {
            $("#servicerecord-tab-pane").html(data);
            restoreScrollPosition();
            getVehicleHaveImportantReminders(vehicleId);
        }
    });
}
function getVehiclePlanRecords(vehicleId) {
    $.get(`/Vehicle/GetPlanRecordsByVehicleId?vehicleId=${vehicleId}`, function (data) {
        if (data) {
            $("#plan-tab-pane").html(data);
            restoreScrollPosition();
            getVehicleHaveImportantReminders(vehicleId);
        }
    });
}
function getVehicleOdometerRecords(vehicleId) {
    $.get(`/Vehicle/GetOdometerRecordsByVehicleId?vehicleId=${vehicleId}`, function (data) {
        if (data) {
            $("#odometer-tab-pane").html(data);
            restoreScrollPosition();
            getVehicleHaveImportantReminders(vehicleId);
        }
    });
}
function getVehicleSupplyRecords(vehicleId) {
    $.get(`/Vehicle/GetSupplyRecordsByVehicleId?vehicleId=${vehicleId}`, function (data) {
        if (data) {
            $("#supply-tab-pane").html(data);
            restoreScrollPosition();
            getVehicleHaveImportantReminders(vehicleId);
        }
    });
}
function getVehicleUpgradeRecords(vehicleId) {
    $.get(`/Vehicle/GetUpgradeRecordsByVehicleId?vehicleId=${vehicleId}`, function (data) {
        if (data) {
            $("#upgrade-tab-pane").html(data);
            restoreScrollPosition();
            getVehicleHaveImportantReminders(vehicleId);
        }
    });
}
function getVehicleGasRecords(vehicleId) {
    $.get(`/Vehicle/GetGasRecordsByVehicleId?vehicleId=${vehicleId}`, function (data) {
        if (data) {
            $("#gas-tab-pane").html(data);
            restoreScrollPosition();
            getVehicleHaveImportantReminders(vehicleId);
        }
    });
}
function getVehicleCollisionRecords(vehicleId) {
    $.get(`/Vehicle/GetCollisionRecordsByVehicleId?vehicleId=${vehicleId}`, function (data) {
        if (data) {
            $("#accident-tab-pane").html(data);
            restoreScrollPosition();
            getVehicleHaveImportantReminders(vehicleId);
        }
    });
}
function getVehicleTaxRecords(vehicleId) {
    $.get(`/Vehicle/GetTaxRecordsByVehicleId?vehicleId=${vehicleId}`, function (data) {
        if (data) {
            $("#tax-tab-pane").html(data);
            restoreScrollPosition();
            getVehicleHaveImportantReminders(vehicleId);
        }
    });
}
function getVehicleReminders(vehicleId) {
    $.get(`/Vehicle/GetReminderRecordsByVehicleId?vehicleId=${vehicleId}`, function (data) {
        if (data) {
            $("#reminder-tab-pane").html(data);
            restoreScrollPosition();
            getVehicleHaveImportantReminders(vehicleId);
        }
    });
}
function getVehicleInspectionRecords(vehicleId) {
    $.get(`/Vehicle/GetInspectionRecordsByVehicleId?vehicleId=${vehicleId}`, function (data) {
        if (data) {
            $("#inspection-tab-pane").html(data);
            restoreScrollPosition();
            getVehicleHaveImportantReminders(vehicleId);
        }
    });
}
function getVehicleEquipmentRecords(vehicleId) {
    $.get(`/Vehicle/GetEquipmentRecordsByVehicleId?vehicleId=${vehicleId}`, function (data) {
        if (data) {
            $("#equipment-tab-pane").html(data);
            restoreScrollPosition();
            getVehicleHaveImportantReminders(vehicleId);
        }
    });
}
function getVehicleReport(vehicleId) {
    $.get(`/Vehicle/GetReportPartialView?vehicleId=${vehicleId}`, function (data) {
        if (data) {
            $("#report-tab-pane").html(data);
            getVehicleHaveImportantReminders(vehicleId);
        }
    })
}
function editVehicle(vehicleId) {
    $.get(`/Vehicle/GetEditVehiclePartialViewById?vehicleId=${vehicleId}`, function (data) {
        if (data) {
            $("#editVehicleModalContent").html(data);
            initTagSelector($("#inputTag"), true);
            initDatePicker($('#inputPurchaseDate'));
            initDatePicker($('#inputSoldDate'));
            $('#editVehicleModal').modal('show');
        }
    });
}
function hideEditVehicleModal() {
    $('#editVehicleModal').modal('hide');
}
function deleteVehicle(vehicleId) {
    Swal.fire({
        title: "Confirm Deletion?",
        text: "This will also delete all data tied to this vehicle. Deleted Vehicles and their associated data cannot be restored.",
        showCancelButton: true,
        confirmButtonText: "Delete",
        confirmButtonColor: "#dc3545"
    }).then((result) => {
        if (result.isConfirmed) {
            $.post('/Vehicle/DeleteVehicle', { vehicleId: vehicleId }, function (data) {
                if (data.success) {
                    window.location.href = '/Home';
                } else {
                    errorToast(data.message);
                }
            })
        }
    });
}
function showAddReminderModal(reminderModalInput) {
    if (reminderModalInput != undefined) {
        reminderModalInput['createdFromRecord'] = true;
        $.post('/Vehicle/GetAddReminderRecordPartialView', { reminderModel: reminderModalInput }, function (data) {
            $("#reminderRecordModalContent").html(data);
            initDatePicker($('#reminderDate'), true);
            initTagSelector($("#reminderRecordTag"));
            $("#reminderRecordModal").modal("show");
        });
    } else {
        $.post('/Vehicle/GetAddReminderRecordPartialView', function (data) {
            $("#reminderRecordModalContent").html(data);
            initDatePicker($('#reminderDate'), true);
            initTagSelector($("#reminderRecordTag"));
            $("#reminderRecordModal").modal("show");
        });
    }
}
function getVehicleHaveImportantReminders(vehicleId) {
    setTimeout(function () {
        $.get(`/Vehicle/GetVehicleHaveUrgentOrPastDueReminders?vehicleId=${vehicleId}`, function (data) {
            if (data) {
                $(".reminderBell").removeClass("bi-bell");
                $(".reminderBell").addClass("bi-bell-fill");
                $(".reminderBell").addClass("text-warning");
                $(".reminderBellDiv").addClass("bell-shake");
            } else {
                $(".reminderBellDiv").removeClass("bell-shake");
                $(".reminderBell").removeClass("bi-bell-fill");
                $(".reminderBell").addClass("bi-bell");
                $(".reminderBell").removeClass("text-warning");
            }
        });
    }, 500);
}
function moveRecord(recordId, source, dest) {
    $("#workAroundInput").show();
    var friendlySource = getImportModeData(source).friendlyName;
    var friendlyDest = getImportModeData(dest).friendlyName;
    var hideModalCallBack;
    var refreshDataCallBack = getImportModeData(source).dataCallBack;
    switch (source) {
        case "ServiceRecord":
            hideModalCallBack = hideAddServiceRecordModal;
            break;
        case "RepairRecord":
            hideModalCallBack = hideAddCollisionRecordModal;
            break;
        case "UpgradeRecord":
            hideModalCallBack = hideAddUpgradeRecordModal;
            break;
    }
    Swal.fire({
        title: "Confirm Move?",
        text: `Move this record from ${friendlySource} to ${friendlyDest}?`,
        showCancelButton: true,
        confirmButtonText: "Move",
        confirmButtonColor: "#dc3545"
    }).then((result) => {
        if (result.isConfirmed) {
            $.post('/Vehicle/MoveRecord', { recordId: recordId, source: source, destination: dest }, function (data) {
                if (data.success) {
                    hideModalCallBack();
                    successToast("Record Moved");
                    var vehicleId = GetVehicleId().vehicleId;
                    refreshDataCallBack(vehicleId);
                } else {
                    errorToast(data.message);
                    $("#workAroundInput").hide();
                }
            });
        } else {
            $("#workAroundInput").hide();
        }
    });
}
function loadSelectedRecurringReminder() {
    if (recurringReminderRecordId != undefined && recurringReminderRecordId.length > 0) {
        if (recurringReminderRecordId.length > 1) {
            //multiple reminders
            $('#multipleRemindersCheck').prop('checked', true);
            $('#multipleRemindersCheck').trigger('change');
            recurringReminderRecordId.map(x => {
                $(`#recurringReminder_${x}`).prop('checked', true);
            });
        }
        else if (recurringReminderRecordId.length == 1) {
            $("#recurringReminderInput").val(recurringReminderRecordId[0]);
        }
    }
}
function showRecurringReminderSelector(descriptionFieldName, noteFieldName) {
    $.get(`/Vehicle/GetRecurringReminderRecordsByVehicleId?vehicleId=${GetVehicleId().vehicleId}`, function (data) {
        if (data) {
            //prompt user to select a recurring reminder
            Swal.fire({
                title: 'Select Recurring Reminder',
                html: data,
                confirmButtonText: 'Select',
                focusConfirm: false,
                didRender: () => {
                    loadSelectedRecurringReminder();
                },
                preConfirm: () => {
                    //validate
                    var selectedRecurringReminderData = getAndValidateSelectedRecurringReminder();
                    if (selectedRecurringReminderData.hasError) {
                        Swal.showValidationMessage(`You must select a recurring reminder`);
                    }
                    return { selectedRecurringReminderData }
                },
            }).then(function (result) {
                if (result.isConfirmed) {
                    recurringReminderRecordId = result.value.selectedRecurringReminderData.ids;
                    let descriptionField = $(`#${descriptionFieldName}`);
                    let noteField = $(`#${noteFieldName}`);
                    if (descriptionField.length > 0) {
                        let descriptionFieldText = result.value.selectedRecurringReminderData.text.join(', ');
                        descriptionField.val(descriptionFieldText);
                    }
                    if (noteField.length > 0 && result.value.selectedRecurringReminderData.text.length > 1) {
                        result.value.selectedRecurringReminderData.text.map(x => {
                            noteField.append(`- ${x}\r\n`);
                        });
                    }
                }
            });
        } else {
            errorToast(genericErrorMessage());
        }
    })
}
function editMultipleRecords(ids, dataType) {
    if (ids.length < 2) {
        return;
    }
    $.post('/Vehicle/GetGenericRecordModal', { recordIds: ids, dataType: dataType }, function (data) {
        if (data) {
            $("#genericRecordEditModalContent").html(data);
            initDatePicker($('#genericRecordDate'));
            initTagSelector($("#genericRecordTag"));
            $("#genericRecordEditModal").modal('show');
        }
    });
}
function hideGenericRecordModal() {
    $("#genericRecordEditModal").modal('hide');
}
function saveGenericRecord() {
    //get values
    var formValues = getAndValidateGenericRecordValues();
    //validate
    if (formValues.hasError) {
        errorToast("Please check the form data");
        return;
    }
    var refreshDataCallBack = getImportModeData(formValues.dataType).dataCallBack;
    //save to db.
    $.post('/Vehicle/EditMultipleRecords', { genericRecordEditModel: formValues }, function (data) {
        if (data.success) {
            successToast(formValues.recordIds.length > 1 ? "Records Updated" : "Record Updated.");
            hideGenericRecordModal();
            refreshDataCallBack(GetVehicleId().vehicleId);
        } else {
            errorToast(data.message);
        }
    })
}
function getAndValidateGenericRecordValues() {
    var genericDate = $("#genericRecordDate").val();
    var genericMileage = $("#genericRecordMileage").val();
    var genericMileageToParse = parseInt(globalParseFloat($("#genericRecordMileage").val())).toString();
    var genericDescription = $("#genericRecordDescription").val();
    var genericCost = $("#genericRecordCost").val();
    var genericNotes = $("#genericRecordNotes").val();
    var genericTags = $("#genericRecordTag").val();
    var genericExtraFields = getAndValidateExtraFields();
    //validation
    var hasError = false;
    if (genericMileage.trim() != '' && (isNaN(genericMileageToParse) || parseInt(genericMileageToParse) < 0)) {
        hasError = true;
        $("#genericRecordMileage").addClass("is-invalid");
    } else {
        $("#genericRecordMileage").removeClass("is-invalid");
    }
    if (genericCost.trim() != '' && !isValidMoney(genericCost)) {
        hasError = true;
        $("#genericRecordCost").addClass("is-invalid");
    } else {
        $("#genericRecordCost").removeClass("is-invalid");
    }
    return {
        hasError: hasError,
        dataType: getGenericRecordEditModelData().dataType,
        recordIds: recordsToEdit,
        editRecord: {
            date: genericDate,
            mileage: genericMileageToParse,
            description: genericDescription,
            cost: genericCost,
            notes: genericNotes,
            tags: genericTags,
            extraFields: genericExtraFields.extraFields
        }
    }
}
function getRecordsDeltaStats(recordIds) {
    if (recordIds.length < 2) {
        return;
    }
    var odometerReadings = [];
    var dateReadings = [];
    var costReadings = [];
    //get all of the odometer readings
    recordIds.map(x => {
        var odometerReading = parseInt($(`tr[data-rowId='${x}'] td[data-column='odometer']`).text());
        if (!isNaN(odometerReading)) {
            odometerReadings.push(odometerReading);
        }
        var dateReading = parseInt($(`tr[data-rowId=${x}] td[data-column='date']`).attr('data-date'));
        if (!isNaN(dateReading)) {
            dateReadings.push(dateReading);
        }
        var costReading = globalParseFloat($(`tr[data-rowId='${x}'] td[data-column='cost']`).text());
        if (costReading > 0) {
            costReadings.push(costReading);
        }
    });
    //get max stats
    var maxOdo = odometerReadings.length > 0 ? odometerReadings.reduce((a, b) => a > b ? a : b) : 0;
    var maxDate = dateReadings.length > 0 ? dateReadings.reduce((a, b) => a > b ? a : b) : 0;
    //get min stats
    var minOdo = odometerReadings.length > 0 ? odometerReadings.reduce((a, b) => a < b ? a : b) : 0;
    var minDate = dateReadings.length > 0 ? dateReadings.reduce((a, b) => a < b ? a : b) : 0;
    //get sum of costs
    var costSum = costReadings.length > 0 ? costReadings.reduce((a, b) => a + b) : 0;
    var diffOdo = maxOdo - minOdo;
    var diffDate = maxDate - minDate;
    var divisibleCount = recordIds.length - 1;
    var averageOdo = diffOdo > 0 ? (diffOdo / divisibleCount).toFixed(2) : '0';
    var averageDays = diffDate > 0 ? Math.floor((diffDate / divisibleCount) / 8.64e7) : '0';
    var averageSum = costSum > 0 ? (costSum / recordIds.length).toFixed(2) : '0';
    costSum = costSum.toFixed(2);
    Swal.fire({
        title: "Record Statistics",
        html: `<p>Average Distance Traveled between Records: ${globalFloatToString(averageOdo)}</p>
                <br />
                <p>Average Days between Records: ${averageDays}</p>
                <br />
                <p>Total Cost: ${globalAppendCurrency(globalFloatToString(costSum))}</p>
                <br />
                <p>Average Cost: ${globalAppendCurrency(globalFloatToString(averageSum))}</p>`
        ,
        icon: "info"
    });
}
function GetAdjustedOdometer(id, odometerInput) {
    //if editing an existing record or vehicle does not have odometer adjustment or input is NaN then just return the original input.
    if (id > 0 || !GetVehicleId().hasOdometerAdjustment || isNaN(odometerInput)) {
        return odometerInput;
    }
    //apply odometer adjustments first.
    var adjustedOdometer = parseInt(odometerInput) + parseInt(GetVehicleId().odometerDifference);
    //apply odometer multiplier.
    adjustedOdometer *= globalParseFloat(GetVehicleId().odometerMultiplier);
    return adjustedOdometer.toFixed(0);
}
function adjustRecordsOdometer(ids, source) {
    if (ids.length == 0) {
        return;
    }
    $("#workAroundInput").show();
    var refreshDataCallBack = getImportModeData(source).dataCallBack;
    var recordVerbiage = ids.length > 1 ? `these ${ids.length} records` : "this record";

    Swal.fire({
        title: "Adjust Odometer?",
        text: `Apply Odometer Adjustments to ${recordVerbiage}?`,
        showCancelButton: true,
        confirmButtonText: "Adjust",
        confirmButtonColor: "#dc3545"
    }).then((result) => {
        if (result.isConfirmed) {
            saveScrollPosition();
            $.post('/Vehicle/AdjustRecordsOdometer', { recordIds: ids, vehicleId: GetVehicleId().vehicleId, importMode: source }, function (data) {
                if (data.success) {
                    successToast(`${ids.length} Record(s) Updated`);
                    var vehicleId = GetVehicleId().vehicleId;
                    refreshDataCallBack(vehicleId);
                } else {
                    errorToast(data.message);
                    $("#workAroundInput").hide();
                }
            });
        } else {
            $("#workAroundInput").hide();
        }
    });
}
function showMultipleRemindersSelector() {
    if ($("#multipleRemindersCheck").is(":checked")) {
        $("#recurringMultipleReminders").show();
        $("#recurringReminderInput").hide();
    } else {
        $("#recurringMultipleReminders").hide();
        $("#recurringReminderInput").show();
    }
}
function getAndValidateSelectedRecurringReminder() {
    if ($("#multipleRemindersCheck").is(":checked")) {
        //validate multiple reminders
        var selectedRecurringRemindersArray = [];
        $("#recurringMultipleReminders :checked").map(function () {
            selectedRecurringRemindersArray.push({
                value: this.value,
                text: $(this).attr("data-description")
            });
        });
        if (selectedRecurringRemindersArray.length == 0) {
            return {
                hasError: true,
                ids: [],
                text: ''
            }
        } else {
            return {
                hasError: false,
                ids: selectedRecurringRemindersArray.map(x=>x.value),
                text: selectedRecurringRemindersArray.map(x=>x.text) 
            }
        }
    } else {
        //validate single reminder
        var selectedRecurringReminder = $("#recurringReminderInput").val();
        var selectedRecurringReminderText = $("#recurringReminderInput option:selected").attr("data-description");
        if (!selectedRecurringReminder || parseInt(selectedRecurringReminder) == 0) {
            return {
                hasError: true,
                ids: [],
                text: ''
            }
        } else {
            return {
                hasError: false,
                ids: [selectedRecurringReminder],
                text: [selectedRecurringReminderText]
            }
        }
    }
}
function getLastOdometerReadingAndIncrement(odometerFieldName) {
    $.get(`/Vehicle/GetMaxMileage?vehicleId=${GetVehicleId().vehicleId}`, function (currentOdometer) {
        let additionalHtml = isNaN(currentOdometer) || currentOdometer == 0 ? '' : `<span>Current Odometer: ${currentOdometer}</span><br/>`;
        Swal.fire({
            title: 'Increment Last Reported Odometer Reading',
            html: `${additionalHtml}
                            <input type="text" inputmode="decimal" id="inputOdometerIncrement" class="swal2-input" placeholder="Increment" onkeydown="handleSwalEnter(event)">
              `,
            confirmButtonText: 'Add',
            focusConfirm: false,
            preConfirm: () => {
                const odometerIncrement = parseInt(globalParseFloat($("#inputOdometerIncrement").val()));
                if (isNaN(odometerIncrement) || odometerIncrement < 0) {
                    Swal.showValidationMessage(`Please enter a positive amount to increment or 0 to use current odometer`);
                }
                return { odometerIncrement }
            },
        }).then(function (result) {
            if (result.isConfirmed) {
                var amountToIncrement = result.value.odometerIncrement;
                    var newAmount = currentOdometer + amountToIncrement;
                    if (!isNaN(newAmount)) {
                        var odometerField = $(`#${odometerFieldName}`);
                        if (odometerField.length > 0) {
                            odometerField.val(newAmount);
                        } else {
                            errorToast(genericErrorMessage());
                        }
                    } else {
                        errorToast(genericErrorMessage());
                    }
            }
        });
    });
}

function showGlobalSearch() {
    $('#globalSearchModal').modal('show');
    restoreGlobalSearchSettings();
}
function hideGlobalSearch() {
    $('#globalSearchModal').modal('hide');
}
function saveGlobalSearchSettings() {
    let globalSearchSettings = {
        incrementalSearch: $('#globalSearchAutoSearchCheck').is(':checked'),
        caseSensitive: $('#globalSearchCaseSensitiveCheck').is(':checked')
    };
    localStorage.setItem('globalSearchSettings', JSON.stringify(globalSearchSettings));
}
function restoreGlobalSearchSettings() {
    let globalSearchSettings = localStorage.getItem('globalSearchSettings');
    if (globalSearchSettings != null) {
        let parsedGlobalSearchSettings = JSON.parse(globalSearchSettings);
        $('#globalSearchAutoSearchCheck').attr('checked', parsedGlobalSearchSettings.incrementalSearch);
        $('#globalSearchCaseSensitiveCheck').attr('checked', parsedGlobalSearchSettings.caseSensitive);
    }
}
function performGlobalSearch() {
    var searchQuery = $('#globalSearchInput').val();
    if (searchQuery.trim() == '') {
        $('#globalSearchInput').addClass('is-invalid');
    } else {
        $('#globalSearchInput').removeClass('is-invalid');
    }
    let caseSensitiveSearch = $("#globalSearchCaseSensitiveCheck").is(':checked');
    saveGlobalSearchSettings();
    $.post('/Vehicle/SearchRecords', { vehicleId: GetVehicleId().vehicleId, searchQuery: searchQuery, caseSensitive: caseSensitiveSearch }, function (data) {
        $('#globalSearchModalResults').html(data);
    });
}
function handleGlobalSearchKeyPress(event) {
    if ($('#globalSearchAutoSearchCheck').is(':checked')) {
        setDebounce(performGlobalSearch);
    } else if (event.keyCode == 13) {
        performGlobalSearch();
    }
}

function loadGlobalSearchResult(recordId, recordType) {
    hideGlobalSearch();
    $.post(`/Vehicle/CheckRecordExist?vehicleId=${GetVehicleId().vehicleId}&importMode=${recordType}&recordId=${recordId}`, function (data) {
        if (data.success) {
            let recordTypeData = getImportModeData(recordType);
            if ($(`#${recordTypeData.tabContainerName}`).hasClass('d-none')) {
                errorToast(`${recordType} Tab Not Enabled`);
                return;
            }
            $(`#${recordTypeData.tabContainerName}`).tab('show');
            waitForElement(`#${recordTypeData.modalContentName}`, recordTypeData.showModalCallBack, recordId);
        } else {
            errorToast(data.message);
        }
    })
}
function loadDefaultTab() {
    //check if tab param exists
    let userDefaultTab = getImportModeData(GetDefaultTab().tab).tabName;
    let tabFromURL = getTabNameFromURL(userDefaultTab);
    waitForElement(`#${tabFromURL}`, () => { $(`#${tabFromURL}`).tab('show'); }, '');
}