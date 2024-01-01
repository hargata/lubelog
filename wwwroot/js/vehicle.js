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
            case "notes-tab":
                getVehicleNote(vehicleId);
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
            $("#ServiceRecordContainer").html(data);
            //initiate datepicker
            $('#serviceRecordDate').datepicker({
                endDate: "+0d"
            });
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
    $('#addServiceRecordModal').modal('show');
}
function hideAddServiceRecordModal() {
    $('#addServiceRecordModal').modal('hide');
}
function addServiceRecordToVehicle() {
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
            successToast("Service Record added.");
            hideAddServiceRecordModal();
            getVehicleServiceRecords(formValues.vehicleId);
        } else {
            errorToast("An error has occurred, please try again later.");
        }
    })
}
function getAndValidateServiceRecordValues() {
    var serviceDate = $("#serviceRecordDate").val();
    var serviceMileage = $("#serviceRecordMileage").val();
    var serviceDescription = $("#serviceRecordDescription").val();
    var serviceCost = $("#serviceRecordCost").val();
    var serviceNotes = $("#serviceRecordNotes").val();
    var vehicleId = GetVehicleId().vehicleId;
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
        hasError: hasError,
        vehicleId: vehicleId,
        date: serviceDate,
        mileage: serviceMileage,
        description: serviceDescription,
        cost: serviceCost,
        notes: serviceNotes
    }
}