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
        $.post('/Vehicle/GetAddReminderRecordPartialView', {reminderModel: reminderModalInput}, function (data) {
            $("#reminderRecordModalContent").html(data);
            initDatePicker($('#reminderDate'), true);
            $("#reminderRecordModal").modal("show");
        });
    } else {
        $.post('/Vehicle/GetAddReminderRecordPartialView', function (data) {
            $("#reminderRecordModalContent").html(data);
            initDatePicker($('#reminderDate'), true);
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
            $.post('/Vehicle/MoveRecord', {recordId: recordId, source: source, destination: dest }, function (data) {
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
function moveRecords(ids, source, dest) {
    if (ids.length == 0) {
        return;
    }
    $("#workAroundInput").show();
    var friendlySource = "";
    var friendlyDest = "";
    var refreshDataCallBack;
    var recordVerbiage = selectedRow.length > 1 ? "these records" : "this record";
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
        text: `Move ${recordVerbiage} from ${friendlySource} to ${friendlyDest}?`,
        showCancelButton: true,
        confirmButtonText: "Move",
        confirmButtonColor: "#dc3545"
    }).then((result) => {
        if (result.isConfirmed) {
            $.post('/Vehicle/MoveRecords', { recordIds: ids, source: source, destination: dest }, function (data) {
                if (data) {
                    successToast("Records Moved");
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
function deleteRecords(ids, source) {
    if (ids.length == 0) {
        return;
    }
    $("#workAroundInput").show();
    var friendlySource = "";
    var refreshDataCallBack;
    var recordVerbiage = selectedRow.length > 1 ? "these records" : "this record";
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
        case "TaxRecord":
            friendlySource = "Taxes";
            refreshDataCallBack = getVehicleTaxRecords;
            break;
        case "SupplyRecord":
            friendlySource = "Supplies";
            refreshDataCallBack = getVehicleSupplyRecords;
            break;
        case "NoteRecord":
            friendlySource = "Notes";
            refreshDataCallBack = getVehicleNotes;
            break;
        case "OdometerRecord":
            friendlySource = "Odometer Records";
            refreshDataCallBack = getVehicleOdometerRecords;
            break;
        case "ReminderRecord":
            friendlySource = "Reminders";
            refreshDataCallBack = getVehicleReminders;
            break;
    }

    Swal.fire({
        title: "Confirm Delete?",
        text: `Delete ${recordVerbiage} from ${friendlySource}?`,
        showCancelButton: true,
        confirmButtonText: "Delete",
        confirmButtonColor: "#dc3545"
    }).then((result) => {
        if (result.isConfirmed) {
            $.post('/Vehicle/DeleteRecords', { recordIds: ids, importMode: source }, function (data) {
                if (data) {
                    successToast("Records Deleted");
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
var selectedRow = [];
var isDragging = false;
$(window).on('mouseup', function (e) {
    rangeMouseUp(e);
});
$(window).on('mousedown', function (e) {
    rangeMouseDown(e);
});
$(window).on('keydown', function (e) {
    var userOnInput = $(e.target).is("input") || $(e.target).is("textarea");
    if (!userOnInput) {
        if (e.ctrlKey && e.which == 65) {
            e.preventDefault();
            e.stopPropagation();
            clearSelectedRows();
            $('.vehicleDetailTabContainer .table tbody tr').addClass('table-active');
            $('.vehicleDetailTabContainer .table tbody tr').map((index, elem) => {
                addToSelectedRows($(elem).attr('data-rowId'));
            });
        }
    }
})
function rangeMouseDown(e) {
    if (isRightClick(e)) {
        return;
    }
    var contextMenuAction = $(e.target).is(".table-context-menu > li > .dropdown-item")
    if (!e.ctrlKey && !contextMenuAction) {
        clearSelectedRows();
    }
    isDragging = true;

    document.documentElement.onselectstart = function () { return false; };
}
function isRightClick(e) {
    if (e.which) {
        return (e.which == 3);
    } else if (e.button) {
        return (e.button == 2);
    }
    return false;
}
function rangeMouseUp(e) {
    if ($(".table-context-menu").length > 0) {
        $(".table-context-menu").hide();
    }
    if (isRightClick(e)) {
        return;
    }
    isDragging = false;
    document.documentElement.onselectstart = function () { return true; };
}
function rangeMouseMove(e) {
    if (isDragging) {
        if (!$(e).hasClass('table-active')) {
            addToSelectedRows($(e).attr('data-rowId'));
            $(e).addClass('table-active');
        }
    }
}
function addToSelectedRows(id) {
    if (selectedRow.findIndex(x=> x == id) == -1) {
        selectedRow.push(id);
    }
}
function removeFromSelectedRows(id) {
    var rowIndex = selectedRow.findIndex(x => x == id)
    if (rowIndex != -1) {
        selectedRow.splice(rowIndex, 1);
    }
}
function clearSelectedRows() {
    selectedRow = [];
    $('.table tr').removeClass('table-active');
}
function showTableContextMenu(e) {
    if (event != undefined) {
        event.preventDefault();
    }
    $(".table-context-menu").show();
    $(".table-context-menu").css({
        position: "absolute",
        left: getMenuPosition(event.clientX, 'width', 'scrollLeft'),
        top: getMenuPosition(event.clientY, 'height', 'scrollTop')
    });
    if (!$(e).hasClass('table-active')) {
        clearSelectedRows();
        addToSelectedRows($(e).attr('data-rowId'));
        $(e).addClass('table-active');
    }
}
function getMenuPosition(mouse, direction, scrollDir) {
    var win = $(window)[direction](),
        scroll = $(window)[scrollDir](),
        menu = $(".table-context-menu")[direction](),
        position = mouse + scroll;

    // opening menu would pass the side of the page
    if (mouse + menu > win && menu < mouse)
        position -= menu;
    return position;
}
function handleTableRowClick(e, callBack, rowId) {
    if (!event.ctrlKey) {
        callBack(rowId);
    } else if (!$(e).hasClass('table-active')) {
        addToSelectedRows($(e).attr('data-rowId'));
        $(e).addClass('table-active');
    } else if ($(e).hasClass('table-active')){
        removeFromSelectedRows($(e).attr('data-rowId'));
        $(e).removeClass('table-active');
    }
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
                    const selectedRecurringReminder = $("#recurringReminderInput").val();
                    const selectedRecurringReminderText = $("#recurringReminderInput").text();
                    if (!selectedRecurringReminder || parseInt(selectedRecurringReminder) == 0) {
                        Swal.showValidationMessage(`You must select a recurring reminder`);
                    }
                    return { selectedRecurringReminder, selectedRecurringReminderText }
                },
            }).then(function (result) {
                if (result.isConfirmed) {
                    recurringReminderRecordId = result.value.selectedRecurringReminder;
                    var descriptionField = $(`#${descriptionFieldName}`);
                    if (descriptionField.length > 0) {
                        descriptionField.val(result.value.selectedRecurringReminderText.trim());
                    }
                }
            });
        } else {
            errorToast(genericErrorMessage());
        }
    })
}