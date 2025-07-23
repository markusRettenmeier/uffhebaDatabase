// Pleaset see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.addEventListener("DOMContentLoaded", () => {
    handlePageLoad();
});
function handlePageLoad() {
    const url = window.location.href;
    if (url.includes("/Login")) handleShowPassowrd();
}

function openNav() {
    if (window.innerWidth <= 600)
        document.getElementById("mySidebar").style.width = "100%"
    else
        document.getElementById("mySidebar").style.width = "600px"
}
function closeNav() {
    document.getElementById("mySidebar").style.width = "0"
}

document.querySelector("body").addEventListener("click", (event) => {
    if (event.target.matches("#generatePDF")) generatePDF();
});
function generatePDF() {
    const element = document.getElementById("detailsDiv");
    const currentId = document.getElementById("currentNumber").innerHTML;

    // eslint-disable-next-line no-undef
    html2pdf()
        .set({
            filename: "Details_Nummer" + currentId + ".pdf",
        })
        .from(element)
        .output("dataurlnewwindow");
}

function handleShowPassowrd() {
    const showPasswordButton = document.getElementById("show_password");
    showPasswordButton.addEventListener("mouseenter", handlerIn);
    showPasswordButton.addEventListener("mouseleave", handlerOut);
    function handlerIn() {
        //Change the attribute to text
        document.getElementById("Password").type = "text";
        document.getElementById("icon").className = "fa-solid fa-eye";
    }
    function handlerOut() {
        //Change the attribute back to password
        document.getElementById("Password").type = "password";
        document.getElementById("icon").className = "fa-solid fa-eye-slash";
    }
}

//Für Register
//var current = location.pathname;
//$('.nav-tabs li a').each(function () {
//    var $this = $(this);
//    if (current.indexOf($this.attr('href')) !== -1)
//        $this.addClass('active');
//})

function AddCityToReceiver(buttonId) {
    SetCityIntoTable(buttonId);
}

function hideModal(modalName) {
    const modal = document.getElementById(modalName);
    if (modal?.classList.contains('modal')) {
        const modalInstance = bootstrap.Modal.getInstance(modal) || new bootstrap.Modal(modal);
        modalInstance.hide();
    }
}

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

if (window.location.href.indexOf('Database') > -1) {
    $(function () {
        $(".chosen-select").chosen()
        $(".chosen").chosen()
    });
}
