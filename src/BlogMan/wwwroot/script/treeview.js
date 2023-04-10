'use strict';

let elements = document.getElementsByClassName("caret");
for (let i = 0; i < elements.length; ++i) {
    let element = elements[i];
    element.addEventListener("click", () => {
        element.parentElement.querySelector(".nested").classList.toggle("active");
        element.classList.toggle("caret-down");
    })
}

let search = document.querySelector(".sidenav input[type=text]");
let icon = search.parentElement.querySelector("svg");
search.addEventListener("focus", () => {
    icon.style.fill = "var(--theme-primary-accent)";
})
search.addEventListener("focusout", () => {
    icon.style.fill = null;
})