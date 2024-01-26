function showAddTaxRecordModal() {
    $.get('/Vehicle/GetAddTaxRecordPartialView', function (data) {
        if (data) {
            $("#taxRecordModalContent").html(data);
            //initiate datepicker
            initDatePicker($('#taxRecordDate'));
            $('#taxRecordModal').modal('show');
        }
    });
}
function showEditTaxRecordModal(taxRecordId) {
    $.get(`/Vehicle/GetTaxRecordForEditById?taxRecordId=${taxRecordId}`, function (data) {
        if (data) {
            $("#taxRecordModalContent").html(data);
            //initiate datepicker
            initDatePicker($('#taxRecordDate'));
            $('#taxRecordModal').modal('show');
        }
    });
}
function enableTaxRecurring() {
    var taxIsRecurring = $("#taxIsRecurring").is(":checked");
    if (taxIsRecurring) {
        $("#taxRecurringMonth").attr('disabled', false);
    } else {
        $("#taxRecurringMonth").attr('disabled', true);
    }
}
function hideAddTaxRecordModal() {
    $('#taxRecordModal').modal('hide');
}
function deleteTaxRecord(taxRecordId) {
    $("#workAroundInput").show();
    Swal.fire({
        title: "Confirm Deletion?",
        text: "Deleted Tax Records cannot be restored.",
        showCancelButton: true,
        confirmButtonText: "Delete",
        confirmButtonColor: "#dc3545"
    }).then((result) => {
        if (result.isConfirmed) {
            $.post(`/Vehicle/DeleteTaxRecordById?taxRecordId=${taxRecordId}`, function (data) {
                if (data) {
                    hideAddTaxRecordModal();
                    successToast("Tax Record Deleted");
                    var vehicleId = GetVehicleId().vehicleId;
                    getVehicleTaxRecords(vehicleId);
                } else {
                    errorToast("An error has occurred, please try again later.");
                }
            });
        } else {
            $("#workAroundInput").hide();
        }
    });
}
function saveTaxRecordToVehicle(isEdit) {
    //get values
    var formValues = getAndValidateTaxRecordValues();
    //validate
    if (formValues.hasError) {
        errorToast("Please check the form data");
        return;
    }
    //save to db.
    $.post('/Vehicle/SaveTaxRecordToVehicleId', { taxRecord: formValues }, function (data) {
        if (data) {
            successToast(isEdit ? "Tax Record Updated" : "Tax Record Added.");
            hideAddTaxRecordModal();
            saveScrollPosition();
            getVehicleTaxRecords(formValues.vehicleId);
            if (formValues.addReminderRecord) {
                setTimeout(function () { showAddReminderModal(formValues); }, 500);
            }
        } else {
            errorToast("An error has occurred, please try again later.");
        }
    })
}
function getAndValidateTaxRecordValues() {
    var taxDate = $("#taxRecordDate").val();
    var taxDescription = $("#taxRecordDescription").val();
    var taxCost = $("#taxRecordCost").val();
    var taxNotes = $("#taxRecordNotes").val();
    var vehicleId = GetVehicleId().vehicleId;
    var taxRecordId = getTaxRecordModelData().id;
    var taxIsRecurring = $("#taxIsRecurring").is(":checked");
    var taxRecurringMonth = $("#taxRecurringMonth").val();
    var addReminderRecord = $("#addReminderCheck").is(":checked");
    //validation
    var hasError = false;
    if (taxDate.trim() == '') { //eliminates whitespace.
        hasError = true;
        $("#taxRecordDate").addClass("is-invalid");
    } else {
        $("#taxRecordDate").removeClass("is-invalid");
    }
    if (taxDescription.trim() == '') {
        hasError = true;
        $("#taxRecordDescription").addClass("is-invalid");
    } else {
        $("#taxRecordDescription").removeClass("is-invalid");
    }
    if (taxCost.trim() == '' || !isValidMoney(taxCost)) {
        hasError = true;
        $("#taxRecordCost").addClass("is-invalid");
    } else {
        $("#taxRecordCost").removeClass("is-invalid");
    }
    return {
        id: taxRecordId,
        hasError: hasError,
        vehicleId: vehicleId,
        date: taxDate,
        description: taxDescription,
        cost: taxCost,
        notes: taxNotes,
        isRecurring: taxIsRecurring,
        recurringInterval: taxRecurringMonth,
        files: uploadedFiles,
        addReminderRecord: addReminderRecord
    }
}