function SetBricknameIntoTable(buttonId) {
    let value = document.getElementById('bricknameSearchResultBrickPotentialID_' + buttonId).innerText;
    document.getElementById('bricknameBrickPotentialID').innerText = value;
    document.getElementById('bricknameBrickPotentialID_Input').value = value;
    value = document.getElementById('bricknameSearchResultName_' + buttonId).innerText;
    document.getElementById('bricknameName').innerText = value;
    value = document.getElementById('bricknameSearchResultUsage_' + buttonId).innerText;
    document.getElementById('bricknameUsageEnum').innerText = value;

    document.getElementById('clearOneRowBricknameTable').style.display = 'inline';
    hideModal('IsBricknameExistingModal');
}
const clearOneRowBricknameTableToggle = document.getElementById('clearOneRowBricknameTable')
if (clearOneRowBricknameTableToggle) {
    clearOneRowBricknameTableToggle.addEventListener('click', function () {
        document.getElementById('bricknameBrickPotentialID').innerText = ''
        document.getElementById('bricknameBrickPotentialID_Input').value = ''
        document.getElementById('bricknameName').innerText = ''
        document.getElementById('bricknameUsageEnum').innerText = ''
        document.getElementById('clearOneRowBricknameTable').style.display = 'none';
    })
}

function SetProcessOfManufactureIntoTable(buttonId) {
    let value = document.getElementById('processOfManufactureSearchResultprocessOfManufactureID_' + buttonId).innerHTML;
    document.getElementById('processOfManufactureProcessOfManufactureID').innerText = value;
    document.getElementById('processOfManufactureProcessOfManufactureID_Input').value = value;
    value = document.getElementById('processOfManufactureSearchResultMainprocess_' + buttonId).innerText;
    document.getElementById('processOfManufactureMainprocess').innerText = value;
    value = document.getElementById('processOfManufactureSearchResultProcessOfManufactureName_' + buttonId).innerText;
    document.getElementById('processOfManufactureProcessOfManufactureName').innerText = value;
    value = document.getElementById('processOfManufactureSearchResultTechnique_' + buttonId).innerText;
    document.getElementById('processOfManufactureTechnique').innerText = value;
    value = document.getElementById('processOfManufactureSearchResultDescription_' + buttonId).innerText;
    document.getElementById('processOfManufactureDescription').innerText = value;

    document.getElementById(`clearOneRowProcessOfManufactureTableButton`).style.display = 'inline';
    hideModal('processOfManufactureModal');
}
function clearOneRowProcessOfManufactureTable() {
    document.getElementById('processOfManufactureProcessOfManufactureID').innerText = ''
    document.getElementById('processOfManufactureProcessOfManufactureID_Input').value = ''
    document.getElementById('processOfManufactureMainprocess').innerText = ''
    document.getElementById('processOfManufactureProcessOfManufactureName').innerText = ''
    document.getElementById('processOfManufactureTechnique').innerText = ''
    document.getElementById('processOfManufactureDescription').innerText = ''
    document.getElementById(`clearOneRowProcessOfManufactureTableButton`).style.display = 'none';
}

