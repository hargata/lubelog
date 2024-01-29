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
function updateCheck() {
    setDebounce(refreshBarChart);
}
function refreshMPGChart() {
    var vehicleId = GetVehicleId().vehicleId;
    var year = getYear();
    $.post('/Vehicle/GetMonthMPGByVehicle', {vehicleId: vehicleId, year: year}, function (data) {
        $("#monthFuelMileageReportContent").html(data);
    })
}
function refreshBarChart() {
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
function exportAttachments() {
    Swal.fire({
        title: 'Export Attachments',
        html: `
        <div id='attachmentTabs'>
        <div class='form-check form-check-inline'>
        <input type="checkbox" id="exportServiceRecord" class="form-check-input me-1" value='ServiceRecord'>
        <label for="exportServiceRecord" class='form-check-label'>Service Record</label>
        </div>
        <div class='form-check form-check-inline'>
        <input type="checkbox" id="exportRepairRecord" class="form-check-input me-1" value='RepairRecord'>
        <label for="exportRepairRecord" class='form-check-label'>Repairs</label>
        </div>
        <div class='form-check form-check-inline'>
        <input type="checkbox" id="exportUpgradeRecord" class="form-check-input me-1" value='UpgradeRecord'>
        <label for="exportUpgradeRecord" class='form-check-label'>Upgrades</label>
        </div>
        <div class='form-check form-check-inline'>
        <input type="checkbox" id="exportGasRecord" class="form-check-input me-1" value='GasRecord'>
        <label for="exportGasRecord" class='form-check-label'>Fuel</label>
        </div>
        <div class='form-check form-check-inline'>
        <input type="checkbox" id="exportTaxRecord" class="form-check-input me-1" value='TaxRecord'>
        <label for="exportTaxRecord" class='form-check-label'>Taxes</label>
        </div>
         <div class='form-check form-check-inline'>
        <input type="checkbox" id="exportOdometerRecord" class="form-check-input me-1" value='OdometerRecord'>
        <label for="exportOdometerRecord" class='form-check-label'>Odometer</label>
        </div>
        </div>
        `,
        confirmButtonText: 'Export',
        showCancelButton: true,
        focusConfirm: false,
        preConfirm: () => {
            var selectedExportTabs = $("#attachmentTabs :checked").map(function () {
                return this.value;
            });
            if (selectedExportTabs.toArray().length == 0) {
                Swal.showValidationMessage(`Please make at least one selection`)
            }
            return { selectedTabs: selectedExportTabs.toArray() }
        },
    }).then(function (result) {
        if (result.isConfirmed) {
            var vehicleId = GetVehicleId().vehicleId;
            $.post('/Vehicle/GetVehicleAttachments', { vehicleId: vehicleId, exportTabs: result.value.selectedTabs }, function (data) {
                if (data.success) {
                    window.location.href = data.message;
                } else {
                    errorToast(data.message);
                }
            })
        }
    });
}