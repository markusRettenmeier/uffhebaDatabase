import { id_count, incrementCountRemove, incrementIdCount} from "./variables.min.js";
import { createRemoveButton } from "./createElements.min.js";

export function addColumn() {
    const source = document.getElementById("propertyHolder_0");
    const clone = source.cloneNode(true);
    clone.id = `propertyHolder_${id_count}`;

    const columnDropDown = clone.querySelector(".columnDropDown");
    columnDropDown.id = `columnName_${id_count}`;
    columnDropDown.value = i18n.get("Option_Select");

    const removeColumn = clone.querySelector(".removeColumn");
    removeColumn.id = `removeColumn_${id_count}`;
    removeColumn.style.display = "flex";

    const inputArea = clone.querySelector(".inputArea");
    inputArea.id = `inputArea_${id_count}`;

    const totalInput = clone.querySelector(".total_input");
    totalInput.id = `total_input${id_count}`;
    totalInput.value = 1;

    const divAdd = clone.querySelector("#divAdd_0");
    divAdd.id = `divAdd_${id_count}`;

    [
        ".multiselect",
        ".inputDiv_0",
        ".fieldAdd",
        ".fieldRemove",
        ".periodAdd",
    ].forEach((name) => {
        let element = clone.querySelector(name);
        if (element) element.remove();
    });

    const formHolder = document.getElementById("propertyHolder-append");
    formHolder.appendChild(clone);

    incrementIdCount();
    incrementCountRemove();

    document.getElementById("removeColumn_0").style.display = "flex";
}
window.addColumn = addColumn;

export function addField(IdParent) {
    const totalInputEl = document.getElementById("total_input" + IdParent);
    let last_input_count = parseInt(totalInputEl.value, 10);

    const sourceDiv = document.getElementById("inputDiv_" + IdParent + "0");
    const cloneDiv = sourceDiv.cloneNode(true);

    cloneDiv.id = "inputDiv_" + IdParent + last_input_count;
    cloneDiv.style.display = "flex";

    const inputBox = cloneDiv.querySelector("#inputBox_" + IdParent + "0");
    if (inputBox) {
        inputBox.id = "inputBox_" + IdParent + last_input_count;
        inputBox.value = "";
    }

    const removeBtn = cloneDiv.querySelector("#removeField_" + IdParent + "0");
    if (removeBtn) {
        removeBtn.id = "removeField_" + IdParent + last_input_count;
    }

    const targetParent = document.getElementById("inputArea_" + IdParent);
    targetParent.appendChild(cloneDiv);

    let new_input_count = last_input_count + 1;
    if (new_input_count === 2) {
        createRemoveButton(IdParent.toString() + last_input_count);
        createRemoveButton(IdParent + "0");
    }

    const periodBtn = document.getElementById("btnPeriod_" + IdParent);
    if (periodBtn) periodBtn.remove();

    const divAdd = document.getElementById("divAdd_" + IdParent);
    if (divAdd) targetParent.insertBefore(divAdd, cloneDiv.nextSibling);

    totalInputEl.value = new_input_count;
}
window.addField = addField;