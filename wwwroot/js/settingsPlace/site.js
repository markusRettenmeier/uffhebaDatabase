function addPostalcode() {
    const container = document.getElementById("postalcodeContainer");
    const index = container.children.length;

    const wrapper = document.createElement("div");
    wrapper.className = "input-group inputDivPostalcode mb-2";

    const divInputs = document.createElement("div");
    divInputs.className = "input-group";

    const postalcodeInput = document.createElement("input");
    postalcodeInput.type = "text";
    postalcodeInput.className = "form-control inputPostalcode";
    postalcodeInput.placeholder = "PLZ";
    postalcodeInput.name = `SettlementNPostalcodeList[${index}].Postalcode.PostalcodeNumber`;

    const divCheckbox = document.createElement("div");
    divCheckbox.className = "form-check";

    const currentNameCheckbox = document.createElement("input");
    currentNameCheckbox.type = "checkbox";
    currentNameCheckbox.className = "form-check-input checkboxCurrentName";
    currentNameCheckbox.name = `SettlementNPostalcodeList[${index}].IsCurrentPostalcode`;
    currentNameCheckbox.id = "currentPostalcode_" + index;
    currentNameCheckbox.value = "true";
    currentNameCheckbox.setAttribute("data-val", "true");

    const currentNameLabel = document.createElement("label");
    currentNameLabel.className = "form-check-label ms-1";
    currentNameLabel.htmlFor = "currentPostalcode_" + index
    currentNameLabel.innerText = "Aktuelle PLZ";

    const removeButton = document.createElement("button");
    removeButton.type = "button";
    removeButton.className = "btn btn-danger removePostalcode";
    removeButton.textContent = "Entfernen";

    divInputs.appendChild(postalcodeInput);
    divCheckbox.appendChild(currentNameCheckbox);
    divCheckbox.appendChild(currentNameLabel);
    wrapper.appendChild(divInputs);
    wrapper.appendChild(divCheckbox);
    wrapper.appendChild(removeButton);
    container.appendChild(wrapper);
}
(function initRemovePostalcodeButtonHandler() {
    const container = document.getElementById('postalcodeContainer');

    if (!container) return; // Frühzeitiger Ausstieg, wenn Tabelle nicht vorhanden

    container.addEventListener('click', function (e) {
        if (e.target && e.target.classList.contains('removePostalcode')) {
            const trTag = e.target.closest('.inputDivPostalcode');
            if (trTag) {
                trTag.remove();
                reindexPostalcodeFields(container);
            }
        }
    });
})();
function reindexPostalcodeFields(container) {
    const entries = Array.from(container.querySelectorAll(".inputDivPostalcode"));

    entries.forEach((entry, index) => {
        entry.id = `inputDivPostalcode${index}`;

        const manufactoryIdInput = entry.querySelector(".inputPostalcode");
        if (manufactoryIdInput) {
            manufactoryIdInput.name = `SettlementNPostalcodeList[${index}].Postalcode.PostalcodeNumber`;
        }
    });
}

