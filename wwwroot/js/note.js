function showAddNoteModal() {
    $.get('/Vehicle/GetAddNotePartialView', function (data) {
        if (data) {
            $("#noteModalContent").html(data);
            initTagSelector($("#noteRecordTag"));
            $('#noteModal').modal('show');
        }
    });
}
function showEditNoteModal(noteId, nocache) {
    if (!nocache) {
        var existingContent = $("#noteModalContent").html();
        if (existingContent.trim() != '') {
            //check if id is same.
            var existingId = getNoteModelData().id;
            if (existingId == noteId && $('[data-changed=true]').length > 0) {
                $('#noteModal').modal('show');
                $('.cached-banner').show();
                return;
            }
        }
    }
    $.get(`/Vehicle/GetNoteForEditById?noteId=${noteId}`, function (data) {
        if (data) {
            $("#noteModalContent").html(data);
            initTagSelector($("#noteRecordTag"));
            $('#noteModal').modal('show');
            bindModalInputChanges('noteModal');
            $('#noteModal').off('shown.bs.modal').on('shown.bs.modal', function () {
                if (getGlobalConfig().useMarkDown) {
                    toggleMarkDownOverlay("noteTextArea");
                }
            });
        }
    });
}
function hideAddNoteModal() {
    $('#noteModal').modal('hide');
}
function deleteNote(noteId) {
    $("#workAroundInput").show();
    Swal.fire({
        title: "Confirm Deletion?",
        text: "Deleted Notes cannot be restored.",
        showCancelButton: true,
        confirmButtonText: "Delete",
        confirmButtonColor: "#dc3545"
    }).then((result) => {
        if (result.isConfirmed) {
            $.post(`/Vehicle/DeleteNoteById?noteId=${noteId}`, function (data) {
                if (data) {
                    hideAddNoteModal();
                    successToast("Note Deleted");
                    var vehicleId = GetVehicleId().vehicleId;
                    getVehicleNotes(vehicleId);
                } else {
                    errorToast(genericErrorMessage());
                }
            });
        } else {
            $("#workAroundInput").hide();
        }
    });
}
function saveNoteToVehicle(isEdit) {
    //get values
    var formValues = getAndValidateNoteValues();
    //validate
    if (formValues.hasError) {
        errorToast("Please check the form data");
        return;
    }
    //save to db.
    $.post('/Vehicle/SaveNoteToVehicleId', { note: formValues }, function (data) {
        if (data) {
            successToast(isEdit ? "Note Updated" : "Note Added.");
            hideAddNoteModal();
            saveScrollPosition();
            getVehicleNotes(formValues.vehicleId);
        } else {
            errorToast(genericErrorMessage());
        }
    })
}
function getAndValidateNoteValues() {
    var noteDescription = $("#noteDescription").val();
    var noteText = $("#noteTextArea").val();
    var vehicleId = GetVehicleId().vehicleId;
    var noteId = getNoteModelData().id;
    var noteIsPinned = $("#noteIsPinned").is(":checked");
    var noteTags = $("#noteRecordTag").val();
    //validation
    var hasError = false;
    if (noteDescription.trim() == '') { //eliminates whitespace.
        hasError = true;
        $("#noteDescription").addClass("is-invalid");
    } else {
        $("#noteDescription").removeClass("is-invalid");
    }
    if (noteText.trim() == '') {
        hasError = true;
        $("#noteTextArea").addClass("is-invalid");
    } else {
        $("#noteTextArea").removeClass("is-invalid");
    }
    return {
        id: noteId,
        hasError: hasError,
        vehicleId: vehicleId,
        description: noteDescription,
        noteText: noteText,
        files: uploadedFiles,
        pinned: noteIsPinned,
        tags: noteTags
    }
}
function pinNotes(ids, toggle, pinStatus) {
    $.post('/Vehicle/PinNotes', { noteIds: ids, isToggle: toggle, pinStatus: pinStatus  }, function (data) {
        if (data) {
            successToast(ids.length > 1 ? `${ids.length} Notes Updated` : "Note Updated.");
            getVehicleNotes(GetVehicleId().vehicleId);
        }
    })
}