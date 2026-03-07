// Externe Bibliotheken
declare const bootstrap: {
  Modal: {
    getInstance(element: HTMLElement): { hide(): void } | null;
    new(element: HTMLElement): { hide(): void };
  };
};

declare const html2pdf: () => {
  set(options: { filename: string }): any;
  from(element: HTMLElement): any;
  output(type: string): void;
};

declare const Fancybox: {
  bind(selector: string, options: any): void;
};

declare const vis: {
  DataSet: new (data: any[]) => any;
  Network: new (container: HTMLElement, data: { nodes: any; edges: any }, options: any) => any;
};

// i18n wird durch TranslationService.ts definiert
declare const i18n: {
  loadTranslations(): Promise<void>;
  get(key: string, defaultValue?: string | null): string;
  format(key: string, ...args: any[]): string;
};

type ConceptRelation = 'synonym' | 'subterm';
type PlaceRelationType = "Bezug" | "child";
type SourcePage = "Edit" | "Create";
// Globale Funktionen
interface Window {
  addColumn(): void;
  addField(idParent: string): void;
  searchReset(): void;
  setSessionStorageData(): void;
  changingView(): void;
  getMoreData(): void;
  addPlace(idx: number): void;
  addParty(idx: number): void;
  addConcept(idx: number, type?: string): void;
  ActivateDeleteButton(): void;
  addConceptConceptualRelationship: (idx: number, relation: ConceptRelation) => void;
  addConceptCollectionRelation: (idx: number) => void;
  initRemoveConceptRelationButtonHandler: () => void;
  addRelatedPlace: (idx: number) => void;
  addToponymy: () => void;
  initializePlaceAndToponymyHandlers: () => void;
  addConceptToCollectionItem(buttonId: number): void;
  SetEraIntoTable(buttonId: number): void;
  addFormFileCollectionItem(sourcePage: SourcePage): void;
  addFormFileOwnershipProof(sourcePage: SourcePage): void;
  removePicture(pictureCount: number): void;
  getIndustryList(): Promise<string>;
  register: () => Promise<void>;
  login: () => Promise<void>;
  verifyForDeletePersonalDataSubmit: () => Promise<void>;
  checkWebAuthnSupport: () => Promise<void>;
  openNav: () => void;
  closeNav: () => void;
  autocomplete: (input: HTMLInputElement, dataList: string[]) => void;
}