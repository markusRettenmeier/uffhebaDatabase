import { showModalFunction, hideModal } from '../helperFunctions';
import { startRegistration } from '@simplewebauthn/browser';
import { showError, showStatus, showSuccess, getTranslation } from '../shared';

let currentBackupCodes: string[] = [];
export function setBackupCodes(codes: string[]) {
  currentBackupCodes = codes;
}
export function getBackupCodes(): string[] {
  return currentBackupCodes;
}

export async function generateBackupCodes(count: number): Promise<string[]> {
  const response = await fetch('/Passkey/GenerateBackupCodes', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' }
  });

  const result = await response.json();
  return result.backupCodes; // Klartext-Codes (nur einmal!)
}

(window as any).downloadBackupCodes = function () {
  hideModal('backupCodesModal');

  const codes = getBackupCodes();
  if (!codes.length) return;
  const usernameInput = document.getElementById('backupCodesUsername') as HTMLInputElement;
  const username = usernameInput.value.trim();
  if (!username.length) return;

  downloadBackupCodeFile(username, codes);
};

(window as any).printBackupCodes = function () {
  hideModal('backupCodesModal');

  const codes = getBackupCodes();
  if (!codes.length) return;
  const usernameInput = document.getElementById('backupCodesUsername') as HTMLInputElement;
  const username = usernameInput.value.trim();
  if (!username.length) return;

  const printWindow = window.open('', '_blank');
  if (!printWindow) return;

  printWindow.document.writeln(`
        <!DOCTYPE html>
        <html>
        <head>
            <title>Sammlerplattform - Backup Codes</title>
            <style>
                body { font-family: Arial, sans-serif; padding: 20px; }
                h1 { color: #333; }
                .code { 
                    font-family: monospace; 
                    font-size: 1.2rem; 
                    margin: 10px 0;
                    padding: 8px;
                    background: #f5f5f5;
                    border-radius: 4px;
                }
                .footer { margin-top: 30px; font-size: 0.8rem; color: #666; }
            </style>
        </head>
        <body>
            <h1>🔐 ${getTranslation('BackupCodes')}</h1>
            <p>${getTranslation('BackupCode_File_Content')}</p>
            <div style="margin: 30px 0;">
                ${codes.map(code => `<div class="code">${code}</div>`).join('')}
            </div>
            <p>${getTranslation('UserName')}: ${username}</p>
            <div class="footer">
                <p>${getTranslation('CreatedAt')}: ${new Date().toLocaleString()}</p>
                <p>${getTranslation('YourSafetyIsImportant')}</p>
            </div>
        </body>
        </html>
    `);
  printWindow.document.close();
  printWindow.print();
  printWindow.close();
};

(window as any).copyBackupCodes = function () {
  hideModal('backupCodesModal');

  const codes = getBackupCodes();
  if (!codes.length) return;

  const usernameInput = document.getElementById('backupCodesUsername') as HTMLInputElement;
  const username = usernameInput.value.trim();
  if (!username.length) return;

  let text = getTranslation('BackupCode_File_Content') + "\n" + codes.join('\n');
  text += `\n${getTranslation('UserName')}: ${username}`
  navigator.clipboard.writeText(text);

  // Feedback anzeigen
  showCopyFeedback();
};

function showCopyFeedback() {
  const btn = document.activeElement as HTMLElement;
  if (!btn) return;

  const originalHtml = btn.innerHTML;
  btn.innerHTML = '<i class="bi bi-check-lg"></i>' + getTranslation('Copied');
  setTimeout(() => {
    btn.innerHTML = originalHtml;
  }, 2000);
}

async function useBackupCode(): Promise<void> {
  hideModal('useBackupCodeModal');

  const codeInput = document.getElementById('BackupCode') as HTMLInputElement;
  const backupCode = codeInput.value.trim();
  if (backupCode.length === 0) {
    return;
  }
  const usernameInput = document.getElementById('Username') as HTMLInputElement;
  const username = usernameInput.value.trim();
  if (username.length === 0) {
    return;
  }

  const response = await fetch('/Passkey/VerifyBackupCode', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ backupCode, username })
  });
  if (response.ok) {
    // Nach erfolgreichem Backup-Code Login
    addAdditionalPasskey(username);
  } else {
    showError("loginStatus", getTranslation('Error_BackupCodeUsername_Invalid'));
  }
}

