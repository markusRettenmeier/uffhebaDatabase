//import { i18n } from "../TranslationService";
//import type { IndustryItem } from "../types";
import { autocomplete } from "../site.js";

//function addPlace(idx: number): void {
//    const tbody = document.getElementById("appendPlaceHere") as HTMLTableSectionElement;
//    if (!tbody) return;

//    const index = tbody.children.length;

//    const toponymElement = document.getElementById(`placeSearchResultToponym_${idx}`);
//    const furtherSpecsElement = document.getElementById(`placeSearchResultFurtherSpecs_${idx}`);
//    const placeIDElement = document.getElementById(`placeSearchResultPlaceID_${idx}`);

//    if (!toponymElement || !furtherSpecsElement || !placeIDElement) {
//        console.error('Required elements not found for index:', idx);
//        return;
//    }

//    const toponym = toponymElement.innerHTML || '';
//    const furtherSpecs = furtherSpecsElement.textContent || '';
//    const placeID = placeIDElement.textContent || '';

//    const row = document.createElement("tr");
//    row.className = `placeTableRow`;

//    const tdName = document.createElement("td");
//  tdName.innerHTML = toponym;

//    const tdSpecs = document.createElement("td");
//    tdSpecs.textContent = furtherSpecs;

//    const tdActions = document.createElement("td");

//    const hiddenInput = document.createElement("input");
//    hiddenInput.type = "hidden";
//    hiddenInput.name = `PlaceList[${index}].PlaceID`;
//    hiddenInput.classList.add('form-control', 'placeResultTableId');
//    hiddenInput.value = placeID;

//    const removeBtn = document.createElement("button");
//    removeBtn.type = "button";
//    removeBtn.classList.add("btn", "btn-danger", "btn-sm", "removePlaceRow");
//    removeBtn.textContent = i18n.get("Remove");

//    tdActions.appendChild(hiddenInput);
//    tdActions.appendChild(removeBtn);

//    row.appendChild(tdName);
//    row.appendChild(tdSpecs);
//    row.appendChild(tdActions);

//    tbody.appendChild(row);
//}

//(function initRemovePlaceButtonHandler(): void {
//    const container = document.getElementById('appendPlaceHere');
//    if (!container) return;

//    container.addEventListener('click', function (e: Event) {
//        const target = e.target as HTMLElement;
//        if (target && target.classList.contains('removePlaceRow')) {
//            const trTag = target.closest('.placeTableRow') as HTMLTableRowElement;
//            if (trTag) {
//                trTag.remove();
//                reindexChildPlaces(container as HTMLTableSectionElement);
//            }
//        }
//    });
//})();

//function reindexChildPlaces(container: HTMLTableSectionElement): void {
//    const rows = container.querySelectorAll(".placeTableRow");

//    rows.forEach((row: Element, i: number) => {
//        const hiddenInput = row.querySelector(".placeResultTableId") as HTMLInputElement;
//        if (hiddenInput) {
//            hiddenInput.name = `PlaceList[${i}].PlaceID`;
//        }
//    });
//}

//function setProductionFacilitiesIntoOptions(stored: string | null): void {
//    const select = document.getElementById("inputProductionFacilityID") as HTMLSelectElement;
//    if (!select) {
//        console.warn('Production facility select element not found');
//        return;
//    }

//    if (!stored) {
//        console.warn('No stored production facility data');
//        return;
//    }

//    let list: ProductionFacilityItem[];
//    try {
//        list = JSON.parse(stored);
//    } catch (error) {
//        console.error('Error parsing stored production facilities:', error);
//        return;
//    }

//    if (!Array.isArray(list) || list.length === 0) {
//        console.warn('Production facility list is empty');
//        return;
//    }

//    // Clear existing options
//    select.innerHTML = '';

//    const defaultOption = document.createElement('option');
//    defaultOption.value = '';
//    defaultOption.textContent = i18n.get("Option_Select");
//    select.appendChild(defaultOption);

//    const selectedProductionFacilityIDElement = document.getElementById("hiddenProductionFacilityID") as HTMLInputElement;
//    let selectedOptionID: string | null = null;

//    if (selectedProductionFacilityIDElement) {
//        selectedOptionID = selectedProductionFacilityIDElement.value;
//        const productionFacilityDiv = document.getElementById("productionFacilityDiv");
//        if (productionFacilityDiv && productionFacilityDiv.contains(selectedProductionFacilityIDElement)) {
//            productionFacilityDiv.removeChild(selectedProductionFacilityIDElement);
//        }
//    }

//    list.forEach((item: ProductionFacilityItem) => {
//        const option = document.createElement('option');
//        option.value = item.id.toString();

//        if (selectedOptionID && item.id === parseInt(selectedOptionID, 10)) {
//            option.selected = true;
//        }

//        option.textContent = item.name;
//        select.appendChild(option);
//    });
//}

// Globale Deklarationen für Event-Handler
//declare global {
//    interface Window {
//        //addPlace: (idx: number) => void;
//        //initRemovePlaceButtonHandler: () => void;
//        //setProductionFacilitiesIntoOptions: (stored: string | null) => void;
//    }
//}

//// Nur im Browser-Kontext verfügbar machen
//if (typeof window !== 'undefined') {
    //window.addPlace = addPlace;
    //window.setProductionFacilitiesIntoOptions = setProductionFacilitiesIntoOptions;
//}

const industryInput = document.getElementById("industryInput") as HTMLInputElement;
let industryArray: string[] = [];
try {
  const industryJson = sessionStorage.getItem("industryList");
  industryArray = industryJson ? JSON.parse(industryJson) : [];
} catch {
  industryArray = [];
}
autocomplete(industryInput, industryArray);