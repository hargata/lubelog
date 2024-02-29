function successToast(message) {
    Swal.fire({
        toast: true,
        position: "top-end",
        showConfirmButton: false,
        timer: 3000,
        title: message,
        timerProgressBar: true,
        icon: "success",
        didOpen: (toast) => {
            toast.onmouseenter = Swal.stopTimer;
            toast.onmouseleave = Swal.resumeTimer;
        }
    })
}
function errorToast(message) {
    Swal.fire({
        toast: true,
        position: "top-end",
        showConfirmButton: false,
        timer: 3000,
        title: message,
        timerProgressBar: true,
        icon: "error",
        didOpen: (toast) => {
            toast.onmouseenter = Swal.stopTimer;
            toast.onmouseleave = Swal.resumeTimer;
        }
    })
}
function viewVehicle(vehicleId) {
    window.location.href = `/Vehicle/Index?vehicleId=${vehicleId}`;
}
function saveVehicle(isEdit) {
    var vehicleId = getVehicleModelData().id;
    var vehicleYear = $("#inputYear").val();
    var vehicleMake = $("#inputMake").val();
    var vehicleModel = $("#inputModel").val();
    var vehicleTags = $("#inputTag").val();
    var vehicleLicensePlate = $("#inputLicensePlate").val();
    var vehicleIsElectric = $("#inputIsElectric").is(":checked");
    var vehicleUseHours = $("#inputUseHours").is(":checked");
    //validate
    var hasError = false;
    if (vehicleYear.trim() == '' || parseInt(vehicleYear) < 1900) {
        hasError = true;
        $("#inputYear").addClass("is-invalid");
    } else {
        $("#inputYear").removeClass("is-invalid");
    }
    if (vehicleMake.trim() == '') {
        hasError = true;
        $("#inputMake").addClass("is-invalid");
    } else {
        $("#inputMake").removeClass("is-invalid");
    }
    if (vehicleModel.trim() == '') {
        hasError = true;
        $("#inputModel").addClass("is-invalid");
    } else {
        $("#inputModel").removeClass("is-invalid");
    }
    if (vehicleLicensePlate.trim() == '') {
        hasError = true;
        $("#inputLicensePlate").addClass("is-invalid");
    } else {
        $("#inputLicensePlate").removeClass("is-invalid");
    }
    if (hasError) {
        return;
    }
    $.post('/Vehicle/SaveVehicle', {
        id: vehicleId,
        imageLocation: uploadedFile,
        year: vehicleYear,
        make: vehicleMake,
        model: vehicleModel,
        licensePlate: vehicleLicensePlate,
        isElectric: vehicleIsElectric,
        tags: vehicleTags,
        useHours: vehicleUseHours
    }, function (data) {
        if (data) {
            if (!isEdit) {
                successToast("Vehicle Added");
                hideAddVehicleModal();
                loadGarage();
            }
            else {
                successToast("Vehicle Updated");
                hideEditVehicleModal();
                viewVehicle(vehicleId);
            }
        } else {
            errorToast(genericErrorMessage());
        }
    });
}
function uploadFileAsync(event) {
    let formData = new FormData();
    formData.append("file", event.files[0]);
    sloader.show();
    $.ajax({
        url: "/Files/HandleFileUpload",
        data: formData,
        cache: false,
        processData: false,
        contentType: false,
        type: 'POST',
        success: function (response) {
            sloader.hide();
            if (response.trim() != '') {
                uploadedFile = response;
            }
        },
        error: function () {
            sloader.hide();
            errorToast("An error has occurred, please check the file size and try again later.")
        }
    });
}
function isValidMoney(input) {
    const euRegex = /^\$?(?=\(.*\)|[^()]*$)\(?\d{1,3}(\.?\d{3})?(,\d{1,3}?)?\)?$/;
    const usRegex = /^\$?(?=\(.*\)|[^()]*$)\(?\d{1,3}(,?\d{3})?(\.\d{1,3}?)?\)?$/;
    return (euRegex.test(input) || usRegex.test(input));
}
function initDatePicker(input, futureOnly) {
    if (futureOnly) {
        input.datepicker({
            startDate: "+0d",
            format: getShortDatePattern().pattern,
            autoclose: true,
            weekStart: getGlobalConfig().firstDayOfWeek
        });
    } else {
        input.datepicker({
            endDate: "+0d",
            format: getShortDatePattern().pattern,
            autoclose: true,
            weekStart: getGlobalConfig().firstDayOfWeek
        });
    }
}
function initTagSelector(input, noDataList) {
    if (noDataList) {
        input.tagsinput({
            useDataList: false
        });
    } else {
        input.tagsinput();
    }
}

