import { i18n } from '../TranslationService';
import { autocomplete, hideModal } from '../helperFunctions';
import { getCIRelationshipList } from '../api';

function Testfunciton(): void {
  console.log("Test");
}

document.addEventListener('input', (event: Event) => {
  const target = event.target as HTMLInputElement;

  if (target.classList?.contains('participantResultTableRelationship') || target.classList?.contains('placeRelationship')) {
    handleRelationshipInput(target);
  }
});
async function handleRelationshipInput(inputElement: HTMLInputElement): Promise<void> {
  const value: string = inputElement.value.trim();

  if (value !== '') {
    let relationshipJson = sessionStorage.getItem('ciRelationshipList');
    if (!relationshipJson) {
      relationshipJson = await getCIRelationshipList();
    }

    let relationshipArray: string[] = [];
    try {
      relationshipArray = relationshipJson ? JSON.parse(relationshipJson) : [];
    }
    catch {
      relationshipArray = [];
    }
    autocomplete(inputElement, relationshipArray);
  }
}

function addConceptToCollectionItem(buttonId: number): void {
  const nonBinaryEntries = Array.from(document.querySelectorAll(".conceptValueNonBinaryDiv"));
  const countNonBinaries = nonBinaryEntries.length;
  const tbody = document.getElementById('appendBoolConceptHere') as HTMLTableSectionElement;
  const index = tbody.children.length + countNonBinaries;

  const conceptId = document.getElementById(`conceptSearchResultConceptID_${buttonId}`)?.textContent || '';
  const nameValue = document.getElementById(`conceptSearchResultName_${buttonId}`)?.textContent || '';
  const furtherSpecsValue = document.getElementById(`conceptSearchResultFurtherSpecs_${buttonId}`)?.textContent || '';

  const row = document.createElement("tr");
  row.className = `conceptTableRow`;

  const tdName = document.createElement("td");
  tdName.textContent = nameValue;

  const tdSpecs = document.createElement("td");
  tdSpecs.textContent = furtherSpecsValue;

  const tdActions = document.createElement("td");

  const conceptIdInput = document.createElement("input");
  conceptIdInput.type = "hidden";
  conceptIdInput.classList.add('form-control', 'conceptId');
  conceptIdInput.name = `ConceptValueList[${index}].ConceptID`;
  conceptIdInput.value = conceptId;

  const inputConceptValue = document.createElement('input');
  inputConceptValue.type = "hidden";
  inputConceptValue.name = `ConceptValueList[${index}].ValueBool`;
  inputConceptValue.value = "true";
  inputConceptValue.classList.add('form-control', 'conceptResultTableValueBool');

  const removeBtn = document.createElement("button");
  removeBtn.type = "button";
  removeBtn.classList.add("btn", "btn-danger", "btn-sm", "removeConceptRow");
  removeBtn.textContent = i18n.get("Remove");

  tdActions.appendChild(conceptIdInput);
  tdActions.appendChild(inputConceptValue);
  tdActions.appendChild(removeBtn);

  row.appendChild(tdName);
  row.appendChild(tdSpecs);
  row.appendChild(tdActions);
  tbody.appendChild(row);

  hideModal('ConceptModal');
}
(function initRemoveConceptButtonHandler(): void {
  const container = document.getElementById('appendBoolConceptHere');
  if (!container) return;

  container.addEventListener('click', function (e: Event) {
    const target = e.target as HTMLElement;
    if (target && target.classList.contains('removeConceptRow')) {
      const trTag = target.closest('.conceptTableRow') as HTMLTableRowElement;
      if (trTag) {
        trTag.remove();
        reindexChildConcepts(container as HTMLTableSectionElement);
      }
    }
  });
})();
function reindexChildConcepts(container: HTMLTableSectionElement): void {
  const binaryEntries = Array.from(container.querySelectorAll(".conceptTableRow"));
  const nonBinaryEntries = Array.from(document.querySelectorAll(".conceptValueNonBinaryDiv"));
  const countNonBinaries = nonBinaryEntries.length;

  binaryEntries.forEach((entry: Element, index: number) => {
    const ConceptValueIdInput = entry.querySelector(".conceptValueId") as HTMLInputElement;
    if (ConceptValueIdInput) {
      ConceptValueIdInput.name = `ConceptValueList[${index + countNonBinaries}].ConceptValueID`;
    }
    const ConceptIdInput = entry.querySelector(".conceptId") as HTMLInputElement;
    if (ConceptIdInput) {
      ConceptIdInput.name = `ConceptValueList[${index + countNonBinaries}].ConceptID`;
    }
    const relationshipInput = entry.querySelector(".conceptValueBool") as HTMLInputElement;
    if (relationshipInput) {
      relationshipInput.name = `ConceptValueList[${index + countNonBinaries}].ValueBool`;
    }
  });
}

