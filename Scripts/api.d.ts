interface TdOptions {
    text?: string;
    id?: string | null;
    scope?: string | null;
}
interface PlaceResult {
    placeID: number;
    toponymyDisplay?: string;
    furtherSpecs?: string;
    [key: string]: any;
}
interface PartyResult {
    partyID: number;
    name?: string;
    type?: string;
    furtherSpecs?: string;
    [key: string]: any;
}
interface ConceptResult {
    conceptID: number;
    conceptName?: string;
    furtherSpecs?: string;
    [key: string]: any;
}
interface VoteData {
    topicId: string;
    voteType: string;
}
declare const createTd: ({ text, id, scope }: TdOptions) => HTMLTableCellElement;
declare const togglePlace: Element | null;
declare function buildToponymySearchResultRow(element: PlaceResult, idx: number): HTMLTableRowElement;
declare function addPlace(idx: number): void;
declare const toggleParty: Element | null;
declare function buildPartySearchResultRow(element: PartyResult, idx: number): HTMLTableRowElement;
declare function addParty(idx: number): void;
declare function sendErrorMessage(error: Error | string): void;
declare const conceptToggle: Element | null;
declare function buildConceptSearchResultRow(element: ConceptResult, idx: number): HTMLTableRowElement;
declare function addConcept(idx: number, type?: string): void;
declare function getCollectionAreaList(): Promise<string | null>;
declare function getProductionFacilityList(): Promise<string | null>;
//# sourceMappingURL=api.d.ts.map