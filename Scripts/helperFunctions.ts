import { TdOptions } from "./types";
import { getTranslation } from "./shared";

export function hideModal(modalName: string): void {
  const modal = document.getElementById(modalName);

  if (modal?.classList.contains('modal')) {
    const modalInstance = bootstrap.Modal.getInstance(modal) || new bootstrap.Modal(modal);
    modalInstance.hide();
  }
}

export function showModalFunction(modalName: string): void {
  const modalElement = document.getElementById(modalName);

  if (modalElement && typeof (window as any).bootstrap !== 'undefined') {
    const modal = new (window as any).bootstrap.Modal(modalElement);
    modal.show();
  }
}


// Fehlerbehandlung (ohne jQuery)
export function sendErrorMessage(error: Error | string): void {
  console.error("fetch-Error:", error);
  const createErrorSpan = document.querySelector<HTMLElement>('#createErrorSpan');
  if (createErrorSpan) {
    createErrorSpan.textContent = error.toString();
    createErrorSpan.style.color = 'red';
  }
}
export function autocomplete(
  input: HTMLInputElement,
  values: string[]
): void {
  let currentFocus: number = -1;

  input.addEventListener("input", () => {
    const val: string = input.value;
    closeAllLists();

    if (!val) return;

    currentFocus = -1;

    const listContainer: HTMLDivElement = document.createElement("div");
    listContainer.id = `${input.id}-autocomplete-list`;
    listContainer.className = "autocomplete-div";

    input.parentElement?.appendChild(listContainer);

    for (const item of values) {
      if (item.toUpperCase().startsWith(val.toUpperCase())) {
        const itemElement: HTMLDivElement = document.createElement("div");

        const strongPart = item.substring(0, val.length);
        const restPart = item.substring(val.length);

        itemElement.innerHTML = `
          <strong>${strongPart}</strong>${restPart}
          <input type="hidden" value="${item}">
        `;

        itemElement.addEventListener("click", () => {
          const hiddenInput = itemElement.querySelector(
            "input"
          ) as HTMLInputElement | null;

          if (hiddenInput) {
            input.value = hiddenInput.value;
          }

          closeAllLists();
        });

        listContainer.appendChild(itemElement);
      }
    }
  });

  input.addEventListener("keydown", (e: KeyboardEvent) => {
    const list = document.getElementById(
      `${input.id}-autocomplete-list`
    );

    const items = list?.getElementsByTagName("div");

    if (!items) return;

    switch (e.key) {
      case "ArrowDown":
        currentFocus++;
        addActive(items);
        break;

      case "ArrowUp":
        currentFocus--;
        addActive(items);
        break;

      case "Enter":
        e.preventDefault();
        if (currentFocus > -1 && items[currentFocus]) {
          items[currentFocus].click();
        }
        break;
    }
  });

  function addActive(elements: HTMLCollectionOf<HTMLDivElement>): void {
    if (!elements.length) return;

    removeActive(elements);

    if (currentFocus >= elements.length) currentFocus = 0;
    if (currentFocus < 0) currentFocus = elements.length - 1;

    elements[currentFocus].classList.add("autocomplete-active");
  }

  function removeActive(elements: HTMLCollectionOf<HTMLDivElement>): void {
    for (const el of Array.from(elements)) {
      el.classList.remove("autocomplete-active");
    }
  }

  function closeAllLists(exceptElement?: EventTarget | null): void {
    const lists = document.getElementsByClassName("autocomplete-div");

    for (const list of Array.from(lists)) {
      if (
        exceptElement !== list &&
        exceptElement !== input
      ) {
        list.parentElement?.removeChild(list);
      }
    }
  }

  document.addEventListener("click", (e: MouseEvent) => {
    closeAllLists(e.target);
  });
}

// Helper-Funktion für Action Cells
export function createActionCell(idx: number, urlPatterns: string[], buttonTextKey: string, onClickFunction: string): HTMLTableCellElement {
  const tdAction = document.createElement('td');
  const div = document.createElement('div');
  div.className = 'btn-group';

  const button = document.createElement('button');
  button.type = 'button';
  button.classList.add('btn', 'btn-primary', 'btn-sm');
  button.textContent = i18n.get(buttonTextKey);
  button.onclick = (): void => {
    const func = (window as any)[onClickFunction];
    if (typeof func === 'function') func(idx);
  };
  div.appendChild(button);

  tdAction.appendChild(div);
  return tdAction;
}
// Helper-Funktion
export const createTd = ({ text = '', id = null, scope = null }: TdOptions): HTMLTableCellElement => {
  const td = document.createElement('td');
  if (id) td.id = id;
  if (scope) td.setAttribute('scope', scope);
  td.textContent = text;
  return td;
};