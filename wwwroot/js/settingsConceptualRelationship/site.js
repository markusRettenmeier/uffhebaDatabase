import { i18n } from '../TranslationService';
import { hideModal } from '../site';
function addConceptConceptualRelationship(idx, relation) {
    const tbody = document.getElementById('appendConceptHere');
    const index = tbody.children.length;
    const conceptIdValue = document.getElementById(`conceptSearchResultConceptID_${idx}`)?.textContent || '';
    const nameValue = document.getElementById(`conceptSearchResultName_${idx}`)?.textContent || '';
    const tr = document.createElement('tr');
    tr.className = 'conceptResultTableTr';
    const tdName = document.createElement("td");
    if (relation === 'synonym') {
        tdName.textContent = i18n.get("SynonymFor") + " " + nameValue;
    }
    else if (relation === 'subterm') {
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
    inputRelationship.name = `ConceptRelationList[${index}].RelationType`;
    if (relation === 'synonym') {
        inputRelationship.value = "0";
    }
    else if (relation === 'subterm') {
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
(function initRemoveConceptRelationButtonHandler() {
    const container = document.getElementById('appendConceptHere');
    if (!container)
        return;
    container.addEventListener('click', function (e) {
        const target = e.target;
        if (target && target.classList.contains('removeConceptRow')) {
            const trTag = target.closest('.conceptResultTableTr');
            if (trTag) {
                trTag.remove();
                reindexConceptRelationFields(container);
            }
        }
    });
})();
function reindexConceptRelationFields(container) {
    const entries = Array.from(container.querySelectorAll(".conceptResultTableTr"));
    entries.forEach((entry, index) => {
        const idInput = entry.querySelector(".conceptResultTableId");
        if (idInput) {
            idInput.name = `ConceptRelationList[${index}].ToConceptID`;
        }
        const relationshipInput = entry.querySelector(".conceptResultTableRelationship");
        if (relationshipInput) {
            relationshipInput.name = `ConceptRelationList[${index}].RelationType`;
        }
    });
}
//// Nur im Browser-Kontext verfügbar machen
if (typeof window !== 'undefined') {
    window.addConceptConceptualRelationship = addConceptConceptualRelationship;
}
