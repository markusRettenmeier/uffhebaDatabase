import { i18n } from '../TranslationService.js';
import { hideModal } from '../site.js';
function addConceptToCollectionItem(buttonId) {
    const tbody = document.getElementById('appendBoolConceptHere');
    const index = tbody.children.length;
    const conceptValueId = document.getElementById(`conceptSearchResultConceptValueID_${buttonId}`)?.textContent || '';
    const nameValue = document.getElementById(`conceptSearchResultName_${buttonId}`)?.textContent || '';
    const furtherSpecsValue = document.getElementById(`conceptSearchResultFurtherSpecs_${buttonId}`)?.textContent || '';
    const row = document.createElement("tr");
    row.className = `conceptTableRow`;
    const tdName = document.createElement("td");
    tdName.textContent = nameValue;
    const tdSpecs = document.createElement("td");
    tdSpecs.textContent = furtherSpecsValue;
    const tdActions = document.createElement("td");
    const hiddenInput = document.createElement("input");
    hiddenInput.type = "hidden";
    hiddenInput.classList.add('form-control', 'conceptResultTableId');
    hiddenInput.name = `ConceptValueList[${index}].ConceptValueID`;
    hiddenInput.value = conceptValueId;
    const inputConceptValue = document.createElement('input');
    inputConceptValue.type = "hidden";
    inputConceptValue.name = `ConceptValueList[${index}].ValueBool`;
    inputConceptValue.required = true;
    inputConceptValue.value = "on";
    inputConceptValue.classList.add('form-control', 'conceptResultTableValueBool');
    const removeBtn = document.createElement("button");
    removeBtn.type = "button";
    removeBtn.classList.add("btn", "btn-danger", "btn-sm", "removeConceptRow");
    removeBtn.textContent = i18n.get("Remove");
    tdActions.appendChild(hiddenInput);
    tdActions.appendChild(inputConceptValue);
    tdActions.appendChild(removeBtn);
    row.appendChild(tdName);
    row.appendChild(tdSpecs);
    row.appendChild(tdActions);
    tbody.appendChild(row);
    hideModal('ConceptModal');
}
(function initRemoveConceptButtonHandler() {
    const container = document.getElementById('appendBoolConceptHere');
    if (!container)
        return;
    container.addEventListener('click', function (e) {
        const target = e.target;
        if (target && target.classList.contains('removeConceptRow')) {
            const trTag = target.closest('.conceptTableRow');
            if (trTag) {
                trTag.remove();
                reindexChildConcepts(container);
            }
        }
    });
})();
function reindexChildConcepts(container) {
    const entries = Array.from(container.querySelectorAll(".conceptTableRow"));
    entries.forEach((entry, index) => {
        const ConceptValueIdInput = entry.querySelector(".conceptResultTableId");
        if (ConceptValueIdInput) {
            ConceptValueIdInput.name = `ConceptValueList[${index}].ConceptValueID`;
        }
        const relationshipInput = entry.querySelector(".conceptResultTableValueBool");
        if (relationshipInput) {
            relationshipInput.name = `ConceptValueList[${index}].ValueBool`;
        }
    });
}
//function addPlace(idx: number): void {
//    const tbody = document.getElementById('appendPlaceHere') as HTMLTableSectionElement;
//    const index = tbody.children.length;
//    const placeIdValue = document.getElementById(`placeSearchResultPlaceID_${idx}`)?.textContent || '';
//    const toponymValue = document.getElementById(`placeSearchResultToponym_${idx}`)?.textContent || '';
//    const furtherSpecsValue = document.getElementById(`placeSearchResultFurtherSpecs_${idx}`)?.textContent || '';
//    const row = document.createElement("tr");
//    row.className = `placeTableRow`;
//    const tdName = document.createElement("td");
//    tdName.textContent = toponymValue;
//    const tdSpecs = document.createElement("td");
//    tdSpecs.textContent = furtherSpecsValue;
//    const tdActions = document.createElement("td");
//    const hiddenInput = document.createElement("input");
//    hiddenInput.type = "hidden";
//    hiddenInput.classList.add('form-control', 'placeResultTableId');
//    hiddenInput.name = `CollectionItemNPlaceList[${index}].PlaceID`;
//    hiddenInput.value = placeIdValue;
//    const tdInput = document.createElement('td');
//    tdInput.setAttribute('scope', 'col');
//    const inputRelationship = document.createElement('input');
//    inputRelationship.name = `CollectionItemNPlaceList[${index}].Relationship`;
//    inputRelationship.required = true;
//    inputRelationship.classList.add('form-control', 'placeResultTableRelationship');
//    tdInput.appendChild(inputRelationship);
//    const removeBtn = document.createElement("button");
//    removeBtn.type = "button";
//    removeBtn.classList.add("btn", "btn-danger", "btn-sm", "removePlaceRow");
//    removeBtn.textContent = i18n.get("Remove");
//    tdActions.appendChild(hiddenInput);
//    tdActions.appendChild(removeBtn);
//    row.appendChild(tdName);
//    row.appendChild(tdSpecs);
//    row.appendChild(tdInput);
//    row.appendChild(tdActions);
//    tbody.appendChild(row);
//    hideModal('placeModal');
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
//    const entries = Array.from(container.querySelectorAll(".placeTableRow"));
//    entries.forEach((entry: Element, index: number) => {
//        const placeIdInput = entry.querySelector(".placeResultTableId") as HTMLInputElement;
//        if (placeIdInput) {
//            placeIdInput.name = `CollectionItemNPlaceList[${index}].PlaceID`;
//        }
//        const relationshipInput = entry.querySelector(".placeResultTableRelationship") as HTMLInputElement;
//        if (relationshipInput) {
//            relationshipInput.name = `CollectionItemNPlaceList[${index}].Relationship`;
//        }
//    });
//}
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
    const hiddenInput = document.createElement("input");
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
    if (!tableBody)
        return;
    tableBody.addEventListener('click', function (e) {
        const target = e.target;
        if (target && target.classList.contains('removeParty')) {
            const trTag = target.closest('.partyResultTableTr');
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
export function SetEraIntoTable(buttonId) {
    let value = document.getElementById('eraSearchResultID_' + buttonId).textContent;
    document.getElementById('CollectionItemEntity_EraID').value = value;
    value = document.getElementById('eraSearchResultName_' + buttonId).textContent;
    document.getElementById('eraName').innerText = value;
    document.getElementById('clearOneRowEraTable').style.display = 'inline';
    hideModal('EraModal');
}
const clearEraButton = document.getElementById('clearOneRowEraTable');
if (clearEraButton) {
    clearEraButton.addEventListener('click', function () {
        document.getElementById('CollectionItemEntity_EraID').value = '';
        document.getElementById('eraName').innerText = '';
        clearEraButton.style.display = 'none';
    });
}
function addFormFileCollectionItem(sourcePage) {
    const perspective = [
        i18n.get("Side_Front"),
        i18n.get("Side_Back"),
        i18n.get("Side_Left"),
        i18n.get("Side_Right"),
        i18n.get("Side_Top"),
        i18n.get("Side_Bottom")
    ];
    const container = document.getElementById("appendInputFormFile");
    const pictureCount = container.children.length;
    const div = document.createElement("div");
    div.className = "mb-3 inputGroupFormFile";
    div.id = `inputGroupFormFile_${pictureCount}`;
    const label = document.createElement("label");
    label.className = "form-label";
    label.setAttribute("for", `CollectionItemPictureList_${pictureCount}__PerspectiveInt`);
    label.innerText = perspective[pictureCount] || i18n.get("Side_Other");
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
    inputPerspectiveInt.value = pictureCount.toString();
    div.appendChild(inputPerspectiveInt);
    if (sourcePage == "Edit") {
        const inputCollectionItemEntityID = document.createElement("input");
        inputCollectionItemEntityID.type = "hidden";
        inputCollectionItemEntityID.name = `CollectionItemPictureList[${pictureCount}].CollectionItemEntityID`;
        inputCollectionItemEntityID.className = "form-control";
        const collectionItemEntityIdInput = document.getElementById("CollectionItemEntity_CollectionItemEntityID");
        inputCollectionItemEntityID.value = collectionItemEntityIdInput.value;
        div.appendChild(inputCollectionItemEntityID);
    }
    container.appendChild(div);
}
function addFormFileOwnershipProof(sourcePage) {
    const container = document.getElementById("appendInputFormFileOwnershipProof");
    const pictureCount = container.children.length;
    const div = document.createElement("div");
    div.className = "mb-3 inputGroupFormFileOwnershipProof";
    div.id = `inputGroupFormFileOwnershipProof_${pictureCount}`;
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
    const selectType = document.createElement("select");
    selectType.name = `OwnershipProofPictureList[${pictureCount}].PerspectiveInt`;
    selectType.className = "form-select";
    div.appendChild(selectType);
    const ownershipProofTypes = [
        "OwnershipProof_Type_BillOfSale",
        "OwnershipProof_Type_Certificate",
        "OwnershipProof_Type_Other"
    ];
    ownershipProofTypes.forEach((element, index) => {
        const option = document.createElement("option");
        option.value = index.toString();
        option.text = i18n.get(element);
        selectType.appendChild(option);
    });
    if (sourcePage == "Edit") {
        const inputCollectionItemEntityID = document.createElement("input");
        inputCollectionItemEntityID.type = "hidden";
        inputCollectionItemEntityID.name = `OwnershipProofPictureList[${pictureCount}].CollectionItemEntityID`;
        inputCollectionItemEntityID.className = "form-control";
        const collectionItemEntityIdInput = document.getElementById("CollectionItemEntity_CollectionItemEntityID");
        inputCollectionItemEntityID.value = collectionItemEntityIdInput.value;
        div.appendChild(inputCollectionItemEntityID);
    }
    container.appendChild(div);
}
(function initRemoveFormFileButtonHandler() {
    const container = document.getElementById('appendInputFormFile');
    if (!container)
        return;
    container.addEventListener('click', function (e) {
        const target = e.target;
        if (target && target.classList.contains('removeFormFileInput')) {
            const inputgroup = target.closest('.inputGroupFormFile');
            if (inputgroup) {
                inputgroup.remove();
            }
        }
    });
})();
function removePicture(pictureCount) {
    const pictureCard = document.getElementById(`pictureCard_${pictureCount}`);
    const cardDiv = document.getElementById('cardDiv');
    if (pictureCard && cardDiv.contains(pictureCard)) {
        cardDiv.removeChild(pictureCard);
    }
    removeFormFileInput(pictureCount);
}
function removeFormFileInput(pictureCount) {
    const appendInputFormFile = document.getElementById('appendInputFormFile');
    const inputGroupFormFile = document.getElementById(`inputGroupFormFile_${pictureCount}`);
    if (appendInputFormFile && inputGroupFormFile && appendInputFormFile.contains(inputGroupFormFile)) {
        appendInputFormFile.removeChild(inputGroupFormFile);
    }
}
export function SetSetIntoTable(buttonId) {
    const setIDElement = document.getElementById(`SetSearchResultSetID_${buttonId}`);
    const setNameElement = document.getElementById(`SetSearchResultSetName_${buttonId}`);
    const idValue = setIDElement.textContent || '';
    const nameValue = setNameElement.textContent || '';
    document.getElementById('CollectionItemEntity_SetID').innerText = idValue;
    document.getElementById('SetName').innerText = nameValue;
    document.getElementById('ClearOneRowSetTable').style.display = 'inline';
    hideModal('SetModal');
}
if (typeof window !== 'undefined') {
    window.addConceptToCollectionItem = addConceptToCollectionItem;
    //window.addPlace = addPlace;
    window.addParty = addParty;
    window.SetEraIntoTable = SetEraIntoTable;
    window.addFormFileCollectionItem = addFormFileCollectionItem;
    window.addFormFileOwnershipProof = addFormFileOwnershipProof;
    window.removePicture = removePicture;
}
