// wwwroot/js/login.ts
import { startAuthentication } from '@simplewebauthn/browser';
import { checkWebAuthnSupport, showError, showLoading, showSuccess, getTranslation } from '../shared';

let loginInProgress = false;

// Main Login Process
// async function verifyProcess(username?: string): Promise<boolean> {
//   if (loginInProgress) {
//     showError('loginStatus', getTranslation('Error_WaitingResponse'));
//     return false;
//   }

//   loginInProgress = true;
//   const btnLogin = document.getElementById('btnLogin') as HTMLButtonElement;

//   try {
//     if (btnLogin) btnLogin.disabled = true;
//     showLoading('loginStatus', getTranslation('Preparing'));

//     // 1. Get assertion options
//     const optionsRes = await fetch('/Passkey/MakeAssertionOptions', {
//       method: 'POST',
//       headers: { 'Content-Type': 'application/json' },
//       body: JSON.stringify({ username: username || null })
//     });

//     if (!optionsRes.ok) {
//       const error = await optionsRes.json();
//       throw new Error(error.error || getTranslation('Error_AssertionOptions_Ocurred'));
//     }

//     const response = await optionsRes.json();

//     if (!response.success) {
//       throw new Error(response.error || getTranslation('Error_NoPasskeysFound'));
//     }

//     showLoading('loginStatus', getTranslation('Login_PleaseAuthenticate'));

//     // 2. Start authentication
//     const assertion = await startAuthentication({ optionsJSON: response.options });

//     showLoading('loginStatus', getTranslation('Login_Verifizing'));

//     // 3. Verify assertion
//     const verifyRes = await fetch('/Passkey/VerifyAssertion', {
//       method: 'POST',
//       headers: { 'Content-Type': 'application/json' },
//       body: JSON.stringify({
//         assertion: assertion,
//         sessionKey: response.sessionKey
//       })
//     });

//     if (!verifyRes.ok) {
//       const error = await verifyRes.json();
//       throw new Error(error.error || getTranslation('Error_VerifyAssertion_Ocurred'));
//     }

//     showSuccess('loginStatus', getTranslation('Login_Success'));

//     loginInProgress = false;
//     if (btnLogin) btnLogin.disabled = false;

//     return true;

//   } catch (error: any) {
//     showError('loginStatus', error.message || getTranslation('Error_Unknown'));
//     loginInProgress = false;
//     if (btnLogin) btnLogin.disabled = false;
//     return false;
//   }
// }

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

    if (!optionsRes.ok) {
      let errorMessage = getTranslation('Error_AssertionOptions_Ocurred');

      try {
        const error = await optionsRes.json();

        if (optionsRes.status === 404) {
          errorMessage = getTranslation('Error_User_NotFound');
        } else if (error.error && typeof error.error === 'string') {
          errorMessage = error.error;
        } else if (error.message && typeof error.message === 'string') {
          errorMessage = error.message;
        }
      } catch (parseError) {
        errorMessage = `${getTranslation('Error_AssertionOptions_Ocurred')} (Status: ${optionsRes.status})`;
      }

      throw new Error(errorMessage);
    }

    const response = await optionsRes.json();

    if (!response.success) {
      throw new Error(response.error || getTranslation('Error_Passkeys_NotFound'));
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
      let errorMessage = getTranslation('Error_VerifyAssertion_Ocurred');

      try {
        const error = await verifyRes.json();

        // Spezifische Fehlermeldungen je nach Statuscode
        if (verifyRes.status === 401) {
          errorMessage = getTranslation('Error_Authentication_Failed');
        } else if (verifyRes.status === 400) {
          const errorText = (error.error || error.message || '').toLowerCase();

          if (errorText.includes('expired') || errorText.includes('session')) {
            errorMessage = getTranslation('Error_Session_Expired');
          } else if (errorText.includes('credential') && errorText.includes('not found')) {
            errorMessage = getTranslation('Error_Credential_NotFound');
          } else if (errorText.includes('user') && errorText.includes('not found')) {
            errorMessage = getTranslation('Error_User_NotFound');
          } else if (typeof error.error === 'string') {
            errorMessage = error.error;
          } else {
            errorMessage = getTranslation('Error_VerifyAssertion_Ocurred');
          }
        } else if (verifyRes.status === 500) {
          errorMessage = getTranslation('Error_Server_Error');
        }

        console.error('Verify assertion error:', { status: verifyRes.status, error });

      } catch (parseError) {
        errorMessage = `${getTranslation('Error_VerifyAssertion_Ocurred')} (Status: ${verifyRes.status})`;
      }

      throw new Error(errorMessage);
    }

    showSuccess('loginStatus', getTranslation('Login_Success'));
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

// Login with username
async function login(): Promise<void> {
  const usernameInput = document.getElementById('Username') as HTMLInputElement;
  const username = usernameInput ? usernameInput.value.trim() : null;

  if (!username) {
    showError('loginStatus', getTranslation('Error_UserName_Required'));
    return;
  }

  const success = await verifyProcess(username);

  if (success) {
    setTimeout(() => {
      window.location.href = '/CollectionAreaDatabase/Index';
    }, 1500);
  }
}

// Login without username (conditional UI)
async function loginWithoutUsername(): Promise<void> {
  const usernameInput = document.getElementById('Username') as HTMLInputElement;
  if (usernameInput) {
    usernameInput.value = '';
  }

  const success = await verifyProcess();

  if (success) {
    setTimeout(() => {
      window.location.href = '/CollectionAreaDatabase/Index';
    }, 1500);
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
  // WebAuthn Support prüfen
  await checkWebAuthnSupport();

  // Event listeners
  const btnLogin = document.getElementById('btnLogin');
  const btnSearch = document.getElementById('btnSearchPasskeys');
  const usernameInput = document.getElementById('Username') as HTMLInputElement;

  if (btnLogin) {
    btnLogin.addEventListener('click', login);
  }

  if (btnSearch) {
    btnSearch.addEventListener('click', loginWithoutUsername);
  }

  if (usernameInput) {
    usernameInput.addEventListener('keypress', (e: KeyboardEvent) => {
      if (e.key === 'Enter') {
        e.preventDefault();
        login();
      }
    });
    setTimeout(() => usernameInput.focus(), 100);
  }
}

// Export für globale Verwendung
(window as any).login = login;
(window as any).loginWithoutUsername = loginWithoutUsername;
(window as any).verifyForDeletePersonalDataSubmit = verifyForDeletePersonalDataSubmit;

// Starte Login-Initialisierung (shared.ts kümmert sich um i18n)
initializeLoginPage();