﻿function showAddGasRecordModal() {
    $.get(`/Vehicle/GetAddGasRecordPartialView?vehicleId=${GetVehicleId().vehicleId}`, function (data) {
        if (data) {
            $("#gasRecordModalContent").html(data);
            //initiate datepicker
            initDatePicker($('#gasRecordDate'));
            initTagSelector($("#gasRecordTag"));
            $('#gasRecordModal').modal('show');
        }
    });
}
function showEditGasRecordModal(gasRecordId, nocache) {
    if (!nocache) {
        var existingContent = $("#gasRecordModalContent").html();
        if (existingContent.trim() != '') {
            //check if id is same.
            var existingId = getGasRecordModelData().id;
            if (existingId == gasRecordId && $('[data-changed=true]').length > 0) {
                $('#gasRecordModal').modal('show');
                $('.cached-banner').show();
                return;
            }
        }
    }
    $.get(`/Vehicle/GetGasRecordForEditById?gasRecordId=${gasRecordId}`, function (data) {
        if (data) {
            $("#gasRecordModalContent").html(data);
            //initiate datepicker
            initDatePicker($('#gasRecordDate'));
            initTagSelector($("#gasRecordTag"));
            $('#gasRecordModal').modal('show');
            bindModalInputChanges('gasRecordModal');
            $('#gasRecordModal').off('shown.bs.modal').on('shown.bs.modal', function () {
                if (getGlobalConfig().useMarkDown) {
                    toggleMarkDownOverlay("gasRecordNotes");
                }
            });
        }
    });
}
function hideAddGasRecordModal() {
    $('#gasRecordModal').modal('hide');
}
function deleteGasRecord(gasRecordId) {
    $("#workAroundInput").show();
    Swal.fire({
        title: "Confirm Deletion?",
        text: "Deleted Gas Records cannot be restored.",
        showCancelButton: true,
        confirmButtonText: "Delete",
        confirmButtonColor: "#dc3545"
    }).then((result) => {
        if (result.isConfirmed) {
            $.post(`/Vehicle/DeleteGasRecordById?gasRecordId=${gasRecordId}`, function (data) {
                if (data) {
                    hideAddGasRecordModal();
                    successToast("Gas Record deleted");
                    var vehicleId = GetVehicleId().vehicleId;
                    getVehicleGasRecords(vehicleId);
                } else {
                    errorToast(genericErrorMessage());
                }
            });
        } else {
            $("#workAroundInput").hide();
        }
    });
}
function saveGasRecordToVehicle(isEdit) {
    //get values
    var formValues = getAndValidateGasRecordValues();
    //validate
    if (formValues.hasError) {
        errorToast("Please check the form data");
        return;
    }
    //save to db.
    $.post('/Vehicle/SaveGasRecordToVehicleId', { gasRecord: formValues }, function (data) {
        if (data) {
            successToast(isEdit ? "Gas Record Updated" : "Gas Record Added.");
            hideAddGasRecordModal();
            saveScrollPosition();
            getVehicleGasRecords(formValues.vehicleId);
        } else {
            errorToast(genericErrorMessage());
        }
    })
}
function getAndValidateGasRecordValues() {
    var gasDate = $("#gasRecordDate").val();
    var gasMileage = parseInt(globalParseFloat($("#gasRecordMileage").val())).toString();
    var gasGallons = $("#gasRecordGallons").val();
    var gasCost = $("#gasRecordCost").val();
    var gasCostType = $("#gasCostType").val();
    var gasIsFillToFull = $("#gasIsFillToFull").is(":checked");
    var gasIsMissed = $("#gasIsMissed").is(":checked");
    var gasNotes = $("#gasRecordNotes").val();
    var gasTags = $("#gasRecordTag").val();
    var vehicleId = GetVehicleId().vehicleId;
    var gasRecordId = getGasRecordModelData().id;
    //Odometer Adjustments
    if (isNaN(gasMileage) && GetVehicleId().odometerOptional) {
        gasMileage = '0';
    }
    gasMileage = GetAdjustedOdometer(gasRecordId, gasMileage);
    //validation
    var hasError = false;
    var extraFields = getAndValidateExtraFields();
    if (extraFields.hasError) {
        hasError = true;
    }
    if (gasDate.trim() == '') { //eliminates whitespace.
        hasError = true;
        $("#gasRecordDate").addClass("is-invalid");
    } else {
        $("#gasRecordDate").removeClass("is-invalid");
    }
    if (gasMileage.trim() == '' || isNaN(gasMileage) || parseInt(gasMileage) < 0) {
        hasError = true;
        $("#gasRecordMileage").addClass("is-invalid");
    } else {
        $("#gasRecordMileage").removeClass("is-invalid");
    }
    if (gasGallons.trim() == '' || globalParseFloat(gasGallons) <= 0) {
        hasError = true;
        $("#gasRecordGallons").addClass("is-invalid");
    } else {
        $("#gasRecordGallons").removeClass("is-invalid");
    }
    if (gasCostType != undefined && gasCostType == 'unit') {
        var convertedGasCost = globalParseFloat(gasCost) * globalParseFloat(gasGallons);
        if (isNaN(convertedGasCost))
        {
            hasError = true;
            $("#gasRecordCost").addClass("is-invalid");
        } else {
            gasCost = globalFloatToString(convertedGasCost.toFixed(2).toString());
            $("#gasRecordCost").removeClass("is-invalid");
        }
    }
    if (gasCost.trim() == '' || !isValidMoney(gasCost)) {
        hasError = true;
        $("#gasRecordCost").addClass("is-invalid");
    } else {
        $("#gasRecordCost").removeClass("is-invalid");
    }
    return {
        id: gasRecordId,
        hasError: hasError,
        vehicleId: vehicleId,
        date: gasDate,
        mileage: gasMileage,
        gallons: gasGallons,
        cost: gasCost,
        files: uploadedFiles,
        tags: gasTags,
        isFillToFull: gasIsFillToFull,
        missedFuelUp: gasIsMissed,
        notes: gasNotes,
        extraFields: extraFields.extraFields
    }
}

