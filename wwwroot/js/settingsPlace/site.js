import { i18n } from "../TranslationService.js";
//function addPlace(idx: number): void {
//  const tbody = document.getElementById("appendPlaceHere") as HTMLTableSectionElement;
//  if (!tbody) {
//    console.error('appendPlaceHere element not found');
//    return;
//  }
//  // Get data from search result elements
//  const toponymElement = document.getElementById(`placeSearchResultToponym_${idx}`);
//  const furtherSpecsElement = document.getElementById(`placeSearchResultFurtherSpecs_${idx}`);
//  const placeIDElement = document.getElementById(`placeSearchResultPlaceID_${idx}`);
//  if (!toponymElement || !furtherSpecsElement || !placeIDElement) {
//    console.error(`Required place search elements not found for index ${idx}`);
//    return;
//  }
//  const toponym = toponymElement.innerHTML || '';
//  const furtherSpecs = furtherSpecsElement.textContent || '';
//  const placeID = placeIDElement.textContent || '';
//  const row = document.createElement("tr");
//  row.className = `placeTableRow`;
//  const tdName = document.createElement("td");
//  tdName.innerHTML = toponym;
//  // Further specifications cell
//  const tdSpecs = document.createElement("td");
//  tdSpecs.textContent = furtherSpecs;
//  const tdRelationType = document.createElement("td");
//  const relationInput = document.createElement("input");
//  relationInput.type = "text";
//  relationInput.className = "form-control";
//  relationInput.name = `ConnectedPlaceList[${idx}].RelationType`;
//  relationInput.placeholder = i18n.get("EnterRelationType") || "Relation type";
//  tdRelationType.appendChild(relationInput);
//  // Actions cell
//  const tdActions = document.createElement("td");
//  // Hidden input for PlaceID
//  const hiddenInput = document.createElement("input");
//  hiddenInput.type = "hidden";
//  hiddenInput.name = `ConnectedPlaceList[${idx}].PlaceID`;
//  hiddenInput.value = placeID;
//  // Remove button
//  const removeBtn = document.createElement("button");
//  removeBtn.type = "button";
//  removeBtn.classList.add("btn", "btn-danger", "btn-sm", "removePlaceRow");
//  removeBtn.textContent = i18n.get("Remove");
//  // Assemble row
//  tdActions.appendChild(hiddenInput);
//  tdActions.appendChild(removeBtn);
//  row.appendChild(tdName);
//  row.appendChild(tdSpecs);
//  row.appendChild(tdActions);
//  tbody.appendChild(row);
//  hideModal('placeModal')
//}
//(function initRemovePlaceButtonHandler(): void {
//  const container = document.getElementById('appendPlaceHere');
//  if (!container) {
//    console.warn('appendPlaceHere container not found for remove button handler');
//    return;
//  }
//  container.addEventListener('click', function (e: Event) {
//    const target = e.target as HTMLElement;
//    if (target && target.classList.contains('removePlaceRow')) {
//      const trTag = target.closest('.placeTableRow') as HTMLTableRowElement;
//      if (trTag) {
//        trTag.remove();
//        reindexChildPlaces(container as HTMLTableSectionElement);
//      }
//    }
//  });
//})();
//function reindexChildPlaces(container: HTMLTableSectionElement): void {
//  const childRows = container.querySelectorAll("tr[data-type='child']") as NodeListOf<HTMLTableRowElement>;
//  childRows.forEach((row: HTMLTableRowElement, i: number) => {
//    const hiddenInput = row.querySelector("input[type='hidden']") as HTMLInputElement;
//    if (hiddenInput) {
//      hiddenInput.name = `ConnectedPlaceList[${i}].PlaceID`;
//    }
//  });
//}
function addToponymy() {
    const container = document.getElementById("toponymyContainer");
    if (!container) {
        console.error('toponymyContainer element not found');
        return;
    }
    const index = container.children.length;
    const wrapper = document.createElement("div");
    wrapper.className = "input-group inputDivToponymy mb-2";
    const divInputs = document.createElement("div");
    divInputs.className = "form-group";
    const toponymyInput = document.createElement("input");
    toponymyInput.type = "text";
    toponymyInput.className = "form-control inputToponymy";
    toponymyInput.name = `ToponymyList[${index}].Name`;
    toponymyInput.placeholder = i18n.get("EnterToponymy");
    divInputs.appendChild(toponymyInput);
    const divCheckbox = document.createElement("div");
    divCheckbox.className = "form-check";
    const checkboxId = `currentName_${index}`;
    const currentNameCheckbox = document.createElement("input");
    currentNameCheckbox.type = "checkbox";
    currentNameCheckbox.className = "form-check-input checkboxCurrentName";
    currentNameCheckbox.name = `ToponymyList[${index}].IsCurrentName`;
    currentNameCheckbox.value = "true";
    currentNameCheckbox.id = checkboxId;
    currentNameCheckbox.setAttribute("data-val", "true");
    divCheckbox.appendChild(currentNameCheckbox);
    const currentNameLabel = document.createElement("label");
    currentNameLabel.className = "form-check-label ms-1";
    currentNameLabel.htmlFor = checkboxId;
    currentNameLabel.textContent = i18n.get("IsCurrentName") || "Is current name";
    divCheckbox.appendChild(currentNameLabel);
    const removeButton = document.createElement("button");
    removeButton.type = "button";
    removeButton.className = "btn btn-danger removeToponmy";
    removeButton.textContent = i18n.get("Remove") || "Remove";
    wrapper.appendChild(divInputs);
    wrapper.appendChild(divCheckbox);
    wrapper.appendChild(removeButton);
    container.appendChild(wrapper);
    // Focus on the new input for better UX
    setTimeout(() => toponymyInput.focus(), 10);
}
(function initRemoveToponymyButtonHandler() {
    const container = document.getElementById('toponymyContainer');
    if (!container) {
        console.warn('toponymyContainer not found for remove button handler');
        return;
    }
    container.addEventListener('click', function (e) {
        const target = e.target;
        if (target && target.classList.contains('removeToponmy')) {
            const wrapperDiv = target.closest('.inputDivToponymy');
            if (wrapperDiv) {
                wrapperDiv.remove();
                reindexToponymyFields(container);
            }
        }
    });
})();
function reindexToponymyFields(container) {
    const entries = Array.from(container.querySelectorAll(".inputDivToponymy"));
    entries.forEach((entry, index) => {
        const toponymyInput = entry.querySelector(".inputToponymy");
        if (toponymyInput) {
            toponymyInput.name = `ToponymyList[${index}].Name`;
        }
        const currentNameCheckbox = entry.querySelector(".checkboxCurrentName");
        if (currentNameCheckbox) {
            currentNameCheckbox.name = `ToponymyList[${index}].IsCurrentName`;
            currentNameCheckbox.id = `currentName_${index}`;
            // Update associated label
            const label = entry.querySelector(`label[for="${currentNameCheckbox.id}"]`);
            if (label) {
                label.htmlFor = `currentName_${index}`;
            }
        }
    });
}
// Export für Module oder globale Verwendung
if (typeof window !== 'undefined') {
    //window.addPlace = addPlace;
    window.addToponymy = addToponymy;
}
