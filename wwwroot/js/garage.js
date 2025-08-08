function showAddVehicleModal() {
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
        $("#garageContainer").html(data);
        loadSettings();
        bindTabEvent();
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
            case "supply-tab":
                getVehicleSupplyRecords();
                break;
            case "calendar-tab":
                getVehicleCalendarEvents();
                break;
        }
        switch (e.relatedTarget.id) { //clear out previous tabs with grids in them to help with performance
            case "supply-tab":
                $("#supply-tab-pane").html("");
                break;
            case "calendar-tab":
                $("#calendar-tab-pane").html("");
                break;
        }
        $(`.lubelogger-tab #${e.target.id}`).addClass('active');
        $(`.lubelogger-mobile-nav #${e.target.id}`).addClass('active');
        $(`.lubelogger-tab #${e.relatedTarget.id}`).removeClass('active');
        $(`.lubelogger-mobile-nav #${e.relatedTarget.id}`).removeClass('active');
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
    var rowData = $(".garage-item");
    if (sender == undefined) {
        rowData.removeClass('override-hide');
        return;
    }
    var tagName = sender.textContent;
    if ($(sender).hasClass("bg-primary")) {
        rowData.removeClass('override-hide');
        $(sender).removeClass('bg-primary');
        $(sender).addClass('bg-secondary');
    } else {
        //hide table rows.
        rowData.addClass('override-hide');
        $(`[data-tags~='${tagName}']`).removeClass('override-hide');
        if ($(".tagfilter.bg-primary").length > 0) {
            //disabling other filters
            $(".tagfilter.bg-primary").addClass('bg-secondary');
            $(".tagfilter.bg-primary").removeClass('bg-primary');
        }
        $(sender).addClass('bg-primary');
        $(sender).removeClass('bg-secondary');
    }
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
function sortGarage(sender) {
    console.log(sender);
    if (event != undefined) {
        event.preventDefault();
    }
    sender = $(sender);
    var sortColumn = sender.text();
    var sortColumnField = sender.attr('data-sortcolumn');
    var sortDirection;
    var sortFieldType = sender.attr('data-sorttype');
    var sortAscIcon = '<i class="bi bi-sort-numeric-down ms-2"></i>';
    var sortDescIcon = '<i class="bi bi-sort-numeric-up-alt ms-2"></i>';
    $('[aria-labelledby="sortDropdown"] li button').removeClass('active');
    
    console.log(sortColumn + ' ' + sortFieldType);

    if (sender.hasClass('sort-asc')) {
        // change to descending sort
        sortDirection = 'desc';
        sender.removeClass('sort-asc');
        sender.addClass('sort-desc');
        sender.html(`${sortColumn}${sortDescIcon}`);
        sender.addClass('active');
        $('#sortDropdown span').text(sortColumn);
        sortVehicles(sortColumnField, sortFieldType, true);

    } else if (sender.hasClass('sort-desc')) {
        // restore to default sort 

        sender.removeClass('sort-desc');
        sender.html(`${sortColumn}`);
        $('[aria-labelledby="sortDropdown"] li button').first().addClass('active');
        $('#sortDropdown span').text($('[aria-labelledby="sortDropdown"] li button').first().text());
        var defaultSort = $('[aria-labelledby="sortDropdown"] li button').first();
        var defaultSortName = defaultSort.text();
        var defaultSortField = defaultSort.attr('data-sortcolumn');
        var defaultSortType = defaultSort.attr('data-sorttype');

        $('#sortDropdown span').text(defaultSortName);

        sortVehicles(defaultSortField, defaultSortType);
    } else {
        //first time sorting, default ascending sort

        if ($(".sort-asc").length > 0) {
            $(".sort-asc").html($(".sort-asc").html().replace(sortAscIcon, ""));
            $(".sort-asc").removeClass("sort-asc");
        }
        if ($(".sort-desc").length > 0) {
            $(".sort-desc").html($(".sort-desc").html().replace(sortDescIcon, ""));
            $(".sort-desc").removeClass("sort-desc");
        }

        sender.addClass('sort-asc');
        sender.addClass('active');
        $('#sortDropdown span').text(sortColumn);
        sender.html(`${sortColumn}${sortAscIcon}`);

        sortVehicles(sortColumnField, sortFieldType);     
    }
    $.post('/Home/SaveVehicleSort', { vehicleSortField: sortColumnField + (sortDirection ? ' ' + sortDirection : '') }, function (data) {
        if (!data) {
            errorToast(genericErrorMessage());
        }
    })
}
function sortVehicles(sortColumnField, sortFieldType, desc) {
    var rowData = $('.garage-item');

    // Get secondary sort field details
    var secondaryButton = $('[aria-labelledby="sortDropdown"] li button.secondary');
    var secondarySortColumnField = secondaryButton.attr('data-sortcolumn');
    var secondarySortFieldType = secondaryButton.attr('data-sorttype');

    var sortedRow = rowData.toArray().sort((a, b) => {
        var currentVal = getSortValue(a, sortColumnField, sortFieldType);
        var nextVal = getSortValue(b, sortColumnField, sortFieldType);

        // Primary sort
        if (['number', 'decimal', 'date', 'time'].includes(sortFieldType)) {
            if (currentVal !== nextVal) {
                return desc ? nextVal - currentVal : currentVal - nextVal;
            }
        } else {
            const comparison = currentVal.localeCompare(nextVal, undefined, { sensitivity: 'base' });
            if (comparison !== 0) {
                return desc ? -comparison : comparison;
            }
        }

        // If primary values are equal, apply secondary sort
        var currentSecondaryVal = getSortValue(a, secondarySortColumnField, secondarySortFieldType);
        var nextSecondaryVal = getSortValue(b, secondarySortColumnField, secondarySortFieldType);

        if (['number', 'decimal', 'date', 'time'].includes(secondarySortFieldType)) {
            return currentSecondaryVal - nextSecondaryVal; // always ascending for secondary
        } else {
            return currentSecondaryVal.localeCompare(nextSecondaryVal, undefined, { sensitivity: 'base' });
        }
    });

    sortedRow.push($('.garage-item-add'));
    $('.vehiclesContainer').html(sortedRow);
}

function getSortValue(element, sortColumnField, sortFieldType) {
    sortFieldType = (sortFieldType || 'text').toLowerCase(); // Default to 'text'

    const attrElement = $(element).find('[data-' + sortColumnField + ']');
    if (!attrElement.length) return '';

    const value = attrElement.attr('data-' + sortColumnField);
    if (value == null || value === '') return '';

    switch (sortFieldType) {
        case 'decimal':
        case 'number': {
            const num = parseFloat(value);
            return isNaN(num) ? Number.NEGATIVE_INFINITY : num;
        }
        case 'date': {
            const timestamp = Date.parse(value);
            return isNaN(timestamp) ? new Date(0) : new Date(timestamp);
        }
        case 'time': {
            // Expecting HH:mm or similar format
            const timeParts = value.split(':');
            if (timeParts.length === 2) {
                const hours = parseInt(timeParts[0], 10);
                const minutes = parseInt(timeParts[1], 10);
                if (!isNaN(hours) && !isNaN(minutes)) {
                    return hours * 60 + minutes; // Convert to minutes for comparison
                }
            }
            return 0;
        }
        case 'location': {
            return value.trim().toLowerCase(); // Just treat as string for now
        }
        case 'text':
        default:
            return value.toString().toLowerCase(); // Case-insensitive string sort
    }
}
