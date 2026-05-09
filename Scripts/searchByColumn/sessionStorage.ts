import { columns } from "./columns";
import { createInput, createAddButton, createListBox, createPeriodButton } from "./createElements";
import { addColumn, addField } from "./addElements";

export function sessionStorageGetInput(index: number, columnNo: number): void {
    const column = columns[index];
    if (!column) return;

    const [localKey, type] = column;
    const localValue = sessionStorage.getItem(localKey);
    if (!localValue) return;

    const valuesArray = localValue.split(";").filter((val) => val !== "");
    let inputNo = 0;

    if (columnNo > 0) {
        addColumn();
    }

    const columnDropdown = document.getElementById(`columnName_${columnNo}`) as HTMLSelectElement;
    if (columnDropdown) columnDropdown.value = localKey;

    switch (type) {
        case "multiselect":
            createListBox(localKey, columnNo.toString());
            valuesArray.forEach((value) => {
                const checkbox = document.querySelector<HTMLInputElement>(`[value="${value}"]`);
                if (checkbox) checkbox.checked = true;
            });
            break;

        case "checkbox":
            createInput(localKey, "checkbox", columnNo.toString());
            const checkboxInput = document.getElementById(`inputBox_${columnNo}0`) as HTMLInputElement;
            if (checkboxInput) checkboxInput.checked = true;
            break;

        default:
            valuesArray.forEach((value, index) => {
                if (index === 0) {
                    createInput(localKey, type, columnNo.toString());
                } else {
                    addField(columnNo.toString());
                }

                const inputBox = document.getElementById(`inputBox_${columnNo}${index}`) as HTMLInputElement;
                if (inputBox) inputBox.value = value;
                inputNo++;
            });

            const totalInput = document.getElementById(`total_input${columnNo}`) as HTMLInputElement;
            if (totalInput) totalInput.value = inputNo.toString();

            if ((type === "date" || type === "number") && inputNo === 1) {
                createPeriodButton(columnNo.toString());
            } else {
                createAddButton(columnNo.toString());
            }
            break;
    }
}

export function setSessionStorageData(): void {
    document.querySelectorAll<HTMLElement>(".HochladenSubmitSuche").forEach((el) => {
        el.style.display = "none";
    });

    document.querySelectorAll<HTMLElement>(".HochladenSpinnerSuche").forEach((el) => {
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

function sessionStorageFillInput(columnName: string): void {
    const inputs = document.querySelectorAll<HTMLInputElement>(`.searchByColumnField[name="Box${columnName}"]`);
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
    }
}

function sessionStorageFillListBox(columnName: string): void {
    const checkedBoxes = document.querySelectorAll<HTMLInputElement>(`[name="Box${columnName}"]:checked`);
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
    }
}

function sessionStorageFillCheckBox(columnName: string): void {
    const checked = document.querySelector<HTMLInputElement>(`[name="Box${columnName}"]:checked`);
    if (checked && typeof checked.value === "string") {
        sessionStorage.setItem(columnName, checked.value);
    }
}

export function changingView(): void {
    const indexChangeButton = document.getElementById("indexChangeButton");
    const indexChangeSpinner = document.getElementById("indexChangeSpinner");

    if (indexChangeButton) indexChangeButton.style.display = "none";
    if (indexChangeSpinner) indexChangeSpinner.style.display = "inline";

    setSessionStorageData();
    getMoreData();
}

export function getMoreData(): void {
    const clonedDiv = document.querySelector<HTMLElement>(".divToTransmit")?.cloneNode(true);
    if (!clonedDiv) return;

    document.querySelectorAll<HTMLElement>(".transmitData").forEach(item => {
        item.appendChild(clonedDiv.cloneNode(true));
    });
}

// Globale Verfügbarkeit
if (typeof window !== 'undefined') {
    (window as any).setSessionStorageData = setSessionStorageData;
    (window as any).changingView = changingView;
    (window as any).getMoreData = getMoreData;
}