function SetCityIntoTableBrickEntity(buttonId) {
    const tbody = document.getElementById('cityResultTableBody');
    const index = tbody.children.length;

    const cityIdValue = document.getElementById('citySearchResultcityID_' + buttonId)?.textContent || '';
    const oeconymValue = document.getElementById('citySearchResultOecoynm_' + buttonId)?.textContent || '';
    const postalcodeValue = document.getElementById('citySearchResultPostalcode_' + buttonId)?.textContent || '';
    const bynameValue = document.getElementById('citySearchResultByname_' + buttonId)?.textContent || '';
    const geographyValue = document.getElementById('citySearchResultGeography_' + buttonId)?.textContent || '';

    const tr = document.createElement('tr');
    tr.id = 'cityResultTableTr_' + index;
    tr.className = 'cityResultTableTr';

    const tdId = document.createElement('td');
    tdId.setAttribute('scope', 'col');
    tdId.textContent = cityIdValue;

    const hiddenInput = document.createElement('input');
    hiddenInput.className = 'cityResultTableId';
    hiddenInput.type = 'hidden';
    hiddenInput.name = `BrickEntityNCityList[${index}].CityID`;
    hiddenInput.value = cityIdValue;
    tdId.appendChild(hiddenInput);
    tr.appendChild(tdId);

    const tdOeconym = document.createElement('td');
    tdOeconym.setAttribute('scope', 'col');
    tdOeconym.textContent = oeconymValue;
    tr.appendChild(tdOeconym);

    const tdPostalcode = document.createElement('td');
    tdPostalcode.setAttribute('scope', 'col');
    tdPostalcode.textContent = postalcodeValue;
    tr.appendChild(tdPostalcode);

    const tdByname = document.createElement('td');
    tdByname.setAttribute('scope', 'col');
    tdByname.textContent = bynameValue;
    tr.appendChild(tdByname);

    const tdGeography = document.createElement('td');
    tdGeography.setAttribute('scope', 'col');
    tdGeography.textContent = geographyValue;
    tr.appendChild(tdGeography);

    const tdInput = document.createElement('td');
    tdInput.setAttribute('scope', 'col');

    const input = document.createElement('input');
    input.name = `BrickEntityNCityList[${index}].Relationship`;
    input.required = true;
    input.className = 'form-control cityResultTableRelationship';
    tdInput.appendChild(input);
    tr.appendChild(tdInput);

    const tdButton = document.createElement('td');
    tdButton.setAttribute('scope', 'col');

    const removeButton = document.createElement('button');
    removeButton.type = 'button';
    removeButton.className = 'btn btn-outline-danger';
    removeButton.textContent = 'Entfernen';
    removeButton.onclick = function () {
        removeCity(`${index}`);
    };
    tdButton.appendChild(removeButton);
    tr.appendChild(tdButton);

    tbody.appendChild(tr);

    const modal = document.getElementById('IsCityExistingModal');
    hideModal(modal);
}
function removeCity(id) {
    const container = document.getElementById("cityResultTableBody");
    const trTag = document.getElementById("cityResultTableTr_" + id);
    if (trTag) {
        trTag.remove();
        reindexCityFields(container);
    }
}
function reindexCityFields(container) {
    const entries = Array.from(container.querySelectorAll(".cityResultTableTr"));

    entries.forEach((entry, index) => {
        entry.id = `cityResultTableBody${index}`;

        const cityIdInput = entry.querySelector(".cityResultTableId");
        if (cityIdInput) {
            cityIdInput.name = `BrickEntityNCityList[${index}].CityID`;
        }

        const relationshipInput = entry.querySelector(".cityResultTableRelationship");
        if (relationshipInput) {
            relationshipInput.name = `BrickEntityNCityList[${index}].Relationship`;
        }
    });
}

