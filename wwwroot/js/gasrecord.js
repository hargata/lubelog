function showAddGasRecordModal() {
    $.get(`/Vehicle/GetAddGasRecordPartialView?vehicleId=${GetVehicleId().vehicleId}`, function (data) {
        if (data) {
            $("#gasRecordModalContent").html(data);
            //initiate datepicker
            initDatePicker($('#gasRecordDate'));
            $('#gasRecordModal').modal('show');
        }
    });
}
function showEditGasRecordModal(gasRecordId) {
    $.get(`/Vehicle/GetGasRecordForEditById?gasRecordId=${gasRecordId}`, function (data) {
        if (data) {
            $("#gasRecordModalContent").html(data);
            //initiate datepicker
            initDatePicker($('#gasRecordDate'));
            $('#gasRecordModal').modal('show');
        }
    });
}
function hideAddGasRecordModal() {
    $('#gasRecordModal').modal('hide');
}
function deleteGasRecord(gasRecordId) {
    $("#workAroundInput").show();
    Swal.fire({
        title: "Confirm Deletion?",
        text: "Deleted Gas Records cannot be restored.",
        showCancelButton: true,
        confirmButtonText: "Delete",
        confirmButtonColor: "#dc3545"
    }).then((result) => {
        if (result.isConfirmed) {
            $.post(`/Vehicle/DeleteGasRecordById?gasRecordId=${gasRecordId}`, function (data) {
                if (data) {
                    hideAddGasRecordModal();
                    successToast("Gas Record deleted");
                    var vehicleId = GetVehicleId().vehicleId;
                    getVehicleGasRecords(vehicleId);
                } else {
                    errorToast("An error has occurred, please try again later.");
                }
            });
        } else {
            $("#workAroundInput").hide();
        }
    });
}
function saveGasRecordToVehicle(isEdit) {
    //get values
    var formValues = getAndValidateGasRecordValues();
    //validate
    if (formValues.hasError) {
        errorToast("Please check the form data");
        return;
    }
    //save to db.
    $.post('/Vehicle/SaveGasRecordToVehicleId', { gasRecord: formValues }, function (data) {
        if (data) {
            successToast(isEdit ? "Gas Record Updated" : "Gas Record Added.");
            hideAddGasRecordModal();
            saveScrollPosition();
            getVehicleGasRecords(formValues.vehicleId);
        } else {
            errorToast("An error has occurred, please try again later.");
        }
    })
}
function getAndValidateGasRecordValues() {
    var gasDate = $("#gasRecordDate").val();
    var gasMileage = parseInt(globalParseFloat($("#gasRecordMileage").val())).toString();
    var gasGallons = $("#gasRecordGallons").val();
    var gasCost = $("#gasRecordCost").val();
    var gasCostType = $("#gasCostType").val();
    var gasIsFillToFull = $("#gasIsFillToFull").is(":checked");
    var gasIsMissed = $("#gasIsMissed").is(":checked");
    var gasNotes = $("#gasRecordNotes").val();
    var vehicleId = GetVehicleId().vehicleId;
    var gasRecordId = getGasRecordModelData().id;
    //validation
    var hasError = false;
    if (gasDate.trim() == '') { //eliminates whitespace.
        hasError = true;
        $("#gasRecordDate").addClass("is-invalid");
    } else {
        $("#gasRecordDate").removeClass("is-invalid");
    }
    if (gasMileage.trim() == '' || parseInt(gasMileage) < 0) {
        hasError = true;
        $("#gasRecordMileage").addClass("is-invalid");
    } else {
        $("#gasRecordMileage").removeClass("is-invalid");
    }
    if (gasGallons.trim() == '' || globalParseFloat(gasGallons) <= 0) {
        hasError = true;
        $("#gasRecordGallons").addClass("is-invalid");
    } else {
        $("#gasRecordGallons").removeClass("is-invalid");
    }
    if (gasCostType != undefined && gasCostType == 'unit') {
        var convertedGasCost = globalParseFloat(gasCost) * globalParseFloat(gasGallons);
        if (isNaN(convertedGasCost))
        {
            hasError = true;
            $("#gasRecordCost").addClass("is-invalid");
        } else {
            gasCost = globalFloatToString(convertedGasCost.toFixed(2).toString());
            $("#gasRecordCost").removeClass("is-invalid");
        }
    }
    if (gasCost.trim() == '' || !isValidMoney(gasCost)) {
        hasError = true;
        $("#gasRecordCost").addClass("is-invalid");
    } else {
        $("#gasRecordCost").removeClass("is-invalid");
    }
    return {
        id: gasRecordId,
        hasError: hasError,
        vehicleId: vehicleId,
        date: gasDate,
        mileage: gasMileage,
        gallons: gasGallons,
        cost: gasCost,
        files: uploadedFiles,
        isFillToFull: gasIsFillToFull,
        missedFuelUp: gasIsMissed,
        notes: gasNotes
    }
}