const toggleCity = document.querySelector('.ExistingCitiesSearchSubmit')
if (toggleCity) {
    toggleCity.addEventListener('click', () => {
        const city = document.querySelector('.InputOeconymSearch').value.trim();
        fetch('/api/collections/listCities?term=' + encodeURIComponent(city))
            .then(res => {
                if (!res.ok) throw new Error(res.statusText);
                return res.json();
            })
            .then(result => {
                const table = document.getElementById('citySearchResultTable');
                const oldTbody = document.getElementById('citySearchResultTableBody');
                if (oldTbody) oldTbody.remove();
                const tbody = table.createTBody();
                tbody.id = 'citySearchResultTableBody';

                if (result.length > 0) {
                    result.forEach((element, idx) => {
                        tbody.appendChild(buildCitySearchResultRow(element, idx));
                    });
                } else {
                }
            })
            .catch(err => {
                sendErrorMessage(err);
            });
    });
}

function getContextFromUrl() {
    const url = window.location.href;
    if (url.includes('BrickDatabase')) return 'BrickDatabase';
    if (url.includes('BodyOfWater') || url.includes('Building') || url.includes('Field') || url.includes('Region') || url.includes('Relief') || url.includes('Settlement') || url.includes('TransportRoute')) return 'PlaceDatabase';
    if (url.includes('ManufactoryDatabase')) return 'ManufactoryDatabase';
    return '';
}

const createTd = ({ text = '', id = null, scope = null }) => {
    const td = document.createElement('td');
    if (id) td.id = id;
    if (scope) td.setAttribute('scope', scope);
    td.textContent = text;
    return td;
};

function buildCitySearchResultRow(element, idx, context) {
    const tr = document.createElement('tr');
    tr.id = `citySearchResult_${idx}`;

    // 1. cityID
    tr.appendChild(createTd({
        text: element.cityID,
        id: `citySearchResultcityID_${idx}`,
        scope: 'row'
    }));

    // 2. Oeconym
    const tdOeconym = document.createElement('td');
    tdOeconym.id = `citySearchResultOecoynm_${idx}`;
    if (element.cityNOeconymList) {
        element.cityNOeconymList.forEach(entry => {
            const name = entry.oeconym?.oeconymName ?? '';
            const node = document.createElement(entry.currentName ? 'strong' : 'span');
            node.textContent = name + ', ';
            tdOeconym.appendChild(node);
        });
    }
    tr.appendChild(tdOeconym);

    // 3. Postalcode
    const tdPostal = document.createElement('td');
    tdPostal.id = `citySearchResultPostalcode_${idx}`;
    if (element.postalcodeList) {
        const codes = element.postalcodeList.map(p => p.postalcodeNumber).join(', ');
        tdPostal.textContent = codes;
    }
    tr.appendChild(tdPostal);

    // 4. Byname
    tr.appendChild(createTd({
        text: element.byname ?? '',
        id: `citySearchResultByname_${idx}`
    }));

    // 5. Geography
    tr.appendChild(createTd({
        text: element.geography?.geographyName ?? '',
        id: `citySearchResultGeography_${idx}`
    }));

    // 6. Action-Button (je nach Kontext)
    const tdAction = document.createElement('td');
    tdAction.id = `citySearchResultAction_${idx}`;
    const btn = document.createElement('button');
    btn.type = 'button';
    btn.className = 'btn btn-primary';
    btn.id = `citySearchResult_${idx}`;

    if (context === 'BrickDatabase') {
        btn.textContent = 'Ort hinzufügen';
        btn.onclick = () => SetCityIntoTableBrickEntity(idx);
        tdAction.appendChild(btn);
    } else if (context === 'CityDatabase') {
        btn.textContent = 'Auswählen';
        btn.onclick = () => AddParentCity(idx);
        tdAction.appendChild(btn);
    } else if (context === 'ManufactoryDatabase') {
        btn.textContent = 'Auswählen';
        btn.onclick = () => AddCityToManufactory(idx);
        tdAction.appendChild(btn);
    }

    tr.appendChild(tdAction);
    return tr;
}

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

//function buildToponymySearchResultRow(element, idx, context) {
//    const tr = document.createElement('tr');
//    tr.id = `placeSearchResult_${idx}`;

//    // 1. placeID
//    tr.appendChild(createTd({
//        text: element.placeID,
//        id: `placeSearchResultPlaceID_${idx}`,
//        scope: 'row'
//    }));

//    // 2. Oeconym
//    const tdOeconym = document.createElement('td');
//    tdOeconym.id = `placeSearchResultOecoynm_${idx}`;
//    if (element.placeNToponymyList) {
//        element.placeNToponymyList.forEach(entry => {
//            const name = entry.toponymy?.toponymyName ?? '';
//            const node = document.createElement(entry.currentName ? 'strong' : 'span');
//            node.textContent = name + ', ';
//            tdOeconym.appendChild(node);
//        });
//    }
//    tr.appendChild(tdOeconym);

