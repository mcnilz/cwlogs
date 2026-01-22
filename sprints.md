# Sprint-Planung: cwlogs

Dieses Dokument unterteilt die Entwicklung in drei Sprints.

## Sprint 1: Projekt-Setup und AWS-Anbindung
**Ziel:** Lauffähige CLI-Basis mit AWS-Authentifizierung.
- [x] Initialisierung des .NET 10 Projekts.
- [x] Einbindung der notwendigen NuGet-Pakete (`AWSSDK.CloudWatchLogs`, `Spectre.Console.Cli`).
- [x] Implementierung der AWS-Profil-Auswahl.
- [x] Grundstruktur der CLI-Befehle (Command Pattern).

## Sprint 2: Exploration (Listen)
**Ziel:** Auflisten von Ressourcen.
- [x] Implementierung `cwlogs groups`: Listet Log-Gruppen tabellarisch auf.
- [x] Implementierung `cwlogs streams <group>`: Listet Log-Streams einer Gruppe auf.
- [x] Fehlerbehandlung bei fehlenden Berechtigungen oder ungültigen Regionen.

## Sprint 3: Log-Konsum (Fetch & Tail)
**Ziel:** Anzeige und Live-Streaming von Logs.
- [x] Implementierung `cwlogs fetch <group>`: Einmaliges Abrufen und Ausgeben von Logs.
- [x] Implementierung der Filterung nach Log-Streams.
- [x] Implementierung `cwlogs tail <group>`: Endlosschleife zum Abrufen neuer Events (Polling).
- [x] Optimierung der Konsolenausgabe (Farben, Zeitstempel).

## Sprint 4: SSO-Support und Credential-Handling
**Ziel:** Unterstützung für AWS SSO und verbessertes Laden von Credentials.
- [x] Einbindung von `AWSSDK.SSO` und `AWSSDK.SSOOIDC`.
- [x] Umstellung auf `DefaultAWSCredentialsIdentityResolver` für robustere Authentifizierung.
- [x] Behebung von Problemen mit Umgebungsvariablen wie `AWS_PROFILE`.

## Sprint 5: Bugfixes und Robustheit
**Ziel:** Behebung von Laufzeitfehlern und Verbesserung der Stabilität.
- [x] Fix `System.InvalidOperationException: Could not find color or style '$LATEST'`.
- [x] Absicherung der Konsolenausgabe gegen Sonderzeichen in Log-Daten (Markup Escaping).

## Sprint 6: Erweiterte Features und Formatierung
**Ziel:** Flexiblere Steuerung der Ausgabe und Stream-Auswahl.
- [x] Tabellen ohne Umrandung standardmäßig.
- [x] Stream-Auswahl per Name oder Index (die neuesten `n` Streams).
- [x] Parameter `--limit` für die Anzahl der Einträge.
- [x] Parameter `--sort` für die Sortierreihenfolge.
- [x] Formatierungsoptionen `--single-line` und `--raw`.

## Sprint 7: Log-Cleanup und Usability
**Ziel:** Verbessertes Handling von AWS-spezifischen Log-Präfixen.
- [x] Implementierung der `--clean` Option zur Entfernung von Lambda-Präfixen.
- [x] Konsistente Anwendung von `--clean` in `fetch` und `tail`.
