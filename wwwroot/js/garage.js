﻿function showAddVehicleModal() {
    uploadedFile = "";
    $.get('/Vehicle/AddVehiclePartialView', function (data) {
        if (data) {
            $("#addVehicleModalContent").html(data);
            initTagSelector($("#inputTag"));
            initDatePicker($('#inputPurchaseDate'));
            initDatePicker($('#inputSoldDate'));
            $('#addVehicleModal').modal('show');
        }
    })
}
function hideAddVehicleModal() {
    $('#addVehicleModal').modal('hide');
}
//refreshable function to reload Garage PartialView
function loadGarage() {
    $.get('/Home/Garage', function (data) {
        $("#garage-tab-pane").html(data);
    });
}
function loadSettings() {
    $.get('/Home/Settings', function (data) {
        $("#settings-tab-pane").html(data);
    });
}
function getVehicleSupplyRecords() {
    $.get(`/Vehicle/GetSupplyRecordsByVehicleId?vehicleId=0`, function (data) {
        if (data) {
            $("#supply-tab-pane").html(data);
            restoreScrollPosition();
        }
    });
}
function GetVehicleId() {
    return { vehicleId: 0, hasOdometerAdjustment: false };
}
function bindTabEvent() {
    $('button[data-bs-toggle="tab"]').on('show.bs.tab', function (e) {
        switch (e.target.id) {
            case "garage-tab":
                loadGarage();
                break;
            case "settings-tab":
                loadSettings();
                break;
            case "supply-tab":
                getVehicleSupplyRecords();
                break;
            case "calendar-tab":
                getVehicleCalendarEvents();
                break;
        }
        $(`.lubelogger-tab #${e.target.id}`).addClass('active');
        $(`.lubelogger-mobile-nav #${e.target.id}`).addClass('active');
        if (e.relatedTarget != null) {
            switch (e.relatedTarget.id) { //clear out previous tabs with grids in them to help with performance
                case "garage-tab":
                    $("#garage-tab-pane").html("");
                    break;
                case "settings-tab":
                    $("#settings-tab-pane").html("");
                    break;
                case "supply-tab":
                    $("#supply-tab-pane").html("");
                    break;
                case "calendar-tab":
                    $("#calendar-tab-pane").html("");
                    break;
            }
            $(`.lubelogger-tab #${e.relatedTarget.id}`).removeClass('active');
            $(`.lubelogger-mobile-nav #${e.relatedTarget.id}`).removeClass('active');
        }
        setBrowserHistory('tab', getTabNameForURL(e.target.id));
    });
}
function getVehicleCalendarEvents() {
    $.get('/Home/Calendar', function (data) {
        if (data) {
            $("#calendar-tab-pane").html(data);
        }
    });
}
function showCalendarReminderModal(id) {
    event.stopPropagation();
    $.get(`/Home/ViewCalendarReminder?reminderId=${id}`, function (data) {
        if (data) {
            $("#reminderRecordCalendarModalContent").html(data);
            $("#reminderRecordCalendarModal").modal('show');
            $('#reminderRecordCalendarModal').off('shown.bs.modal').on('shown.bs.modal', function () {
                if (getGlobalConfig().useMarkDown) {
                    toggleMarkDownOverlay("reminderNotes");
                }
            });
        }
    })
}
function hideCalendarReminderModal() {
    $("#reminderRecordCalendarModal").modal('hide');
}
function generateReminderItem(id, urgency, description) {
    if (description.trim() == '') {
        return;
    }
    switch (urgency) {
        case "VeryUrgent":
            return `<p class="badge text-wrap bg-danger reminder-calendar-item mb-2" onclick='showCalendarReminderModal(${id})'>${encodeHTMLInput(description)}</p>`;
        case "PastDue":
            return `<p class="badge text-wrap bg-secondary reminder-calendar-item mb-2" onclick='showCalendarReminderModal(${id})'>${encodeHTMLInput(description)}</p>`;
        case "Urgent":
            return `<p class="badge text-wrap text-bg-warning reminder-calendar-item mb-2" onclick='showCalendarReminderModal(${id})'>${encodeHTMLInput(description)}</p>`;
        case "NotUrgent":
            return `<p class="badge text-wrap bg-success reminder-calendar-item mb-2" onclick='showCalendarReminderModal(${id})'>${encodeHTMLInput(description)}</p>`;
    }
}
function markDoneCalendarReminderRecord(reminderRecordId, e) {
    event.stopPropagation();
    $.post(`/Vehicle/PushbackRecurringReminderRecord?reminderRecordId=${reminderRecordId}`, function (data) {
        if (data) {
            hideCalendarReminderModal();
            successToast("Reminder Updated");
            getVehicleCalendarEvents();
        } else {
            errorToast(genericErrorMessage());
        }
    });
}
function deleteCalendarReminderRecord(reminderRecordId, e) {
    if (e != undefined) {
        event.stopPropagation();
    }
    $("#workAroundInput").show();
    Swal.fire({
        title: "Confirm Deletion?",
        text: "Deleted Reminders cannot be restored.",
        showCancelButton: true,
        confirmButtonText: "Delete",
        confirmButtonColor: "#dc3545"
    }).then((result) => {
        if (result.isConfirmed) {
            $.post(`/Vehicle/DeleteReminderRecordById?reminderRecordId=${reminderRecordId}`, function (data) {
                if (data) {
                    hideCalendarReminderModal();
                    successToast("Reminder Deleted");
                    getVehicleCalendarEvents();
                } else {
                    errorToast(genericErrorMessage());
                }
            });
        } else {
            $("#workAroundInput").hide();
        }
    });
}
function initCalendar() {
    if (groupedDates.length == 0) {
        //group dates
        eventDates.map(x => {
            var existingIndex = groupedDates.findIndex(y => y.date == x.date);
            if (existingIndex == -1) {
                groupedDates.push({ date: x.date, reminders: [`${generateReminderItem(x.id, x.urgency, x.description)}`] });
            } else if (existingIndex > -1) {
                groupedDates[existingIndex].reminders.push(`${generateReminderItem(x.id, x.urgency, x.description)}`);
            }
        });
    }
    $(".reminderCalendarViewContent").datepicker({
        startDate: "+0d",
        format: getShortDatePattern().pattern,
        todayHighlight: true,
        weekStart: getGlobalConfig().firstDayOfWeek,
        beforeShowDay: function (date) {
            var reminderDateIndex = groupedDates.findIndex(x => (x.date == date.getTime() || x.date == (date.getTime() - date.getTimezoneOffset() * 60000))); //take into account server timezone offset
            if (reminderDateIndex > -1) {
                return {
                    enabled: true,
                    classes: 'reminder-exist',
                    content: `<div class='text-wrap' style='height:20px;'><p>${date.getDate()}</p>${groupedDates[reminderDateIndex].reminders.join('<br>')}</div>`
                }
            }
        }
    });
}
function performLogOut() {
    $.post('/Login/LogOut', function (data) {
        if (data) {
            window.location.href = data;
        }
    })
}
function loadPinnedNotes(vehicleId) {
    var hoveredGrid = $(`#gridVehicle_${vehicleId}`);
    if (hoveredGrid.attr("data-bs-title") != '') {
        hoveredGrid.tooltip("show");
    }
}
function hidePinnedNotes(vehicleId) {
    if ($(`#gridVehicle_${vehicleId}`).attr('data-bs-title') != '') {
        $(`#gridVehicle_${vehicleId}`).tooltip("hide");
    }
}

