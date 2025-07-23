"use strict";

import { columns, columnsSimple } from "./columns.min.js";
import {
  createInput,
  createAddButton,
  createListBox,
  createPeriodButton
} from "./createElements.min.js";
import {
    addColumn,
    addField
} from "./addElements.min.js";

export function sessionStorageGetInput(index, columnNo) {
  let localKey = "";
  let type = "";

  const isGesamtansicht = window.location.href.includes("/Gesamtansicht");
  const isKurzansicht = window.location.href.includes("/Kurzansicht");

  if (isGesamtansicht) {
    localKey = columns[index][0];
    type = columns[index][1];
  } else if (isKurzansicht) {
    localKey = columnsSimple[index][0];
    type = columnsSimple[index][1];
  }

  const localValue = sessionStorage.getItem(localKey);
  if (!localValue) return;

  const valuesArray = localValue.split(";").filter((val) => val !== "");
  let inputNo = 0;

  if (columnNo > 0) {
    addColumn();
  }

  const columnDropdown = document.getElementById("columnName_" + columnNo);
  if (columnDropdown) columnDropdown.value = localKey;

  switch (type) {
    case "multiselect":
      createListBox(localKey, columnNo);
      valuesArray.forEach((value) => {
        const checkbox = document.querySelector(`[value="${value}"]`);
        if (checkbox) checkbox.checked = true;
      });
      break;

    case "checkbox":
      { createInput(localKey, "checkbox", columnNo);
      const checkboxInput = document.getElementById(`inputBox_${columnNo}0`);
      if (checkboxInput) checkboxInput.checked = true;
      break; }

    default:
      { valuesArray.forEach((value, index) => {
        if (index === 0) {
          createInput(localKey, type, columnNo);
        } else {
          addField(columnNo);
        }

        const inputBox = document.getElementById(
          `inputBox_${columnNo}${index}`
        );
        if (inputBox) inputBox.value = value;
        inputNo++;
      });

      const totalInput = document.getElementById(`total_input${columnNo}`);
      if (totalInput) totalInput.value = inputNo;

      if ((type === "date" || type === "number") && inputNo === 1) {
        createPeriodButton(columnNo);
      } else {
        createAddButton(columnNo);
      }
      break; }
  }
}
export function setSessionStorageData() {
  console.log("📦 setSessionStorageData gestartet.");

  // Spinner zeigen, Submit ausblenden
  document
    .querySelectorAll(".HochladenSubmitSuche")
    .forEach((el) => (el.style.display = "none"));
  document
    .querySelectorAll(".HochladenSpinnerSuche")
    .forEach((el) => (el.style.display = "inline"));

  sessionStorage.clear();
  console.log("🧹 sessionStorage geleert.");

  const columnsLength = columns.length;
  for (let i = 0; i < columnsLength; i++) {
    const [column, type] = columns[i];
    console.log(`🔍 Bearbeite Spalte: ${column} (Typ: ${type})`);

    switch (type) {
      case "checkbox":
        sessionStorageFillCheckBox(column);
        break;
      case "multiselect":
        sessionStorageFillListBox(column);
        break;
      default:
        sessionStorageFillInput(column);
    }
  }

  console.log("✅ setSessionStorageData abgeschlossen.");
}
window.setSessionStorageData = setSessionStorageData;

function sessionStorageFillInput(columnName) {
  const inputs = document.querySelectorAll(
    `.searchByColumnField[name="Box${columnName}"]`
  );
  let itemsColumn = "";

  inputs.forEach((input) => {
    if (!input || typeof input.value !== "string") return;
    const value = input.value.trim();
    if (value !== "") {
      itemsColumn += value + ";";
    }
  });

  if (itemsColumn !== "") {
    sessionStorage.setItem(columnName, itemsColumn);
    console.log(`📝 Eingabe gespeichert (${columnName}):`, itemsColumn);
  }
}

function sessionStorageFillListBox(columnName) {
  const checkedBoxes = document.querySelectorAll(
    `[name="Box${columnName}"]:checked`
  );
  let itemsColumn = "";

  checkedBoxes.forEach((checkbox) => {
    if (!checkbox || typeof checkbox.value !== "string") return;
    const value = checkbox.value.trim();
    if (value !== "") {
      itemsColumn += value + ";";
    }
  });

  if (itemsColumn !== "") {
    sessionStorage.setItem(columnName, itemsColumn);
    console.log(`📋 Mehrfachauswahl gespeichert (${columnName}):`, itemsColumn);
  }
}

function sessionStorageFillCheckBox(columnName) {
  const checked = document.querySelector(
    `[name="Box${columnName}"]:checked`
  );
  if (checked && typeof checked.value === "string") {
    sessionStorage.setItem(columnName, checked.value);
    console.log(`✅ Checkbox gespeichert (${columnName}):`, checked.value);
  }
}

function changingView() {
    document.getElementById("indexChangeButton").style.display = "none";
    document.getElementById("indexChangeSpinner").style.display = "inline";

    setSessionStorageData();
    getMoreData();
}
window.changingView = changingView;

function getMoreData() {
    let clonedDiv = document.querySelector(".divToTransmit").cloneNode(true);
    document.querySelectorAll(".transmitData").forEach(item => {
        item.append(clonedDiv.cloneNode(true));
    });
}
window.getMoreData = getMoreData;