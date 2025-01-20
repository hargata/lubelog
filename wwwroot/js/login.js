function performLogin() {
    var userName = $("#inputUserName").val();
    var userPassword = $("#inputUserPassword").val();
    var isPersistent = $("#inputPersistent").is(":checked");
    $.post('/Login/Login', {userName: userName, password: userPassword, isPersistent: isPersistent}, function (data) {
        if (data) {
            //check for redirectURL
            var redirectURL = getRedirectURL().url;
            if (redirectURL.trim() != "") {
                window.location.href = redirectURL;
            } else {
                window.location.href = '/Home';
            }
        } else {
            errorToast("Invalid Login Credentials, please try again.");
        }
    })
}
function performRegistration() {
    var token = $("#inputToken").val();
    var userName = $("#inputUserName").val();
    var userPassword = $("#inputUserPassword").val();
    var userEmail = $("#inputEmail").val();
    $.post('/Login/Register', { userName: userName, password: userPassword, token: token, emailAddress: userEmail }, function (data) {
        if (data.success) {
            successToast(data.message);
            setTimeout(function () { window.location.href = '/Login/Index' }, 500);
        } else {
            errorToast(data.message);
        }
    });
}
function requestPasswordReset() {
    var userName = $("#inputUserName").val();
    $.post('/Login/RequestResetPassword', { userName: userName }, function (data) {
        if (data.success) {
            successToast(data.message);
            setTimeout(function () { window.location.href = '/Login/Index' }, 500);
        } else {
            errorToast(data.message);
        }
    })
}
function performPasswordReset() {
    var token = $("#inputToken").val();
    var userPassword = $("#inputUserPassword").val();
    var userEmail = $("#inputEmail").val();
    $.post('/Login/PerformPasswordReset', { password: userPassword, token: token, emailAddress: userEmail }, function (data) {
        if (data.success) {
            successToast(data.message);
            setTimeout(function () { window.location.href = '/Login/Index' }, 500);
        } else {
            errorToast(data.message);
        }
    });
}

function handlePasswordKeyPress(event) {
    if (event.keyCode == 13) {
        performLogin();
    }
}

function remoteLogin() {
    $.get('/Login/GetRemoteLoginLink', function (data) {
        if (data) {
            window.location.href = data;
        }
    })
}
function sendRegistrationToken() {
    Swal.fire({
        title: 'Please Provide an Email Address',
        html: `
                            <input type="text" id="inputTokenEmail" class="swal2-input" placeholder="Email Address" onkeydown="handleSwalEnter(event)">
                            `,
        confirmButtonText: 'Send',
        focusConfirm: false,
        preConfirm: () => {
            const tokenEmail = $("#inputTokenEmail").val();
            if (!tokenEmail || tokenEmail.trim() == '') {
                Swal.showValidationMessage(`Please enter a valid email address`);
            }
            return { tokenEmail }
        },
    }).then(function (result) {
        if (result.isConfirmed) {
            $.post('/Login/SendRegistrationToken', { emailAddress: result.value.tokenEmail }, function (data) {
                if (data.success) {
                    successToast(data.message);
                } else {
                    errorToast(data.message);
                }
            });
        }
    });
}