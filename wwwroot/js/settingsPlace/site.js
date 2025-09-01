function AddParentCity(buttonId) {
    SetCityIntoTable(buttonId);
}

function SetCityIntoTable(buttonId) {
    var value = document.getElementById('citySearchResultcityID_' + buttonId).innerHTML;
    document.getElementById('city_ID').innerHTML = value;
    value = document.getElementById('citySearchResultOecoynm_' + buttonId).innerHTML;
    document.getElementById('city').innerHTML = value;
    value = document.getElementById('citySearchResultPostalcode_' + buttonId).innerHTML;
    document.getElementById('postalcode').innerHTML = value;
    value = document.getElementById('citySearchResultByname_' + buttonId).innerHTML;
    document.getElementById('byname').innerHTML = value;
    value = document.getElementById('citySearchResultGeography_' + buttonId).innerHTML;
    document.getElementById('geography').innerHTML = value;

    document.getElementById('clearOneRowCityTable').style.display = 'inline';
    hideModal('IsCityExistingModal')
}
const toggleClearOneRowCityTable = document.getElementById('clearOneRowCityTable')
if (toggleClearOneRowCityTable) {
    toggleClearOneRowCityTable.addEventListener('click', () => {
        document.getElementById('city_ID').innerHTML = ''
        document.getElementById('city').innerHTML = ''
        document.getElementById('postalcode').innerHTML = ''
        document.getElementById('byname').innerHTML = ''
        document.getElementById('geography').innerHTML = ''
        document.getElementById('clearOneRowCityTable').style.display = 'none'
    })
}


//$('.addOeconymCreateCity').on('click', AddOeconymCreateCity);
//function AddOeconymCreateCity() {
//    let source = $('.oeconymOriginalCreateCity'), clone = source.clone(true)
//    let lastElement = $('.InputOeconymName').last()
//    let lastId = lastElement.attr('id')
//    let splittedId = lastId.split('_').pop()
//    let currentId = parseInt(splittedId) + 1

//    clone.attr('class', 'input-group pb-1 DivOeconym').attr('id', 'DivOeconym_' + currentId)
//    clone.find('.InputOeconymName').attr('id', 'InputOeconymName_' + currentId).val('')
//    clone.find('.InputCurrentName').attr('id', 'InputCurrentName_' + currentId).val('')
//    clone.find('.addOeconymCreateCity').remove()
//    clone.find('.removeOeconymCreateCity').attr('id', currentId).show()

//    clone.appendTo('.appendOeconymCreateCity');
//}
//$('.removeOeconymCreateCity').on('click', function () {
//    let id = $(this).prop('id')
//    $('div#DivOeconym_' + id).remove()
//})

//$(".createCitySubmitButton").on('click', function () {
//    var value = document.getElementById('city_ID').innerHTML
//    $('#parentCityInput').val(value)

//    var oeconym = ''
//    var count = 1;
//    $(".InputPostalcodeNumber").each(function () {
//        if (this.value != '') {
//            createNewInput('PostalcodeNumberList', this.value.trim())
//        }
//    });
//    $(".InputOeconymName").each(function () {
//        if (this.value != '') {
//            var oeconymName = this.value.trim()
//            var currentName = $('#InputCurrentName_' + count).is(":checked")
//            oeconym = oeconymName + "§§" + currentName
//        }
//        count++
//        createNewInput('OeconymList', oeconym)
//    });
//})
//function createNewInput(name, value) {
//    $('<input>').attr({
//        type: 'hidden',
//        name: name,
//        value: value
//    }).appendTo('form')
//}

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

    //const hiddenEraInput = document.createElement("input");
    //hiddenEraInput.type = "hidden";
    //hiddenEraInput.className = "form-control inputEraID";
    //hiddenEraInput.name = `SettlementNPostalcodeList[${index}].EraID`;

    //const eraInput = document.createElement("input");
    //eraInput.type = "text";
    //eraInput.className = "form-control inputEra";
    //eraInput.placeholder = "Ära";

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
    //divInputs.appendChild(hiddenEraInput);
    //divInputs.appendChild(eraInput);
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

        //const citySelect = entry.querySelector(".inputEraID");
        //if (citySelect) {
        //    citySelect.name = `SettlementNPostalcodeList[${index}].EraID`;
        //}
    });
}

