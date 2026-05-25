// wwwroot/js/register.ts
import { startRegistration } from '@simplewebauthn/browser';
import { checkWebAuthnSupport, getTranslation } from '../shared';

function isValidEmail(email: string): boolean {
  const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return emailPattern.test(email);
}

function showRegisterStatus(html: string): void {
  const statusDiv = document.getElementById('registerStatus') as HTMLDivElement;
  if (statusDiv) statusDiv.innerHTML = html;
}

function showRegisterError(message: string): void {
  showRegisterStatus(`
        <div class="alert alert-danger alert-dismissible fade show" role="alert" aria-live="assertive">
            <i class="bi bi-exclamation-triangle me-2"></i>
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="${getTranslation('Close')}"></button>
        </div>
    `);

  const btnRegister = document.getElementById('btnRegister') as HTMLButtonElement;
  btnRegister.disabled = false;
  btnRegister.innerHTML = `<i class="bi bi-shield-check me-2"></i>${getTranslation('Register')}`;
}

function showRegisterSuccess(message: string): void {
  showRegisterStatus(`
        <div class="alert alert-success">
            <i class="bi bi-check-circle me-2"></i>
            ${message}
        </div>
    `);
}

function validateForm(displayName: string, email: string): boolean {
  showRegisterStatus('');

  if (!displayName) {
    showRegisterError(getTranslation('Error_DisplayName_Required'));
    return false;
  }
  if (displayName.length < 2) {
    showRegisterError(getTranslation('Error_DisplayName_StringLength'));
    return false;
  }
  if (email && !isValidEmail(email)) {
    showRegisterError(getTranslation('Error_Email_Invalid'));
    return false;
  }

  return true;
}

function downloadUsernameFile(username: string): void {
  const content = getTranslation('Register_Username_File_Content', username);
  const blob = new Blob([content], { type: 'text/plain' });
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = `passkey_username_${username.substring(0, 8)}.txt`;
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
  URL.revokeObjectURL(url);
}

async function register(): Promise<void> {
  const displayNameInput = document.getElementById('DisplayName') as HTMLInputElement;
  const displayName = displayNameInput.value.trim();
  const emailInput = document.getElementById('Email') as HTMLInputElement;
  const email = emailInput.value.trim();

  if (!validateForm(displayName, email)) {
    return;
  }

  const btnRegister = document.getElementById('btnRegister') as HTMLButtonElement;
  try {
    btnRegister.disabled = true;
    btnRegister.innerHTML = `<span class="spinner-border spinner-border-sm me-2" role="status" aria-live="polite"></span>${getTranslation('Preparing')}`;

    const startResponse = await fetch('/Passkey/StartRegistration', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ displayName, email })
    });

    const startResult = await startResponse.json();

    if (!startResponse.ok) {
      showRegisterError(startResult.error || getTranslation('Error_Server_Error'));
      return;
    }
    console.log('Server options received:', startResult.options);

    // Prüfe auf fehlende Challenge
    if (!startResult.options?.challenge) {
      console.error('Missing challenge in server response!');
      showRegisterError(getTranslation('Error_PasskeyChallenge_Required'));
      return;
    }
    // Prüfe auf user.id (muss existieren)
    if (!startResult.options?.user?.id) {
      console.error('Missing user.id in server response!');
      showRegisterError(getTranslation('Error_PasskeyUserId_Missing'));
      return;
    }
    console.log('StartRegistration Response:', startResult);
    console.log('Session Key:', startResult.sessionKey);
    console.log('Options:', startResult.options);

    btnRegister.innerHTML = `<span class="spinner-border spinner-border-sm me-2" role="status" aria-live="polite"></span>${getTranslation('Register_Completing')}`;

    let attestationResponse;
    try {
      attestationResponse = await startRegistration({ optionsJSON: startResult.options });
    } catch (e) {
      showRegisterError(getTranslation('Error_Register_Aborted'));
      console.error('Error object:', e);
      return;
    }

    downloadUsernameFile(startResult.userName);

    const completeResponse = await fetch('/Passkey/MakeCredential', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        sessionKey: startResult.sessionKey,
        attestationResponse: attestationResponse
      })
    });

    const completeResult = await completeResponse.json();
    //if (!completeResponse.ok) {
    //  showRegisterError(completeResult.error || getTranslation('Error_Server_Error'));
    //  return;
    //}
    console.log('Complete Response Status:', completeResponse.status);
    console.log('Complete Response OK:', completeResponse.ok);
    console.log('Complete Result:', completeResult);

    if (!completeResponse.ok) {
      // Zeige spezifischen Server-Fehler an
      const errorMessage = completeResult.error ||
        completeResult.title ||
        completeResult.detail ||
        getTranslation('Error_Server_Error');
      console.error('Server error details:', completeResult);
      showRegisterError(errorMessage);
      return;
    }

    showRegisterSuccess(`${getTranslation('Success_Passkey_Registered')} ${getTranslation('Register_Redirect')}`);
    window.location.href = '/collectionAreaDatabase/Index';
  } catch (err) {
    const error = err instanceof Error ? err : new Error(String(err));
    showRegisterError(error.message);
  }
}

// Register-spezifische Initialisierung
async function initializeRegisterPage(): Promise<void> {
  await checkWebAuthnSupport();

  const btnRegister = document.getElementById('btnRegister');
  if (btnRegister) {
    btnRegister.addEventListener('click', register);
  }

  // Enter-Taste Support
  const displayNameInput = document.getElementById('DisplayName') as HTMLInputElement;
  if (displayNameInput) {
    displayNameInput.addEventListener('keypress', (e: KeyboardEvent) => {
      if (e.key === 'Enter') {
        e.preventDefault();
        register();
      }
    });
  }
}

// Globale Exporte
window.register = register;
window.checkWebAuthnSupport = checkWebAuthnSupport;

// Starte Register-Initialisierung
initializeRegisterPage();