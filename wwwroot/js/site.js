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

var canvas = document.getElementById('myCanvas'),
    ctx = canvas.getContext('2d'),
    hasInput = false,
    rect = canvas.getBoundingClientRect();

createLinesInCanvas();

function createLinesInCanvas() {
    // Längs gestrichelte Linie
    ctx.beginPath();
    ctx.setLineDash([1, 2]);
    ctx.moveTo(0, 175);
    ctx.lineTo(700, 175);
    ctx.stroke();
    // Quer gestrichelte Linie
    ctx.beginPath();
    ctx.setLineDash([1, 2]);
    ctx.moveTo(350, 0);
    ctx.lineTo(350, 350);
    ctx.stroke();
}

function clearCanvas() {
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    removeDivsOnCanvas()
    let inputsWithTextPosition = document.getElementsByClassName('InputWithTextPosition')
    for (let i = 0; i < inputsWithTextPosition.length; i++) {
        inputsWithTextPosition[i].remove()
    }
    if (hasInput) {
        document.getElementById('canvasInput').remove()
        hasInput = false
    }

    createLinesInCanvas();
}

//window.onscroll = function (e) {
//    let divOfCanvas = document.getElementById('divOfCanvas')
//    let pOfCanvas = document.getElementById('pOfCanvas')
//    ////if (e.pageY === 0) {
//    ////    divOfCanvas.style.display = 'block'; // Show canvas
//    ////    pOfCanvas.style.display = 'none'
//    ////} else {
//    ////    divOfCanvas.style.display = 'none'; // Hide canvas
//    ////    pOfCanvas.style.display = 'block'
//    ////}
//    if (e.pageY === 0) {
//        canvas.poi
//        $(".divOfCanvas").css("pointer-events", "none");
//        pOfCanvas.style.display = 'none'
//    } else {
//        $(".divOfCanvas").css("pointer-events", "all");
//        pOfCanvas.style.display = 'block'
//    }
//}

canvas.onmousedown = function (e) {
    if (window.scrollY > 0) {
        e.preventDefault;
        return false;
    }

    if (hasInput) {
        document.getElementById('canvasInput').remove()
        hasInput = false
    }

    var startX = e.pageX;
    //var startY = e.clientY;
    var startY = e.pageY;

    let resizableDiv = document.createElement("div");
    resizableDiv.className = 'resizableDiv';
    resizableDiv.style.left = startX + 'px';
    resizableDiv.style.top = startY + 'px';
    resizableDiv.style.backgroundColor = 'transparent';
    resizableDiv.style.border = '3px dashed #4286f4';
    resizableDiv.style.position = 'fixed';
    document.body.appendChild(resizableDiv);

    // Create resizers and append to div
    const resizerPositions = ['top-left', 'top-right', 'bottom-left', 'bottom-right'];
    resizerPositions.forEach(pos => {
        let resizer = document.createElement('div');
        resizer.className = `resizer ${pos}`;
        if (pos == 'top-left' || pos == 'bottom-right') {
            resizer.style.cursor = 'nwse-resize'
        }
        else {
            resizer.style.cursor = 'nesw-resize'
        }
        resizableDiv.appendChild(resizer);
    });

    const minimum_size = 0;
    let original_width = 0;
    let original_height = 0;
    let divWidth = 0
    let divHeight = 0
    canvas.addEventListener('mousemove', resize)
    canvas.onmouseup = function () {
        canvas.removeEventListener('mousemove', resize)

        if (hasInput) return;
        addInput();
    }

    function resize(e) {
        //    if (currentResizer.classList.contains('bottom-right')) {
        divWidth = original_width + (e.pageX - startX);
        divHeight = original_height + (e.pageY - startY)
        if (divWidth > minimum_size) {
            resizableDiv.style.width = divWidth + 'px'
        }
        if (divHeight > minimum_size) {
            resizableDiv.style.height = divHeight + 'px'
        }
        //    }
        //    else if (currentResizer.classList.contains('bottom-left')) {
        //        const height = original_height + (e.clientY - startY)
        //        const width = original_width - (e.clientX - startX)
        //        if (height > minimum_size) {
        //            resizableDiv.style.height = height + 'px'
        //        }
        //        if (width > minimum_size) {
        //            resizableDiv.style.width = width + 'px'
        //            resizableDiv.style.left = original_x + (e.clientX - startX) + 'px'
        //        }
        //    }
        //    else if (currentResizer.classList.contains('top-right')) {
        //        const width = original_width + (e.clientX - startX)
        //        const height = original_height - (e.clientY - startY)
        //        if (width > minimum_size) {
        //            resizableDiv.style.width = width + 'px'
        //        }
        //        if (height > minimum_size) {
        //            resizableDiv.style.height = height + 'px'
        //            resizableDiv.style.top = original_y + (e.clientY - startY) + 'px'
        //        }
        //    }
        //    //else {
        //    //    const width = original_width - (e.clientX - startX)
        //    //    const height = original_height - (e.clientY - startY)
        //    //    if (width > minimum_size) {
        //    //        resizableDiv.style.width = width + 'px'
        //    //        resizableDiv.style.left = original_x + (e.clientX - startX) + 'px'
        //    //    }
        //    //    if (height > minimum_size) {
        //    //        resizableDiv.style.height = height + 'px'
        //    //        resizableDiv.style.top = original_y + (e.clientY - startY) + 'px'
        //    //    }
        //    //}
    }

        //function stopResize() {
        //    window.removeEventListener('mousemove', resize)

        //    if (hasInput) return;
        //    addInput();
        //}
    //}
    function addInput() {
        var input = document.createElement('input');

        input.type = 'text';
        input.id = 'canvasInput'
        input.style.position = 'fixed';
        input.style.left = startX + 'px';
        input.style.top = startY + 'px'

        input.onkeydown = handleEnter;

        document.body.appendChild(input);
        removeDivsOnCanvas();

        input.focus();

        hasInput = true;
    }

    //Key handler for input box:
    function handleEnter(e) {
        var keyCode = e.keyCode;
        if (keyCode === 13) {
            drawText(this.value, parseInt(this.style.left, 10), parseInt(this.style.top, 10));
            addInputWithTextAndPositions(this.value)
            document.body.removeChild(this);
            hasInput = false;
        }
    }

    //Draw the text onto canvas:
    function drawText(txt, startX, startY) {
        ctx.textBaseline = 'top';
        ctx.textAlign = 'left';
        ctx.font = divHeight + 'px sans-serif';
        ctx.fillText(txt, startX - rect.x, startY - rect.y);
    }

    function addInputWithTextAndPositions(txt) {
        let input = document.createElement('input')
        let xPosition = Math.round(startX - rect.x)
        let yPosition = Math.round(startY - rect.y)

        input.type = 'hidden'
        input.className = 'InputWithTextPosition'
        input.name = 'TextPositionString'
        input.value = txt + '§§' + divHeight + '§§' + xPosition + '§§' + yPosition

        let div = document.getElementById('AppendTextPosition')
        div.appendChild(input);
    }
}
function removeDivsOnCanvas() {
    let divsOnCanvas = document.getElementsByClassName('resizableDiv');
    while (divsOnCanvas.length > 0) {
        divsOnCanvas[0].parentNode.removeChild(divsOnCanvas[0]);
    }
}

