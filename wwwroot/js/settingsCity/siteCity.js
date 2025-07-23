
function AddParentCity(buttonId) {
    SetCityIntoTable(buttonId);
}

function SetCityIntoTable(buttonId) {
    var value = document.getElementById('citySearchResultcityID_' + buttonId).innerHTML;
    document.getElementById('city_ID').innerHTML = value;
    value = document.getElementById('citySearchResultOecoynm_' + buttonId).innerHTML;
    document.getElementById('city').innerHTML = value;
    value = document.getElementById('citySearchResultPostalcode_' + buttonId).innerHTML;
    document.getElementById('postalcode').innerHTML = value;
    value = document.getElementById('citySearchResultByname_' + buttonId).innerHTML;
    document.getElementById('byname').innerHTML = value;
    value = document.getElementById('citySearchResultGeography_' + buttonId).innerHTML;
    document.getElementById('geography').innerHTML = value;

    $('#clearOneRowCityTable').show()
    $('#IsCityExistingModal').modal('hide')
}
$('#clearOneRowCityTable').on('click', function () {
    document.getElementById('city_ID').innerHTML = ''
    document.getElementById('city').innerHTML = ''
    document.getElementById('postalcode').innerHTML = ''
    document.getElementById('byname').innerHTML = ''
    document.getElementById('geography').innerHTML = ''
    $('#clearOneRowCityTable').hide()
})


$('.addOeconymCreateCity').on('click', AddOeconymCreateCity);
function AddOeconymCreateCity() {
    let source = $('.oeconymOriginalCreateCity'), clone = source.clone(true)
    let lastElement = $('.InputOeconymName').last()
    let lastId = lastElement.attr('id')
    let splittedId = lastId.split('_').pop()
    let currentId = parseInt(splittedId) + 1

    clone.attr('class', 'input-group pb-1 DivOeconym').attr('id', 'DivOeconym_' + currentId)
    clone.find('.InputOeconymName').attr('id', 'InputOeconymName_' + currentId).val('')
    clone.find('.InputCurrentName').attr('id', 'InputCurrentName_' + currentId).val('')
    clone.find('.addOeconymCreateCity').remove()
    clone.find('.removeOeconymCreateCity').attr('id', currentId).show()

    clone.appendTo('.appendOeconymCreateCity');
}
$('.removeOeconymCreateCity').on('click', function () {
    let id = $(this).prop('id')
    $('div#DivOeconym_' + id).remove()
})

$(".createCitySubmitButton").on('click', function () {
    var value = document.getElementById('city_ID').innerHTML
    $('#parentCityInput').val(value)

    var oeconym = ''
    var count = 1;
    $(".InputPostalcodeNumber").each(function () {
        if (this.value != '') {
            createNewInput('PostalcodeNumberList', this.value.trim())
        }
    });
    $(".InputOeconymName").each(function () {
        if (this.value != '') {
            var oeconymName = this.value.trim()
            var currentName = $('#InputCurrentName_' + count).is(":checked")
            oeconym = oeconymName + "§§" + currentName
        }
        count++
        createNewInput('OeconymList', oeconym)
    });
})
function createNewInput(name, value) {
    $('<input>').attr({
        type: 'hidden',
        name: name,
        value: value
    }).appendTo('form')
}