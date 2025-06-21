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
        case '5':
            $(".setup-wizard-nav").hide();
            break;
        case '1':
        case '2':
        case '3':
            $(".setup-wizard-nav").show();
            $(".btn-prev").show();
            $(".btn-next").show();
            $(".btn-save").hide();
            break;
        case '4':
            $(".setup-wizard-nav").show();
            $(".btn-prev").show();
            $(".btn-next").hide();
            $(".btn-save").show();
            break;
    }
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
function saveSetup() {
    let setupData = {
        PostgresConnection: $("#inputPostgres").val(),
        AllowedFileExtensions: $("#inputFileExt").val(),
        CustomLogoURL: $("#inputLogoURL").val(),
        CustomSmallLogoURL: $("#inputSmallLogoURL").val(),
        MessageOfTheDay: $("#inputMOTD").val(),
        WebHookURL: $("#inputWebHook").val(),
        ServerURL: $("#inputDomain").val(),
        CustomWidgetsEnabled: $("#inputCustomWidget").val(),
        InvariantAPIEnabled: $("#inputInvariantAPI").val(),
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
            UserInfoURL: $("#inputOIDCUserInfo").val()
        },
        ReminderUrgencyConfig: {
            UrgentDays: $("#inputUrgentDays").val(),
            VeryUrgentDays: $("#inputVeryUrgentDays").val(),
            UrgentDistance: $("#inputUrgentDistance").val(),
            VeryUrgentDistance: $("#inputVeryUrgentDistance").val()
        },
        DefaultReminderEmail: $("#inputDefaultReminderEmail").val(),
        EnableRootUserOIDC: $("#inputOIDCRootUser").val()
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
    let rootUserOIDC = $("#inputOIDCRootUser");
    if (rootUserOIDC.length > 0) {
        setupData["EnableRootUserOIDC"] = $("#inputOIDCRootUser").val();
    }
    $.post('/Home/WriteServerConfiguration', { serverConfig: setupData }, function (data) {
        if (data) {
            nextSetupPage();
        } else {
            errorToast(genericErrorMessage());
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