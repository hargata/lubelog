function focusKioskVehicle(sender) {
    let vehicleId = $(sender).attr('data-vehicleId');
    showKioskVehicle(vehicleId);
}
function showKioskVehicle(vehicleId) {
    $.get('/Home/GetKioskVehicleInfo', { vehicleId: vehicleId }, function (data) {
        $('.kiosk-tab-content').html(data);
        $('.kiosk-card-container').hide();
        $(`.kiosk-card-container:has(.kiosk-card[data-vehicleId='${vehicleId}'])`).show();
        $(`.kiosk-card-container .kiosk-card[data-vehicleId='${vehicleId}']`).addClass('kiosk-card-expanded');
        $('.kiosk-card-details').show();
        $(".kiosk-content").masonry();
        setBrowserHistory('vehicleId', vehicleId)
    });
}
function showAllKioskVehicle(event) {
    event.stopPropagation()
    $('.kiosk-card-container').show();
    $('.kiosk-card').removeClass('kiosk-card-expanded');
    $('.kiosk-card-details').hide();
    $(".kiosk-content").masonry();
    setBrowserHistory('vehicleId', '');
}
function redirectToPlanner() {
    let currentParams = new URLSearchParams(window.location.search);
    currentParams.set('kioskMode', 'plan');
    let updatedURL = `${window.location.origin}${window.location.pathname}?${currentParams.toString()}`;
    window.location.href = updatedURL;
}
function redirectToReminder() {
    let currentParams = new URLSearchParams(window.location.search);
    currentParams.set('kioskMode', 'reminder');
    let updatedURL = `${window.location.origin}${window.location.pathname}?${currentParams.toString()}`;
    window.location.href = updatedURL;
}
function redirectToKiosk() {
    let currentParams = new URLSearchParams(window.location.search);
    currentParams.delete('kioskMode');
    let updatedURL = `${window.location.origin}${window.location.pathname}?${currentParams.toString()}`;
    window.location.href = updatedURL;
}
function filterKioskPlan(sender) {
    let selectedVal = $(sender).val();
    if (selectedVal != 0) {
        $('[data-vehicleId]').hide();
        $(`[data-vehicleId='${selectedVal}']`).show();
        setBrowserHistory('vehicleId', selectedVal);
    } else {
        $('[data-vehicleId]').show();
        setBrowserHistory('vehicleId', '');
    }
}
function showKioskPlan(sender) {
    let planCard = $(sender).closest('[data-recordId]');
    let swimlane = $(sender).closest('[data-column]');
    $('[data-recordId]').hide();
    $('[data-column]').hide();
    swimlane.show();
    planCard.show();
    $('.kiosk-plan-details-container').show();
    let dataToCopy = planCard.find('.kiosk-plan-details-copyable').html();
    $('.kiosk-plan-details-target').html(dataToCopy);
    setMarkDownPlanNotes($('.kiosk-plan-details-target .stickerNote'));
}
function setMarkDownPlanNotes(elem) {
    if (elem.length > 0) {
        let originalStickerNote = elem.html().trim();
        let markDownStickerNote = markdown(originalStickerNote);
        elem.html(markDownStickerNote);
    }
}
function showAllKioskPlan() {
    $('[data-column]').show();
    $('[data-recordId]').show();
    $('.kiosk-plan-details-container').hide();
    $('.kiosk-plan-selector').trigger('change');
}
function filterKioskReminder(sender) {
    let selectedVal = $(sender).val();
    if (selectedVal != 0) {
        $('[data-vehicleId]').hide();
        $(`[data-vehicleId='${selectedVal}']`).show();
        setBrowserHistory('vehicleId', selectedVal);
    } else {
        $('[data-vehicleId]').show();
        setBrowserHistory('vehicleId', '');
    }
    $('.kiosk-content').masonry();
}
function showAllKioskReminder() {
    $('[data-recordId]').show();
    $('.kiosk-reminder-selector').trigger('change');
    $('.kiosk-card').removeClass('kiosk-card-expanded');
    $('.kiosk-card-details').hide();
    $('.kiosk-reminder-selector').prop('disabled', false);
    $('.kiosk-content').masonry();
}
function showKioskReminder(sender) {
    let reminderCard = $(sender).closest('[data-recordId]');
    let reminderKioskCard = reminderCard.find('.kiosk-card');
    if (reminderKioskCard.hasClass('kiosk-card-expanded')) {
        showAllKioskReminder();
        return;
    }
    $('[data-recordId]').hide();
    reminderCard.show();
    reminderKioskCard.addClass('kiosk-card-expanded');
    $('.kiosk-reminder-selector').prop('disabled', true);
    $('.kiosk-card-details').show();
    let dataToCopy = reminderCard.find('.stickerNote').html();
    $('.kiosk-tab-content').html(dataToCopy);
    setMarkDownPlanNotes($('.kiosk-tab-content'));
    $(".kiosk-content").masonry();
}
function kioskSelectLoadVehicleIdFromParam(elem) {
    let currentParams = new URLSearchParams(window.location.search);
    let vehicleIdToLoad = currentParams.get('vehicleId');
    if (vehicleIdToLoad != null && elem.find(`option[value='${vehicleIdToLoad}']`).length > 0) {
        elem.val(vehicleIdToLoad);
        elem.trigger('change');
    }
}

