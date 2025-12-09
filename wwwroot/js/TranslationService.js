class TranslationService {
    constructor() {
        this.translations = {};
        this.currentCulture = this.getCurrentCulture();
        this.storageKey = `translations_${this.currentCulture}`;
        this.cacheDuration = 24 * 60 * 60 * 1000; // 24 Stunden
    }

    getCurrentCulture() {
        // Kultur vom Meta-Tag oder HTML lang-Attribut
        const metaCulture = document.querySelector('meta[name="culture"]')?.content;
        const htmlLang = document.documentElement.lang;
        return metaCulture || htmlLang || 'de-DE';
    }

    // Local Storage mit Verfallsdatum
    cacheTranslations() {
        const cacheData = {
            data: this.translations,
            timestamp: Date.now(),
            culture: this.currentCulture
        };
        localStorage.setItem(this.storageKey, JSON.stringify(cacheData));
    }

    getCachedTranslations() {
        try {
            const cached = localStorage.getItem(this.storageKey);
            if (!cached) return null;

            const cacheData = JSON.parse(cached);

            // Prüfen ob Cache abgelaufen oder falsche Kultur
            const isExpired = Date.now() - cacheData.timestamp > this.cacheDuration;
            const isWrongCulture = cacheData.culture !== this.currentCulture;

            if (isExpired || isWrongCulture) {
                localStorage.removeItem(this.storageKey);
                return null;
            }

            return cacheData.data;
        } catch (error) {
            console.warn('Failed to parse cached translations:', error);
            localStorage.removeItem(this.storageKey);
            return null;
        }
    }

    async loadTranslations() {
        // 1. Aus Cache laden falls verfügbar
        const cached = this.getCachedTranslations();
        if (cached) {
            this.translations = cached;
            console.log('Translations loaded from cache');
            return;
        }

        try {
            const response = await fetch(`/api/TranslationsJs?culture=${this.currentCulture}`);
            if (!response.ok) {
                throw new Error(`HTTP ${response.status}`);
            }

            this.translations = await response.json();
            this.cacheTranslations();
            console.log('Translations loaded from API and cached');
        } catch (error) {
            console.error('Failed to load translations:', error);
            // Fallback: Leeres Dictionary
            this.translations = {};
        }
    }

    get(key, defaultValue = null) {
        return this.translations[key] || defaultValue || key;
    }

    format(key, ...args) {
        let translation = this.get(key);
        return translation.replace(/{(\d+)}/g, (match, index) => {
            return typeof args[index] !== 'undefined' ? args[index] : match;
        });
    }

    // Cache explizit löschen (z.B. bei Sprachwechsel)
    clearCache() {
        localStorage.removeItem(this.storageKey);
        console.log('Translation cache cleared');
    }

    // Cache für alle Sprachen löschen
    clearAllCaches() {
        Object.keys(localStorage).forEach(key => {
            if (key.startsWith('translations_')) {
                localStorage.removeItem(key);
            }
        });
        console.log('All translation caches cleared');
    }
}

// Globale Instanz
const i18n = new TranslationService();