$(function () {
    $(document.body).on('change', 'input.InputGeographyName', function (event) {
        callAjaxGeographyID()
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