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
function handlePasswordKeyPress(event) {
    if (event.keyCode == 13) {
        performLogin();
    }
}