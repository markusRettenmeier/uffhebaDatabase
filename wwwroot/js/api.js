$(document.body).on('keydown', 'input.InputGeographyName',function (event) {
    $(this).autocomplete({
        source: '/api/collections/autocompleteGeographyName'
    })
})
$(document.body).on('keydown', 'input.InputEra', function (event) {
    $(this).autocomplete({
        source: '/api/collections/autocompleteEra'
    })
})
$(document.body).on('keydown', 'input.InputManufactory', function (event) {
    $(this).autocomplete({
        source: '/api/collections/autocompleteManufactory'
    })
})
$(document.body).on('keydown', 'input.InputProductionFacility', function (event) {
    $(this).autocomplete({
        source: '/api/collections/autocompleteProductionFacility'
    })
})

$(function () {
    $(document.body).on('change', 'input.InputGeographyName', function (event) {
        callAjaxGeographyID()
    })
})
$(function () {
    $(document.body).on('change', 'input.InputEra', function (event) {
        var currentId = $(this).prop('id').split('_').pop()
        callAjaxEraID(currentId)
    })
})
$(function () {
    $(document.body).on('change', 'input.InputManufactory', function (event) {
        var currentId = $(this).prop('id').split('_').pop()
        callAjaxManufactoryID(currentId)
    })
})
$(function () {
    $(document.body).on('change', 'input.InputProductionFacility', function (event) {
        var currentId = $(this).prop('id').split('_').pop()
        callAjaxProductionFacilityID(currentId)
    })
})

function callAjaxGeographyID(InputId) {
    var geography = $('#InputGeographyName_' + InputId).val()
    if (geography != '') {
        $.ajax({
            url: '/api/collections/autocompleteGeographyID',
            data: { term: geography },
            success: function (result) {
                $('#InputGeographyID_' + InputId).val(result)
            },
            error: function (xhr, status, error) {
                sendErrorMessage(xhr)
            }
        })
    }
}

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

function callAjaxManufactoryID(Id) {
    var manufactory = $('#InputManufactory_' + Id).val()
    if (manufactory != '') {
        $.ajax({
            url: '/api/collections/autocompleteManufactoryID',
            data: { term: manufactory },
            success: function (result) {
                if (result == '')
                    $('#InputManufactory_' + Id).val('Bitte Verlag erstellen').css('color', 'red')
                else {
                    $('#InputManufactoryID_' + Id).val(result)
                    $.ajax({
                        url: '/api/collections/autocompleteCityOfManufactory',
                        data: { term: result },
                        success: function (newResult) {
                            let select = document.getElementById('TownOfManufactorySelect_' + Id)
                            // if sth else was selected before, replaceChildren clears the former options
                            select.replaceChildren()
                            createOptionInSelect('', select, 'Wählen Sie einen Ort aus, falls genannt')
                            if (newResult.length > 0) {
                                newResult.forEach(function (element) {
                                    if (element.cityNOeconymICollection != null) {
                                        if (element.geography != null)
                                            createOptionInSelect(element.city_ID, select, element.cityNOeconymICollection[0].oeconym.oeconymName + ' (' + element.geography.geographyName + ')')
                                        else
                                            createOptionInSelect(element.city_ID, select, element.cityNOeconymICollection[0].oeconym.oeconymName)
                                    }
                                })
                            }
                                else {
                                    $('#citySearchResultTable').find('tbody').append('<tr><td>Kein Eintrag vorhanden, bitte Ort erstellen</td></tr>')
                                }
                        },
                        error: function (xhr, status, error) {
                            sendErrorMessage(xhr)
                        }
                    })
                }
            },
            error: function (xhr, status, error) {
                sendErrorMessage(xhr)
            }
        })
    }
}

function callAjaxProductionFacilityID() {
    var industrialSector = $('.InputProductionFacility').val()
    if (industrialSector != '') {
        $.ajax({
            url: '/api/collections/autocompleteProductionFacilityID',
            data: { term: industrialSector },
            success: function (result) {
                $('.InputProductionFacilityID').val(result)
                if (result == '')
                    $('.InputProductionFacility').val('Bitte Branche über Administrator erfassen').css('color', 'red')
                else
                    $('.InputProductionFacility').css('color', 'black')
            },
            error: function (xhr, status, error) {
                sendErrorMessage(xhr)
            }
        })
    }
}

$(".ExistingCitiesSearchSubmit").on('click', function () {
    var city = $('.InputOeconymSearch').val()
    $.ajax({
        url: '/api/collections/listCities',
        data: { term: city },
        success: function (result) {
            let TableBody = document.getElementById('citySearchResultTableBody')
            if (TableBody != null)
                TableBody.remove()
            let myTable = document.getElementById("citySearchResultTable")
            let mybody = myTable.createTBody()
            mybody.setAttribute('id', 'citySearchResultTableBody');
            if (result.length > 0) {
                result.forEach(function (element, count) {
                    var innerHtml = '<tr id="citySearchResult_' + count + '">'
                    innerHtml = CitySearchResultInnerHtml(innerHtml, count, element)
                    if (window.location.href.indexOf('PostcardDatabase') > -1) {
                        innerHtml += '<td id="citySearchResultAction_' + count + '" ><div class="btn-group" role="group" aria-label="Assign City"><button class="btn btn-primary " id="citySearchResult_' + count + '" onclick="AddCityToPostcard(' + count + ')" type="button">Zu Postkarte hinzufügen</button><button type="button" class="btn btn-primary" id="citySearchResult_' + count + '" onclick="AddCityToReceiver(' + count + ')">Zu Empfänger hinzufügen</button></div></td></tr >'
                    } else if (window.location.href.indexOf('CityDatabase') > -1) {
                        innerHtml += '<td id="citySearchResultAction_' + count + '"><button class="btn btn-primary SearchResultButton" id="citySearchResult_' + count + '" onclick="AddParentCity(' + count + ')" type="button">Auswählen</button></td></tr>'
                    } else if (window.location.href.indexOf('ManufactoryDatabase') > -1) {
                        innerHtml += '<td id="citySearchResultAction_' + count + '"><button class="btn btn-primary SearchResultButton" id="citySearchResult_' + count + '" onclick="AddCityToManufactory(' + count + ')" type="button">Auswählen</button></td></tr>'
                    }
                    $('#citySearchResultTable').find('tbody').append(innerHtml)
                });
            }
            else {
                $('#citySearchResultTable').find('tbody').append('<tr><td>Kein Eintrag vorhanden, bitte Ort erstellen</td></tr>')
            }
        },
        error: function (xhr, status, error) {
            sendErrorMessage(xhr)
        }
    })
})

