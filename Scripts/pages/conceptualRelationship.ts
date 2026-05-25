import { i18n } from '../TranslationService';
import { hideModal } from '../helperFunctions';

type ConceptRelation = 'synonym' | 'subterm';

function addConceptToConcept(idx: number, relation: ConceptRelation): void {
  const tbody = document.getElementById('appendConceptHere') as HTMLTableSectionElement;
  const index = tbody.children.length;

  const conceptIdValue = document.getElementById(`conceptSearchResultConceptID_${idx}`)?.textContent || '';
  const nameValue = document.getElementById(`conceptSearchResultName_${idx}`)?.textContent || '';

  const tr = document.createElement('tr');
  tr.className = 'conceptResultTableTr';

  const tdName = document.createElement("td");

  if (relation === 'synonym') {
    tdName.textContent = i18n.get("SynonymFor") + " " + nameValue;
  } else if (relation === 'subterm') {
    tdName.textContent = i18n.get("SubTermOf") + " " + nameValue;
  }

  const tdActions = document.createElement("td");

  const inputFromConcept = document.createElement("input");
  inputFromConcept.type = "hidden";
  inputFromConcept.classList.add('form-control', 'conceptResultTableId');
  inputFromConcept.name = `ConceptRelationList[${index}].ToConceptID`;
  inputFromConcept.value = conceptIdValue;

  const inputRelationship = document.createElement('input');
  inputRelationship.type = 'hidden';
  inputRelationship.classList.add('form-control', 'conceptResultTableRelationship');
  inputRelationship.name = `ConceptRelationList[${index}].RelationTypeInt`;
  inputRelationship.setAttribute("data-val", "true");
  inputRelationship.setAttribute("data-val-required", i18n.get("Error_Relationship_Required"));
  inputRelationship.setAttribute("aria-required", "true");
  inputRelationship.setAttribute("aria-invalid", "false");
  inputRelationship.setAttribute("aria-describedby", `ConceptRelationList_${index}__Relationship-error`);

  if (relation === 'synonym') {
    inputRelationship.value = "0";
  } else if (relation === 'subterm') {
    inputRelationship.value = "1";
  }

  const removeBtn = document.createElement("button");
  removeBtn.type = "button";
  removeBtn.classList.add("btn", "btn-danger", "btn-sm", "removeConceptRow");
  removeBtn.textContent = "Entfernen";

  tdActions.appendChild(inputFromConcept);
  tdActions.appendChild(inputRelationship);
  tdActions.appendChild(removeBtn);

  tr.appendChild(tdName);
  tr.appendChild(tdActions);

  tbody.appendChild(tr);

  hideModal('ConceptModal');
}

(function initRemoveConceptRelationButtonHandler(): void {
  const container = document.getElementById('appendConceptHere');
  if (!container) return;

  container.addEventListener('click', function (e: Event) {
    const target = e.target as HTMLElement;
    if (target && target.classList.contains('removeConceptRow')) {
      const trTag = target.closest('.conceptResultTableTr') as HTMLTableRowElement;
      if (trTag) {
        trTag.remove();
        reindexConceptRelationFields(container as HTMLTableSectionElement);
      }
    }
  });
})();

function reindexConceptRelationFields(container: HTMLTableSectionElement): void {
  const entries = Array.from(container.querySelectorAll(".conceptResultTableTr"));

  entries.forEach((entry: Element, index: number) => {
    const idInput = entry.querySelector(".conceptResultTableId") as HTMLInputElement;
    if (idInput) {
      idInput.name = `ConceptRelationList[${index}].ToConceptID`;
    }

    const relationshipInput = entry.querySelector(".conceptResultTableRelationship") as HTMLInputElement;
    if (relationshipInput) {
      relationshipInput.name = `ConceptRelationList[${index}].RelationType`;
    }
  });
}

//// Nur im Browser-Kontext verfügbar machen
if (typeof window !== 'undefined') {
  window.addConceptToConcept = addConceptToConcept;
}