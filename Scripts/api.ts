import { i18n } from './TranslationService';
import type { PlaceResult, ParticipantResult, ConceptResult, EraElement, ConceptualRelationshipResponse } from './types';
import { sendErrorMessage, createTd, createActionCell } from "./helperFunctions";


export async function getAndSetConceptualRelationshipGraph(): Promise<void> {
  const rootConceptInput = document.getElementById('RootConceptID') as HTMLInputElement;
  const rootconceptID = rootConceptInput.value;

  try {
    const response = await fetch('/api/collections/conceptualRelationship?RootConceptID=' + encodeURIComponent(rootconceptID));

    if (!response.ok) {
      throw new Error(response.statusText);
    }

    const result: ConceptualRelationshipResponse = await response.json();

    const nodes = new vis.DataSet(result.nodes);
    const edges = new vis.DataSet(result.edges);

    const container = document.getElementById("network") as HTMLElement;

    new vis.Network(container, { nodes, edges }, {
      edges: {
        arrows: "to",
        font: { align: "middle" },
        length: 200
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

  } catch (err: unknown) {
    if (err instanceof Error) {
      sendErrorMessage(err);
    } else {
      sendErrorMessage(new Error('Unknown error occurred'));
    }
  }
}
export async function getCollectionAreaList(): Promise<string | null> {
  try {
    const response = await fetch("/api/collections/listCollectionAreas");
    if (!response.ok) throw new Error(`HTTP ${response.status}`);

    const json = await response.json();
    const jsonString = JSON.stringify(json);
    sessionStorage.setItem('collectionAreasList', jsonString);
    return jsonString;
  } catch (error) {
    console.error("Error fetching collection areas:", error);
    return null;
  }
}

export async function getIndustryList(): Promise<string | null> {
  try {
    const response = await fetch("/api/collections/listIndustries");
    if (!response.ok) throw new Error(`HTTP ${response.status}`);

    const json = await response.json();
    const jsonString = JSON.stringify(json);
    sessionStorage.setItem('industryList', jsonString);
    return jsonString;
  } catch (error) {
    console.error("Error fetching industries:", error);
    return null;
  }
}

export async function getCIRelationshipList(): Promise<string | null> {
  try {
    const response = await fetch("/api/collections/listCIRelationships");
    if (!response.ok) throw new Error(`HTTP ${response.status}`);

    const json = await response.json();
    const jsonString = JSON.stringify(json);
    sessionStorage.setItem('ciRelationshipList', jsonString);
    return jsonString;
  } catch (error) {
    console.error("Error fetching ciRelationships:", error);
    return null;
  }
}

export function initializePlaceSearch(): void {
  const togglePlace = document.querySelector<HTMLButtonElement>('.placeSearchSubmit');
  if (!togglePlace) return;

  togglePlace.addEventListener('click', () => {
    const inputElement = document.querySelector<HTMLInputElement>('.inputPlaceSearch');
    if (!inputElement) return;

    const toponymyName = inputElement.value.trim();

    fetch('/api/collections/listPlaces', {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ toponym: toponymyName }),
    })
      .then((res: Response): Promise<PlaceResult[]> => {
        if (!res.ok) throw new Error(res.statusText);
        return res.json();
      })
      .then((result: PlaceResult[]): void => {
        const table = document.getElementById('placeSearchResultTable') as HTMLTableElement;
        if (!table) return;

        const oldTbody = document.getElementById('placeSearchResultTableBody');
        oldTbody?.remove();

        const tbody = table.createTBody();
        tbody.id = 'placeSearchResultTableBody';

        if (result.length > 0) {
          result.forEach((element: PlaceResult, idx: number): void => {
            tbody.appendChild(buildToponymySearchResultRow(element, idx));
          });
        } else {
          const tr = document.createElement('tr');
          tr.appendChild(createTd({
            text: i18n.get("NothingFound"),
            scope: 'row'
          }));
          tbody.appendChild(tr);
        }
      })
      .catch((err: Error): void => {
        sendErrorMessage(err);
      });
  });
}
export function buildToponymySearchResultRow(element: PlaceResult, idx: number): HTMLTableRowElement {
  const tr = document.createElement('tr');
  tr.id = `placeSearchResult_${idx}`;

  tr.appendChild(createTd({
    text: element.placeID.toString(),
    id: `placeSearchResultPlaceID_${idx}`,
    scope: 'row'
  }));

  const tdToponym = document.createElement('td');
  tdToponym.id = `placeSearchResultToponym_${idx}`;
  tdToponym.innerHTML = element.toponymyDisplay || '';
  tr.appendChild(tdToponym);

  const tdFurtherSpecs = document.createElement('td');
  tdFurtherSpecs.id = `placeSearchResultFurtherSpecs_${idx}`;
  tdFurtherSpecs.textContent = element.furtherSpecs || '';
  tr.appendChild(tdFurtherSpecs);

  const tdAction = createActionCell(idx, ['Individual', 'Organization', 'CollectionItem'], 'Place_Add', 'addPlace');
  tr.appendChild(tdAction);

  return tr;
}

export function initializeParticipantSearch(): void {
  const toggleParticipant = document.querySelector<HTMLButtonElement>('.participantSearchSubmit');
  if (!toggleParticipant) return;

  toggleParticipant.addEventListener('click', (): void => {
    const inputElement = document.querySelector<HTMLInputElement>('.inputParticipantSearch');
    const participantTypeElement = document.getElementById('participantType') as HTMLSelectElement;

    if (!inputElement || !participantTypeElement) return;

    const participantName = inputElement.value.trim();
    const participantType = participantTypeElement.value;

    fetch('/api/collections/listParties', {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ name: participantName, type: participantType }),
    })
      .then((res: Response): Promise<ParticipantResult[]> => {
        if (!res.ok) throw new Error(res.statusText);
        return res.json();
      })
      .then((result: ParticipantResult[]): void => {
        const table = document.getElementById('participantSearchResultTable') as HTMLTableElement;
        if (!table) return;

        const oldTbody = document.getElementById('participantSearchResultTableBody');
        oldTbody?.remove();

        const tbody = table.createTBody();
        tbody.id = 'participantSearchResultTableBody';

        if (result.length > 0) {
          result.forEach((element: ParticipantResult, idx: number): void => {
            tbody.appendChild(buildParticipantSearchResultRow(element, idx));
          });
        } else {
          const tr = document.createElement('tr');
          tr.appendChild(createTd({
            text: i18n.get("NothingFound"),
            scope: 'row'
          }));
          tbody.appendChild(tr);
        }
      })
      .catch((err: Error): void => {
        sendErrorMessage(err);
      });
  });
}
export function buildParticipantSearchResultRow(element: ParticipantResult, idx: number): HTMLTableRowElement {
  const tr = document.createElement('tr');
  tr.id = `participantSearchResult_${idx}`;

  tr.appendChild(createTd({
    text: element.participantID.toString(),
    id: `participantSearchResultParticipantID_${idx}`,
    scope: 'row'
  }));

  tr.appendChild(createTd({
    text: element.name,
    id: `participantSearchResultName_${idx}`,
    scope: 'row'
  }));

  tr.appendChild(createTd({
    text: element.type || '',
    id: `participantSearchResultType_${idx}`,
    scope: 'row'
  }));

  const tdFurtherSpecs = document.createElement('td');
  tdFurtherSpecs.id = `participantSearchResultFurtherSpecs_${idx}`;
  tdFurtherSpecs.textContent = element.furtherSpecs || '';
  tr.appendChild(tdFurtherSpecs);

  const tdAction = document.createElement('td');
  const btnAction = document.createElement('button');
  btnAction.type = 'button';
  btnAction.classList.add('btn', 'btn-primary', 'btn-sm');
  btnAction.textContent = i18n.get("Add");
  btnAction.onclick = (): void => window.addParticipant?.(idx);
  tdAction.appendChild(btnAction);

  tr.appendChild(tdAction);
  return tr;
}

