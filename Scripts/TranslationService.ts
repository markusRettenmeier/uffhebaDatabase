interface TranslationCache {
    data: Record<string, string>;
    timestamp: number;
    culture: string;
}

export class TranslationService {
    #translations: Record<string, string> = {};
    #currentCulture: string;
    #storageKey: string;
    readonly #cacheDuration = 24 * 60 * 60 * 1000;

    #languageNames: Record<string, string> = {
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

    #getCurrentCulture(): string {
        const metaCulture = document.querySelector<HTMLMetaElement>('meta[name="culture"]')?.content;
        const htmlLang = document.documentElement.lang;
        const browserLanguage = navigator.language;

        return metaCulture ?? htmlLang ?? browserLanguage ?? 'de-DE';
    }

    #cacheTranslations(): void {
        const cacheData: TranslationCache = {
            data: this.#translations,
            timestamp: Date.now(),
            culture: this.#currentCulture
        };
        localStorage.setItem(this.#storageKey, JSON.stringify(cacheData));
    }

    #getCachedTranslations(): Record<string, string> | null {
        try {
            const cached = localStorage.getItem(this.#storageKey);
            if (!cached) return null;

            const cacheData = JSON.parse(cached) as TranslationCache;
            const isExpired = Date.now() - cacheData.timestamp > this.#cacheDuration;
            const isWrongCulture = cacheData.culture !== this.#currentCulture;

            if (isExpired || isWrongCulture) {
                localStorage.removeItem(this.#storageKey);
                return null;
            }

            return cacheData.data;
        } catch {
            localStorage.removeItem(this.#storageKey);
            return null;
        }
    }

    async loadTranslations(): Promise<void> {
        const cached = this.#getCachedTranslations();
        if (cached) {
            this.#translations = cached;
            return;
        }

        try {
            const response = await fetch(`/api/TranslationsJs?culture=${encodeURIComponent(this.#currentCulture)}`);
            if (!response.ok) throw new Error(`HTTP ${response.status}`);

            const data = await response.json() as Record<string, string>;
            this.#translations = data;
            this.#cacheTranslations();
        } catch {
            this.#translations = {};
        }
    }

    get(key: string, defaultValue: string | null = null): string {
        return this.#translations[key] ?? defaultValue ?? key;
    }

    format(key: string, ...args: (string | number)[]): string {
        let translation = this.get(key);
        if (args.length === 0) return translation;

        return translation.replaceAll(/{(\d+)}/g, (match: string, index: string): string => {
            const argIndex = Number.parseInt(index, 10);
            return args[argIndex]?.toString() ?? match;
        });
    }

    clearCache(): void {
        localStorage.removeItem(this.#storageKey);
    }

    clearAllCaches(): void {
        const keys = Array.from({ length: localStorage.length }, (_, i) => localStorage.key(i));
        const translationKeys = keys.filter((key): key is string =>
            key?.startsWith('translations_') ?? false
        );

        translationKeys.forEach(key => localStorage.removeItem(key));
    }
}

// Globale Instanz
export const i18n = new TranslationService();

// Global verfügbar machen
if (typeof window !== 'undefined') {
    (window as any).i18n = i18n;
}