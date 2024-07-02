function showAddOdometerRecordModal() {
    $.get(`/Vehicle/GetAddOdometerRecordPartialView?vehicleId=${GetVehicleId().vehicleId}`, function (data) {
        if (data) {
            $("#odometerRecordModalContent").html(data);
            //initiate datepicker
            initDatePicker($('#odometerRecordDate'));
            initTagSelector($("#odometerRecordTag"));
            $('#odometerRecordModal').modal('show');
        }
    });
}
function showEditOdometerRecordModal(odometerRecordId, nocache) {
    if (!nocache) {
        var existingContent = $("#odometerRecordModalContent").html();
        if (existingContent.trim() != '') {
            //check if id is same.
            var existingId = getOdometerRecordModelData().id;
            if (existingId == odometerRecordId && $('[data-changed=true]').length > 0) {
                $('#odometerRecordModal').modal('show');
                $('.cached-banner').show();
                return;
            }
        }
    }
    $.get(`/Vehicle/GetOdometerRecordForEditById?odometerRecordId=${odometerRecordId}`, function (data) {
        if (data) {
            $("#odometerRecordModalContent").html(data);
            //initiate datepicker
            initDatePicker($('#odometerRecordDate'));
            initTagSelector($("#odometerRecordTag"));
            $('#odometerRecordModal').modal('show');
            bindModalInputChanges('odometerRecordModal');
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
                    errorToast(genericErrorMessage());
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
            errorToast(genericErrorMessage());
        }
    })
}
function getAndValidateOdometerRecordValues() {
    var serviceDate = $("#odometerRecordDate").val();
    var initialOdometerMileage = parseInt(globalParseFloat($("#initialOdometerRecordMileage").val())).toString();
    var serviceMileage = parseInt(globalParseFloat($("#odometerRecordMileage").val())).toString();
    var serviceNotes = $("#odometerRecordNotes").val();
    var serviceTags = $("#odometerRecordTag").val();
    var vehicleId = GetVehicleId().vehicleId;
    var odometerRecordId = getOdometerRecordModelData().id;
    //Odometer Adjustments
    serviceMileage = GetAdjustedOdometer(odometerRecordId, serviceMileage);
    //validation
    var hasError = false;
    var extraFields = getAndValidateExtraFields();
    if (extraFields.hasError) {
        hasError = true;
    }
    if (serviceDate.trim() == '') { //eliminates whitespace.
        hasError = true;
        $("#odometerRecordDate").addClass("is-invalid");
    } else {
        $("#odometerRecordDate").removeClass("is-invalid");
    }
    if (serviceMileage.trim() == '' || isNaN(serviceMileage) || parseInt(serviceMileage) < 0) {
        hasError = true;
        $("#odometerRecordMileage").addClass("is-invalid");
    } else {
        $("#odometerRecordMileage").removeClass("is-invalid");
    }
    if (isNaN(initialOdometerMileage) || parseInt(initialOdometerMileage) < 0) {
        hasError = true;
        $("#initialOdometerRecordMileage").addClass("is-invalid");
    } else {
        $("#initialOdometerRecordMileage").removeClass("is-invalid");
    }
    return {
        id: odometerRecordId,
        hasError: hasError,
        vehicleId: vehicleId,
        date: serviceDate,
        initialMileage: initialOdometerMileage,
        mileage: serviceMileage,
        notes: serviceNotes,
        tags: serviceTags,
        files: uploadedFiles,
        extraFields: extraFields.extraFields
    }
}

function recalculateDistance() {
    //force distance recalculation
    //reserved for when data is incoherent with negative distances due to non-chronologica order of odometer records.
    var vehicleId = GetVehicleId().vehicleId
    $.post(`/Vehicle/ForceRecalculateDistanceByVehicleId?vehicleId=${vehicleId}`, function (data) {
        if (data) {
            successToast("Odometer Records Updated")
            getVehicleOdometerRecords(vehicleId);
        } else {
            errorToast(genericErrorMessage());
        }
    });
}

function editMultipleOdometerRecords(ids) {
    $.post('/Vehicle/GetOdometerRecordsEditModal', { recordIds: ids }, function (data) {
        if (data) {
            $("#odometerRecordModalContent").html(data);
            //initiate datepicker
            initDatePicker($('#odometerRecordDate'));
            initTagSelector($("#odometerRecordTag"));
            $('#odometerRecordModal').modal('show');
        }
    });
}
function saveMultipleOdometerRecordsToVehicle() {
    var odometerDate = $("#odometerRecordDate").val();
    var initialOdometerMileage = $("#initialOdometerRecordMileage").val();
    var odometerMileage = $("#odometerRecordMileage").val();
    var initialOdometerMileageToParse = parseInt(globalParseFloat($("#initialOdometerRecordMileage").val())).toString();
    var odometerMileageToParse = parseInt(globalParseFloat($("#odometerRecordMileage").val())).toString();
    var odometerNotes = $("#odometerRecordNotes").val();
    var odometerTags = $("#odometerRecordTag").val();
    //validation
    var hasError = false;
    if (odometerMileage.trim() != '' && (isNaN(odometerMileageToParse) || parseInt(odometerMileageToParse) < 0)) {
        hasError = true;
        $("#odometerRecordMileage").addClass("is-invalid");
    } else {
        $("#odometerRecordMileage").removeClass("is-invalid");
    }
    if (initialOdometerMileage.trim() != '' && (isNaN(initialOdometerMileageToParse) || parseInt(initialOdometerMileageToParse) < 0)) {
        hasError = true;
        $("#odometerRecordMileage").addClass("is-invalid");
    } else {
        $("#odometerRecordMileage").removeClass("is-invalid");
    }
    if (hasError) {
        errorToast("Please check the form data");
        return;
    }
    var formValues = {
        recordIds: recordsToEdit,
        editRecord: {
            date: odometerDate,
            initialMileage: initialOdometerMileageToParse,
            mileage: odometerMileageToParse,
            notes: odometerNotes,
            tags: odometerTags
        }
    }
    $.post('/Vehicle/SaveMultipleOdometerRecords', { editModel: formValues }, function (data) {
        if (data) {
            successToast("Odometer Records Updated");
            hideAddOdometerRecordModal();
            saveScrollPosition();
            getVehicleOdometerRecords(GetVehicleId().vehicleId);
        } else {
            errorToast(genericErrorMessage());
        }
    })
}
function toggleInitialOdometerEnabled() {
    if ($("#initialOdometerRecordMileage").prop("disabled")) {
        $("#initialOdometerRecordMileage").prop("disabled", false);
    } else {
        $("#initialOdometerRecordMileage").prop("disabled", true);
    }
    
}