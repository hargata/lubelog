function showAddOdometerRecordModal() {
    $.get(`/Vehicle/GetAddOdometerRecordPartialView?vehicleId=${GetVehicleId().vehicleId}`, function (data) {
        if (data) {
            $("#odometerRecordModalContent").html(data);
            //initiate datepicker
            initDatePicker($('#odometerRecordDate'));
            initTagSelector($("#odometerRecordTag"));
            $('#odometerRecordModal').modal('show');
        }
    });
}
function showEditOdometerRecordModal(odometerRecordId, nocache) {
    if (!nocache) {
        var existingContent = $("#odometerRecordModalContent").html();
        if (existingContent.trim() != '') {
            //check if id is same.
            var existingId = getOdometerRecordModelData().id;
            if (existingId == odometerRecordId && $('[data-changed=true]').length > 0) {
                $('#odometerRecordModal').modal('show');
                $('.cached-banner').show();
                return;
            }
        }
    }
    $.get(`/Vehicle/GetOdometerRecordForEditById?odometerRecordId=${odometerRecordId}`, function (data) {
        if (data) {
            $("#odometerRecordModalContent").html(data);
            //initiate datepicker
            initDatePicker($('#odometerRecordDate'));
            initTagSelector($("#odometerRecordTag"));
            $('#odometerRecordModal').modal('show');
            bindModalInputChanges('odometerRecordModal');
            $('#odometerRecordModal').off('shown.bs.modal').on('shown.bs.modal', function () {
                if (getGlobalConfig().useMarkDown) {
                    toggleMarkDownOverlay("odometerRecordNotes");
                }
            });
        }
    });
}
function hideAddOdometerRecordModal() {
    $('#odometerRecordModal').modal('hide');
}
function deleteOdometerRecord(odometerRecordId) {
    $("#workAroundInput").show();
    Swal.fire({
        title: "Confirm Deletion?",
        text: "Deleted Odometer Records cannot be restored.",
        showCancelButton: true,
        confirmButtonText: "Delete",
        confirmButtonColor: "#dc3545"
    }).then((result) => {
        if (result.isConfirmed) {
            $.post(`/Vehicle/DeleteOdometerRecordById?odometerRecordId=${odometerRecordId}`, function (data) {
                if (data) {
                    hideAddOdometerRecordModal();
                    successToast("Odometer Record Deleted");
                    var vehicleId = GetVehicleId().vehicleId;
                    getPaginatedVehicleOdometerRecords(vehicleId);
                } else {
                    errorToast(genericErrorMessage());
                }
            });
        } else {
            $("#workAroundInput").hide();
        }
    });
}
function saveOdometerRecordToVehicle(isEdit) {
    //get values
    var formValues = getAndValidateOdometerRecordValues();
    //validate
    if (formValues.hasError) {
        errorToast("Please check the form data");
        return;
    }
    //save to db.
    $.post('/Vehicle/SaveOdometerRecordToVehicleId', { odometerRecord: formValues }, function (data) {
        if (data) {
            successToast(isEdit ? "Odometer Record Updated" : "Odometer Record Added.");
            hideAddOdometerRecordModal();
            saveScrollPosition();
            getPaginatedVehicleOdometerRecords(formValues.vehicleId);
            if (formValues.addReminderRecord) {
                setTimeout(function () { showAddReminderModal(formValues); }, 500);
            }
        } else {
            errorToast(genericErrorMessage());
        }
    })
}
function getAndValidateOdometerRecordValues() {
    var serviceDate = $("#odometerRecordDate").val();
    var initialOdometerMileage = parseInt(globalParseFloat($("#initialOdometerRecordMileage").val())).toString();
    var serviceMileage = parseInt(globalParseFloat($("#odometerRecordMileage").val())).toString();
    var serviceNotes = $("#odometerRecordNotes").val();
    var serviceTags = $("#odometerRecordTag").val();
    var vehicleId = GetVehicleId().vehicleId;
    var odometerValidation = GetVehicleId().odometerValidation;
    var maxOdometerDifference = parseInt(globalParseFloat(GetVehicleId().maxOdometerDifference));
    var odometerRecordId = getOdometerRecordModelData().id;
    //Odometer Adjustments
    serviceMileage = GetAdjustedOdometer(odometerRecordId, serviceMileage);
    //validation
    var hasError = false;
    var extraFields = getAndValidateExtraFields();
    if (extraFields.hasError) {
        hasError = true;
    }
    if (serviceDate.trim() == '') { //eliminates whitespace.
        hasError = true;
        $("#odometerRecordDate").addClass("is-invalid");
    } else {
        $("#odometerRecordDate").removeClass("is-invalid");
    }
    if (serviceMileage.trim() == '' || isNaN(serviceMileage) || parseInt(serviceMileage) < 0) {
        hasError = true;
        $("#odometerRecordMileage").addClass("is-invalid");
        $("#empty-feedback").removeClass("d-none");
        $("#empty-feedback").addClass("invalid-feedback");
    } else {
        $("#odometerRecordMileage").removeClass("is-invalid");
        $("#empty-feedback").removeClass("invalid-feedback");
        $("#empty-feedback").addClass("d-none");

        if (String(odometerValidation).toLowerCase() === 'true') {
            validateOdometerInput();
        }
    }
    if (isNaN(initialOdometerMileage) || parseInt(initialOdometerMileage) < 0) {
        hasError = true;
        $("#initialOdometerRecordMileage").addClass("is-invalid");
    } else {
        $("#initialOdometerRecordMileage").removeClass("is-invalid");
    }
    
    return {
        id: odometerRecordId,
        hasError: hasError,
        vehicleId: vehicleId,
        date: serviceDate,
        initialMileage: initialOdometerMileage,
        mileage: serviceMileage,
        notes: serviceNotes,
        tags: serviceTags,
        files: uploadedFiles,
        extraFields: extraFields.extraFields
    }

    function validateOdometerInput() {
        if (serviceMileage - initialOdometerMileage > maxOdometerDifference) {
            hasError = true;
            $("#odometerRecordMileage").addClass("is-invalid");
            $("#maxDifference-feedback").removeClass("d-none");
            $("#maxDifference-feedback").addClass("invalid-feedback");
        } else {
            $("#odometerRecordMileage").removeClass("is-invalid");
            $("#maxDifference-feedback").removeClass("invalid-feedback");
            $("#maxDifference-feedback").addClass("d-none");

            if (serviceMileage - initialOdometerMileage < 0) {
                hasError = true;
                $("#odometerRecordMileage").addClass("is-invalid");
                $("#negative-feedback").removeClass("d-none");
                $("#negative-feedback").addClass("invalid-feedback");
            } else {
                $("#odometerRecordMileage").removeClass("is-invalid");
                $("#negative-feedback").removeClass("invalid-feedback");
                $("#negative-feedback").addClass("d-none");
            }
        }
    }
}

