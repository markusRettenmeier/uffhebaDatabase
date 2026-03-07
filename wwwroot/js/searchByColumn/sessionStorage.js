import { columns } from "./columns.js";
import { createInput, createAddButton, createListBox, createPeriodButton } from "./createElements.js";
import { addColumn, addField } from "./addElements.js";
export function sessionStorageGetInput(index, columnNo) {
    const column = columns[index];
    if (!column)
        return;
    const [localKey, type] = column;
    const localValue = sessionStorage.getItem(localKey);
    if (!localValue)
        return;
    const valuesArray = localValue.split(";").filter((val) => val !== "");
    let inputNo = 0;
    if (columnNo > 0) {
        addColumn();
    }
    const columnDropdown = document.getElementById(`columnName_${columnNo}`);
    if (columnDropdown)
        columnDropdown.value = localKey;
    switch (type) {
        case "multiselect":
            createListBox(localKey, columnNo.toString());
            valuesArray.forEach((value) => {
                const checkbox = document.querySelector(`[value="${value}"]`);
                if (checkbox)
                    checkbox.checked = true;
            });
            break;
        case "checkbox":
            createInput(localKey, "checkbox", columnNo.toString());
            const checkboxInput = document.getElementById(`inputBox_${columnNo}0`);
            if (checkboxInput)
                checkboxInput.checked = true;
            break;
        default:
            valuesArray.forEach((value, index) => {
                if (index === 0) {
                    createInput(localKey, type, columnNo.toString());
                }
                else {
                    addField(columnNo.toString());
                }
                const inputBox = document.getElementById(`inputBox_${columnNo}${index}`);
                if (inputBox)
                    inputBox.value = value;
                inputNo++;
            });
            const totalInput = document.getElementById(`total_input${columnNo}`);
            if (totalInput)
                totalInput.value = inputNo.toString();
            if ((type === "date" || type === "number") && inputNo === 1) {
                createPeriodButton(columnNo.toString());
            }
            else {
                createAddButton(columnNo.toString());
            }
            break;
    }
}
export function setSessionStorageData() {
    document.querySelectorAll(".HochladenSubmitSuche").forEach((el) => {
        el.style.display = "none";
    });
    document.querySelectorAll(".HochladenSpinnerSuche").forEach((el) => {
        el.style.display = "inline";
    });
    sessionStorage.clear();
    columns.forEach(([columnName, type]) => {
        switch (type) {
            case "checkbox":
                sessionStorageFillCheckBox(columnName);
                break;
            case "multiselect":
                sessionStorageFillListBox(columnName);
                break;
            default:
                sessionStorageFillInput(columnName);
        }
    });
}
function sessionStorageFillInput(columnName) {
    const inputs = document.querySelectorAll(`.searchByColumnField[name="Box${columnName}"]`);
    let itemsColumn = "";
    inputs.forEach((input) => {
        if (!input || typeof input.value !== "string")
            return;
        const value = input.value.trim();
        if (value !== "") {
            itemsColumn += value + ";";
        }
    });
    if (itemsColumn !== "") {
        sessionStorage.setItem(columnName, itemsColumn);
    }
}
function sessionStorageFillListBox(columnName) {
    const checkedBoxes = document.querySelectorAll(`[name="Box${columnName}"]:checked`);
    let itemsColumn = "";
    checkedBoxes.forEach((checkbox) => {
        if (!checkbox || typeof checkbox.value !== "string")
            return;
        const value = checkbox.value.trim();
        if (value !== "") {
            itemsColumn += value + ";";
        }
    });
    if (itemsColumn !== "") {
        sessionStorage.setItem(columnName, itemsColumn);
    }
}
function sessionStorageFillCheckBox(columnName) {
    const checked = document.querySelector(`[name="Box${columnName}"]:checked`);
    if (checked && typeof checked.value === "string") {
        sessionStorage.setItem(columnName, checked.value);
    }
}
export function changingView() {
    const indexChangeButton = document.getElementById("indexChangeButton");
    const indexChangeSpinner = document.getElementById("indexChangeSpinner");
    if (indexChangeButton)
        indexChangeButton.style.display = "none";
    if (indexChangeSpinner)
        indexChangeSpinner.style.display = "inline";
    setSessionStorageData();
    getMoreData();
}
export function getMoreData() {
    const clonedDiv = document.querySelector(".divToTransmit")?.cloneNode(true);
    if (!clonedDiv)
        return;
    document.querySelectorAll(".transmitData").forEach(item => {
        item.appendChild(clonedDiv.cloneNode(true));
    });
}
// Globale Verfügbarkeit
if (typeof window !== 'undefined') {
    window.setSessionStorageData = setSessionStorageData;
    window.changingView = changingView;
    window.getMoreData = getMoreData;
}
