// Copyright (c) Matthias Wolf, Mawosoft.

"use strict";

(function () {

    function unhide(elem) {
        if (!elem?.classList.contains('hidden')) return false;
        if (elem.dataset.source) {
            elem.innerHTML = document.getElementById(elem.dataset.source)?.innerHTML;
            delete elem.dataset.source;
        }
        elem.classList.remove('hidden');
        return true;
    }

    const buttonClick = function (ev) {
        const target = document.getElementById(this.dataset.for);
        if (unhide(target)) {
            if (this.parentElement?.tagName == 'SUMMARY') {
                const elem = this.parentElement.parentElement;
                if (elem?.tagName == 'DETAILS' && !elem.open) elem.open = true;
            }
        }
        else {
            target?.classList.add('hidden');
        }
    }

    document.querySelectorAll('button[data-for]').forEach((b) => b.addEventListener('click', buttonClick));

    window.addEventListener('pagehide', () => {
        window.sessionStorage.setItem(document.location.pathname, JSON.stringify({
            details: Array.from(document.querySelectorAll('details'), (d) => !!d.open),
            unhidden: Array.from(document.querySelectorAll('button[data-for]'), (b) => b.dataset.for).filter((id) => {
                const elem = document.getElementById(id);
                return !!elem && !elem.classList.contains('hidden');
            }),
        }));
    });

    const pageState = JSON.parse(window.sessionStorage.getItem(document.location.pathname));
    if (pageState) {
        const details = document.querySelectorAll('details');
        if (details.length === pageState.details.length) {
            for (let i = 0; i < details.length; i++) details[i].open = pageState.details[i];
        }
        pageState.unhidden.forEach((id) => unhide(document.getElementById(id)));
    }

})();
