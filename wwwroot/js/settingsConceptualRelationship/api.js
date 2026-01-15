async function getAndSetConceptualRelationshipGraph()
{
    const rootconceptID = document.getElementById('RootConceptID').value;
    fetch('/api/collections/conceptualRelationship?RootConceptID=' + encodeURIComponent(rootconceptID))
        .then(res => {
            if (!res.ok) throw new Error(res.statusText);
            return res.json();
        })
        .then(result => {
            const nodes = new vis.DataSet(result.nodes);
            const edges = new vis.DataSet(result.edges);

            const container = document.getElementById("network");
            new vis.Network(container, { nodes, edges }, {
                edges: {
                    arrows: i18n.get("to"),
                    font: { align: "middle" },
                    length: 200
                },
                nodes: {
                    shape: "box",
                    color: { background: "#e0f2fe", border: "#0284c7" },
                    font: { color: "#0f172a", face: "Arial" }
                },
                physics: { enabled: true }
            });
        })
        .catch(err => {
            sendErrorMessage(err);
        });
}