const sloader = {
    show: function () {
        var sLoaderElement = `<div class='sloader'><div class='loader'></div></div>`
        $("body").append(sLoaderElement);
    },
    hide: function () {
        $(".sloader").remove();
    }

}