function showAddServiceRecordModal() {
    $.get('/Vehicle/GetAddServiceRecordPartialView', function (data) {
        if (data) {
            $("#serviceRecordModalContent").html(data);
            //initiate datepicker
            initDatePicker($('#serviceRecordDate'));
            initTagSelector($("#serviceRecordTag"));
            $('#serviceRecordModal').modal('show');
        }
    });
}
function showEditServiceRecordModal(serviceRecordId) {
    $.get(`/Vehicle/GetServiceRecordForEditById?serviceRecordId=${serviceRecordId}`, function (data) {
        if (data) {
            $("#serviceRecordModalContent").html(data);
            //initiate datepicker
            initDatePicker($('#serviceRecordDate'));
            initTagSelector($("#serviceRecordTag"));
            $('#serviceRecordModal').modal('show');
            $('#serviceRecordModal').off('shown.bs.modal').on('shown.bs.modal', function () {
                if (getGlobalConfig().useMarkDown) {
                    toggleMarkDownOverlay("serviceRecordNotes");
                }
            });
        }
    });
}
function hideAddServiceRecordModal() {
    $('#serviceRecordModal').modal('hide');
}
function deleteServiceRecord(serviceRecordId) {
    $("#workAroundInput").show();
    Swal.fire({
        title: "Confirm Deletion?",
        text: "Deleted Service Records cannot be restored.",
        showCancelButton: true,
        confirmButtonText: "Delete",
        confirmButtonColor: "#dc3545"
    }).then((result) => {
        if (result.isConfirmed) {
            $.post(`/Vehicle/DeleteServiceRecordById?serviceRecordId=${serviceRecordId}`, function (data) {
                if (data) {
                    hideAddServiceRecordModal();
                    successToast("Service Record Deleted");
                    var vehicleId = GetVehicleId().vehicleId;
                    getVehicleServiceRecords(vehicleId);
                } else {
                    errorToast(genericErrorMessage());
                }
            });
        } else {
            $("#workAroundInput").hide();
        }
    });
}
function saveServiceRecordToVehicle(isEdit) {
    //get values
    var formValues = getAndValidateServiceRecordValues();
    //validate
    if (formValues.hasError) {
        errorToast("Please check the form data");
        return;
    }
    //save to db.
    $.post('/Vehicle/SaveServiceRecordToVehicleId', { serviceRecord: formValues }, function (data) {
        if (data) {
            successToast(isEdit ? "Service Record Updated" : "Service Record Added.");
            hideAddServiceRecordModal();
            saveScrollPosition();
            getVehicleServiceRecords(formValues.vehicleId);
            if (formValues.addReminderRecord) {
                setTimeout(function () { showAddReminderModal(formValues); }, 500);
            }
        } else {
            errorToast(genericErrorMessage());
        }
    })
}
function getAndValidateServiceRecordValues() {
    var serviceDate = $("#serviceRecordDate").val();
    var serviceMileage = parseInt(globalParseFloat($("#serviceRecordMileage").val())).toString();
    var serviceDescription = $("#serviceRecordDescription").val();
    var serviceCost = $("#serviceRecordCost").val();
    var serviceNotes = $("#serviceRecordNotes").val();
    var serviceTags = $("#serviceRecordTag").val();
    var vehicleId = GetVehicleId().vehicleId;
    var serviceRecordId = getServiceRecordModelData().id;
    var addReminderRecord = $("#addReminderCheck").is(":checked");
    //validation
    var hasError = false;
    var extraFields = getAndValidateExtraFields();
    if (extraFields.hasError) {
        hasError = true;
    }
    if (serviceDate.trim() == '') { //eliminates whitespace.
        hasError = true;
        $("#serviceRecordDate").addClass("is-invalid");
    } else {
        $("#serviceRecordDate").removeClass("is-invalid");
    }
    if (serviceMileage.trim() == '' || isNaN(serviceMileage) || parseInt(serviceMileage) < 0) {
        hasError = true;
        $("#serviceRecordMileage").addClass("is-invalid");
    } else {
        $("#serviceRecordMileage").removeClass("is-invalid");
    }
    if (serviceDescription.trim() == '') {
        hasError = true;
        $("#serviceRecordDescription").addClass("is-invalid");
    } else {
        $("#serviceRecordDescription").removeClass("is-invalid");
    }
    if (serviceCost.trim() == '' || !isValidMoney(serviceCost)) {
        hasError = true;
        $("#serviceRecordCost").addClass("is-invalid");
    } else {
        $("#serviceRecordCost").removeClass("is-invalid");
    }
    return {
        id: serviceRecordId,
        hasError: hasError,
        vehicleId: vehicleId,
        date: serviceDate,
        mileage: serviceMileage,
        description: serviceDescription,
        cost: serviceCost,
        notes: serviceNotes,
        files: uploadedFiles,
        supplies: selectedSupplies,
        tags: serviceTags,
        addReminderRecord: addReminderRecord,
        extraFields: extraFields.extraFields
    }
}