function addRelatedPlace(idx) {
    const tbody = document.getElementById("appendPlaceHere");

    if (tbody.querySelector("tr[data-type='related']")) {
        alert("Es darf nur ein Related Place existieren.");
        return;
    }
    addPlaceToRelationTable(idx, "Bezug");
}
function addParentPlace(idx) {
    const tbody = document.getElementById("appendPlaceHere");

    if (tbody.querySelector("tr[data-type='parent']")) {
        alert("Es darf nur ein Parent Place existieren.");
        return;
    }
    addPlaceToRelationTable(idx, "Eltern");
}
function addChildPlace(idx) {
    addPlaceToRelationTable(idx, "Kind");
}
function addPlaceToRelationTable(idx, type) {
    const tbody = document.getElementById("appendPlaceHere");
    const row = document.createElement("tr");
    row.dataset.type = type;
    row.className = `placeTableRow`;

    const tdName = document.createElement("td");
    let value = document.getElementById(`placeSearchResultToponym_${idx}`).innerHTML;
    tdName.innerHTML = value;

    const tdSpecs = document.createElement("td");
    value = document.getElementById(`placeSearchResultFurtherSpecs_${idx}`).innerHTML;
    tdSpecs.innerHTML = value;

    const tdType = document.createElement("td");
    tdType.textContent = type;

    const tdActions = document.createElement("td");

    let hiddenInput = document.createElement("input");
    hiddenInput.type = "hidden";

    if (type === "Bezug") {
        hiddenInput.name = "Settlement.RelatedPlaceID";
        hiddenInput.value = document.getElementById(`placeSearchResultPlaceID_${idx}`);
    } else if (type === "Eltern") {
        hiddenInput.name = "Place.ParentPlaceID";
    } else if (type === "Kind") {
        const index = tbody.querySelectorAll("tr[data-type='child']").length;
        hiddenInput.name = `ChildPlaceList[${index}].PlaceID`;
    }
    hiddenInput.value = document.getElementById(`placeSearchResultPlaceID_${idx}`).innerHTML;

    const removeBtn = document.createElement("button");
    removeBtn.type = "button";
    removeBtn.classList.add("btn", "btn-danger", "btn-sm", "removePlaceRow");
    removeBtn.textContent = "Entfernen";

    tdActions.appendChild(hiddenInput);
    tdActions.appendChild(removeBtn);

    row.appendChild(tdName);
    row.appendChild(tdSpecs);
    row.appendChild(tdType);
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
    const childRows = document.querySelectorAll("#appendPlaceHere tr[data-type='child']");
    childRows.forEach((row, i) => {
        const hidden = row.querySelector("input[type='hidden']");
        hidden.name = `ChildPlaceList[${i}].PlaceID`;
    });
}

function addToponymy() {
    const container = document.getElementById("toponymyContainer");
    const index = container.children.length;

    const wrapper = document.createElement("div");
    wrapper.className = "input-group inputDivToponymy mb-2";

    const divInputs = document.createElement("div");
    divInputs.className = "input-group";

    const toponymyInput = document.createElement("input");
    toponymyInput.type = "text";
    toponymyInput.className = "form-control inputToponymy";
    toponymyInput.placeholder = "Geografischer Name";
    toponymyInput.name = `PlaceNToponymyList[${index}].Toponymy.ToponymyName`;

    divInputs.appendChild(toponymyInput);

    const divCheckbox = document.createElement("div");
    divCheckbox.className = "form-check";

    const currentNameCheckbox = document.createElement("input");
    currentNameCheckbox.type = "checkbox";
    currentNameCheckbox.className = "form-check-input checkboxCurrentName";
    currentNameCheckbox.name = `PlaceNToponymyList[${index}].IsCurrentName`;
    currentNameCheckbox.value = "true";
    currentNameCheckbox.id = "currentName_" + index
    currentNameCheckbox.setAttribute("data-val", "true");
    divCheckbox.appendChild(currentNameCheckbox);

    const currentNameLabel = document.createElement("label");
    currentNameLabel.className = "form-check-label ms-1";
    currentNameLabel.htmlFor = "currentName_" + index
    currentNameLabel.innerText = "Aktueller Name";
    divCheckbox.appendChild(currentNameLabel);

    const removeButton = document.createElement("button");
    removeButton.type = "button";
    removeButton.className = "btn btn-danger removeToponmy";
    removeButton.textContent = "Entfernen";

    wrapper.appendChild(divInputs);
    wrapper.appendChild(divCheckbox);
    wrapper.appendChild(removeButton);
    container.appendChild(wrapper);
}
(function initRemoveToponymyButtonHandler() {
    const container = document.getElementById('toponymyContainer');
    if (!container) return;

    container.addEventListener('click', function (e) {
        if (e.target && e.target.classList.contains('removeToponmy')) {
            const trTag = e.target.closest('.inputDivToponymy');
            if (trTag) {
                trTag.remove();
                reindexToponymyFields(container);
            }
        }
    });
})();
function reindexToponymyFields(container) {
    const entries = Array.from(container.querySelectorAll(".inputDivToponymy"));

    entries.forEach((entry, index) => {
        const manufactoryIdInput = entry.querySelector(".inputToponymy");
        if (manufactoryIdInput) {
            manufactoryIdInput.name = `PlaceNToponymyList[${index}].Toponymy.ToponymyName`;
        }

        const isCurrentNameCheckbox = entry.querySelector(".checkboxCurrentName");
        if (isCurrentNameCheckbox) {
            isCurrentNameCheckbox.name = `PlaceNToponymyList[${index}].IsCurrentName`;
        }
    });
}