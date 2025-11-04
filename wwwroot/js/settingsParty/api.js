async function getProductionFacilityList() {
    const response = await fetch("/api/collections/listProductionFacilities", {
        method: "GET"
    });
    const json = await response.json();
    sessionStorage.setItem('productionFacilityList', JSON.stringify(json));
    return JSON.stringify(json); // damit handlePageLoad sofort etwas zurückbekommt
}