// Stripe: In production, this should check CSRF, and not pass the session ID.
// The customer ID for the portal should be pulled from the
// authenticated user on the server.
document.addEventListener('DOMContentLoaded', async () => {
    let searchParams = new URLSearchParams(window.location.search);
    if (searchParams.has('session_id')) {
        const session_id = searchParams.get('session_id');
        document.getElementById('session-id').setAttribute('value', session_id);
    }
});

$(".createManufactorySubmitApiButton").on('click', function () {
    var manufactoryName = $(".ManufactoryModalInputName").val()
    var productionFacilityName = $(".ManufactoryModalInputProductionFacility").val()

    const createManufactorySpan = $("#createManufactorySpan");
    if (manufactoryName == undefined) {
        displayErrorMessage(createManufactorySpan, 'Herstellername fehlt');
    } else {
        var requestData = {
            Manufactory: {
                ManufactoryName: manufactoryName
            },
            ProductionFacility: {
                ProductionFacilityName: productionFacilityName
            }
        };
        $.ajax({
            url: '/Api/ManufactoryDatabaseRestAPI/CreateManufactorySubmit',
            data: JSON.stringify(requestData),
            type: "POST",
            contentType: "application/json",
            success: function (result) {
                $("#createManufactorySpan").text(result);
            },
            error: function (xhr, status, error) {
                sendErrorMessage(xhr)
            }
        });
    }
})

$(".createCitySubmitApiButton").on('click', function () {
    var postalcodeNumberList = [];
    $(".CityModalInputPostalcodeNumber").each(function () {
        if (this.value != '') {
            postalcodeNumberList.push(this.value)
        }
    });
    var oeconymList = []
    oeconymList.push($(".CityModalInputOeconym").val()+ '§§true')
    var Byname = $(".CityModalInputByname").val()
    var geographyName = $(".CityModalInputGeographyName").val()

    const createCitySpan = $("#createCitySpan");
    if (postalcodeNumberList.length === 0) {
        displayErrorMessage(createCitySpan, 'PLZ fehlt');
    } else if (oeconymList.length === '') {
        displayErrorMessage(createCitySpan, 'Ort fehlt');
    } else {
        var requestData = {
            City: {
                Byname: Byname
            },
            Geography: {
                GeographyName: geographyName
            },
            OeconymList: oeconymList,
            PostalcodeNumberList: postalcodeNumberList
        };
        $.ajax({
            url: '/Api/CityDatabaseRestAPI/CreateCitySubmit',
            data: JSON.stringify(requestData),
            type: "POST",
            contentType: "application/json",
            success: function (result) {
                $("#createCitySpan").text(result);
            },
            error: function (xhr, status, error) {
                sendErrorMessage(xhr)
            }
        });
    }
})

function createOptionInSelect(value, select, innerHtml) {
    let newOption = document.createElement("option")
    
    newOption.innerHTML = innerHtml
    newOption.value = value
    select.appendChild(newOption)
}

function CitySearchResultInnerHtml(innerHtml, count, element1) {
    innerHtml += '<td scope="row" id="citySearchResultcityID_' + count + '">' + element1.city_ID + '</td>'
    innerHtml += '<td id="citySearchResultOecoynm_' + count + '">'
    if (element1.cityNOeconymICollection) {
        element1.cityNOeconymICollection.forEach(function(element2) {
            if (element2.currentName) {
                innerHtml += '<strong>' + element2.oeconym.oeconymName + '</strong>, '
            }
            else {
                innerHtml += element2.oeconym.oeconymName + ', '
            }
        })
    }
    innerHtml += '</td>'
    innerHtml += '<td id="citySearchResultPostalcode_' + count + '">'
    if (element1.postalcodeICollection) {
        element1.postalcodeICollection.forEach(function (element2) {
            innerHtml += element2.postalcodeNumber + ', '
        })
    }
    innerHtml += '</td>'
    innerHtml += '<td id="citySearchResultByname_' + count + '">'
    if (element1.Byname != null)
        innerHtml += element1.Byname
    innerHtml += '</td>'
    innerHtml += '<td id="citySearchResultGeography_' + count + '">'
    if (element1.geography != null)
        innerHtml += element1.geography.geographyName
    innerHtml += '</td>'

    return innerHtml
}

function getValueById(id) {
    return $(`#${id}`).val().trim();
}

function sendErrorMessage(xhr) {
    const createCitySpan = $("#createCitySpan");
    var errorMessage = xhr.status + ': ' + xhr.statusText
    console.log('AJAX Error: ' + errorMessage)
    displayErrorMessage(createCitySpan, errorMessage)

    // Log or display the detailed error message returned by the server
    console.log(xhr.responseText)
}
function displayErrorMessage(element, message) {
    element.text(message).css('color', 'red');
}