function saveUserGasTabPreferences() {
    var gasUnit = $("[data-gas='consumption']").attr("data-unit");
    var fuelMileageUnit = $("[data-gas='fueleconomy']").attr("data-unit");
    $.post('/Vehicle/SaveUserGasTabPreferences', { gasUnit: gasUnit, fuelMileageUnit: fuelMileageUnit }, function (data) {
        if (!data) {
            errorToast("Error Saving User Preferences");
        }
    });
}

function convertGasConsumptionUnits(currentUnit, destinationUnit, save) {
    var sender = $("[data-gas='consumption']");
    if (currentUnit == "US gal") {
        switch (destinationUnit) {
            case "l":
                $("[data-gas-type='consumption']").map((index, elem) => {
                    var convertedAmount = globalParseFloat(elem.innerText) * 3.785;
                    elem.innerText = globalFloatToString(convertedAmount.toFixed(2));
                    sender.text(sender.text().replace(sender.attr("data-unit"), "l"));
                    sender.attr("data-unit", "l");
                });
                $("[data-gas-type='unitcost']").map((index, elem) => {
                    var convertedAmount = globalParseFloat(elem.innerText) / 3.785;
                    var decimalPoints = getGlobalConfig().useThreeDecimals ? 3 : 2;
                    elem.innerText = `${globalAppendCurrency(globalFloatToString(convertedAmount.toFixed(decimalPoints)))}`;
                });
                if (save) { setDebounce(saveUserGasTabPreferences); }
                break;
            case "imp gal":
                $("[data-gas-type='consumption']").map((index, elem) => {
                    var convertedAmount = globalParseFloat(elem.innerText) / 1.201;
                    elem.innerText = globalFloatToString(convertedAmount.toFixed(2));
                    sender.text(sender.text().replace(sender.attr("data-unit"), "imp gal"));
                    sender.attr("data-unit", "imp gal");
                });
                $("[data-gas-type='unitcost']").map((index, elem) => {
                    var convertedAmount = globalParseFloat(elem.innerText) * 1.201;
                    var decimalPoints = getGlobalConfig().useThreeDecimals ? 3 : 2;
                    elem.innerText = `${globalAppendCurrency(globalFloatToString(convertedAmount.toFixed(decimalPoints)))}`;
                });
                if (save) { setDebounce(saveUserGasTabPreferences); }
                break;
        }
    } else if (currentUnit == "l") {
        switch (destinationUnit) {
            case "US gal":
                $("[data-gas-type='consumption']").map((index, elem) => {
                    var convertedAmount = globalParseFloat(elem.innerText) / 3.785;
                    elem.innerText = globalFloatToString(convertedAmount.toFixed(2));
                    sender.text(sender.text().replace(sender.attr("data-unit"), "US gal"));
                    sender.attr("data-unit", "US gal");
                });
                $("[data-gas-type='unitcost']").map((index, elem) => {
                    var convertedAmount = globalParseFloat(elem.innerText) * 3.785;
                    var decimalPoints = getGlobalConfig().useThreeDecimals ? 3 : 2;
                    elem.innerText = `${globalAppendCurrency(globalFloatToString(convertedAmount.toFixed(decimalPoints)))}`;
                });
                if (save) { setDebounce(saveUserGasTabPreferences); }
                break;
            case "imp gal":
                $("[data-gas-type='consumption']").map((index, elem) => {
                    var convertedAmount = globalParseFloat(elem.innerText) / 4.546;
                    elem.innerText = globalFloatToString(convertedAmount.toFixed(2));
                    sender.text(sender.text().replace(sender.attr("data-unit"), "imp gal"));
                    sender.attr("data-unit", "imp gal");
                });
                $("[data-gas-type='unitcost']").map((index, elem) => {
                    var convertedAmount = globalParseFloat(elem.innerText) * 4.546;
                    var decimalPoints = getGlobalConfig().useThreeDecimals ? 3 : 2;
                    elem.innerText = `${globalAppendCurrency(globalFloatToString(convertedAmount.toFixed(decimalPoints)))}`;
                });
                if (save) { setDebounce(saveUserGasTabPreferences); }
                break;
        }
    } else if (currentUnit == "imp gal") {
        switch (destinationUnit) {
            case "US gal":
                $("[data-gas-type='consumption']").map((index, elem) => {
                    var convertedAmount = globalParseFloat(elem.innerText) * 1.201;
                    elem.innerText = globalFloatToString(convertedAmount.toFixed(2));
                    sender.text(sender.text().replace(sender.attr("data-unit"), "US gal"));
                    sender.attr("data-unit", "US gal");
                });
                $("[data-gas-type='unitcost']").map((index, elem) => {
                    var convertedAmount = globalParseFloat(elem.innerText) / 1.201;
                    var decimalPoints = getGlobalConfig().useThreeDecimals ? 3 : 2;
                    elem.innerText = `${globalAppendCurrency(globalFloatToString(convertedAmount.toFixed(decimalPoints)))}`;
                });
                if (save) { setDebounce(saveUserGasTabPreferences); }
                break;
            case "l":
                $("[data-gas-type='consumption']").map((index, elem) => {
                    var convertedAmount = globalParseFloat(elem.innerText) * 4.546;
                    elem.innerText = globalFloatToString(convertedAmount.toFixed(2));
                    sender.text(sender.text().replace(sender.attr("data-unit"), "l"));
                    sender.attr("data-unit", "l");
                });
                $("[data-gas-type='unitcost']").map((index, elem) => {
                    var convertedAmount = globalParseFloat(elem.innerText) / 4.546;
                    var decimalPoints = getGlobalConfig().useThreeDecimals ? 3 : 2;
                    elem.innerText = `${globalAppendCurrency(globalFloatToString(convertedAmount.toFixed(decimalPoints)))}`;
                });
                if (save) { setDebounce(saveUserGasTabPreferences); }
                break;
        }
    }
}

