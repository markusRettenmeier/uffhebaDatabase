// Pleaset see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

$('body').on('click', '#generatePDF', generatePDF)
var countUploadfiles = 0;

/* Set the width of the searchsidebar to 600px and the left margin of the page content to 600px */
function openNav() {
    if (window.innerWidth <= 600)
        document.getElementById("mySidebar").style.width = "100%"
    else
        document.getElementById("mySidebar").style.width = "600px"
}

/* Set the width of the sidebar to 0 and the left margin of the page content to 0 */
function closeNav() {
    document.getElementById("mySidebar").style.width = "0"
}

function generatePDF() {
    const element = document.getElementById("AnalysisDiv")
    const opt = {
        margin: [10,10,10,10],
        filename: 'Analyse_Ergebnis.pdf',
        image: { type: 'jpeg', quality: 0.98 },
        html2canvas: { scale: 2, y: 0 },
        pagebreak: {mode: 'avoid-all'},
        jsPDF: { unit: 'mm', format: 'letter', orientation: 'portrait' }
    }
    html2pdf().set(opt).from(element).save()
}

$(".AddSiteDeleteBtn").on('click', function () {
    let my_id_value = $(this).data('id')
    $(".modal-body #AddSitesDeleteText").text(my_id_value)
    $('[name="AddSitesDeleteConfirm"]').val(my_id_value)
})
function startSpinner() {
    var fileInput1 = document.getElementById('fileInput1');
    var fileInput2 = document.getElementById('fileInput2');
    if (fileInput1.files.length != 0 && fileInput2.files.length != 0) {
        $(".submitButton").hide()
        $(".spinnerButton").show()
    } else
        alert('Vorder- und Rückseite werden benötigt')
}

//Show and hide password in Login
$(function () {
    $('#show_password').on("mouseenter", handlerIn).on("mouseleave", handlerOut)
    function handlerIn()
    {
        //Change the attribute to text
        $('#txtPassword').attr('type', 'text')
        $('.icon').removeClass('fa fa-eye-slash').addClass('fa fa-eye')
    }
    function handlerOut()
    {
        //Change the attribute back to password
        $('#txtPassword').attr('type', 'password')
        $('.icon').removeClass('fa fa-eye').addClass('fa fa-eye-slash')
    }
    //CheckBox Show Password
    $('#ShowPassword').on('click', function () {
        $('#Password').attr('type', $(this).is(':checked') ? 'text' : 'password');
    });
})

//Für Register
var current = location.pathname;
$('.nav-tabs li a').each(function () {
    var $this = $(this);
    if (current.indexOf($this.attr('href')) !== -1)
        $this.addClass('active');
})

//Wenn Farbe in Auswahlfenster geändert wurde, dann soll auch RGB geändert werden
$('body').on('input', '.color', colorChange)
function colorChange() {
    let id = this.id.split('_').pop()
    let color = this.value
    let hexInput = document.querySelector('#hex_' + id)
    hexInput.value = color
}

// Nur wenn einfarbig, dann Farbe wählbar
$('body').on("change", '.colorProcessing', ImageColorChange)
function ImageColorChange() {
    if (this.value == 2)
        $('.colorImage').show()
    else
        $('.colorImage').hide()
}

//Wenn AddEera geklickt wurde, dann sollen bereits eingegebene Daten gesichert werden
$('body').on('click', '.StoreInputs', StoreInputs)
function StoreInputs() {
    document.querySelectorAll(".storeInput").forEach((el, idx) => {
        let id = el.id
        if (id != null) {
            let value = el.value
            if (el.type == 'checkbox' && el.checked == false)
                value = false
            if (value != '' && value != false && value != 0 && value != '#000000')
                sessionStorage.setItem(id, value)
        }
    });
}

//Aus AddCity, AddEra, AddManufactory zurück, sollen alle zuvor zwischengespeicherte Daten automaitsch wieder befülltl werden
if (window.location.href.indexOf('Database/Create') > -1) {
    for (const [key, value] of Object.entries(sessionStorage)) {
        let keyArray = key.split('_')
        if (keyArray[1] > 1) {
            switch (keyArray[0]) {
                case "InputCity":
                    AddMoreCities()
                    break;
                case "ManufactoryID":
                    AddMoreManufactory()
                    break;
            }
        }
        let element = $('#' + key).val(value)
        if (element.is(':checkbox'))
            element.prop('checked', true)
        sessionStorage.removeItem(key)
    }
}

//Anzeigen von PostcardEntities, abhängig von PostcardPotential_ID
function listEntityShow(PotentialId) {
    $('#' + PotentialId).show()
}

$('.acceptAnalyzedValue').on('click', function () {
    let nameID = $(this).prop('name').split('_').pop()
    let id = $(this).prop('id')
    $('#Input_' + nameID).prop('name', id + 'WhiteList')
})
$('.declineAnalyzedValue').on('click', function () {
    let nameID = $(this).prop('name').split('_').pop()
    let id = $(this).prop('id')
    $('#Input_' + nameID).prop('name', id + 'BlackList')
})