function kioskLoadVehicleIdFromParam() {
    let currentParams = new URLSearchParams(window.location.search);
    let vehicleIdToLoad = currentParams.get('vehicleId');
    if (vehicleIdToLoad != null) {
        showKioskVehicle(vehicleIdToLoad);
    }
}
function setAccessToken(accessToken) {
    //use this function to never worry about user session expiring.
    $.ajaxSetup({
        headers: {
            'Authorization': `Basic ${accessToken}`
        }
    });
    console.log("Access Token for Kiosk Mode Configured!");
}
function initKiosk() {
    $("body > div").removeClass("container");
    $("body > div").css('height', '100vh');
    subtractAmount = parseInt($("#kioskContainer").width() * 0.0016); //remove 0.0016% of width every 100 ms which will approximate to one minute.
    if (subtractAmount < 2) {
        subtractAmount = 2;
    }
    retrieveKioskContent();
    //acquire wakeLock;
    try {
        navigator.wakeLock.request('screen').then((wl) => {
            kioskWakeLock = wl;
        });
    } catch (err) {
        warnToast('Wake Lock Not Acquired');
    }
}
function retrieveKioskContent() {
    clearInterval(refreshTimer);
    if (kioskMode != 'Cycle') {
        $.post('/Home/KioskContent', { exclusions: exceptionList, kioskMode: kioskMode }, function (data) {
            $("#kioskContainer").html(data);
            $(".kiosk-content").masonry();
            if ($(".no-data-message").length == 0) {
                $(".progress-bar").width($("#kioskContainer").width());
                setTimeout(function () { startTimer() }, 500);
            }
        });
    } else {
        //cycle mode
        switch (currentKioskMode) {
            case "Vehicle":
                currentKioskMode = "Reminder";
                break;
            case "Reminder":
                currentKioskMode = "Plan";
                break;
            case "Plan":
                currentKioskMode = "Vehicle";
                break;
        }
        $.post('/Home/KioskContent', { exclusions: exceptionList, kioskMode: currentKioskMode }, function (data) {
            $("#kioskContainer").html(data);
            $(".kiosk-content").masonry();
            if ($(".no-data-message").length > 0) {
                //if no data on vehicle page
                if (currentKioskMode == "Vehicle") {
                    return; //exit
                } else {
                    retrieveKioskContent(); //skip until we hit a page with content.
                }
            } else {
                $(".progress-bar").width($("#kioskContainer").width());
                setTimeout(function () { startTimer() }, 500);
            }
        });
    }

}
function startTimer() {
    refreshTimer = setInterval(function () {
        var currentWidth = $(".progress-bar").width();
        if (currentWidth > 0) {
            $(".progress-bar").width(currentWidth - subtractAmount);
        } else {
            retrieveKioskContent();
        }
    }, 100);
}
function addVehicleToExceptionList(vehicleId) {
    Swal.fire({
        title: "Remove Vehicle from Dashboard?",
        text: "Removed vehicles can be restored by refreshing the page",
        showCancelButton: true,
        confirmButtonText: "Remove",
        confirmButtonColor: "#dc3545"
    }).then((result) => {
        if (result.isConfirmed) {
            exceptionList.push(vehicleId);
            if (kioskMode == 'Cycle') {
                //remove the vehicle programmatically.
                $(`[data-vehicleId=${vehicleId}]`).remove();
            } else {
                //force a refresh
                retrieveKioskContent();
            }
        }
    });
}