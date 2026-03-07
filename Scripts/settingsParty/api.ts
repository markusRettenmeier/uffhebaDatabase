import { Industry } from "../types";

async function getIndustryList(): Promise<string> {
    try {
        const response = await fetch("/api/collections/listIndustries", {
            method: "GET",
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const json: Industry[] = await response.json();
        const jsonString = JSON.stringify(json);
      sessionStorage.setItem('industryList', jsonString);
        return jsonString; // damit handlePageLoad sofort etwas zurückbekommt

    } catch (error: unknown) {
        console.error('Error fetching production facilities:', error);
        sessionStorage.removeItem('industryList');

        if (error instanceof Error) {
            throw new Error(`Failed to load production facilities: ${error.message}`);
        }
        throw new Error('Failed to load production facilities');
    }
}
if (typeof window !== 'undefined') {
    window.getIndustryList = getIndustryList;
}