const createTd = ({ text = '', id = null, scope = null }) => {
    const td = document.createElement('td');
    if (id) td.id = id;
    if (scope) td.setAttribute('scope', scope);
    td.textContent = text;
    return td;
};

const togglePlace = document.querySelector('.placeSearchSubmit')
if (togglePlace) {
    togglePlace.addEventListener('click', () => {
        const toponymyName = document.querySelector('.inputPlaceSearch').value.trim();
        const toponymyType = document.getElementById('toponymyType').value;
        fetch('/api/collections/listPlaces', {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                toponym: toponymyName,
                toponymyType: toponymyType
            }),
        })
            .then(res => {
                if (!res.ok) throw new Error(res.statusText);
                return res.json();
            })
            .then(result => {
                const table = document.getElementById('placeSearchResultTable');
                const oldTbody = document.getElementById('placeSearchResultTableBody');
                if (oldTbody) oldTbody.remove();
                const tbody = table.createTBody();
                tbody.id = 'placeSearchResultTableBody';

                if (result.length > 0) {
                    result.forEach((element, idx) => {
                        tbody.appendChild(buildToponymySearchResultRow(element, idx));
                    });
                } else {
                    const tr = document.createElement('tr');
                    tr.appendChild(createTd({
                            text: 'Keine Einträge zum Suchwort vorhanden.',
                            scope: 'row'
                    }))
                    tbody.appendChild(tr);
                }
            })
            .catch(err => {
                sendErrorMessage(err);
            });
    });
}
function buildToponymySearchResultRow(element, idx) {
    const tr = document.createElement('tr');
    tr.id = `placeSearchResult_${idx}`;

    tr.appendChild(createTd({
        text: element.placeID,
        id: `placeSearchResultPlaceID_${idx}`,
        scope: 'row'
    }));

    const tdToponym = document.createElement('td');
    tdToponym.id = `placeSearchResultToponym_${idx}`;
    //Wichtig: innerHtml, weil sonst <string> nicht interpretiert wird
    tdToponym.innerHTML = element.oeconymDisplay || '';
    tr.appendChild(tdToponym);

    tr.appendChild(createTd({
        text: element.toponymyType,
        id: `placeSearchResultToponymyType_${idx}`,
        scope: 'row'
    }));

    const tdFurtherSpecs = document.createElement('td');
    tdFurtherSpecs.id = `placeSearchResultFurtherSpecs_${idx}`;
    tdFurtherSpecs.textContent = element.furtherSpecs || '';
    tr.appendChild(tdFurtherSpecs);

    const tdAction = document.createElement('td');
    const divAction = document.createElement('div');
    divAction.className = 'btn-group';
    divAction.role = 'group';
    divAction.ariaLabel = "Aktion Div";
    const url = window.location.href;

    if (url.includes('BodyOfWater') || url.includes('Building') || url.includes('Field') || url.includes('Region') || url.includes('Relief') || url.includes('Settlement') || url.includes('TransportRoute')) {
        const btnParentPlace = document.createElement('button');
        btnParentPlace.type = 'button';
        btnParentPlace.classList.add('btn', 'btn-primary', 'placeSearchResultParentButton');
        btnParentPlace.textContent = 'Eltern Ort hinzufügen';
        btnParentPlace.onclick = () => addParentPlace(idx);
        divAction.appendChild(btnParentPlace);

        const btnChildPlace = document.createElement('button');
        btnChildPlace.type = 'button';
        btnChildPlace.classList.add('btn', 'btn-primary', 'placeSearchResultChildButton');
        btnChildPlace.textContent = 'Kind Ort hinzufügen';
        btnChildPlace.onclick = () => addChildPlace(idx);
        divAction.appendChild(btnChildPlace);

        if (url.includes('Settlement')) {
            const btnRelatedPlace = document.createElement('button');
            btnRelatedPlace.type = 'button';
            btnRelatedPlace.classList.add('btn', 'btn-primary', 'placeSearchResultRelatedButton');
            btnRelatedPlace.textContent = 'Bezugsort hinzufügen';
            btnRelatedPlace.onclick = () => addRelatedPlace(idx);
            divAction.appendChild(btnRelatedPlace);
        }
    } else if (url.includes('Individual') || url.includes('Organization') || url.includes('CollectionItem')) {
        const btnParentPlace = document.createElement('button');
        btnParentPlace.type = 'button';
        btnParentPlace.classList.add('btn', 'btn-primary', 'btn-sm');
        btnParentPlace.textContent = 'Ort hinzufügen';
        btnParentPlace.onclick = () => addPlace(idx);
        divAction.appendChild(btnParentPlace);
    }
    tdAction.appendChild(divAction);

    tr.appendChild(tdAction);
    return tr;
}

