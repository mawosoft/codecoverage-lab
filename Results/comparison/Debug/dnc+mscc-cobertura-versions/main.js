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

    function showInlineMetrics(show) {
        const tables = document.querySelectorAll('table[data-metrics]');
        const buttons = document.querySelectorAll('button[data-metrics]');
        if (show) {
            tables.forEach((t) => t.classList.remove('hidden'));
            buttons.forEach((b) => b.textContent = 'M');
        }
        else {
            tables.forEach((t) => t.classList.add('hidden'));
            buttons.forEach((b) => b.textContent = '+');
        }
    }

    const buttonClick = function (ev) {
        const target = document.getElementById(this.dataset.for);
        if (unhide(target)) {
            // details -> summary -> h2/h3 -> button
            let elem = this.parentElement?.parentElement;
            if (elem?.tagName === 'SUMMARY') {
                elem = elem.parentElement;
                if (elem?.tagName === 'DETAILS' && !elem.open) elem.open = true;
            }
        }
        else {
            target?.classList.add('hidden');
        }
    }

    const tdRangeMouseOver = function (ev) {
        const target = document.getElementById(this.dataset.for)?.firstChild;
        const startend = this.dataset.range.split(' ');
        const range = document.createRange();
        range.setStart(target, parseInt(startend[0]));
        range.setEnd(target, parseInt(startend[1]));
        CSS.highlights.set('line-range', new Highlight(range));
    }

    const tdRangeMouseOut = function (ev) {
        CSS.highlights.delete('line-range');
    }

    document.querySelectorAll('button[data-for]').forEach((b) => b.addEventListener('click', buttonClick));

    document.querySelectorAll('td[data-for]').forEach((b) => {
        b.addEventListener('mouseover', tdRangeMouseOver);
        b.addEventListener('mouseout', tdRangeMouseOut);
    });

    document.getElementById('chkInlineMetrics')?.addEventListener('click', function (ev) {
        showInlineMetrics(this.checked);
    })

    window.addEventListener('pagehide', () => {
        window.sessionStorage.setItem(document.location.pathname, JSON.stringify({
            details: Array.from(document.querySelectorAll('details'), (d) => !!d.open),
            unhidden: Array.from(document.querySelectorAll('button[data-for]'), (b) => b.dataset.for).filter((id) => {
                const elem = document.getElementById(id);
                return !!elem && !elem.classList.contains('hidden');
            }),
            inline: document.getElementById('chkInlineMetrics')?.checked,
        }));
    });

    const pageState = JSON.parse(window.sessionStorage.getItem(document.location.pathname));
    if (pageState) {
        const details = document.querySelectorAll('details');
        if (details.length === pageState.details.length) {
            for (let i = 0; i < details.length; i++) details[i].open = pageState.details[i];
        }
        pageState.unhidden.forEach((id) => unhide(document.getElementById(id)));
        const chkInlineMetrics = document.getElementById('chkInlineMetrics');
        if (chkInlineMetrics) {
            chkInlineMetrics.checked = !!pageState.inline;
            if (pageState.inline) showInlineMetrics(true);
        }
    }

})();
