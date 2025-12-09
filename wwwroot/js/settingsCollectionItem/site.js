function SetProcessOfManufactureIntoTable(buttonId) {
    let value = document.getElementById('processOfManufactureSearchResultprocessOfManufactureID_' + buttonId).textContent;
    document.getElementById('CollectionItemEntity_ProcessOfManufactureID').value = value;
    value = document.getElementById('processOfManufactureSearchResultMainprocess_' + buttonId).textContent;
    document.getElementById('processOfManufactureMainprocess').innerText = value;
    value = document.getElementById('processOfManufactureSearchResultProcessOfManufactureName_' + buttonId).textContent;
    document.getElementById('processOfManufactureProcessOfManufactureName').innerText = value;
    value = document.getElementById('processOfManufactureSearchResultTechnique_' + buttonId).textContent;
    document.getElementById('processOfManufactureTechnique').innerText = value;

    document.getElementById(`clearOneRowProcessOfManufactureTableButton`).style.display = 'inline';
    hideModal('processOfManufactureModal');
}
(function initRemoveProcessOfManufactureButtonHandler() {
    const container = document.getElementById('processOfManufactureTable');
    if (!container) return;

    container.addEventListener('click', function (e) {
        if (e.target && e.target.id == 'clearOneRowProcessOfManufactureTableButton') {
            document.getElementById('CollectionItemEntity_ProcessOfManufactureID').value = ''
            document.getElementById('processOfManufactureMainprocess').innerText = ''
            document.getElementById('processOfManufactureProcessOfManufactureName').innerText = ''
            document.getElementById('processOfManufactureTechnique').innerText = ''
            document.getElementById(`clearOneRowProcessOfManufactureTableButton`).style.display = 'none';
        }
    });
})();

function addConcept(buttonId, relation) {
    let value = document.getElementById(`conceptSearchResultConceptID_${buttonId}`).textContent;
    document.getElementById('CollectionItemEntity_ConceptID').value = value;
    value = document.getElementById(`conceptSearchResultName_${buttonId}`).textContent;
    document.getElementById('ConceptName').innerText = value;

    document.getElementById(`clearOneRowConcept`).style.display = 'inline';
    hideModal('ConceptModal');
}
(function initRemoveConceptButtonHandler() {
    const container = document.getElementById('ConceptTable');
    if (!container) return;

    container.addEventListener('click', function (e) {
        if (e.target && e.target.id == 'clearOneRowConcept') {
            document.getElementById('CollectionItemEntity_ConceptID').value = ''
            document.getElementById('ConceptName').innerText = ''
            document.getElementById(`clearOneRowConcept`).style.display = 'none';
        }
    });
})();

