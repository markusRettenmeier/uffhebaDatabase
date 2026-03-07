export class TranslationService {
    #translations = {};
    #currentCulture;
    #storageKey;
    #cacheDuration = 24 * 60 * 60 * 1000;
    #languageNames = {
        'de': 'Deutsch',
        'de-DE': 'Deutsch (Deutschland)',
        'de-AT': 'Deutsch (Österreich)',
        'de-CH': 'Deutsch (Schweiz)',
        'en': 'Englisch',
        'en-US': 'Englisch (USA)',
        'en-GB': 'Englisch (Großbritannien)',
    };
    constructor() {
        this.#currentCulture = this.#getCurrentCulture();
        this.#storageKey = `translations_${this.#currentCulture}`;
    }
    #getCurrentCulture() {
        const metaCulture = document.querySelector('meta[name="culture"]')?.content;
        const htmlLang = document.documentElement.lang;
        const browserLanguage = navigator.language;
        return metaCulture ?? htmlLang ?? browserLanguage ?? 'de-DE';
    }
    #cacheTranslations() {
        const cacheData = {
            data: this.#translations,
            timestamp: Date.now(),
            culture: this.#currentCulture
        };
        localStorage.setItem(this.#storageKey, JSON.stringify(cacheData));
    }
    #getCachedTranslations() {
        try {
            const cached = localStorage.getItem(this.#storageKey);
            if (!cached)
                return null;
            const cacheData = JSON.parse(cached);
            const isExpired = Date.now() - cacheData.timestamp > this.#cacheDuration;
            const isWrongCulture = cacheData.culture !== this.#currentCulture;
            if (isExpired || isWrongCulture) {
                localStorage.removeItem(this.#storageKey);
                return null;
            }
            return cacheData.data;
        }
        catch {
            localStorage.removeItem(this.#storageKey);
            return null;
        }
    }
    async loadTranslations() {
        const cached = this.#getCachedTranslations();
        if (cached) {
            this.#translations = cached;
            return;
        }
        try {
            const response = await fetch(`/api/TranslationsJs?culture=${encodeURIComponent(this.#currentCulture)}`);
            if (!response.ok)
                throw new Error(`HTTP ${response.status}`);
            const data = await response.json();
            this.#translations = data;
            this.#cacheTranslations();
        }
        catch {
            this.#translations = {};
        }
    }
    get(key, defaultValue = null) {
        return this.#translations[key] ?? defaultValue ?? key;
    }
    format(key, ...args) {
        let translation = this.get(key);
        if (args.length === 0)
            return translation;
        return translation.replaceAll(/{(\d+)}/g, (match, index) => {
            const argIndex = Number.parseInt(index, 10);
            return args[argIndex]?.toString() ?? match;
        });
    }
    clearCache() {
        localStorage.removeItem(this.#storageKey);
    }
    clearAllCaches() {
        const keys = Array.from({ length: localStorage.length }, (_, i) => localStorage.key(i));
        const translationKeys = keys.filter((key) => key?.startsWith('translations_') ?? false);
        translationKeys.forEach(key => localStorage.removeItem(key));
    }
}
// Globale Instanz
export const i18n = new TranslationService();
// Global verfügbar machen
if (typeof window !== 'undefined') {
    window.i18n = i18n;
}