function ConcludeLists() {
    var publisher = '';
    var publisherCount = 0;
    var manufactoryIDCityID = '';
    var manufactoryIDCityIDCount = 0;
    var mcc = 0;
    var address = '';
    var adressCount = 0;
    var postmark = '';
    var postmarkCount = 0;

    var inputs = document.getElementsByClassName('dataSet')
    for (var i = 0; i < inputs.length; i++) {
        if (publisherCount == 3) {
            createNewInput('PublisherList', publisher)
            publisherCount = 0
            publisher = ''
        } else if (manufactoryIDCityIDCount == 2) {
            if (mcc > 2)
                createNewInput('ManufactoryIDCityIDList', manufactoryIDCityID)
            manufactoryIDCityIDCount = 0
            manufactoryIDCityID = ''
        }

        let currentClass = inputs[i].className
        const findTerm = (term) => {
            if (currentClass.includes(term)) {
                return currentClass;
            }
        };
        switch (currentClass) {
            case findTerm('PublisherAnalysis'):
                if (inputs[i].value != undefined)                
                    publisher += inputs[i].value + '§§'
                publisherCount++
                break
            case findTerm('PublisherCreateEdit'):
                if (inputs[i].value != undefined)                
                    manufactoryIDCityID += inputs[i].value + '§§'
                manufactoryIDCityIDCount++
                mcc++
                break
            case findTerm('Address'):
                if (inputs[i].value != undefined)
                    address += inputs[i].value + '§§'
                adressCount++
                break
        }
    }

    if (publisherCount == 3) {
        createNewInput('PublisherList', publisher)
        publisherCount = 0
        publisher = ''
    } else if (manufactoryIDCityIDCount == 2) {
        createNewInput('ManufactoryIDCityIDList', manufactoryIDCityID)
        manufactoryIDCityIDCount = 0
        manufactoryIDCityID = ''
    } else if (adressCount == 5) {
        createNewInput('AddressList', address)
        adressCount = 0
        address = ''
    }

    document.querySelectorAll('.InputPostcardCityID').forEach(TrIntoInput)

    var value = document.getElementById('city_ID').innerHTML
    $('.InputReceiverCityID').val(value)
}

function TrIntoInput(item, idx, arr) {
    createNewInput('CityIDList', item.innerHTML)
}

function createNewInput(name, value) {
    $('<input>').attr({
        type: 'hidden',
        name: name,
        value: value
    }).appendTo('form')
}

function AddCityToManufactory(buttonId) {
    RemoveRowFromTable()
    AddCityMultiRow(buttonId);
}
function AddCityToPostcard(buttonId) {
    AddCityMultiRow(buttonId);
}
function AddCityMultiRow(buttonId) {
    var lastId = $('#cityTable tr:last').attr('id');
    var lastNumber = 0;
    if (lastId != undefined) {
        lastNumber = parseInt(lastId.split('_').pop()) + 1
    }

    var innerHtml = '<tr id="trCity_' + lastNumber + '">';
    var value = document.getElementById('citySearchResultcityID_' + buttonId).innerHTML;
    innerHtml += '<td scope="col" class="InputPostcardCityID">' + value + '</td>';
    value = document.getElementById('citySearchResultOecoynm_' + buttonId).innerHTML;
    innerHtml += '<td scope="col">' + value + '</td>';
    value = document.getElementById('citySearchResultPostalcode_' + buttonId).innerHTML;
    innerHtml += '<td scope="col">' + value + '</td>';
    value = document.getElementById('citySearchResultByname_' + buttonId).innerHTML;
    innerHtml += '<td scope="col">' + value + '</td>';
    value = document.getElementById('citySearchResultGeography_' + buttonId).innerHTML;
    innerHtml += '<td scope="col" id="geography">' + value + '</td>';
    innerHtml += '<td scope="col"><button type="button" id="' + lastNumber + '" class="btn btn-outline-danger DeleteCity">Entfernen</button></td>';
    innerHtml += '</tr>';

    $('#cityTable').find('tbody').append(innerHtml);
    $('#IsCityExistingModal').modal('hide');
}
function RemoveRowFromTable() {
    let trCity0 = document.getElementById("trCity_0");
    if (trCity0 != null) {
        trCity0.remove()
    }
}

function AddCityToReceiver(buttonId) {
    //Copy results from modal to screen
    SetValueIntoTable(buttonId);
}
$(document).on('click', '.DeleteCity', function () {
    let id = $(this).prop('id')
    document.querySelector('#trCity_' + id)?.remove()
})

