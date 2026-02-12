function showInspectionRecordTemplateSelectorModal() {
    var vehicleId = GetVehicleId().vehicleId;
    $.get(`/Vehicle/GetInspectionRecordTemplatesByVehicleId?vehicleId=${vehicleId}`, function (data) {
        if (data) {
            $("#inspectionRecordTemplateModalContent").html(data);
            $('#inspectionRecordTemplateModal').modal('show');
            clearModalContentOnHide($('#inspectionRecordTemplateModal'));
        }
    });
}
function hideInspectionRecordTemplateSelectorModal() {
    $('#inspectionRecordTemplateModal').modal('hide');
}
function showAddInspectionRecordTemplateModal() {
    $.get('/Vehicle/GetAddInspectionRecordTemplatePartialView', function (data) {
        if (data) {
            $("#inspectionRecordTemplateEditModalContent").html(data);
            //initiate tag selector
            initTagSelector($("#inspectionRecordTemplateTag"));
            hideInspectionRecordTemplateSelectorModal();
            $('#inspectionRecordTemplateEditModal').modal('show');
            clearModalContentOnHide($('#inspectionRecordTemplateEditModal'));
        }
    });
}
function showEditInspectionRecordTemplateModal(inspectionRecordTemplateId) {
    $.get(`/Vehicle/GetEditInspectionRecordTemplatePartialView?inspectionRecordTemplateId=${inspectionRecordTemplateId}`, function (data) {
        if (data) {
            $("#inspectionRecordTemplateEditModalContent").html(data);
            //initiate tag selector
            initTagSelector($("#inspectionRecordTemplateTag"));
            hideInspectionRecordTemplateSelectorModal();
            $('#inspectionRecordTemplateEditModal').modal('show');
            clearModalContentOnHide($('#inspectionRecordTemplateEditModal'));
        }
    });
}
function hideInspectionRecordTemplateModal() {
    $('#inspectionRecordTemplateEditModal').modal('hide');
    showInspectionRecordTemplateSelectorModal();
}
function addInspectionRecordField() {
    $.get('/Vehicle/GetAddInspectionRecordFieldPartialView', function (data) {
        $("#inspectionRecordFields").append(data);
    });
}
function deleteInspectionRecordField(e) {
    $(e).closest('[data-type="field"]').remove();
}
function duplicateInspectionRecordField(e) {
    let currentField = $(e).closest('[data-type="field"]');
    let clonedField = currentField.clone();
    //$("#inspectionRecordFields").append(clonedField);
    clonedField.insertAfter(currentField);
}
function setDropDownOptionSelected(dropDownElem) {
    let selectedVal = $(dropDownElem).val();
    $(dropDownElem).find('option').removeAttr('selected');
    $(dropDownElem).find(`option[value="${selectedVal}"]`).attr('selected', '');
}
function handleInspectionRecordFieldTypeChange(e) {
    setDropDownOptionSelected(e);
    let selectedVal = $(e).val();
    switch (selectedVal) {
        case 'Radio':
        case 'Check':
            $.get('/Vehicle/GetAddInspectionRecordFieldOptionsPartialView', function (data) {
                $(e).closest('[data-type="field"]').find('[data-type="fieldOptions"]').html(data);
            });
            $(e).closest('[data-type="field"]').find('[data-type="fieldActionItem"]').show();
            break;
        case 'Text':
            $(e).closest('[data-type="field"]').find('[data-type="fieldOptions"]').html("");
            $(e).closest('[data-type="field"]').find('[data-type="fieldActionItem"]').hide();
            break;
    }
}
function handleInspectionRecordFieldHasActionItemChange(e) {
    if ($(e).is(":checked")) {
        $(e).closest('[data-type="field"]').find('[data-type="fieldActionItemContainer"]').collapse('show');
    } else {
        $(e).closest('[data-type="field"]').find('[data-type="fieldActionItemContainer"]').collapse('hide');
    }
}
function addInspectionRecordFieldOption(e) {
    $.get('/Vehicle/GetAddInspectionRecordFieldOptionPartialView', function (data) {
        $(e).closest('[data-type="field"]').find('[data-type="fieldOptions"]').append(data);
    });
}
function deleteInspectionRecordFieldOption(e) {
    $(e).closest('[data-type="fieldOption"]').remove();
}
function getAndValidateInspectionRecordTemplate() {
    let hasError = false;
    let inspectionDescription = $("#inspectionRecordDescription").val();
    if (inspectionDescription.trim() == '') {
        hasError = true;
        $("#inspectionRecordDescription").addClass("is-invalid");
    } else {
        $("#inspectionRecordDescription").removeClass("is-invalid");
    }
    let inspectionTags = $("#inspectionRecordTemplateTag").val();
    let inspectionRecordId = getInspectionRecordModelData().id;
    let vehicleId = GetVehicleId().vehicleId;
    let inspectionRecordTemplateData = {
        id: inspectionRecordId,
        vehicleId: vehicleId,
        description: inspectionDescription,
        tags: inspectionTags,
        reminderRecordId: recurringReminderRecordId
    }
    let templateFields = [];
    //process fields
    $('#inspectionRecordFields > [data-type="field"]').map((index, elem) => {
        let fieldElem = $(elem);
        let hasActionItem = fieldElem.find('[data-type="fieldHasActionItem"]').is(":checked");
        let fieldType = fieldElem.find('[data-type="fieldType"]').val();
        let fieldDescriptionElem = fieldElem.find('[data-type="fieldDescription"]');
        if (fieldDescriptionElem.val().trim() == '') {
            hasError = true;
            fieldDescriptionElem.addClass('is-invalid');
        } else {
            fieldDescriptionElem.removeClass('is-invalid');
        }
        let fieldData = {
            description: fieldDescriptionElem.val(),
            fieldType: fieldType,
            hasNotes: fieldElem.find('[data-type="fieldHasNotes"]').is(":checked"),
            hasActionItem: hasActionItem
        };
        if (hasActionItem) {
            fieldData["actionItemType"] = fieldElem.find('[data-type="fieldActionItemType"]').val();
            fieldData["actionItemPriority"] = fieldElem.find('[data-type="fieldActionItemPriority"]').val();
            let actionItemDescriptionElem = fieldElem.find('[data-type="fieldActionItemDescription"]');
            fieldData["actionItemDescription"] = actionItemDescriptionElem.val();
            if (actionItemDescriptionElem.val().trim() == '') {
                hasError = true;
                actionItemDescriptionElem.addClass('is-invalid');
            } else {
                actionItemDescriptionElem.removeClass('is-invalid');
            }
        }
        if (fieldType != 'Text') {
            let fieldOptions = [];
            fieldElem.find('[data-type="fieldOptions"]').find('[data-type="fieldOption"]').map((optionIndex, optionElem) => {
                let fieldOptionElem = $(optionElem);
                let fieldOptionTextElem = fieldOptionElem.find('[data-type="fieldOptionText"]');
                if (fieldOptionTextElem.val().trim() == '') {
                    hasError = true;
                    fieldOptionTextElem.addClass('is-invalid');
                } else {
                    fieldOptionTextElem.removeClass('is-invalid');
                }
                fieldOptions.push({
                    description: fieldOptionTextElem.val(),
                    isFail: fieldOptionElem.find('[data-type="fieldOptionIsFail"]').is(":checked")
                });
            });
            fieldData["options"] = fieldOptions;
            if (fieldOptions.length == 0) {
                //user has not supplied any options
                fieldElem.find('[data-type="fieldType"]').addClass('is-invalid');
                hasError = true;
            } else {
                fieldElem.find('[data-type="fieldType"]').removeClass('is-invalid');
            }
        }
        else {
            fieldElem.find('[data-type="fieldType"]').removeClass('is-invalid');
        }
        templateFields.push(fieldData);
    });
    inspectionRecordTemplateData["fields"] = templateFields;
    inspectionRecordTemplateData["hasError"] = hasError;
    return inspectionRecordTemplateData;
}
function saveInspectionRecordTemplateToVehicle(isEdit) {
    let formValues = getAndValidateInspectionRecordTemplate();
    if (formValues.hasError) {
        errorToast("Please check the form data");
        return;
    }
    $.post('/Vehicle/SaveInspectionRecordTemplateToVehicleId', { inspectionRecordTemplate: formValues }, function (data) {
        if (data.success) {
            successToast(isEdit ? "Inspection Record Template Updated" : "Inspection Record Template Added.");
            hideInspectionRecordTemplateModal();
        } else {
            errorToast(data.message);
        }
    })
}
function deleteInspectionRecordTemplate(inspectionRecordTemplateId) {
    $("#workAroundInput").show();
    confirmDelete("Deleted Inspection Templates cannot be restored.", (result) => {
        if (result.isConfirmed) {
            $.post(`/Vehicle/DeleteInspectionRecordTemplateById?inspectionRecordTemplateId=${inspectionRecordTemplateId}`, function (data) {
                $("#workAroundInput").hide();
                if (data.success) {
                    successToast("Inspection Template Deleted");
                    hideInspectionRecordTemplateModal();
                } else {
                    errorToast(data.message);
                    $("#workAroundInput").hide();
                }
            });
        } else {
            $("#workAroundInput").hide();
        }
    });
}
function useInspectionRecordTemplate(inspectionRecordTemplateId) {
    $.get(`/Vehicle/GetAddInspectionRecordPartialView?inspectionRecordTemplateId=${inspectionRecordTemplateId}`, function (data) {
        if (isOperationResponse(data)) {
            return;
        }
        else if (data) {
            $("#inspectionRecordModalContent").html(data);
            hideInspectionRecordTemplateSelectorModal();
            //initiate datepicker
            initDatePicker($('#inspectionRecordDate'));
            initTagSelector($("#inspectionRecordTag"));
            $("#inspectionRecordModal").modal('show');
            clearModalContentOnHide($("#inspectionRecordModal"));
        } else {
            errorToast(genericErrorMessage());
        }
    });
}
function hideAddInspectionRecordModal(showSelector) {
    $("#inspectionRecordModalContent").html('');
    $("#inspectionRecordModal").modal('hide');
    if (showSelector) {
        showInspectionRecordTemplateSelectorModal();
    }
}
function getAndValidateInspectionRecord() {
    let hasError = false;
    let inspectionDescription = $("#inspectionRecordDescription").val();
    let inspectionDate = $("#inspectionRecordDate").val();
    let inspectionMileage = $("#inspectionRecordMileage").val();
    let inspectionCost = $("#inspectionRecordCost").val();
    let inspectionTags = $("#inspectionRecordTag").val();
    let inspectionRecordId = 0;
    var addReminderRecord = $("#addReminderCheck").is(":checked");
    let vehicleId = GetVehicleId().vehicleId;
    //Odometer Adjustments
    if (isNaN(inspectionMileage) && GetVehicleId().odometerOptional) {
        inspectionMileage = '0';
    }
    inspectionMileage = GetAdjustedOdometer(inspectionRecordId, inspectionMileage);
    //validations
    if (inspectionDescription.trim() == '') {
        hasError = true;
        $("#inspectionRecordDescription").addClass("is-invalid");
    } else {
        $("#inspectionRecordDescription").removeClass("is-invalid");
    }
    if (inspectionDate.trim() == '') { //eliminates whitespace.
        hasError = true;
        $("#inspectionRecordDate").addClass("is-invalid");
    } else {
        $("#inspectionRecordDate").removeClass("is-invalid");
    }
    if (inspectionMileage.trim() == '' || isNaN(inspectionMileage) || parseInt(inspectionMileage) < 0) {
        hasError = true;
        $("#inspectionRecordMileage").addClass("is-invalid");
    } else {
        $("#inspectionRecordMileage").removeClass("is-invalid");
    }
    if (inspectionCost.trim() == '' || !isValidMoney(inspectionCost)) {
        hasError = true;
        $("#inspectionRecordCost").addClass("is-invalid");
    } else {
        $("#inspectionRecordCost").removeClass("is-invalid");
    }
    let inspectionRecordData = {
        id: inspectionRecordId,
        vehicleId: vehicleId,
        date: inspectionDate,
        mileage: inspectionMileage,
        cost: inspectionCost,
        description: inspectionDescription,
        tags: inspectionTags,
        reminderRecordId: recurringReminderRecordId,
        files: uploadedFiles,
        addReminderRecord: addReminderRecord
    }
    let recordFields = [];
    //process fields
    $('#inspectionRecordFields > [data-type="field"]').map((index, elem) => {
        let fieldElem = $(elem);
        let hasActionItem = fieldElem.find('[data-type="fieldActionItemContainer"]').length > 0;
        let hasNotes = fieldElem.find('[data-type="fieldNotes"]').length > 0;
        let fieldType = fieldElem.attr('data-fieldtype');
        let fieldData = {
            description: fieldElem.find('[data-type="fieldDescription"]').text(),
            fieldType: fieldType,
            hasNotes: hasNotes,
            hasActionItem: hasActionItem
        };
        if (hasActionItem) {
            fieldData["actionItemDescription"] = fieldElem.find('[data-type="fieldActionItemDescription"]').val();
            fieldData["actionItemType"] = fieldElem.find('[data-type="fieldActionItemType"]').val();
            fieldData["actionItemPriority"] = fieldElem.find('[data-type="fieldActionItemPriority"]').val();
        }
        if (hasNotes) {
            let fieldNoteElem = fieldElem.find('[data-type="fieldNotes"]');
            fieldData["notes"] = fieldNoteElem.val();
        }
        if (fieldType != 'Text') {
            let fieldOptions = [];
            fieldElem.find('[data-type="fieldOptions"]').find('[data-type="fieldOption"]').map((optionIndex, optionElem) => {
                let fieldOptionElem = $(optionElem);
                fieldOptions.push({
                    description: fieldOptionElem.closest('[data-type="fieldOptionContainer"]').find('[data-type="fieldOptionText"]').text(),
                    isSelected: fieldOptionElem.is(":checked"),
                    isFail: fieldOptionElem.attr('data-field') == 'fail'
                });
            });
            fieldData["options"] = fieldOptions;
            //user must select at least one option for radio fields
            if (fieldType == 'Radio' && fieldOptions.filter(x=>x.isSelected).length == 0) {
                fieldElem.find('[data-type="fieldOptions"]').find('[data-type="fieldOption"]').addClass('is-invalid');
                hasError = true;
            } else {
                fieldElem.find('[data-type="fieldOptions"]').find('[data-type="fieldOption"]').removeClass('is-invalid');
            }
        } else {
            //handle text field
            let fieldOptions = [];
            let fieldTextOptionElem = fieldElem.find('[data-type="fieldOptions"]').find('[data-type="fieldOption"]');
            if (fieldTextOptionElem.val().trim() == '') {
                hasError = true;
                fieldTextOptionElem.addClass('is-invalid');
            } else {
                fieldTextOptionElem.removeClass('is-invalid');
            }
            fieldOptions.push({
                description: fieldTextOptionElem.val(),
                isSelected: true,
                isFail: false
            });
            fieldData["options"] = fieldOptions;
        }
        recordFields.push(fieldData);
    });
    inspectionRecordData["fields"] = recordFields;
    inspectionRecordData["hasError"] = hasError;
    return inspectionRecordData;
}
function saveinspectionRecordToVehicle() {
    //get values
    var formValues = getAndValidateInspectionRecord();
    //validate
    if (formValues.hasError) {
        errorToast("Please check the form data");
        return;
    }
    $.post('/Vehicle/SaveInspectionRecordToVehicleId', { inspectionRecord: formValues }, function (data) {
        if (data.success) {
            successToast("Inspection Record Added.");
            hideAddInspectionRecordModal();
            saveScrollPosition();
            getVehicleInspectionRecords(formValues.vehicleId);
            if (formValues.addReminderRecord) {
                setTimeout(function () { showAddReminderModal(formValues); }, 500);
            }
        } else {
            errorToast(data.message);
        }
    })
}
function updateInspectionRecord(recordId) {
    let inspectionTags = $("#inspectionRecordTag").val();
    let inspectionRecord = {
        id: recordId,
        files: uploadedFiles,
        tags: inspectionTags
    }
    let vehicleId = GetVehicleId().vehicleId;
    $.post('/Vehicle/UpdateInspectionRecord', { inspectionRecord: inspectionRecord }, function (data) {
        if (data.success) {
            successToast("Inspection Record Updated.");
            hideAddInspectionRecordModal();
            saveScrollPosition();
            getVehicleInspectionRecords(vehicleId);
        } else {
            errorToast(data.message);
        }
    })
}
function showEditInspectionRecordModal(inspectionRecordId) {
    $.get(`/Vehicle/GetViewInspectionRecordPartialView?inspectionRecordId=${inspectionRecordId}`, function (data) {
        if (isOperationResponse(data)) {
            return;
        }
        else if (data) {
            $("#inspectionRecordModalContent").html(data);
            //initiate tag selector
            initTagSelector($("#inspectionRecordTag"));
            $("#inspectionRecordModal").modal('show');
            clearModalContentOnHide($("#inspectionRecordModal"));
        } else {
            errorToast(genericErrorMessage());
        }
    });
}
function deleteInspectionRecord(inspectionRecordId) {
    $("#workAroundInput").show();
    confirmDelete("Deleted Inspection Records cannot be restored.", (result) => {
        if (result.isConfirmed) {
            $.post(`/Vehicle/DeleteInspectionRecordById?inspectionRecordId=${inspectionRecordId}`, function (data) {
                if (data.success) {
                    hideAddInspectionRecordModal();
                    successToast("Inspection Record Deleted");
                    var vehicleId = GetVehicleId().vehicleId;
                    getVehicleInspectionRecords(vehicleId);
                } else {
                    errorToast(data.message);
                    $("#workAroundInput").hide();
                }
            });
        } else {
            $("#workAroundInput").hide();
        }
    });
}
function moveInspectionRecordField(e, isDown) {
    let currentField = $(e).closest('[data-type="field"]');
    if (isDown) {
        let nextField = currentField.next('[data-type="field"]');
        if (nextField.length != 0) {
            currentField.insertAfter(nextField);
        }
    } else {
        let prevField = currentField.prev('[data-type="field"]');
        if (prevField.length != 0) {
            currentField.insertBefore(prevField);
        }
    }
}
function duplicateInspectionRecordTemplateToVehicle() {
    let inspectionRecordsIds = [];
    inspectionRecordsIds.push(getInspectionRecordModelData().id);
    duplicateRecordsToOtherVehicles(inspectionRecordsIds, 'InspectionRecord');
}
function duplicateInspectionRecordTemplate() {
    let inspectionRecordsIds = [];
    inspectionRecordsIds.push(getInspectionRecordModelData().id);
    duplicateRecords(inspectionRecordsIds, 'InspectionRecord');
}