function addParticipant(idx: number): void {
  const tbody = document.getElementById('appendParticipantHere') as HTMLTableSectionElement;
  const index = tbody.children.length;

  const participantIdValue = document.getElementById(`participantSearchResultParticipantID_${idx}`)?.textContent || '';
  const nameValue = document.getElementById(`participantSearchResultName_${idx}`)?.textContent || '';
  const furtherSpecsValue = document.getElementById(`participantSearchResultFurtherSpecs_${idx}`)?.textContent || '';

  const tr = document.createElement('tr');
  tr.className = 'participantResultTableTr';
  tr.setAttribute('data-type', 'child');

  const tdName = document.createElement("td");
  tdName.textContent = nameValue;

  const tdSpecs = document.createElement("td");
  tdSpecs.textContent = furtherSpecsValue;

  const tdInput = document.createElement('td');
  tdInput.setAttribute('scope', 'col');

  const relationInput = document.createElement('input');
  relationInput.id = `ParticipantToCollectionItemList_${index}__Relationship`;
  relationInput.type = "text";
  relationInput.classList.add('form-control', 'participantResultTableRelationship');
  relationInput.name = `ConnectedParticipantList[${index}].Relationship`;
  relationInput.setAttribute("data-val", "true");
  relationInput.setAttribute("data-val-required", i18n.get("Error_Relationship_Required"));
  relationInput.setAttribute("aria-required", "true");
  relationInput.setAttribute("aria-invalid", "false");
  relationInput.setAttribute("aria-describedby", `ConnectedParticipantList_${index}__Relationship-error`);
  tdInput.appendChild(relationInput);

  const relationshipValidationSpan = document.createElement('span');
  relationshipValidationSpan.classList.add("text-danger", "field-validation-valid");
  relationshipValidationSpan.setAttribute('data-valmsg-for', `ConnectedParticipantList[${index}].Relationship`);
  relationshipValidationSpan.setAttribute('data-valmsg-replace', 'true');

  // Nach dem Input einfügen
  relationInput.insertAdjacentElement('afterend', relationshipValidationSpan);

  const tdActions = document.createElement("td");

  const hiddenInput = document.createElement("input");
  hiddenInput.type = "hidden";
  hiddenInput.classList.add('form-control', 'participantResultTableId');
  hiddenInput.name = `ConnectedParticipantList[${index}].Id`;
  hiddenInput.value = participantIdValue;

  const removeBtn = document.createElement("button");
  removeBtn.type = "button";
  removeBtn.classList.add("btn", "btn-danger", "btn-sm", "removeParticipant");
  removeBtn.textContent = i18n.get("Remove");

  tdActions.appendChild(hiddenInput);
  tdActions.appendChild(removeBtn);
  tr.appendChild(tdName);
  tr.appendChild(tdSpecs);
  tr.appendChild(tdInput);
  tr.appendChild(tdActions);
  tbody.appendChild(tr);

  try {
    const $row = $(tr);
    const $form = $row.closest('form');
    if ($form && ($ as any).validator) {
      $form.removeData('validator');
      $form.removeData('unobtrusiveValidation');
      ($ as any).validator.unobtrusive.parse($form);
    } else {
      // fallback: parse just the new row
      ($ as any).validator.unobtrusive.parse($row);
    }
  } catch (e) {
    // ignore if jQuery/validator not available
    console.warn('Validation parse skipped', e);
  }

  hideModal('participantModal');
}
(function initRemoveParticipantButtonHandler(): void {
  const tableBody = document.getElementById('appendParticipantHere');
  if (!tableBody) return;

  tableBody.addEventListener('click', function (e: Event) {
    const target = e.target as HTMLElement;
    if (target && target.classList.contains('removeParticipant')) {
      const trTag = target.closest('.participantResultTableTr') as HTMLTableRowElement;
      if (trTag) {
        trTag.remove();
        reindexParticipantFields(tableBody as HTMLTableSectionElement);
      }
    }
  });
})();
function reindexParticipantFields(container: HTMLTableSectionElement): void {
  const trs = Array.from(container.querySelectorAll(".participantResultTableTr"));

  trs.forEach((tr: Element, index: number) => {
    const participantIdInput = tr.querySelector(".participantResultTableId") as HTMLInputElement;
    if (participantIdInput) {
      participantIdInput.name = `ConnectedParticipantList[${index}].Id`;
    }

    const relationshipInput = tr.querySelector(".participantResultTableRelationship") as HTMLInputElement;
    if (relationshipInput) {
      relationshipInput.name = `ConnectedParticipantList[${index}].Relationship`;
      relationshipInput.id = `ConnectedParticipantList_${index}__Relationship`;
    }
    // update validation span's data-valmsg-for if present
    const validationSpan = tr.querySelector('[data-valmsg-for]') as HTMLElement;
    if (validationSpan) {
      validationSpan.setAttribute('data-valmsg-for', `PlaceToCollectionItemList[${index}].Relationship`);
    }
  });

  // Re-parse validation for the whole form after reindexing
  try {
    const $container = $(container);
    const $form = $container.closest('form');
    if ($form && ($ as any).validator) {
      $form.removeData('validator');
      $form.removeData('unobtrusiveValidation');
      ($ as any).validator.unobtrusive.parse($form);
    }
  } catch (e) {
    console.warn('Validation re-parse skipped', e);
  }
}

