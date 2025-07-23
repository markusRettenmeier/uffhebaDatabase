$(function () {
    $(document.body).on('change', 'input.InputEra', function (event) {
        var currentId = $(this).prop('id').split('_').pop()
        callAjaxEraID(currentId)
    })
})

function callAjaxEraID(Id) {
    var era = $('#InputEra_' + Id).val()
    if (era != '') {
        $.ajax({
            url: '/api/collections/autocompleteEraID',
            data: { term: era },
            success: function (result) {
                if (result == '')
                    $('#InputEra_' + Id).val('Bitte Verlag erstellen').css('color', 'red')
                else {
                    $('.InputEraID_' + Id).val(result)
                    if (result == '')
                        $('.InputEra').val('Bitte Ära erfassen').css('color', 'red')
                    else
                        $('.InputEra').css('color', 'black')
                }
            },
            error: function (xhr, status, error) {
                sendErrorMessage(xhr)
            }
        })
    }
}

const toggle = document.querySelector('.ExistingCitiesSearchSubmit')
if (toggle) {
    toggle.addEventListener('click', () => {
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
                const context = getContextFromUrl();

                if (result.length > 0) {
                    result.forEach((element, idx) => {
                        tbody.appendChild(buildCitySearchResultRow(element, idx, context));
                    });
                } else {
                    returnSearchResultEmpty('city')
                }
            })
            .catch(err => {
                sendErrorMessage(err);
            });
    });
}

function getContextFromUrl() {
    const url = window.location.href;
    if (url.includes('PostcardDatabase')) return 'PostcardDatabase';
    if (url.includes('BrickDatabase')) return 'BrickDatabase';
    if (url.includes('CityDatabase')) return 'CityDatabase';
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

    //if (context === 'PostcardDatabase') {
    //    btn.textContent = 'Zu Postkarte hinzufügen';
    //    btn.onclick = () => AddCityMultiRow(idx);
    //    tdAction.appendChild(btn);
    //    const btn2 = document.createElement('button');
    //    btn2.className = 'btn btn-primary';
    //    btn2.type = 'button';
    //    btn2.textContent = 'Zu Empfänger hinzufügen';
    //    btn2.onclick = () => AddCityToReceiver(idx);
    //    tdAction.appendChild(btn2);
//} else
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
function sendErrorMessage(xhr) {
    console.log("fetch-Error:" + xhr)
    const createCitySpan = $("#createCitySpan");
    createCitySpan.text(message).css('color', 'red');
}