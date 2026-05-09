import { id_count, incrementCountRemove, incrementIdCount } from "./variables";
import { createRemoveButton } from "./createElements";
import { i18n } from "../TranslationService";

export function addColumn(): void {
  const source = document.getElementById("propertyHolder_0");
  if (!source) return;

  const clone = source.cloneNode(true) as HTMLElement;
  clone.id = `propertyHolder_${id_count}`;

  const dropDownLabel = clone.querySelector<HTMLLabelElement>(".columnDropDownLabel");
  if (dropDownLabel) {
    dropDownLabel.htmlFor = `columnName_${id_count}`;
  }

  const columnDropDown = clone.querySelector<HTMLSelectElement>(".columnDropDown");
  if (columnDropDown) {
    columnDropDown.id = `columnName_${id_count}`;
    //columnDropDown.value = "Column_Select";
  }

  const removeColumn = clone.querySelector<HTMLButtonElement>(".removeColumn");
  if (removeColumn) {
    removeColumn.id = `removeColumn_${id_count}`;
    removeColumn.style.display = "flex";
  }

  const inputArea = clone.querySelector<HTMLDivElement>(".inputArea");
  if (inputArea) {
    inputArea.id = `inputArea_${id_count}`;
  }

  const totalInput = clone.querySelector<HTMLInputElement>(".total_input");
  if (totalInput) {
    totalInput.id = `total_input${id_count}`;
    totalInput.value = "1";
  }

  const divAdd = clone.querySelector<HTMLDivElement>("#divAdd_0");
  if (divAdd) {
    divAdd.id = `divAdd_${id_count}`;
  }

  [
    ".multiselect",
    ".inputDiv_0",
    ".fieldAdd",
    ".fieldRemove",
    ".periodAdd",
  ].forEach((selector) => {
    const element = clone.querySelector(selector);
    element?.remove();
  });

  const formHolder = document.getElementById("propertyHolder-append");
  if (formHolder) {
    formHolder.appendChild(clone);
  }

  incrementIdCount();
  incrementCountRemove();

  const firstRemoveButton = document.getElementById("removeColumn_0") as HTMLElement;
  if (firstRemoveButton) {
    firstRemoveButton.style.display = "flex";
  }
}

// Global verfügbar machen
if (typeof window !== 'undefined') {
  (window as any).addColumn = addColumn;
}

export function addField(IdParent: string): void {
  const totalInputEl = document.getElementById(`total_input${IdParent}`) as HTMLInputElement;
  if (!totalInputEl) return;

  let last_input_count = Number.parseInt(totalInputEl.value, 10);

  const sourceDiv = document.getElementById(`inputDiv_${IdParent}0`);
  if (!sourceDiv) return;

  const cloneDiv = sourceDiv.cloneNode(true) as HTMLElement;
  cloneDiv.id = `inputDiv_${IdParent}${last_input_count}`;
  cloneDiv.style.display = "flex";

  const inputBox = cloneDiv.querySelector<HTMLInputElement>(`#inputBox_${IdParent}0`);
  if (inputBox) {
    inputBox.id = `inputBox_${IdParent}${last_input_count}`;
    inputBox.value = "";
  }

  const removeBtn = cloneDiv.querySelector<HTMLButtonElement>(`#removeField_${IdParent}0`);
  if (removeBtn) {
    removeBtn.id = `removeField_${IdParent}${last_input_count}`;
  }

  const targetParent = document.getElementById(`inputArea_${IdParent}`);
  if (!targetParent) return;

  targetParent.appendChild(cloneDiv);

  const new_input_count = last_input_count + 1;
  if (new_input_count === 2) {
    createRemoveButton(IdParent + last_input_count.toString());
    createRemoveButton(IdParent + "0");
  }

  const periodBtn = document.getElementById(`btnPeriod_${IdParent}`);
  periodBtn?.remove();

  const divAdd = document.getElementById(`divAdd_${IdParent}`);
  if (divAdd) {
    targetParent.insertBefore(divAdd, cloneDiv.nextSibling);
  }

  totalInputEl.value = new_input_count.toString();
}

// Global verfügbar machen
if (typeof window !== 'undefined') {
  (window as any).addField = addField;
}