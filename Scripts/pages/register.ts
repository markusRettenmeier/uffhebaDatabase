import { startRegistration } from '@simplewebauthn/browser';

export function checkWebAuthnSupport() {
  if (
    typeof window.PublicKeyCredential !== 'function' ||
    !navigator.credentials ||
    typeof navigator.credentials.create !== 'function'
  ) {
    showRegisterError('WebAuthn wird von diesem Browser nicht unterstützt')
  }
  showRegisterSuccess('WebAuthn / Passkeys werden unterstützt');
}
window.checkWebAuthnSupport = checkWebAuthnSupport;

function validateForm(displayName: string, email: string) {
  showRegisterStatus('');

  if (!displayName) {
    showRegisterError('Anzeigename ist erforderlich');
    return false;
  }
  if (displayName.length < 2) {
    showRegisterError('Anzeigename muss mindestens 2 Zeichen lang sein');
    return false;
  }

  if (email && !isValidEmail(email)) {
    showRegisterError('Bitte geben Sie eine gültige E-Mail-Adresse ein');
    return false;
  }

  return true;
}
function isValidEmail(email: string) {
  const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return emailPattern.test(email);
}

function showRegisterStatus(html: string) {
  const statusDiv = document.getElementById('registerStatus') as HTMLDivElement;
  if (statusDiv)
    statusDiv.innerHTML = html;
}
function showRegisterError(message: string) {
  showRegisterStatus(`
        <div class="alert alert-danger alert-dismissible fade show" role="alert" aria-live="assertive">
            <i class="bi bi-exclamation-triangle me-2"></i>
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `);

  const btnRegister = document.getElementById('btnRegister') as HTMLButtonElement;
  btnRegister.disabled = false;
  btnRegister.innerHTML = '<i class="bi bi-shield-check me-2"></i>Mit Passkey registrieren';
}
function showRegisterSuccess(message: string) {
  showRegisterStatus(`
        <div class="alert alert-success">
            <i class="bi bi-check-circle me-2"></i>
            ${message}
        </div>
    `);
}

async function register() {
  const displayNameInput = document.getElementById('DisplayName') as HTMLInputElement;
  const displayName = displayNameInput.value.trim();
  const emailInput = document.getElementById('Email') as HTMLInputElement;
  const email = emailInput.value.trim();

  if (!validateForm(displayName, email)) {
    return;
  }

  const btnRegister = document.getElementById('btnRegister') as HTMLButtonElement;
  try {
    // Disable button and show loading
    btnRegister.disabled = true;
    btnRegister.innerHTML = '<span class="spinner-border spinner-border-sm me-2" role="status" aria-live="polite"></span>Wird vorbereitet...';

    const startResponse = await fetch('/Passkey/StartRegistration', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        displayName: displayName,
        email: email
      })
    });
    const startResult = await startResponse.json();
    if (!startResponse.ok) {
      showRegisterError(startResult.error || 'Serverfehler');
      return;
    }

    btnRegister.innerHTML = '<span class="spinner-border spinner-border-sm me-2" role="status" aria-live="polite"></span>Wird abgeschlossen...';

    let attestationResponse;
    try {
      attestationResponse = await startRegistration(startResult.options);
    } catch (e) {
      showRegisterError('Registrierung wurde abgebrochen.');
      return;
    }

    const completeResponse = await fetch('/Passkey/MakeCredential', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        sessionKey: startResult.sessionKey,
        attestationResponse: attestationResponse
      })
    });
    const completeResult = await completeResponse.json();
    if (!completeResponse.ok) {
      showRegisterError(completeResult.error || 'Serverfehler');
      return;
    }

    showRegisterSuccess('Registrierung erfolgreich! Sie werden in Kürze weitergeleitet...');
    window.location.href = '/collectionItemDatabase/Index';
  } catch (err) {
    const error = err instanceof Error ? err : new Error(String(err));
    // User-friendly error messages
    let errorMessage = 'Ein Fehler ist aufgetreten';

    if (error.message.includes('abgebrochen')) {
      errorMessage = 'Die Passkey-Erstellung wurde abgebrochen.';
    } else if (error.message.includes('bereits vergeben')) {
      errorMessage = 'Dieser Benutzername ist bereits vergeben.';
    } else if (error.message.includes('existiert bereits')) {
      errorMessage = 'Dieser Passkey ist bereits registriert.';
    } else if (error.message.includes('HTTPS')) {
      errorMessage = 'Sicherheitsfehler: Diese Seite muss über HTTPS aufgerufen werden.';
    } else if (error.message.includes('nicht unterstützt')) {
      errorMessage = 'Ihr Browser oder Gerät unterstützt keine Passkeys.';
    } else if (error.message.includes('Ungültige Challenge')) {
      errorMessage = 'Die Sitzung ist abgelaufen. Bitte laden Sie die Seite neu.';
    } else {
      errorMessage = `Fehler: ${error.message}`;
    }

    showRegisterError(errorMessage);
  }
}
window.register = register;