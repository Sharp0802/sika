'use strict';
document
    .querySelectorAll("link[rel=defer]")
    .forEach(async (defer) => {
        await new Promise(() => defer.setAttribute("rel", "stylesheet"));
    })