document.addEventListener('click', (event) => {
  const button = (event.target as HTMLElement).closest('.addEra');
  if (!button) return;

  const buttonId = button.getAttribute('id');
  if (!buttonId) return;

  const eraSearchResultID = document.getElementById(`eraSearchResultID_${buttonId}`);
  const eraSearchResultName = document.getElementById(`eraSearchResultName_${buttonId}`);
  const eraIdInput = document.getElementById('EraID') as HTMLInputElement;
  const eraNameElement = document.getElementById('eraName');
  const clearButton = document.getElementById('clearOneRowEraTable');

  if (eraSearchResultID && eraSearchResultName && eraIdInput && eraNameElement && clearButton) {
    eraIdInput.value = eraSearchResultID.textContent || '';
    eraNameElement.innerText = eraSearchResultName.textContent || '';
    clearButton.style.display = 'inline';
    hideModal('EraModal');
  }
});

const clearEraButton = document.getElementById('clearOneRowEraTable');
if (clearEraButton) {
  clearEraButton.addEventListener('click', function () {
    (document.getElementById('eraId') as HTMLInputElement).value = '';
    document.getElementById('eraName')!.innerText = '';
    (clearEraButton as HTMLElement).style.display = 'none';
  });
}

type SourcePage = "Edit" | "Create";

