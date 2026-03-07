import { i18n } from "../TranslationService.js";
import { createTd, sendErrorMessage } from "../api.js";
import { SetEraIntoTable, SetSetIntoTable } from "./site.js";
const eraToggle = document.querySelector(".eraSearchSubmit");
if (eraToggle) {
    eraToggle.addEventListener('click', () => {
        const nameInput = document.getElementById('inputEraNameSearch');
        const name = nameInput.value;
        fetch('/api/collections/listEras?name=' + encodeURIComponent(name))
            .then(res => {
            if (!res.ok)
                throw new Error(res.statusText);
            return res.json();
        })
            .then((result) => {
            const tableBody = document.getElementById('eraSearchResultTableBody');
            if (tableBody != null)
                tableBody.remove();
            const myTable = document.getElementById("eraSearchResultTable");
            const tbody = myTable.createTBody();
            tbody.setAttribute('id', 'eraSearchResultTableBody');
            if (result.length > 0) {
                result.forEach((element, idx) => {
                    tbody.appendChild(buildEraSearchResultRow(element, idx));
                });
            }
            else {
                const tr = document.createElement('tr');
                const td = document.createElement('td');
                td.textContent = i18n.get("NothingFound");
                tr.appendChild(td);
                tbody.appendChild(tr);
            }
        })
            .catch(err => {
            sendErrorMessage(err);
        });
    });
}
function buildEraSearchResultRow(element, idx) {
    const tr = document.createElement('tr');
    tr.id = `eraSearchResult_${idx}`;
    tr.appendChild(createTd({
        text: element.eraID,
        id: `eraSearchResultID_${idx}`,
        scope: 'row'
    }));
    tr.appendChild(createTd({
        text: element.eraName,
        id: `eraSearchResultName_${idx}`,
        scope: 'row'
    }));
    const tdAction = document.createElement('td');
    const btn = document.createElement('button');
    btn.type = 'button';
    btn.className = 'btn btn-primary';
    btn.textContent = i18n.get("Add");
    btn.onclick = () => SetEraIntoTable(idx);
    tdAction.appendChild(btn);
    tr.appendChild(tdAction);
    return tr;
}
const setToggle = document.querySelector('.SetSearchSubmit');
if (setToggle) {
    setToggle.addEventListener('click', () => {
        const setNameInput = document.getElementById('InputSetName');
        const setName = setNameInput.value.trim();
        fetch('/api/collections/listSets?name=' + encodeURIComponent(setName))
            .then(res => {
            if (!res.ok)
                throw new Error(res.statusText);
            return res.json();
        })
            .then((result) => {
            const table = document.getElementById('SetSearchResultTable');
            const oldTbody = document.getElementById('SetSearchResultTableBody');
            if (oldTbody)
                oldTbody.remove();
            const tbody = table.createTBody();
            tbody.id = 'SetSearchResultTableBody';
            if (result.length > 0) {
                result.forEach((element, idx) => {
                    tbody.appendChild(BuildSetSearchResultRow(element, idx));
                });
            }
            else {
                const tr = document.createElement('tr');
                const td = document.createElement('td');
                td.textContent = i18n.get("NothingFound");
                tr.appendChild(td);
                tbody.appendChild(tr);
            }
        })
            .catch(err => {
            sendErrorMessage(err);
        });
    });
}
function BuildSetSearchResultRow(element, idx) {
    const tr = document.createElement('tr');
    tr.id = `SetSearchResult_${idx}`;
    tr.appendChild(createTd({
        text: element.setId,
        id: `SetSearchResultSetID_${idx}`,
        scope: 'row'
    }));
    tr.appendChild(createTd({
        text: element.setName,
        id: `SetSearchResultSetName_${idx}`,
        scope: 'row'
    }));
    const tdAction = document.createElement('td');
    tdAction.id = `SetSearchResultAction_${idx}`;
    const btn = document.createElement('button');
    btn.type = 'button';
    btn.className = 'btn btn-primary';
    btn.id = `SetSearchResult_${idx}`;
    btn.textContent = i18n.get("Add");
    btn.onclick = () => SetSetIntoTable(idx);
    tdAction.appendChild(btn);
    tr.appendChild(tdAction);
    return tr;
}
