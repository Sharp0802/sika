'use strict';

let search = document.getElementById("search-box");
search.addEventListener("keyup", () => {
    let filter = search.value.toUpperCase();
    
    let tree = document.getElementById("tree-view");
    let hrefs = tree.querySelectorAll("a");
    
    for (const href in hrefs) {
        let parent = href.parentElement;
        parent.style.display = href.toUpperCase().indexOf(filter) > -1 ? "" : "none"
    }
})
