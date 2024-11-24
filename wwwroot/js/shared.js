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
    var vehiclePurchaseDate = $("#inputPurchaseDate").val();
    var vehicleSoldDate = $("#inputSoldDate").val();
    var vehicleLicensePlate = $("#inputLicensePlate").val();
    var vehicleIsElectric = $("#inputFuelType").val() == 'Electric';
    var vehicleIsDiesel = $("#inputFuelType").val() == 'Diesel';
    var vehicleUseHours = $("#inputUseHours").is(":checked");
    var vehicleOdometerOptional = $("#inputOdometerOptional").is(":checked");
    var vehicleHasOdometerAdjustment = $("#inputHasOdometerAdjustment").is(':checked');
    var vehicleOdometerMultiplier = $("#inputOdometerMultiplier").val();
    var vehicleOdometerDifference = parseInt(globalParseFloat($("#inputOdometerDifference").val())).toString();
    var vehiclePurchasePrice = $("#inputPurchasePrice").val();
    var vehicleSoldPrice = $("#inputSoldPrice").val();
    var vehicleIdentifier = $("#inputIdentifier").val();
    var vehicleDashboardMetrics = $("#collapseMetricInfo :checked").map(function () {
        return this.value;
    }).toArray();
    var extraFields = getAndValidateExtraFields();
    //validate
    var hasError = false;
    if (extraFields.hasError) {
        hasError = true;
    }
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
    if (vehicleIdentifier == "LicensePlate") {
        if (vehicleLicensePlate.trim() == '') {
            hasError = true;
            $("#inputLicensePlate").addClass("is-invalid");
        } else {
            $("#inputLicensePlate").removeClass("is-invalid");
        }
    } else {
        $("#inputLicensePlate").removeClass("is-invalid");
        //check if extra fields have value.
        var vehicleIdentifierExtraField = extraFields.extraFields.filter(x => x.name == vehicleIdentifier);
        //check if extra field exists.
        if (vehicleIdentifierExtraField.length == 0) {
            $(".modal.fade.show").find(`.extra-field [placeholder='${vehicleIdentifier}']`).addClass("is-invalid");
            hasError = true;
        } else {
            $(".modal.fade.show").find(`.extra-field [placeholder='${vehicleIdentifier}']`).removeClass("is-invalid");
        }
    }
    
    if (vehicleHasOdometerAdjustment) {
        //validate odometer adjustments
        //validate multiplier
        if (vehicleOdometerMultiplier.trim() == '' || !isValidMoney(vehicleOdometerMultiplier)) {
            hasError = true;
            $("#inputOdometerMultiplier").addClass("is-invalid");
        } else {
            $("#inputOdometerMultiplier").removeClass("is-invalid");
        }
        //validate difference
        if (vehicleOdometerDifference.trim() == '' || isNaN(vehicleOdometerDifference)) {
            hasError = true;
            $("#inputOdometerDifference").addClass("is-invalid");
        } else {
            $("#inputOdometerDifference").removeClass("is-invalid");
        }
    }
    if (vehiclePurchasePrice.trim() != '' && !isValidMoney(vehiclePurchasePrice)) {
        hasError = true;
        $("#inputPurchasePrice").addClass("is-invalid");
        $("#collapsePurchaseInfo").collapse('show');
    } else {
        $("#inputPurchasePrice").removeClass("is-invalid");
    }
    if (vehicleSoldPrice.trim() != '' && !isValidMoney(vehicleSoldPrice)) {
        hasError = true;
        $("#inputSoldPrice").addClass("is-invalid");
        $("#collapsePurchaseInfo").collapse('show');
    } else {
        $("#inputSoldPrice").removeClass("is-invalid");
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
        isDiesel: vehicleIsDiesel,
        tags: vehicleTags,
        useHours: vehicleUseHours,
        extraFields: extraFields.extraFields,
        purchaseDate: vehiclePurchaseDate,
        soldDate: vehicleSoldDate,
        odometerOptional: vehicleOdometerOptional,
        hasOdometerAdjustment: vehicleHasOdometerAdjustment,
        odometerMultiplier: vehicleOdometerMultiplier,
        odometerDifference: vehicleOdometerDifference,
        purchasePrice: vehiclePurchasePrice,
        soldPrice: vehicleSoldPrice,
        dashboardMetrics: vehicleDashboardMetrics,
        vehicleIdentifier: vehicleIdentifier
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
function toggleOdometerAdjustment() {
    var isChecked = $("#inputHasOdometerAdjustment").is(':checked');
    if (isChecked) {
        $("#odometerAdjustments").collapse('show');
    } else {
        $("#odometerAdjustments").collapse('hide');
    }
}
function uploadThumbnail(event) {
    var originalImage = event.files[0];
    var maxHeight = 290;
    try {
        //load image and perform Hermite resize
        var img = new Image();
        img.onload = function () {
            URL.revokeObjectURL(img.src);
            var imgWidth = img.width;
            var imgHeight = img.height;
            if (imgHeight > maxHeight) {
                //only scale if height is greater than threshold
                var imgScale = maxHeight / imgHeight;
                var newImgWidth = imgWidth * imgScale;
                var newImgHeight = imgHeight * imgScale;
                var resizedCanvas = hermiteResize(img, newImgWidth, newImgHeight);
                resizedCanvas.toBlob((blob) => {
                    let file = new File([blob], originalImage.name, { type: "image/jpeg" });
                    uploadFileAsync(file);
                }, 'image/jpeg');
            } else {
                uploadFileAsync(originalImage);
            }
        }
        img.src = URL.createObjectURL(originalImage);
    } catch (error) {
        console.log(`Error while attempting to upload and resize thumbnail - ${error}`);
        uploadFileAsync(originalImage);
    }
}
//Resize method using Hermite interpolation
//JS implementation by viliusle
function hermiteResize(origImg, width, height) {
    var canvas = document.createElement("canvas");
    var ctx = canvas.getContext("2d");
    canvas.width = origImg.width;
    canvas.height = origImg.height;
    ctx.drawImage(origImg, 0, 0);

    var width_source = canvas.width;
    var height_source = canvas.height;
    width = Math.round(width);
    height = Math.round(height);

    var ratio_w = width_source / width;
    var ratio_h = height_source / height;
    var ratio_w_half = Math.ceil(ratio_w / 2);
    var ratio_h_half = Math.ceil(ratio_h / 2);

   
    var img = ctx.getImageData(0, 0, width_source, height_source);
    var img2 = ctx.createImageData(width, height);
    var data = img.data;
    var data2 = img2.data;

    for (var j = 0; j < height; j++) {
        for (var i = 0; i < width; i++) {
            var x2 = (i + j * width) * 4;
            var weight = 0;
            var weights = 0;
            var weights_alpha = 0;
            var gx_r = 0;
            var gx_g = 0;
            var gx_b = 0;
            var gx_a = 0;
            var center_y = (j + 0.5) * ratio_h;
            var yy_start = Math.floor(j * ratio_h);
            var yy_stop = Math.ceil((j + 1) * ratio_h);
            for (var yy = yy_start; yy < yy_stop; yy++) {
                var dy = Math.abs(center_y - (yy + 0.5)) / ratio_h_half;
                var center_x = (i + 0.5) * ratio_w;
                var w0 = dy * dy; //pre-calc part of w
                var xx_start = Math.floor(i * ratio_w);
                var xx_stop = Math.ceil((i + 1) * ratio_w);
                for (var xx = xx_start; xx < xx_stop; xx++) {
                    var dx = Math.abs(center_x - (xx + 0.5)) / ratio_w_half;
                    var w = Math.sqrt(w0 + dx * dx);
                    if (w >= 1) {
                        //pixel too far
                        continue;
                    }
                    //hermite filter
                    weight = 2 * w * w * w - 3 * w * w + 1;
                    var pos_x = 4 * (xx + yy * width_source);
                    //alpha
                    gx_a += weight * data[pos_x + 3];
                    weights_alpha += weight;
                    //colors
                    if (data[pos_x + 3] < 255)
                        weight = weight * data[pos_x + 3] / 250;
                    gx_r += weight * data[pos_x];
                    gx_g += weight * data[pos_x + 1];
                    gx_b += weight * data[pos_x + 2];
                    weights += weight;
                }
            }
            data2[x2] = gx_r / weights;
            data2[x2 + 1] = gx_g / weights;
            data2[x2 + 2] = gx_b / weights;
            data2[x2 + 3] = gx_a / weights_alpha;
        }
    }
    //clear and resize canvas
    canvas.width = width;
    canvas.height = height;
    ctx.clearRect(0, 0, width, height);

    //draw
    ctx.putImageData(img2, 0, 0);
    return canvas;
}
function uploadFileAsync(event) {
    let formData = new FormData();
    if (event.files != undefined && event.files.length > 0) {
        formData.append("file", event.files[0]);
    } else {
        formData.append("file", event);
    }
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
    const euRegex = /^\$?(?=\(.*\)|[^()]*$)\(?\d{1,3}((\.\d{3}){0,8}|(\d{3}){0,8})(,\d{1,3}?)?\)?$/;
    const usRegex = /^\$?(?=\(.*\)|[^()]*$)\(?\d{1,3}((,\d{3}){0,8}|(\d{3}){0,8})(\.\d{1,3}?)?\)?$/;
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
var windowWidthForCompare = 0;
function bindWindowResize() {
    windowWidthForCompare = window.innerWidth;
    $(window).on('resize', function () {
        if (window.innerWidth != windowWidthForCompare) {
            hideMobileNav();
            windowWidthForCompare = window.innerWidth;
        }
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
        var newSum = "0.00";
        if (labelsToSum.length > 0) {
            newSum = labelsToSum.map(x => globalParseFloat(x.textContent)).reduce((a, b,) => a + b).toFixed(2);
        }
        sumLabel.text(`${sumLabel.text().split(':')[0]}: ${globalAppendCurrency(globalFloatToString(newSum))}`)
    }
    //Sum Distance
    var sumDistanceLabel = $("[data-aggregate-type='sum-distance']");
    if (sumDistanceLabel.length > 0) {
        var distanceLabelsToSum = $("[data-record-type='distance']").parent(":not('.override-hide')").children("[data-record-type='distance']").toArray();
        var newDistanceSum = 0;
        if (distanceLabelsToSum.length > 0) {
            newDistanceSum = distanceLabelsToSum.map(x => globalParseFloat(x.textContent)).reduce((a, b,) => a + b).toFixed(0);
        }
        sumDistanceLabel.text(`${sumDistanceLabel.text().split(':')[0]}: ${newDistanceSum}`)
    }
    //Count
    var newCount = $("[data-record-type='cost']").parent(":not('.override-hide')").length;
    var countLabel = $("[data-aggregate-type='count']");
    countLabel.text(`${countLabel.text().split(':')[0]}: ${newCount}`);
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
            $(event).val(""); //clear out the filename from the uploader
            if (response.length > 0) {
                uploadedFiles.push.apply(uploadedFiles, response);
                $.post('/Vehicle/GetFilesPendingUpload', { uploadedFiles: uploadedFiles }, function (viewData) {
                    $("#filesPendingUpload").html(viewData);
                });
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
    if (fileLocation.startsWith("/temp/")) {
        if ($("#documentsPendingUploadList > li").length == 0) {
            $("#documentsPendingUploadLabel").text("");
        }
    } else if (fileLocation.startsWith("/documents/")) {
        if ($("#uploadedDocumentsList > li").length == 0) {
            $("#uploadedDocumentsLabel").text("");
        }
    }
}
function editFileName(fileLocation, event) {
    Swal.fire({
        title: 'Rename File',
        html: `
                    <input type="text" id="newFileName" class="swal2-input" placeholder="New File Name" onkeydown="handleSwalEnter(event)">
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
    //get extra fields in modal that is currently open.
    var extraFieldsVisible = $(".modal.fade.show").find(".extra-field");
    extraFieldsVisible.map((index, elem) => {
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
function duplicateRecordsToOtherVehicles(ids, source) {
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

    $.get(`/Home/GetVehicleSelector?vehicleId=${GetVehicleId().vehicleId}`, function (data) {
        if (data) {
            //prompt user to select a vehicle
            Swal.fire({
                title: 'Duplicate to Vehicle(s)',
                html: data,
                confirmButtonText: 'Duplicate',
                focusConfirm: false,
                preConfirm: () => {
                    //validate
                    var selectedVehicleData = getAndValidateSelectedVehicle();
                    if (selectedVehicleData.hasError) {
                        Swal.showValidationMessage(`You must select a vehicle`);
                    }
                    return { selectedVehicleData }
                },
            }).then(function (result) {
                if (result.isConfirmed) {
                    $.post('/Vehicle/DuplicateRecordsToOtherVehicles', { recordIds: ids, vehicleIds: result.value.selectedVehicleData.ids, importMode: source}, function (data) {
                        if (data) {
                            successToast(`${ids.length} Record(s) Duplicated`);
                        } else {
                            errorToast(genericErrorMessage());
                        }
                    });
                }
            });
        } else {
            errorToast(genericErrorMessage());
        }
    })
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
        if ((e.ctrlKey || e.metaKey) && e.which == 65) {
            e.preventDefault();
            e.stopPropagation();
            selectAllRows();
        }
    }
});
function getCurrentTab() {
    return $(".tab-pane.active.show").attr('id');
}
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
    var contextMenuAction = $(e.target).parents(".table-context-menu > li > .dropdown-item").length > 0 || $(e.target).is(".table-context-menu > li > .dropdown-item");
    var selectMode = $("#chkSelectMode").length > 0 ? $("#chkSelectMode").is(":checked") : false;
    if (!(e.ctrlKey || e.metaKey || selectMode) && !contextMenuAction) {
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
    if (isDragging) {
        isDragging = false;
    }
    event.stopPropagation();
}
function rangeMouseUp(e) {
    if ($(".table-context-menu").length > 0) {
        $(".table-context-menu").fadeOut("fast");
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
    $(".table-context-menu").fadeIn("fast");
    determineContextMenuItems();
    $(".table-context-menu").css({
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
    if (GetVehicleId().hasOdometerAdjustment) {
        $(".context-menu-odometer-adjustment").show();
    } else {
        $(".context-menu-odometer-adjustment").hide();
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
    var selectMode = $("#chkSelectMode").length > 0 ? $("#chkSelectMode").is(":checked") : false;
    if (!(event.ctrlKey || event.metaKey || selectMode)) {
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
        $(".table-context-menu").fadeIn("fast");
        $(".table-context-menu").css({
            left: getMenuPosition(xPosition, 'width', 'scrollLeft'),
            top: getMenuPosition(yPosition, 'height', 'scrollTop')
        });
        determineContextMenuItems();
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
function handleSupplyAddCostKeyDown(event) {
    handleSwalEnter(event);
    interceptDecimalKeys(event);
}
function replenishSupplies() {
    Swal.fire({
        title: 'Replenish Supplies',
        html: `
                            <input type="text" id="inputSupplyAddQuantity" class="swal2-input" placeholder="Quantity" onkeydown="interceptDecimalKeys(event)" onkeyup="fixDecimalInput(this, 2)">
                            <br />
                            <input type="text" id="inputSupplyAddCost" class="swal2-input" placeholder="Cost" onkeydown="handleSupplyAddCostKeyDown(event)" onkeyup="fixDecimalInput(this, 2)">
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
            } else if (replcost.trim() == '' && (isNaN(quantitybeforeRepl) || quantitybeforeRepl == 0)) {
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
function showTableColumns(e, tabName) {
    //logic for extra field since we dont hardcode the data-column type
    var showColumn = $(e).is(':checked');
    var columnName = $(e).attr('data-column-toggle');
    if (showColumn) {
        $(`[data-column='${columnName}']`).show();
    } else {
        $(`[data-column='${columnName}']`).hide();
    }
    saveUserColumnPreferences(tabName);
}
function searchTableRows(tabName) {
    Swal.fire({
        title: 'Search Records',
        html: `
                            <input type="text" id="inputSearch" class="swal2-input" placeholder="Keyword(case sensitive)" onkeydown="handleSwalEnter(event)">
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
            var splitSearchString = result.value.searchString.split('=');
            if (result.value.searchString.includes('=') && splitSearchString.length == 2) {
                //column specific search.
                //get column index
                var columns = $(`#${tabName} table th`).toArray().map(x => x.innerText);
                var columnName = splitSearchString[0];
                var colSearchString = splitSearchString[1];
                var colIndex = columns.findIndex(x => x == columnName) + 1;
                filteredRows = $(`#${tabName} table tbody tr td:nth-child(${colIndex}):contains('${colSearchString}')`).parent();
            }
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
function loadUserColumnPreferences(columns) {
    if (columns.length == 0) {
        //user has no preference saved, reset to default
        return;
    }
    //uncheck all columns
    $(".col-visible-toggle").prop("checked", false);
    //hide all columns
    $('[data-column]').hide();
    //toggle visibility of each column
    columns.map(x => {
        var defaultColumn = $(`[data-column-toggle='${x}'].col-visible-toggle`);
        if (defaultColumn.length > 0) {
            defaultColumn.prop("checked", true);
            $(`[data-column='${x}']`).show();
        }
    });
}
function saveUserColumnPreferences(importMode) {
    var visibleColumns = $('.col-visible-toggle:checked').map((index, elem) => $(elem).attr('data-column-toggle')).toArray();
    var columnPreference = {
        tab: importMode,
        visibleColumns: visibleColumns
    };
    $.post('/Vehicle/SaveUserColumnPreferences', { columnPreference: columnPreference }, function (data) {
        if (!data) {
            errorToast(genericErrorMessage());
        }
    });
}
function copyToClipboard(e) {
    var textToCopy = e.textContent.trim();
    navigator.clipboard.writeText(textToCopy);
    successToast("Copied to Clipboard");
}
function noPropagation() {
    event.stopPropagation();
}
var checkExist;

function waitForElement(element, callBack, callBackParameter) {
    checkExist = setInterval(function () {
        if ($(`${element}`).length) {
            callBack(callBackParameter);
            clearInterval(checkExist);
        }
    }, 100); // check every 100ms
}
function bindModalInputChanges(modalName) {
    //bind text inputs
    $(`#${modalName} input[type='text'], #${modalName} input[type='number'], #${modalName} textarea`).off('input').on('input', function (e) {
        $(e.currentTarget).attr('data-changed', true);
    });
    $(`#${modalName} select, #${modalName} input[type='checkbox']`).off('input').on('input', function (e) {
        $(e.currentTarget).attr('data-changed', true);
    });
}
function handleModalPaste(e, recordType) {
    var clipboardFiles = e.clipboardData.files;
    var acceptableFileFormats = $(`#${recordType}`).attr("accept");
    var acceptableFileFormatsArray = acceptableFileFormats.split(',');
    var acceptableFiles = new DataTransfer();
    if (clipboardFiles.length > 0) {
        for (var x = 0; x < clipboardFiles.length; x++) {
            if (acceptableFileFormats != "*") {
                var fileExtension = `.${clipboardFiles[x].name.split('.').pop()}`;
                if (acceptableFileFormatsArray.includes(fileExtension)) {
                    acceptableFiles.items.add(clipboardFiles[x]);
                }
            } else {
                acceptableFiles.items.add(clipboardFiles[x]);
            }
        }
        $(`#${recordType}`)[0].files = acceptableFiles.files;
        $(`#${recordType}`).trigger('change');
    }
}
function handleEnter(e) {
    if ((event.ctrlKey || event.metaKey) && event.which == 13) {
        var saveButton = $(e).parent().find(".modal-footer .btn-primary");
        if (saveButton.length > 0) {
            saveButton.first().trigger('click');
        }
    }
}
function handleSwalEnter(e) {
    if (e.which == 13) {
        Swal.clickConfirm();
    }
}
function togglePasswordVisibility(elem) {
    var passwordField = $(elem).parent().siblings("input");
    var passwordButton = $(elem).find('.bi');
    if (passwordField.attr("type") == "password") {
        passwordField.attr("type", "text");
        passwordButton.removeClass('bi-eye');
        passwordButton.addClass('bi-eye-slash');
    } else {
        passwordField.attr("type", "password");
        passwordButton.removeClass('bi-eye-slash');
        passwordButton.addClass('bi-eye');
    }
}