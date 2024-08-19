// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.getElementById("btn_open").addEventListener("click", open_close_menu);
var side_menu = document.getElementById("menu_side");
var btn_open = document.getElementById("btn_open");
var body = document.getElementById("body");
function open_close_menu() {
    body.classList.toggle("body_move");
    side_menu.classList.toggle("menu__side_move");
}
if (window.innerWidth < 760) {

    body.classList.add("body_move");
    side_menu.classList.add("menu__side_move");
}



window.addEventListener("resize", function () {

    if (window.innerWidth > 760) {

        body.classList.remove("body_move");
        side_menu.classList.remove("menu__side_move");
    }

    if (window.innerWidth < 760) {

        body.classList.add("body_move");
        side_menu.classList.add("menu__side_move");
    }

});

