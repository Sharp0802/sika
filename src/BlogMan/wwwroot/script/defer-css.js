
'use strict';
document.querySelectorAll("link[rel=defer]").forEach((defer) => {
    defer.setAttribute("rel", "stylesheet");
})
