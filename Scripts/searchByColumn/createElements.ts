import { i18n } from '../TranslationService.js';

interface ElementAttributes {
    [key: string]: string;
}

export function createInput(columnName: string, columnType: string, parentIdInput: string): void {
    const newChq = document.getElementById(`inputArea_${parentIdInput}`);
    if (!newChq) return;

    const elementDiv = createElement("div", {
        class: `input-group inputDiv_${parentIdInput}`,
        id: `inputDiv_${parentIdInput}0`,
    });

    const elementInput = createElement("input", {
        id: `inputBox_${parentIdInput}0`,
        name: `${columnName}`,
    });

    const typeSettings: Record<string, ElementAttributes> = {
        text: { type: "text", class: "searchByColumnField form-control" },
        year: { type: "number", max: "2100", class: "searchByColumnField form-control" },
        number: { type: "number", class: "searchByColumnField form-control" },
        date: { type: "date", class: "searchByColumnField form-control" },
        checkbox: { type: "checkbox", class: "searchByColumnField form-check-input" },
    };

    const settings = typeSettings[columnType] || {};
    Object.entries(settings).forEach(([key, value]) =>
        elementInput.setAttribute(key, value)
    );

    elementDiv.appendChild(elementInput);
    newChq.appendChild(elementDiv);

    const divAdd = document.getElementById(`divAdd_${parentIdInput}`);
    if (divAdd) {
        newChq.appendChild(divAdd);
    }
}

export function createListBox(colName: string, parentId: string): void {
    const newChq = document.getElementById(`inputArea_${parentId}`);
    if (!newChq) return;

    const newListBox = createElement("table", {
        id: `multiselect_${parentId}`,
        class: "multiselect",
    });

    const options: Record<string, Array<{ value: string; label: string }>> = {
        PartyTypeInt: [
            { value: "0", label: i18n.get("Individual") },
            { value: "1", label: i18n.get("Organization") }
        ],
        Organization_OrganizationTypeInt: [
            { value: "0", label: i18n.get("Company") },
            { value: "1", label: i18n.get("Institution") },
            { value: "2", label: i18n.get("Other") }
        ],
    };

    const currentOptions = options[colName];
    if (currentOptions) {
        currentOptions.forEach((option) => {
            const row = createElement("tr");
            const tdCheckbox = createElement("td");
            const tdLabel = createElement("td");

            const checkbox = createElement("input", {
                type: "checkbox",
                name: `Box${colName}`,
                value: option.value,
            });

            const label = createElement("label", {}, option.label);

            tdCheckbox.appendChild(checkbox);
            tdLabel.appendChild(label);
            row.appendChild(tdCheckbox);
            row.appendChild(tdLabel);
            newListBox.appendChild(row);
        });
    }

    newChq.appendChild(newListBox);
}

export function createAddButton(idParent: string): void {
    const addButton = createElement(
        "button",
        {
            class: "btn btn-outline-info start-0 fieldAdd",
            type: "button",
            id: `addField_${idParent}`,
        },
        "+"
    );

    document.getElementById(`divAdd_${idParent}`)?.appendChild(addButton);
}

export function createPeriodButton(idParent: string): void {
    const periodButton = createElement(
        "button",
        {
            class: "btn btn-outline-info periodAdd",
            type: "button",
            id: `btnPeriod_${idParent}`,
        },
        i18n.get("NumberRange_Change")
    );

    document.getElementById(`divAdd_${idParent}`)?.appendChild(periodButton);
}

export function createRemoveButton(idParent: string): void {
    const removeButton = createElement(
        "button",
        {
            class: "btn btn-outline-danger fieldRemove",
            type: "button",
            id: `removeField_${idParent}`,
        },
        "-"
    );

    const inputDiv = document.getElementById(`inputDiv_${idParent}`);
    if (inputDiv) {
        inputDiv.appendChild(removeButton);
    }
}

function createElement(tag: string, attributes: ElementAttributes = {}, textContent: string = ""): HTMLElement {
    const element = document.createElement(tag);
    Object.entries(attributes).forEach(([key, value]) =>
        element.setAttribute(key, value)
    );
    if (textContent) element.textContent = textContent;
    return element;
}