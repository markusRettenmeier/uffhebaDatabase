export interface TranslationCache {
    data: Record<string, string>;
    timestamp: number;
    culture: string;
}
export interface Translations {
    [key: string]: string;
}
export interface LanguageMap {
    [code: string]: string;
}
export declare class TranslationService {
    #private;
    constructor();
    loadTranslations(): Promise<void>;
    get(key: string, defaultValue?: string | null): string;
    format(key: string, ...args: (string | number)[]): string;
    clearCache(): void;
    clearAllCaches(): void;
    /**
     * Wechselt die Sprache und lädt neue Übersetzungen
     */
    changeCulture(newCulture: string): Promise<void>;
    /**
     * Gibt alle verfügbaren Übersetzungen zurück
     */
    getAll(): Translations;
    /**
     * Prüft ob ein Key existiert
     */
    has(key: string): boolean;
    /**
     * Gibt die aktuelle Kultur zurück
     */
    getCurrentCultureCode(): string;
    /**
     * Gibt den Sprachnamen zurück
     */
    getLanguageName(cultureCode?: string): string;
    /**
     * Fügt eine neue Übersetzung hinzu (für dynamische Inhalte)
     */
    addTranslation(key: string, value: string): void;
    /**
     * Fügt mehrere Übersetzungen hinzu
     */
    addTranslations(translations: Translations): void;
    /**
     * Registriert einen Event-Listener für Sprachwechsel
     */
    onCultureChanged(callback: (culture: string, languageName: string) => void): void;
    /**
     * Gibt die Basis-Sprache zurück (z.B. "de" aus "de-DE")
     */
    getBaseLanguage(): string;
    /**
     * Prüft ob die aktuelle Sprache RTL ist
     */
    isRTL(): boolean;
    /**
     * Gibt die Textausrichtung basierend auf der Sprache zurück
     */
    getTextDirection(): 'ltr' | 'rtl';
    /**
     * Neues ES2024 Feature: Promise.withResolvers()
     * Erstellt ein Promise mit externer Kontrolle
     */
    loadWithTimeout(timeoutMs?: number): Promise<boolean>;
}
export declare const i18n: TranslationService;
export default i18n;
//# sourceMappingURL=TranslationService.d.ts.map