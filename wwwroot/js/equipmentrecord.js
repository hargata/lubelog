function showAddEquipmentRecordModal() {
    $.get('/Vehicle/GetAddEquipmentRecordPartialView', function (data) {
        if (data) {
            $("#equipmentRecordModalContent").html(data);
            initTagSelector($("#equipmentRecordTag"));
            $('#equipmentRecordModal').modal('show');
        }
    });
}
function showEditEquipmentRecordModal(equipmentRecordId, nocache) {
    if (!nocache) {
        var existingContent = $("#equipmentRecordModalContent").html();
        if (existingContent.trim() != '') {
            //check if id is same.
            var existingId = getEquipmentRecordModelData().id;
            if (existingId == equipmentRecordId && $('[data-changed=true]').length > 0) {
                $('#equipmentRecordModal').modal('show');
                $('.cached-banner').show();
                return;
            }
        }
    }
    $.get(`/Vehicle/GetEquipmentRecordForEditById?equipmentRecordId=${equipmentRecordId}`, function (data) {
        if (data) {
            $("#equipmentRecordModalContent").html(data);
            initTagSelector($("#equipmentRecordTag"));
            $('#equipmentRecordModal').modal('show');
            bindModalInputChanges('equipmentRecordModal');
            $('#equipmentRecordModal').off('shown.bs.modal').on('shown.bs.modal', function () {
                if (getGlobalConfig().useMarkDown) {
                    toggleMarkDownOverlay("equipmentRecordNotes");
                }
            });
        }
    });
}
function hideAddEquipmentRecordModal() {
    $('#equipmentRecordModal').modal('hide');
}
function deleteEquipmentRecord(equipmentRecordId) {
    $("#workAroundInput").show();
    Swal.fire({
        title: "Confirm Deletion?",
        text: "Deleted Equipment Records cannot be restored.",
        showCancelButton: true,
        confirmButtonText: "Delete",
        confirmButtonColor: "#dc3545"
    }).then((result) => {
        if (result.isConfirmed) {
            $.post(`/Vehicle/DeleteEquipmentRecordById?equipmentRecordId=${equipmentRecordId}`, function (data) {
                if (data.success) {
                    hideAddEquipmentRecordModal();
                    successToast("Equipment Record Deleted");
                    var vehicleId = GetVehicleId().vehicleId;
                    getVehicleEquipmentRecords(vehicleId);
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
function saveEquipmentRecordToVehicle(isEdit) {
    //get values
    var formValues = getAndValidateEquipmentRecordValues();
    //validate
    if (formValues.hasError) {
        errorToast("Please check the form data");
        return;
    }
    //save to db.
    $.post('/Vehicle/SaveEquipmentRecordToVehicleId', { equipmentRecord: formValues }, function (data) {
        if (data.success) {
            successToast(isEdit ? "Equipment Record Updated" : "Equipment Record Added.");
            hideAddEquipmentRecordModal();
            saveScrollPosition();
            getVehicleEquipmentRecords(formValues.vehicleId);
        } else {
            errorToast(data.message);
        }
    })
}
function getAndValidateEquipmentRecordValues() {
    var equipmentDescription = $("#equipmentRecordDescription").val();
    var equipmentNotes = $("#equipmentRecordNotes").val();
    var equipmentTags = $("#equipmentRecordTag").val();
    var equipmentIsEquipped = $("#equipmentEquippedCheck").is(":checked");
    var vehicleId = GetVehicleId().vehicleId;
    var equipmentRecordId = getEquipmentRecordModelData().id;
    //validation
    var hasError = false;
    var extraFields = getAndValidateExtraFields();
    if (extraFields.hasError) {
        hasError = true;
    }
    if (equipmentDescription.trim() == '') {
        hasError = true;
        $("#equipmentRecordDescription").addClass("is-invalid");
    } else {
        $("#equipmentRecordDescription").removeClass("is-invalid");
    }
    return {
        id: equipmentRecordId,
        hasError: hasError,
        vehicleId: vehicleId,
        description: equipmentDescription,
        isEquipped: equipmentIsEquipped,
        notes: equipmentNotes,
        files: uploadedFiles,
        tags: equipmentTags,
        extraFields: extraFields.extraFields
    }
}