//nur für Ciy, kann weg
function addOeconym() {
    const container = document.getElementById("oeconym");
    const index = container.children.length;

    const wrapper = document.createElement("div");
    wrapper.className = "input-group inputDivOeconym mb-2";

    const oeconymInput = document.createElement("input");
    oeconymInput.type = "text";
    oeconymInput.className = "form-control inputOeconym";
    oeconymInput.placeholder = "Ortsname";
    oeconymInput.name = `CityOeconymList[${index}].Oeconym.OeconymName`;

    const hiddenEraInput = document.createElement("input");
    hiddenEraInput.type = "hidden";
    hiddenEraInput.className = "form-control inputEraID";
    hiddenEraInput.name = `CityOeconymList[${index}].EraID`;

    const eraInput = document.createElement("input");
    eraInput.type = "text";
    eraInput.className = "form-control inputEra";
    eraInput.placeholder = "Ära";

    const currentNameCheckbox = document.createElement("input");
    currentNameCheckbox.type = "checkbox";
    currentNameCheckbox.className = "form-check-input checkboxCurrentName";
    currentNameCheckbox.name = `CityOeconymList[${index}].IsCurrentName`;

    const removeButton = document.createElement("button");
    removeButton.type = "button";
    removeButton.className = "btn btn-danger removeOeconym";
    removeButton.textContent = "Entfernen";

    wrapper.appendChild(oeconymInput);
    wrapper.appendChild(hiddenEraInput);
    wrapper.appendChild(eraInput);
    wrapper.appendChild(currentNameCheckbox);
    wrapper.appendChild(removeButton);
    container.appendChild(wrapper);
}
(function initRemoveOeconymButtonHandler() {
    const container = document.getElementById('oeconymContainer');
    if (!container) return;

    container.addEventListener('click', function (e) {
        if (e.target && e.target.classList.contains('removeOeconym')) {
            const trTag = e.target.closest('.inputDivOeconym');
            if (trTag) {
                trTag.remove();
                reindexOeconymFields(container);
            }
        }
    });
})();
function reindexOeconymFields(container) {
    const entries = Array.from(container.querySelectorAll(".inputDivOeconym"));

    entries.forEach((entry, index) => {
        entry.id = `inputDivOeconym${index}`;

        const manufactoryIdInput = entry.querySelector(".inputOeconym");
        if (manufactoryIdInput) {
            manufactoryIdInput.name = `CityOeconymList[${index}].Oeconym.OeconymName`;
        }

        const citySelect = entry.querySelector(".inputEraID");
        if (citySelect) {
            citySelect.name = `CityPostalcodeList[${index}].EraID`;
        }

        const isCurrentNameCheckbox = entry.querySelector(".checkboxCurrentName");
        if (isCurrentNameCheckbox) {
            isCurrentNameCheckbox.name = `CityPostalcodeList[${index}].IsCurrentName`;
        }
    });
}

//// Hält den aktuellen Status, um die Bedingungen zu prüfen
//const selectedPlaces = {
//    relatedPlace: null,
//    parentPlace: null,
//    childPlaces: []
//};

//// Mapping für Eingabetypen
//const placeTypeMap = {
//    related: { label: "RelatedPlace", inputName: "Settlement.RelatedPlaceID" },
//    parent: { label: "ParentPlace", inputName: "Place.ParentPlaceID" },
//    child: { label: "ChildPlace", inputName: "ChildPlaceList[].PlaceID" }
//};

//// Füge Place in Tabelle ein
//function addPlaceToRelationTable(place, relationType) {
//    const tbody = document.getElementById('appendPlaceHere');

//    // Prüfen, ob Eintrag schon existiert
//    if (relationType === "related" && selectedPlaces.relatedPlace) {
//        alert("Es darf nur 1 RelatedPlace geben.");
//        return;
//    }
//    if (relationType === "parent" && selectedPlaces.parentPlace) {
//        alert("Es darf nur 1 ParentPlace geben.");
//        return;
//    }
//    if (relationType === "child" && selectedPlaces.childPlaces.includes(place.placeID)) {
//        alert("Dieses ChildPlace wurde schon hinzugefügt.");
//        return;
//    }

//    const hiddenInput = document.createElement('input');
//    hiddenInput.type = 'hidden';
//    hiddenInput.name = placeTypeMap[relationType].inputName;
//    hiddenInput.value = place.placeID;

