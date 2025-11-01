function showInspectionRecordTemplateSelectorModal() {
    var vehicleId = GetVehicleId().vehicleId;
    $.get(`/Vehicle/GetInspectionRecordTemplatesByVehicleId?vehicleId=${vehicleId}`, function (data) {
        if (data) {
            $("#inspectionRecordTemplateModalContent").html(data);
            $('#inspectionRecordTemplateModal').modal('show');
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
            hideInspectionRecordTemplateSelectorModal();
            //initiate tag selector
            initTagSelector($("#inspectionRecordTemplateTag"));
            $('#inspectionRecordTemplateEditModal').modal('show');
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
    let clonedField = $(e).closest('[data-type="field"]').clone();
    $("#inspectionRecordFields").append(clonedField);
}
function handleInspectionRecordFieldTypeChange(e) {
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
        if (data) {
            successToast(isEdit ? "Inspection Record Template Updated" : "Inspection Record Template Added.");
            hideInspectionRecordTemplateModal();
            getVehicleCollisionRecords(formValues.vehicleId);
        } else {
            errorToast(genericErrorMessage());
        }
    })
}
function deleteInspectionRecordTemplate(inspectionRecordTemplateId) {
    $("#workAroundInput").show();
    Swal.fire({
        title: "Confirm Deletion?",
        text: "Deleted Inspection Templates cannot be restored.",
        showCancelButton: true,
        confirmButtonText: "Delete",
        confirmButtonColor: "#dc3545"
    }).then((result) => {
        if (result.isConfirmed) {
            $.post(`/Vehicle/DeleteInspectionRecordTemplateById?inspectionRecordTemplateId=${inspectionRecordTemplateId}`, function (data) {
                $("#workAroundInput").hide();
                if (data) {
                    successToast("Inspection Template Deleted");
                    hideInspectionRecordTemplateModal();
                } else {
                    errorToast(genericErrorMessage());
                }
            });
        } else {
            $("#workAroundInput").hide();
        }
    });
}
function useInspectionRecordTemplate(inspectionRecordTemplateId) {
    $.get(`/Vehicle/GetAddInspectionRecordPartialView?inspectionRecordTemplateId=${inspectionRecordTemplateId}`, function (data) {
        if (data) {
            $("#inspectionRecordModalContent").html(data);
            hideInspectionRecordTemplateSelectorModal();
            //initiate datepicker
            initDatePicker($('#inspectionRecordDate'));
            initTagSelector($("#inspectionRecordTag"));
            $("#inspectionRecordModal").modal('show');
        } else {
            errorToast(genericErrorMessage());
        }
    });
}
function hideAddInspectionRecordModal() {
    $("#inspectionRecordModal").modal('hide');
    showInspectionRecordTemplateSelectorModal();
}
//function showEditCollisionRecordModal(collisionRecordId, nocache) {
//    if (!nocache) {
//        var existingContent = $("#collisionRecordModalContent").html();
//        if (existingContent.trim() != '') {
//            //check if id is same.
//            var existingId = getCollisionRecordModelData().id;
//            if (existingId == collisionRecordId && $('[data-changed=true]').length > 0) {
//                $('#collisionRecordModal').modal('show');
//                $('.cached-banner').show();
//                return;
//            }
//        }
//    }
//    $.get(`/Vehicle/GetCollisionRecordForEditById?collisionRecordId=${collisionRecordId}`, function (data) {
//        if (data) {
//            $("#collisionRecordModalContent").html(data);
//            //initiate datepicker
//            initDatePicker($('#collisionRecordDate'));
//            initTagSelector($("#collisionRecordTag"));
//            $('#collisionRecordModal').modal('show');
//            bindModalInputChanges('collisionRecordModal');
//            $('#collisionRecordModal').off('shown.bs.modal').on('shown.bs.modal', function () {
//                if (getGlobalConfig().useMarkDown) {
//                    toggleMarkDownOverlay("collisionRecordNotes");
//                }
//            });
//        }
//    });
//}
//function hideAddCollisionRecordModal() {
//    $('#collisionRecordModal').modal('hide');
//}
//function deleteCollisionRecord(collisionRecordId) {
//    $("#workAroundInput").show();
//    Swal.fire({
//        title: "Confirm Deletion?",
//        text: "Deleted Repair Records cannot be restored.",
//        showCancelButton: true,
//        confirmButtonText: "Delete",
//        confirmButtonColor: "#dc3545"
//    }).then((result) => {
//        if (result.isConfirmed) {
//            $.post(`/Vehicle/DeleteCollisionRecordById?collisionRecordId=${collisionRecordId}`, function (data) {
//                if (data) {
//                    hideAddCollisionRecordModal();
//                    successToast("Repair Record Deleted");
//                    var vehicleId = GetVehicleId().vehicleId;
//                    getVehicleCollisionRecords(vehicleId);
//                } else {
//                    errorToast(genericErrorMessage());
//                }
//            });
//        } else {
//            $("#workAroundInput").hide();
//        }
//    });
//}
//function saveCollisionRecordToVehicle(isEdit) {
//    //get values
//    var formValues = getAndValidateCollisionRecordValues();
//    //validate
//    if (formValues.hasError) {
//        errorToast("Please check the form data");
//        return;
//    }
//    //save to db.
//    $.post('/Vehicle/SaveCollisionRecordToVehicleId', { collisionRecord: formValues }, function (data) {
//        if (data) {
//            successToast(isEdit ? "Repair Record Updated" : "Repair Record Added.");
//            hideAddCollisionRecordModal();
//            saveScrollPosition();
//            getVehicleCollisionRecords(formValues.vehicleId);
//            if (formValues.addReminderRecord) {
//                setTimeout(function () { showAddReminderModal(formValues); }, 500);
//            }
//        } else {
//            errorToast(genericErrorMessage());
//        }
//    })
//}
//function getAndValidateCollisionRecordValues() {
//    var collisionDate = $("#collisionRecordDate").val();
//    var collisionMileage = parseInt(globalParseFloat($("#collisionRecordMileage").val())).toString();
//    var collisionDescription = $("#collisionRecordDescription").val();
//    var collisionCost = $("#collisionRecordCost").val();
//    var collisionNotes = $("#collisionRecordNotes").val();
//    var collisionTags = $("#collisionRecordTag").val();
//    var vehicleId = GetVehicleId().vehicleId;
//    var collisionRecordId = getCollisionRecordModelData().id;
//    var addReminderRecord = $("#addReminderCheck").is(":checked");
//    //Odometer Adjustments
//    if (isNaN(collisionMileage) && GetVehicleId().odometerOptional) {
//        collisionMileage = '0';
//    }
//    collisionMileage = GetAdjustedOdometer(collisionRecordId, collisionMileage);
//    //validation
//    var hasError = false;
//    var extraFields = getAndValidateExtraFields();
//    if (extraFields.hasError) {
//        hasError = true;
//    }
//    if (collisionDate.trim() == '') { //eliminates whitespace.
//        hasError = true;
//        $("#collisionRecordDate").addClass("is-invalid");
//    } else {
//        $("#collisionRecordDate").removeClass("is-invalid");
//    }
//    if (collisionMileage.trim() == '' || isNaN(collisionMileage) || parseInt(collisionMileage) < 0) {
//        hasError = true;
//        $("#collisionRecordMileage").addClass("is-invalid");
//    } else {
//        $("#collisionRecordMileage").removeClass("is-invalid");
//    }
//    if (collisionDescription.trim() == '') {
//        hasError = true;
//        $("#collisionRecordDescription").addClass("is-invalid");
//    } else {
//        $("#collisionRecordDescription").removeClass("is-invalid");
//    }
//    if (collisionCost.trim() == '' || !isValidMoney(collisionCost)) {
//        hasError = true;
//        $("#collisionRecordCost").addClass("is-invalid");
//    } else {
//        $("#collisionRecordCost").removeClass("is-invalid");
//    }
//    return {
//        id: collisionRecordId,
//        hasError: hasError,
//        vehicleId: vehicleId,
//        date: collisionDate,
//        mileage: collisionMileage,
//        description: collisionDescription,
//        cost: collisionCost,
//        notes: collisionNotes,
//        files: uploadedFiles,
//        supplies: selectedSupplies,
//        tags: collisionTags,
//        addReminderRecord: addReminderRecord,
//        extraFields: extraFields.extraFields,
//        requisitionHistory: supplyUsageHistory,
//        deletedRequisitionHistory: deletedSupplyUsageHistory,
//        reminderRecordId: recurringReminderRecordId,
//        copySuppliesAttachment: copySuppliesAttachments
//    }
//}