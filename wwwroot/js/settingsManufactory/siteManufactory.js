function AddCityToManufactory(buttonId) {
    RemoveRowFromTable("trCity_0")
    addCityMultiRow(buttonId);
}

function addCityMultiRow(buttonId) {
    var lastId = $('#cityTable tr:last').attr('id');
    var lastNumber = 0;
    if (lastId != undefined) {
        lastNumber = parseInt(lastId.split('_').pop()) + 1
    }

    var innerHtml = '<tr id="trCity_' + lastNumber + '">';
    var value = document.getElementById('citySearchResultcityID_' + buttonId).innerHTML;
    innerHtml += '<td scope="col">' + value + '</td>';
    value = document.getElementById('citySearchResultOecoynm_' + buttonId).innerHTML;
    innerHtml += '<td scope="col">' + value + '</td>';
    value = document.getElementById('citySearchResultPostalcode_' + buttonId).innerHTML;
    innerHtml += '<td scope="col">' + value + '</td>';
    value = document.getElementById('citySearchResultByname_' + buttonId).innerHTML;
    innerHtml += '<td scope="col">' + value + '</td>';
    value = document.getElementById('citySearchResultGeography_' + buttonId).innerHTML;
    innerHtml += '<td scope="col">' + value + '</td>';
    innerHtml += '<td scope="col"><button type="button" id="' + lastNumber + '" class="btn btn-outline-danger deleteCity">Entfernen</button></td>';
    innerHtml += '</tr>';
    $('#cityTable').find('tbody').append(innerHtml);
    $('#IsCityExistingModal').modal('hide');
}
//document.getElementById.on('click', '.deleteCity', function () {
//    let id = $(this).prop('id')
//    document.querySelector('#trCity_' + id)?.remove()
//})
function RemoveRowFromTable(row) {
    let tr = document.getElementById(row);
    if (tr != null) {
        tr.remove()
    }
}

$('.addCityCreateManufactory').on('click', AddCityCreateManufactory);
function AddCityCreateManufactory() {
    let source = $('.CityOriginalCreateManufactory'), clone = source.clone(true)
    let lastElement = $('.InputCity').last()
    let lastId = lastElement.attr('id')
    let splittedId = lastId.split('_').pop()
    let currentId = parseInt(splittedId) + 1

    clone.attr('class', 'input-group pb-1 DivCity').attr('id', 'DivCity_' + currentId)
    clone.find('.InputCity').attr('id', 'InputCity_' + currentId).val('')
    clone.find('.addCityCreateManufactory').remove()
    clone.find('.removeCityCreateManufactory').attr('id', currentId).show()

    clone.appendTo('.appendCityCreateManufactory');
}
$('.removeCityCreateanufacturer').on('click', function () {
    let id = $(this).prop('id')
    $('div#DivCity_' + id).remove()
})