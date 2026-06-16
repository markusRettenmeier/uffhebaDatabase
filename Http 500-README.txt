Notwendig solange Bge3M3 als semantische Suche dient, wenn zu Cohere gewechselt wird, ist dieser Scritt unnötig.

1. Bei centron kommt im Windows-Webhosting in der Regel das Plesk Control Panel zum Einsatz. Standardmäßig laufen dort IIS-Anwendungspools oft im 32-Bit-Modus, um Server-Ressourcen zu schonen. Da die ONNX Runtime zwingend eine 64-Bit-Architektur (x64) verlangt, blockiert diese Einstellung das Laden der DLL.
Sie können die 64-Bit-Unterstützung über Ihr centron-Kundencenter wie folgt erzwingen:
## Weg 1: Über das centron Webpanel (Plesk) deaktivieren
Da x64 bei modernen Betriebssystemen der Standard ist, müssen Sie im Panel nach der Option suchen, die den 32-Bit-Modus erlaubt, und diese deaktivieren. [1] 

   1. Melden Sie sich im centron Kundenmenü an und öffnen Sie Ihr Webhosting-Panel (Plesk).
   2. Navigieren Sie zu Websites & Domains.
   3. Suchen Sie nach dem Punkt Dedizierter IIS-Anwendungspool (meistens unter den erweiterten Einstellungen oder in der rechten Seitenleiste).
   4. Suchen Sie dort nach der Option 32-Bit-Anwendungen zulassen (bzw. "Enable 32-bit applications").
   5. Setzen Sie diese Option auf Nein / Deaktiviert. Dadurch schaltet der IIS-Anwendungspool automatisch auf reines 64-Bit (x64) um.
   6. Klicken Sie auf Recycle / Neu starten, um die Änderungen sofort anzuwenden. [2, 3] 

## Weg 2: Der "OutOfProcess"-Workaround (Empfohlen)
Falls centron Ihnen im Shared Hosting keinen direkten Zugriff auf die Bit-Einstellungen des IIS-Anwendungspools gewährt, gibt es einen eleganten Hebel in Ihrer web.config.
Wenn Sie das Hosting-Modell auf OutOfProcess umstellen, startet ASP.NET die Anwendung nicht mehr innerhalb des IIS (der eventuell auf 32-Bit limitiert ist), sondern startet einen eigenen, separaten Windows-Prozess für Ihre App. Dieser Prozess richtet sich nach der Architektur Ihrer hochgeladenen Binärdateien (welche durch Ihr Publishing auf win-x64 eingestellt sind).
Ersetzen Sie den <aspNetCore>-Tag in Ihrer auf dem Server liegenden web.config mit folgendem Inhalt:

<aspNetCore processPath="dotnet" 
            arguments=".\IhrProjektName.dll" 
            hostingModel="outofprocess" 
            stdoutLogEnabled="true" 
            stdoutLogFile=".\logs\stdout" />

(Ersetzen Sie IhrProjektName.dll mit dem echten Namen Ihrer Kompilation).
## Wichtiger finaler Schritt
Stellen Sie sicher, dass beim Hochladen Ihres Projekts über Visual Studio oder die CLI der richtige Runtime Identifier gewählt wurde, damit auch die x64-Versionen kompiliert werden:
dotnet publish -c Release -r win-x64 --self-contained false
Sollten die Optionen im Plesk-Panel gesperrt sein oder der OutOfProcess-Wechsel fehlschlagen, schreiben Sie eine kurze Nachricht an den centron-Support: „Bitte stellen Sie den dedizierten IIS-Anwendungspool für meine Domain auf 64-Bit um (32-Bit-Anwendungen deaktivieren), da ich eine native x64-C++-Bibliothek (ONNX Runtime) laden muss.“ [1] 
Lassen Sie mich wissen, ob die Umstellung auf OutOfProcess in der web.config den Fehler bereits beheben konnte!

[1] [https://talk.plesk.com](https://talk.plesk.com/threads/enable-32-bit-applications-by-command-line-windows.358004/)
[2] [https://docs.plesk.com](https://docs.plesk.com/de-DE/obsidian/reseller-guide/websiteverwaltung/websites-und-domains/erweiterte-websiteverwaltung/einrichten-des-iisanwendungspools-windows.65282/)
[3] [https://docs.centron.de](https://docs.centron.de/docs/product/webhosting/how-tos/verwaltung-via-webpanel/)

Das Log liefert die entscheidende Information. Der Fehlercode hat sich von 0x8007007E (Datei nicht gefunden) zu 0x8007045A (ERROR_DLL_INIT_FAILED) geändert.
Das bedeutet: Ihre ASP.NET-Anwendung findet die onnxruntime.dll jetzt erfolgreich. Allerdings bricht Windows das Laden der Datei ab, weil der Serverumgebung eine fundamentale Systemkomponente fehlt, die ONNX zum Initialisieren benötigt.
Bei ONNX Runtime 1.26.0 ist dies zu 99% die OpenMP-Laufzeitumgebung (vcomp140.dll). Diese wird für die parallele CPU-Berechnung benötigt und fehlt auf centron-Webhosting-Servern standardmäßig im Systemverzeichnis.
## Die Lösung
Da Sie die DLLs auf dem Server nicht installieren können, müssen Sie die fehlenden Windows-Systembibliotheken direkt mit Ihrer App hochladen.

   1. Suchen Sie die Datei lokal: Öffnen Sie auf Ihrem lokalen PC den Windows-Dateimanager und navigieren Sie zu C:\Windows\System32\.
   2. Kopieren Sie die Datei: Suchen Sie nach der Datei vcomp140.dll (das ist die Microsoft OpenMP-Laufzeitbibliothek).
   3. Hochladen auf den Server: Laden Sie diese vcomp140.dll per FTP/Plesk direkt in das Hauptverzeichnis Ihrer Web-App auf dem centron-Server hoch (in denselben Ordner, in den Sie zuvor auch onnxruntime.dll gelegt haben).
   4. Zusatz-Sicherheit: Falls noch nicht geschehen, stellen Sie sicher, dass sich auch die Dateien msvcp140.dll und vcruntime140.dll aus Ihrem lokalen System32-Ordner flach in diesem Serververzeichnis befinden.

## Warum das passiert
ONNX Runtime versucht beim Start (OrtGetApiBase), Multithreading-Routinen über OpenMP zu registrieren. Findet Windows die vcomp140.dll im System nicht, bricht die Initialisierung der onnxruntime.dll mit genau dem Fehler 0x8007045A ab. Sobald die Datei direkt neben der Anwendung liegt, lädt Windows sie bevorzugt.
Führen Sie nach dem Hochladen der Datei einen Recycle / Neustart des Anwendungspools im centron-Panel durch, damit der IIS den Cache der fehlgeschlagenen DLLs bereinigt.
Falls nach dem Hochladen der vcomp140.dll ein neuer Fehler im Log erscheint, lassen Sie mich wissen, welcher Fehlercode oder Text dort steht.

