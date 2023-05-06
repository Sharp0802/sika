'use strict';

function GetDepth(element, n = 0) {
    if (element === undefined)
        return n;
    return GetDepth(element.parent, n + 1)
}

function OrderByDepth(a, b) {
    return GetDepth(a) - GetDepth(b);
}

function UpdateQuery() {
    let filter = search.value.toUpperCase().replace(/\s/g, "");

    let tree = document.getElementById("tree-view");
    for (const href of tree.getElementsByTagName("a"))
    {
        let parent = href.parentElement;
        parent.style.display = href.textContent.toUpperCase().replace(/\s/g, "").indexOf(filter) > -1 ? "" : "none";
    }

    for (const ul of tree.querySelectorAll("ul.nested")) {
        let necessary = false;
        for (const li of ul.children)
            if (li.style.display !== "none")
                necessary = true;
        ul.style.display = necessary ? "" : "none";
    }

    for (const lst of Array.from(tree.getElementsByTagName("li")).sort(OrderByDepth)) {
        let uls = lst.children;
        if (uls.length === 1)
            continue;
        
        let necessary = false;
        for (const ul of uls)
        {
            if (ul.tagName.toLowerCase() !== "ul")
                continue;
            if (ul.style.display !== "none") {
                necessary = true;
            }
        }
        lst.style.display = necessary ? "" : "none";
    }
}

let searchBox = document.getElementById("search-box");
searchBox.addEventListener("input", UpdateQuery);
