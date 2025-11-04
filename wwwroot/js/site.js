// Pleaset see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.addEventListener("DOMContentLoaded", () => {
    handlePageLoad();
});
async function handlePageLoad() {
    const url = window.location.href;
    if (url.includes("/Login")) handleShowPassowrd();
    if (url.includes("OrganizationDatabase/Create") || url.includes("OrganizationDatabase/Edit")) {
        let stored = sessionStorage.getItem('productionFacilityList');

        if (!stored) {
            stored = await getProductionFacilityList(); // warte auf fetch
        }

        setProductionFacilitesIntoOptions(stored);
    }
    if (url.includes("Database") || url.includes("Settings")) {
        let stored = sessionStorage.getItem('collectionAreasList');
        if (!stored) {
            stored = await getCollectionAreaList(); // warte auf fetch
        }
        setCollectionAreasIntoOptions(stored);
    }
    if (url.includes("CollectionItemDatabase")) {
        getColorList();
        getMaterialList();
    }
    if (url.includes("ConceptualRelationshipDatabase/Index")) {
        getAndSetConceptualRelationshipGraph();
    }
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

function hideModal(modalName) {
    const modal = document.getElementById(modalName);
    if (modal?.classList.contains('modal')) {
        const modalInstance = bootstrap.Modal.getInstance(modal) || new bootstrap.Modal(modal);
        modalInstance.hide();
    }
}

function setCollectionAreasIntoOptions(stored) {
    const select = document.getElementById("appendCollectionAreasHere");
    if (!select) return;
    if (!stored) return;

    const list = JSON.parse(stored);
    if (!list || list.length === 0) return;

    list.forEach(item => {
        const li = document.createElement('li');
        li.className = 'nav-item';

        const a = document.createElement('a');
        a.className = 'nav-link';
        a.href = `/CollectionItemDatabase/Index?collectionAreaID=${item.id}`;
        a.textContent = item.name;

        select.appendChild(li);
        li.appendChild(a);
    });
}