function returnToGarage() {
    window.location.href = '/Home';
}
function saveVehicleNote(vehicleId) {
    var noteText = $("#noteTextArea").val();
    $.post('/Vehicle/SaveNoteToVehicle', { vehicleId: vehicleId, noteText: noteText }, function (data) {
        if (data) {
            //window.location.href = '/Home';
        }
    })
}
//tap into tabchange 
$(document).ready(function () {
    var vehicleId = GetVehicleId().vehicleId;
    getVehicleNote(vehicleId);
});

function getVehicleNote(vehicleId) {
    $.get(`/Vehicle/GetNoteByVehicleId?vehicleId=${vehicleId}`, function (data) {
        if (data) {
            $("#noteTextArea").val(data);
        }
    });
}