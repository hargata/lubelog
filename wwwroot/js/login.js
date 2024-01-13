function performLogin() {
    var userName = $("#inputUserName").val();
    var userPassword = $("#inputUserPassword").val();
    var isPersistent = $("#inputPersistent").is(":checked");
    $.post('/Login/Login', {userName: userName, password: userPassword, isPersistent: isPersistent}, function (data) {
        if (data) {
            window.location.href = '/Home';
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
function handlePasswordKeyPress(event) {
    if (event.keyCode == 13) {
        performLogin();
    }
}