function filterGarage(sender) {
    let searchQuery = $('#garageSearchInput').val();
    if (sender == undefined) {
        searchAndFilterGarage(undefined, searchQuery);
        return;
    }
    var tagName = sender.textContent;
    if ($(sender).hasClass("bg-primary")) {
        searchAndFilterGarage(undefined, searchQuery);
        $(sender).removeClass('bg-primary');
        $(sender).addClass('bg-secondary');
    } else {
        searchAndFilterGarage(tagName, searchQuery);
        if ($(".tagfilter.bg-primary").length > 0) {
            //disabling other filters
            $(".tagfilter.bg-primary").addClass('bg-secondary');
            $(".tagfilter.bg-primary").removeClass('bg-primary');
        }
        $(sender).addClass('bg-primary');
        $(sender).removeClass('bg-secondary');
    }
}
function handleGarageSearchKeyPress(event) {
    if (event.keyCode == 13) {
        searchGarage();
    } else {
        setDebounce(searchGarage);
    }
}
function searchGarage() {
    let searchTerm = $('#garageSearchInput').val();
    let activeTag = $(".tagfilter.bg-primary").length > 0 ? $(".tagfilter.bg-primary")[0].textContent : undefined;
    searchAndFilterGarage(activeTag, searchTerm);
}
function searchAndFilterGarage(searchTag, searchTerm) {
    let rowData = $(".garage-item");
    let searchTagEmpty = searchTag == undefined || searchTag.trim() == '';
    let searchQueryEmpty = searchTerm == undefined || searchTerm.trim() == '';
    if (searchTagEmpty && searchQueryEmpty) {
        //show all garage items
        rowData.removeClass('override-hide');
        return;
    }
    //hide all garage items
    rowData.addClass('override-hide');
    if (!searchQueryEmpty && !searchTagEmpty) {
        $(`.garage-item .card-body .garage-item-attribute:containsNC('${searchTerm}')`).closest(`.garage-item[data-tags~='${searchTag}']`).removeClass('override-hide');
    } else {
        if (!searchTagEmpty) {
            //show all garage items with matching tags
            $(`[data-tags~='${searchTag}']`).removeClass('override-hide');
        }
        if (!searchQueryEmpty) {
            //show all garage items with matching search terms
            $(`.garage-item .card-body .garage-item-attribute:containsNC('${searchTerm}')`).closest('.garage-item').removeClass('override-hide');
        }
    }
}
function sortVehicles(desc) {
    //get row data
    var rowData = $('.garage-item');
    var sortedRow = rowData.toArray().sort((a, b) => {
        var currentVal = globalParseFloat($(a).find(".garage-item-year").attr('data-unit'));
        var nextVal = globalParseFloat($(b).find(".garage-item-year").attr('data-unit'));
        if (desc) {
            return nextVal - currentVal;
        } else {
            return currentVal - nextVal;
        }
    });
    sortedRow.push($('.garage-item-add'))
    $('.vehiclesContainer').html(sortedRow);
}

