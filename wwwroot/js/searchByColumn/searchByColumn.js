import { id_count, countRemove, decreaseCountRemove, resetCounters } from "./variables.min.js";
import { columns } from "./columns.js";
import { createInput, createAddButton, createListBox, createPeriodButton} from "./createElements.js";
import { sessionStorageGetInput } from "./sessionStorage.js";
import { addField, addColumn} from "./addElements.min.js";

export let columnsSelectedBefore = [];
export let chosenColumn;

"use strict";

document.addEventListener("click", function (event) {
  if (
    event.target.classList.contains("fieldAdd") ||
    event.target.classList.contains("periodAdd")
  ) {
    const parentId = event.target.closest("[id]")?.id;
    if (parentId) {
      addField(parentId.split("_").pop());
    }
  } else if (event.target.classList.contains("fieldRemove")) {
    removeField(event);
  } else if (event.target.classList.contains("addColumn")) {
    addColumn(event);
  } else if (event.target.classList.contains("removeColumn")) {
    removeColumn(event);
  } else if (event.target.classList.contains("columnDropDown")) {
    onClickColumnDropDown(event);
  }
});

document.addEventListener("change", function (event) {
  if (event.target.classList.contains("columnDropDown")) {
    changeColumnDropdown(event);
  }
});

if (window.location.href.includes("/Sammlung")) {
  let columnNo = 0;

  storedColumns.forEach(([name], i) => {
    if (sessionStorage.getItem(name)) {
      sessionStorageGetInput(i, columnNo, storedColumns);
      columnNo++;
    }
  });

  if (columnNo === 0) {
    document.querySelectorAll("#propertyHolder_0.columnDropDown").value =
      "Wähle_Spalte";
  }
}

function onClickColumnDropDown(event) {
  const select = event.target;
  chosenColumn = select.value;

  // Show all options first
  Array.from(select.options).forEach((option) => (option.hidden = false));

  // Hide selected columns
  columnsSelectedBefore.forEach((col) => {
    const optionToHide = select.querySelector(`option[value="${col}"]`);
    if (optionToHide) optionToHide.hidden = true;
  });

  // Always hide the default "Wähle_Spalte" option
  const defaultOption = select.querySelector(`option[value=""]`);
  if (defaultOption) defaultOption.hidden = true;
}

function changeColumnDropdown(event) {
  let target = event.target.closest("[id]");
  let column, parentId;
  if (target) {
    parentId = target.id.split("_").pop();
    column = target.value;
  }

    if (chosenColumn != i18n.get("Option_Select")) {
    columnsSelectedBefore.splice(
      columnsSelectedBefore.indexOf(chosenColumn),
      1
    );
  }
  columnsSelectedBefore.push(column);

  let inputDiv = document.querySelector(".inputDiv_" + parentId);
  if (inputDiv != null) inputDiv.parentNode.removeChild(inputDiv);
  ["btnPeriod_", "multiselect_", "removeField_", "addField_"].forEach(
    (prefix) => {
      let element = document.getElementById(prefix + parentId);
      if (element) element.remove();
    }
  );
  document.getElementById("total_input" + parentId).value = 1;

  let columnType = columns.find(([name]) => name === column)?.[1];
  if (!columnType) return;

  switch (columnType) {
    case "multiselect":
      createListBox(column, parentId);
      break;
    case "year":
    case "date":
    case "number":
      createInput(column, columnType, parentId);
      createPeriodButton(parentId);
      break;
    case "checkbox":
      createInput(column, "checkbox", parentId);
      break;
    default:
      createInput(column, columnType, parentId);
      createAddButton(parentId);
      break;
  }
}

function removeField(event) {
  const button = event.target;
  const parentDiv = button.closest("div");
  if (!parentDiv) return;

  const classSuffix = parentDiv.className.split("_").pop();
  const totalInputEl = document.getElementById("total_input" + classSuffix);
  let totalInputs = parseInt(totalInputEl.value);

  const type = parentDiv.querySelector("input")?.type;
  const parentId = parentDiv.id;

  parentDiv.remove();
  totalInputs--;
  totalInputEl.value = totalInputs;

  const container = document.getElementById("inputArea_" + classSuffix);
  const remainingInputs = Array.from(
    container.querySelectorAll(`.inputDiv_${classSuffix}`)
  ).filter((div) => div.id.startsWith("inputDiv_"));

  // Reassign the new first div to _0
  if (parentId === `inputDiv_${classSuffix}0` && remainingInputs.length > 0) {
    const firstDiv = remainingInputs[0];
    firstDiv.id = `inputDiv_${classSuffix}0`;

    const input = firstDiv.querySelector("input");
    if (input) input.id = `inputBox_${classSuffix}0`;

    const btn = firstDiv.querySelector("button");
    if (btn) btn.id = `removeField_${classSuffix}0`;
  }

  if (type === "number" || type === "date") {
    createPeriodButton(classSuffix);
  }

  if (totalInputs === 1) {
    const removeBtn = document.getElementById(`removeField_${classSuffix}0`);
    if (removeBtn) removeBtn.remove();
  }

  reindexFields(classSuffix);
}

