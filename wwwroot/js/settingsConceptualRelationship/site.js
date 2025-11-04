function addConcept(idx, relation) {
    const tbody = document.getElementById('appendConceptHere');
    const index = tbody.children.length;

    const conceptIdValue = document.getElementById(`conceptSearchResultConceptID_${idx}`)?.textContent || '';
    const nameValue = document.getElementById(`conceptSearchResultName_${idx}`)?.textContent || '';

    const tr = document.createElement('tr');
    tr.className = 'conceptResultTableTr';

    const tdName = document.createElement("td");
    if (relation == 'synonym') {
        tdName.textContent = 'Synonym für ' + nameValue;
    }
    else if (relation == 'subterm') {
        tdName.textContent = 'Unterbegriff von ' + nameValue;
    }
    else {
        tdName.textContent = 'Kurzbegriff für ' + nameValue;
    }
    const tdActions = document.createElement("td");

    let inputFromConcept = document.createElement("input");
    inputFromConcept.type = "hidden";
    inputFromConcept.classList.add('form-control', 'conceptResultTableId');
    inputFromConcept.name = `ConceptRelationList[${index}].ToConceptID`;
    inputFromConcept.value = conceptIdValue

    const inputRelationship = document.createElement('input');
    inputRelationship.type = 'hidden';
    inputRelationship.classList.add('form-control', 'conceptResultTableRelationship');
    inputRelationship.name = `ConceptRelationList[${index}].RelationType`;
    if (relation == 'synonym') {
        inputRelationship.value = 0;
    }
    else if (relation == 'subterm') {
        inputRelationship.value = 1;
    }
    else {
        inputRelationship.value = 2;
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
    if (!container) return;

    container.addEventListener('click', function (e) {
        if (e.target && e.target.classList.contains('removeConceptRow')) {
            const trTag = e.target.closest('.conceptResultTableTr');
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
        const primaryColorCheckbox = entry.querySelector(".conceptResultTableRelationship");
        if (primaryColorCheckbox) {
            primaryColorCheckbox.name = `ConceptRelationList[${index}].RelationTypeInt`;
        }
    });
}