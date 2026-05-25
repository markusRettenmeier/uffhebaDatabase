// Event Listener
// wwwroot/js/shared.ts
import { i18n } from './TranslationService';
import { getAndSetConceptualRelationshipGraph, getCollectionAreaList, initializePlaceSearch, initializeParticipantSearch, initializeConceptSearch, initializeEraSearch } from './api';
import { hideModal } from './helperFunctions';

// Globale Variablen
let isI18nReady = false;

// Zentrale Initialisierung
async function initializeShared(): Promise<void> {
  if (!isI18nReady) {
    await i18n.loadTranslations();
    isI18nReady = true;
  }
}

// WebAuthn Support prüfen (zentral)
export async function checkWebAuthnSupport(): Promise<boolean> {
  await initializeShared();

  const isSupported = typeof window.PublicKeyCredential !== 'undefined' &&
    typeof navigator.credentials !== 'undefined' &&
    typeof navigator.credentials.get === 'function';

  const statusDiv = document.getElementById('webauthnStatus');
  if (statusDiv) {
    if (isSupported) {
      statusDiv.innerHTML = `<div class="alert alert-success"><i class="bi bi-check-circle me-2"></i>${i18n.get('WebAuthn_Supported')}</div>`;
    } else {
      statusDiv.innerHTML = `<div class="alert alert-warning"><i class="bi bi-exclamation-triangle me-2"></i>${i18n.get('WebAuthn_Not_Supported')}</div>`;
      const btnLogin = document.getElementById('btnLogin') as HTMLButtonElement;
      const btnSearch = document.getElementById('btnSearchPasskeys') as HTMLButtonElement;
      const btnRegister = document.getElementById('btnRegister') as HTMLButtonElement;
      if (btnLogin) btnLogin.disabled = true;
      if (btnSearch) btnSearch.disabled = true;
      if (btnRegister) btnRegister.disabled = true;
      return false;
    }
  }
  return isSupported;
}

// Zentrale Status-Funktionen
export function showStatus(containerId: string, html: string): void {
  const statusDiv = document.getElementById(containerId);
  if (statusDiv) statusDiv.innerHTML = html;
}

export function showError(containerId: string, message: string): void {
  showStatus(containerId, `
        <div class="alert alert-danger alert-dismissible fade show" role="alert">
            <i class="bi bi-exclamation-triangle me-2"></i>
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="${i18n.get('Close')}"></button>
        </div>
    `);
}

export function showLoading(containerId: string, message: string): void {
  showStatus(containerId, `
        <div class="alert alert-info">
            <div class="d-flex align-items-center">
                <div class="spinner-border spinner-border-sm me-2" role="status">
                    <span class="visually-hidden">${i18n.get('Loading')}</span>
                </div>
                <span>${message}</span>
            </div>
        </div>
    `);
}

export function showSuccess(containerId: string, message: string): void {
  showStatus(containerId, `
        <div class="alert alert-success">
            <i class="bi bi-check-circle me-2"></i>
            ${message}
        </div>
    `);
}

// Helper Funktionen
export function getTranslation(key: string, ...args: any[]): string {
  let text = i18n.get(key);
  if (args.length > 0) {
    text = text.replace(/{(\d+)}/g, (_, i) => args[i] || '');
  }
  return text;
}

// Page Load Handler
document.addEventListener("DOMContentLoaded", async (): Promise<void> => {
  await initializeShared();

  // Suchfunktionen (falls vorhanden)
  if (typeof initializePlaceSearch === 'function') initializePlaceSearch();
  if (typeof initializeParticipantSearch === 'function') initializeParticipantSearch();
  if (typeof initializeConceptSearch === 'function') initializeConceptSearch();
  if (typeof initializeEraSearch === 'function') initializeEraSearch();

  const url = window.location.href;

  // Weitere Seiten
  if (url.includes("/ConceptualRelationshipDatabase/IndexSpecific")) {
    if (typeof getAndSetConceptualRelationshipGraph === 'function') getAndSetConceptualRelationshipGraph();
  }

  if (url.includes("Database") || url.includes("Settings")) {
    let stored = sessionStorage.getItem('collectionAreasList');
    if (!stored) {
      stored = await getCollectionAreaList();
    }
    if (stored) {
      setCollectionAreasIntoOptions(stored);
    }
  }
});

