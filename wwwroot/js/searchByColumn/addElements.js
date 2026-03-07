import { id_count, incrementCountRemove, incrementIdCount } from "./variables.js";
import { createRemoveButton } from "./createElements.js";
import { i18n } from "../TranslationService.js";
export function addColumn() {
    const source = document.getElementById("propertyHolder_0");
    if (!source)
        return;
    const clone = source.cloneNode(true);
    clone.id = `propertyHolder_${id_count}`;
    const columnDropDown = clone.querySelector(".columnDropDown");
    if (columnDropDown) {
        columnDropDown.id = `columnName_${id_count}`;
        columnDropDown.value = i18n.get("Option_Select");
    }
    const removeColumn = clone.querySelector(".removeColumn");
    if (removeColumn) {
        removeColumn.id = `removeColumn_${id_count}`;
        removeColumn.style.display = "flex";
    }
    const inputArea = clone.querySelector(".inputArea");
    if (inputArea) {
        inputArea.id = `inputArea_${id_count}`;
    }
    const totalInput = clone.querySelector(".total_input");
    if (totalInput) {
        totalInput.id = `total_input${id_count}`;
        totalInput.value = "1";
    }
    const divAdd = clone.querySelector("#divAdd_0");
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
    const firstRemoveButton = document.getElementById("removeColumn_0");
    if (firstRemoveButton) {
        firstRemoveButton.style.display = "flex";
    }
}
// Global verfügbar machen
if (typeof window !== 'undefined') {
    window.addColumn = addColumn;
}
export function addField(IdParent) {
    const totalInputEl = document.getElementById(`total_input${IdParent}`);
    if (!totalInputEl)
        return;
    let last_input_count = Number.parseInt(totalInputEl.value, 10);
    const sourceDiv = document.getElementById(`inputDiv_${IdParent}0`);
    if (!sourceDiv)
        return;
    const cloneDiv = sourceDiv.cloneNode(true);
    cloneDiv.id = `inputDiv_${IdParent}${last_input_count}`;
    cloneDiv.style.display = "flex";
    const inputBox = cloneDiv.querySelector(`#inputBox_${IdParent}0`);
    if (inputBox) {
        inputBox.id = `inputBox_${IdParent}${last_input_count}`;
        inputBox.value = "";
    }
    const removeBtn = cloneDiv.querySelector(`#removeField_${IdParent}0`);
    if (removeBtn) {
        removeBtn.id = `removeField_${IdParent}${last_input_count}`;
    }
    const targetParent = document.getElementById(`inputArea_${IdParent}`);
    if (!targetParent)
        return;
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
    window.addField = addField;
}
