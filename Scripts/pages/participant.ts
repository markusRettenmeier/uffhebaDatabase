import { autocomplete, hideModal } from "../helperFunctions";
import { i18n } from '../TranslationService';
import { getIndustryList } from "../api";

async function handlePageLoad(): Promise<void> {
  const url = window.location.href;
  if (url.includes("OrganizationDatabase/Create") || url.includes("OrganizationDatabase/Edit")) {
    let stored = sessionStorage.getItem('industryList');
    if (!stored) {
      stored = await getIndustryList();
    }
    initializeIndustryAutocomplete();
  }
}

export function initializeIndustryAutocomplete(): void {
  const industryInput = document.getElementById("Industry") as HTMLInputElement;
  let industryArray: string[] = [];
  try {
    const industryJson = sessionStorage.getItem("industryList");
    industryArray = industryJson ? JSON.parse(industryJson) : [];
  } catch {
    industryArray = [];
  }
  autocomplete(industryInput, industryArray);
}


document.addEventListener('click', (event) => {
  const eraToggle = document.getElementById('addEra') as HTMLButtonElement;
  if (!eraToggle) {
    return;
  }

  const buttonId = eraToggle.getAttribute('id');
  if (!buttonId) {
    return;
  }

  const eraIdValue = document.getElementById(`eraSearchResultID_${buttonId}`)?.textContent || '';
  const nameValue = document.getElementById(`eraSearchResultName_${buttonId}`)?.textContent || '';

  const tbody = document.getElementById('appendEraHere') as HTMLTableSectionElement;
  const index = tbody.children.length;

  const tr = document.createElement('tr');
  tr.className = 'eraResultTableTr';

  const tdName = document.createElement("td");
  tdName.textContent = nameValue;

  const tdActions = document.createElement("td");

  const hiddenInput = document.createElement("input");
  hiddenInput.type = "hidden";
  hiddenInput.classList.add('form-control', 'eraResultTableId');
  hiddenInput.name = `ConnectedEraList[${index}].EraId`;
  hiddenInput.value = eraIdValue;

  const removeBtn = document.createElement("button");
  removeBtn.type = "button";
  removeBtn.classList.add("btn", "btn-danger", "btn-sm", "removeEra");
  removeBtn.textContent = i18n.get("Remove");

  tdActions.appendChild(hiddenInput);
  tdActions.appendChild(removeBtn);
  tr.appendChild(tdName);
  tr.appendChild(tdActions);
  tbody.appendChild(tr);

  hideModal('EraModal');
});
(function initRemoveEraButtonHandler(): void {
  const tableBody = document.getElementById('appendEraHere');
  if (!tableBody) return;

  tableBody.addEventListener('click', function (e: Event) {
    const target = e.target as HTMLElement;
    if (target && target.classList.contains('removeEra')) {
      const trTag = target.closest('.eraResultTableTr') as HTMLTableRowElement;
      if (trTag) {
        trTag.remove();
        reindexEraFields(tableBody as HTMLTableSectionElement);
      }
    }
  });
})();
function reindexEraFields(container: HTMLTableSectionElement): void {
  const entries = Array.from(container.querySelectorAll(".eraResultTableTr"));

  entries.forEach((entry: Element, index: number) => {
    const eraIdInput = entry.querySelector(".eraResultTableId") as HTMLInputElement;
    if (eraIdInput) {
      eraIdInput.name = `ConnectedEraList[${index}].EraId`;
    }
  });
}

//if (typeof window !== 'undefined') {
//  window.addEra = addEra;
//}