function reindexFields(parentSuffix) {
  const container = document.getElementById("inputArea_" + parentSuffix);
  const inputDivs = Array.from(
    container.querySelectorAll(`.inputDiv_${parentSuffix}`)
  ).filter((div) => div.id.startsWith("inputDiv_"));

  inputDivs.forEach((div, index) => {
    const newId = `inputDiv_${parentSuffix}${index}`;
    div.id = newId;

    // Update input box
    const input = div.querySelector("input");
    if (input) {
      input.id = `inputBox_${parentSuffix}${index}`;
    }

    // Update remove button
    const btn = div.querySelector("button");
    if (btn) {
      btn.id = `removeField_${parentSuffix}${index}`;
    }
  });

  // Update total input count
  const totalInputEl = document.getElementById("total_input" + parentSuffix);
  totalInputEl.value = inputDivs.length;

  // Remove extra remove buttons if only one input remains
  if (inputDivs.length === 1) {
    const removeBtn = document.getElementById(`removeField_${parentSuffix}0`);
    if (removeBtn) removeBtn.remove();
  }
}

// Function to add a new column dynamically


// Function to remove a column dynamically
function removeColumn(event) {
  let closestForm = event.target.closest("div").parentElement;
  if (!closestForm) return;

  let closestId = closestForm.id;
  let selectedColumn = closestForm.querySelector(
    ".columnDropDown option:checked"
  )?.value;

  columnsSelectedBefore = columnsSelectedBefore.filter(
    (col) => col !== selectedColumn
  );
  closestForm.remove();

  if (closestId === "propertyHolder_0") {
    let firstHolder = document.querySelector(".SbCDiv:first-child");
    if (!firstHolder) return;
    firstHolder.id = "propertyHolder_0";

    let idMappings = {
      ".removeColumn": "removeColumn_0",
      ".columnDropDown": "columnName_0",
      ".divAdd": "divAdd_0",
      ".fieldAdd": "addField_0",
      ".periodAdd": "btnPeriod_0",
      ".inputArea": "inputArea_0",
      ".total_input": "total_input0",
      ".multiselect": "multiselect_0",
    };

    Object.entries(idMappings).forEach(([selector, newId]) => {
      let element = firstHolder.querySelector(selector);
      if (element) element.id = newId;
    });

    let inputDivs = firstHolder.querySelectorAll(
      '#inputArea_0 div[id^="inputDiv"]'
    );
    inputDivs.forEach((div, index) => {
      div.id = `inputDiv_0${index}`;
      div.className = "input-group inputDiv_0";
      let input = div.querySelector("input");
      if (input) input.id = `inputBox_0${index}`;
      let button = div.querySelector("button");
      if (button) button.id = `removeField_0${index}`;
    });
  }

    decreaseCountRemove();
  if (countRemove === 1) {
    let removeButton = document.getElementById("removeColumn_0");
    if (removeButton) removeButton.style.display = "none";
  }
}

function searchReset() {
  window.sessionStorage.clear();

  for (let i = 1; i < id_count; i++) {
    const propertyHolder = document.getElementById("propertyHolder_" + i);
    if (propertyHolder) {
      propertyHolder.remove();
    }
  }

  const baseForm = document.getElementById("propertyHolder_0");
  if (baseForm) {
    const multiselect = baseForm.querySelector(".multiselect");
    if (multiselect) multiselect.remove();

    const inputArea = baseForm.querySelector(".inputArea");
    if (inputArea) {
      inputArea.querySelectorAll(".inputDiv_0").forEach((div) => div.remove());
    }

    const fieldAdd = baseForm.querySelector(".fieldAdd");
    if (fieldAdd) fieldAdd.remove();

    const addPeriod = baseForm.querySelector(".periodAdd");
    if (addPeriod) addPeriod.remove();

    const totalInput = baseForm.querySelector(".total_input");
    if (totalInput) totalInput.value = 1;

    const columnDropdown = baseForm.querySelector(".columnDropDown");
    if (columnDropdown) columnDropdown.value = "Wähle_Spalte";
  }

    resetCounters();
  columnsSelectedBefore = [];
}
window.searchReset = searchReset;
