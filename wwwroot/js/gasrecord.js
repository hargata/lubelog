function showAddGasRecordModal() {
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
function showEditGasRecordModal(gasRecordId) {
    $.get(`/Vehicle/GetGasRecordForEditById?gasRecordId=${gasRecordId}`, function (data) {
        if (data) {
            $("#gasRecordModalContent").html(data);
            //initiate datepicker
            initDatePicker($('#gasRecordDate'));
            initTagSelector($("#gasRecordTag"));
            $('#gasRecordModal').modal('show');
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
                    errorToast("An error has occurred, please try again later.");
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
            errorToast("An error has occurred, please try again later.");
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
    //validation
    var hasError = false;
    if (gasDate.trim() == '') { //eliminates whitespace.
        hasError = true;
        $("#gasRecordDate").addClass("is-invalid");
    } else {
        $("#gasRecordDate").removeClass("is-invalid");
    }
    if (gasMileage.trim() == '' || parseInt(gasMileage) < 0) {
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
        notes: gasNotes
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
                    elem.innerText = convertedAmount.toFixed(2);
                    sender.text(sender.text().replace(sender.attr("data-unit"), "l"));
                    sender.attr("data-unit", "l");
                });
                $("[data-gas-type='unitcost']").map((index, elem) => {
                    var convertedAmount = globalParseFloat(elem.innerText) / 3.785;
                    var decimalPoints = getGlobalConfig().useThreeDecimals ? 3 : 2;
                    elem.innerText = `${getGlobalConfig().currencySymbol}${convertedAmount.toFixed(decimalPoints)}`;
                });
                if (save) { setDebounce(saveUserGasTabPreferences); }
                break;
            case "imp gal":
                $("[data-gas-type='consumption']").map((index, elem) => {
                    var convertedAmount = globalParseFloat(elem.innerText) / 1.201;
                    elem.innerText = convertedAmount.toFixed(2);
                    sender.text(sender.text().replace(sender.attr("data-unit"), "imp gal"));
                    sender.attr("data-unit", "imp gal");
                });
                $("[data-gas-type='unitcost']").map((index, elem) => {
                    var convertedAmount = globalParseFloat(elem.innerText) * 1.201;
                    var decimalPoints = getGlobalConfig().useThreeDecimals ? 3 : 2;
                    elem.innerText = `${getGlobalConfig().currencySymbol}${convertedAmount.toFixed(decimalPoints)}`;
                });
                if (save) { setDebounce(saveUserGasTabPreferences); }
                break;
        }
    } else if (currentUnit == "l") {
        switch (destinationUnit) {
            case "US gal":
                $("[data-gas-type='consumption']").map((index, elem) => {
                    var convertedAmount = globalParseFloat(elem.innerText) / 3.785;
                    elem.innerText = convertedAmount.toFixed(2);
                    sender.text(sender.text().replace(sender.attr("data-unit"), "US gal"));
                    sender.attr("data-unit", "US gal");
                });
                $("[data-gas-type='unitcost']").map((index, elem) => {
                    var convertedAmount = globalParseFloat(elem.innerText) * 3.785;
                    var decimalPoints = getGlobalConfig().useThreeDecimals ? 3 : 2;
                    elem.innerText = `${getGlobalConfig().currencySymbol}${convertedAmount.toFixed(decimalPoints)}`;
                });
                if (save) { setDebounce(saveUserGasTabPreferences); }
                break;
            case "imp gal":
                $("[data-gas-type='consumption']").map((index, elem) => {
                    var convertedAmount = globalParseFloat(elem.innerText) / 4.546;
                    elem.innerText = convertedAmount.toFixed(2);
                    sender.text(sender.text().replace(sender.attr("data-unit"), "imp gal"));
                    sender.attr("data-unit", "imp gal");
                });
                $("[data-gas-type='unitcost']").map((index, elem) => {
                    var convertedAmount = globalParseFloat(elem.innerText) * 4.546;
                    var decimalPoints = getGlobalConfig().useThreeDecimals ? 3 : 2;
                    elem.innerText = `${getGlobalConfig().currencySymbol}${convertedAmount.toFixed(decimalPoints)}`;
                });
                if (save) { setDebounce(saveUserGasTabPreferences); }
                break;
        }
    } else if (currentUnit == "imp gal") {
        switch (destinationUnit) {
            case "US gal":
                $("[data-gas-type='consumption']").map((index, elem) => {
                    var convertedAmount = globalParseFloat(elem.innerText) * 1.201;
                    elem.innerText = convertedAmount.toFixed(2);
                    sender.text(sender.text().replace(sender.attr("data-unit"), "US gal"));
                    sender.attr("data-unit", "US gal");
                });
                $("[data-gas-type='unitcost']").map((index, elem) => {
                    var convertedAmount = globalParseFloat(elem.innerText) / 1.201;
                    var decimalPoints = getGlobalConfig().useThreeDecimals ? 3 : 2;
                    elem.innerText = `${getGlobalConfig().currencySymbol}${convertedAmount.toFixed(decimalPoints)}`;
                });
                if (save) { setDebounce(saveUserGasTabPreferences); }
                break;
            case "l":
                $("[data-gas-type='consumption']").map((index, elem) => {
                    var convertedAmount = globalParseFloat(elem.innerText) * 4.546;
                    elem.innerText = convertedAmount.toFixed(2);
                    sender.text(sender.text().replace(sender.attr("data-unit"), "l"));
                    sender.attr("data-unit", "l");
                });
                $("[data-gas-type='unitcost']").map((index, elem) => {
                    var convertedAmount = globalParseFloat(elem.innerText) / 4.546;
                    var decimalPoints = getGlobalConfig().useThreeDecimals ? 3 : 2;
                    elem.innerText = `${getGlobalConfig().currencySymbol}${convertedAmount.toFixed(decimalPoints)}`;
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
                        elem.innerText = convertedAmount.toFixed(2);
                    }
                    //update labels up top.
                    var newAverage = globalParseFloat($("#averageFuelMileageLabel").text().replace("Average Fuel Economy: ", ""));
                    if (newAverage > 0) {
                        newAverage = 100 / newAverage;
                        $("#averageFuelMileageLabel").text(`Average Fuel Economy: ${newAverage.toFixed(2)}`)
                    }
                    var newMin = globalParseFloat($("#minFuelMileageLabel").text().replace("Min Fuel Economy: ", ""));
                    if (newMin > 0) {
                        newMin = 100 / newMin;
                        $("#minFuelMileageLabel").text(`Min Fuel Economy: ${newMin.toFixed(2)}`)
                    }
                    var newMax = globalParseFloat($("#maxFuelMileageLabel").text().replace("Max Fuel Economy: ", ""));
                    if (newMax > 0) {
                        newMax = 100 / newMax;
                        $("#maxFuelMileageLabel").text(`Max Fuel Economy: ${newMax.toFixed(2)}`)
                    }
                    sender.text(sender.text().replace(sender.attr("data-unit"), "km/l"));
                    sender.attr("data-unit", "km/l");
                });
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
                        elem.innerText = convertedAmount.toFixed(2);
                    }
                    var newAverage = globalParseFloat($("#averageFuelMileageLabel").text().replace("Average Fuel Economy: ", ""));
                    if (newAverage > 0) {
                        newAverage = 100 / newAverage;
                        $("#averageFuelMileageLabel").text(`Average Fuel Economy: ${newAverage.toFixed(2)}`)
                    }
                    var newMin = globalParseFloat($("#minFuelMileageLabel").text().replace("Min Fuel Economy: ", ""));
                    if (newMin > 0) {
                        newMin = 100 / newMin;
                        $("#minFuelMileageLabel").text(`Min Fuel Economy: ${newMin.toFixed(2)}`)
                    }
                    var newMax = globalParseFloat($("#maxFuelMileageLabel").text().replace("Max Fuel Economy: ", ""));
                    if (newMax > 0) {
                        newMax = 100 / newMax;
                        $("#maxFuelMileageLabel").text(`Max Fuel Economy: ${newMax.toFixed(2)}`)
                    }
                    sender.text(sender.text().replace(sender.attr("data-unit"), "l/100km"));
                    sender.attr("data-unit", "l/100km");
                });
                if (save) { setDebounce(saveUserGasTabPreferences); }
                break;
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