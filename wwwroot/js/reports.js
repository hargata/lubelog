function getYear() {
    return $("#yearOption").val() ?? '0';
}
function getAndValidateSelectedColumns() {
    var reportVisibleColumns = [];
    var reportExtraFields = [];
    $("#columnSelector :checked").map(function () {
        if ($(this).hasClass('column-default')) {
            reportVisibleColumns.push(this.value);
        } else {
            reportExtraFields.push(this.value);
        }
    });
    if (reportVisibleColumns.length + reportExtraFields.length == 0) {
        return {
            hasError: true,
            visibleColumns: [],
            extraFields: []
        }
    } else {
        return {
            hasError: false,
            visibleColumns: reportVisibleColumns,
            extraFields: reportExtraFields
        }
    }
}
function getSavedReportParameters() {
    var vehicleId = GetVehicleId().vehicleId;
    var selectedReportColumns = sessionStorage.getItem(`${vehicleId}_selectedReportColumns`);
    if (selectedReportColumns != null) {
        selectedReportColumns = JSON.parse(selectedReportColumns);
        //unselected everything
        $(".column-extrafield").prop('checked', false);
        $(".column-default").prop('checked', false);
        //load selected checkboxes
        selectedReportColumns.extraFields.map(x => {
            $(`[value='${x}'].column-extrafield`).prop('checked', true);
        })
        selectedReportColumns.visibleColumns.map(x => {
            $(`[value='${x}'].column-default`).prop('checked', true);
        })
    }
}
function generateVehicleHistoryReport() {
    var vehicleId = GetVehicleId().vehicleId;
    $.get(`/Vehicle/GetReportParameters`, function (data) {
        if (data) {
            //prompt user to select columns
            Swal.fire({
                title: 'Select Columns',
                html: data,
                confirmButtonText: 'Generate Report',
                focusConfirm: false,
                preConfirm: () => {
                    //validate
                    var selectedColumnsData = getAndValidateSelectedColumns();
                    if (selectedColumnsData.hasError) {
                        Swal.showValidationMessage(`You must select at least one column`);
                    }
                    return { selectedColumnsData }
                },
                didOpen: () => {
                    getSavedReportParameters();
                }
            }).then(function (result) {
                if (result.isConfirmed) {
                    //save params in sessionStorage
                    sessionStorage.setItem(`${vehicleId}_selectedReportColumns`, JSON.stringify(result.value.selectedColumnsData));
                    //post params
                    $.post(`/Vehicle/GetVehicleHistory?vehicleId=${vehicleId}`, {
                        reportParameter: result.value.selectedColumnsData
                    }, function (data) {
                        if (data) {
                            $("#vehicleHistoryReport").html(data);
                            setTimeout(function () {
                                window.print();
                            }, 500);
                        }
                    })
                }
            });
        } else {
            errorToast(genericErrorMessage());
        }
    })
}
function updateCheckAll() {
    var isChecked = $("#selectAllExpenseCheck").is(":checked");
    $(".reportCheckBox").prop('checked', isChecked);
    setDebounce(refreshBarChart);
}
function updateCheck() {
    setDebounce(refreshBarChart);
    var allIsChecked = $(".reportCheckBox:checked").length == 6;
    $("#selectAllExpenseCheck").prop("checked", allIsChecked);
}
function refreshMPGChart() {
    var vehicleId = GetVehicleId().vehicleId;
    var year = getYear();
    $.post('/Vehicle/GetMonthMPGByVehicle', {vehicleId: vehicleId, year: year}, function (data) {
        $("#monthFuelMileageReportContent").html(data);
    })
}
function setSelectedMetrics() {
    var selectedMetricCheckBoxes = [];
    $(".reportCheckBox:checked").map((index, elem) => {
        selectedMetricCheckBoxes.push(elem.id);
    });
    var yearMetric = $('#yearOption').val();
    var reminderMetric = $("#reminderOption").val();
    var vehicleId = GetVehicleId().vehicleId;
    sessionStorage.setItem(`${vehicleId}_selectedMetricCheckBoxes`, JSON.stringify(selectedMetricCheckBoxes));
    sessionStorage.setItem(`${vehicleId}_yearMetric`, yearMetric);
    sessionStorage.setItem(`${vehicleId}_reminderMetric`, reminderMetric);
}
function getSelectedMetrics() {
    var vehicleId = GetVehicleId().vehicleId;
    var selectedMetricCheckBoxes = sessionStorage.getItem(`${vehicleId}_selectedMetricCheckBoxes`);
    var yearMetric = sessionStorage.getItem(`${vehicleId}_yearMetric`);
    var reminderMetric = sessionStorage.getItem(`${vehicleId}_reminderMetric`);
    if (selectedMetricCheckBoxes != null && yearMetric != null && reminderMetric != null) {
        selectedMetricCheckBoxes = JSON.parse(selectedMetricCheckBoxes);
        $(".reportCheckBox").prop('checked', false);
        $("#selectAllExpenseCheck").prop("checked", false);
        selectedMetricCheckBoxes.map(x => {
            $(`#${x}`).prop('checked', true);
        });
        if (selectedMetricCheckBoxes.length == 6) {
            $("#selectAllExpenseCheck").prop("checked", true);
        }
        //check if option is available
        if ($("#yearOption").has(`option[value=${yearMetric}]`).length > 0) {
            $('#yearOption').val(yearMetric);
        }
        $("#reminderOption").val(reminderMetric);
        //retrieve data.
        yearUpdated();
        updateReminderPie();
        return true;
    }
    return false;
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
    if ($("#odometerExpenseCheck").is(":checked")) {
        selectedMetrics.push('OdometerRecord');
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
    setSelectedMetrics();
}
function showBarChartTable() {
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
    if ($("#odometerExpenseCheck").is(":checked")) {
        selectedMetrics.push('OdometerRecord');
    }

    $.post('/Vehicle/GetCostByMonthAndYearByVehicle',
        {
            vehicleId: vehicleId,
            selectedMetrics: selectedMetrics,
            year: year
        }, function (data) {
            $("#vehicleDataTableModalContent").html(data);
            $("#vehicleDataTableModal").modal('show');
        });
}
function toggleBarChartTableData() {
    //find out which column data type is shown
    if (!$('[report-data="cost"]').hasClass('d-none')) {
        //currently cost is shown.
        $('[report-data="cost"]').addClass('d-none');
        $('[report-data="distance"]').removeClass('d-none');
    }
    else if (!$('[report-data="distance"]').hasClass('d-none')) {
        //currently distance is shown.
        $('[report-data="distance"]').addClass('d-none');
        $('[report-data="costperdistance"]').removeClass('d-none');
    }
    else if (!$('[report-data="costperdistance"]').hasClass('d-none')) {
        //currently cost per distance is shown.
        $('[report-data="costperdistance"]').addClass('d-none');
        $('[report-data="cost"]').removeClass('d-none');
    }
}
function updateReminderPie() {
    var vehicleId = GetVehicleId().vehicleId;
    var daysToAdd = $("#reminderOption").val();
    setSelectedMetrics();
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
        <div class='form-check form-check-inline'>
        <input type="checkbox" id="exportNoteRecord" class="form-check-input me-1" value='NoteRecord'>
        <label for="exportNoteRecord" class='form-check-label'>Notes</label>
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
function showDataTable() {
    var vehicleId = GetVehicleId().vehicleId;
    var year = getYear();
    $.get(`/Vehicle/GetCostTableForVehicle?vehicleId=${vehicleId}`, { year: year }, function (data) {
        $("#vehicleDataTableModalContent").html(data);
        $("#vehicleDataTableModal").modal('show');
    });
}
function hideDataTable() {
    $("#vehicleDataTableModal").modal('hide');
}
function showGlobalSearch() {
    $('#globalSearchModal').modal('show');
}
function hideGlobalSearch() {
    $('#globalSearchModal').modal('hide');
}
function performGlobalSearch() {
    var searchQuery = $('#globalSearchInput').val();
    if (searchQuery.trim() == '') {
        $('#globalSearchInput').addClass('is-invalid');
    } else {
        $('#globalSearchInput').removeClass('is-invalid');
    }
    $.post('/Vehicle/SearchRecords', { vehicleId: GetVehicleId().vehicleId, searchQuery: searchQuery }, function (data) {
        $('#globalSearchModalResults').html(data);
    });
}
function handleGlobalSearchKeyPress(event) {
    if ($('#globalSearchAutoSearchCheck').is(':checked')){
        setDebounce(performGlobalSearch);
    } else if (event.keyCode == 13) {
        performGlobalSearch();
    }
}

function loadGlobalSearchResult(recordId, recordType) {
    hideGlobalSearch();
    switch (recordType) {
        case "ServiceRecord":
            $('#servicerecord-tab').tab('show');
            waitForElement('#serviceRecordModalContent', showEditServiceRecordModal, recordId);
            break;
        case "RepairRecord":
            $('#accident-tab').tab('show');
            waitForElement('#collisionRecordModalContent', showEditCollisionRecordModal, recordId);
            break;
        case "UpgradeRecord":
            $('#upgrade-tab').tab('show');
            waitForElement('#upgradeRecordModalContent', showEditUpgradeRecordModal, recordId);
            break;
        case "TaxRecord":
            $('#tax-tab').tab('show');
            waitForElement('#taxRecordModalContent', showEditTaxRecordModal, recordId);
            break;
        case "SupplyRecord":
            $('#supply-tab').tab('show');
            waitForElement('#supplyRecordModalContent', showEditSupplyRecordModal, recordId);
            break;
        case "NoteRecord":
            $('#notes-tab').tab('show');
            waitForElement('#noteModalContent', showEditNoteModal, recordId);
            break;
        case "OdometerRecord":
            $('#odometer-tab').tab('show');
            waitForElement('#odometerRecordModalContent', showEditOdometerRecordModal, recordId);
            break;
        case "ReminderRecord":
            $('#reminder-tab').tab('show');
            waitForElement('#reminderRecordModalContent', showEditReminderRecordModal, recordId);
            break;
        case "GasRecord":
            $('#gas-tab').tab('show');
            waitForElement('#gasRecordModalContent', showEditGasRecordModal, recordId);
            break;
        case "PlanRecord":
            $('#plan-tab').tab('show');
            waitForElement('#planRecordModalContent', showEditPlanRecordModal, recordId);
            break;
    }
}
function loadCustomWidgets() {
    $.get('/Vehicle/GetAdditionalWidgets', function (data) {
        $("#vehicleCustomWidgetsModalContent").html(data);
        $("#vehicleCustomWidgetsModal").modal('show');
    })
}
function hideCustomWidgetsModal() {
    $("#vehicleCustomWidgetsModal").modal('hide');
}