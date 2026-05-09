import type { ColumnDefinition } from '../types';

export const columns: ColumnDefinition[] = [
  ["PersonalIdentificationNumber", "text"],
  ["SerialNumber", "text"],
  ["FilingLocation", "text"],
  ["CollectionItemEntityID", "number"],
  ["UniqueName", "text"],

  ["CollectionAreaName", "text"],

  ["PlaceNToponymyList_Toponymy_ToponymyName", "text"],
  ["FurtherSpecs", "text"],

  ["ParticipantName", "text"],
  ["Individual_Pseudonym", "text"],
  ["Individual_Signature", "text"],
  ["Organization_Industry_IndustryName", "text"],
  ["Organization_PlaceList_PlaceNToponymyList_Toponymy_ToponymyName", "text"],

  ["ConceptName", "text"],

  ["CollectionAreaName", "text"],

  ["EraName", "text"],

  ["CollectionItemRelationshipName", "text"],
];