async function addAdditionalPasskey(username: string) {
  const btnAdd = document.getElementById('btnAddPasskey') as HTMLButtonElement;
  if (btnAdd) btnAdd.disabled = true;

  showStatus("loginStatus", getTranslation('Preparing'));

  try {
    // 1. Neue Passkey-Registrierung starten
    const startResponse = await fetch('/Passkey/StartAdditionalPasskey', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' }
    });
    const startResult = await startResponse.json();
    if (!startResponse.ok) {
      const error = await startResponse.json();
      showError("loginStatus", error.error.value || startResult.error || getTranslation('Error_Server_Error'));
      return;
    }
    if (!startResult.options?.challenge) {
      showError("loginStatus", getTranslation('Error_PasskeyChallenge_Required'));
      return;
    }
    if (!startResult.options?.user?.id) {
      showError("loginStatus", getTranslation('Error_PasskeyUserId_Missing'));
      return;
    }

    showStatus("loginStatus", getTranslation('Passkey_ConfirmNew'));

    // 2. Passkey registrieren (gleicher Ablauf wie bei Neuregistrierung)
    let attestationResponse = await startRegistration(
      { optionsJSON: startResult.options }
    );    

    showStatus("loginStatus", getTranslation('Passkey_Saving'));

    // 3. Passkey zum Benutzer hinzufügen
    const addResponse = await fetch('/Passkey/AddPasskey', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        sessionKey: startResult.sessionKey,
        attestationResponse: attestationResponse
      })
    });
    const addResult = await addResponse.json();
    if (!addResponse.ok) {
      const errorMessage = addResult.error.value ||
        getTranslation('Error_Server_Error');
      showError("loginStatus", errorMessage);

      return;
    }

    showSuccess("loginStatus", `${getTranslation('Success_Passkey_Registered')} ${getTranslation('PasskeyCount', addResult.passkeyCount)}`);

    // 2.4 Neue Backup-Codes generieren (alte wurden bereits beim Login ungültig)
    const newBackupCodes = await generateBackupCodes(10);

    setBackupCodes(newBackupCodes);

    // 2.5 Neue Backup-Codes anzeigen und zum Ausdrucken auffordern
    showBackupCodesModal(newBackupCodes, username, {
      requirePrint: true,
      message: getTranslation("OldBackupCodesInvalid")
    });
  } catch (error: any) {
    showError("loginStatus", error.message);
  } finally {
    if (btnAdd) btnAdd.disabled = false;
  }
}

export function showBackupCodesModal(codes: string[], username: string, options?: { requirePrint?: boolean; message?: string }) {
  downloadBackupCodeFile(username, codes);

  showModalFunction('backupCodesModal');

  const codeInput = document.getElementById('backupCodesList') as HTMLDivElement;
  codeInput.innerHTML = codes.map(code => `<div class="backup-code-item p-2 bg-light rounded text-center"><code style="font-size: 1.2rem;">${code}</code></div>`).join('');

  const usernameInput = document.getElementById('backupCodesUsername') as HTMLInputElement;
  usernameInput.value = username;

  const messageDiv = document.getElementById('backupCodesMessage') as HTMLDivElement;
  if (messageDiv && options?.message) {
    messageDiv.innerHTML = options.message;
  }
}
function downloadBackupCodeFile(username: string, backupCode: string[]): void {
  const content = getTranslation('BackupCode_File_Content', backupCode, username, new Date().toLocaleString());
  const blob = new Blob([content], { type: 'text/plain' });
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = `passkey_username_${username.substring(0, 8)}_Date_${new Date().toLocaleString()}.txt`;
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
  URL.revokeObjectURL(url);
}

window.useBackupCode = useBackupCode;