function showMobileNav() {
    $(".lubelogger-mobile-nav").addClass("lubelogger-mobile-nav-show");
}
function hideMobileNav() {
    $(".lubelogger-mobile-nav").removeClass("lubelogger-mobile-nav-show");
}
function bindWindowResize() {
    $(window).on('resize', function () {
        hideMobileNav();
    });
}
function encodeHTMLInput(input) {
    const encoded = document.createElement('div');
    encoded.innerText = input;
    return encoded.innerHTML;
}
function decodeHTMLEntities(text) {
    return $("<textarea/>")
        .html(text)
        .text();
}
var debounce = null;
function setDebounce(callBack) {
    clearTimeout(debounce);
    debounce = setTimeout(function () {
        callBack();
    }, 1000);
}
var storedTableRowState = null;
function toggleSort(tabName, sender) {
    var sortColumn = sender.textContent;
    var sortAscIcon = '<i class="bi bi-sort-numeric-down ms-2"></i>';
    var sortDescIcon = '<i class="bi bi-sort-numeric-down-alt ms-2"></i>';
    sender = $(sender);
    //order of sort - asc, desc, reset
    if (sender.hasClass('sort-asc')) {
        sender.removeClass('sort-asc');
        sender.addClass('sort-desc');
        sender.html(`${sortColumn}${sortDescIcon}`);
        sortTable(tabName, sortColumn, true);
    } else if (sender.hasClass('sort-desc')) {
        //restore table
        sender.removeClass('sort-desc');
        sender.html(`${sortColumn}`);
        $(`#${tabName} table tbody`).html(storedTableRowState);
        filterTable(tabName, $(".tagfilter.bg-primary").get(0), true);
    } else {
        //first time sorting.
        //check if table was sorted before by a different column(only relevant to fuel tab)
        if (storedTableRowState != null && ($(".sort-asc").length > 0 || $(".sort-desc").length > 0)) {
            //restore table state.
            $(`#${tabName} table tbody`).html(storedTableRowState);
            //reset other sorted columns
            if ($(".sort-asc").length > 0) {
                $(".sort-asc").html($(".sort-asc").html().replace(sortAscIcon, ""));
                $(".sort-asc").removeClass("sort-asc");
            }
            if ($(".sort-desc").length > 0) {
                $(".sort-desc").html($(".sort-desc").html().replace(sortDescIcon, ""));
                $(".sort-desc").removeClass("sort-desc");
            }
        }
        sender.addClass('sort-asc');
        sender.html(`${sortColumn}${sortAscIcon}`);
        storedTableRowState = null;
        storedTableRowState = $(`#${tabName} table tbody`).html();
        sortTable(tabName, sortColumn, false);
    }
}
function sortTable(tabName, columnName, desc) {
    //get column index.
    var columns = $(`#${tabName} table th`).toArray().map(x => x.innerText);
    var colIndex = columns.findIndex(x => x == columnName);
    //get row data
    var rowData = $(`#${tabName} table tbody tr`);
    var sortedRow = rowData.toArray().sort((a, b) => {
        var currentVal = globalParseFloat(a.children[colIndex].textContent);
        var nextVal = globalParseFloat(b.children[colIndex].textContent);
        if (desc) {
            return nextVal - currentVal;
        } else {
            return currentVal - nextVal;
        }
    });
    $(`#${tabName} table tbody`).html(sortedRow);
    filterTable(tabName, $(".tagfilter.bg-primary").get(0), true);
}
function filterTable(tabName, sender, isSort) {
    var rowData = $(`#${tabName} table tbody tr`);
    if (sender == undefined) {
        rowData.removeClass('override-hide');
        return;
    }
    var tagName = sender.textContent;
    //check for other applied filters
    if ($(sender).hasClass("bg-primary")) {
        if (!isSort) {
            rowData.removeClass('override-hide');
            $(sender).removeClass('bg-primary');
            $(sender).addClass('bg-secondary');
            updateAggregateLabels();
        } else {
            rowData.addClass('override-hide');
            $(`[data-tags~='${tagName}']`).removeClass('override-hide');
            updateAggregateLabels();
        }
    } else {
        //hide table rows.
        rowData.addClass('override-hide');
        $(`[data-tags~='${tagName}']`).removeClass('override-hide');
        updateAggregateLabels();
        if ($(".tagfilter.bg-primary").length > 0) {
            //disabling other filters
            $(".tagfilter.bg-primary").addClass('bg-secondary');
            $(".tagfilter.bg-primary").removeClass('bg-primary');
        }
        $(sender).addClass('bg-primary');
        $(sender).removeClass('bg-secondary');
    }
}