export function initializeConceptSearch(): void {
  const conceptToggle = document.querySelector<HTMLButtonElement>(".conceptSearchSubmit");
  if (conceptToggle) {
    conceptToggle.addEventListener('click', () => {
      const inputName = document.getElementById('inputConceptSearch') as HTMLInputElement;
      const name = inputName.value;
      const inputCollectionAreaID = document.getElementById('InputCollectionAreaID') as HTMLInputElement;
      const collectionAreaID = inputCollectionAreaID.value;
      const inputRootConceptID = document.getElementById('InputRootConceptID') as HTMLInputElement;
      const rootConceptID = inputRootConceptID.value;

      if (!inputName || !inputCollectionAreaID || !inputRootConceptID) return;

      fetch('/api/collections/listConcepts?conceptName=' + encodeURIComponent(name) + "&collectionAreaID=" + encodeURIComponent(collectionAreaID)
        + "&rootConceptId=" + encodeURIComponent(rootConceptID))
        .then((res: Response): Promise<ConceptResult[]> => {
          if (!res.ok) throw new Error(res.statusText);
          return res.json();
        })
        .then(result => {
          let myTable = document.getElementById("conceptSearchResultTable") as HTMLTableElement;
          if (!myTable) return;

          const tableBody = document.getElementById('conceptSearchResultTableBody');
          tableBody?.remove();

          let tbody = myTable.createTBody()
          tbody.id = 'conceptSearchResultTableBody';

          if (result.length > 0) {
            result.forEach((element, idx) => {
              tbody.appendChild(buildConceptSearchResultRow(element, idx));
            });
          } else {
            const tr = document.createElement('tr');
            tr.appendChild(createTd({
              text: i18n.get("NothingFound"),
              scope: 'row'
            }));
            tbody.appendChild(tr);
          }
        })
        .catch(err => {
          sendErrorMessage(err);
        });
    });
  }
}
function buildConceptSearchResultRow(element: ConceptResult, idx: number): HTMLTableRowElement {
  const tr = document.createElement('tr');
  tr.id = `conceptSearchResult_${idx}`;

  tr.appendChild(createTd({
    text: element.conceptID.toString(),
    id: `conceptSearchResultConceptID_${idx}`,
    scope: 'row'
  }));

  tr.appendChild(createTd({
    text: element.conceptName,
    id: `conceptSearchResultName_${idx}`,
    scope: 'row'
  }));

  tr.appendChild(createTd({
    text: element.furtherSpecs,
    id: `conceptSearchResultFurtherSpecs_${idx}`,
    scope: 'row'
  }));

  const tdAction = document.createElement('td');

  const divAction = document.createElement('div');
  divAction.className = 'btn-group';

  const url = window.location.href;
  if (url.includes('ConceptualRelationshipDatabase')) {
    const btnSynonym = document.createElement('button');
    btnSynonym.type = 'button';
    btnSynonym.className = 'btn btn-primary';
    btnSynonym.textContent = i18n.get("Concept_SynonymAdd");
    btnSynonym.onclick = (): void => window.addConceptToConcept?.(idx, 'synonym');
    divAction.appendChild(btnSynonym);

    const btnSubTerm = document.createElement('button');
    btnSubTerm.type = 'button';
    btnSubTerm.className = 'btn btn-primary';
    btnSubTerm.textContent = i18n.get("Concept_ParentConceptAdd");
    btnSubTerm.onclick = (): void => window.addConceptToConcept?.(idx, 'subterm');
    divAction.appendChild(btnSubTerm);
  }
  else if (url.includes('CollectionItemDatabase')) {
    const btn = document.createElement('button');
    btn.type = 'button';
    btn.className = 'btn btn-primary';
    btn.textContent = i18n.get("Add");
    btn.onclick = (): void => window.addConceptToCollectionItem?.(idx);
    divAction.appendChild(btn);
  }

  tdAction.appendChild(divAction);
  tr.appendChild(tdAction);

  return tr;
}
export function initializeEraSearch(): void {
  const eraToggle = document.querySelector(".eraSearchSubmit") as HTMLButtonElement;
  if (eraToggle) {
    eraToggle.addEventListener('click', () => {
      const nameInput = document.getElementById('inputEraNameSearch') as HTMLInputElement;
      const name = nameInput.value;

      fetch('/api/collections/listEras?name=' + encodeURIComponent(name))
        .then(res => {
          if (!res.ok) throw new Error(res.statusText);
          return res.json();
        })
        .then((result: EraElement[]) => {
          const tableBody = document.getElementById('eraSearchResultTableBody');
          if (tableBody != null)
            tableBody.remove();

          const myTable = document.getElementById("eraSearchResultTable") as HTMLTableElement;
          const tbody = myTable.createTBody();
          tbody.setAttribute('id', 'eraSearchResultTableBody');

          if (result.length > 0) {
            result.forEach((element: EraElement, idx: number) => {
              tbody.appendChild(buildEraSearchResultRow(element, idx));
            });
          } else {
            const tr = document.createElement('tr');
            const td = document.createElement('td');
            td.textContent = i18n.get("NothingFound");
            tr.appendChild(td);
            tbody.appendChild(tr);
          }
        })
        .catch(err => {
          sendErrorMessage(err);
        });
    });
  }
}
function buildEraSearchResultRow(element: EraElement, idx: number): HTMLTableRowElement {
  const tr = document.createElement('tr');
  tr.id = `eraSearchResult_${idx}`;

  tr.appendChild(createTd({
    text: element.eraID,
    id: `eraSearchResultID_${idx}`,
    scope: 'row'
  }));

  tr.appendChild(createTd({
    text: element.eraName,
    id: `eraSearchResultName_${idx}`,
    scope: 'row'
  }));

  const tdAction = document.createElement('td');
  const btn = document.createElement('button');
  btn.type = 'button';
  btn.className = 'btn btn-primary addEra';
  btn.textContent = i18n.get("Add");
  btn.id = `${idx}`;
  tdAction.appendChild(btn);

  tr.appendChild(tdAction);
  return tr;
}