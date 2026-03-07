import { i18n } from './TranslationService.js';
import { getCollectionAreaList, getIndustryList, initializePlaceSearch, initializePartySearch } from './api.js';
import { getAndSetConceptualRelationshipGraph } from './settingsConceptualRelationship/api.js';
export async function handlePageLoad() {
    await i18n.loadTranslations();
    initializePlaceSearch();
    initializePartySearch();
    const url = window.location.href;
    if (url.includes("/Login"))
        window.checkWebAuthnSupport();
    if (url.includes("OrganizationDatabase/Create") || url.includes("OrganizationDatabase/Edit")) {
        let stored = sessionStorage.getItem('industryList');
        if (!stored) {
            stored = await getIndustryList();
        }
    }
    if (url.includes("Database") || url.includes("Settings")) {
        let stored = sessionStorage.getItem('collectionAreasList');
        if (!stored) {
            stored = await getCollectionAreaList();
        }
        if (stored) {
            setCollectionAreasIntoOptions(stored);
        }
    }
    if (url.includes("ConceptualRelationshipDatabase/IndexSpecific")) {
        getAndSetConceptualRelationshipGraph();
    }
}
function addPlace(idx) {
    const tbody = document.getElementById("appendPlaceHere");
    if (!tbody) {
        console.error('appendPlaceHere element not found');
        return;
    }
    const indexofTable = tbody.children.length;
    // Get data from search result elements
    const toponymElement = document.getElementById(`placeSearchResultToponym_${idx}`);
    const furtherSpecsElement = document.getElementById(`placeSearchResultFurtherSpecs_${idx}`);
    const placeIDElement = document.getElementById(`placeSearchResultPlaceID_${idx}`);
    if (!toponymElement || !furtherSpecsElement || !placeIDElement) {
        console.error(`Required place search elements not found for index ${idx}`);
        return;
    }
    const toponym = toponymElement.innerHTML || '';
    const furtherSpecs = furtherSpecsElement.textContent || '';
    const placeID = placeIDElement.textContent || '';
    const row = document.createElement("tr");
    row.className = `placeTableRow`;
    const tdName = document.createElement("td");
    tdName.innerHTML = toponym;
    // Further specifications cell
    const tdSpecs = document.createElement("td");
    tdSpecs.textContent = furtherSpecs;
    const tdRelationType = document.createElement("td");
    const relationInput = document.createElement("input");
    relationInput.type = "text";
    relationInput.className = "form-control";
    relationInput.name = `ConnectedPlaceList[${indexofTable}].RelationType`;
    relationInput.placeholder = i18n.get("EnterRelationType") || "Relation type";
    tdRelationType.appendChild(relationInput);
    // Actions cell
    const tdActions = document.createElement("td");
    // Hidden input for PlaceID
    const hiddenInput = document.createElement("input");
    hiddenInput.type = "hidden";
    hiddenInput.name = `ConnectedPlaceList[${indexofTable}].PlaceID`;
    hiddenInput.value = placeID;
    // Remove button
    const removeBtn = document.createElement("button");
    removeBtn.type = "button";
    removeBtn.classList.add("btn", "btn-danger", "btn-sm", "removePlaceRow");
    removeBtn.textContent = i18n.get("Remove");
    // Assemble row
    tdActions.appendChild(hiddenInput);
    tdActions.appendChild(removeBtn);
    row.appendChild(tdName);
    row.appendChild(tdSpecs);
    row.appendChild(tdActions);
    tbody.appendChild(row);
    hideModal('placeModal');
}
(function initRemovePlaceButtonHandler() {
    const container = document.getElementById('appendPlaceHere');
    if (!container) {
        console.warn('appendPlaceHere container not found for remove button handler');
        return;
    }
    container.addEventListener('click', function (e) {
        const target = e.target;
        if (target && target.classList.contains('removePlaceRow')) {
            const trTag = target.closest('.placeTableRow');
            if (trTag) {
                trTag.remove();
                reindexChildPlaces(container);
            }
        }
    });
})();
function reindexChildPlaces(container) {
    const childRows = container.querySelectorAll("tr[data-type='child']");
    childRows.forEach((row, i) => {
        const hiddenInput = row.querySelector("input[type='hidden']");
        if (hiddenInput) {
            hiddenInput.name = `ConnectedPlaceList[${i}].PlaceID`;
        }
    });
}
export function setCollectionAreasIntoOptions(stored) {
    const select = document.getElementById("appendCollectionAreasHere");
    if (!select)
        return;
    let list;
    try {
        list = JSON.parse(stored);
    }
    catch (error) {
        console.error("Failed to parse collection areas:", error);
        return;
    }
    if (!Array.isArray(list) || list.length === 0)
        return;
    list.forEach(item => {
        const li = document.createElement('li');
        li.className = 'nav-item';
        const a = document.createElement('a');
        a.className = 'nav-link';
        a.href = `/CollectionItemDatabase/Index?collectionAreaID=${item.id}`;
        a.textContent = item.name;
        select.appendChild(li);
        li.appendChild(a);
    });
}
function openNav() {
    const sidebar = document.getElementById("mySidebar");
    if (!sidebar)
        return;
    if (window.innerWidth <= 600) {
        sidebar.style.width = "100%";
    }
    else {
        sidebar.style.width = "600px";
    }
}
function closeNav() {
    const sidebar = document.getElementById("mySidebar");
    if (sidebar) {
        sidebar.style.width = "0";
    }
}
export function hideModal(modalName) {
    const modal = document.getElementById(modalName);
    if (modal?.classList.contains('modal')) {
        const modalInstance = bootstrap.Modal.getInstance(modal) || new bootstrap.Modal(modal);
        modalInstance.hide();
    }
}
// Event Listener
document.addEventListener("DOMContentLoaded", () => {
    handlePageLoad();
});
export function autocomplete(input, values) {
    let currentFocus = -1;
    input.addEventListener("input", () => {
        const val = input.value;
        closeAllLists();
        if (!val)
            return;
        currentFocus = -1;
        const listContainer = document.createElement("div");
        listContainer.id = `${input.id}-autocomplete-list`;
        listContainer.className = "autocomplete-items";
        input.parentElement?.appendChild(listContainer);
        for (const item of values) {
            if (item.toUpperCase().startsWith(val.toUpperCase())) {
                const itemElement = document.createElement("div");
                const strongPart = item.substring(0, val.length);
                const restPart = item.substring(val.length);
                itemElement.innerHTML = `
          <strong>${strongPart}</strong>${restPart}
          <input type="hidden" value="${item}">
        `;
                itemElement.addEventListener("click", () => {
                    const hiddenInput = itemElement.querySelector("input");
                    if (hiddenInput) {
                        input.value = hiddenInput.value;
                    }
                    closeAllLists();
                });
                listContainer.appendChild(itemElement);
            }
        }
    });
    input.addEventListener("keydown", (e) => {
        const list = document.getElementById(`${input.id}-autocomplete-list`);
        const items = list?.getElementsByTagName("div");
        if (!items)
            return;
        switch (e.key) {
            case "ArrowDown":
                currentFocus++;
                addActive(items);
                break;
            case "ArrowUp":
                currentFocus--;
                addActive(items);
                break;
            case "Enter":
                e.preventDefault();
                if (currentFocus > -1 && items[currentFocus]) {
                    items[currentFocus].click();
                }
                break;
        }
    });
    function addActive(elements) {
        if (!elements.length)
            return;
        removeActive(elements);
        if (currentFocus >= elements.length)
            currentFocus = 0;
        if (currentFocus < 0)
            currentFocus = elements.length - 1;
        elements[currentFocus].classList.add("autocomplete-active");
    }
    function removeActive(elements) {
        for (const el of Array.from(elements)) {
            el.classList.remove("autocomplete-active");
        }
    }
    function closeAllLists(exceptElement) {
        const lists = document.getElementsByClassName("autocomplete-items");
        for (const list of Array.from(lists)) {
            if (exceptElement !== list &&
                exceptElement !== input) {
                list.parentElement?.removeChild(list);
            }
        }
    }
    document.addEventListener("click", (e) => {
        closeAllLists(e.target);
    });
}
if (typeof window !== 'undefined') {
    window.addPlace = addPlace;
    window.openNav = openNav;
    window.closeNav = closeNav;
}
