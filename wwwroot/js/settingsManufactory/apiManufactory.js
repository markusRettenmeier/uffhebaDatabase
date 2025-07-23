$(function () {
    $(document.body).on('change', 'input.InputProductionFacility', function (event) {
        var currentId = $(this).prop('id').split('_').pop()
        callAjaxProductionFacilityID(currentId)
    })
})
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