function SetPersonIntoTable(buttonId) {
    const tbody = document.getElementById('personResultTableBody');
    const index = tbody.children.length;

    const personIdValue = document.getElementById('personSearchResultPersonID_' + buttonId)?.textContent || '';
    const nameValue = document.getElementById('personSearchResultName_' + buttonId)?.textContent || '';
    const signatureValue = document.getElementById('personSearchResultPersonSignature_' + buttonId)?.textContent || '';
    const pseudonymValue = document.getElementById('personSearchResultPersonPseudonym_' + buttonId)?.textContent || '';

    const tr = document.createElement('tr');
    tr.className = 'personResultTableTr';

    const tdId = document.createElement('td');
    tdId.setAttribute('scope', 'col');
    tdId.textContent = personIdValue;

    const hiddenInput = document.createElement('input');
    hiddenInput.className = 'personResultTableId';
    hiddenInput.type = 'hidden';
    hiddenInput.name = `BrickEntityNPersonList[${index}].PersonID`;
    hiddenInput.value = personIdValue;
    tdId.appendChild(hiddenInput);
    tr.appendChild(tdId);

    const tdName = document.createElement('td');
    tdName.setAttribute('scope', 'col');
    tdName.textContent = nameValue;
    tr.appendChild(tdName);

    const tdSignature = document.createElement('td');
    tdSignature.setAttribute('scope', 'col');
    tdSignature.textContent = signatureValue;
    tr.appendChild(tdSignature);

    const tdPseudonym = document.createElement('td');
    tdPseudonym.setAttribute('scope', 'col');
    tdPseudonym.textContent = pseudonymValue;
    tr.appendChild(tdPseudonym);

    const tdInput = document.createElement('td');
    tdInput.setAttribute('scope', 'col');

    const input = document.createElement('input');
    input.name = `BrickEntityNPersonList[${index}].Relationship`;
    input.required = true;
    input.className = 'form-control personResultTableRelationship';
    tdInput.appendChild(input);
    tr.appendChild(tdInput);

    const tdButton = document.createElement('td');
    tdButton.setAttribute('scope', 'col');

    const removeButton = document.createElement('button');
    removeButton.type = 'button';
    removeButton.className = 'btn btn-outline-danger removePerson';
    removeButton.textContent = 'Entfernen';
    tdButton.appendChild(removeButton);
    tr.appendChild(tdButton);

    tbody.appendChild(tr);

    hideModal('IsPersonExistingModal');
}
(function initRemovePersonButtonHandler() {
    const tableBody = document.getElementById('personResultTableBody');
    if (!tableBody) return;

    tableBody.addEventListener('click', function (e) {
        if (e.target && e.target.classList.contains('removePerson')) {
            const trTag = e.target.closest('.personResultTableTr');
            if (trTag) {
                trTag.remove();
                reindexPersonFields(tableBody);
            }
        }
    });
})();

function reindexPersonFields(container) {
    const entries = Array.from(container.querySelectorAll(".personResultTableTr"));

    entries.forEach((entry, index) => {
        entry.id = `personResultTableBody${index}`;

        const personIdInput = entry.querySelector(".personResultTableId");
        if (personIdInput) {
            personIdInput.name = `BrickEntityNPersonList[${index}].PersonID`;
        }

        const relationshipInput = entry.querySelector(".personResultTableRelationship");
        if (relationshipInput) {
            relationshipInput.name = `BrickEntityNPersonList[${index}].Relationship`;
        }
    });
}

function SetEraIntoTable(buttonId) {
    var value = document.getElementById('eraSearchResultEraID_' + buttonId).innerHTML;
    document.getElementById('eraID_Table').innerText = value;
    document.getElementById('eraID_Input').value = value;
    value = document.getElementById('eraSearchEraName_' + buttonId).innerHTML;
    document.getElementById('eraName').innerText = value;

    document.getElementById('clearOneRowEraTable').style.display = 'inline';
    hideModal('EraModal');
}
$('#clearOneRowEraTable').on('click', function () {
    document.getElementById('eraID_Table').innerText = ''
    document.getElementById('eraID_Input').innerText = ''
    document.getElementById('eraName').innerText = ''
    $('#clearOneRowEraTable').hide()
})

function addManufactory() {
    const container = document.getElementById("manufactoryContainer");
    const index = container.children.length;

    const wrapper = document.createElement("div");
    wrapper.className = "input-group inputDivManufactory mb-2";

    const hiddenInput = document.createElement("input");
    hiddenInput.type = "hidden";
    hiddenInput.className = "form-control inputManufactoryID";
    hiddenInput.name = `BrickEntityNManufactoryNCityList[${index}].ManufactoryID`;

    const textInput = document.createElement("input");
    textInput.type = "text";
    textInput.className = "form-control inputManufactory";
    textInput.placeholder = "Verlag";

    const select = document.createElement("select");
    select.className = "form-select townOfManufactorySelect";
    select.name = `BrickEntityNManufactoryNCityList[${index}].CityID`;

    const defaultOption = document.createElement("option");
    defaultOption.value = "";
    defaultOption.textContent = "Ort";
    select.appendChild(defaultOption);

    const removeButton = document.createElement("button");
    removeButton.type = "button";
    removeButton.className = "btn btn-danger removeManufactory";
    removeButton.textContent = "Entfernen";
    wrapper.appendChild(hiddenInput);
    wrapper.appendChild(textInput);
    wrapper.appendChild(select);
    wrapper.appendChild(removeButton);

    container.appendChild(wrapper);
}
(function initRemoveManufactoryButtonHandler() {
    const container = document.getElementById('manufactoryContainer');

    if (!container) return; // Frühzeitiger Ausstieg, wenn Tabelle nicht vorhanden

    container.addEventListener('click', function (e) {
        if (e.target && e.target.classList.contains('removeManufactory')) {
            const trTag = e.target.closest('.inputDivManufactory');
            if (trTag) {
                trTag.remove();
                reindexManufactoryFields(tableBody);
            }
        }
    });
})();
function reindexManufactoryFields(container) {
    const entries = Array.from(container.querySelectorAll(".inputDivManufactory"));

    entries.forEach((entry, index) => {
        entry.id = `inputDiv_manufactory${index}`;

        const manufactoryIdInput = entry.querySelector(".inputManufactoryID");
        if (manufactoryIdInput) {
            manufactoryIdInput.name = `BrickEntityNManufactoryNCityList[${index}].ManufactoryID`;
        }

        const citySelect = entry.querySelector(".townOfManufactorySelect");
        if (citySelect) {
            citySelect.name = `BrickEntityNManufactoryNCityList[${index}].CityID`;
        }
    });
}