function addPlace(idx) {
    const tbody = document.getElementById('appendPlaceHere');
    const index = tbody.children.length;

    const placeIdValue = document.getElementById(`placeSearchResultPlaceID_${idx}`)?.textContent || '';
    const toponymValue = document.getElementById(`placeSearchResultToponym_${idx}`)?.textContent || '';
    const furtherSpecsValue = document.getElementById(`placeSearchResultFurtherSpecs_${idx}`)?.textContent || '';

    const row = document.createElement("tr");
    row.className = `placeTableRow`;

    const tdName = document.createElement("td");
    tdName.textContent = toponymValue;

    const tdSpecs = document.createElement("td");
    tdSpecs.textContent = furtherSpecsValue;

    const tdActions = document.createElement("td");

    let hiddenInput = document.createElement("input");
    hiddenInput.type = "hidden";
    hiddenInput.classList.add('form-control', 'placeResultTableId');
    hiddenInput.name = `CollectionItemNPlaceList[${index}].PlaceID`;
    hiddenInput.value = placeIdValue;

    const tdInput = document.createElement('td');
    tdInput.setAttribute('scope', 'col');

    const inputRelationship = document.createElement('input');
    inputRelationship.name = `CollectionItemNPlaceList[${index}].Relationship`;
    inputRelationship.required = true;
    inputRelationship.classList.add('form-control', 'placeResultTableRelationship');
    tdInput.appendChild(inputRelationship);

    const removeBtn = document.createElement("button");
    removeBtn.type = "button";
    removeBtn.classList.add("btn", "btn-danger", "btn-sm", "removePlaceRow");
    removeBtn.textContent = i18n.get("Remove");

    tdActions.appendChild(hiddenInput);
    tdActions.appendChild(removeBtn);
    row.appendChild(tdName);
    row.appendChild(tdSpecs);
    row.appendChild(tdInput);
    row.appendChild(tdActions);
    tbody.appendChild(row);

    hideModal('placeModal');
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
function reindexChildPlaces(container) {
    const entries = Array.from(container.querySelectorAll(".placeTableRow"));

    entries.forEach((entry, index) => {
        const cityIdInput = entry.querySelector(".placeResultTableId");
        if (cityIdInput) {
            cityIdInput.name = `CollectionItemNPlaceList[${index}].CityID`;
        }

        const relationshipInput = entry.querySelector(".placeResultTableRelationship");
        if (relationshipInput) {
            relationshipInput.name = `CollectionItemNPlaceList[${index}].Relationship`;
        }
    });
}

function addParty(idx) {
    const tbody = document.getElementById('appendPartyHere');
    const index = tbody.children.length;

    const partyIdValue = document.getElementById(`partySearchResultPartyID_${idx}`)?.textContent || '';
    const nameValue = document.getElementById(`partySearchResultName_${idx}`)?.textContent || '';
    const furtherSpecsValue = document.getElementById(`partySearchResultFurtherSpecs_${idx}`)?.textContent || '';

    const tr = document.createElement('tr');
    tr.className = 'partyResultTableTr';

    const tdName = document.createElement("td");
    tdName.textContent = nameValue;

    const tdSpecs = document.createElement("td");
    tdSpecs.textContent = furtherSpecsValue;

    const tdInput = document.createElement('td');
    tdInput.setAttribute('scope', 'col');

    const inputRelationship = document.createElement('input');
    inputRelationship.name = `CollectionItemNPartyList[${index}].Relationship`;
    inputRelationship.required = true;
    inputRelationship.classList.add('form-control', 'partyResultTableRelationship');
    tdInput.appendChild(inputRelationship);

    const tdActions = document.createElement("td");

    let hiddenInput = document.createElement("input");
    hiddenInput.type = "hidden";
    hiddenInput.classList.add('form-control', 'partyResultTableId');
    hiddenInput.name = `CollectionItemNPartyList[${index}].PartyID`;
    hiddenInput.value = partyIdValue;

    const removeBtn = document.createElement("button");
    removeBtn.type = "button";
    removeBtn.classList.add("btn", "btn-danger", "btn-sm", "removePartyRow");
    removeBtn.textContent = i18n.get("Remove");

    tdActions.appendChild(hiddenInput);
    tdActions.appendChild(removeBtn);
    tr.appendChild(tdName);
    tr.appendChild(tdSpecs);
    tr.appendChild(tdInput);
    tr.appendChild(tdActions);
    tbody.appendChild(tr);

    hideModal('partyModal');
}
(function initRemovePartyButtonHandler() {
    const tableBody = document.getElementById('partyResultTableBody');
    if (!tableBody) return;

    tableBody.addEventListener('click', function (e) {
        if (e.target && e.target.classList.contains('removeParty')) {
            const trTag = e.target.closest('.partyResultTableTr');
            if (trTag) {
                trTag.remove();
                reindexPartyFields(tableBody);
            }
        }
    });
})();

function reindexPartyFields(container) {
    const entries = Array.from(container.querySelectorAll(".partyResultTableTr"));

    entries.forEach((entry, index) => {
        const partyIdInput = entry.querySelector(".partyResultTableId");
        if (partyIdInput) {
            partyIdInput.name = `CollectionItemNPartyList[${index}].PartyID`;
        }

        const relationshipInput = entry.querySelector(".partyResultTableRelationship");
        if (relationshipInput) {
            relationshipInput.name = `CollectionItemNPartyList[${index}].Relationship`;
        }
    });
}

function SetEraIntoTable(buttonId) {
    var value = document.getElementById('eraSearchResultID_' + buttonId).textContent;
    document.getElementById('CollectionItemEntity_EraID').value = value;
    value = document.getElementById('eraSearchResultName_' + buttonId).textContent;
    document.getElementById('eraName').innerText = value;

    document.getElementById('clearOneRowEraTable').style.display = 'inline';
    hideModal('EraModal');
}
$('#clearOneRowEraTable').on('click', function () {
    document.getElementById('CollectionItemEntity_EraID').value = ''
    document.getElementById('eraName').innerText = ''
    $('#clearOneRowEraTable').hide()
})

function addColor() {
    const container = document.getElementById("colorContainer");
    const index = container.children.length;

    const card = document.createElement("div");
    card.className = "card cardColor m-2";
    card.style.width = "18rem";

    const cardBody = document.createElement("div");
    cardBody.className = "card-body";

    const select = document.createElement("select");
    select.name = `CollectionItemNColorList[${index}].ColorID`;
    select.id = "selectColor" + index;
    select.className = "form-select selectColorID";

    const colorList = JSON.parse(sessionStorage.getItem('colorList') || '[]');
    const autocompleteSource = colorList.map(x => ({
        name: x.colorName,
        value: x.colorID
    }));
    const defaultOption = document.createElement("option");
    defaultOption.selected = true;
    defaultOption.value = "";
    defaultOption.textContent = i18n.get("Color_Select");
    select.appendChild(defaultOption);

    autocompleteSource.forEach((element) => {
        const option = document.createElement("option");
        option.selected = false;
        option.value = element.value;
        option.textContent = element.name;
        select.appendChild(option);
    });

    const mainColorDiv = document.createElement("div");
    mainColorDiv.className = "form-check";

    const mainColorCheckbox = document.createElement("input");
    mainColorCheckbox.type = "checkbox";
    mainColorCheckbox.className = "form-check-input checkboxPrimaryColor";
    mainColorCheckbox.name = `CollectionItemNColorList[${index}].IsPrimaryColor`;
    mainColorCheckbox.value = "true";
    mainColorCheckbox.id = `mainColorCheckbox${index}`;
    mainColorCheckbox.setAttribute("data-val", "true");

    const mainColorLabel = document.createElement("label");
    mainColorLabel.className = "form-check-label";
    mainColorLabel.setAttribute("for", `mainColorCheckbox${index}`);
    mainColorLabel.textContent = i18n.get("IsPrimaryColor");
    mainColorDiv.appendChild(mainColorCheckbox);
    mainColorDiv.appendChild(mainColorLabel);

    const removeButton = document.createElement("button");
    removeButton.type = "button";
    removeButton.className = "btn btn-danger removeColor";
    removeButton.textContent = i18n.get("Remove");

    cardBody.appendChild(select);
    cardBody.appendChild(mainColorDiv);
    cardBody.appendChild(removeButton);
    card.appendChild(cardBody);
    container.appendChild(card);
}
(function initRemoveColorButtonHandler() {
    const container = document.getElementById('colorContainer');
    if (!container) return;

    container.addEventListener('click', function (e) {
        if (e.target && e.target.classList.contains('removeColor')) {
            const trTag = e.target.closest('.cardColor');
            if (trTag) {
                trTag.remove();
                reindexConceptRelationFields(container);
            }
        }
    });
})();
function reindexConceptRelationFields(container) {
    const entries = Array.from(container.querySelectorAll(".cardColor"));

    entries.forEach((entry, index) => {
        const manufactoryIdInput = entry.querySelector(".selectColorID");
        if (manufactoryIdInput) {
            manufactoryIdInput.name = `CollectionItemNColorList[${index}].ColorID`;
        }
        const primaryColorCheckbox = entry.querySelector(".checkboxPrimaryColor");
        if (primaryColorCheckbox) {
            primaryColorCheckbox.name = `CollectionItemNColorList[${index}].IsPrimaryColor`;
        }
    });
}

function addMaterial() {
    const container = document.getElementById("materialContainer");
    const index = container.children.length;

    const card = document.createElement("div");
    card.className = "card cardMaterial m-2";
    card.style.width = "18rem";

    const cardBody = document.createElement("div");
    cardBody.className = "card-body";

    const select = document.createElement("select");
    select.name = `CollectionItemNMaterialList[${index}].MaterialID`;
    select.id = "selectMaterial" + index;
    select.className = "form-select selectMaterialID";

    const materialList = JSON.parse(sessionStorage.getItem('materialList') || '[]');
    const autocompleteSource = materialList.map(x => ({
        name: x.name,
        value: x.materialID
    }));
    const defaultOption = document.createElement("option");
    defaultOption.selected = true;
    defaultOption.value = "";
    defaultOption.textContent = i18n.get("Material_Select");
    select.appendChild(defaultOption);

    autocompleteSource.forEach((element) => {
        const option = document.createElement("option");
        option.selected = false;
        option.value = element.value;
        option.textContent = element.name;
        select.appendChild(option);
    });

    const mainMaterialDiv = document.createElement("div");
    mainMaterialDiv.className = "form-check";

    const mainMaterialCheckbox = document.createElement("input");
    mainMaterialCheckbox.type = "checkbox";
    mainMaterialCheckbox.className = "form-check-input checkboxPrimaryMaterial";
    mainMaterialCheckbox.name = `CollectionItemNMaterialList[${index}].IsPrimaryMaterial`;
    mainMaterialCheckbox.value = "true";
    mainMaterialCheckbox.id = `mainMaterialCheckbox${index}`; // <<< wichtig!
    mainMaterialCheckbox.setAttribute("data-val", "true");

    const mainMaterialLabel = document.createElement("label");
    mainMaterialLabel.className = "form-check-label";
    mainMaterialLabel.setAttribute("for", `mainMaterialCheckbox${index}`);
    mainMaterialLabel.textContent = i18n.get("IsPrimaryMaterial");
    mainMaterialDiv.appendChild(mainMaterialCheckbox);
    mainMaterialDiv.appendChild(mainMaterialLabel);

    const removeButton = document.createElement("button");
    removeButton.type = "button";
    removeButton.className = "btn btn-danger removeMaterial";
    removeButton.textContent = i18n.get("Remove");

    cardBody.appendChild(select);
    cardBody.appendChild(mainMaterialDiv);
    cardBody.appendChild(removeButton);
    card.appendChild(cardBody);
    container.appendChild(card);
}
(function initRemoveMaterialButtonHandler() {
    const container = document.getElementById('materialContainer');
    if (!container) return;

    container.addEventListener('click', function (e) {
        if (e.target && e.target.classList.contains('removeMaterial')) {
            const card = e.target.closest('.cardMaterial');
            if (card) {
                card.remove();
                reindexMaterialFields(container);
            }
        }
    });
})();
function reindexMaterialFields(container) {
    const entries = Array.from(container.querySelectorAll(".cardMaterial"));

    entries.forEach((entry, index) => {
        const materialIdInput = entry.querySelector(".selectMaterialID");
        if (materialIdInput) {
            materialIdInput.name = `CollectionItemNMaterialList[${index}].MaterialID`;
        }
        const primaryColorCheckbox = entry.querySelector(".checkboxPrimaryMaterial");
        if (primaryColorCheckbox) {
            primaryColorCheckbox.name = `CollectionItemNMaterialList[${index}].IsPrimaryMaterial`;
        }
    });
}

function addInputFormFile(sourcePage) {
    const perspective = [i18n.get("Side_Front"), i18n.get("Side_Back"), i18n.get("Side_Left"), i18n.get("Side_Right"), i18n.get("Side_Top"), i18n.get("Side_Bottom")]
    const container = document.getElementById("appendInputFormFile");
    const pictureCount = container.children.length;

    const div = document.createElement("div");
    div.className = "mb-3 inputGroupFormFile";
    div.id = `inputGroupFormFile_${pictureCount}`

    const label = document.createElement("label");
    label.className = "form-label";
    label.setAttribute("for", `CollectionItemPictureList_${pictureCount}__PerspectiveInt`);
    label.innerText = perspective[pictureCount];
    div.appendChild(label);

    if (sourcePage == "Edit") {
        const inputCollectionItemPictureID = document.createElement("input");
        inputCollectionItemPictureID.type = "hidden";
        inputCollectionItemPictureID.name = `CollectionItemPictureList[${pictureCount}].CollectionItemPictureID`;
        div.appendChild(inputCollectionItemPictureID);
    }

    const divInputGroup = document.createElement("div");
    divInputGroup.className = "input-group";

    const inputFile = document.createElement("input");
    inputFile.type = "file";
    inputFile.name = `CollectionItemPictureList[${pictureCount}].Datei`;
    inputFile.className = "form-control";
    divInputGroup.appendChild(inputFile);

    const removeButton = document.createElement("button");
    removeButton.type = "button";
    removeButton.className = "btn btn-danger removeFormFileInput";
    removeButton.textContent = i18n.get("Remove");
    divInputGroup.appendChild(removeButton);
    div.appendChild(divInputGroup);

    const inputPerspectiveInt = document.createElement("input");
    inputPerspectiveInt.type = "hidden";
    inputPerspectiveInt.name = `CollectionItemPictureList[${pictureCount}].PerspectiveInt`;
    inputPerspectiveInt.className = "form-control";
    inputPerspectiveInt.value = pictureCount;
    div.appendChild(inputPerspectiveInt);

    if (sourcePage == "Edit") {
        const inputCollectionItemEntityID = document.createElement("input");
        inputCollectionItemEntityID.type = "hidden";
        inputCollectionItemEntityID.name = `CollectionItemPictureList[${pictureCount}].CollectionItemEntityID`;
        inputPerspectiveInt.className = "form-control";
        inputCollectionItemEntityID.value = document.getElementById("CollectionItemEntity_CollectionItemEntityID").value;
        div.appendChild(inputCollectionItemEntityID);
    }

    container.appendChild(div);
}
(function initRemoveFormFileButtonHandler() {
    const container = document.getElementById('appendInputFormFile');
    if (!container) return;

    container.addEventListener('click', function (e) {
        if (e.target && e.target.classList.contains('removeFormFileInput')) {
            const inputgroup = e.target.closest('.inputGroupFormFile');
            if (inputgroup) {
                inputgroup.remove();
            }
        }
    });
})();

function removePicture(pictureCount) {
    const pictureCard = document.getElementById(`pictureCard_${pictureCount}`);
    const cardDiv = document.getElementById('cardDiv');
    cardDiv.removeChild(pictureCard);

    removeFormFileInput(pictureCount)
}

if (window.location.href.indexOf('Database') > -1) {
    $(function () {
        $(".chosen-select").chosen()
        $(".chosen").chosen()
    });
}
(function showPotentialTable() {
    const checkbox = document.getElementById('IsPartOfASeries');
    if (!checkbox) return;

    checkbox.addEventListener('click', function (e) {
        if (e.target && e.target.checked) {
            const potentialTable = document.getElementById('PotentialTable');
            if (potentialTable) {
                potentialTable.style.display = 'block';
            }
        }
    });
})();
function SetCollectionItemPotentialIntoTable(buttonId) {
    let value = document.getElementById(`CollectionItemPotentialSearchResultPotentialID_${buttonId}`).value;
    document.getElementById('CollectionItemPotentialID').innerText = value;

    document.getElementById(`ClearOneRowCollectionItemPotentialTable`).style.display = 'inline';
    hideModal('CollectionItemPotentialModal');
}
function clearOneRowProcessOfManufactureTable() {
    document.getElementById('ProcessOfManufacture_ProcessOfManufactureID').value = '';
    document.getElementById('ClearOneRowCollectionItemPotentialTable').style.display = 'none';
}