function updateAggregateLabels() {
    //Sum
    var sumLabel = $("[data-aggregate-type='sum']");
    if (sumLabel.length > 0) {
        var labelsToSum = $("[data-record-type='cost']").parent(":not('.override-hide')").children("[data-record-type='cost']").toArray();
        var newSum = 0;
        if (labelsToSum.length > 0) {
            newSum = labelsToSum.map(x => globalParseFloat(x.textContent)).reduce((a, b,) => a + b).toFixed(2);
        }
        sumLabel.text(`${sumLabel.text().split(':')[0]}: ${getGlobalConfig().currencySymbol}${newSum}`)
    }
    //Count
    var newCount = $("[data-record-type='cost']").parent(":not('.override-hide')").length;
    var countLabel = $("[data-aggregate-type='count']");
    countLabel.text(`${countLabel.text().split(':')[0]}: ${newCount}`)
}

function uploadVehicleFilesAsync(event) {
    let formData = new FormData();
    var files = event.files;
    for (var x = 0; x < files.length; x++) {
        formData.append("file", files[x]);
    }
    sloader.show();
    $.ajax({
        url: "/Files/HandleMultipleFileUpload",
        data: formData,
        cache: false,
        processData: false,
        contentType: false,
        type: 'POST',
        success: function (response) {
            sloader.hide();
            if (response.length > 0) {
                uploadedFiles.push.apply(uploadedFiles, response);
            }
        },
        error: function () {
            sloader.hide();
            errorToast("An error has occurred, please check the file size and try again later.")
        }
    });
}
function deleteFileFromUploadedFiles(fileLocation, event) {
    event.parentElement.parentElement.parentElement.remove();
    uploadedFiles = uploadedFiles.filter(x => x.location != fileLocation);
}
function editFileName(fileLocation, event) {
    Swal.fire({
        title: 'Rename File',
        html: `
                    <input type="text" id="newFileName" class="swal2-input" placeholder="New File Name">
                    `,
        confirmButtonText: 'Rename',
        focusConfirm: false,
        preConfirm: () => {
            const newFileName = $("#newFileName").val();
            if (!newFileName) {
                Swal.showValidationMessage(`Please enter a valid file name`)
            }
            return { newFileName }
        },
    }).then(function (result) {
        if (result.isConfirmed) {
            var linkDisplayObject = $(event.parentElement.parentElement).find('a')[0];
            linkDisplayObject.text = result.value.newFileName;
            var editFileIndex = uploadedFiles.findIndex(x => x.location == fileLocation);
            uploadedFiles[editFileIndex].name = result.value.newFileName;
        }
    });
}
var scrollPosition = 0;
function saveScrollPosition() {
    scrollPosition = $(".vehicleDetailTabContainer").scrollTop();
}
function restoreScrollPosition() {
    $(".vehicleDetailTabContainer").scrollTop(scrollPosition);
    scrollPosition = 0;
}
function toggleMarkDownOverlay(textAreaName) {
    var textArea = $(`#${textAreaName}`);
    if ($(".markdown-overlay").length > 0) {
        $(".markdown-overlay").remove();
        return;
    }
    var text = textArea.val();
    if (text == undefined) {
        return;
    }
    if (text.length > 0) {
        var formatted = markdown(text);
        //var overlay div
        var overlayDiv = `<div class='markdown-overlay' style="z-index: 1060; position:absolute; top:${textArea.css('top')}; left:${textArea.css('left')}; width:${textArea.css('width')}; height:${textArea.css('height')}; padding:${textArea.css('padding')}; overflow-y:auto; background-color:var(--bs-modal-bg);">${formatted}</div>`;
        textArea.parent().children(`label[for=${textAreaName}]`).append(overlayDiv);
    }
}
function showLinks(e) {
    var textAreaName = $(e.parentElement).attr("for");
    toggleMarkDownOverlay(textAreaName);
}
function printTab() {
    setTimeout(function () {
        window.print();
    }, 500);
}
function exportVehicleData(mode) {
    var vehicleId = GetVehicleId().vehicleId;
    $.get('/Vehicle/ExportFromVehicleToCsv', { vehicleId: vehicleId, mode: mode }, function (data) {
        if (!data) {
            errorToast(genericErrorMessage());
        } else {
            window.location.href = data;
        }
    });
}
function showBulkImportModal(mode) {
    $.get(`/Vehicle/GetBulkImportModalPartialView?mode=${mode}`, function (data) {
        if (data) {
            $("#bulkImportModalContent").html(data);
            $("#bulkImportModal").modal('show');
        }
    })
}
function hideBulkImportModal() {
    $("#bulkImportModal").modal('hide');
}
function getAndValidateExtraFields() {
    var hasError = false;
    var outputData = [];
    $(".extra-field").map((index, elem) => {
        var extraFieldName = $(elem).children("label").text();
        var extraFieldInput = $(elem).children("input");
        var extraFieldValue = extraFieldInput.val();
        var extraFieldIsRequired = extraFieldInput.hasClass('extra-field-required');
        if (extraFieldIsRequired && extraFieldValue.trim() == '') {
            hasError = true;
            extraFieldInput.addClass("is-invalid");
        } else {
            extraFieldInput.removeClass("is-invalid");
        }
        //only push fields with value in them
        if (extraFieldValue.trim() != '') {
            outputData.push({ name: extraFieldName, value: extraFieldValue, isRequired: extraFieldIsRequired });
        }
    });
    return { hasError: hasError, extraFields: outputData };
}
function toggleSupplyUsageHistory() {
    var container = $("#supplyUsageHistoryModalContainer");
    if (container.hasClass("d-none")) {
        container.removeClass("d-none");
    } else {
        container.addClass("d-none");
    }
}
function moveRecords(ids, source, dest) {
    if (ids.length == 0) {
        return;
    }
    $("#workAroundInput").show();
    var friendlySource = "";
    var friendlyDest = "";
    var refreshDataCallBack;
    var recordVerbiage = ids.length > 1 ? `these ${ids.length} records` : "this record";
    switch (source) {
        case "ServiceRecord":
            friendlySource = "Service Records";
            refreshDataCallBack = getVehicleServiceRecords;
            break;
        case "RepairRecord":
            friendlySource = "Repairs";
            refreshDataCallBack = getVehicleCollisionRecords;
            break;
        case "UpgradeRecord":
            friendlySource = "Upgrades";
            refreshDataCallBack = getVehicleUpgradeRecords;
            break;
    }
    switch (dest) {
        case "ServiceRecord":
            friendlyDest = "Service Records";
            break;
        case "RepairRecord":
            friendlyDest = "Repairs";
            break;
        case "UpgradeRecord":
            friendlyDest = "Upgrades";
            break;
    }

    Swal.fire({
        title: "Confirm Move?",
        text: `Move ${recordVerbiage} from ${friendlySource} to ${friendlyDest}?`,
        showCancelButton: true,
        confirmButtonText: "Move",
        confirmButtonColor: "#dc3545"
    }).then((result) => {
        if (result.isConfirmed) {
            $.post('/Vehicle/MoveRecords', { recordIds: ids, source: source, destination: dest }, function (data) {
                if (data) {
                    successToast(`${ids.length} Record(s) Moved`);
                    var vehicleId = GetVehicleId().vehicleId;
                    refreshDataCallBack(vehicleId);
                } else {
                    errorToast(genericErrorMessage());
                }
            });
        } else {
            $("#workAroundInput").hide();
        }
    });
}
function deleteRecords(ids, source) {
    if (ids.length == 0) {
        return;
    }
    $("#workAroundInput").show();
    var friendlySource = "";
    var refreshDataCallBack;
    var recordVerbiage = ids.length > 1 ? `these ${ids.length} records` : "this record";
    switch (source) {
        case "ServiceRecord":
            friendlySource = "Service Records";
            refreshDataCallBack = getVehicleServiceRecords;
            break;
        case "RepairRecord":
            friendlySource = "Repairs";
            refreshDataCallBack = getVehicleCollisionRecords;
            break;
        case "UpgradeRecord":
            friendlySource = "Upgrades";
            refreshDataCallBack = getVehicleUpgradeRecords;
            break;
        case "TaxRecord":
            friendlySource = "Taxes";
            refreshDataCallBack = getVehicleTaxRecords;
            break;
        case "SupplyRecord":
            friendlySource = "Supplies";
            refreshDataCallBack = getVehicleSupplyRecords;
            break;
        case "NoteRecord":
            friendlySource = "Notes";
            refreshDataCallBack = getVehicleNotes;
            break;
        case "OdometerRecord":
            friendlySource = "Odometer Records";
            refreshDataCallBack = getVehicleOdometerRecords;
            break;
        case "ReminderRecord":
            friendlySource = "Reminders";
            refreshDataCallBack = getVehicleReminders;
            break;
        case "GasRecord":
            friendlySource = "Fuel Records";
            refreshDataCallBack = getVehicleGasRecords;
            break;
    }

    Swal.fire({
        title: "Confirm Delete?",
        text: `Delete ${recordVerbiage} from ${friendlySource}?`,
        showCancelButton: true,
        confirmButtonText: "Delete",
        confirmButtonColor: "#dc3545"
    }).then((result) => {
        if (result.isConfirmed) {
            $.post('/Vehicle/DeleteRecords', { recordIds: ids, importMode: source }, function (data) {
                if (data) {
                    successToast(`${ids.length} Record(s) Deleted`);
                    var vehicleId = GetVehicleId().vehicleId;
                    refreshDataCallBack(vehicleId);
                } else {
                    errorToast(genericErrorMessage());
                }
            });
        } else {
            $("#workAroundInput").hide();
        }
    });
}
function duplicateRecords(ids, source) {
    if (ids.length == 0) {
        return;
    }
    $("#workAroundInput").show();
    var friendlySource = "";
    var refreshDataCallBack;
    var recordVerbiage = ids.length > 1 ? `these ${ids.length} records` : "this record";
    switch (source) {
        case "ServiceRecord":
            friendlySource = "Service Records";
            refreshDataCallBack = getVehicleServiceRecords;
            break;
        case "RepairRecord":
            friendlySource = "Repairs";
            refreshDataCallBack = getVehicleCollisionRecords;
            break;
        case "UpgradeRecord":
            friendlySource = "Upgrades";
            refreshDataCallBack = getVehicleUpgradeRecords;
            break;
        case "TaxRecord":
            friendlySource = "Taxes";
            refreshDataCallBack = getVehicleTaxRecords;
            break;
        case "SupplyRecord":
            friendlySource = "Supplies";
            refreshDataCallBack = getVehicleSupplyRecords;
            break;
        case "NoteRecord":
            friendlySource = "Notes";
            refreshDataCallBack = getVehicleNotes;
            break;
        case "OdometerRecord":
            friendlySource = "Odometer Records";
            refreshDataCallBack = getVehicleOdometerRecords;
            break;
        case "ReminderRecord":
            friendlySource = "Reminders";
            refreshDataCallBack = getVehicleReminders;
            break;
        case "GasRecord":
            friendlySource = "Fuel Records";
            refreshDataCallBack = getVehicleGasRecords;
            break;
    }

    Swal.fire({
        title: "Confirm Duplicate?",
        text: `Duplicate ${recordVerbiage}?`,
        showCancelButton: true,
        confirmButtonText: "Duplicate",
        confirmButtonColor: "#dc3545"
    }).then((result) => {
        if (result.isConfirmed) {
            $.post('/Vehicle/DuplicateRecords', { recordIds: ids, importMode: source }, function (data) {
                if (data) {
                    successToast(`${ids.length} Record(s) Duplicated`);
                    var vehicleId = GetVehicleId().vehicleId;
                    refreshDataCallBack(vehicleId);
                } else {
                    errorToast(genericErrorMessage());
                }
            });
        } else {
            $("#workAroundInput").hide();
        }
    });
}
var selectedRow = [];
var isDragging = false;
$(window).on('mouseup', function (e) {
    rangeMouseUp(e);
});
$(window).on('mousedown', function (e) {
    rangeMouseDown(e);
});
$(window).on('keydown', function (e) {
    var userOnInput = $(e.target).is("input") || $(e.target).is("textarea");
    if (!userOnInput) {
        if (e.ctrlKey && e.which == 65) {
            e.preventDefault();
            e.stopPropagation();
            selectAllRows();
        }
    }
})
function selectAllRows() {
    clearSelectedRows();
    $('.vehicleDetailTabContainer .table tbody tr:visible').addClass('table-active');
    $('.vehicleDetailTabContainer .table tbody tr:visible').map((index, elem) => {
        addToSelectedRows($(elem).attr('data-rowId'));
    });
}
function rangeMouseDown(e) {
    if (isRightClick(e)) {
        return;
    }
    var contextMenuAction = $(e.target).is(".table-context-menu > li > .dropdown-item")
    if (!e.ctrlKey && !contextMenuAction) {
        clearSelectedRows();
    }
    isDragging = true;

    document.documentElement.onselectstart = function () { return false; };
}
function isRightClick(e) {
    if (e.which) {
        return (e.which == 3);
    } else if (e.button) {
        return (e.button == 2);
    }
    return false;
}
function stopEvent() {
    event.stopPropagation();
}
function rangeMouseUp(e) {
    if ($(".table-context-menu").length > 0) {
        $(".table-context-menu").hide();
    }
    if (isRightClick(e)) {
        return;
    }
    isDragging = false;
    document.documentElement.onselectstart = function () { return true; };
}
function rangeMouseMove(e) {
    if (isDragging) {
        if (!$(e).hasClass('table-active')) {
            addToSelectedRows($(e).attr('data-rowId'));
            $(e).addClass('table-active');
        }
    }
}
function addToSelectedRows(id) {
    if (selectedRow.findIndex(x => x == id) == -1) {
        selectedRow.push(id);
    }
}
function removeFromSelectedRows(id) {
    var rowIndex = selectedRow.findIndex(x => x == id)
    if (rowIndex != -1) {
        selectedRow.splice(rowIndex, 1);
    }
}
function clearSelectedRows() {
    selectedRow = [];
    $('.table tr').removeClass('table-active');
}
function getDeviceIsTouchOnly() {
    if (navigator.maxTouchPoints > 0 && matchMedia('(pointer: coarse)').matches && !matchMedia('(any-pointer: fine)').matches) {
        return true;
    } else {
        return false;
    }
}
function showTableContextMenu(e) {
    if (event != undefined) {
        event.preventDefault();
    }
    if (getDeviceIsTouchOnly()) {
        return;
    }
    $(".table-context-menu").show();
    determineContextMenuItems();
    $(".table-context-menu").css({
        position: "absolute",
        left: getMenuPosition(event.clientX, 'width', 'scrollLeft'),
        top: getMenuPosition(event.clientY, 'height', 'scrollTop')
    });
    if (!$(e).hasClass('table-active')) {
        clearSelectedRows();
        addToSelectedRows($(e).attr('data-rowId'));
        $(e).addClass('table-active');
    }
}
function determineContextMenuItems() {
    var tableRows = $('.table tbody tr:visible');
    var tableRowsActive = $('.table tr.table-active');
    if (tableRowsActive.length == 1) {
        //only one row selected
        $(".context-menu-active-single").show();
        $(".context-menu-active-multiple").hide();
    } else if (tableRowsActive.length > 1) {
        //multiple rows selected
        $(".context-menu-active-single").hide();
        $(".context-menu-active-multiple").show();
    } else {
        //nothing was selected, bug case.
        $(".context-menu-active-single").hide();
        $(".context-menu-active-multiple").hide();
    }
    if (tableRows.length > 1) {
        $(".context-menu-multiple").show();
        if (tableRows.length == tableRowsActive.length) {
            //all rows are selected, show deselect all button.
            $(".context-menu-deselect-all").show();
            $(".context-menu-select-all").hide();
        } else if (tableRows.length != tableRowsActive.length) {
            //not all rows are selected, show select all button.
            $(".context-menu-select-all").show();
            $(".context-menu-deselect-all").hide();
        }
    } else {
        $(".context-menu-multiple").hide();
    }
}
function getMenuPosition(mouse, direction, scrollDir) {
    var win = $(window)[direction](),
        scroll = $(window)[scrollDir](),
        menu = $(".table-context-menu")[direction](),
        position = mouse + scroll;

    // opening menu would pass the side of the page
    if (mouse + menu > win && menu < mouse)
        position -= menu;
    return position;
}
function handleTableRowClick(e, callBack, rowId) {
    if (!event.ctrlKey) {
        callBack(rowId);
    } else if (!$(e).hasClass('table-active')) {
        addToSelectedRows($(e).attr('data-rowId'));
        $(e).addClass('table-active');
    } else if ($(e).hasClass('table-active')) {
        removeFromSelectedRows($(e).attr('data-rowId'));
        $(e).removeClass('table-active');
    }
}

