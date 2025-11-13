function showAddTaxRecordModal() {
    $.get('/Vehicle/GetAddTaxRecordPartialView', function (data) {
        if (data) {
            $("#taxRecordModalContent").html(data);
            //initiate datepicker
            initDatePicker($('#taxRecordDate'));
            initTagSelector($("#taxRecordTag"));
            $('#taxRecordModal').modal('show');
        }
    });
}
function showEditTaxRecordModal(taxRecordId, nocache) {
    if (!nocache) {
        var existingContent = $("#taxRecordModalContent").html();
        if (existingContent.trim() != '') {
            //check if id is same.
            var existingId = getTaxRecordModelData().id;
            if (existingId == taxRecordId && $('[data-changed=true]').length > 0) {
                $('#taxRecordModal').modal('show');
                $('.cached-banner').show();
                return;
            }
        }
    }
    $.get(`/Vehicle/GetTaxRecordForEditById?taxRecordId=${taxRecordId}`, function (data) {
        if (data) {
            $("#taxRecordModalContent").html(data);
            //initiate datepicker
            initDatePicker($('#taxRecordDate'));
            initTagSelector($("#taxRecordTag"));
            $('#taxRecordModal').modal('show');
            bindModalInputChanges('taxRecordModal');
            $('#taxRecordModal').off('shown.bs.modal').on('shown.bs.modal', function () {
                if (getGlobalConfig().useMarkDown) {
                    toggleMarkDownOverlay("taxRecordNotes");
                }
            });
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
                if (data.success) {
                    hideAddTaxRecordModal();
                    successToast("Tax Record Deleted");
                    var vehicleId = GetVehicleId().vehicleId;
                    getVehicleTaxRecords(vehicleId);
                } else {
                    errorToast(data.message);
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
        if (data.success) {
            successToast(isEdit ? "Tax Record Updated" : "Tax Record Added.");
            hideAddTaxRecordModal();
            saveScrollPosition();
            getVehicleTaxRecords(formValues.vehicleId);
            if (formValues.addReminderRecord) {
                setTimeout(function () { showAddReminderModal(formValues); }, 500);
            }
        } else {
            errorToast(data.message);
        }
    })
}
function checkCustomMonthIntervalForTax() {
    var selectedValue = $("#taxRecurringMonth").val();
    if (selectedValue == "Other") {
        $("#workAroundInput").show();
        Swal.fire({
            title: 'Specify Custom Time Interval',
            html: `
                            <input type="text" inputmode="numeric" id="inputCustomMonth" class="swal2-input" placeholder="Months" onkeydown="handleSwalEnter(event)">
                            <select class="swal2-select" id="inputCustomMonthUnit">
                                <option value="Months">Months</option>
                                <option value="Days">Days</option>
                            </select>
                            `,
            confirmButtonText: 'Set',
            focusConfirm: false,
            preConfirm: () => {
                const customMonth = $("#inputCustomMonth").val();
                if (!customMonth || isNaN(parseInt(customMonth)) || parseInt(customMonth) <= 0) {
                    Swal.showValidationMessage(`Please enter a valid number`);
                }
                const customMonthUnit = $("#inputCustomMonthUnit").val();
                return { customMonth, customMonthUnit }
            },
        }).then(function (result) {
            if (result.isConfirmed) {
                customMonthInterval = result.value.customMonth;
                customMonthIntervalUnit = result.value.customMonthUnit;
                $("#taxRecurringMonth > option[value='Other']").text(`Other: ${result.value.customMonth} ${result.value.customMonthUnit}`);
            } else {
                $("#taxRecurringMonth").val(getTaxRecordModelData().monthInterval);
            }
            $("#workAroundInput").hide();
        });
    }
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
    var taxTags = $("#taxRecordTag").val();
    var addReminderRecord = $("#addReminderCheck").is(":checked");
    //validation
    var hasError = false;
    var extraFields = getAndValidateExtraFields();
    if (extraFields.hasError) {
        hasError = true;
    }
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
        customMonthInterval: customMonthInterval,
        customMonthIntervalUnit: customMonthIntervalUnit,
        tags: taxTags,
        files: uploadedFiles,
        addReminderRecord: addReminderRecord,
        extraFields: extraFields.extraFields,
        reminderRecordId: recurringReminderRecordId
    }
}

function checkRecurringTaxes() {
    let vehicleId = GetVehicleId().vehicleId
    $.post('/Vehicle/CheckRecurringTaxRecords', { vehicleId: vehicleId }, function (data) {
        if (data) {
            //notify users that recurring tax records were updated and they should refresh the page to see the new changes.
            infoToast(`Recurring Tax Records Updated!<br /><br /><a class='text-link' style='cursor:pointer;' onclick='viewVehicle(${vehicleId})'>Refresh to see new records</a>`);
        }
    })
}