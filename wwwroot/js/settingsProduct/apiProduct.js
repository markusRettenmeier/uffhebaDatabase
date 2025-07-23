document.addEventListener("DOMContentLoaded", () => {
    handlePageLoad();
});

function handlePageLoad() {
    const url = window.location.href;

    if (url.includes("BrickDatabase")) {
        getManufactoryList();
        getColorList();
    }
}

async function getManufactoryList() {
    const productionFacilityValue = document.getElementById("productionFacilityInput").value;
    const url = "/api/collections/listManufactorys";
    try {
        const response = await fetch(url, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                productionFacility: productionFacilityValue,
                manufactory: "",
                oeconomy: ""
            }),
        });
        if (!response.ok) {
            throw new Error(`Response status: ${response.status}`);
        }

        const json = await response.json();
        sessionStorage.setItem('manufactoryList', JSON.stringify(json));
        //console.log('Gespeichert:', sessionStorage.getItem('manufactoryList'));
    } catch (error) {
        console.error(error.message);
    }
}

async function getColorList() {
    try {
        const response = await fetch("/api/collections/listColors");
        if (!response.ok) {
            throw new Error(`Response status: ${response.status}`);
        }

        const json = await response.json();
        sessionStorage.setItem('colorList', JSON.stringify(json));
        console.log('Gespeichert:', sessionStorage.getItem('colorList'));
    } catch (error) {
        console.error(error.message);
    }
}

$(document.body).on('keydown', 'input.InputManufactory', function (event) {
    const manufactoryList = JSON.parse(sessionStorage.getItem('manufactoryList') || '[]');
    const autocompleteSource = manufactoryList.map(x => ({
        label: x.manufactoryName,
        value: x.manufactoryName,
        id: x.manufactoryID,
        cities: x.cityList
    }));

    $(this).autocomplete({
        source: autocompleteSource,
        select: function (event, ui) {
            const selected = ui.item;

            const inputGroup = $(this).closest('.input-group');
            inputGroup.find('.InputManufactoryID').val(selected.id);

            const selectElement = inputGroup.find('.TownOfManufactorySelect');
            selectElement.empty();
            if (selected.cities && selected.cities.length > 0) {
                selectElement.append(`<option value="">Ort wählen</option>`);
                selected.cities.forEach(city => {
                    const oeconym = city.oeconym || 'Unbekannt';
                    selectElement.append(`<option value="${city.cityID}">${oeconym}</option>`);
                });
            } else {
                selectElement.append('<option disabled>Keine Ort vorhanden</option>');
            }
        }
    });
})

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


$(".ExistingBricknameSearchSubmit").on('click', function () {
    var brickname = $('.InputBricknameSearch').val()
    var usageInt = document.getElementById("SelectUsage").value
    $.ajax({
        url: '/api/collections/listBricknames',
        data: {
            brickname: brickname,
            usageInt: usageInt
        },
        success: function (result) {
            let TableBody = document.getElementById('bricknameSearchResultTableBody')
            if (TableBody != null)
                TableBody.remove()
            let myTable = document.getElementById("bricknameSearchResultTable")
            let mybody = myTable.createTBody()
            mybody.setAttribute('id', 'bricknameSearchResultTableBody');
            if (result.length > 0) {
                result.forEach(function (element, count) {
                    var innerHtml = '<tr id="bricknameSearchResult_' + count + '">'
                    if (element.brickPotential != null) {
                        innerHtml += '<td scope="row" id="bricknameSearchResultBrickPotentialID_' + count + '">' + element.brickPotential.brickPotentialID + '</td>'
                    } else {
                        innerHtml += '<td scope="row" id="bricknameSearchResultBrickPotentialID_' + count + '"></td>'
                    }
                    innerHtml += '<td scope="row" id="bricknameSearchResultName_' + count + '">' + element.name + '</td>'
                    if (element.brickPotential != null) {
                        innerHtml += '<td scope="row" id="bricknameSearchResultUsage_' + count + '">' + element.brickPotential.usageEnumDescription + '</td>'
                    }
                    else {
                        innerHtml += '<td scope="row" id="bricknameSearchResultUsage_' + count + '"></td>'
                    }
                    innerHtml += '<td id="bricknameSearchResultAction_' + count + '" ><button class="btn btn-primary " id="bricknameSearchResult_' + count + '" onclick="SetBricknameIntoTable(' + count + ')" type="button">Hinzufügen</button></td></tr>'
                    $('#bricknameSearchResultTable').find('tbody').append(innerHtml)
                });
            }
            else {
                $('#bricknameSearchResultTable').find('tbody').append('<tr><td>Kein Eintrag vorhanden, bitte Ziegelname erstellen</td></tr>')
            }
        },
        error: function (xhr) {
            sendErrorMessage(xhr)
        }
    })
})

