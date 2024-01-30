function showAddOdometerRecordModal() {
    $.get('/Vehicle/GetAddOdometerRecordPartialView', function (data) {
        if (data) {
            $("#odometerRecordModalContent").html(data);
            //initiate datepicker
            initDatePicker($('#odometerRecordDate'));
            $('#odometerRecordModal').modal('show');
        }
    });
}
function showEditOdometerRecordModal(odometerRecordId) {
    $.get(`/Vehicle/GetOdometerRecordForEditById?odometerRecordId=${odometerRecordId}`, function (data) {
        if (data) {
            $("#odometerRecordModalContent").html(data);
            //initiate datepicker
            initDatePicker($('#odometerRecordDate'));
            $('#odometerRecordModal').modal('show');
            $('#odometerRecordModal').off('shown.bs.modal').on('shown.bs.modal', function () {
                if (getGlobalConfig().useMarkDown) {
                    toggleMarkDownOverlay("odometerRecordNotes");
                }
            });
        }
    });
}
function hideAddOdometerRecordModal() {
    $('#odometerRecordModal').modal('hide');
}
function deleteOdometerRecord(odometerRecordId) {
    $("#workAroundInput").show();
    Swal.fire({
        title: "Confirm Deletion?",
        text: "Deleted Odometer Records cannot be restored.",
        showCancelButton: true,
        confirmButtonText: "Delete",
        confirmButtonColor: "#dc3545"
    }).then((result) => {
        if (result.isConfirmed) {
            $.post(`/Vehicle/DeleteOdometerRecordById?odometerRecordId=${odometerRecordId}`, function (data) {
                if (data) {
                    hideAddOdometerRecordModal();
                    successToast("Odometer Record Deleted");
                    var vehicleId = GetVehicleId().vehicleId;
                    getVehicleOdometerRecords(vehicleId);
                } else {
                    errorToast("An error has occurred, please try again later.");
                }
            });
        } else {
            $("#workAroundInput").hide();
        }
    });
}
function saveOdometerRecordToVehicle(isEdit) {
    //get values
    var formValues = getAndValidateOdometerRecordValues();
    //validate
    if (formValues.hasError) {
        errorToast("Please check the form data");
        return;
    }
    //save to db.
    $.post('/Vehicle/SaveOdometerRecordToVehicleId', { odometerRecord: formValues }, function (data) {
        if (data) {
            successToast(isEdit ? "Odometer Record Updated" : "Odometer Record Added.");
            hideAddOdometerRecordModal();
            saveScrollPosition();
            getVehicleOdometerRecords(formValues.vehicleId);
            if (formValues.addReminderRecord) {
                setTimeout(function () { showAddReminderModal(formValues); }, 500);
            }
        } else {
            errorToast("An error has occurred, please try again later.");
        }
    })
}
function getAndValidateOdometerRecordValues() {
    var serviceDate = $("#odometerRecordDate").val();
    var serviceMileage = parseInt(globalParseFloat($("#odometerRecordMileage").val())).toString();
    var serviceNotes = $("#odometerRecordNotes").val();
    var vehicleId = GetVehicleId().vehicleId;
    var odometerRecordId = getOdometerRecordModelData().id;
    //validation
    var hasError = false;
    if (serviceDate.trim() == '') { //eliminates whitespace.
        hasError = true;
        $("#odometerRecordDate").addClass("is-invalid");
    } else {
        $("#odometerRecordDate").removeClass("is-invalid");
    }
    if (serviceMileage.trim() == '' || parseInt(serviceMileage) < 0) {
        hasError = true;
        $("#odometerRecordMileage").addClass("is-invalid");
    } else {
        $("#odometerRecordMileage").removeClass("is-invalid");
    }
    return {
        id: odometerRecordId,
        hasError: hasError,
        vehicleId: vehicleId,
        date: serviceDate,
        mileage: serviceMileage,
        notes: serviceNotes,
        files: uploadedFiles
    }
}