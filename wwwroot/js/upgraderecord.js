﻿function showAddUpgradeRecordModal() {
    $.get('/Vehicle/GetAddUpgradeRecordPartialView', function (data) {
        if (data) {
            $("#upgradeRecordModalContent").html(data);
            //initiate datepicker
            initDatePicker($('#upgradeRecordDate'));
            initTagSelector($("#upgradeRecordTag"));
            $('#upgradeRecordModal').modal('show');
        }
    });
}
function showEditUpgradeRecordModal(upgradeRecordId, nocache) {
    if (!nocache) {
        var existingContent = $("#upgradeRecordModalContent").html();
        if (existingContent.trim() != '') {
            //check if id is same.
            var existingId = getUpgradeRecordModelData().id;
            if (existingId == upgradeRecordId && $('[data-changed=true]').length > 0) {
                $('#upgradeRecordModal').modal('show');
                $('.cached-banner').show();
                return;
            }
        }
    }
    $.get(`/Vehicle/GetUpgradeRecordForEditById?upgradeRecordId=${upgradeRecordId}`, function (data) {
        if (data) {
            $("#upgradeRecordModalContent").html(data);
            //initiate datepicker
            initDatePicker($('#upgradeRecordDate'));
            initTagSelector($("#upgradeRecordTag"));
            $('#upgradeRecordModal').modal('show');
            bindModalInputChanges('upgradeRecordModal');
            $('#upgradeRecordModal').off('shown.bs.modal').on('shown.bs.modal', function () {
                if (getGlobalConfig().useMarkDown) {
                    toggleMarkDownOverlay("upgradeRecordNotes");
                }
            });
        }
    });
}
function hideAddUpgradeRecordModal() {
    $('#upgradeRecordModal').modal('hide');
}
function deleteUpgradeRecord(upgradeRecordId) {
    $("#workAroundInput").show();
    Swal.fire({
        title: "Confirm Deletion?",
        text: "Deleted Upgrade Records cannot be restored.",
        showCancelButton: true,
        confirmButtonText: "Delete",
        confirmButtonColor: "#dc3545"
    }).then((result) => {
        if (result.isConfirmed) {
            $.post(`/Vehicle/DeleteUpgradeRecordById?upgradeRecordId=${upgradeRecordId}`, function (data) {
                if (data) {
                    hideAddUpgradeRecordModal();
                    successToast("Upgrade Record Deleted");
                    var vehicleId = GetVehicleId().vehicleId;
                    getVehicleUpgradeRecords(vehicleId);
                } else {
                    errorToast(genericErrorMessage());
                }
            });
        } else {
            $("#workAroundInput").hide();
        }
    });
}
function saveUpgradeRecordToVehicle(isEdit) {
    //get values
    var formValues = getAndValidateUpgradeRecordValues();
    //validate
    if (formValues.hasError) {
        errorToast("Please check the form data");
        return;
    }
    //save to db.
    $.post('/Vehicle/SaveUpgradeRecordToVehicleId', { upgradeRecord: formValues }, function (data) {
        if (data) {
            successToast(isEdit ? "Upgrade Record Updated" : "Upgrade Record Added.");
            hideAddUpgradeRecordModal();
            saveScrollPosition();
            getVehicleUpgradeRecords(formValues.vehicleId);
            if (formValues.addReminderRecord) {
                setTimeout(function () { showAddReminderModal(formValues); }, 500);
            }
        } else {
            errorToast(genericErrorMessage());
        }
    })
}
function getAndValidateUpgradeRecordValues() {
    var upgradeDate = $("#upgradeRecordDate").val();
    var upgradeMileage = parseInt(globalParseFloat($("#upgradeRecordMileage").val())).toString();
    var upgradeDescription = $("#upgradeRecordDescription").val();
    var upgradeCost = $("#upgradeRecordCost").val();
    var upgradeNotes = $("#upgradeRecordNotes").val();
    var upgradeTags = $("#upgradeRecordTag").val();
    var vehicleId = GetVehicleId().vehicleId;
    var upgradeRecordId = getUpgradeRecordModelData().id;
    var addReminderRecord = $("#addReminderCheck").is(":checked");
    //Odometer Adjustments
    if (isNaN(upgradeMileage) && GetVehicleId().odometerOptional) {
        upgradeMileage = '0';
    }
    upgradeMileage = GetAdjustedOdometer(upgradeRecordId, upgradeMileage);
    //validation
    var hasError = false;
    var extraFields = getAndValidateExtraFields();
    if (extraFields.hasError) {
        hasError = true;
    }
    if (upgradeDate.trim() == '') { //eliminates whitespace.
        hasError = true;
        $("#upgradeRecordDate").addClass("is-invalid");
    } else {
        $("#upgradeRecordDate").removeClass("is-invalid");
    }
    if (upgradeMileage.trim() == '' || isNaN(upgradeMileage) || parseInt(upgradeMileage) < 0) {
        hasError = true;
        $("#upgradeRecordMileage").addClass("is-invalid");
    } else {
        $("#upgradeRecordMileage").removeClass("is-invalid");
    }
    if (upgradeDescription.trim() == '') {
        hasError = true;
        $("#upgradeRecordDescription").addClass("is-invalid");
    } else {
        $("#upgradeRecordDescription").removeClass("is-invalid");
    }
    if (upgradeCost.trim() == '' || !isValidMoney(upgradeCost)) {
        hasError = true;
        $("#upgradeRecordCost").addClass("is-invalid");
    } else {
        $("#upgradeRecordCost").removeClass("is-invalid");
    }
    return {
        id: upgradeRecordId,
        hasError: hasError,
        vehicleId: vehicleId,
        date: upgradeDate,
        mileage: upgradeMileage,
        description: upgradeDescription,
        cost: upgradeCost,
        notes: upgradeNotes,
        files: uploadedFiles,
        supplies: selectedSupplies,
        tags: upgradeTags,
        addReminderRecord: addReminderRecord,
        extraFields: extraFields.extraFields,
        requisitionHistory: supplyUsageHistory,
        reminderRecordId: recurringReminderRecordId,
        copySuppliesAttachment: copySuppliesAttachments
    }
}