$(".ExistingPersonSearchSubmit").on('click', function () {
    const name = document.getElementById('inputPersonNameSearch').value;
    const signature = document.getElementById('inputPersonSignatureSearch').value;
    const pseudonym = document.getElementById('inputPersonPseudonymSearch').value;
    $.ajax({
        url: '/api/collections/listPersons',
        data: {
            name: name,
            signature: signature,
            pseudonym: pseudonym
        },
        success: function (result) {
            const tableBody = document.getElementById('personSearchResultTableBody')
            if (tableBody != null)
                tableBody.remove()
            let myTable = document.getElementById("personSearchResultTable")
            let mybody = myTable.createTBody()
            mybody.setAttribute('id', 'personSearchResultTableBody');
            if (result.length > 0) {
                result.forEach(function (element, count) {
                    var innerHtml = '<tr id="personSearchResult_' + count + '">'
                    innerHtml += '<td scope="row" id="personSearchResultPersonID_' + count + '">' + element.personID + '</td>'
                    innerHtml += '<td scope="row" id="personSearchResultName_' + count + '">' + element.name + '</td>'
                    innerHtml += '<td scope="row" id="personSearchResultPersonPseudonym_' + count + '">' + element.pseudonym + '</td>'
                    innerHtml += '<td scope="row" id="personSearchResultPersonSignature_' + count + '">' + element.signature + '</td>'
                    innerHtml += '<td id="personSearchResultAction_' + count + '" ><button class="btn btn-primary " id="personSearchResult_' + count + '" onclick="SetPersonIntoTable(' + count + ')" type="button">Hinzufügen</button></td></tr>'
                    $('#personSearchResultTableBody').append(innerHtml)
                });
            }
            else {
                $('#personSearchResultTableBody').append('<tr><td>Kein Eintrag vorhanden, bitte Person erstellen</td></tr>')
            }
        },
        error: function (xhr) {
            sendErrorMessage(xhr)
        }
    })
})
$(".eraSearchSubmit").on('click', function () {
    const name = document.getElementById('inputEraNameSearch').value;
    $.ajax({
        url: '/api/collections/listEras',
        data: {
            name: name
        },
        success: function (result) {
            const tableBody = document.getElementById('eraSearchResultTableBody')
            if (tableBody != null)
                tableBody.remove()
            let myTable = document.getElementById("eraSearchResultTable")
            let mybody = myTable.createTBody()
            mybody.setAttribute('id', 'eraSearchResultTableBody');
            if (result.length > 0) {
                result.forEach(function (element, count) {
                    var innerHtml = '<tr id="personSearchResult_' + count + '">'
                    innerHtml += '<td scope="row" id="eraSearchResultEraID_' + count + '">' + element.eraID + '</td>'
                    innerHtml += '<td scope="row" id="eraSearchEraName_' + count + '">' + element.eraName + '</td>'
                    innerHtml += '<td id="eraSearchResultAction_' + count + '" ><button class="btn btn-primary " id="eraSearchResult_' + count + '" onclick="SetEraIntoTable(' + count + ')" type="button">Hinzufügen</button></td></tr>'
                    $('#eraSearchResultTableBody').append(innerHtml)
                });
            }
            else {
                $('#eraSearchResultTableBody').append('<tr><td>Kein Eintrag vorhanden, bitte Person erstellen</td></tr>')
            }
        },
        error: function (xhr) {
            sendErrorMessage(xhr)
        }
    })
})

const manufactoryToggle = document.querySelector(".ExistingManufactorySearchSubmit")
if (manufactoryToggle) {
    manufactoryToggle.addEventListener('click', () => {
        var manufactory = $('.InputManufactorySearch').val()
        var productionFacility = $('.InputProductionFacilitySearch').val()
        var oeconym = $('.InputOeconymSearch').val()
        $.ajax({
            url: '/api/collections/listManufactorys',
            data: {
                manufactory: manufactory,
                productionFacility: productionFacility,
                oeconym: oeconym
            },
            success: function (result) {
                let TableBody = document.getElementById('manufactorySearchResultTableBody')
                if (TableBody != null)
                    TableBody.remove()
                let myTable = document.getElementById("manufactorySearchResultTable")
                let mybody = myTable.createTBody()
                mybody.setAttribute('id', 'manufactorySearchResultTableBody');
                if (result.length > 0) {
                    result.forEach(function (element, count) {
                        var innerHtml = '<tr id="manufactorySearchResult_' + count + '">'
                        innerHtml += '<td scope="row" id="manufactorySearchResultManufactoryID_' + count + '">' + element.manufactory_ID + '</td>'
                        innerHtml += '<td scope="row" id="manufactorySearchResultName_' + count + '">' + element.manufactoryName + '</td>'
                        innerHtml += '<td scope="row" ><select class="TownOfManufactorySelect form-select" aria-label="Ziegeleiort" id="manufactorySearchResultCity_' + count + '"></td>'
                        innerHtml += '<td scope="row" id="manufactorySearchResultProductionFacility_' + count + '">' + element.productionFacility.productionFacilityName + '</td>'
                        innerHtml += '<td id="manufactorySearchResultAction_' + count + '" ><button class="btn btn-primary" onclick="SetManufactoryIntoTable(' + count + ')" type="button">Hinzufügen</button></td></tr>'
                        $('#manufactorySearchResultTable').find('tbody').append(innerHtml)
                        let select = document.getElementById('manufactorySearchResultCity_' + count)
                        createOptionInSelect('', select, 'Wählen Sie einen Ort aus, falls genannt')
                        if (element.cityICollection != null) {
                            if (element.geography != null)
                                createOptionInSelect(element.cityICollection[0].city_ID, select, element.cityICollection[0].cityNOeconymICollection[0].oeconym.oeconymName + ' (' + element.geography.geographyName + ')')
                            else
                                createOptionInSelect(element.cityICollection[0].city_ID, select, element.cityICollection[0].cityNOeconymICollection[0].oeconym.oeconymName)
                        }
                    });
                }
                else {
                    $('#manufactorySearchResultTable').find('tbody').append('<tr><td>Kein Eintrag vorhanden, bitte Ziegelei erstellen</td></tr>')
                }
            },
            error: function (xhr) {
                sendErrorMessage(xhr)
            }
        })
    })
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
    btn.textContent = 'Hinzufügen';
    btn.onclick = () => SetProcessOfManufactureIntoTable(idx);
    tdAction.appendChild(btn);

    tr.appendChild(tdAction);
    return tr;
}
function createOptionInSelect(value, select, innerHtml) {
    let newOption = document.createElement("option")

    newOption.innerHTML = innerHtml
    newOption.value = value
    select.appendChild(newOption)
}