function makeResizableDiv(div) {
    const element = document.querySelector(div);
    const resizers = document.querySelectorAll(div + ' .resizer')
    const minimum_size = 20;
    let original_width = 0;
    let original_height = 0;
    let original_x = 0;
    let original_y = 0;
    let original_mouse_x = 0;
    let original_mouse_y = 0;
    for (let i = 0; i < resizers.length; i++) {
        const currentResizer = resizers[i];
        currentResizer.addEventListener('mousedown', function (e) {
            e.preventDefault()
            original_width = parseFloat(getComputedStyle(element, null).getPropertyValue('width').replace('px', ''));
            original_height = parseFloat(getComputedStyle(element, null).getPropertyValue('height').replace('px', ''));
            original_x = element.getBoundingClientRect().left;
            original_y = element.getBoundingClientRect().top;
            original_mouse_x = e.pageX;
            original_mouse_y = e.pageY;
            window.addEventListener('mousemove', resize)
            window.addEventListener('mouseup', stopResize)
        })

        function resize(e) {
            if (currentResizer.classList.contains('bottom-right')) {
                const width = original_width + (e.pageX - original_mouse_x);
                const height = original_height + (e.pageY - original_mouse_y)
                if (width > minimum_size) {
                    element.style.width = width + 'px'
                }
                if (height > minimum_size) {
                    element.style.height = height + 'px'
                }
            }
            else if (currentResizer.classList.contains('bottom-left')) {
                const height = original_height + (e.pageY - original_mouse_y)
                const width = original_width - (e.pageX - original_mouse_x)
                if (height > minimum_size) {
                    element.style.height = height + 'px'
                }
                if (width > minimum_size) {
                    element.style.width = width + 'px'
                    element.style.left = original_x + (e.pageX - original_mouse_x) + 'px'
                }
            }
            else if (currentResizer.classList.contains('top-right')) {
                const width = original_width + (e.pageX - original_mouse_x)
                const height = original_height - (e.pageY - original_mouse_y)
                if (width > minimum_size) {
                    element.style.width = width + 'px'
                }
                if (height > minimum_size) {
                    element.style.height = height + 'px'
                    element.style.top = original_y + (e.pageY - original_mouse_y) + 'px'
                }
            }
            else {
                const width = original_width - (e.pageX - original_mouse_x)
                const height = original_height - (e.pageY - original_mouse_y)
                if (width > minimum_size) {
                    element.style.width = width + 'px'
                    element.style.left = original_x + (e.pageX - original_mouse_x) + 'px'
                }
                if (height > minimum_size) {
                    element.style.height = height + 'px'
                    element.style.top = original_y + (e.pageY - original_mouse_y) + 'px'
                }
            }
        }

        function stopResize() {
            window.removeEventListener('mousemove', resize)
        }
    }
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
    RemoveRowFromTable("trCity_0")
    AddCityMultiRow(buttonId);
}

