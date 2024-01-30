function showAddVehicleModal() {
    uploadedFile = "";
    $.get('/Vehicle/AddVehiclePartialView', function (data) {
        if (data) {
            $("#addVehicleModalContent").html(data);
        }
    })
    $('#addVehicleModal').modal('show');
}
function hideAddVehicleModal() {
    $('#addVehicleModal').modal('hide');
}
//refreshable function to reload Garage PartialView
function loadGarage() {
    $.get('/Home/Garage', function (data) {
        $("#garageContainer").html(data);
        loadSettings();
    });
}
function loadSettings() {
    $.get('/Home/Settings', function (data) {
        $("#settings-tab-pane").html(data);
    });
}
function performLogOut() {
    $.post('/Login/LogOut', function (data) {
        if (data) {
            window.location.href = '/Login';
        }
    })
}
function loadPinnedNotes(vehicleId) {
    var hoveredGrid = $(`#gridVehicle_${vehicleId}`);
    if (hoveredGrid.attr("data-bs-title") == undefined) {
        $.get(`/Vehicle/GetPinnedNotesByVehicleId?vehicleId=${vehicleId}`, function (data) {
            if (data.length > 0) {
                //converted pinned notes to html.
                var htmlString = "<ul class='list-group list-group-flush'>";
                data.forEach(x => {
                    htmlString += `<li><b>${x.description}</b> : ${x.noteText}</li>`;
                });
                htmlString += "</ul>";
                hoveredGrid.attr("data-bs-title", htmlString);
                new bootstrap.Tooltip(hoveredGrid);
                hoveredGrid.tooltip("show");
            } else {
                hoveredGrid.attr("data-bs-title", "");
            }
        });
    } else {
        hoveredGrid.tooltip("show");
    }
}
function hidePinnedNotes(vehicleId) {
    $(`#gridVehicle_${vehicleId}`).tooltip("hide");
}