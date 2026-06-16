import { startAuthentication } from '@simplewebauthn/browser';
import { checkWebAuthnSupport, showError, showLoading, showSuccess, getTranslation } from '../shared';

let loginInProgress = false;

// Login without username (conditional UI)
async function loginWithoutUsername(): Promise<void> {
  const success = await verifyProcess();

  if (success) {
    setTimeout(() => {
      window.location.href = '/CollectionAreaDatabase/Index';
    }, 1500);
  }
}

async function verifyProcess(username?: string): Promise<boolean> {
  if (loginInProgress) {
    showError('loginStatus', getTranslation('Error_WaitingResponse'));
    return false;
  }

  loginInProgress = true;
  const btnLogin = document.getElementById('btnLogin') as HTMLButtonElement;

  try {
    if (btnLogin) btnLogin.disabled = true;
    showLoading('loginStatus', getTranslation('Preparing'));

    // 1. Get assertion options
    const optionsRes = await fetch('/Passkey/MakeAssertionOptions', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username: username || null })
    });
    const response = await optionsRes.json();
    if (!optionsRes.ok) {
      let errorMessage = getTranslation('Error_Unknown');
      showError('loginStatus', errorMessage);
    }

    showLoading('loginStatus', getTranslation('Login_PleaseAuthenticate'));

    // 2. Start authentication
    const assertion = await startAuthentication({ optionsJSON: response.options });

    showLoading('loginStatus', getTranslation('Login_Verifizing'));

    // 3. Verify assertion
    const verifyRes = await fetch('/Passkey/VerifyAssertion', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        assertion: assertion,
        sessionKey: response.sessionKey
      })
    });

    if (!verifyRes.ok) {
      let errorMessage = getTranslation('Error_Unknown');
      throw new Error(errorMessage);
    }

    showSuccess('loginStatus', getTranslation('Success_Login'));
    loginInProgress = false;
    if (btnLogin) btnLogin.disabled = false;
    return true;
  } catch (error: any) {
    let userMessage = '';
    const errorMessage = typeof error.message === 'object'
      ? JSON.stringify(error.message)
      : (error.message || '');

    // Erkennung basierend auf Fehlertypen
    if (error.name === 'NotAllowedError') {
      userMessage = getTranslation('Error_Authentication_Cancelled');
    } else if (error.name === 'NotSupportedError') {
      userMessage = getTranslation('Error_WebAuthn_NotSupported');
    } else if (error.name === 'SecurityError') {
      userMessage = getTranslation('Error_Security_HTTPS_Required');
    } else if (error.name === 'TimeoutError' || errorMessage.toLowerCase().includes('timeout')) {
      userMessage = getTranslation('Error_Timeout');
    } else if (errorMessage.toLowerCase().includes('network') || errorMessage.toLowerCase().includes('fetch')) {
      userMessage = getTranslation('Error_Network');
    } else if (errorMessage && errorMessage !== '[object Object]') {
      userMessage = errorMessage;
    } else {
      userMessage = getTranslation('Error_Unknown');
    }

    showError('loginStatus', userMessage);
    loginInProgress = false;
    if (btnLogin) btnLogin.disabled = false;
    return false;
  }
}

// For Delete Personal Data verification
async function verifyForDeletePersonalDataSubmit(): Promise<void> {
  const success = await verifyProcess();

  if (success) {
    setTimeout(() => {
      window.location.href = '/userSettings/DeletePersonalDataSubmit';
    }, 1500);
  }
}

// Login-spezifische Initialisierung
async function initializeLoginPage(): Promise<void> {
  await checkWebAuthnSupport();

  const btnSearch = document.getElementById('btnSearchPasskeys') as HTMLButtonElement;
  if (btnSearch) {
    btnSearch.addEventListener('click', loginWithoutUsername);
  }
}

// Export für globale Verwendung
(window as any).loginWithoutUsername = loginWithoutUsername;
(window as any).verifyForDeletePersonalDataSubmit = verifyForDeletePersonalDataSubmit;

// Starte Login-Initialisierung (shared.ts kümmert sich um i18n)
initializeLoginPage();