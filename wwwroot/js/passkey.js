"use strict";
class PasskeyManager {
    static isWebAuthnSupported() {
        return !!window.PublicKeyCredential &&
            typeof PublicKeyCredential.isUserVerifyingPlatformAuthenticatorAvailable === 'function';
    }
    static async checkSupport() {
        if (!this.isWebAuthnSupported()) {
            console.error('WebAuthn wird nicht unterstützt');
            return false;
        }
        try {
            const isPlatformAuthenticatorAvailable = await PublicKeyCredential
                .isUserVerifyingPlatformAuthenticatorAvailable();
            const isConditionalMediationAvailable = await PublicKeyCredential
                .isConditionalMediationAvailable?.() ?? false;
            return isPlatformAuthenticatorAvailable || isConditionalMediationAvailable;
        }
        catch (error) {
            console.error('Fehler beim Prüfen der WebAuthn-Unterstützung:', error);
            return false;
        }
    }
    static async register() {
        if (!await this.checkSupport()) {
            alert('Passkeys werden in Ihrem Browser nicht unterstützt.');
            return;
        }
        try {
            const request = {
                authenticatorSelection: {
                    authenticatorAttachment: 'platform',
                    residentKey: 'required',
                    userVerification: 'required'
                },
                attestationConveyancePreference: 'none',
                extensions: {
                    credProps: true,
                    sessionKey: true
                }
            };
            const response = await fetch('/Passkey/MakeCredentialOptions', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(request)
            });
            if (!response.ok)
                throw new Error('Options request failed');
            const data = await response.json();
            const options = data.options;
            // Session Key zu Extensions hinzufügen
            options.extensions = options.extensions || {};
            options.extensions.sessionKey = data.sessionKey;
            const credential = await navigator.credentials.create({
                publicKey: options
            });
            await this.submitCredential(credential);
        }
        catch (error) {
            console.error('Registrierung fehlgeschlagen:', error);
            alert('Registrierung fehlgeschlagen. Bitte versuchen Sie es erneut.');
        }
    }
    static async submitCredential(credential) {
        const attestationResponse = credential.response;
        const data = {
            id: credential.id,
            rawId: this.arrayBufferToBase64(credential.rawId),
            type: credential.type,
            clientExtensionResults: credential.getClientExtensionResults(),
            response: {
                attestationObject: this.arrayBufferToBase64(attestationResponse.attestationObject),
                clientDataJSON: this.arrayBufferToBase64(attestationResponse.clientDataJSON)
            }
        };
        const response = await fetch('/Passkey/MakeCredential', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });
        if (response.ok) {
            window.location.href = '/Passkey/Success';
        }
        else {
            throw new Error('Credential submission failed');
        }
    }
    static async login(username) {
        if (!await this.checkSupport()) {
            alert('Passkey-Anmeldung wird nicht unterstützt.');
            return;
        }
        try {
            const request = {
                username: username,
                extensions: {
                    sessionKey: true
                }
            };
            const response = await fetch('/Passkey/MakeAssertionOptions', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(request)
            });
            if (!response.ok)
                throw new Error('Options request failed');
            const data = await response.json();
            const options = data.options;
            // Session Key zu Extensions hinzufügen
            options.extensions = options.extensions || {};
            options.extensions.sessionKey = data.sessionKey;
            const credential = await navigator.credentials.get({
                publicKey: options,
                mediation: 'conditional'
            });
            await this.submitAssertion(credential);
        }
        catch (error) {
            console.error('Anmeldung fehlgeschlagen:', error);
            alert('Anmeldung fehlgeschlagen. Bitte versuchen Sie es erneut.');
        }
    }
    static async submitAssertion(credential) {
        const assertionResponse = credential.response;
        const data = {
            id: credential.id,
            rawId: this.arrayBufferToBase64(credential.rawId),
            type: credential.type,
            clientExtensionResults: credential.getClientExtensionResults(),
            response: {
                authenticatorData: this.arrayBufferToBase64(assertionResponse.authenticatorData),
                clientDataJSON: this.arrayBufferToBase64(assertionResponse.clientDataJSON),
                signature: this.arrayBufferToBase64(assertionResponse.signature),
                userHandle: assertionResponse.userHandle ?
                    this.arrayBufferToBase64(assertionResponse.userHandle) : null
            }
        };
        const response = await fetch('/Passkey/MakeAssertion', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });
        if (response.ok) {
            const result = await response.json();
            if (result.redirectUrl) {
                window.location.href = result.redirectUrl;
            }
        }
        else {
            throw new Error('Assertion submission failed');
        }
    }
    static async deleteCredential(credentialId) {
        try {
            const response = await fetch('/Passkey/DeleteCredential', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    credentialId: this.base64ToUint8Array(credentialId)
                })
            });
            return response.ok;
        }
        catch (error) {
            console.error('Löschen fehlgeschlagen:', error);
            return false;
        }
    }
    static arrayBufferToBase64(buffer) {
        const bytes = new Uint8Array(buffer);
        let binary = '';
        for (let i = 0; i < bytes.byteLength; i++) {
            binary += String.fromCharCode(bytes[i]);
        }
        return btoa(binary);
    }
    static base64ToUint8Array(base64) {
        const binary = atob(base64);
        const bytes = new Uint8Array(binary.length);
        for (let i = 0; i < binary.length; i++) {
            bytes[i] = binary.charCodeAt(i);
        }
        return bytes;
    }
}
//type AttestationConveyancePreference = 'none' | 'indirect' | 'direct' | 'enterprise';
// Auto-login auf Login-Seite
if (window.location.pathname.includes('/Passkey/Login')) {
    PasskeyManager.checkSupport().then(supported => {
        if (supported) {
            const loginButton = document.getElementById('passkey-login-btn');
            if (loginButton) {
                loginButton.style.display = 'block';
            }
        }
    });
}
// Conditional UI bei Passwort-Feldern
document.addEventListener('DOMContentLoaded', () => {
    const passwordInputs = document.querySelectorAll('input[type="password"]');
    passwordInputs.forEach(input => {
        input.addEventListener('focus', async () => {
            if (await PasskeyManager.checkSupport()) {
                PasskeyManager.login();
            }
        });
    });
});
window.PasskeyManager = PasskeyManager;
