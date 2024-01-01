function showAddVehicleModal() {
    $('#addVehicleModal').modal('show');
}
function hideAddVehicleModal() {
    $('#addVehicleModal').modal('hide');
}
$(document).ready(function () {
    loadGarage();
});
//refreshable function to reload Garage PartialView
function loadGarage() {
    $.get('/Home/Garage', function (data) {
        $("#garageContainer").html(data);
    });
}
function viewVehicle(vehicleId) {
    window.location.href = `/Vehicle/Index?vehicleId=${vehicleId}`;
}
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