function convertFuelMileageUnits(currentUnit, destinationUnit, save) {
    var sender = $("[data-gas='fueleconomy']");
    if (currentUnit == "l/100km") {
        switch (destinationUnit) {
            case "km/l":
                $("[data-gas-type='fueleconomy']").map((index, elem) => {
                    var convertedAmount = globalParseFloat(elem.innerText);
                    if (convertedAmount > 0) {
                        convertedAmount = 100 / convertedAmount;
                        elem.innerText = globalFloatToString(convertedAmount.toFixed(2));
                    }
                });
                //update labels up top.
                if ($("#averageFuelMileageLabel").length > 0) {
                    var newAverage = globalParseFloat($("#averageFuelMileageLabel").text().split(":")[1].trim());
                    if (newAverage > 0) {
                        newAverage = 100 / newAverage;
                        var averageLabel = $("#averageFuelMileageLabel");
                        averageLabel.text(`${averageLabel.text().split(':')[0]}: ${globalFloatToString(newAverage.toFixed(2))}`);
                    }
                }
                if ($("#minFuelMileageLabel").length > 0) {
                    var newMin = globalParseFloat($("#minFuelMileageLabel").text().split(":")[1].trim());
                    if (newMin > 0) {
                        newMin = 100 / newMin;
                        var minLabel = $("#minFuelMileageLabel");
                        minLabel.text(`${minLabel.text().split(':')[0]}: ${globalFloatToString(newMin.toFixed(2))}`);
                    }
                }
                if ($("#maxFuelMileageLabel").length > 0) {
                    var newMax = globalParseFloat($("#maxFuelMileageLabel").text().split(":")[1].trim());
                    if (newMax > 0) {
                        newMax = 100 / newMax;
                        var maxLabel = $("#maxFuelMileageLabel");
                        maxLabel.text(`${maxLabel.text().split(':')[0]}: ${globalFloatToString(newMax.toFixed(2))}`);
                    }
                }
                sender.text(sender.text().replace(sender.attr("data-unit"), "km/l"));
                sender.attr("data-unit", "km/l");
                if (save) { setDebounce(saveUserGasTabPreferences); }
                break;
        }
    } else if (currentUnit == "km/l") {
        switch (destinationUnit) {
            case "l/100km":
                $("[data-gas-type='fueleconomy']").map((index, elem) => {
                    var convertedAmount = globalParseFloat(elem.innerText);
                    if (convertedAmount > 0) {
                        convertedAmount = 100 / convertedAmount;
                        elem.innerText = globalFloatToString(convertedAmount.toFixed(2));
                    }
                });
                if ($("#averageFuelMileageLabel").length > 0) {
                    var newAverage = globalParseFloat($("#averageFuelMileageLabel").text().split(":")[1].trim());
                    if (newAverage > 0) {
                        newAverage = 100 / newAverage;
                        var averageLabel = $("#averageFuelMileageLabel");
                        averageLabel.text(`${averageLabel.text().split(':')[0]}: ${globalFloatToString(newAverage.toFixed(2))}`);
                    }
                }
                if ($("#minFuelMileageLabel").length > 0) {
                    var newMin = globalParseFloat($("#minFuelMileageLabel").text().split(":")[1].trim());
                    if (newMin > 0) {
                        newMin = 100 / newMin;
                        var minLabel = $("#minFuelMileageLabel");
                        minLabel.text(`${minLabel.text().split(':')[0]}: ${globalFloatToString(newMin.toFixed(2))}`);
                    }
                }
                if ($("#maxFuelMileageLabel").length > 0) {
                    var newMax = globalParseFloat($("#maxFuelMileageLabel").text().split(":")[1].trim());
                    if (newMax > 0) {
                        newMax = 100 / newMax;
                        var maxLabel = $("#maxFuelMileageLabel");
                        maxLabel.text(`${maxLabel.text().split(':')[0]}: ${globalFloatToString(newMax.toFixed(2))}`);
                    }
                }
                sender.text(sender.text().replace(sender.attr("data-unit"), "l/100km"));
                sender.attr("data-unit", "l/100km");
                if (save) { setDebounce(saveUserGasTabPreferences); }
                break;
        }
    }
}
function toggleGasFilter(sender) {
    filterTable('gas-tab-pane', sender);
    updateMPGLabels();
}
function updateMPGLabels() {
    var averageLabel = $("#averageFuelMileageLabel");
    var minLabel = $("#minFuelMileageLabel");
    var maxLabel = $("#maxFuelMileageLabel");
    if (averageLabel.length > 0 && minLabel.length > 0 && maxLabel.length > 0) {
        var rowsToAggregate = $("[data-aggregated='true']").parent(":not('.override-hide')");
        var rowMPG = rowsToAggregate.children('[data-gas-type="fueleconomy"]').toArray().map(x => globalParseFloat(x.textContent));
        var maxMPG = rowMPG.length > 0 ? rowMPG.reduce((a, b) => a > b ? a : b) : 0;
        var minMPG = rowMPG.length > 0 ? rowMPG.filter(x=>x>0).reduce((a, b) => a < b ? a : b) : 0;
        var totalMilesTraveled = rowMPG.length > 0 ? rowsToAggregate.children('[data-gas-type="mileage"]').toArray().map(x => globalParseFloat($(x).attr("data-gas-aggregate"))).reduce((a, b) => a + b) : 0;
        var totalGasConsumed = rowMPG.length > 0 ? rowsToAggregate.children('[data-gas-type="consumption"]').toArray().map(x => globalParseFloat($(x).attr("data-gas-aggregate"))).reduce((a, b) => a + b) : 0;
        if (totalGasConsumed > 0) {
            var averageMPG = totalMilesTraveled / totalGasConsumed;
            if (!getGlobalConfig().useMPG && $("[data-gas='fueleconomy']").attr("data-unit") != 'km/l' && averageMPG > 0) {
                averageMPG = 100 / averageMPG;
            }
            averageLabel.text(`${averageLabel.text().split(':')[0]}: ${globalFloatToString(averageMPG.toFixed(2))}`);
        } else {
            averageLabel.text(`${averageLabel.text().split(':')[0]}: 0.00`);
        }
        if (!getGlobalConfig().useMPG && $("[data-gas='fueleconomy']").attr("data-unit") != 'km/l') {
            maxLabel.text(`${maxLabel.text().split(':')[0]}: ${globalFloatToString(minMPG.toFixed(2))}`);
            minLabel.text(`${minLabel.text().split(':')[0]}: ${globalFloatToString(maxMPG.toFixed(2))}`);
        }
        else {
            minLabel.text(`${minLabel.text().split(':')[0]}: ${globalFloatToString(minMPG.toFixed(2))}`);
            maxLabel.text(`${maxLabel.text().split(':')[0]}: ${globalFloatToString(maxMPG.toFixed(2))}`);
        }
    }
}