const toggleParty = document.querySelector('.partySearchSubmit')
if (toggleParty) {
    toggleParty.addEventListener('click', () => {
        const partyName = document.querySelector('.inputPartySearch').value.trim();
        const partyType = document.getElementById('partyType').value;
        fetch('/api/collections/listParties', {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                name: partyName,
                type: partyType
            }),
        })
            .then(res => {
                if (!res.ok) throw new Error(res.statusText);
                return res.json();
            })
            .then(result => {
                const table = document.getElementById('partySearchResultTable');
                const oldTbody = document.getElementById('partySearchResultTableBody');
                if (oldTbody) oldTbody.remove();
                const tbody = table.createTBody();
                tbody.id = 'partySearchResultTableBody';

                if (result.length > 0) {
                    result.forEach((element, idx) => {
                        tbody.appendChild(buildPartySearchResultRow(element, idx));
                    });
                } else {
                    const tr = document.createElement('tr');
                    tr.appendChild(createTd({
                            text: 'Keine Einträge zum Suchwort vorhanden.',
                            scope: 'row'
                    }))
                    tbody.appendChild(tr);
                }
            })
            .catch(err => {
                sendErrorMessage(err);
            });
    });
}
function buildPartySearchResultRow(element, idx) {
    const tr = document.createElement('tr');
    tr.id = `partySearchResult_${idx}`;

    tr.appendChild(createTd({
        text: element.partyID,
        id: `partySearchResultPartyID_${idx}`,
        scope: 'row'
    }));

    const tdName = document.createElement('td');
    tdName.id = `partySearchResultName_${idx}`;
    tdName.textContent = element.name || '';
    tr.appendChild(tdName);

    tr.appendChild(createTd({
        text: element.type,
        id: `partySearchResultType_${idx}`,
        scope: 'row'
    }));

    const tdFurtherSpecs = document.createElement('td');
    tdFurtherSpecs.id = `partySearchResultFurtherSpecs_${idx}`;
    tdFurtherSpecs.textContent = element.furtherSpecs || '';
    tr.appendChild(tdFurtherSpecs);

    const tdAction = document.createElement('td');
    const btnAction = document.createElement('button');
    btnAction.type = 'button';
    btnAction.classList.add('btn', 'btn-primary', 'btn-sm');
    btnAction.textContent = 'Ort hinzufügen';
    btnAction.onclick = () => addParty(idx);
    tdAction.appendChild(btnAction);

    tr.appendChild(tdAction);
    return tr;
}

function sendErrorMessage(xhr) {
    console.log("fetch-Error:" + xhr)
    const createCitySpan = $("#createCitySpan");
    createCitySpan.text(xhr).css('color', 'red');
}

function enableAutocomplete(inputID, inputName, sourceUrl, sourceStorage) {
    inputName.addEventListener('input', async function () {
        if (sourceUrl != "") {
            const query = inputName.value.trim();
            if (query.length < 2) return; // Warte, bis mindestens 2 Zeichen eingegeben sind

            try {
                const response = await fetch(`${sourceUrl}?q=${encodeURIComponent(query)}`);
                if (!response.ok) throw new Error('Netzwerkfehler');

                const suggestions = await response.json();
                showAutocompleteSuggestions(inputID, inputName, suggestions);
            } catch (error) {
                console.error('Autocomplete Fehler:', error);
            }
        }
        else if (sourceStorage != "") {
            showAutocompleteSuggestions(inputID, inputName, sourceStorage);
        }
    });
}

function showAutocompleteSuggestions(inputID, inputName, suggestions) {
    let existingDropdown = document.querySelector('.autocomplete-dropdown');
    if (existingDropdown) existingDropdown.remove();

    if (suggestions.length === 0) return;

    const dropdown = document.createElement('ul');
    dropdown.className = 'autocomplete-dropdown';
    dropdown.style.position = 'absolute';
    dropdown.style.left = `${inputName.offsetLeft}px`;
    dropdown.style.top = `${inputName.offsetTop + inputName.offsetHeight}px`;
    dropdown.style.width = `${inputName.offsetWidth}px`;
    dropdown.style.listStyle = 'none';
    dropdown.style.padding = '5px';
    dropdown.style.margin = '0';
    dropdown.style.backgroundColor = 'white';
    dropdown.style.border = '1px solid #ccc';
    dropdown.style.zIndex = '1000';

    suggestions.forEach(suggestion => {
        const item = document.createElement('li');
        item.textContent = suggestion.value;
        item.style.padding = '5px';
        item.style.cursor = 'pointer';

        item.addEventListener('click', function () {
            inputID.value = suggestion.id;
            inputName.value = suggestion.value;
            dropdown.remove();
        });

        dropdown.appendChild(item);
    });

    document.body.appendChild(dropdown);

    // Schließen, wenn außerhalb geklickt wird
    document.addEventListener('click', function closeDropdown(event) {
        if (!dropdown.contains(event.target) && event.target !== inputName) {
            dropdown.remove();
            document.removeEventListener('click', closeDropdown);
        }
    });
}

