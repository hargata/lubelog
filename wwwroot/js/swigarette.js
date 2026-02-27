function toggleAPICollapse(sender) {
    let headerElem = $(sender);
    let collapseElem = headerElem.closest('.apiContainer').children('.collapse');
    if ($(sender).hasClass('collapsed')) {
        collapseElem.collapse('show');
        $(sender).removeClass('collapsed');
    } else {
        collapseElem.collapse('hide');
        $(sender).addClass('collapsed');
    }
}
function testAPIEndpoint(sender) {
    let apiTester = $(sender).closest('.collapse').children('.api-tester');
    apiTester.toggleClass('d-none');
}
function executeAPIEndpoint(sender) {
    let apiPath = $(sender).attr('data-endpoint');
    let apiMethodType = $(sender).attr('data-method');
    let apiData = {};
    let hasError = false;
    let isFileUpload = false;
    //find result box
    let apiResult = $(sender).closest('.form-group').children('.api-tester-result');
    apiResult.children('.api-tester-result-text').val('');
    try {
        //find body
        let apiBodyElem = $(sender).closest('.form-group').children('.api-tester-body');
        if (apiBodyElem.length > 0) {
            if (apiBodyElem.attr('data-file') == "false") {
                if (apiBodyElem.val().trim() == '') {
                    hasError = true;
                    apiBodyElem.addClass('is-invalid');
                }
                else {
                    apiBodyElem.removeClass('is-invalid');
                    apiData[apiBodyElem.attr('data-param')] = JSON.parse(apiBodyElem.val());
                }
            }
            else {
                isFileUpload = true;
                let formData = new FormData();
                let files = apiBodyElem[0].files;
                if (files.length == 0) {
                    hasError = true;
                    apiBodyElem.addClass('is-invalid');
                } else {
                    apiBodyElem.removeClass('is-invalid');
                    for (var x = 0; x < files.length; x++) {
                        formData.append(apiBodyElem.attr('data-param'), files[x]);
                    }
                    apiData = formData;
                }
            }
        }
        //find query params
        let apiQueryElems = $(sender).closest('.form-group').find('.api-tester-param');
        if (apiQueryElems.length > 0) {
            apiQueryElems.map((index, elem) => {
                if ($(elem).attr('data-required') == 'true' && $(elem).val().trim() == '') {
                    $(elem).addClass('is-invalid');
                    hasError = true;
                } else {
                    $(elem).removeClass('is-invalid');
                    apiData[$(elem).attr('data-param')] = $(elem).val();
                }
            })
        }
    } catch (error) {
        apiResult.removeClass('d-none');
        apiResult.children('.api-tester-result-text').val(error.message);
        hasError = true;
    }
    
    if (hasError) {
        return;
    }
    let currentParams = new URLSearchParams(window.location.search);
    let apiKey = currentParams.get('apiKey');
    if (apiKey != null) {
        apiPath = `${apiPath}?apiKey=${apiKey}`;
    }
    let ajaxConfig = {
        url: apiPath,
        type: apiMethodType,
        data: apiData,
        success: function (response) {
            apiResult.removeClass('d-none');
            apiResult.children('.api-tester-result-text').val(JSON.stringify(response, null, 2));
        },
        error: function (xhr, status, error) {
            apiResult.removeClass('d-none');
            apiResult.children('.api-tester-result-text').val(JSON.stringify(xhr.responseJSON, null, 2));
        }
    };
    if (isFileUpload) {
        ajaxConfig['processData'] = false;
        ajaxConfig['cache'] = false;
        ajaxConfig['contentType'] = false;
    }
    //execute AJAX
    $.ajax(ajaxConfig);
}
function copyAPIPath(sender) {
    let textToCopy = $(sender).attr('data-endpoint');
    navigator.clipboard.writeText(textToCopy);
    successToast("Copied to Clipboard");
}