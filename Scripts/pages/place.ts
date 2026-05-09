import { i18n } from "../TranslationService";

function addToponymy(): void {
  const container = document.getElementById("toponymyContainer") as HTMLDivElement;
  if (!container) {
    console.error('toponymyContainer element not found');
    return;
  }

  const index = container.children.length;

  const divWrapper = document.createElement("div");
  divWrapper.className = "input-group inputDivToponymy mb-2";

  const divInputs = document.createElement("div");
  divInputs.className = "form-group";

  const toponymyInput = document.createElement("input");
  toponymyInput.type = "text";
  toponymyInput.className = "form-control inputToponymy";
  toponymyInput.name = `ToponymyList[${index}].Name`;
  toponymyInput.placeholder = i18n.get("EnterToponymy");

  divInputs.appendChild(toponymyInput);

  const divCheckbox = document.createElement("div");
  divCheckbox.className = "form-check";

  const checkboxId = `ToponymyList_${index}__IsCurrentName`;
  const currentNameCheckbox = document.createElement("input");
  currentNameCheckbox.type = "checkbox";
  currentNameCheckbox.className = "form-check-input checkboxCurrentName";
  currentNameCheckbox.name = `ToponymyList[${index}].IsCurrentName`;
  currentNameCheckbox.value = "true";
  currentNameCheckbox.id = checkboxId;
  currentNameCheckbox.setAttribute("data-val", "true");
  currentNameCheckbox.setAttribute("data-val-required", i18n.get("Error_Name_Required"));
  currentNameCheckbox.setAttribute("aria-required", "true");
  currentNameCheckbox.setAttribute("aria-invalid", "false");
  currentNameCheckbox.setAttribute("aria-describedby", `ToponymyList_${index}__Name-error`);
  divCheckbox.appendChild(currentNameCheckbox);

  const currentNameLabel = document.createElement("label");
  currentNameLabel.className = "form-check-label ms-1";
  currentNameLabel.htmlFor = checkboxId;
  currentNameLabel.textContent = i18n.get("IsCurrentName") || "Is current name";
  divCheckbox.appendChild(currentNameLabel);

  const removeButton = document.createElement("button");
  removeButton.type = "button";
  removeButton.className = "btn btn-danger removeToponmy";
  removeButton.textContent = i18n.get("Remove") || "Remove";

  divWrapper.appendChild(divInputs);
  divWrapper.appendChild(divCheckbox);
  divWrapper.appendChild(removeButton);
  container.appendChild(divWrapper);

  // Focus on the new input for better UX
  setTimeout(() => toponymyInput.focus(), 10);
}
(function initRemoveToponymyButtonHandler(): void {
  const container = document.getElementById('toponymyContainer');
  if (!container) {
    return;
  }

  container.addEventListener('click', function (e: Event) {
    const target = e.target as HTMLElement;
    if (target && target.classList.contains('removeToponmy')) {
      const wrapperDiv = target.closest('.inputDivToponymy') as HTMLDivElement;
      if (wrapperDiv) {
        wrapperDiv.remove();
        reindexToponymyFields(container as HTMLDivElement);
      }
    }
  });
})();
function reindexToponymyFields(container: HTMLDivElement): void {
  const entries = Array.from(container.querySelectorAll(".inputDivToponymy")) as HTMLDivElement[];

  entries.forEach((entry: HTMLDivElement, index: number) => {
    const toponymyInput = entry.querySelector(".inputToponymy") as HTMLInputElement;
    if (toponymyInput) {
      toponymyInput.name = `ToponymyList[${index}].Name`;
    }

    const currentNameCheckbox = entry.querySelector(".checkboxCurrentName") as HTMLInputElement;
    if (currentNameCheckbox) {
      currentNameCheckbox.name = `ToponymyList[${index}].IsCurrentName`;
      currentNameCheckbox.id = `ToponymyList_${index}__IsCurrentName`;

      // Update associated label
      const label = entry.querySelector(`label[for="${currentNameCheckbox.id}"]`) as HTMLLabelElement;
      if (label) {
        label.htmlFor = `ToponymyList_${index}__IsCurrentName`;
      }
    }
  });
}

// Export für Module oder globale Verwendung
if (typeof window !== 'undefined') {
  window.addToponymy = addToponymy;
}