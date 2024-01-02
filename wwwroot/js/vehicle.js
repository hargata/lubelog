function returnToGarage() {
    window.location.href = '/Home';
}
function saveVehicleNote(vehicleId) {
    var noteText = $("#noteTextArea").val();
    $.post('/Vehicle/SaveNoteToVehicle', { vehicleId: vehicleId, noteText: noteText }, function (data) {
        if (data) {
            successToast("Note saved successfully.");
        } else {
            errorToast("An error has occurred, please try again later.");
        }
    })
}
$(document).ready(function () {
    var vehicleId = GetVehicleId().vehicleId;
    //bind tabs
    $('button[data-bs-toggle="tab"]').on('show.bs.tab', function (e) {
        switch (e.target.id) {
            case "servicerecord-tab":
                getVehicleServiceRecords(vehicleId);
                break;
            case "notes-tab":
                getVehicleNote(vehicleId);
                break;
        }
        switch (e.relatedTarget.id) { //clear out previous tabs with grids in them to help with performance
            case "servicerecord-tab":
                $("#servicerecord-tab-pane").html("");
                break;
        }
    });
    getVehicleServiceRecords(vehicleId);
});

function getVehicleNote(vehicleId) {
    $.get(`/Vehicle/GetNoteByVehicleId?vehicleId=${vehicleId}`, function (data) {
        if (data) {
            $("#noteTextArea").val(data);
        }
    });
}
function getVehicleServiceRecords(vehicleId) {
    $.get(`/Vehicle/GetServiceRecordsByVehicleId?vehicleId=${vehicleId}`, function (data) {
        if (data) {
            $("#servicerecord-tab-pane").html(data);
        }
    })
}
function DeleteVehicle(vehicleId) {
    $.post('/Vehicle/DeleteVehicle', { vehicleId: vehicleId }, function (data) {
        if (data) {
            window.location.href = '/Home';
        }
    })
}
function showAddServiceRecordModal() {
    $.get('/Vehicle/GetAddServiceRecordPartialView', function (data) {
        if (data) {
            $("#serviceRecordModalContent").html(data);
            //initiate datepicker
            $('#serviceRecordDate').datepicker({
                endDate: "+0d"
            });
            $('#serviceRecordModal').modal('show');
        }
    });
}
function showEditServiceRecordModal(serviceRecordId) {
    $.get(`/Vehicle/GetServiceRecordForEditById?serviceRecordId=${serviceRecordId}`, function (data) {
        if (data) {
            $("#serviceRecordModalContent").html(data);
            //initiate datepicker
            $('#serviceRecordDate').datepicker({
                endDate: "+0d"
            });
            $('#serviceRecordModal').modal('show');
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
                    successToast("Service Record deleted");
                    var vehicleId = GetVehicleId().vehicleId;
                    getVehicleServiceRecords(vehicleId);
                } else {
                    errorToast("An error has occurred, please try again later.");
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
            getVehicleServiceRecords(formValues.vehicleId);
        } else {
            errorToast("An error has occurred, please try again later.");
        }
    })
}
function uploadServiceRecordFilesAsync() {
    let formData = new FormData();
    var files = $("#serviceRecordFiles")[0].files;
    for (var x = 0; x < files.length; x++) {
        formData.append("file", files[x]);
    }
    $.ajax({
        url: "/Files/HandleMultipleFileUpload",
        data: formData,
        cache: false,
        processData: false,
        contentType: false,
        type: 'POST',
        success: function (response) {
            if (response.length > 0) {
                uploadedFiles.push.apply(uploadedFiles, response);
            }
        }
    });
}
function getAndValidateServiceRecordValues() {
    var serviceDate = $("#serviceRecordDate").val();
    var serviceMileage = $("#serviceRecordMileage").val();
    var serviceDescription = $("#serviceRecordDescription").val();
    var serviceCost = $("#serviceRecordCost").val();
    var serviceNotes = $("#serviceRecordNotes").val();
    var vehicleId = GetVehicleId().vehicleId;
    var serviceRecordId = getServiceRecordModelData().id;
    //validation
    var hasError = false;
    if (serviceDate.trim() == '') { //eliminates whitespace.
        hasError = true;
        $("#serviceRecordDate").addClass("is-invalid");
    } else {
        $("#serviceRecordDate").removeClass("is-invalid");
    }
    if (serviceMileage.trim() == '' || parseInt(serviceMileage) < 0) {
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
    if (serviceCost.trim() == '') {
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
        files: uploadedFiles
    }
}
function deleteServiceRecordFile(fileLocation, event) {
    event.parentElement.remove();
    uploadedFiles = uploadedFiles.filter(x => x.location != fileLocation);
}