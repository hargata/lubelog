function getYear() {
    return $("#yearOption").val() ?? '0';
}
function getAndValidateSelectedColumns() {
    var reportVisibleColumns = [];
    var reportExtraFields = [];
    var tagFilterMode = $("#tagSelector").val();
    var tagsToFilter = $("#tagSelectorInput").val();
    var filterByDateRange = $("#dateRangeSelector").is(":checked");
    var printIndividualRecords = $("#printIndividualRecordsCheck").is(":checked");
    var startDate = $("#dateRangeStartDate").val();
    var endDate = $("#dateRangeEndDate").val();
    $("#columnSelector :checked").map(function () {
        if ($(this).hasClass('column-default')) {
            reportVisibleColumns.push(this.value);
        } else {
            reportExtraFields.push(this.value);
        }
    });
    var hasValidationError = false;
    var validationErrorMessage = "";
    if (reportVisibleColumns.length + reportExtraFields.length == 0) {
        hasValidationError = true;
        validationErrorMessage = "You must select at least one column";
    }
    if (filterByDateRange) {
        //validate date range
        let startDateTicks = $("#dateRangeStartDate").datepicker('getDate')?.getTime();
        let endDateTicks = $("#dateRangeEndDate").datepicker('getDate')?.getTime();
        if (!startDateTicks || !endDateTicks || startDateTicks > endDateTicks) {
            hasValidationError = true;
            validationErrorMessage = "Invalid date range";
        }
    }

    if (hasValidationError) {
        return {
            hasError: true,
            errorMessage: validationErrorMessage,
            visibleColumns: [],
            extraFields: [],
            tagFilter: tagFilterMode,
            tags: [],
            filterByDateRange: filterByDateRange,
            startDate: '',
            endDate: '',
            printIndividualRecords: printIndividualRecords
        }
    } else {
        return {
            hasError: false,
            errorMessage: '',
            visibleColumns: reportVisibleColumns,
            extraFields: reportExtraFields,
            tagFilter: tagFilterMode,
            tags: tagsToFilter,
            filterByDateRange: filterByDateRange,
            startDate: startDate,
            endDate: endDate,
            printIndividualRecords: printIndividualRecords
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
        });
        selectedReportColumns.visibleColumns.map(x => {
            $(`[value='${x}'].column-default`).prop('checked', true);
        });
        $("#tagSelector").val(selectedReportColumns.tagFilter);
        selectedReportColumns.tags.map(x => {
            $("#tagSelectorInput").append(`<option value='${x}'>${x}</option>`)
        });
        $("#dateRangeSelector").prop('checked', selectedReportColumns.filterByDateRange);
        $("#dateRangeStartDate").val(selectedReportColumns.startDate);
        $("#dateRangeEndDate").val(selectedReportColumns.endDate);
        $("#printIndividualRecordsCheck").prop('checked', selectedReportColumns.printIndividualRecords);
    }
}
function generateVehicleHistoryReport() {
    var vehicleId = GetVehicleId().vehicleId;
    $.get(`/Vehicle/GetReportParameters`, function (data) {
        if (data) {
            //prompt user to select columns
            Swal.fire({
                html: data,
                confirmButtonText: 'Generate Report',
                focusConfirm: false,
                preConfirm: () => {
                    //validate
                    var selectedColumnsData = getAndValidateSelectedColumns();
                    if (selectedColumnsData.hasError) {
                        Swal.showValidationMessage(selectedColumnsData.errorMessage);
                    }
                    return { selectedColumnsData }
                },
                didOpen: () => {
                    getSavedReportParameters();
                    initTagSelector($("#tagSelectorInput"));
                    initDatePicker($('#dateRangeStartDate'));
                    initDatePicker($('#dateRangeEndDate'));
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
                            printContainer(data);
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
        refreshReportHeader();
    })
}
function refreshReportHeader() {
    var vehicleId = GetVehicleId().vehicleId;
    var year = getYear();
    $.post('/Vehicle/GetSummaryForVehicle', { vehicleId: vehicleId, year: year }, function (data) {
        $("#reportHeaderContent").html(data);
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
function showBarChartTable(elemClicked) {
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
            //highlight clicked row.
            if (elemClicked.length > 0) {
                var rowClickedIndex = elemClicked[0].index + 1;
                var rowToHighlight = $("#vehicleDataTableModalContent").find(`tbody > tr:nth-child(${rowClickedIndex})`);
                if (rowToHighlight.length > 0) {
                    rowToHighlight.addClass('table-info');
                }
            }
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
function toggleCostTableHint() {
    if ($(".cost-table-hint").hasClass("d-none")) {
        $(".cost-table-hint").removeClass("d-none");
    } else {
        $(".cost-table-hint").addClass("d-none");
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
function showDataTable(elemClicked) {
    var vehicleId = GetVehicleId().vehicleId;
    var year = getYear();
    $.get(`/Vehicle/GetCostTableForVehicle?vehicleId=${vehicleId}`, { year: year }, function (data) {
        $("#vehicleDataTableModalContent").html(data);
        $("#vehicleDataTableModal").modal('show');
        if (elemClicked.length > 0) {
            var rowClickedIndex = elemClicked[0].index + 1;
            var rowToHighlight = $("#vehicleDataTableModalContent").find(`tbody > tr:nth-child(${rowClickedIndex})`);
            if (rowToHighlight.length > 0) {
                rowToHighlight.addClass('table-info');
            }
        }
    });
}
function hideDataTable() {
    $("#vehicleDataTableModal").modal('hide');
}
function loadVehicleImageMap() {
    var vehicleId = GetVehicleId().vehicleId;
    $.get(`/Vehicle/GetVehicleImageMap?vehicleId=${vehicleId}`, function (data) {
        $("#vehicleDataTableModalContent").html(data);
        $("#vehicleDataTableModal").modal('show');
    });
}
function loadRecordsByTags(tags) {
    $.post('/Vehicle/SearchRecordsByTags', { vehicleId: GetVehicleId().vehicleId, tags: tags }, function (data) {
        $('#vehicleMaintenanceMapResults').html(data);
        $('#vehicleMaintenanceMapResults').show();
    });
}
function loadMapSearchResult(id, recordType) {
    hideDataTable();
    loadGlobalSearchResult(id, recordType);
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

function showReportAdvancedParameters() {
    if ($(".report-advanced-parameters").hasClass("d-none")) {
        $(".report-advanced-parameters").removeClass("d-none");
    } else {
        $(".report-advanced-parameters").addClass("d-none");
    }
}