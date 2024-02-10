function showEditReminderRecordModal(reminderId) {
    $.get(`/Vehicle/GetReminderRecordForEditById?reminderRecordId=${reminderId}`, function (data) {
        if (data) {
            $("#reminderRecordModalContent").html(data); 
            initDatePicker($('#reminderDate'), true);
            $("#reminderRecordModal").modal("show");
            $('#reminderRecordModal').off('shown.bs.modal').on('shown.bs.modal', function () {
                if (getGlobalConfig().useMarkDown) {
                    toggleMarkDownOverlay("reminderNotes");
                }
            });
        }
    });
}
function hideAddReminderRecordModal() {
    $('#reminderRecordModal').modal('hide');
}
function checkCustomMonthInterval() {
    var selectedValue = $("#reminderRecurringMonth").val();
    if (selectedValue == "Other") {
        $("#workAroundInput").show();
        Swal.fire({
            title: 'Specify Custom Month Interval',
            html: `
                            <input type="text" id="inputCustomMileage" class="swal2-input" placeholder="Months">
                            `,
            confirmButtonText: 'Set',
            focusConfirm: false,
            preConfirm: () => {
                const customMonth = $("#inputCustomMileage").val();
                if (!customMonth || isNaN(parseInt(customMonth)) || parseInt(customMonth) <= 0) {
                    Swal.showValidationMessage(`Please enter a valid number`);
                }
                return { customMonth }
            },
        }).then(function (result) {
            if (result.isConfirmed) {
                customMonthInterval = result.value.customMonth;
                $("#reminderRecurringMonth > option[value='Other']").text(`Other: ${result.value.customMonth}`);
            } else {
                $("#reminderRecurringMonth").val(getReminderRecordModelData().monthInterval);
            }
            $("#workAroundInput").hide();
        });
    }
}
function checkCustomMileageInterval() {
    var selectedValue = $("#reminderRecurringMileage").val();
    if (selectedValue == "Other") {
        $("#workAroundInput").show();
        Swal.fire({
            title: 'Specify Custom Mileage Interval',
            html: `
                            <input type="text" id="inputCustomMileage" class="swal2-input" placeholder="Mileage">
                            `,
            confirmButtonText: 'Set',
            focusConfirm: false,
            preConfirm: () => {
                const customMileage = $("#inputCustomMileage").val();
                if (!customMileage || isNaN(parseInt(customMileage)) || parseInt(customMileage) <= 0) {
                    Swal.showValidationMessage(`Please enter a valid number`);
                }
                return { customMileage }
            },
        }).then(function (result) {
            if (result.isConfirmed) {
                customMileageInterval = result.value.customMileage;
                $("#reminderRecurringMileage > option[value='Other']").text(`Other: ${result.value.customMileage}`);
            } else {
                $("#reminderRecurringMileage").val(getReminderRecordModelData().mileageInterval);
            }
            $("#workAroundInput").hide();
        });
    }
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
                    errorToast(genericErrorMessage());
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
            saveScrollPosition();
            getVehicleReminders(formValues.vehicleId);
        } else {
            errorToast(genericErrorMessage());
        }
    })
}
function appendMileageToOdometer(increment) {
    var reminderMileage = $("#reminderMileage").val();
    var reminderMileageIsInvalid = reminderMileage.trim() == '' || parseInt(reminderMileage) < 0;
    if (reminderMileageIsInvalid) {
        reminderMileage = 0;
    } else {
        reminderMileage = parseInt(reminderMileage);
    }
    reminderMileage += increment;
    $("#reminderMileage").val(reminderMileage);
}

function enableRecurring() {
    var reminderIsRecurring = $("#reminderIsRecurring").is(":checked");
    if (reminderIsRecurring) {
        $("#reminderRecurringMileage").attr('disabled', false);
        $("#reminderRecurringMonth").attr('disabled', false);
    } else {
        $("#reminderRecurringMileage").attr('disabled', true);
        $("#reminderRecurringMonth").attr('disabled', true);
    }
}

function markDoneReminderRecord(reminderRecordId, e) {
    event.stopPropagation();
    var vehicleId = GetVehicleId().vehicleId;
    $.post(`/Vehicle/PushbackRecurringReminderRecord?reminderRecordId=${reminderRecordId}`, function (data) {
        if (data) {
            successToast("Reminder Updated");
            getVehicleReminders(vehicleId);
        } else {
            errorToast(genericErrorMessage());
        }
    });
}

function getAndValidateReminderRecordValues() {
    var reminderDate = $("#reminderDate").val();
    var reminderMileage = parseInt(globalParseFloat($("#reminderMileage").val())).toString();
    var reminderDescription = $("#reminderDescription").val();
    var reminderNotes = $("#reminderNotes").val();
    var reminderOption = $('#reminderOptions input:radio:checked').val();
    var reminderIsRecurring = $("#reminderIsRecurring").is(":checked");
    var reminderRecurringMonth = $("#reminderRecurringMonth").val();
    var reminderRecurringMileage = $("#reminderRecurringMileage").val();
    var reminderCustomMileageInterval = customMileageInterval;
    var vehicleId = GetVehicleId().vehicleId;
    var reminderId = getReminderRecordModelData().id;
    //validation
    var hasError = false;
    var reminderDateIsInvalid = reminderDate.trim() == ''; //eliminates whitespace.
    var reminderMileageIsInvalid = reminderMileage.trim() == '' || isNaN(reminderMileage) || parseInt(reminderMileage) < 0;
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
        metric: reminderOption,
        isRecurring: reminderIsRecurring,
        reminderMileageInterval: reminderRecurringMileage,
        reminderMonthInterval: reminderRecurringMonth,
        customMileageInterval: customMileageInterval,
        customMonthInterval: customMonthInterval
    }
}