import { startAuthentication } from '@simplewebauthn/browser';


async function verifyProcess() {
    const optionsRes = await fetch('/passkey/MakeAssertionOptions', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
    });
    if (!optionsRes.ok) {
        throw new Error('Failed to get assertion options');
    }

    const response = await optionsRes.json();
    const assertion = await startAuthentication(response.options);

    const verifyRes = await fetch('/passkey/VerifyAssertion', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            assertion,
            sessionKey: response.sessionKey
        })
    });
    if (!verifyRes.ok) {
        throw new Error('Login failed');
    }
}

async function login() {
    await verifyProcess();

    window.location.href = '/collectionItemDatabase/Index';
}
window.login = login;

async function verifyForDeletePersonalDataSubmit() {
    await verifyProcess();

    window.location.href = '/userSettings/DeletePersonalDataSubmit';
}
window.verifyForDeletePersonalDataSubmit = verifyForDeletePersonalDataSubmit;