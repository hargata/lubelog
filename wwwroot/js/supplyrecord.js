function showAddSupplyRecordModal() {
    $.get('/Vehicle/GetAddSupplyRecordPartialView', function (data) {
        if (data) {
            $("#supplyRecordModalContent").html(data);
            //initiate datepicker
            initDatePicker($('#supplyRecordDate'));
            $('#supplyRecordModal').modal('show');
        }
    });
}
function showEditSupplyRecordModal(supplyRecordId) {
    $.get(`/Vehicle/GetSupplyRecordForEditById?supplyRecordId=${supplyRecordId}`, function (data) {
        if (data) {
            $("#supplyRecordModalContent").html(data);
            //initiate datepicker
            initDatePicker($('#supplyRecordDate'));
            $('#supplyRecordModal').modal('show');
        }
    });
}
function hideAddSupplyRecordModal() {
    $('#supplyRecordModal').modal('hide');
}
function deleteSupplyRecord(supplyRecordId) {
    $("#workAroundInput").show();
    Swal.fire({
        title: "Confirm Deletion?",
        text: "Deleted Supply Records cannot be restored.",
        showCancelButton: true,
        confirmButtonText: "Delete",
        confirmButtonColor: "#dc3545"
    }).then((result) => {
        if (result.isConfirmed) {
            $.post(`/Vehicle/DeleteSupplyRecordById?supplyRecordId=${supplyRecordId}`, function (data) {
                if (data) {
                    hideAddSupplyRecordModal();
                    successToast("Supply Record Deleted");
                    var vehicleId = GetVehicleId().vehicleId;
                    getVehicleSupplyRecords(vehicleId);
                } else {
                    errorToast("An error has occurred, please try again later.");
                }
            });
        } else {
            $("#workAroundInput").hide();
        }
    });
}
function saveSupplyRecordToVehicle(isEdit) {
    //get values
    var formValues = getAndValidateSupplyRecordValues();
    //validate
    if (formValues.hasError) {
        errorToast("Please check the form data");
        return;
    }
    //save to db.
    $.post('/Vehicle/SaveSupplyRecordToVehicleId', { supplyRecord: formValues }, function (data) {
        if (data) {
            successToast(isEdit ? "Supply Record Updated" : "Supply Record Added.");
            hideAddSupplyRecordModal();
            saveScrollPosition();
            getVehicleSupplyRecords(formValues.vehicleId);
            if (formValues.addReminderRecord) {
                setTimeout(function () { showAddReminderModal(formValues); }, 500);
            }
        } else {
            errorToast("An error has occurred, please try again later.");
        }
    })
}
function getAndValidateSupplyRecordValues() {
    var supplyDate = $("#supplyRecordDate").val();
    var supplyPartNumber = $("#supplyRecordPartNumber").val();
    var supplyDescription = $("#supplyRecordDescription").val();
    var supplySupplier = $("#supplyRecordSupplier").val();
    var supplyQuantity = $("#supplyRecordQuantity").val();
    var supplyCost = $("#supplyRecordCost").val();
    var supplyNotes = $("#supplyRecordNotes").val();
    var vehicleId = GetVehicleId().vehicleId;
    var supplyRecordId = getSupplyRecordModelData().id;
    //validation
    var hasError = false;
    if (supplyDate.trim() == '') { //eliminates whitespace.
        hasError = true;
        $("#supplyRecordDate").addClass("is-invalid");
    } else {
        $("#supplyRecordDate").removeClass("is-invalid");
    }
    if (supplyDescription.trim() == '') {
        hasError = true;
        $("#supplyRecordDescription").addClass("is-invalid");
    } else {
        $("#supplyRecordDescription").removeClass("is-invalid");
    }
    if (supplyQuantity.trim() == '' || !isValidMoney(supplyQuantity) || globalParseFloat(supplyQuantity) < 0) {
        hasError = true;
        $("#supplyRecordQuantity").addClass("is-invalid");
    } else {
        $("#supplyRecordQuantity").removeClass("is-invalid");
    }
    if (supplyCost.trim() == '' || !isValidMoney(supplyCost)) {
        hasError = true;
        $("#supplyRecordCost").addClass("is-invalid");
    } else {
        $("#supplyRecordCost").removeClass("is-invalid");
    }
    return {
        id: supplyRecordId,
        hasError: hasError,
        vehicleId: vehicleId,
        date: supplyDate,
        partNumber: supplyPartNumber,
        partSupplier: supplySupplier,
        description: supplyDescription,
        cost: supplyCost,
        notes: supplyNotes,
        quantity: supplyQuantity,
        files: uploadedFiles
    }
}