// Globale Exporte für window
(window as any).checkWebAuthnSupport = checkWebAuthnSupport;
function setCollectionAreasIntoOptions(stored: string): void {
  const select = document.getElementById("appendCollectionAreasHere");
  if (!select) return;

  let list: { id: number; name: string }[];
  try {
    list = JSON.parse(stored);
  } catch (error) {
    console.error("Failed to parse collection areas:", error);
    return;
  }

  if (!Array.isArray(list) || list.length === 0) return;

  list.forEach(item => {
    const li = document.createElement('li');
    li.className = 'nav-item';

    const a = document.createElement('a');
    a.className = 'nav-link text-white';
    a.href = `/CollectionItemDatabase/Index?collectionAreaID=${item.id}`;
    a.textContent = item.name;

    select.appendChild(li);
    li.appendChild(a);
  });
}

function addPlace(idx: number): void {
  const tbody = document.getElementById("appendPlaceHere") as HTMLTableSectionElement;
  if (!tbody) {
    console.error('appendPlaceHere element not found');
    return;
  }
  const index = tbody.children.length;

  // Get data from search result elements
  const toponymElement = document.getElementById(`placeSearchResultToponym_${idx}`);
  const furtherSpecsElement = document.getElementById(`placeSearchResultFurtherSpecs_${idx}`);
  const placeIDElement = document.getElementById(`placeSearchResultPlaceID_${idx}`);

  if (!toponymElement || !furtherSpecsElement || !placeIDElement) {
    console.error(`Required place search elements not found for index ${idx}`);
    return;
  }

  const toponym = toponymElement.innerHTML || '';
  const furtherSpecs = furtherSpecsElement.textContent || '';
  const placeID = placeIDElement.textContent || '';

  const row = document.createElement("tr");
  row.className = `placeTableTr`;
  // Mark row so reindex can find it
  row.setAttribute('data-type', 'child');

  const tdName = document.createElement("td");
  tdName.innerHTML = toponym;

  // Further specifications cell
  const tdSpecs = document.createElement("td");
  tdSpecs.textContent = furtherSpecs;
  const url = window.location.href;

  // Actions cell
  const tdActions = document.createElement("td");

  // Hidden input for PlaceID
  const hiddenInput = document.createElement("input");
  hiddenInput.type = "hidden";
  hiddenInput.className = "placeId";
  hiddenInput.name = `ConnectedPlaceList[${index}].Id`;
  hiddenInput.value = placeID;

  // Remove button
  const removeBtn = document.createElement("button");
  removeBtn.type = "button";
  removeBtn.classList.add("btn", "btn-danger", "btn-sm", "removePlace");
  removeBtn.textContent = i18n.get("Remove");

  // Assemble row
  tdActions.appendChild(hiddenInput);
  tdActions.appendChild(removeBtn);

  row.appendChild(tdName);
  row.appendChild(tdSpecs);
  //if (!url.includes("/Place")) {
  if (url.includes("/CollectionItem")) {
    const tdRelationType = document.createElement("td");

    // Deutsche Validierung manuell setzen
    const relationshipInput = document.createElement("input");
    relationshipInput.id = `ConnectedPlaceList_${index}__Relationship`
    relationshipInput.type = "text";
    relationshipInput.classList.add("form-control", "placeRelationship");
    relationshipInput.name = `ConnectedPlaceList[${index}].Relationship`;
    relationshipInput.setAttribute("data-val", "true");
    relationshipInput.setAttribute("data-val-required", i18n.get("Error_Relationship_Required"));
    relationshipInput.setAttribute("aria-required", "true");
    relationshipInput.setAttribute("aria-invalid", "false");
    relationshipInput.setAttribute("aria-describedby", `ConnectedPlaceList_${index}__Relationship-error`);

    tdRelationType.appendChild(relationshipInput);

    const relationshipValidationSpan = document.createElement('span');
    relationshipValidationSpan.classList.add("text-danger", "field-validation-valid");
    relationshipValidationSpan.setAttribute('data-valmsg-for', `ConnectedPlaceList[${index}].Relationship`);
    relationshipValidationSpan.setAttribute('data-valmsg-replace', 'true');

    relationshipInput.insertAdjacentElement('afterend', relationshipValidationSpan);
    row.appendChild(tdRelationType);
  }
  row.appendChild(tdActions);

  // Append row before parsing validation so elements are in DOM
  tbody.appendChild(row);

  // Reinitialize unobtrusive validation for the containing form (preferred)
  try {
    const $row = $(row);
    const $form = $row.closest('form');
    if ($form && ($ as any).validator) {
      $form.removeData('validator');
      $form.removeData('unobtrusiveValidation');
      ($ as any).validator.unobtrusive.parse($form);
    } else {
      // fallback: parse just the new row
      ($ as any).validator.unobtrusive.parse($row);
    }
  } catch (e) {
    // ignore if jQuery/validator not available
    console.warn('Validation parse skipped', e);
  }

  hideModal('placeModal')
}
(function initRemovePlaceButtonHandler(): void {
  const container = document.getElementById('appendPlaceHere');
  if (!container) {
    return;
  }

  container.addEventListener('click', function (e: Event) {
    const target = e.target as HTMLElement;
    if (target && target.classList.contains('removePlace')) {
      const trTag = target.closest('.placeTableTr') as HTMLTableRowElement;
      if (trTag) {
        trTag.remove();
        reindexChildPlaces(container as HTMLTableSectionElement);
      }
    }
  });
})();
function reindexChildPlaces(container: HTMLTableSectionElement): void {
  const childRows = container.querySelectorAll("tr[data-type='child']") as NodeListOf<HTMLTableRowElement>;

  childRows.forEach((row: HTMLTableRowElement, i: number) => {
    const hiddenInput = row.querySelector(".placeId") as HTMLInputElement;
    if (hiddenInput) {
      hiddenInput.name = `ConnectedPlaceList[${i}].Id`;
    }
    const relationshipInput = row.querySelector(".placeRelationship") as HTMLInputElement;
    if (relationshipInput) {
      relationshipInput.name = `ConnectedPlaceList[${i}].Relationship`;
      relationshipInput.id = `ConnectedPlaceList_${i}__Relationship`;
    }
    // update validation span's data-valmsg-for if present
    const validationSpan = row.querySelector('[data-valmsg-for]') as HTMLElement;
    if (validationSpan) {
      validationSpan.setAttribute('data-valmsg-for', `ConnectedPlaceList[${i}].Relationship`);
    }
  });

  // Re-parse validation for the whole form after reindexing
  try {
    const $container = $(container);
    const $form = $container.closest('form');
    if ($form && ($ as any).validator) {
      $form.removeData('validator');
      $form.removeData('unobtrusiveValidation');
      ($ as any).validator.unobtrusive.parse($form);
    }
  } catch (e) {
    console.warn('Validation re-parse skipped', e);
  }
}

function openNav(): void {
  const sidebar = document.getElementById("mySidebar");
  if (!sidebar) return;

  if (window.innerWidth <= 600) {
    sidebar.style.width = "100%";
  } else {
    sidebar.style.width = "600px";
  }
}
function closeNav(): void {
  const sidebar = document.getElementById("mySidebar");
  if (sidebar) {
    sidebar.style.width = "0";
  }
}

const bodyToggle = document.querySelector('body') as HTMLBodyElement;
bodyToggle.addEventListener('click', function (e: MouseEvent) {
  const sideNavebar = document.getElementById("mySidebar") as HTMLDivElement;
  const target = e.target as HTMLElement;

  // Prüfe, ob der Klick auf die Sidebar selbst oder auf den Button erfolgt
  const isSidebar = target.closest('#mySidebar');
  const isOpenButton = target.closest('#openSidebarButton'); // ID deines Open-Buttons

  if (sideNavebar && sideNavebar.style.width !== "0" && !isSidebar && !isOpenButton) {
    closeNav();
  }
});

if (typeof window !== 'undefined') {
  window.addPlace = addPlace;
  window.openNav = openNav;
  window.closeNav = closeNav;
}