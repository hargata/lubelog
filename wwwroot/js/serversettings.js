function loadSetupPage(pageNumber) {
    let pageElem = $(`.setup-wizard-content[data-page="${pageNumber}"]`);
    if (pageElem.length > 0) {
        $('.setup-wizard-content').hide();
        pageElem.show();
    }
    determineSetupButtons();
}
function determineSetupButtons() {
    let currentVisiblePage = $(".setup-wizard-content:visible").attr('data-page');
    switch (currentVisiblePage) {
        case '0':
        case '7':
            $(".setup-wizard-nav").hide();
            break;
        case '1':
        case '2':
        case '3':
        case '4':
        case '5':
            $(".setup-wizard-nav").show();
            $(".btn-prev").show();
            $(".btn-next").show();
            $(".btn-save").show();
            break;
        case '6':
            $(".setup-wizard-nav").show();
            $(".btn-prev").show();
            $(".btn-next").hide();
            $(".btn-save").show();
            break;
    }
}
function lastSetupPage() {
    let pageArray = $(".setup-wizard-content").map((index, elem) => parseInt($(elem).attr('data-page'))).toArray();
    let maxPage = Math.max(...pageArray);
    loadSetupPage(maxPage);
}
function nextSetupPage() {
    let currentVisiblePage = $(".setup-wizard-content:visible").attr('data-page');
    let nextPage = parseInt(currentVisiblePage) + 1;
    loadSetupPage(nextPage);
}
function previousSetupPage() {
    let currentVisiblePage = $(".setup-wizard-content:visible").attr('data-page');
    let prevPage = parseInt(currentVisiblePage) - 1;
    loadSetupPage(prevPage);
}
function loadLocaleSample() {
    let selectedLocale = $("#inputLocale").val();
    let selectedDateTimeLocale = $("#inputDateTimeLocale").val();
    if (selectedLocale.trim() == '') {
        $("#localeSampleContainer").hide();
        $("#dateTimeLocaleOverride").hide();
    } else {
        $("#dateTimeLocaleOverride").show();
        $.get(`/Home/GetLocaleSample?locale=${selectedLocale}&dtlocale=${selectedDateTimeLocale}`, function (data) {
            $("#localeSampleContainer").html(data);
            $("#localeSampleContainer").show();
        })
    }
}
function updateRangeLabel(e) {
    let labelElem = $(e).closest('.form-group').find('.rangeLabel');
    labelElem.text($(e).val());
}
function saveSetup() {
    let setupData = {
        LocaleOverride: $("#inputLocale").val(),
        LocaleDateTimeOverride: $("#inputDateTimeLocale").val(),
        PostgresConnection: $("#inputPostgres").val(),
        AllowedFileExtensions: $("#inputFileExt").val(),
        CustomLogoURL: $("#inputLogoURL").val(),
        CustomSmallLogoURL: $("#inputSmallLogoURL").val(),
        MessageOfTheDay: $("#inputMOTD").val(),
        WebHookURL: $("#inputWebHook").val(),
        ServerURL: $("#inputDomain").val(),
        CustomWidgetsEnabled: $("#inputCustomWidget").val(),
        InvariantAPIEnabled: $("#inputInvariantAPI").val(),
        WebSocketEnabled: $("#inputWebSocket").val(),
        CookieLifeSpan: $("#inputCookieLifeSpan").val(),
        SMTPConfig: {
            EmailServer: $("#inputSMTPServer").val(),
            EmailFrom: $("#inputSMTPFrom").val(),
            Port: $("#inputSMTPPort").val(),
            Username: $("#inputSMTPUsername").val(),
            Password: $("#inputSMTPPassword").val()
        },
        OIDCConfig: {
            Name: $("#inputOIDCProvider").val(),
            ClientId: $("#inputOIDCClient").val(),
            ClientSecret: $("#inputOIDCSecret").val(),
            AuthURL: $("#inputOIDCAuth").val(),
            TokenURL: $("#inputOIDCToken").val(),
            RedirectURL: $("#inputOIDCRedirect").val(),
            Scope: $("#inputOIDCScope").val(),
            ValidateState: $("#inputOIDCState").val(),
            DisableRegularLogin: $("#inputOIDCDisable").val(),
            UsePKCE: $("#inputOIDCPKCE").val(),
            LogOutURL: $("#inputOIDCLogout").val(),
            UserInfoURL: $("#inputOIDCUserInfo").val(),
            JwksURL: $("#inputOIDCJwks").val(),
            DisableRegistration: $("#inputOIDCRegistrationMode").val() == '1',
            AutoGenerateTokens: $("#inputOIDCRegistrationMode").val() == '2'
        },
        ReminderUrgencyConfig: {
            UrgentDays: $("#inputUrgentDays").val(),
            VeryUrgentDays: $("#inputVeryUrgentDays").val(),
            UrgentDistance: $("#inputUrgentDistance").val(),
            VeryUrgentDistance: $("#inputVeryUrgentDistance").val()
        },
        DefaultReminderEmail: $("#inputDefaultReminderEmail").val(),
        EnableRootUserOIDC: $("#inputOIDCRootUser").val(),
        KestrelAppConfig: {
            Endpoints: {
                Http: {
                    Url: $("#inputHTTPURL").val()
                },
                HttpsInlineCertFile: {
                    Url: $("#inputHTTPSURL").val(),
                    Certificate: {
                        Path: $("#inputHTTPSCertLocation").val(),
                        Password: $("#inputHTTPSCertPassword").val()
                    }
                }
            }
        },
        EnableAutomatedEvents: $("#inputAutomatedEvents").val(),
        NotificationConfig: null,
        SkippedSettings: []
    };
    let registrationMode = $("#inputRegistrationMode");
    if (registrationMode.length > 0) {
        switch (registrationMode.val()) {
            case '0':
                setupData["DisableRegistration"] = 'false';
                setupData["OpenRegistration"] = 'false'
                break;
            case '1':
                setupData["DisableRegistration"] = 'true';
                setupData["OpenRegistration"] = 'false'
                break;
            case '2':
                setupData["DisableRegistration"] = 'false';
                setupData["OpenRegistration"] = 'true'
                break;
        }
    }
    //nullify skipped settings
    if ($("#skipSMTP").is(":checked")) {
        setupData["SMTPConfig"] = null;
        setupData.SkippedSettings.push('SMTP');
    }
    if ($("#skipOIDC").is(":checked")) {
        setupData["OIDCConfig"] = null;
        setupData.SkippedSettings.push('OIDC');
    }
    if ($("#skipPostgres").is(":checked")) {
        setupData["PostgresConnection"] = null;
        setupData.SkippedSettings.push('Postgres');
    }
    if ($("#skipHTTPS").is(":checked")) {
        setupData["KestrelAppConfig"] = null;
        setupData.SkippedSettings.push('HTTPS');
    }
    let rootUserOIDC = $("#inputOIDCRootUser");
    if (rootUserOIDC.length > 0) {
        setupData["EnableRootUserOIDC"] = $("#inputOIDCRootUser").val();
    }
    //notification config
    if (setupData["EnableAutomatedEvents"] == 'true') {
        let [hourToCheck, minuteToCheck] = $("#inputAutomatedEventsTime").val().split(':');
        let notificationConfig = {
            HourToCheck: hourToCheck,
            MinuteToCheck: minuteToCheck,
            DaysToCache: $("#inputDaysToCache").val(),
            UseEmailNotification: $("#inputUseEmailNotification").val(),
            UrgenciesTracked: [],
            AutomatedEvents: [],
            ServiceConfigs: []
        }
        $("#inputUrgenciesTracked :checked").map(function () {
            notificationConfig.UrgenciesTracked.push(this.value);
        });
        $("#inputAutomatedEventsList :checked").map(function () {
            notificationConfig.AutomatedEvents.push(this.value);
        });
        $("#inputNotificationServiceConfig .serviceConfig").map((index, elem) => {
            let headerValues = $(elem).find('.serviceConfigHeaders').val();
            if (headerValues.trim() == '') {
                headerValues = '{}';
            }
            let serviceConfig = {
                Url: $(elem).find('.serviceConfigUrl').val(),
                ContentType: $(elem).find('.serviceConfigContentType').val(),
                PriorityMapping: {
                    NotUrgent: $(elem).find('.serviceConfigNotUrgentPriority').val(),
                    Urgent: $(elem).find('.serviceConfigUrgentPriority').val(),
                    VeryUrgent: $(elem).find('.serviceConfigVeryUrgentPriority').val(),
                    PastDue: $(elem).find('.serviceConfigPastDuePriority').val()
                },
                Headers: JSON.parse(headerValues),
                Body: $(elem).find('.serviceConfigBody').val()
            };
            notificationConfig.ServiceConfigs.push(serviceConfig);
        });
        setupData["NotificationConfig"] = notificationConfig;
    }
    $.post('/Home/WriteServerConfiguration', { serverConfig: setupData }, function (data) {
        if (data) {
            lastSetupPage();
        } else {
            errorToast(genericErrorMessage());
        }
    })
}
function addNewNotificationServiceConfig() {
    $.get('/Home/GetNotificationServiceConfigPartialView', function (data) {
        $('#inputNotificationServiceConfig').append(data);
    });
}
function deleteNotificationServiceConfig(e) {
    $(e).closest('.serviceConfig').remove();
}
function sendTestNotification(e) {
    let serviceConfigElem = $(e).closest('.serviceConfig');
    let headerValues = serviceConfigElem.find('.serviceConfigHeaders').val();
    if (headerValues.trim() == '') {
        headerValues = '{}';
    }
    let serviceConfigToTest = {
        Url: serviceConfigElem.find('.serviceConfigUrl').val(),
        ContentType: serviceConfigElem.find('.serviceConfigContentType').val(),
        PriorityMapping: {
            NotUrgent: serviceConfigElem.find('.serviceConfigNotUrgentPriority').val(),
            Urgent: serviceConfigElem.find('.serviceConfigUrgentPriority').val(),
            VeryUrgent: serviceConfigElem.find('.serviceConfigVeryUrgentPriority').val(),
            PastDue: serviceConfigElem.find('.serviceConfigPastDuePriority').val()
        },
        Headers: JSON.parse(headerValues),
        Body: serviceConfigElem.find('.serviceConfigBody').val()
    }
    $.post('/Home/SendTestNotification', { serviceConfig: serviceConfigToTest }, function (data) {
        if (data.success) {
            successToast(data.message);
        } else {
            errorToast(data.message);
        }
    })
}
function sendTestEmail() {
    let mailConfig = {
        EmailServer: $("#inputSMTPServer").val(),
        EmailFrom: $("#inputSMTPFrom").val(),
        Port: $("#inputSMTPPort").val(),
        Username: $("#inputSMTPUsername").val(),
        Password: $("#inputSMTPPassword").val()
    }
    Swal.fire({
        title: 'Send Test Email',
        html: `
                                        <input type="text" id="testEmailRecipient" class="swal2-input" placeholder="Email Address" onkeydown="handleSwalEnter(event)">
                                        `,
        confirmButtonText: 'Send',
        focusConfirm: false,
        preConfirm: () => {
            const emailRecipient = $("#testEmailRecipient").val();
            if (!emailRecipient || emailRecipient.trim() == '') {
                Swal.showValidationMessage(`Please enter a valid email address`);
            }
            return { emailRecipient }
        },
    }).then(function (result) {
        if (result.isConfirmed) {
            $.post('/Home/SendTestEmail', { emailAddress: result.value.emailRecipient, mailConfig: mailConfig }, function (data) {
                if (data.success) {
                    successToast(data.message);
                } else {
                    errorToast(data.message);
                }
            });
        }
    });
}
function importOpenIDConfig() {
    Swal.fire({
        title: 'Import OpenID Config',
        html: `
                                        <input type="text" id="openIdImportEndpoint" class="swal2-input" placeholder=".well-known endpoint" onkeydown="handleSwalEnter(event)">
                                        `,
        confirmButtonText: 'Import',
        focusConfirm: false,
        preConfirm: () => {
            const importEndpoint = $("#openIdImportEndpoint").val();
            if (!importEndpoint || importEndpoint.trim() == '') {
                Swal.showValidationMessage(`Please enter a valid URL`);
            }
            return { importEndpoint }
        },
    }).then(function (result) {
        if (result.isConfirmed) {
            $.post('/Home/ImportOpenIDConfiguration', { configUrl: result.value.importEndpoint }, function (data) {
                if (data != null && data != undefined) {
                    if (data.authorization_endpoint != null) {
                        $('#inputOIDCAuth').val(data.authorization_endpoint);
                    }
                    if (data.token_endpoint != null) {
                        $('#inputOIDCToken').val(data.token_endpoint);
                    }
                    if (data.userinfo_endpoint != null) {
                        $('#inputOIDCUserInfo').val(data.userinfo_endpoint);
                    }
                    if (data.jwks_uri != null) {
                        $('#inputOIDCJwks').val(data.jwks_uri);
                    }
                    if (data.end_session_endpoint != null) {
                        $('#inputOIDCLogout').val(data.end_session_endpoint);
                    }
                }
                else {
                    errorToast(genericErrorMessage());
                }
            });
        }
    });
}
function nextOnSkip(sender) {
    if ($(sender).is(":checked")) {
        nextSetupPage();
    }
}