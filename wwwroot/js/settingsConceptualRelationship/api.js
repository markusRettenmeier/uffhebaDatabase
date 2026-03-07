import { sendErrorMessage } from "../api.js";
import { i18n } from "../TranslationService.js";
export async function getAndSetConceptualRelationshipGraph() {
    const rootConceptInput = document.getElementById('RootConceptID');
    const rootconceptID = rootConceptInput.value;
    try {
        const response = await fetch('/api/collections/conceptualRelationship?RootConceptID=' + encodeURIComponent(rootconceptID));
        if (!response.ok) {
            throw new Error(response.statusText);
        }
        const result = await response.json();
        const nodes = new vis.DataSet(result.nodes);
        const edges = new vis.DataSet(result.edges);
        const container = document.getElementById("network");
        new vis.Network(container, { nodes, edges }, {
            edges: {
                arrows: "to",
                font: { align: "middle" },
                length: 200,
                label: i18n.get("to")
            },
            nodes: {
                shape: "box",
                color: {
                    background: "#e0f2fe",
                    border: "#0284c7"
                },
                font: {
                    color: "#0f172a",
                    face: "Arial"
                }
            },
            physics: { enabled: true }
        });
    }
    catch (err) {
        if (err instanceof Error) {
            sendErrorMessage(err);
        }
        else {
            sendErrorMessage(new Error('Unknown error occurred'));
        }
    }
}
