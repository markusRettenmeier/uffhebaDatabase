async function getColorList() {
    try {
        const response = await fetch("/api/collections/listColors");
        if (!response.ok) {
            throw new Error(`Response status: ${response.status}`);
        }

        const json = await response.json();
        sessionStorage.setItem('colorList', JSON.stringify(json));
        //console.log('Gespeichert:', sessionStorage.getItem('colorList'));
    } catch (error) {
        console.error(error.message);
    }
}


async function getMaterialList() {
    try {
        const response = await fetch("/api/collections/listMaterials");
        if (!response.ok) {
            throw new Error(`Response status: ${response.status}`);
        }

        const json = await response.json();
        sessionStorage.setItem('materialList', JSON.stringify(json));
        //console.log('Gespeichert:', sessionStorage.getItem('manufactoryList'));
    } catch (error) {
        console.error(error.message);
    }
}

$(document.body).on('keydown', 'input.inputColor', function (event) {
    const colorList = JSON.parse(sessionStorage.getItem('colorList') || '[]');
    const autocompleteSource = colorList.map(x => ({
        label: x.colorName,
        value: x.colorName,
        id: x.colorID
    }));

    $(this).autocomplete({
        source: autocompleteSource,
        select: function (event, ui) {
            const selected = ui.item;

            const inputGroup = $(this).closest('.input-group');
            inputGroup.find('.inputColorID').val(selected.id);
        }
    });
})

const eraToggle = document.querySelector(".eraSearchSubmit");
if (eraToggle) {
    eraToggle.addEventListener('click', () => {
        const name = document.getElementById('inputEraNameSearch').value;
        fetch('/api/collections/listEras?name=' + encodeURIComponent(name))
            .then(res => {
                if (!res.ok) throw new Error(res.statusText);
                return res.json();
            })
            .then(result => {
                const tableBody = document.getElementById('eraSearchResultTableBody')
                if (tableBody != null)
                    tableBody.remove()
                let myTable = document.getElementById("eraSearchResultTable")
                let tbody = myTable.createTBody()
                tbody.setAttribute('id', 'eraSearchResultTableBody');

                if (result.length > 0) {
                    result.forEach((element, idx) => {
                        tbody.appendChild(buildEraSearchResultRow(element, idx));
                    });
                } else {
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

const collectionItemPotentialToggle = document.querySelector('.CollectionItemPotentialSearchSubmit')
if (collectionItemPotentialToggle) {
    collectionItemPotentialToggle.addEventListener('click', () => {
        const potentialID = document.getElementById('InputCollectionItemPotentialID').value.trim();
        const collectionAreaID = document.getElementById('InputCollectionAreaIDSearch').value.trim();
        fetch('/api/collections/listPotentials', {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    potentialID: potentialID,
                    collectionAreaID: collectionAreaID
                }),
            })
            .then(res => {
                if (!res.ok) throw new Error(res.statusText);
                return res.json();
            })
            .then(result => {
                const table = document.getElementById('CollectionItemPotentialSearchResultTable');
                const oldTbody = document.getElementById('CollectionItemPotentialSearchResultTableBody');
                if (oldTbody) oldTbody.remove();
                const tbody = table.createTBody();
                tbody.id = 'CollectionItemPotentialSearchResultTableBody';

                if (result.length > 0) {
                    result.forEach((element, idx) => {
                        tbody.appendChild(buildCollectionItemPotentialSearchResultRow(element, idx));
                    });
                } else {
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

function buildCollectionItemPotentialSearchResultRow(element, idx) {
    const tr = document.createElement('tr');
    tr.id = `CollectionItemPotentialSearchResult_${idx}`;

    const pictureTd = document.createElement('td');   
    const imgPath = `/images/Thumbnail/${element.leadingPictureID}.webp`;    
    const img = document.createElement('img');
    img.src = imgPath;
    img.alt = 'Vorschaubild';
    img.style.maxWidth = '100px';
    img.style.maxHeight = '100px';
    img.style.objectFit = 'contain';
    pictureTd.appendChild(img);
    tr.appendChild(pictureTd);

    tr.appendChild(createTd({
        text: element.collectionItemPotentialID,
        id: `CollectionItemPotentialSearchResultPotentialID_${idx}`,
        scope: 'row'
    }));

    const tdAction = document.createElement('td');
    tdAction.id = `CollectionItemPotentialSearchResultAction_${idx}`;

    const btn = document.createElement('button');
    btn.type = 'button';
    btn.className = 'btn btn-primary';
    btn.id = `CollectionItemPotentialSearchResult_${idx}`;
    btn.textContent = i18n.get("Add");
    btn.onclick = () => SetCollectionItemPotentialIntoTable(idx);
    tdAction.appendChild(btn);

    tr.appendChild(tdAction);
    return tr;
}

const processOfManufactoryToggle = document.querySelector('.processOfManufactureSearchSubmit')
if (processOfManufactoryToggle) {
    processOfManufactoryToggle.addEventListener('click', () => {
        const process = document.getElementById('inputProcessOfManufactureSearch').value.trim();
        fetch('/api/collections/listProcessOfManufacture?process=' + encodeURIComponent(process))
            .then(res => {
                if (!res.ok) throw new Error(res.statusText);
                return res.json();
            })
            .then(result => {
                const table = document.getElementById('processOfManufactureSearchResultTable');
                const oldTbody = document.getElementById('processOfManufactureSearchResultTableBody');
                if (oldTbody) oldTbody.remove();
                const tbody = table.createTBody();
                tbody.id = 'processOfManufactureSearchResultTableBody';

                if (result.length > 0) {
                    result.forEach((element, idx) => {
                        tbody.appendChild(buildProcessOfManufactureSearchResultRow(element, idx));
                    });
                } else {
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

function buildProcessOfManufactureSearchResultRow(element, idx) {
    const tr = document.createElement('tr');
    tr.id = `processOfManufactureSearchResult_${idx}`;

    tr.appendChild(createTd({
        text: element.processOfManufactureID,
        id: `processOfManufactureSearchResultprocessOfManufactureID_${idx}`,
        scope: 'row'
    }));

    tr.appendChild(createTd({
        text: element.mainprocess,
        id: `processOfManufactureSearchResultMainprocess_${idx}`,
        scope: 'row'
    }))

    tr.appendChild(createTd({
        text: element.processOfManufactureName,
        id: `processOfManufactureSearchResultProcessOfManufactureName_${idx}`,
        scope: 'row'
    }))

    tr.appendChild(createTd({
        text: element.technique,
        id: `processOfManufactureSearchResultTechnique_${idx}`
    }));

    tr.appendChild(createTd({
        text: element.geography?.geographyName ?? '',
        id: `processOfManufactureSearchResultDescription_${idx}`
    }));

    const tdAction = document.createElement('td');
    tdAction.id = `processOfManufactureSearchResultAction_${idx}`;
    const btn = document.createElement('button');
    btn.type = 'button';
    btn.className = 'btn btn-primary';
    btn.id = `processOfManufactureSearchResult_${idx}`;
    btn.textContent = i18n.get("Add");
    btn.onclick = () => SetProcessOfManufactureIntoTable(idx);
    tdAction.appendChild(btn);

    tr.appendChild(tdAction);
    return tr;
}