import { startRegistration } from '@simplewebauthn/browser';
import { checkWebAuthnSupport, showError, showLoading, showStatus, showSuccess, getTranslation } from '../shared';
import { setBackupCodes, generateBackupCodes, showBackupCodesModal } from './backup'

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

async function register() {
  const passkeyResult = await registerPasskey();
  if (passkeyResult === "") {
    return; // Registrierung fehlgeschlagen oder abgebrochen, daher Abbruch
  }

  const backupCodes = await generateBackupCodes(10);
  setBackupCodes(backupCodes);

  showBackupCodesModal(backupCodes, passkeyResult, {
    message: getTranslation('Success_Registration')
  });
}

async function registerPasskey(): Promise<string> {
  const displayNameInput = document.getElementById('DisplayName') as HTMLInputElement;
  const displayName = displayNameInput.value.trim();
  const emailInput = document.getElementById('Email') as HTMLInputElement;
  const email = emailInput.value.trim();

  const btnRegister = document.getElementById('btnRegister') as HTMLButtonElement;
  if (btnRegister) btnRegister.disabled = true;
  showLoading('registerStatus', getTranslation('Preparing'));

  try {
    const startResponse = await fetch('/Passkey/StartRegistration', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ displayName, email })
    });

    const startResult = await startResponse.json();

    if (!startResponse.ok) {
      showError("registerStatus", startResult.error.value || getTranslation('Error_Server_Error'));
      return "";
    }
    // Prüfe auf fehlende Challenge
    if (!startResult.options?.challenge) {
      showError("registerStatus", getTranslation('Error_PasskeyChallenge_Required'));
      return "";
    }
    // Prüfe auf user.id (muss existieren)
    if (!startResult.options?.user?.id) {
      showError("registerStatus", getTranslation('Error_PasskeyUserId_Missing'));
      return "";
    }

    showStatus("registerStatus", getTranslation('Register_Completing'));

    let attestationResponse;
    try {
      attestationResponse = await startRegistration({ optionsJSON: startResult.options });
    } catch (e) {
      showError("registerStatus", getTranslation('Error_Register_Aborted'));
      return "";
    }

    showStatus("registerStatus", getTranslation('Passkey_ConfirmNew'));

    const completeResponse = await fetch('/Passkey/MakeCredential', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        sessionKey: startResult.sessionKey,
        attestationResponse: attestationResponse
      })
    });

    showStatus("registerStatus", getTranslation('Passkey_Saving'));

    const completeResult = await completeResponse.json();
    if (!completeResponse.ok) {
      // Zeige spezifischen Server-Fehler an
      const errorMessage = completeResult.error.value ||
        getTranslation('Error_Server_Error');
      showError("registerStatus", errorMessage);
      return "";
    }

    showSuccess("registerStatus", `${getTranslation('Success_Passkey_Registered')} ${getTranslation('Register_Redirect')}`);

    return startResult.userName;
  } catch (err) {
    const error = err instanceof Error ? err : new Error(String(err));
    showError("registerStatus", error.message);
    return "";
  }
}

// Passkey entfernen
async function removePasskey(credentialIdBase64: string) {
  //if (!confirm('Möchten Sie diesen Passkey wirklich entfernen?')) return;

  // Konvertierung von Base64 zu byte[] für den Server
  const credentialId = Uint8Array.from(atob(credentialIdBase64), c => c.charCodeAt(0));

  const response = await fetch('/Passkey/RemovePasskey', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ credentialId: Array.from(credentialId) })
  });

  const result = await response.json();

  if (result.success) {
    showSuccess("registerStatus", getTranslation('Success_Passkey_Removed'));
    loadMyPasskeys(); // Liste aktualisieren
  } else {
    showError("registerStatus", result.error || getTranslation('Error_Passkey_RemoveFailed'));
  }
}
// Alle Passkeys des Benutzers anzeigen
async function loadMyPasskeys() {
  try {
    const response = await fetch('/Passkey/GetMyPasskeys');
    const result = await response.json();

    if (result.success && result.passkeys) {
      const container = document.getElementById('passkeysList');
      if (container) {
        container.innerHTML = result.passkeys.map((p: any) => `
                    <div class="passkey-item">
                        <div>
                            <strong>${p.deviceName || getTranslation('Unknown_Device')}</strong>
                            <div class="small text-muted">
                                Registriert am: ${new Date(p.regDate).toLocaleDateString()}
                            </div>
                        </div>
                        <button class="btn btn-sm btn-danger" 
                                onclick="removePasskey('${p.credentialId}')">
                            Entfernen
                        </button>
                    </div>
                `).join('');
      }
    }
  } catch (error) {
    console.error('Error loading passkeys:', error);
  }
}

// Globale Exporte
window.register = register;
window.checkWebAuthnSupport = checkWebAuthnSupport;

// Starte Register-Initialisierung
initializeRegisterPage();