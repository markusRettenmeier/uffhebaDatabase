import { id_count, countRemove, decreaseCountRemove, resetCounters } from "./variables.js";
import { columns } from "./columns.js";
import { createInput, createAddButton, createListBox, createPeriodButton } from "./createElements.js";
import { sessionStorageGetInput } from "./sessionStorage.js";
import { addField, addColumn } from "./addElements.js";
import { i18n } from "../TranslationService.js";
export let columnsSelectedBefore = [];
export let chosenColumn = "";
document.addEventListener("click", function (event) {
    const target = event.target;
    if (target.classList.contains("fieldAdd") || target.classList.contains("periodAdd")) {
        const parentElement = target.closest("[id]");
        if (parentElement) {
            const parentId = parentElement.id.split("_").pop();
            if (parentId)
                addField(parentId);
        }
    }
    else if (target.classList.contains("fieldRemove")) {
        removeField(event);
    }
    else if (target.classList.contains("addColumn")) {
        addColumn();
    }
    else if (target.classList.contains("removeColumn")) {
        removeColumn(event);
    }
    else if (target.classList.contains("columnDropDown")) {
        onClickColumnDropDown(event);
    }
});
document.addEventListener("change", function (event) {
    const target = event.target;
    if (target.classList.contains("columnDropDown")) {
        changeColumnDropdown(event);
    }
});
if (window.location.href.includes("/Sammlung")) {
    let columnNo = 0;
    const storedColumns = columns; // Annahme: columns sind die gespeicherten Spalten
    storedColumns.forEach(([name], i) => {
        if (sessionStorage.getItem(name)) {
            sessionStorageGetInput(i, columnNo);
            columnNo++;
        }
    });
    if (columnNo === 0) {
        const dropdowns = document.querySelectorAll("#propertyHolder_0 .columnDropDown");
        dropdowns.forEach(dropdown => {
            dropdown.value = i18n.get("Option_Select");
        });
    }
}
function onClickColumnDropDown(event) {
    const select = event.target;
    chosenColumn = select.value;
    Array.from(select.options).forEach((option) => (option.hidden = false));
    columnsSelectedBefore.forEach((col) => {
        const optionToHide = select.querySelector(`option[value="${col}"]`);
        if (optionToHide)
            optionToHide.setAttribute("type", "hidden");
    });
    const defaultOption = select.querySelector('option[value=""]');
    if (defaultOption)
        defaultOption.setAttribute("type", "hidden");
}
function changeColumnDropdown(event) {
    const target = event.target;
    const parentElement = target.closest("[id]");
    let parentId;
    let column;
    if (parentElement) {
        parentId = parentElement.id.split("_").pop();
        column = target.value;
    }
    if (!parentId || !column)
        return;
    if (chosenColumn !== i18n.get("Option_Select")) {
        const index = columnsSelectedBefore.indexOf(chosenColumn);
        if (index > -1) {
            columnsSelectedBefore.splice(index, 1);
        }
    }
    if (column !== i18n.get("Option_Select")) {
        columnsSelectedBefore.push(column);
    }
    const inputDiv = document.querySelector(`.inputDiv_${parentId}`);
    if (inputDiv)
        inputDiv.remove();
    ["btnPeriod_", "multiselect_", "removeField_", "addField_"].forEach((prefix) => {
        const element = document.getElementById(`${prefix}${parentId}`);
        element?.remove();
    });
    const totalInput = document.getElementById(`total_input${parentId}`);
    if (totalInput)
        totalInput.value = "1";
    const columnType = columns.find(([name]) => name === column)?.[1];
    if (!columnType)
        return;
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
    if (!parentDiv)
        return;
    const classSuffix = parentDiv.className.split("_").pop();
    if (!classSuffix)
        return;
    const totalInputEl = document.getElementById(`total_input${classSuffix}`);
    if (!totalInputEl)
        return;
    let totalInputs = Number.parseInt(totalInputEl.value);
    const parentId = parentDiv.id;
    parentDiv.remove();
    totalInputs--;
    totalInputEl.value = totalInputs.toString();
    const container = document.getElementById(`inputArea_${classSuffix}`);
    if (!container)
        return;
    const remainingInputs = Array.from(container.querySelectorAll(`.inputDiv_${classSuffix}`)).filter((div) => div.id.startsWith("inputDiv_"));
    if (parentId === `inputDiv_${classSuffix}0` && remainingInputs.length > 0) {
        const firstDiv = remainingInputs[0];
        firstDiv.id = `inputDiv_${classSuffix}0`;
        const input = firstDiv.querySelector("input");
        if (input)
            input.id = `inputBox_${classSuffix}0`;
        const btn = firstDiv.querySelector("button");
        if (btn)
            btn.id = `removeField_${classSuffix}0`;
    }
    const type = parentDiv.querySelector("input")?.type;
    if (type === "number" || type === "date") {
        createPeriodButton(classSuffix);
    }
    if (totalInputs === 1) {
        const removeBtn = document.getElementById(`removeField_${classSuffix}0`);
        removeBtn?.remove();
    }
    reindexFields(classSuffix);
}
function reindexFields(parentSuffix) {
    const container = document.getElementById(`inputArea_${parentSuffix}`);
    if (!container)
        return;
    const inputDivs = Array.from(container.querySelectorAll(`.inputDiv_${parentSuffix}`)).filter((div) => div.id.startsWith("inputDiv_"));
    inputDivs.forEach((div, index) => {
        const newId = `inputDiv_${parentSuffix}${index}`;
        div.id = newId;
        const input = div.querySelector("input");
        if (input) {
            input.id = `inputBox_${parentSuffix}${index}`;
        }
        const btn = div.querySelector("button");
        if (btn) {
            btn.id = `removeField_${parentSuffix}${index}`;
        }
    });
    const totalInputEl = document.getElementById(`total_input${parentSuffix}`);
    if (totalInputEl) {
        totalInputEl.value = inputDivs.length.toString();
    }
    if (inputDivs.length === 1) {
        const removeBtn = document.getElementById(`removeField_${parentSuffix}0`);
        removeBtn?.remove();
    }
}
function removeColumn(event) {
    const button = event.target;
    const closestForm = button.closest("div")?.parentElement;
    if (!closestForm)
        return;
    const closestId = closestForm.id;
    const selectedColumn = closestForm.querySelector(".columnDropDown option:checked")?.value;
    if (selectedColumn) {
        columnsSelectedBefore = columnsSelectedBefore.filter((col) => col !== selectedColumn);
    }
    closestForm.remove();
    if (closestId === "propertyHolder_0") {
        const firstHolder = document.querySelector(".SbCDiv:first-child");
        if (!firstHolder)
            return;
        firstHolder.id = "propertyHolder_0";
        const idMappings = {
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
            const element = firstHolder.querySelector(selector);
            if (element)
                element.id = newId;
        });
        const inputDivs = firstHolder.querySelectorAll('#inputArea_0 div[id^="inputDiv"]');
        inputDivs.forEach((div, index) => {
            div.id = `inputDiv_0${index}`;
            div.className = "input-group inputDiv_0";
            const input = div.querySelector("input");
            if (input)
                input.id = `inputBox_0${index}`;
            const button = div.querySelector("button");
            if (button)
                button.id = `removeField_0${index}`;
        });
    }
    decreaseCountRemove();
    if (countRemove === 1) {
        const removeButton = document.getElementById("removeColumn_0");
        if (removeButton)
            removeButton.style.display = "none";
    }
}
export function searchReset() {
    sessionStorage.clear();
    for (let i = 1; i < id_count; i++) {
        const propertyHolder = document.getElementById(`propertyHolder_${i}`);
        propertyHolder?.remove();
    }
    const baseForm = document.getElementById("propertyHolder_0");
    if (baseForm) {
        const multiselect = baseForm.querySelector(".multiselect");
        multiselect?.remove();
        const inputArea = baseForm.querySelector(".inputArea");
        if (inputArea) {
            inputArea.querySelectorAll(".inputDiv_0").forEach((div) => div.remove());
        }
        const fieldAdd = baseForm.querySelector(".fieldAdd");
        fieldAdd?.remove();
        const addPeriod = baseForm.querySelector(".periodAdd");
        addPeriod?.remove();
        const totalInput = baseForm.querySelector(".total_input");
        if (totalInput)
            totalInput.value = "1";
        const columnDropdown = baseForm.querySelector(".columnDropDown");
        if (columnDropdown)
            columnDropdown.value = i18n.get("Option_Select");
    }
    resetCounters();
    columnsSelectedBefore = [];
}
// Globale Verfügbarkeit
if (typeof window !== 'undefined') {
    window.searchReset = searchReset;
}