function addFormFileCollectionItem(sourcePage: SourcePage): void {
  const perspective = [
    i18n.get("Side_Front"),
    i18n.get("Side_Back"),
    i18n.get("Side_Left"),
    i18n.get("Side_Right"),
    i18n.get("Side_Top"),
    i18n.get("Side_Bottom")
  ];

  const container = document.getElementById("appendInputFormFileCollectionItem") as HTMLDivElement;
  const newPictureCount = container.children.length;
  const existingVisiblePictures = document.querySelectorAll('#cardDiv .col:not([style*="display: none"])').length;
  const newIndex = existingVisiblePictures + newPictureCount;

  const div = document.createElement("div");
  div.className = "mb-3 inputGroupFormFile";
  div.id = `inputGroupFormFile_${newIndex}`;

  const label = document.createElement("label");
  label.className = "form-label";
  label.setAttribute("for", `CollectionItemPictureList_${newIndex}__PerspectiveInt`);
  label.innerText = perspective[newIndex] || i18n.get("Side_Other");
  div.appendChild(label);

  if (sourcePage == "Edit") {
    const inputCollectionItemPictureID = document.createElement("input");
    inputCollectionItemPictureID.type = "hidden";
    inputCollectionItemPictureID.name = `CollectionItemPictureList[${newIndex}].Id`;
    div.appendChild(inputCollectionItemPictureID);
  }

  const divInputGroup = document.createElement("div");
  divInputGroup.className = "input-group";

  const inputFile = document.createElement("input");
  inputFile.type = "file";
  inputFile.name = `CollectionItemPictureList[${newIndex}].IFormFile`;
  inputFile.className = "form-control";
  inputFile.setAttribute("data-val", "true");
  inputFile.setAttribute("data-val-required", i18n.get("Error_CollectionItemPicture_Required"));
  inputFile.setAttribute("aria-required", "true");
  inputFile.setAttribute("aria-invalid", "false");
  inputFile.setAttribute("aria-describedby", `CollectionItemPictureList_${newIndex}__IFormFile-error`);
  divInputGroup.appendChild(inputFile);

  const removeButton = document.createElement("button");
  removeButton.type = "button";
  removeButton.className = "btn btn-danger removeFormFileInput";
  removeButton.textContent = i18n.get("Remove");
  divInputGroup.appendChild(removeButton);
  div.appendChild(divInputGroup);

  if (sourcePage == "Create") {
    const inputPerspectiveInt = document.createElement("input");
    inputPerspectiveInt.type = "hidden";
    inputPerspectiveInt.name = `CollectionItemPictureList[${newIndex}].PerspectiveInt`;
    inputPerspectiveInt.className = "form-control";
    //inputPerspectiveInt.value = newIndex.toString();
    inputPerspectiveInt.value = (newIndex % perspective.length).toString();
    div.appendChild(inputPerspectiveInt);
  }
  else if (sourcePage == "Edit") {
    const selectPerspective = document.createElement("select");
    selectPerspective.name = `CollectionItemPictureList[${newIndex}].PerspectiveInt`;
    selectPerspective.className = "form-select";

    perspective.forEach((element, idx) => {
      const optionPerspective = document.createElement("option");
      optionPerspective.value = idx.toString();
      optionPerspective.text = element;
      if (idx === (newIndex % perspective.length)) {
        optionPerspective.selected = true;
      }
      selectPerspective.appendChild(optionPerspective);
    })

    div.appendChild(selectPerspective);
  }

  container.appendChild(div);
}
(function initRemoveCollectionItemPictureButtonHandler(): void {
  const container = document.getElementById('appendInputFormFileCollectionItem');
  if (!container) return;

  container.addEventListener('click', function (e: Event) {
    const target = e.target as HTMLElement;
    if (target && target.classList.contains('removeFormFileInput')) {
      const inputgroup = target.closest('.inputGroupFormFile') as HTMLDivElement;
      if (inputgroup) {
        inputgroup.remove();
      }
    }
  });

  initDeleteExistingPictureHandler();
})();

function initDeleteExistingPictureHandler(): void {
  const deleteButtons = document.querySelectorAll('[data-delete-picture]');
  deleteButtons.forEach(button => {
    button.addEventListener('click', (e) => {
      const btn = e.currentTarget as HTMLElement;
      const pictureId = btn.getAttribute('data-picture-id');
      const pictureIndex = btn.getAttribute('data-picture-index');

      if (pictureId && pictureIndex) {
        markPictureForDeletion(pictureIndex, pictureId);
      }
    });
  });
}
// Array für zu löschende Bild-IDs
function markPictureForDeletion(pictureIndex: string, pictureId: string): void {
  // Bildkarte ausblenden/entfernen
  const pictureCard = document.getElementById(`pictureCard_${pictureIndex}`);
  if (pictureCard) {
    pictureCard.style.display = 'none';
  }

  // ID zum Hidden-Feld hinzufügen
  const deletedIdsInput = document.getElementById('deletedPictureIds') as HTMLInputElement;
  if (deletedIdsInput) {
    const currentIds = deletedIdsInput.value ? deletedIdsInput.value.split(',') : [];
    if (!currentIds.includes(pictureId)) {
      currentIds.push(pictureId);
      deletedIdsInput.value = currentIds.join(',');
    }
  }
}

//function addFormFileOwnershipProof(sourcePage: SourcePage): void {
//  const container = document.getElementById("appendInputFormFileOwnershipProof") as HTMLDivElement;
//  const pictureCount = container.children.length;

//  const div = document.createElement("div");
//  div.className = "mb-3 inputGroupFormFileOwnershipProof";
//  //div.id = `inputGroupFormFileOwnershipProof_${pictureCount}`;

