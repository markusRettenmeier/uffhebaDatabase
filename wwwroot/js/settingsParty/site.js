function addPlace(idx) {
    const tbody = document.getElementById("appendPlaceHere");
    const index = tbody.children.length;

    const row = document.createElement("tr");
    row.className = `placeTableRow`;

    const tdName = document.createElement("td");
    let value = document.getElementById(`placeSearchResultToponym_${idx}`).textContent;
    tdName.innerHTML = value;

    const tdSpecs = document.createElement("td");
    value = document.getElementById(`placeSearchResultFurtherSpecs_${idx}`).textContent;
    tdSpecs.innerHTML = value;

    const tdActions = document.createElement("td");

    let hiddenInput = document.createElement("input");
    hiddenInput.type = "hidden";
    hiddenInput.name = `PlaceList[${index}].PlaceID`;
    hiddenInput.classList.add('form-control', 'placeResultTableId');
    hiddenInput.value = document.getElementById(`placeSearchResultPlaceID_${idx}`).textContent;

    const removeBtn = document.createElement("button");
    removeBtn.type = "button";
    removeBtn.classList.add("btn", "btn-danger", "btn-sm", "removePlaceRow");
    removeBtn.textContent = "Entfernen";

    tdActions.appendChild(hiddenInput);
    tdActions.appendChild(removeBtn);

    row.appendChild(tdName);
    row.appendChild(tdSpecs);
    row.appendChild(tdActions);

    tbody.appendChild(row);
}
(function initRemovePlaceButtonHandler() {
    const container = document.getElementById('appendPlaceHere');
    if (!container) return;

    container.addEventListener('click', function (e) {
        if (e.target && e.target.classList.contains('removePlaceRow')) {
            const trTag = e.target.closest('.placeTableRow');
            if (trTag) {
                trTag.remove();
                reindexChildPlaces(container);
            }
        }
    });
})();
function reindexChildPlaces() {
    const childRows = document.querySelectorAll("#appendPlaceHere");
    childRows.forEach((row, i) => {
        const hidden = row.querySelector(".placeResultTableId");
        if (hidden) {
            hidden.name = `PlaceList[${i}].PlaceID`;
        }
    });
}

function setProductionFacilitesIntoOptions(stored) {
    const select = document.getElementById("inputProductionFacilityID");
    if (!select) return;

    if (!stored) return;
    const list = JSON.parse(stored);
    if (!list || list.length === 0) return;
    // Clear existing options
    select.innerHTML = '';
    
    const defaultOption = document.createElement('option');
    defaultOption.value = '';
    defaultOption.textContent = '-- Bitte wählen --';
    select.appendChild(defaultOption);

    const selectedProductionFacilityIDElement = document.getElementById("hiddenProductionFacilityID");
    let selectedOptionID = null;
    if (selectedProductionFacilityIDElement != null) {
        selectedOptionID = selectedProductionFacilityIDElement.value;
        document.getElementById("productionFacilityDiv").removeChild(selectedProductionFacilityIDElement);
    }
    
    list.forEach(item => {
        const option = document.createElement('option');
        option.value = item.id
        if (item.id === parseInt(selectedOptionID)) {
            option.selected = true;
        }
        option.textContent = item.name;
        select.appendChild(option);
    });    
}