function returnToGarage() {
    window.location.href = '/Home';
}
$(document).ready(function () {
    var vehicleId = GetVehicleId().vehicleId;
    //bind tabs
    $('button[data-bs-toggle="tab"]').on('show.bs.tab', function (e) {
        switch (e.target.id) {
            case "servicerecord-tab":
                getVehicleServiceRecords(vehicleId);
                break;
            case "notes-tab":
                getVehicleNotes(vehicleId);
                break;
            case "gas-tab":
                getVehicleGasRecords(vehicleId);
                break;
            case "accident-tab":
                getVehicleCollisionRecords(vehicleId);
                break;
            case "tax-tab":
                getVehicleTaxRecords(vehicleId);
                break;
            case "report-tab":
                getVehicleReport(vehicleId);
                break;
            case "reminder-tab":
                getVehicleReminders(vehicleId);
                break;
            case "upgrade-tab":
                getVehicleUpgradeRecords(vehicleId);
                break;
            case "supply-tab":
                getVehicleSupplyRecords(vehicleId);
                break;
            case "plan-tab":
                getVehiclePlanRecords(vehicleId);
                break;
            case "odometer-tab":
                getVehicleOdometerRecords(vehicleId);
                break;
        }
        switch (e.relatedTarget.id) { //clear out previous tabs with grids in them to help with performance
            case "servicerecord-tab":
                $("#servicerecord-tab-pane").html("");
                break;
            case "gas-tab":
                $("#gas-tab-pane").html("");
                break;
            case "accident-tab":
                $("#accident-tab-pane").html("");
                break;
            case "tax-tab":
                $("#tax-tab-pane").html("");
                break;
            case "report-tab":
                $("#report-tab-pane").html("");
                break;
            case "reminder-tab":
                $("#reminder-tab-pane").html("");
                break;
            case "upgrade-tab":
                $("#upgrade-tab-pane").html("");
                break;
            case "notes-tab":
                $("#notes-tab-pane").html("");
                break;
            case "supply-tab":
                $("#supply-tab-pane").html("");
                break;
            case "plan-tab":
                $("#plan-tab-pane").html("");
                break;
            case "odometer-tab":
                $("#odometer-tab-pane").html("");
                break;
        }
        $(`.lubelogger-tab #${e.target.id}`).addClass('active');
        $(`.lubelogger-mobile-nav #${e.target.id}`).addClass('active');
        $(`.lubelogger-tab #${e.relatedTarget.id}`).removeClass('active');
        $(`.lubelogger-mobile-nav #${e.relatedTarget.id}`).removeClass('active');
    });
    var defaultTab = GetDefaultTab().tab;
    switch (defaultTab) {
        case "ServiceRecord":
            getVehicleServiceRecords(vehicleId);
            break;
        case "NoteRecord":
            getVehicleNotes(vehicleId);
            break;
        case "GasRecord":
            getVehicleGasRecords(vehicleId);
            break;
        case "RepairRecord":
            getVehicleCollisionRecords(vehicleId);
            break;
        case "TaxRecord":
            getVehicleTaxRecords(vehicleId);
            break;
        case "Dashboard":
            getVehicleReport(vehicleId);
            break;
        case "ReminderRecord":
            getVehicleReminders(vehicleId);
            break;
        case "UpgradeRecord":
            getVehicleUpgradeRecords(vehicleId);
            break;
        case "SupplyRecord":
            getVehicleSupplyRecords(vehicleId);
            break;
        case "PlanRecord":
            getVehiclePlanRecords(vehicleId);
            break;
        case "OdometerRecord":
            getVehicleOdometerRecords(vehicleId);
            break;
    }
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
                if (data) {
                    window.location.href = '/Home';
                }
            })
        }
    });
}
function showAddReminderModal(reminderModalInput) {
    if (reminderModalInput != undefined) {
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
    var friendlySource = "";
    var friendlyDest = "";
    var hideModalCallBack;
    var refreshDataCallBack;
    switch (source) {
        case "ServiceRecord":
            friendlySource = "Service Records";
            hideModalCallBack = hideAddServiceRecordModal;
            refreshDataCallBack = getVehicleServiceRecords;
            break;
        case "RepairRecord":
            friendlySource = "Repairs";
            hideModalCallBack = hideAddCollisionRecordModal;
            refreshDataCallBack = getVehicleCollisionRecords;
            break;
        case "UpgradeRecord":
            friendlySource = "Upgrades";
            hideModalCallBack = hideAddUpgradeRecordModal;
            refreshDataCallBack = getVehicleUpgradeRecords;
            break;
    }
    switch (dest) {
        case "ServiceRecord":
            friendlyDest = "Service Records";
            break;
        case "RepairRecord":
            friendlyDest = "Repairs";
            break;
        case "UpgradeRecord":
            friendlyDest = "Upgrades";
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
                if (data) {
                    hideModalCallBack();
                    successToast("Record Moved");
                    var vehicleId = GetVehicleId().vehicleId;
                    refreshDataCallBack(vehicleId);
                } else {
                    errorToast(genericErrorMessage());
                }
            });
        } else {
            $("#workAroundInput").hide();
        }
    });
}
function showRecurringReminderSelector(descriptionFieldName) {
    $.get(`/Vehicle/GetRecurringReminderRecordsByVehicleId?vehicleId=${GetVehicleId().vehicleId}`, function (data) {
        if (data) {
            //prompt user to select a recurring reminder
            Swal.fire({
                title: 'Select Recurring Reminder',
                html: data,
                confirmButtonText: 'Select',
                focusConfirm: false,
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
                    var descriptionField = $(`#${descriptionFieldName}`);
                    if (descriptionField.length > 0) {
                        descriptionField.val(result.value.selectedRecurringReminderData.text);
                    }
                }
            });
        } else {
            errorToast(genericErrorMessage());
        }
    })
}
function editMultipleRecords(ids, dataType) {
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
    var refreshDataCallBack;
    switch (formValues.dataType) {
        case "ServiceRecord":
            refreshDataCallBack = getVehicleServiceRecords;
            break;
        case "RepairRecord":
            refreshDataCallBack = getVehicleCollisionRecords;
            break;
        case "UpgradeRecord":
            refreshDataCallBack = getVehicleUpgradeRecords;
            break;
    }
    //save to db.
    $.post('/Vehicle/EditMultipleRecords', { genericRecordEditModel: formValues }, function (data) {
        if (data) {
            successToast(formValues.recordIds.length > 1 ? "Records Updated" : "Record Updated.");
            hideGenericRecordModal();
            refreshDataCallBack(GetVehicleId().vehicleId);
        } else {
            errorToast(genericErrorMessage());
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
            tags: genericTags
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
    var friendlySource = "";
    var refreshDataCallBack;
    var recordVerbiage = ids.length > 1 ? `these ${ids.length} records` : "this record";
    switch (source) {
        case "ServiceRecord":
            friendlySource = "Service Records";
            refreshDataCallBack = getVehicleServiceRecords;
            break;
        case "RepairRecord":
            friendlySource = "Repairs";
            refreshDataCallBack = getVehicleCollisionRecords;
            break;
        case "UpgradeRecord":
            friendlySource = "Upgrades";
            refreshDataCallBack = getVehicleUpgradeRecords;
            break;
        case "OdometerRecord":
            friendlySource = "Odometer Records";
            refreshDataCallBack = getVehicleOdometerRecords;
            break;
        case "GasRecord":
            friendlySource = "Fuel Records";
            refreshDataCallBack = getVehicleGasRecords;
            break;
    }

    Swal.fire({
        title: "Adjust Odometer?",
        text: `Apply Odometer Adjustments to ${recordVerbiage}?`,
        showCancelButton: true,
        confirmButtonText: "Adjust",
        confirmButtonColor: "#dc3545"
    }).then((result) => {
        if (result.isConfirmed) {
            $.post('/Vehicle/AdjustRecordsOdometer', { recordIds: ids, vehicleId: GetVehicleId().vehicleId, importMode: source }, function (data) {
                if (data) {
                    successToast(`${ids.length} Record(s) Updated`);
                    var vehicleId = GetVehicleId().vehicleId;
                    refreshDataCallBack(vehicleId);
                } else {
                    errorToast(genericErrorMessage());
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
                text: $(this).parent().find('.form-check-label').text()
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
                text: selectedRecurringRemindersArray.map(x=>x.text).join(', ')
            }
        }
    } else {
        //validate single reminder
        var selectedRecurringReminder = $("#recurringReminderInput").val();
        var selectedRecurringReminderText = $("#recurringReminderInput option:selected").text();
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
                text: selectedRecurringReminderText
            }
        }
    }
}
function getLastOdometerReadingAndIncrement(odometerFieldName) {
    Swal.fire({
        title: 'Increment Last Reported Odometer Reading',
        html: `
                            <input type="text" id="inputOdometerIncrement" class="swal2-input" placeholder="Increment">
              `,
        confirmButtonText: 'Add',
        focusConfirm: false,
        preConfirm: () => {
            const odometerIncrement = parseInt(globalParseFloat($("#inputOdometerIncrement").val()));
            if (isNaN(odometerIncrement) || odometerIncrement <= 0) {
                Swal.showValidationMessage(`Please enter a non-zero amount to increment`);
            }
            return { odometerIncrement }
        },
    }).then(function (result) {
        if (result.isConfirmed) {
            var amountToIncrement = result.value.odometerIncrement;
            $.get(`/Vehicle/GetMaxMileage?vehicleId=${GetVehicleId().vehicleId}`, function (data) {
                var newAmount = data + amountToIncrement;
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
            });
        }
    });
}