//  if (sourcePage == "Edit") {
//    const inputCollectionItemPictureID = document.createElement("input");
//    inputCollectionItemPictureID.type = "hidden";
//    inputCollectionItemPictureID.className = "ownershipPictureId";
//    inputCollectionItemPictureID.name = `OwnershipProofPictureList[${pictureCount}].Id`;
//    div.appendChild(inputCollectionItemPictureID);
//  }

//  const divInputGroup = document.createElement("div");
//  divInputGroup.className = "input-group";

//  const inputFile = document.createElement("input");
//  inputFile.type = "file";
//  inputFile.name = `OwnershipProofPictureList[${pictureCount}].FormFile`;
//  inputFile.className = "form-control ownershipPictureFormFile";
//  divInputGroup.appendChild(inputFile);

//  const removeButton = document.createElement("button");
//  removeButton.type = "button";
//  removeButton.className = "btn btn-danger removeFormFileInput";
//  removeButton.textContent = i18n.get("Remove");
//  divInputGroup.appendChild(removeButton);
//  div.appendChild(divInputGroup);

//  const ownershipProofTypes = [
//    "OwnershipProof_Type_BillOfSale",
//    "OwnershipProof_Type_Certificate",
//    "OwnershipProof_Type_Other"
//  ];

//  const selectType = document.createElement("select");
//  selectType.name = `OwnershipProofPictureList[${pictureCount}].Type`;
//  selectType.className = "form-select ownershipProofType";
//  div.appendChild(selectType);

//  ownershipProofTypes.forEach((element, index) => {
//    const option = document.createElement("option");
//    option.value = index.toString();
//    option.text = i18n.get(element);
//    selectType.appendChild(option);
//  });

//  container.appendChild(div);
//}
//(function initRemoveOwnershipProofPictureButtonHandler(): void {
//  const container = document.getElementById('appendInputFormFileOwnershipProof') as HTMLDivElement;
//  if (!container) return;

//  container.addEventListener('click', function (e: Event) {
//    const target = e.target as HTMLElement;
//    if (target && target.classList.contains('removeFormFileInput')) {
//      const inputgroup = target.closest('.inputGroupFormFileOwnershipProof') as HTMLDivElement;
//      if (inputgroup) {
//        inputgroup.remove();
//        reindexChildOwnershipProofPicture(container as HTMLDivElement);
//      }
//    }
//  });
//})();
//function reindexChildOwnershipProofPicture(container: HTMLDivElement): void {
//  const entries = Array.from(container.querySelectorAll(".inputGroupFormFileOwnershipProof"));

//  entries.forEach((entry: Element, index: number) => {
//    const idInput = entry.querySelector(".ownershipPictureId") as HTMLInputElement;
//    if (idInput) {
//      idInput.name = `OwnershipProofPictureList[${index}].id`;
//    }

//    const fileInput = entry.querySelector(".ownershipPictureFormFile") as HTMLInputElement;
//    if (fileInput) {
//      fileInput.name = `OwnershipProofPictureList[${index}].FormFile`;
//    }

//    const typeInput = entry.querySelector(".ownershipProofType") as HTMLInputElement;
//    if (typeInput) {
//      typeInput.name = `OwnershipProofPictureList[${index}].Type`;
//    }
//  });
//}

function removePicture(pictureCount: number): void {
  const pictureCard = document.getElementById(`pictureCard_${pictureCount}`);
  const cardDiv = document.getElementById('cardDiv') as HTMLDivElement;

  if (pictureCard && cardDiv.contains(pictureCard)) {
    cardDiv.removeChild(pictureCard);
  }

  removeFormFileInput(pictureCount);
}
function removeFormFileInput(pictureCount: number): void {
  const appendInputFormFileCollectionItem = document.getElementById('appendInputFormFileCollectionItem');
  const inputGroupFormFile = document.getElementById(`inputGroupFormFile_${pictureCount}`);

  if (appendInputFormFileCollectionItem && inputGroupFormFile && appendInputFormFileCollectionItem.contains(inputGroupFormFile)) {
    appendInputFormFileCollectionItem.removeChild(inputGroupFormFile);
  }
}

if (typeof window !== 'undefined') {
  window.addConceptToCollectionItem = addConceptToCollectionItem;
  window.addParticipant = addParticipant;
  window.addFormFileCollectionItem = addFormFileCollectionItem;
  //window.addFormFileOwnershipProof = addFormFileOwnershipProof;
  window.removePicture = removePicture;
}