//    tr.appendChild(createTd({
//        text: element.ToponymyType,
//        id: `placeSearchResultToponymyType_${idx}`,
//        scope: 'row'
//    }));

//    // 3. Postalcode
//    //let furtherSpecs;
//    //if (element.settlementData) {
//    //    //if (element.postalcodeList) {
//    //    //    furtherSpecs = "PLz: ";
//    //    //    const furtherSpecs = element.postalcodeList.map(p => p.postalcodeNumber).join(', ');
//    //    //    furtherSpecs = furtherSpecs + "; ";
//    //    //}
//    //    //furtherSpecs += "Siedlung: ";
//    //    //furtherSpecs += element.settlement.settlementName ?? '';
//    //    //furtherSpecs += "; ";
//    //    //furtherSpecs += element.settlement?.byname ?? '';
//    //    //furtherSpecs += "; " + (element.settlement?.relatedPlace?.placeNToponymyList?. ?? '');

//    //}
//    const tdFurtherSpecs = document.createElement('td');
//    tdFurtherSpecs.id = `placeSearchResultPostalcode_${idx}`;
//    tdFurtherSpecs.textContent = element.settlementData ?? '';
//    tr.appendChild(tdFurtherSpecs);

//    // 6. Action-Button (je nach Kontext)
//    const tdAction = document.createElement('td');
//    tdAction.id = `placeSearchResultAction_${idx}`;
//    const btn = document.createElement('button');
//    btn.type = 'button';
//    btn.className = 'btn btn-primary';
//    btn.id = `placeSearchResult_${idx}`;
//    btn.textContent = 'Ort hinzufügen';

//    if (context === 'ProductDatabase') {
//        btn.onclick = () => SetCityIntoTableBrickEntity(idx);
//    } else if (context === 'CityDatabase') {
//        btn.onclick = () => addPlace(idx);
//    } else if (context === 'ManufactoryDatabase') {
//        btn.onclick = () => AddCityToManufactory(idx);
//    }
//    tdAction.appendChild(btn);

//    tr.appendChild(tdAction);
//    return tr;
//}
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
    tdToponym.innerHTML = element.oeconymDisplay || '';
    tr.appendChild(tdToponym);

    tr.appendChild(createTd({
        text: element.toponymyType,
        id: `placeSearchResultToponymyType_${idx}`,
        scope: 'row'
    }));

    // 4. FurtherSpecs
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
    //if (context === 'ProductDatabase') {
    //    btn.onclick = () => SetCityIntoTableBrickEntity(idx);
//} else
    if (url.includes('BodyOfWater') || url.includes('Building') || url.includes('Field') || url.includes('Region') || url.includes('Relief') || url.includes('Settlement') || url.includes('TransportRoute')) {
        const btnParentPlace = document.createElement('button');
        btnParentPlace.type = 'button';
        btnParentPlace.className = 'btn btn-primary placeSearchResultParentButton';
        //btnParentPlace.id = `placeSearchResultParentButton_${idx}`;
        btnParentPlace.textContent = 'Eltern Ort hinzufügen';
        btnParentPlace.onclick = () => addParentPlace(idx);
        divAction.appendChild(btnParentPlace);

        const btnChildPlace = document.createElement('button');
        btnChildPlace.type = 'button';
        btnChildPlace.className = 'btn btn-primary placeSearchResultChildButton';
        //btnChildPlace.id = `placeSearchResultChildButton_${idx}`;
        btnChildPlace.textContent = 'Kind Ort hinzufügen';
        btnChildPlace.onclick = () => addChildPlace(idx);
        divAction.appendChild(btnChildPlace);

        if (url.includes('Settlement')) {
            const btnRelatedPlace = document.createElement('button');
            btnRelatedPlace.type = 'button';
            btnRelatedPlace.className = 'btn btn-primary placeSearchResultRelatedButton';
            //btnRelatedPlace.id = `placeSearchResultRelatedButton_${idx}`;
            btnRelatedPlace.textContent = 'Bezugsort hinzufügen';
            btnRelatedPlace.onclick = () => addRelatedPlace(idx);
            divAction.appendChild(btnRelatedPlace);
        }
    } else if (url.includes('Individual') || url.includes('Organization')) {
        const btnParentPlace = document.createElement('button');
        btnParentPlace.type = 'button';
        btnParentPlace.className = 'btn btn-primary';
        btnParentPlace.textContent = 'Ort hinzufügen';
        btnParentPlace.onclick = () => addPlace(idx);
        divAction.appendChild(btnParentPlace);
    }
    tdAction.appendChild(divAction);

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
    // Vorhandene Dropdowns entfernen
    let existingDropdown = document.querySelector('.autocomplete-dropdown');
    if (existingDropdown) existingDropdown.remove();

    if (suggestions.length === 0) return;

    // Dropdown-Container erstellen
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