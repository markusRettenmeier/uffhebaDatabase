export interface TdOptions {
    text?: string;
    id?: string | null;
    scope?: string | null;
}

export interface SearchResultBase {
    id: number;
    name?: string;
    furtherSpecs?: string;
    [key: string]: any;
}

export interface PlaceResult extends SearchResultBase {
    placeID: number;
    oeconomymDisplay?: string;
}

export interface PartyResult extends SearchResultBase {
    partyID: number;
    type?: string;
}

export interface ConceptResult extends SearchResultBase {
    conceptID: number;
    conceptName?: string;
}

export interface EraResult extends SearchResultBase {
    eraID: number;
    eraName?: string;
}

export interface SetResult extends SearchResultBase {
    setId: number;
    setName?: string;
}

export interface CollectionArea {
    id: number;
    name: string;
    [key: string]: any;
}

export interface Industry {
    id: number;
    name: string;
    [key: string]: any;
}

export interface VoteRequest {
    topicId: string;
    voteType: string;
}

export interface ApiResponse<T = any> {
    success: boolean;
    data?: T;
    message?: string;
}

// Ergänzung zu types.ts
export interface ColumnDefinition extends Array<string | string> {
    0: string;  // column name
    1: 'text' | 'number' | 'year' | 'date' | 'checkbox' | 'multiselect';  // column type
}

export interface EraElement {
    eraID: string;
    eraName: string;
}

export interface SetElement {
    setId: string;
    setName: string;
}

export interface ConceptSearchResult {
    conceptIdValue: string;
    nameValue: string;
}

interface NetworkNode {
    id: number;
    label: string;
}

interface NetworkEdge {
    from: number;
    to: number;
    label?: string;
}

export interface ConceptualRelationshipResponse {
    nodes: NetworkNode[];
    edges: NetworkEdge[];
}

export interface IndustryItem {
    id: number;
    name: string;
}