async function getCollectionAreaList() {
    const response = await fetch("/api/collections/listCollectionAreas");
    const json = await response.json();
    sessionStorage.setItem('collectionAreasList', JSON.stringify(json));
    return JSON.stringify(json); // damit handlePageLoad sofort etwas zurückbekommt
}

const conceptToggle = document.querySelector(".conceptSearchSubmit");
if (conceptToggle) {
    conceptToggle.addEventListener('click', () => {
        const name = document.getElementById('inputConceptSearch').value;
        fetch('/api/collections/listConcepts?conceptName=' + encodeURIComponent(name))
            .then(res => {
                if (!res.ok) throw new Error(res.statusText);
                return res.json();
            })
            .then(result => {
                const tableBody = document.getElementById('conceptSearchResultTableBody')
                if (tableBody != null)
                    tableBody.remove()
                let myTable = document.getElementById("conceptSearchResultTable")
                let tbody = myTable.createTBody()
                tbody.setAttribute('id', 'conceptSearchResultTableBody');
                if (result.length > 0) {
                    result.forEach((element, idx) => {
                        tbody.appendChild(buildConceptSearchResultRow(element, idx));
                    });
                } else {
                    const tr = document.createElement('tr');
                    const td = document.createElement('td');
                    td.textContent = `Kein Eintrag vorhanden, bitte erstellen Sie ihn.`;
                    tr.appendChild(td);
                    tbody.appendChild(tr);
                }
            })
            .catch(err => {
                sendErrorMessage(err);
            });
    });
}

function buildConceptSearchResultRow(element, idx) {
    const tr = document.createElement('tr');
    tr.id = `conceptSearchResult_${idx}`;

    tr.appendChild(createTd({
        text: element.conceptID,
        id: `conceptSearchResultConceptID_${idx}`,
        scope: 'row'
    }));

    tr.appendChild(createTd({
        text: element.conceptName,
        id: `conceptSearchResultName_${idx}`,
        scope: 'row'
    }));

    tr.appendChild(createTd({
        text: element.description,
        id: `conceptSearchResultDescription_${idx}`,
        scope: 'row'
    }));

    const tdAction = document.createElement('td');

    const divAction = document.createElement('div');
    divAction.className = 'btn-group';
    divAction.role = 'group'

    const url = window.location.href;
    if (url.includes('ConceptualRelationshipDatabase')) {
        const btnSynonym = document.createElement('button');
        btnSynonym.type = 'button';
        btnSynonym.className = 'btn btn-primary';
        btnSynonym.textContent = 'Als Synonym hinzufügen';
        btnSynonym.onclick = () => addConcept(idx, 'synonym');
        divAction.appendChild(btnSynonym);

        const btnSubTerm = document.createElement('button');
        btnSubTerm.type = 'button';
        btnSubTerm.className = 'btn btn-primary';
        btnSubTerm.textContent = 'Als Oberbegriff hinzufügen';
        btnSubTerm.onclick = () => addConcept(idx, 'subterm');
        divAction.appendChild(btnSubTerm);

        const btnShortterm = document.createElement('button');
        btnShortterm.type = 'button';
        btnShortterm.className = 'btn btn-primary';
        btnShortterm.textContent = 'Als Kurzbezeichnung hinzufügen';
        btnShortterm.onclick = () => addConcept(idx, 'shortterm');
        divAction.appendChild(btnShortterm);
    }
    else if (url.includes('CollectionItemDatabase')) {
        const btn = document.createElement('button');
        btn.type = 'button';
        btn.className = 'btn btn-primary';
        btn.textContent = 'Hinzufügen';
        btn.onclick = () => addConcept(idx,'');
        divAction.appendChild(btn);
    }

    tdAction.appendChild(divAction);
    tr.appendChild(tdAction);

    return tr;
}