function recalculateDistance() {
    //force distance recalculation
    //reserved for when data is incoherent with negative distances due to non-chronological order of odometer records.
    var vehicleId = GetVehicleId().vehicleId
    $.post(`/Vehicle/ForceRecalculateDistanceByVehicleId?vehicleId=${vehicleId}`, function (data) {
        if (data) {
            successToast("Odometer Records Updated")
            getVehicleOdometerRecords(vehicleId);
        } else {
            errorToast(genericErrorMessage());
        }
    });
}

function editMultipleOdometerRecords(ids) {
    if (ids.length < 2) {
        return;
    }
    $.post('/Vehicle/GetOdometerRecordsEditModal', { recordIds: ids }, function (data) {
        if (data) {
            $("#odometerRecordModalContent").html(data);
            //initiate datepicker
            initDatePicker($('#odometerRecordDate'));
            initTagSelector($("#odometerRecordTag"));
            $('#odometerRecordModal').modal('show');
        }
    });
}
function saveMultipleOdometerRecordsToVehicle() {
    var odometerDate = $("#odometerRecordDate").val();
    var initialOdometerMileage = $("#initialOdometerRecordMileage").val();
    var odometerMileage = $("#odometerRecordMileage").val();
    var initialOdometerMileageToParse = parseInt(globalParseFloat($("#initialOdometerRecordMileage").val())).toString();
    var odometerMileageToParse = parseInt(globalParseFloat($("#odometerRecordMileage").val())).toString();
    var odometerNotes = $("#odometerRecordNotes").val();
    var odometerTags = $("#odometerRecordTag").val();
    var odometerExtraFields = getAndValidateExtraFields();
    //validation
    var hasError = false;
    if (odometerMileage.trim() != '' && (isNaN(odometerMileageToParse) || parseInt(odometerMileageToParse) < 0)) {
        hasError = true;
        $("#odometerRecordMileage").addClass("is-invalid");
    } else {
        $("#odometerRecordMileage").removeClass("is-invalid");
    }
    if (initialOdometerMileage.trim() != '' && (isNaN(initialOdometerMileageToParse) || parseInt(initialOdometerMileageToParse) < 0)) {
        hasError = true;
        $("#odometerRecordMileage").addClass("is-invalid");
    } else {
        $("#odometerRecordMileage").removeClass("is-invalid");
    }
    if (hasError) {
        errorToast("Please check the form data");
        return;
    }
    var formValues = {
        recordIds: recordsToEdit,
        editRecord: {
            date: odometerDate,
            initialMileage: initialOdometerMileageToParse,
            mileage: odometerMileageToParse,
            notes: odometerNotes,
            tags: odometerTags,
            extraFields: odometerExtraFields.extraFields
        }
    }
    $.post('/Vehicle/SaveMultipleOdometerRecords', { editModel: formValues }, function (data) {
        if (data) {
            successToast("Odometer Records Updated");
            hideAddOdometerRecordModal();
            saveScrollPosition();
            getPaginatedVehicleOdometerRecords(GetVehicleId().vehicleId);
        } else {
            errorToast(genericErrorMessage());
        }
    })
}
function toggleInitialOdometerEnabled() {
    if ($("#initialOdometerRecordMileage").prop("disabled")) {
        $("#initialOdometerRecordMileage").prop("disabled", false);
    } else {
        $("#initialOdometerRecordMileage").prop("disabled", true);
    }
    
}
function showTripModal() {
    $(".odometer-modal").addClass('d-none');
    $(".trip-modal").removeClass('d-none');
    //set current odometer
    $(".trip-odometer").text($("#initialOdometerRecordMileage").val());
}
function hideTripModal() {
    //check if recording is in progress
    if (tripTimer != undefined || tripWakeLock != undefined) {
        Swal.fire({
            title: "Confirm Exit?",
            text: "Recording in Progress, Exit?",
            showCancelButton: true,
            confirmButtonText: "Exit",
            confirmButtonColor: "#dc3545"
        }).then((result) => {
            if (result.isConfirmed) {
                stopRecording();
                $(".odometer-modal").removeClass('d-none');
                $(".trip-modal").addClass('d-none');
            }
        });
    } else {
        $(".odometer-modal").removeClass('d-none');
        $(".trip-modal").addClass('d-none');
    }
}
function startRecording() {
    if (navigator.geolocation && navigator.wakeLock) {
        try {
            navigator.wakeLock.request('screen').then((wl) => {
                tripWakeLock = wl;
                tripTimer = setInterval(() => {
                    navigator.geolocation.getCurrentPosition(recordPosition, stopRecording, { maximumAge: 1000, timeout: 4000, enableHighAccuracy: true });
                }, 5000);
                $(".trip-start").addClass('d-none');
                $(".trip-stop").removeClass('d-none');
                //modify modal to prevent closing
                $("#odometerRecordModal").on("hide.bs.modal", function (event) {
                    event.preventDefault();
                    hideTripModal();
                });
            });
        } catch (err) {
            errorToast('Location Services not Enabled');
        }
    } else {
        errorToast('Browser does not support GeoLocation and/or WakeLock API');
    }
}
function recordPosition(position) {
    var currentLat = position.coords.latitude;
    var currentLong = position.coords.longitude;
    if (tripLastPosition == undefined) {
        tripLastPosition = {
            latitude: currentLat,
            longitude: currentLong
        }
        tripCoordinates.push(`${currentLat},${currentLong}`);
    } else {
        //calculate distance
        var distanceTraveled = calculateDistance(tripLastPosition.latitude, tripLastPosition.longitude, currentLat, currentLong);
        var recordedTotalOdometer = getRecordedOdometer();
        if (distanceTraveled >= 0.1) { //if greater than 0.1 mile or KM then it's significant
            recordedTotalOdometer += distanceTraveled;
            var recordedOdometerString = recordedTotalOdometer.toString().split('.');
            $(".trip-odometer").html(recordedOdometerString[0]);
            if (recordedOdometerString.length == 2) {
                if (recordedOdometerString[1].toString().length > 3) {
                    $(".trip-odometer-sub").html(recordedOdometerString[1].toString().substring(0, 3));
                } else {
                    $(".trip-odometer-sub").html(recordedOdometerString[1].toString());
                }
                $(".trip-odometer-sub").attr("data-value", recordedOdometerString[1]);
            }
            //update last position
            tripLastPosition = {
                latitude: currentLat,
                longitude: currentLong
            }
            tripCoordinates.push(`${currentLat},${currentLong}`);
        }
    }
}
function stopRecording(errMsg) {
    if (errMsg && errMsg.code) {
        switch (errMsg.code) {
            case 1:
                errorToast(errMsg.message);
                break;
            case 2:
                errorToast("Location Unavailable");
                break;
        }
    }
    if (tripTimer != undefined) {
        clearInterval(tripTimer);
        tripTimer = undefined;
    }
    if (tripWakeLock != undefined) {
        tripWakeLock.release();
        tripWakeLock = undefined;
    }
    if (tripLastPosition != undefined) {
        tripLastPosition = undefined;
    }
    $(".trip-start").removeClass('d-none');
    $(".trip-stop").addClass('d-none');
    $("#odometerRecordModal").off("hide.bs.modal");
    if (parseInt(getRecordedOdometer()) != $("#initialOdometerRecordMileage").val()) {
        $(".trip-save").removeClass('d-none');
    }
}
// Converts numeric degrees to radians
function toRad(Value) {
    return Value * Math.PI / 180;
}
//haversine
function calculateDistance(lat1, lon1, lat2, lon2) {
    var earthRadius = 6371; // km radius of the earth
    var dLat = toRad(lat2 - lat1);
    var dLon = toRad(lon2 - lon1);
    var lat1 = toRad(lat1);
    var lat2 = toRad(lat2);

    var sinOne = Math.sin(dLat / 2) * Math.sin(dLat / 2) +
        Math.sin(dLon / 2) * Math.sin(dLon / 2) * Math.cos(lat1) * Math.cos(lat2);
    var tanOne = 2 * Math.atan2(Math.sqrt(sinOne), Math.sqrt(1 - sinOne));
    var calculatedDistance = earthRadius * tanOne; 
    if (getGlobalConfig().useMPG) {
        calculatedDistance *= 0.621; //convert to mile if needed.
    }
    return Math.abs(calculatedDistance);
}
function getRecordedOdometer() {
    var recordedOdometer = $(".trip-odometer").html();
    var recordedSubOdometer = $(".trip-odometer-sub").attr("data-value");
    return parseFloat(`${recordedOdometer}.${recordedSubOdometer}`);
}
function saveRecordedOdometer() {
    //save coordinates into a CSV file and upload
    if (tripCoordinates.length > 1) {
        //update current odometer value
        $("#odometerRecordMileage").val(parseInt(getRecordedOdometer()).toString());
        //generate attachment
        $.post('/Files/UploadCoordinates', { coordinates: tripCoordinates }, function (response) {
            uploadedFiles.push(response);
            $.post('/Vehicle/GetFilesPendingUpload', { uploadedFiles: uploadedFiles }, function (viewData) {
                $("#filesPendingUpload").html(viewData);
                tripCoordinates = ["Latitude,Longitude"];
            });
        });
    }
    hideTripModal();
}
function toggleSubOdometer() {
    if ($(".trip-odometer-sub").hasClass("d-none")) {
        $(".trip-odometer-sub").removeClass("d-none");
    } else {
        $(".trip-odometer-sub").addClass("d-none");
    }
}
function checkTripRecorder() {
    //check if connection is https, browser supports required API, and that vehicle does not use engine hours
    if (location.protocol != 'https:' || !navigator.geolocation || !navigator.wakeLock || GetVehicleId().useEngineHours) {
        $(".trip-show").remove();
    } else {
        $(".trip-show").removeClass('d-none');
    }
}