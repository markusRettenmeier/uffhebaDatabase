//document.addEventListener("DOMContentLoaded", () => {
//    handlePageLoad();
//});

//function handlePageLoad() {
//    const url = window.location.href;

//    if (url.includes("OrganizationDatabase/Create") || url.includes("OrganizationDatabase/Edit")) {
//        let stored = sessionStorage.getItem('productionFacilityList');
//        if (!stored) {
//            stored = await getProductionFacilityList();
//        }
//        setProductionFacilitesIntoOptions(stored);
//    }
//}

//async function getProductionFacilityList() {
//    const url = "/api/collections/listProductionFacilities";

//    try {
//        const response = await fetch(url);
//        if (!response.ok) {
//            throw new Error(`Response status: ${response.status}`);
//        }

//        const json = await response.json();
//        sessionStorage.setItem('productionFacilityList', JSON.stringify(json));
//        console.log('Gespeichert:', sessionStorage.getItem('productionFacilityList'));
//    } catch (error) {
//        console.error("Fehler beim Abrufen der Branchen:", error.message);
//    }
//}

//document.addEventListener("DOMContentLoaded", async () => {
//    handlePageLoad();
//});

async function handlePageLoad() {
    const url = window.location.href;

    if (url.includes("OrganizationDatabase/Create") || url.includes("OrganizationDatabase/Edit")) {
        let stored = sessionStorage.getItem('productionFacilityList');

        if (!stored) {
            stored = await getProductionFacilityList(); // warte auf fetch
        }

        setProductionFacilitesIntoOptions(stored);
    }
}

async function getProductionFacilityList() {
    const response = await fetch("/api/collections/listProductionFacilities", {
        method: "GET"
    });
    const json = await response.json();
    sessionStorage.setItem('productionFacilityList', JSON.stringify(json));
    return JSON.stringify(json); // damit handlePageLoad sofort etwas zurückbekommt
}
