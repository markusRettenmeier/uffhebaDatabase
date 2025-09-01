document.addEventListener("DOMContentLoaded", () => {
    handlePageLoad();
});

function handlePageLoad() {
    const url = window.location.href;

    //if (url.includes("Database")) {
    //    getEraList();
    //}
    //getEraList();
}
//async function getEraList() {
//    try {
//        const response = await fetch("/api/collections/listEras");
//        if (!response.ok) {
//            throw new Error(`Response status: ${response.status}`);
//        }

//        const json = await response.json();
//        sessionStorage.setItem('eraList', JSON.stringify(json));
//        console.log('Gespeichert:', sessionStorage.getItem('eraList'));
//    } catch (error) {
//        console.error(error.message);
//    }
//}
//async function getTyponymyList() {
//    try {
//        const response = await fetch("/api/collections/listToponymys");
//        if (!response.ok) {
//            throw new Error(`Response status: ${response.status}`);
//        }

//        const json = await response.json();
//        sessionStorage.setItem('toponymyList', JSON.stringify(json));
//        console.log('Gespeichert:', sessionStorage.getItem('toponymyList'));
//    } catch (error) {
//        console.error(error.message);
//    }
//}

//$(function () {
//    $(document.body).on('change', 'input.InputGeographyName', function (event) {
//        callAjaxGeographyID()
//    })
//})
//function callAjaxGeographyID(InputId) {
//    var geography = $('#InputGeographyName_' + InputId).val()
//    if (geography != '') {
//        $.ajax({
//            url: '/api/collections/autocompleteGeographyID',
//            data: { term: geography },
//            success: function (result) {
//                $('#InputGeographyID_' + InputId).val(result)
//            },
//            error: function (xhr, status, error) {
//                sendErrorMessage(xhr)
//            }
//        })
//    }
//}

//$(document.body).on('keydown', 'input.inputEra', function (event) {
//    const eraList = JSON.parse(sessionStorage.getItem('eraList') || '[]');
//    const autocompleteSource = eraList.map(x => ({
//        label: x.eraName,
//        value: x.eraName,
//        id: x.eraID
//    }));

//    $(this).autocomplete({
//        source: autocompleteSource,
//        select: function (event, ui) {
//            const selected = ui.item;

//            const inputGroup = $(this).closest('.input-group');
//            inputGroup.find('.inputEraID').val(selected.id);
//        }
//    });
//})

//$(document.body).on('keydown', 'input.inputToponymy', function (event) {
//    const toponymyList = JSON.parse(sessionStorage.getItem('toponymyList') || '[]');
//    const autocompleteSource = toponymyList.map(x => ({
//        label: x.toponymyName,
//        value: x.toponymyName,
//        id: x.toponymyID
//    }));

//    $(this).autocomplete({
//        source: autocompleteSource,
//        select: function (event, ui) {
//            const selected = ui.item;

//            const inputGroup = $(this).closest('.input-group');
//            inputGroup.find('.inputToponymyID').val(selected.id);
//        }
//    });
//})

//const toggleEra = document.querySelector('.inputEra')
//if (toggleEra) {
//    toggleEra.addEventListener('click', () => {
//        const eraList = JSON.parse(sessionStorage.getItem('eraList') || '[]');
//        const autocompleteSource = eraList.map(x => ({
//            label: x.eraName,
//            value: x.eraName,
//            id: x.eraID
//        }));

//        const inputId = document.getElementById()
//        enableAutocomplete(toggleEra, "", autocompleteSource);
//    })
//}

//const toggleToponymy = document.querySelector('.inputToponymy')
//if (toggleToponymy) {
//    toggleToponymy.addEventListener('click', () => {
//        const toponymyList = JSON.parse(sessionStorage.getItem('toponymyList') || '[]');
//        const autocompleteSource = toponymyList.map(x => ({
//            label: x.toponymyName,
//            value: x.toponymyName,
//            id: x.toponymyID
//        }));

//        enableAutocomplete(toggleToponymy, "", autocompleteSource);
//    })
//}