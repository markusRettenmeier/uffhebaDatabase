"use strict";

export function createInput(columnName, columnType, parentIdInput) {
  let newChq = document.getElementById(`inputArea_${parentIdInput}`);
  if (!newChq) return;

  let elementDiv = createElement("div", {
    class: `input-group inputDiv_${parentIdInput}`,
    id: `inputDiv_${parentIdInput}0`,
  });
  let elementInput = createElement("input",  {
    id: `inputBox_${parentIdInput}0`,
    name: `${columnName}`,
  });

  // Assign input attributes based on columnType
  let typeSettings = {
    text: { type: "text", class: "searchByColumnField form-control" },
    year: {
      type: "number",
      max: "2100",
      class: "searchByColumnField form-control",
    },
    number: { type: "number", class: "searchByColumnField form-control" },
    date: { type: "date", class: "searchByColumnField form-control" },
    checkbox: {
      type: "checkbox",
      class: "searchByColumnField form-check-input",
    },
  };

  Object.entries(typeSettings[columnType] || {}).forEach(([key, value]) =>
    elementInput.setAttribute(key, value)
  );

  elementDiv.appendChild(elementInput);
  newChq.appendChild(elementDiv);
  const divAdd = document.getElementById(`divAdd_${parentIdInput}`);
  newChq.appendChild(divAdd);
}
// Function to create a multiselect table based on predefined options
export function createListBox(colName, parentId) {
  let newChq = document.getElementById(`inputArea_${parentId}`);
  if (!newChq) return;

  let newListBox = createElement("table", {
    id: `multiselect_${parentId}`,
    class: "multiselect",
  });

  const options = {
      ToponymyTypeInt: [
          { value: "0", label: i18n.get("Field") },
          { value: "1", label: i18n.get("Region") },
          { value: "2", label: i18n.get("TransportRoute") },
          { value: "3", label: i18n.get("BodyOfWater") },
          { value: "4", label: i18n.get("Settlement") },
          { value: "5", label: i18n.get("Building") },
          { value: "6", label: i18n.get("Relief") },
    ],
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

  if (options[colName]) {
    options[colName].forEach((option) => {
      let row = createElement("tr");
      let tdCheckbox = createElement("td");
      let tdLabel = createElement("td");

      let checkbox = createElement("input", {
        type: "checkbox",
        name: `Box${colName}`,
        value: option.value,
      });
      let label = createElement("label", {}, option.label);

      tdCheckbox.appendChild(checkbox);
      tdLabel.appendChild(label);
      row.appendChild(tdCheckbox);
      row.appendChild(tdLabel);
      newListBox.appendChild(row);
    });
  }

  newChq.appendChild(newListBox);
}
// Function to create an add button
export function createAddButton(idParent) {
  let addButton = createElement(
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
// Function to create a period button
export function createPeriodButton(idParent) {
  let periodButton = createElement(
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
// Function to create a remove button
export function createRemoveButton(idParent) {
  let removeButton = createElement(
    "button",
    {
      class: "btn btn-outline-danger fieldRemove",
      type: "button",
      id: `removeField_${idParent}`,
    },
    "-"
  );

  let inputDiv = document.getElementById(`inputDiv_${idParent}`);
  if (inputDiv) {
    inputDiv.appendChild(removeButton);
  }
}

// Helper function to create an element with attributes
function createElement(tag, attributes = {}, textContent = "") {
  let element = document.createElement(tag);
  Object.entries(attributes).forEach(([key, value]) =>
    element.setAttribute(key, value)
  );
  if (textContent) element.textContent = textContent;
  return element;
}