function AddParentCity(buttonId) {
    //Copy results from modal to screen
    SetValueIntoTable(buttonId);
}
function SetValueIntoTable(buttonId) {
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

$('body').on('click', '.AddMoreManufactorys', AddMoreManufactorys);
function AddMoreManufactorys() {
    let source = $('.FirstManufactory'), clone = source.clone(true)
    let numberOfInputs = document.getElementsByClassName('InputManufactory').length + 1

    clone.attr('class', 'col').attr('id', 'DivManufactory_' + numberOfInputs).show()
    clone.find('.InputManufactory').attr('id','InputManufactory_' + numberOfInputs).val('')
    clone.find('.InputManufactoryID')
        .attr('id', 'InputManufactoryID_' + numberOfInputs)
        .attr('class', 'form-control dataSet storeInput InputManufactoryID PublisherCreateEdit').val('')
    clone.find('.TownOfManufactorySelect')
        .attr('id', 'TownOfManufactorySelect_' + numberOfInputs)
        .attr('class', 'form-select dataSet storeInput TownOfManufactorySelect PublisherCreateEdit').val('')
    clone.find('.DeleteManufactory').attr('id', numberOfInputs)

    clone.appendTo('.AppendManufactoryHere');
}

$('.DeleteManufactory').on('click', function() {
    let id = $(this).prop('id')
    $('div#DivManufactory_' + id).remove()
})

if (window.location.href.indexOf('CreatePostcard') > -1 || window.location.href.indexOf('EditPostcard') > -1) {
    $('body').on("change", '.checkDisablity', changeDisabled)
    function changeDisabled() {
        if (document.getElementById('checkImage').checked) {
            //when grayout of label works
            //document.getElementById('FieldsetImage').disabled = false
            $('#DivImage').show()
        } else {
            $('#DivImage').hide()
        }
        if (document.getElementById('checkSender').checked) {
            $('#DivSender').show()
        } else {
            $('#DivSender').hide()
        }
        if (document.getElementById('checkReceiver').checked) {
            $('#DivReceiver').show()
        } else {
            $('#DivReceiver').hide()
        }
    }
}

function ActivateDeleteButton() {
    $('.deletePersonalData').attr('disabled', false)
}

$('.addPostalcodeCreateCity').on('click', AddPostalcodeCreateCity);
function AddPostalcodeCreateCity() {
    let source = $('.postalcodeOriginalCreateCity'), clone = source.clone(true)
    let lastElement = $('.InputPostalcodeNumber').last()
    let lastId = lastElement.attr('id')
    let splittedId = lastId.split('_').pop()
    let currentId = parseInt(splittedId) +1

    clone.attr('class', 'input-group pb-1 DivPostalcode').attr('id', 'DivPostalcode_' + currentId)
    clone.find('.InputPostalcodeNumber').attr('id', 'InputPostalcodeNumber_' + currentId).val('')
    clone.find('.addPostalcodeCreateCity').remove()
    clone.find('.removePostalcodeCreateCity').attr('id', currentId).show()

    clone.appendTo('.appendPostalcodeCreateCity');
}
$('.removePostalcodeCreateCity').on('click', function () {
    let id = $(this).prop('id')
    $('div#DivPostalcode_' + id).remove()
})

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

//function addDynamicElement(sourceClass, targetClass, idPrefix) {
//    $(sourceClass).on('click', function () {
//        let source = $(this).closest('.input-group').find(sourceClass.replace('.', ''))
//        let clone = source.clone(true)
//        let lastElement = $(`.${targetClass}`).last()
//        let lastId = lastElement.attr('id')
//        let splittedId = lastId.split('_').pop()
//        let currentId = parseInt(splittedId) + 1

//        clone.attr('class', sourceClass.replace('.', '') + ' pb-1 ' + targetClass).attr('id', targetClass.replace('.', '') + '_' + currentId)
//        clone.find(sourceClass).attr('id', sourceClass.replace('.', '') + '_' + currentId).val('')
//        clone.find('.add' + sourceClass.replace('.', '')).remove()
//        clone.find('.remove' + sourceClass.replace('.', '')).attr('id', currentId).show()

//        clone.appendTo('.' + targetClass);
//    });
//}

//function removeDynamicElement(className, targetClass) {
//    $(document).on('click', className, function () {
//        let id = $(this).prop('id')
//        $('div#' + targetClass.replace('.', '') + '_' + id).remove()
//    });
//}

//addDynamicElement('.addPostalcodeCreateCity', '.appendPostalcodeCreateCity', 'DivPostalcode');
//removeDynamicElement('.removePostalcodeCreateCity', '.appendPostalcodeCreateCity');
//addDynamicElement('.addCityCreateManufactory', '.appendCityCreateManufactory', 'DivCity');
//removeDynamicElement('.removeCityCreateManufactory', '.appendCityCreateManufactory');

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
            createNewInput('PostalcodeNumberList',this.value.trim())
        }
    });
    $(".InputOeconymName").each(function () {
        if (this.value != '') {
            var oeconymName = this.value.trim()
            var currentName = $('#InputCurrentName_' + count).is(":checked")
            oeconym = oeconymName +"§§" + currentName
        }
        count++
        createNewInput('OeconymList', oeconym)
    });
})
$(".createCitySubmitButton").on('click', function () {
    var value = document.getElementsByClassName('city_ID').innerHTML
    $('#cityInput').val(value)
})