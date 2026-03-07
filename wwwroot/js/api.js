import { i18n } from './TranslationService.js';
// Fehlerbehandlung (ohne jQuery)
export function sendErrorMessage(error) {
    console.error("fetch-Error:", error);
    const createErrorSpan = document.querySelector('#createErrorSpan');
    if (createErrorSpan) {
        createErrorSpan.textContent = error.toString();
        createErrorSpan.style.color = 'red';
    }
}
// Collection Areas
export async function getCollectionAreaList() {
    try {
        const response = await fetch("/api/collections/listCollectionAreas");
        if (!response.ok)
            throw new Error(`HTTP ${response.status}`);
        const json = await response.json();
        const jsonString = JSON.stringify(json);
        sessionStorage.setItem('collectionAreasList', jsonString);
        return jsonString;
    }
    catch (error) {
        console.error("Error fetching collection areas:", error);
        return null;
    }
}
// Production Facilities
export async function getIndustryList() {
    try {
        const response = await fetch("/api/collections/listProductionFacilities");
        if (!response.ok)
            throw new Error(`HTTP ${response.status}`);
        const json = await response.json();
        const jsonString = JSON.stringify(json);
        sessionStorage.setItem('industryList', jsonString);
        return jsonString;
    }
    catch (error) {
        console.error("Error fetching industries:", error);
        return null;
    }
}
export function initializePlaceSearch() {
    const togglePlace = document.querySelector('.placeSearchSubmit');
    if (!togglePlace)
        return;
    togglePlace.addEventListener('click', () => {
        const inputElement = document.querySelector('.inputPlaceSearch');
        if (!inputElement)
            return;
        const toponymyName = inputElement.value.trim();
        fetch('/api/collections/listPlaces', {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ toponym: toponymyName }),
        })
            .then((res) => {
            if (!res.ok)
                throw new Error(res.statusText);
            return res.json();
        })
            .then((result) => {
            const table = document.getElementById('placeSearchResultTable');
            if (!table)
                return;
            const oldTbody = document.getElementById('placeSearchResultTableBody');
            oldTbody?.remove();
            const tbody = table.createTBody();
            tbody.id = 'placeSearchResultTableBody';
            if (result.length > 0) {
                result.forEach((element, idx) => {
                    tbody.appendChild(buildToponymySearchResultRow(element, idx));
                });
            }
            else {
                const tr = document.createElement('tr');
                tr.appendChild(createTd({
                    text: i18n.get("NothingFound"),
                    scope: 'row'
                }));
                tbody.appendChild(tr);
            }
        })
            .catch((err) => {
            sendErrorMessage(err);
        });
    });
}
export function buildToponymySearchResultRow(element, idx) {
    const tr = document.createElement('tr');
    tr.id = `placeSearchResult_${idx}`;
    tr.appendChild(createTd({
        text: element.placeID.toString(),
        id: `placeSearchResultPlaceID_${idx}`,
        scope: 'row'
    }));
    const tdToponym = document.createElement('td');
    tdToponym.id = `placeSearchResultToponym_${idx}`;
    tdToponym.innerHTML = element.toponymyDisplay || '';
    tr.appendChild(tdToponym);
    const tdFurtherSpecs = document.createElement('td');
    tdFurtherSpecs.id = `placeSearchResultFurtherSpecs_${idx}`;
    tdFurtherSpecs.textContent = element.furtherSpecs || '';
    tr.appendChild(tdFurtherSpecs);
    const tdAction = createActionCell(idx, ['Individual', 'Organization', 'CollectionItem'], 'Place_Add', 'addPlace');
    tr.appendChild(tdAction);
    return tr;
}
// Party Search
export function initializePartySearch() {
    const toggleParty = document.querySelector('.partySearchSubmit');
    if (!toggleParty)
        return;
    toggleParty.addEventListener('click', () => {
        const inputElement = document.querySelector('.inputPartySearch');
        const partyTypeElement = document.getElementById('partyType');
        if (!inputElement || !partyTypeElement)
            return;
        const partyName = inputElement.value.trim();
        const partyType = partyTypeElement.value;
        fetch('/api/collections/listParties', {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ name: partyName, type: partyType }),
        })
            .then((res) => {
            if (!res.ok)
                throw new Error(res.statusText);
            return res.json();
        })
            .then((result) => {
            const table = document.getElementById('partySearchResultTable');
            if (!table)
                return;
            const oldTbody = document.getElementById('partySearchResultTableBody');
            oldTbody?.remove();
            const tbody = table.createTBody();
            tbody.id = 'partySearchResultTableBody';
            if (result.length > 0) {
                result.forEach((element, idx) => {
                    tbody.appendChild(buildPartySearchResultRow(element, idx));
                });
            }
            else {
                const tr = document.createElement('tr');
                tr.appendChild(createTd({
                    text: i18n.get("NothingFound"),
                    scope: 'row'
                }));
                tbody.appendChild(tr);
            }
        })
            .catch((err) => {
            sendErrorMessage(err);
        });
    });
}
export function buildPartySearchResultRow(element, idx) {
    const tr = document.createElement('tr');
    tr.id = `partySearchResult_${idx}`;
    tr.appendChild(createTd({
        text: element.partyID.toString(),
        id: `partySearchResultPartyID_${idx}`,
        scope: 'row'
    }));
    const tdName = document.createElement('td');
    tdName.id = `partySearchResultName_${idx}`;
    tdName.textContent = element.name || '';
    tr.appendChild(tdName);
    tr.appendChild(createTd({
        text: element.type || '',
        id: `partySearchResultType_${idx}`,
        scope: 'row'
    }));
    const tdFurtherSpecs = document.createElement('td');
    tdFurtherSpecs.id = `partySearchResultFurtherSpecs_${idx}`;
    tdFurtherSpecs.textContent = element.furtherSpecs || '';
    tr.appendChild(tdFurtherSpecs);
    const tdAction = document.createElement('td');
    const btnAction = document.createElement('button');
    btnAction.type = 'button';
    btnAction.classList.add('btn', 'btn-primary', 'btn-sm');
    btnAction.textContent = i18n.get("Add");
    btnAction.onclick = () => window.addParty?.(idx);
    tdAction.appendChild(btnAction);
    tr.appendChild(tdAction);
    return tr;
}
// Helper-Funktion für Action Cells
function createActionCell(idx, urlPatterns, buttonTextKey, onClickFunction) {
    const tdAction = document.createElement('td');
    const divAction = document.createElement('div');
    divAction.className = 'btn-group';
    divAction.setAttribute('role', 'group');
    divAction.setAttribute('aria-label', "Aktion Div");
    //const url = window.location.href;
    //const shouldShow = urlPatterns.some(pattern => url.includes(pattern));
    //if (shouldShow) {
    const button = document.createElement('button');
    button.type = 'button';
    button.classList.add('btn', 'btn-primary', 'btn-sm');
    button.textContent = i18n.get(buttonTextKey);
    button.onclick = () => {
        const func = window[onClickFunction];
        if (typeof func === 'function')
            func(idx);
    };
    divAction.appendChild(button);
    //}
    tdAction.appendChild(divAction);
    return tdAction;
}
// Helper-Funktion
export const createTd = ({ text = '', id = null, scope = null }) => {
    const td = document.createElement('td');
    if (id)
        td.id = id;
    if (scope)
        td.setAttribute('scope', scope);
    td.textContent = text;
    return td;
};