//    const tr = document.createElement('tr');
//    tr.dataset.placeId = place.placeID;
//    tr.dataset.relationType = relationType;

//    const tdName = document.createElement('td');
//    tdName.innerHTML = place.oeconymDisplay || '';
//    tr.appendChild(tdName);

//    const tdSpecs = document.createElement('td');
//    tdSpecs.textContent = place.furtherSpecs || '';
//    tr.appendChild(tdSpecs);

//    const tdRelation = document.createElement('td');
//    tdRelation.textContent = placeTypeMap[relationType].label;
//    tr.appendChild(tdRelation);

//    const tdActions = document.createElement('td');
//    const removeBtn = document.createElement('button');
//    removeBtn.type = 'button';
//    removeBtn.className = 'btn btn-danger btn-sm';
//    removeBtn.textContent = 'Entfernen';
//    removeBtn.onclick = function () {
//        removePlaceFromRelation(tr, hiddenInput, relationType, place.placeID);
//    };
//    tdActions.appendChild(removeBtn);
//    tr.appendChild(tdActions);
//    tr.appendChild(hiddenInput);
//    tbody.appendChild(tr);

//    if (relationType === "related") {
//        selectedPlaces.relatedPlace = place.placeID;
//    } else if (relationType === "parent") {
//        selectedPlaces.parentPlace = place.placeID;
//    } else if (relationType === "child") {
//        selectedPlaces.childPlaces.push(place.placeID);
//    }
//}

//// Entfernt Place aus Tabelle + Status
//function removePlaceFromRelation(row, hiddenInput, relationType, placeId) {
//    row.remove();
//    hiddenInput.remove();

//    if (relationType === "related") {
//        selectedPlaces.relatedPlace = null;
//    } else if (relationType === "parent") {
//        selectedPlaces.parentPlace = null;
//    } else if (relationType === "child") {
//        selectedPlaces.childPlaces = selectedPlaces.childPlaces.filter(id => id !== placeId);
//    }
//}

//let searchResults = [];
//// Diese Funktion wird von der Suchergebnis-Tabelle aufgerufen
//function addPlace(idx) {
//    // Hole die Daten aus der Ergebniszeile (z. B. aus deinem fetch-Ergebnis gecached)
//    const place = searchResults[idx]; // searchResults ist ein globales Array mit den fetch-Ergebnissen
//    const relationType = prompt("Beziehungstyp eingeben: related, parent oder child");

//    if (!["related", "parent", "child"].includes(relationType)) {
//        alert("Ungültiger Beziehungstyp.");
//        return;
//    }

//    addPlaceToRelationTable(place, relationType);
//}

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

    // Hidden Input je nach Typ
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

    // Remove Button
    const removeBtn = document.createElement("button");
    removeBtn.type = "button";
    removeBtn.classList.add("btn", "btn-danger", "btn-sm", "removePlaceRow");
    removeBtn.textContent = "Entfernen";
    //removeBtn.addEventListener("click", () => {
    //    row.remove();
    //    if (type === "child") {
    //        reindexChildPlaces();
    //    }
    //});

    // Zusammensetzen
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

    //const hiddenEraInput = document.createElement("input");
    //hiddenEraInput.type = "hidden";
    //hiddenEraInput.className = "form-control inputEraID";
    //hiddenEraInput.name = `PlaceNToponymyList[${index}].EraID`;

    //const eraInput = document.createElement("input");
    //eraInput.type = "text";
    //eraInput.className = "form-control inputEra";
    //eraInput.placeholder = "Ära";

    divInputs.appendChild(toponymyInput);
    //divInputs.appendChild(hiddenEraInput);
    //divInputs.appendChild(eraInput);

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

    //wrapper.appendChild(toponymyInput);
    //wrapper.appendChild(hiddenEraInput);
    //wrapper.appendChild(eraInput);
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

        //const citySelect = entry.querySelector(".inputEraID");
        //if (citySelect) {
        //    citySelect.name = `PlaceNToponymyList[${index}].EraID`;
        //}

        const isCurrentNameCheckbox = entry.querySelector(".checkboxCurrentName");
        if (isCurrentNameCheckbox) {
            isCurrentNameCheckbox.name = `PlaceNToponymyList[${index}].IsCurrentName`;
        }
    });
}