function toggleUnits(sender) {
    event.preventDefault();
    //check which column to convert.
    sender = $(sender); 
    if (sender.attr("data-gas") == "consumption") {
        switch (sender.attr("data-unit")) {
            case "US gal":
                convertGasConsumptionUnits("US gal", "l", true);
                break;
            case "l":
                convertGasConsumptionUnits("l", "imp gal", true);
                break;
            case "imp gal":
                convertGasConsumptionUnits("imp gal", "US gal", true);
                break;
        }
    } else if (sender.attr("data-gas") == "fueleconomy") {
        switch (sender.attr("data-unit")) {
            case "l/100km":
                convertFuelMileageUnits("l/100km", "km/l", true);
                break;
            case "km/l":
                convertFuelMileageUnits("km/l", "l/100km", true);
                break;
        }
    }
}

function searchGasTableRows() {
    var tabName = 'gas-tab-pane';
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
            updateMPGLabels();
        }
    });
}
function editMultipleGasRecords(ids) {
    $.post('/Vehicle/GetGasRecordsEditModal', { recordIds: ids }, function (data) {
        if (data) {
            $("#gasRecordModalContent").html(data);
            //initiate datepicker
            initDatePicker($('#gasRecordDate'));
            initTagSelector($("#gasRecordTag"));
            $('#gasRecordModal').modal('show');
        }
    });
}
function saveMultipleGasRecordsToVehicle() {
    var gasDate = $("#gasRecordDate").val();
    var gasMileage = $("#gasRecordMileage").val();
    var gasMileageToParse = parseInt(globalParseFloat($("#gasRecordMileage").val())).toString();
    var gasConsumption = $("#gasRecordConsumption").val();
    var gasCost = $("#gasRecordCost").val();
    var gasNotes = $("#gasRecordNotes").val();
    var gasTags = $("#gasRecordTag").val();
    var gasExtraFields = getAndValidateExtraFields();
    //validation
    var hasError = false;
    if (gasMileage.trim() != '' && (isNaN(gasMileageToParse) || parseInt(gasMileageToParse) < 0)) {
        hasError = true;
        $("#gasRecordMileage").addClass("is-invalid");
    } else {
        $("#gasRecordMileage").removeClass("is-invalid");
    }
    if (gasConsumption.trim() != '' && !isValidMoney(gasConsumption)) {
        hasError = true;
        $("#gasRecordConsumption").addClass("is-invalid");
    } else {
        $("#gasRecordConsumption").removeClass("is-invalid");
    }
    if (gasCost.trim() != '' && !isValidMoney(gasCost)) {
        hasError = true;
        $("#gasRecordCost").addClass("is-invalid");
    } else {
        $("#gasRecordCost").removeClass("is-invalid");
    }
    if (hasError) {
        errorToast("Please check the form data");
        return;
    }
    var formValues = {
        recordIds: recordsToEdit,
        editRecord: {
            date: gasDate,
            mileage: gasMileageToParse,
            gallons: gasConsumption,
            cost: gasCost,
            notes: gasNotes,
            tags: gasTags,
            extraFields: gasExtraFields.extraFields
        }
    }
    $.post('/Vehicle/SaveMultipleGasRecords', { editModel: formValues }, function (data) {
        if (data) {
            successToast("Gas Records Updated");
            hideAddGasRecordModal();
            saveScrollPosition();
            getVehicleGasRecords(GetVehicleId().vehicleId);
        } else {
            errorToast(genericErrorMessage());
        }
    })
}