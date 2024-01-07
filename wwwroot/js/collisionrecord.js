function showAddCollisionRecordModal() {
    $.get('/Vehicle/GetAddCollisionRecordPartialView', function (data) {
        if (data) {
            $("#collisionRecordModalContent").html(data);
            //initiate datepicker
            $('#collisionRecordDate').datepicker({
                endDate: "+0d",
                format: getShortDatePattern().pattern
            });
            $('#collisionRecordModal').modal('show');
        }
    });
}
function showEditCollisionRecordModal(collisionRecordId) {
    $.get(`/Vehicle/GetCollisionRecordForEditById?collisionRecordId=${collisionRecordId}`, function (data) {
        if (data) {
            $("#collisionRecordModalContent").html(data);
            //initiate datepicker
            $('#collisionRecordDate').datepicker({
                endDate: "+0d",
                format: getShortDatePattern().pattern
            });
            $('#collisionRecordModal').modal('show');
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
                    errorToast("An error has occurred, please try again later.");
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
            getVehicleCollisionRecords(formValues.vehicleId);
            if (formValues.addReminderRecord) {
                setTimeout(function () { showAddReminderModal(formValues); }, 500);
            }
        } else {
            errorToast("An error has occurred, please try again later.");
        }
    })
}
function getAndValidateCollisionRecordValues() {
    var collisionDate = $("#collisionRecordDate").val();
    var collisionMileage = $("#collisionRecordMileage").val();
    var collisionDescription = $("#collisionRecordDescription").val();
    var collisionCost = $("#collisionRecordCost").val();
    var collisionNotes = $("#collisionRecordNotes").val();
    var vehicleId = GetVehicleId().vehicleId;
    var collisionRecordId = getCollisionRecordModelData().id;
    var addReminderRecord = $("#addReminderCheck").is(":checked");
    //validation
    var hasError = false;
    if (collisionDate.trim() == '') { //eliminates whitespace.
        hasError = true;
        $("#collisionRecordDate").addClass("is-invalid");
    } else {
        $("#collisionRecordDate").removeClass("is-invalid");
    }
    if (collisionMileage.trim() == '' || parseInt(collisionMileage) < 0) {
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
    if (collisionCost.trim() == '') {
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
        addReminderRecord: addReminderRecord
    }
}
function deleteCollisionRecordFile(fileLocation, event) {
    event.parentElement.remove();
    uploadedFiles = uploadedFiles.filter(x => x.location != fileLocation);
}