function showTableContextMenuForMobile(e, xPosition, yPosition) {
    if (!$(e).hasClass('table-active')) {
        addToSelectedRows($(e).attr('data-rowId'));
        $(e).addClass('table-active');
        shakeTableRow(e);
    } else {
        $(".table-context-menu").show();
        determineContextMenuItems();
        $(".table-context-menu").css({
            position: "absolute",
            left: getMenuPosition(xPosition, 'width', 'scrollLeft'),
            top: getMenuPosition(yPosition, 'height', 'scrollTop')
        });
    }
}
function shakeTableRow(e) {
    $(e).addClass('tablerow-shake');
    setTimeout(function () { $(e).removeClass('tablerow-shake'); }, 1200)
}
var rowTouchTimer;
var rowTouchDuration = 800;
function detectRowLongTouch(sender) {
    var touchX = event.touches[0].clientX;
    var touchY = event.touches[0].clientY;
    if (!rowTouchTimer) {
        rowTouchTimer = setTimeout(function () { showTableContextMenuForMobile(sender, touchX, touchY); detectRowTouchEndPremature(sender); }, rowTouchDuration);
    }
}
function detectRowTouchEndPremature(sender) {
    if (rowTouchTimer) {
        clearTimeout(rowTouchTimer);
        rowTouchTimer = null;
    }
}
function replenishSupplies() {
    Swal.fire({
        title: 'Replenish Supplies',
        html: `
                            <input type="text" id="inputSupplyAddQuantity" class="swal2-input" placeholder="Quantity">
                            <br />
                            <input type="text" id="inputSupplyAddCost" class="swal2-input" placeholder="Cost">
                            <br />
                            <span class='small'>leave blank to use unit cost calculation</span>
              `,
        confirmButtonText: 'Replenish',
        focusConfirm: false,
        preConfirm: () => {
            const replquantity = globalParseFloat($("#inputSupplyAddQuantity").val());
            const replcost = $("#inputSupplyAddCost").val();
            const parsedReplCost = globalParseFloat(replcost);
            var quantitybeforeRepl = globalParseFloat($('#supplyRecordQuantity').val());
            if (isNaN(replquantity) || (replcost.trim() != '' && isNaN(parsedReplCost))) {
                Swal.showValidationMessage(`Please enter a valid quantity and cost`);
            } else if (replcost.trim() == '' && (isNaN(quantitybeforeRepl) || quantitybeforeRepl == 0)){
                Swal.showValidationMessage(`Unable to use unit cost calculation, please provide cost`);
            }
            return { replquantity, replcost, parsedReplCost }
        },
    }).then(function (result) {
        if (result.isConfirmed) {
            var replenishedCost = result.value.replcost;
            var parsedReplenishedCost = result.value.parsedReplCost;
            var replenishedQuantity = result.value.replquantity;
            var currentCost = globalParseFloat($('#supplyRecordCost').val())
            if (isNaN(currentCost)) {
                currentCost = 0;
            }
            var currentQuantity = globalParseFloat($('#supplyRecordQuantity').val());
            var newQuantity = currentQuantity + replenishedQuantity;
            if (replenishedCost.trim() == '') {

                var unitCost = currentCost / currentQuantity;
                var newCost = newQuantity * unitCost;
                //set text fields.
                $('#supplyRecordCost').val(globalFloatToString(newCost.toFixed(3).toString()));
                $('#supplyRecordQuantity').val(globalFloatToString(newQuantity.toFixed(3).toString()));
            } else {
                var newCost = currentCost + parsedReplenishedCost;
                //set text fields.
                $('#supplyRecordCost').val(globalFloatToString(newCost.toFixed(3).toString()));
                $('#supplyRecordQuantity').val(globalFloatToString(newQuantity.toFixed(3).toString()));
            }
        }
    });
}
function showTableColumns(e, isExtraField) {
    //logic for extra field since we dont hardcode the data-column type
    if (isExtraField) {
        var showColumn = $(e).is(':checked');
        var columnName = $(e).parent().find('.form-check-label').text();
        if (showColumn) {
            $(`[data-column='${columnName}']`).show();
        } else {
            $(`[data-column='${columnName}']`).hide();
        }
    } else {
        var showColumn = $(e).is(':checked');
        var columnName = $(e).attr('data-column-toggle');
        if (showColumn) {
            $(`[data-column='${columnName}']`).show();
        } else {
            $(`[data-column='${columnName}']`).hide();
        }
    }
}
function searchTableRows(tabName) {
    Swal.fire({
        title: 'Search Records',
        html: `
                            <input type="text" id="inputSearch" class="swal2-input" placeholder="Keyword(case sensitive)">
                            `,
        confirmButtonText: 'Search',
        focusConfirm: false,
        preConfirm: () => {
            const searchString = $("#inputSearch").val();
            return { searchString }
        },
    }).then(function (result) {
        if (result.isConfirmed) {
            var rowData = $(`#${tabName} table tbody tr`);
            var filteredRows = $(`#${tabName} table tbody tr td:contains('${result.value.searchString}')`).parent();
            if (result.value.searchString.trim() == '') {
                rowData.removeClass('override-hide');
            } else {
                rowData.addClass('override-hide');
                filteredRows.removeClass('override-hide');
            }
            $(".tagfilter.bg-primary").addClass('bg-secondary').removeClass('bg-primary');
            updateAggregateLabels();
        }
    });
}