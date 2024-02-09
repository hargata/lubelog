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
            autoclose: true
        });
    } else {
        input.datepicker({
            endDate: "+0d",
            format: getShortDatePattern().pattern,
            autoclose: true
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
    $(window).resize(function () {
        hideMobileNav();
    });
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
        var newSum = $("[data-record-type='cost']").parent(":not('.override-hide')").children("[data-record-type='cost']").toArray().map(x => globalParseFloat(x.textContent)).reduce((a, b,) => a + b).toFixed(2);
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