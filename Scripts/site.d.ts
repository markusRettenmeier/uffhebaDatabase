declare const i18n: {
    loadTranslations(): Promise<void>;
    get(key: string): string;
};
declare const bootstrap: {
    Modal: {
        getInstance(element: HTMLElement): {
            hide(): void;
        } | null;
        new (element: HTMLElement): {
            hide(): void;
        };
    };
};
declare const html2pdf: () => {
    set(options: {
        filename: string;
    }): any;
    from(element: HTMLElement): any;
    output(type: string): void;
};
interface ProductionFacility {
    id: number;
    name: string;
    [key: string]: any;
}
interface CollectionArea {
    id: number;
    name: string;
    [key: string]: any;
}
declare function handlePageLoad(): Promise<void>;
declare function openNav(): void;
declare function closeNav(): void;
declare function generatePDF(): void;
declare function handleShowPassword(): void;
declare function hideModal(modalName: string): void;
declare function setCollectionAreasIntoOptions(stored: string): void;
declare function setProductionFacilitiesIntoOptions(stored: string): void;
declare function getAndSetConceptualRelationshipGraph(): void;
declare const $: any;
declare function ActivateDeleteButton(): void;
//# sourceMappingURL=site.d.ts.map