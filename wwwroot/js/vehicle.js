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
function exportVehicleData(mode) {
    var vehicleId = GetVehicleId().vehicleId;
    $.get('/Vehicle/ExportFromVehicleToCsv', { vehicleId: vehicleId, mode: mode }, function (data) {
        if (!data) {
            errorToast("An error occurred, please try again later");
        } else {
            window.location.href = data;
        }
    });
}
function showBulkImportModal(mode) {
    $.get(`/Vehicle/GetBulkImportModalPartialView?mode=${mode}`, function (data) {
        if (data) {
            $("#bulkImportModalContent").html(data);
            $("#bulkImportModal").modal('show');
        }
    })
}
function hideBulkImportModal(){
    $("#bulkImportModal").modal('hide');
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
function uploadVehicleFilesAsync(event) {
    let formData = new FormData();
    var files = event.files;
    for (var x = 0; x < files.length; x++) {
        formData.append("file", files[x]);
    }
    sloader.show();
    $.ajax({
        url: "/Files/HandleMultipleFileUpload",
        data: formData,
        cache: false,
        processData: false,
        contentType: false,
        type: 'POST',
        success: function (response) {
            sloader.hide();
            if (response.length > 0) {
                uploadedFiles.push.apply(uploadedFiles, response);
            }
        },
        error: function () {
            sloader.hide();
            errorToast("An error has occurred, please check the file size and try again later.")
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
function printTab() {
    setTimeout(function () {
        window.print();
    }, 500);
}
function deleteFileFromUploadedFiles(fileLocation, event) {
    event.parentElement.parentElement.parentElement.remove();
    uploadedFiles = uploadedFiles.filter(x => x.location != fileLocation);
}
function editFileName(fileLocation, event) {
    Swal.fire({
        title: 'Rename File',
        html: `
                    <input type="text" id="newFileName" class="swal2-input" placeholder="New File Name">
                    `,
        confirmButtonText: 'Rename',
        focusConfirm: false,
        preConfirm: () => {
            const newFileName = $("#newFileName").val();
            if (!newFileName) {
                Swal.showValidationMessage(`Please enter a valid file name`)
            }
            return { newFileName }
        },
    }).then(function (result) {
        if (result.isConfirmed) {
            var linkDisplayObject = $(event.parentElement.parentElement).find('a')[0];
            linkDisplayObject.text = result.value.newFileName;
            var editFileIndex = uploadedFiles.findIndex(x => x.location == fileLocation);
            uploadedFiles[editFileIndex].name = result.value.newFileName;
        }
    });
}
var scrollPosition = 0;
function saveScrollPosition() {
    scrollPosition = $(".vehicleDetailTabContainer").scrollTop();
}
function restoreScrollPosition() {
    $(".vehicleDetailTabContainer").scrollTop(scrollPosition);
    scrollPosition = 0;
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
                    errorToast("An error has occurred, please try again later.");
                }
            });
        } else {
            $("#workAroundInput").hide();
        }
    });
}
function toggleMarkDownOverlay(textAreaName) {
    var textArea = $(`#${textAreaName}`);
    if ($(".markdown-overlay").length > 0) {
        $(".markdown-overlay").remove();
        return;
    }
    var text = textArea.val();
    if (text == undefined) {
        return;
    }
    if (text.length > 0) {
        var formatted = markdown(text);
        //var overlay div
        var overlayDiv = `<div class='markdown-overlay' style="z-index: 1060; position:absolute; top:${textArea.css('top')}; left:${textArea.css('left')}; width:${textArea.css('width')}; height:${textArea.css('height')}; padding:${textArea.css('padding')}; overflow-y:auto; background-color:var(--bs-modal-bg);">${formatted}</div>`;
        textArea.parent().children(`label[for=${textAreaName}]`).append(overlayDiv);
    }
}
function showLinks(e) {
    var textAreaName = $(e.parentElement).attr("for");
    toggleMarkDownOverlay(textAreaName);
}