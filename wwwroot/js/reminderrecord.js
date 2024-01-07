function showEditReminderRecordModal(reminderId) {
    $.get(`/Vehicle/GetReminderRecordForEditById?reminderRecordId=${reminderId}`, function (data) {
        if (data) {
            $("#reminderRecordModalContent").html(data); 
            $('#reminderDate').datepicker({
                startDate: "+0d"
            });
            $("#reminderRecordModal").modal("show");
        }
    });
}
function hideAddReminderRecordModal() {
    $('#reminderRecordModal').modal('hide');
}
function deleteReminderRecord(reminderRecordId, e) {
    if (e != undefined) {
        event.stopPropagation();
    }
    $("#workAroundInput").show();
    Swal.fire({
        title: "Confirm Deletion?",
        text: "Deleted Reminders cannot be restored.",
        showCancelButton: true,
        confirmButtonText: "Delete",
        confirmButtonColor: "#dc3545"
    }).then((result) => {
        if (result.isConfirmed) {
            $.post(`/Vehicle/DeleteReminderRecordById?reminderRecordId=${reminderRecordId}`, function (data) {
                if (data) {
                    hideAddReminderRecordModal();
                    successToast("Reminder Deleted");
                    var vehicleId = GetVehicleId().vehicleId;
                    getVehicleReminders(vehicleId);
                } else {
                    errorToast("An error has occurred, please try again later.");
                }
            });
        } else {
            $("#workAroundInput").hide();
        }
    });
}
function saveReminderRecordToVehicle(isEdit) {
    //get values
    var formValues = getAndValidateReminderRecordValues();
    //validate
    if (formValues.hasError) {
        errorToast("Please check the form data");
        return;
    }
    //save to db.
    $.post('/Vehicle/SaveReminderRecordToVehicleId', { reminderRecord: formValues }, function (data) {
        if (data) {
            successToast(isEdit ? "Reminder Updated" : "Reminder Added.");
            hideAddReminderRecordModal();
            getVehicleReminders(formValues.vehicleId);
        } else {
            errorToast("An error has occurred, please try again later.");
        }
    })
}
function getAndValidateReminderRecordValues() {
    var reminderDate = $("#reminderDate").val();
    var reminderMileage = $("#reminderMileage").val();
    var reminderDescription = $("#reminderDescription").val();
    var reminderNotes = $("#reminderNotes").val();
    var reminderOption = $('#reminderOptions input:radio:checked').val();
    var vehicleId = GetVehicleId().vehicleId;
    var reminderId = getReminderRecordModelData().id;
    //validation
    var hasError = false;
    var reminderDateIsInvalid = reminderDate.trim() == ''; //eliminates whitespace.
    var reminderMileageIsInvalid = reminderMileage.trim() == '' || parseInt(reminderMileage) < 0;
    if ((reminderOption == "Both" || reminderOption == "Date") && reminderDateIsInvalid) { 
        hasError = true;
        $("#reminderDate").addClass("is-invalid");
    } else if (reminderOption == "Date") {
        $("#reminderDate").removeClass("is-invalid");
    }
    if ((reminderOption == "Both" || reminderOption == "Odometer") && reminderMileageIsInvalid) {
        hasError = true;
        $("#reminderMileage").addClass("is-invalid");
    } else if (reminderOption == "Odometer") {
        $("#reminderMileage").removeClass("is-invalid");
    }
    if (reminderDescription.trim() == '') {
        hasError = true;
        $("#reminderDescription").addClass("is-invalid");
    } else {
        $("#reminderDescription").removeClass("is-invalid");
    }
    if (reminderOption == undefined) {
        hasError = true;
        $("#reminderMetricDate").addClass("is-invalid");
        $("#reminderMetricOdometer").addClass("is-invalid");
        $("#reminderMetricBoth").addClass("is-invalid");
    } else {
        $("#reminderMetricDate").removeClass("is-invalid");
        $("#reminderMetricOdometer").removeClass("is-invalid");
        $("#reminderMetricBoth").removeClass("is-invalid");
    }

    return {
        id: reminderId,
        hasError: hasError,
        vehicleId: vehicleId,
        date: reminderDate,
        mileage: reminderMileage,
        description: reminderDescription,
        notes: reminderNotes,
        metric: reminderOption
    }
}