function addColor() {
    const container = document.getElementById("colorContainer");
    const index = container.children.length;

    const card = document.createElement("div");
    card.className = "card cardColor m-2";
    card.style.width = "18rem";

    const cardBody = document.createElement("div");
    cardBody.className = "card-body";

    const select = document.createElement("select");
    select.name = `ProductNColorVariantList[${index}].ColorID`;
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
    defaultOption.textContent = "Farbe wählen";
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
    mainColorCheckbox.name = `ProductNColorVariantList[${index}].IsPrimaryColor`;
    mainColorCheckbox.value = "true";
    mainColorCheckbox.id = `mainColorCheckbox${index}`; // <<< wichtig!
    mainColorCheckbox.setAttribute("data-val", "true");

    const mainColorLabel = document.createElement("label");
    mainColorLabel.className = "form-check-label";
    mainColorLabel.setAttribute("for", `mainColorCheckbox${index}`);
    mainColorLabel.textContent = "Hauptfarbe";
    mainColorDiv.appendChild(mainColorCheckbox);
    mainColorDiv.appendChild(mainColorLabel);

    const noteInput = document.createElement("input");
    noteInput.type = "text";
    noteInput.className = "form-control inputColorNote";
    noteInput.name = `ProductNColorVariantList[${index}].Note`;
    noteInput.placeholder = "Notiz";

    const removeButton = document.createElement("button");
    removeButton.type = "button";
    removeButton.className = "btn btn-danger removeColor";
    removeButton.textContent = "Entfernen";

    cardBody.appendChild(select);
    cardBody.appendChild(noteInput);
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
                reindexManufactoryFields(container);
            }
        }
    });
})();
function reindexMaterialFields(container) {
    const entries = Array.from(container.querySelectorAll(".cardColor"));

    entries.forEach((entry, index) => {
        const manufactoryIdInput = entry.querySelector(".selectColorID");
        if (manufactoryIdInput) {
            manufactoryIdInput.name = `ProductNColorVariantList[${index}].ColorID`;
        }
        const primaryColorCheckbox = entry.querySelector(".checkboxPrimaryColor");
        if (primaryColorCheckbox) {
            primaryColorCheckbox.name = `ProductNColorVariantList[${index}].IsPrimaryColor`;
        }
        const noteInput = entry.querySelector(".inputColorNote");
        if (noteInput) {
            noteInput.name = `ProductNColorVariantList[${index}].Note`;
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
    select.name = `ProductNMaterialList[${index}].MaterialID`;
    select.id = "selectMaterial" + index;
    select.className = "form-select selectMaterialID";

    const materialList = JSON.parse(sessionStorage.getItem('materialList') || '[]');
    const autocompleteSource = materialList.map(x => ({
        name: x.materialName,
        value: x.materialID
    }));
    const defaultOption = document.createElement("option");
    defaultOption.selected = true;
    defaultOption.value = "";
    defaultOption.textContent = "Farbe wählen";
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
    mainMaterialCheckbox.name = `ProductNMaterialList[${index}].IsPrimaryMaterial`;
    mainMaterialCheckbox.value = "true";
    mainMaterialCheckbox.id = `mainMaterialCheckbox${index}`; // <<< wichtig!
    mainMaterialCheckbox.setAttribute("data-val", "true");
    mainMaterialCheckbox.setAttribute("data-val-required", "Bitte wählen Sie aus, ob es das Hauptmaterial ist.");

    const mainMaterialLabel = document.createElement("label");
    mainMaterialLabel.className = "form-check-label";
    mainMaterialLabel.setAttribute("for", `mainMaterialCheckbox${index}`);
    mainMaterialLabel.textContent = "Hauptmaterial";
    mainMaterialDiv.appendChild(mainMaterialCheckbox);
    mainMaterialDiv.appendChild(mainMaterialLabel);

    const removeButton = document.createElement("button");
    removeButton.type = "button";
    removeButton.className = "btn btn-danger removeMaterial";
    removeButton.textContent = "Entfernen";

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
            const card = e.target.closest('.cardColor');
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
        const manufactoryIdInput = entry.querySelector(".selectMaterialID");
        if (manufactoryIdInput) {
            manufactoryIdInput.name = `ProductNMaterialList[${index}].MaterialID`;
        }
        const primaryColorCheckbox = entry.querySelector(".checkboxPrimaryMaterial");
        if (primaryColorCheckbox) {
            primaryColorCheckbox.name = `ProductNMaterialList[${index}].IsPrimaryMaterial`;
        }
    });
}

function addInputFormFile(sourcePage) {
    const perspective = ["Vorderseite", "Rückseite", "Linke Seite", "Rechte Seite"]
    const container = document.getElementById("appendInputFormFile");
    const pictureCount = container.children.length;

    const div = document.createElement("div");
    div.className = "mb-3 inputGroupFormFile";
    div.id = `inputGroupFormFile_${pictureCount}`

    const label = document.createElement("label");
    label.className = "form-label";
    label.setAttribute("for", `ProductPictureList_${pictureCount}__PerspectiveInt`);
    label.innerText = perspective[pictureCount];
    div.appendChild(label);

    if (sourcePage == "Edit") {
        const inputProductPictureID = document.createElement("input");
        inputProductPictureID.type = "hidden";
        inputProductPictureID.name = `ProductPictureList[${pictureCount}].ProductPictureID`;
        div.appendChild(inputProductPictureID);
    }

    const divInputGroup = document.createElement("div");
    divInputGroup.className = "input-group";

    const inputFile = document.createElement("input");
    inputFile.type = "file";
    inputFile.name = `ProductPictureList[${pictureCount}].Datei`;
    inputFile.className = "form-control";
    divInputGroup.appendChild(inputFile);

    const removeButton = document.createElement("button");
    removeButton.type = "button";
    removeButton.className = "btn btn-danger removeFormFileInput";
    removeButton.textContent = "Entfernen";
    divInputGroup.appendChild(removeButton);
    div.appendChild(divInputGroup);

    const inputPerspectiveInt = document.createElement("input");
    inputPerspectiveInt.type = "hidden";
    inputPerspectiveInt.name = `ProductPictureList[${pictureCount}].PerspectiveInt`;
    inputPerspectiveInt.className = "form-control";
    inputPerspectiveInt.value = pictureCount;
    div.appendChild(inputPerspectiveInt);

    if (sourcePage == "Edit") {
        const inputBrickEntityID = document.createElement("input");
        inputBrickEntityID.type = "hidden";
        inputBrickEntityID.name = `ProductPictureList[${pictureCount}].BrickEntityID`;
        inputPerspectiveInt.className = "form-control";
        inputBrickEntityID.value = document.getElementById("BrickEntity_BrickEntityID").value;
        div.appendChild(inputBrickEntityID);
    }

    container.appendChild(div);
}
//function removeFormFileInput(pictureCount) {
//    const appendInputFormFile = document.getElementById('appendInputFormFile');
//    const inputGroupFormFile = document.getElementById(`inputGroupFormFile_${pictureCount}`)
//    appendInputFormFile.removeChild(inputGroupFormFile);
//}
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