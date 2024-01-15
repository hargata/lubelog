function showAddUpgradeRecordModal() {
    $.get('/Vehicle/GetAddUpgradeRecordPartialView', function (data) {
        if (data) {
            $("#upgradeRecordModalContent").html(data);
            //initiate datepicker
            initDatePicker($('#upgradeRecordDate'));
            $('#upgradeRecordModal').modal('show');
        }
    });
}
function showEditUpgradeRecordModal(upgradeRecordId) {
    $.get(`/Vehicle/GetUpgradeRecordForEditById?upgradeRecordId=${upgradeRecordId}`, function (data) {
        if (data) {
            $("#upgradeRecordModalContent").html(data);
            //initiate datepicker
            initDatePicker($('#upgradeRecordDate'));
            $('#upgradeRecordModal').modal('show');
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
                    errorToast("An error has occurred, please try again later.");
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
            errorToast("An error has occurred, please try again later.");
        }
    })
}
function getAndValidateUpgradeRecordValues() {
    var upgradeDate = $("#upgradeRecordDate").val();
    var upgradeMileage = $("#upgradeRecordMileage").val();
    var upgradeDescription = $("#upgradeRecordDescription").val();
    var upgradeCost = $("#upgradeRecordCost").val();
    var upgradeNotes = $("#upgradeRecordNotes").val();
    var vehicleId = GetVehicleId().vehicleId;
    var upgradeRecordId = getUpgradeRecordModelData().id;
    var addReminderRecord = $("#addReminderCheck").is(":checked");
    //validation
    var hasError = false;
    if (upgradeDate.trim() == '') { //eliminates whitespace.
        hasError = true;
        $("#upgradeRecordDate").addClass("is-invalid");
    } else {
        $("#upgradeRecordDate").removeClass("is-invalid");
    }
    if (upgradeMileage.trim() == '' || parseInt(upgradeMileage) < 0) {
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
        addReminderRecord: addReminderRecord
    }
}