var touchtimer;
var touchduration = 800;
function detectLongTouch(sender) {
    if ($(sender).hasClass("active")) {
        if (!touchtimer) {
            touchtimer = setTimeout(function () { sortGarage(sender, true); detectTouchEndPremature(sender); }, touchduration);
        }
    }
}
function detectTouchEndPremature(sender) {
    if (touchtimer) {
        clearTimeout(touchtimer);
        touchtimer = null;
    }
}

function sortGarage(sender, isMobile) {
    if (event != undefined) {
        event.preventDefault();
    }
    sender = $(sender);
    if (sender.hasClass("active")) {
        //do sorting only if garage is the active tab.
        var sortColumn = sender.text();
        var garageIcon = '<i class="bi bi-car-front me-2"></i>';
        var sortAscIcon = '<i class="bi bi-sort-numeric-down ms-2"></i>';
        var sortDescIcon = '<i class="bi bi-sort-numeric-down-alt ms-2"></i>';
        if (sender.hasClass('sort-asc')) {
            sender.removeClass('sort-asc');
            sender.addClass('sort-desc');
            sender.html(isMobile ? `<span class="ms-2 display-3">${garageIcon}${sortColumn}${sortDescIcon}</span>` : `${garageIcon}${sortColumn}${sortDescIcon}`);
            sortVehicles(true);
        } else if (sender.hasClass('sort-desc')) {
            //restore table
            sender.removeClass('sort-desc');
            sender.html(isMobile ? `<span class="ms-2 display-3">${garageIcon}${sortColumn}</span>` : `${garageIcon}${sortColumn}`);
            resetSortGarage();
        } else {
            //first time sorting.
            //check if table was sorted before by a different column(only relevant to fuel tab)
            if ($("[default-sort]").length > 0 && ($(".sort-asc").length > 0 || $(".sort-desc").length > 0)) {
                //restore table state.
                resetSortGarage();
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
            sender.html(isMobile ? `<span class="ms-2 display-3">${garageIcon}${sortColumn}${sortAscIcon}</span>` : `${garageIcon}${sortColumn}${sortAscIcon}`);
            //append sortRowId to the vehicle container
            if ($("[default-sort]").length == 0) {
                $(`.garage-item`).map((index, elem) => {
                    $(elem).attr("default-sort", index);
                });
            }
            sortVehicles(false);
        }
    }
}
function resetSortGarage() {
    var rowData = $(`.garage-item`);
    var sortedRow = rowData.toArray().sort((a, b) => {
        var currentVal = $(a).attr('default-sort');
        var nextVal = $(b).attr('default-sort');
        return currentVal - nextVal;
    });
    $(".garage-item-add").map((index, elem) => {
        sortedRow.push(elem);
    })
    $(`.vehiclesContainer`).html(sortedRow);
}

let dragged = null;
let draggedId = 0;
function dragEnter(event) {
    event.preventDefault();
}
function dragStart(event, vehicleId) {
    dragged = event.target;
    draggedId = vehicleId;
    event.dataTransfer.setData('text/plain', draggedId);
}
function dragOver(event) {
    event.preventDefault();
}
function dropBox(event, targetVehicleId) {
    if (dragged.parentElement != event.target && event.target != dragged && draggedId != targetVehicleId) {
        copyContributors(draggedId, targetVehicleId);
    }
    event.preventDefault();
}
function copyContributors(sourceVehicleId, destVehicleId) {
    var sourceVehicleName = $(`#gridVehicle_${sourceVehicleId} .card-body`).children('h5').map((index, elem) => { return elem.innerText }).toArray().join(" ");
    var destVehicleName = $(`#gridVehicle_${destVehicleId} .card-body`).children('h5').map((index, elem) => { return elem.innerText }).toArray().join(" ");
    Swal.fire({
        title: "Copy Collaborators?",
        text: `Copy collaborators over from ${sourceVehicleName} to ${destVehicleName}?`,
        showCancelButton: true,
        confirmButtonText: "Copy",
        confirmButtonColor: "#0d6efd"
    }).then((result) => {
        if (result.isConfirmed) {
            $.post('/Vehicle/DuplicateVehicleCollaborators', { sourceVehicleId: sourceVehicleId, destVehicleId: destVehicleId }, function (data) {
                if (data.success) {
                    successToast("Collaborators Copied");
                    loadGarage();
                } else {
                    errorToast(data.message);
                }
            })
        } else {
            $("#workAroundInput").hide();
        }
    });
}

function showAccountInformationModal() {
    $.get('/Home/GetUserAccountInformationModal', function (data) {
        $('#accountInformationModalContent').html(data);
        $('#accountInformationModal').modal('show');
    })
}

function showRootAccountInformationModal() {
    $.get('/Home/GetRootAccountInformationModal', function (data) {
        $('#accountInformationModalContent').html(data);
        $('#accountInformationModal').modal('show');
    })
}
function validateAndSaveRootUserAccount() {
    var hasError = false;
    if ($('#inputUsername').val().trim() == '') {
        $('#inputUsername').addClass("is-invalid");
        hasError = true;
    } else {
        $('#inputUsername').removeClass("is-invalid");
    }
    if ($('#inputPassword').val().trim() == '') {
        $('#inputPassword').addClass("is-invalid");
        hasError = true;
    } else {
        $('#inputPassword').removeClass("is-invalid");
    }
    if (hasError) {
        errorToast("Please check the form data");
        return;
    }
    var userAccountInfo = {
        userName: $('#inputUsername').val(),
        password: $('#inputPassword').val()
    }
    $.post('/Login/CreateLoginCreds', { credentials: userAccountInfo }, function (data) {
        if (data) {
            //hide modal
            hideAccountInformationModal();
            successToast('Root Account Updated');
            performLogOut();
        } else {
            errorToast(data.message);
        }
    });
}

function hideAccountInformationModal() {
    $('#accountInformationModal').modal('hide');
}
function validateAndSaveUserAccount() {
    var hasError = false;
    if ($('#inputUsername').val().trim() == '') {
        $('#inputUsername').addClass("is-invalid");
        hasError = true;
    } else {
        $('#inputUsername').removeClass("is-invalid");
    }
    if ($('#inputEmail').val().trim() == '') {
        $('#inputEmail').addClass("is-invalid");
        hasError = true;
    } else {
        $('#inputEmail').removeClass("is-invalid");
    }
    if ($('#inputToken').val().trim() == '') {
        $('#inputToken').addClass("is-invalid");
        hasError = true;
    } else {
        $('#inputToken').removeClass("is-invalid");
    }
    if (hasError) {
        errorToast("Please check the form data");
        return;
    }
    var userAccountInfo = {
        userName: $('#inputUsername').val(),
        password: $('#inputPassword').val(),
        emailAddress: $('#inputEmail').val(),
        token: $('#inputToken').val()
    }
    $.post('/Home/UpdateUserAccount', { userAccount: userAccountInfo }, function (data) {
        if (data.success) {
            //hide modal
            hideAccountInformationModal();
            successToast('Profile Updated');
            performLogOut();
        } else {
            errorToast(data.message);
        }
    });
}
function generateTokenForUser() {
    $.post('/Home/GenerateTokenForUser', function (data) {
        if (data) {
            successToast('Token sent');
        } else {
            errorToast(genericErrorMessage())
        }
    });
}
function loadTabFromURL() {
    let tabFromURL = getTabNameFromURL('garage');
    waitForElement(`#${tabFromURL}`, () => { $(`#${tabFromURL}`).tab('show'); }, '');
}