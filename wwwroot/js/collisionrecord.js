function showAddCollisionRecordModal() {
    $.get('/Vehicle/GetAddCollisionRecordPartialView', function (data) {
        if (data) {
            $("#collisionRecordModalContent").html(data);
            //initiate datepicker
            initDatePicker($('#collisionRecordDate'));
            initTagSelector($("#collisionRecordTag"));
            $('#collisionRecordModal').modal('show');
        }
    });
}
function showEditCollisionRecordModal(collisionRecordId, nocache) {
    if (!nocache) {
        var existingContent = $("#collisionRecordModalContent").html();
        if (existingContent.trim() != '') {
            //check if id is same.
            var existingId = getCollisionRecordModelData().id;
            if (existingId == collisionRecordId && $('[data-changed=true]').length > 0) {
                $('#collisionRecordModal').modal('show');
                $('.cached-banner').show();
                return;
            }
        }
    }
    $.get(`/Vehicle/GetCollisionRecordForEditById?collisionRecordId=${collisionRecordId}`, function (data) {
        if (data) {
            $("#collisionRecordModalContent").html(data);
            //initiate datepicker
            initDatePicker($('#collisionRecordDate'));
            initTagSelector($("#collisionRecordTag"));
            $('#collisionRecordModal').modal('show');
            bindModalInputChanges('collisionRecordModal');
            $('#collisionRecordModal').off('shown.bs.modal').on('shown.bs.modal', function () {
                if (getGlobalConfig().useMarkDown) {
                    toggleMarkDownOverlay("collisionRecordNotes");
                }
            });
        }
    });
}
function hideAddCollisionRecordModal() {
    $('#collisionRecordModal').modal('hide');
}
function deleteCollisionRecord(collisionRecordId) {
    $("#workAroundInput").show();
    Swal.fire({
        title: "Confirm Deletion?",
        text: "Deleted Repair Records cannot be restored.",
        showCancelButton: true,
        confirmButtonText: "Delete",
        confirmButtonColor: "#dc3545"
    }).then((result) => {
        if (result.isConfirmed) {
            $.post(`/Vehicle/DeleteCollisionRecordById?collisionRecordId=${collisionRecordId}`, function (data) {
                if (data) {
                    hideAddCollisionRecordModal();
                    successToast("Repair Record Deleted");
                    var vehicleId = GetVehicleId().vehicleId;
                    getVehicleCollisionRecords(vehicleId);
                } else {
                    errorToast(genericErrorMessage());
                }
            });
        } else {
            $("#workAroundInput").hide();
        }
    });
}
function saveCollisionRecordToVehicle(isEdit) {
    //get values
    var formValues = getAndValidateCollisionRecordValues();
    //validate
    if (formValues.hasError) {
        errorToast("Please check the form data");
        return;
    }
    //save to db.
    $.post('/Vehicle/SaveCollisionRecordToVehicleId', { collisionRecord: formValues }, function (data) {
        if (data) {
            successToast(isEdit ? "Repair Record Updated" : "Repair Record Added.");
            hideAddCollisionRecordModal();
            saveScrollPosition();
            getVehicleCollisionRecords(formValues.vehicleId);
            if (formValues.addReminderRecord) {
                setTimeout(function () { showAddReminderModal(formValues); }, 500);
            }
        } else {
            errorToast(genericErrorMessage());
        }
    })
}
function getAndValidateCollisionRecordValues() {
    var collisionDate = $("#collisionRecordDate").val();
    var collisionMileage = parseInt(globalParseFloat($("#collisionRecordMileage").val())).toString();
    var collisionDescription = $("#collisionRecordDescription").val();
    var collisionCost = $("#collisionRecordCost").val();
    var collisionNotes = $("#collisionRecordNotes").val();
    var collisionTags = $("#collisionRecordTag").val();
    var vehicleId = GetVehicleId().vehicleId;
    var collisionRecordId = getCollisionRecordModelData().id;
    var addReminderRecord = $("#addReminderCheck").is(":checked");
    //Odometer Adjustments
    if (isNaN(collisionMileage) && GetVehicleId().odometerOptional) {
        collisionMileage = '0';
    }
    collisionMileage = GetAdjustedOdometer(collisionRecordId, collisionMileage);
    //validation
    var hasError = false;
    var extraFields = getAndValidateExtraFields();
    if (extraFields.hasError) {
        hasError = true;
    }
    if (collisionDate.trim() == '') { //eliminates whitespace.
        hasError = true;
        $("#collisionRecordDate").addClass("is-invalid");
    } else {
        $("#collisionRecordDate").removeClass("is-invalid");
    }
    if (collisionMileage.trim() == '' || isNaN(collisionMileage) || parseInt(collisionMileage) < 0) {
        hasError = true;
        $("#collisionRecordMileage").addClass("is-invalid");
    } else {
        $("#collisionRecordMileage").removeClass("is-invalid");
    }
    if (collisionDescription.trim() == '') {
        hasError = true;
        $("#collisionRecordDescription").addClass("is-invalid");
    } else {
        $("#collisionRecordDescription").removeClass("is-invalid");
    }
    if (collisionCost.trim() == '' || !isValidMoney(collisionCost)) {
        hasError = true;
        $("#collisionRecordCost").addClass("is-invalid");
    } else {
        $("#collisionRecordCost").removeClass("is-invalid");
    }
    return {
        id: collisionRecordId,
        hasError: hasError,
        vehicleId: vehicleId,
        date: collisionDate,
        mileage: collisionMileage,
        description: collisionDescription,
        cost: collisionCost,
        notes: collisionNotes,
        files: uploadedFiles,
        supplies: selectedSupplies,
        tags: collisionTags,
        addReminderRecord: addReminderRecord,
        extraFields: extraFields.extraFields,
        requisitionHistory: supplyUsageHistory,
        deletedRequisitionHistory: deletedSupplyUsageHistory,
        reminderRecordId: recurringReminderRecordId,
        copySuppliesAttachment: copySuppliesAttachments
    }
}