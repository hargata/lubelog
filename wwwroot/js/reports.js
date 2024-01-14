function getYear() {
    return $("#yearOption").val();
}
function generateVehicleHistoryReport() {
    var vehicleId = GetVehicleId().vehicleId;
    $.get(`/Vehicle/GetVehicleHistory?vehicleId=${vehicleId}`, function (data) {
        if (data) {
            $("#vehicleHistoryReport").html(data);
            setTimeout(function () {
                window.print();
            }, 500);
        }
    })
}
var debounce = null;
function updateCheck(sender) {
    clearTimeout(debounce);
    debounce = setTimeout(function () {
        refreshBarChart();
    }, 1000);
}
function refreshMPGChart() {
    var vehicleId = GetVehicleId().vehicleId;
    var year = getYear();
    $.post('/Vehicle/GetMonthMPGByVehicle', {vehicleId: vehicleId, year: year}, function (data) {
        $("#monthFuelMileageReportContent").html(data);
    })
}
function refreshBarChart(callBack) {
    var selectedMetrics = [];
    var vehicleId = GetVehicleId().vehicleId;
    var year = getYear();

    if ($("#serviceExpenseCheck").is(":checked")) {
        selectedMetrics.push('ServiceRecord');
    }
    if ($("#repairExpenseCheck").is(":checked")) {
        selectedMetrics.push('RepairRecord');
    }
    if ($("#upgradeExpenseCheck").is(":checked")) {
        selectedMetrics.push('UpgradeRecord');
    }
    if ($("#gasExpenseCheck").is(":checked")) {
        selectedMetrics.push('GasRecord');
    }
    if ($("#taxExpenseCheck").is(":checked")) {
        selectedMetrics.push('TaxRecord');
    }

    $.post('/Vehicle/GetCostByMonthByVehicle',
        {
            vehicleId: vehicleId,
            selectedMetrics: selectedMetrics,
            year: year
        }, function (data) {
            $("#gasCostByMonthReportContent").html(data);
                refreshMPGChart();
        });
}
function updateReminderPie() {
    var vehicleId = GetVehicleId().vehicleId;
    var daysToAdd = $("#reminderOption").val();
    $.get(`/Vehicle/GetReminderMakeUpByVehicle?vehicleId=${vehicleId}`, { daysToAdd: daysToAdd }, function (data) {
        $("#reminderMakeUpReportContent").html(data);
    });
}
//called when year selected is changed.
function yearUpdated() {
    var vehicleId = GetVehicleId().vehicleId;
    var year = getYear();
    $.get(`/Vehicle/GetCostMakeUpForVehicle?vehicleId=${vehicleId}`, { year: year }, function (data) {
        $("#costMakeUpReportContent").html(data);
        refreshBarChart();
    })
}
function refreshCollaborators() {
    var vehicleId = GetVehicleId().vehicleId;
    $.get(`/Vehicle/GetCollaboratorsForVehicle?vehicleId=${vehicleId}`, function (data) {
        $("#collaboratorContent").html(data);
    });
}