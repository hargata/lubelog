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