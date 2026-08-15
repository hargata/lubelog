function showAddInsuranceRecordModal() {
    $.get('/Vehicle/GetAddInsuranceRecordPartialView', function (data) {
        if (data) {
            $("#insuranceRecordModalContent").html(data);
            //initiate datepicker
            initDatePicker($('#insuranceRecordDate'));
            initTagSelector($("#insuranceRecordTag"));
            $('#insuranceRecordModal').modal('show');
        }
    });
}
function showEditInsuranceRecordModal(insuranceRecordId, nocache) {
    if (!nocache) {
        var existingContent = $("#insuranceRecordModalContent").html();
        if (existingContent.trim() != '') {
            //check if id is same.
            var existingId = getInsuranceRecordModelData().id;
            if (existingId == insuranceRecordId && $('[data-changed=true]').length > 0) {
                $('#insuranceRecordModal').modal('show');
                $('.cached-banner').show();
                return;
            }
        }
    }
    $.get(`/Vehicle/GetInsuranceRecordForEditById?insuranceRecordId=${insuranceRecordId}`, function (data) {
        if (data) {
            $("#insuranceRecordModalContent").html(data);
            //initiate datepicker
            initDatePicker($('#insuranceRecordDate'));
            initTagSelector($("#insuranceRecordTag"));
            $('#insuranceRecordModal').modal('show');
            bindModalInputChanges('insuranceRecordModal');
            $('#insuranceRecordModal').off('shown.bs.modal').on('shown.bs.modal', function () {
                if (getGlobalConfig().useMarkDown) {
                    toggleMarkDownOverlay("insuranceRecordNotes");
                }
            });
        }
    });
}
function enableInsuranceRecurring() {
    var insuranceIsRecurring = $("#insuranceIsRecurring").is(":checked");
    if (insuranceIsRecurring) {
        $("#insuranceRecurringMonth").attr('disabled', false);
    } else {
        $("#insuranceRecurringMonth").attr('disabled', true);
    }
}
function hideAddInsuranceRecordModal() {
    $('#insuranceRecordModal').modal('hide');
}
function deleteInsuranceRecord(insuranceRecordId) {
    $("#workAroundInput").show();
    confirmDelete("Deleted Insurance Records cannot be restored.", (result) => {
        if (result.isConfirmed) {
            $.post(`/Vehicle/DeleteInsuranceRecordById?insuranceRecordId=${insuranceRecordId}`, function (data) {
                if (data.success) {
                    hideAddInsuranceRecordModal();
                    successToast("Insurance Record Deleted");
                    var vehicleId = GetVehicleId().vehicleId;
                    getVehicleInsuranceRecords(vehicleId);
                } else {
                    errorToast(data.message);
                    $("#workAroundInput").hide();
                }
            });
        } else {
            $("#workAroundInput").hide();
        }
    });
}
function saveInsuranceRecordToVehicle(isEdit) {
    //get values
    var formValues = getAndValidateInsuranceRecordValues();
    //validate
    if (formValues.hasError) {
        errorToast("Please check the form data");
        return;
    }
    //save to db.
    $.post('/Vehicle/SaveInsuranceRecordToVehicleId', { insuranceRecord: formValues }, function (data) {
        if (data.success) {
            successToast(isEdit ? "Insurance Record Updated" : "Insurance Record Added.");
            hideAddInsuranceRecordModal();
            saveScrollPosition();
            getVehicleInsuranceRecords(formValues.vehicleId);
            if (formValues.addReminderRecord) {
                setTimeout(function () { showAddReminderModal(formValues); }, 500);
            }
        } else {
            errorToast(data.message);
        }
    })
}
function checkCustomMonthIntervalForInsurance() {
    var selectedValue = $("#insuranceRecurringMonth").val();
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
                $("#insuranceRecurringMonth > option[value='Other']").text(`Other: ${result.value.customMonth} ${result.value.customMonthUnit}`);
            } else {
                $("#insuranceRecurringMonth").val(getInsuranceRecordModelData().monthInterval);
            }
            $("#workAroundInput").hide();
        });
    }
}
function getAndValidateInsuranceRecordValues() {
    var insuranceDate = $("#insuranceRecordDate").val();
    var insuranceDescription = $("#insuranceRecordDescription").val();
    var insuranceProvider = $("#insuranceRecordProvider").val();
    var insurancePolicyNumber = $("#insuranceRecordPolicyNumber").val();
    var insuranceCost = $("#insuranceRecordCost").val();
    var insuranceNotes = $("#insuranceRecordNotes").val();
    var vehicleId = GetVehicleId().vehicleId;
    var insuranceRecordId = getInsuranceRecordModelData().id;
    var insuranceIsRecurring = $("#insuranceIsRecurring").is(":checked");
    var insuranceRecurringMonth = $("#insuranceRecurringMonth").val();
    var insuranceTags = $("#insuranceRecordTag").val();
    var addReminderRecord = $("#addReminderCheck").is(":checked");
    //validation
    var hasError = false;
    var extraFields = getAndValidateExtraFields();
    if (extraFields.hasError) {
        hasError = true;
    }
    if (insuranceDate.trim() == '') { //eliminates whitespace.
        hasError = true;
        $("#insuranceRecordDate").addClass("is-invalid");
    } else {
        $("#insuranceRecordDate").removeClass("is-invalid");
    }
    if (insuranceDescription.trim() == '') {
        hasError = true;
        $("#insuranceRecordDescription").addClass("is-invalid");
    } else {
        $("#insuranceRecordDescription").removeClass("is-invalid");
    }
    if (insuranceCost.trim() == '' || !isValidMoney(insuranceCost)) {
        hasError = true;
        $("#insuranceRecordCost").addClass("is-invalid");
    } else {
        $("#insuranceRecordCost").removeClass("is-invalid");
    }
    return {
        id: insuranceRecordId,
        hasError: hasError,
        vehicleId: vehicleId,
        date: insuranceDate,
        description: insuranceDescription,
        provider: insuranceProvider,
        policyNumber: insurancePolicyNumber,
        cost: insuranceCost,
        notes: insuranceNotes,
        isRecurring: insuranceIsRecurring,
        recurringInterval: insuranceRecurringMonth,
        customMonthInterval: customMonthInterval,
        customMonthIntervalUnit: customMonthIntervalUnit,
        tags: insuranceTags,
        files: uploadedFiles,
        addReminderRecord: addReminderRecord,
        extraFields: extraFields.extraFields,
        reminderRecordId: recurringReminderRecordId
    }
}

function checkRecurringInsurance() {
    let vehicleId = GetVehicleId().vehicleId
    $.post('/Vehicle/CheckRecurringInsuranceRecords', { vehicleId: vehicleId }, function (data) {
        if (data) {
            //notify users that recurring insurance records were updated and they should refresh the page to see the new changes.
            infoToast(`Recurring Insurance Records Updated!<br /><br /><a class='text-link' style='cursor:pointer;' onclick='viewVehicle(${vehicleId})'>Refresh to see new records</a>`);
        }
    })
}