function AddBrickname(buttonId) {
    var value = document.getElementById('bricknameSearchResultBrickPotentialID_' + buttonId).innerHTML;
    document.getElementById('bricknameBrickPotentialID').innerHTML = value;
    value = document.getElementById('bricknameSearchResultName_' + buttonId).innerHTML;
    document.getElementById('bricknameName').innerHTML = value;
    value = document.getElementById('bricknameSearchResultUsage_' + buttonId).innerHTML;
    document.getElementById('bricknameUsageEnum').innerHTML = value;

    $('#clearOneRowBricknameTable').show()
    $('#IsBricknameExistingModal').modal('hide')
}
$('#clearOneRowBricknameTable').on('click', function () {
    document.getElementById('bricknameBrickPotentialID').innerHTML = ''
    document.getElementById('bricknameName').innerHTML = ''
    document.getElementById('bricknameUsageEnum').innerHTML = ''
    $('#clearOneRowBricknameTable').hide()
})

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
function RemoveRowFromTable(row) {
    let tr = document.getElementById(row);
    if (tr != null) {
        tr.remove()
    }
}

$(document).on('click', '.DeleteCity', function () {
    let id = $(this).prop('id')
    document.querySelector('#trCity_' + id)?.remove()
})
function AddCityToReceiver(buttonId) {
    //Copy results from modal to screen
    SetCityIntoTable(buttonId);
}
function AddParentCity(buttonId) {
    //Copy results from modal to screen
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

function SetBricknameIntoTable(buttonId) {
    var value = document.getElementById('bricknameSearchResultBricknameID_' + buttonId).innerHTML;
    document.getElementById('brickname_ID').innerHTML = value;
    value = document.getElementById('bricknameSearchResultBrickname_' + buttonId).innerHTML;
    document.getElementById('brickname').innerHTML = value;
    value = document.getElementById('bricknameSearchResultUsage_' + buttonId).innerHTML;
    document.getElementById('usage').innerHTML = value;

    $('#clearOneRowBricknameTable').show()
    $('#IsBricknameExistingModal').modal('hide')
}
$('#clearOneRowBricknameTable').on('click', function () {
    document.getElementById('brickname_ID').innerHTML = ''
    document.getElementById('brickname').innerHTML = ''
    document.getElementById('usage').innerHTML = ''
    $('#clearOneRowBricknameTable').hide()
})

function SetManufacturerIntoTable(buttonId) {
    var value = document.getElementById('manufacturerSearchResultmanufacturerID_' + buttonId).innerHTML;
    document.getElementById('Manufacturer_ID').innerHTML = value;
    value = document.getElementById('manufacturerSearchResultName_' + buttonId).innerHTML;
    document.getElementById('ManufacturerName').innerHTML = value;
    value = document.getElementById('manufacturerSearchResultPersonSignature_' + buttonId).innerHTML;
    document.getElementById('ManufacturerSignature').innerHTML = value;

    $('#clearOneRowManufacturerTable').show()
    $('#IsBrickmakerExistingModal').modal('hide')
}
$('#clearOneRowManufacturerTable').on('click', function () {
    document.getElementById('Manufacturer_ID').innerHTML = ''
    document.getElementById('ManufacturerName').innerHTML = ''
    document.getElementById('ManufacturerSignature').innerHTML = ''
    $('#clearOneRowManufacturerTable').hide()
})

function SetManufactoryIntoTable(buttonId) {
    var value = document.getElementById('manufactorySearchResultManufactoryID_' + buttonId).innerHTML;
    document.getElementById('Manufactory_ID').innerHTML = value;
    value = document.getElementById('manufactorySearchResultName_' + buttonId).innerHTML;
    document.getElementById('ManufactoryName').innerHTML = value;
    var select = document.getElementById('manufactorySearchResultCity_' + buttonId);
    value = select.options[select.selectedIndex].value;
    document.getElementById('CityOfManufactory_ID').innerHTML = value;
    value = select.options[select.selectedIndex].text;
    document.getElementById('CityOfManufactory').innerHTML = value;
    value = document.getElementById('manufactorySearchResultProductionFacility_' + buttonId).innerHTML;
    document.getElementById('ProductionFacilityName').innerHTML = value;

    $('#clearOneRowManufactoryTable').show()
    $('#IsManufactoryExistingModal').modal('hide')
}
$('#clearOneRowManufactoryTable').on('click', function () {
    document.getElementById('Manufactory_ID').innerHTML = ''
    document.getElementById('ManufactoryName').innerHTML = ''
    document.getElementById('CityOfManufactory_ID').innerHTML = ''
    document.getElementById('CityOfManufactory').innerHTML = ''
    document.getElementById('ProductionFacilityName').innerHTML = ''
    $('#clearOneRowManufactoryTable').hide()
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

if (window.location.href.indexOf('Database') > -1) {
    $(function () {
        $(".chosen-select").chosen()
        $(".chosen").chosen()
    });
}