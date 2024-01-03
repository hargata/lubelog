function showAddVehicleModal() {
    uploadedFile = "";
    $.get('/Home/AddVehiclePartialView', function (data) {
        if (data) {
            $("#addVehicleModalContent").html(data);
        }
    })
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