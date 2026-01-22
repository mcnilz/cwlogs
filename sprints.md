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

## Sprint 8: Refactoring und Clean Code
**Ziel:** Reduzierung von Code-Duplikaten und Verbesserung der Wartbarkeit.
- [x] Einführung einer Basisklasse `LogBaseCommand` für gemeinsame Funktionalität von `fetch` und `tail`.
- [x] Extraktion von `ResolveStreams`, `PrintLogEvent` und `CleanLambdaMessage`.
- [x] Refactoring von `FetchCommand` und `TailCommand` zur Nutzung der Basisklasse.

## Sprint 9: Auto-Completion
**Ziel:** Unterstützung für PowerShell Auto-Completion.
- [x] Implementierung eines versteckten `_complete` Befehls für dynamische Daten.
- [x] Implementierung des `completion` Befehls zur Generierung des PowerShell-Scripts.
- [x] Unterstützung für Befehle, Optionen, Log-Gruppen und Log-Streams.
- [x] Dokumentation der Installation des Scripts.

## Sprint 10: Completion-Verfeinerung
**Ziel:** Robusteres PowerShell-Completion-Skript.
- [x] Verbesserung der Positionsargument-Erkennung (LogGroups).
- [x] Berücksichtigung von Optionen bei der Argument-Zählung.
- [x] Stabilisierung der LogStream-Vervollständigung.
- [x] Fix für `cwlogs fetch <TAB>` bei leerem Completion-Präfix.
- [x] Fallback-Logik für Gruppen-Vervollständigung hinzugefügt.

## Sprint 11: Robuste Resource-Completion
Ziel: Erhöhung der Zuverlässigkeit der Vervollständigung durch explizite Parameter.
- [x] Einführung der `--group` Option für alle relevanten Befehle.
- [x] Anpassung der Command-Logik zur Bevorzugung von `--group`.
- [x] Aktualisierung des PowerShell-Completion-Skripts zur Nutzung von `--group`.
- [x] Verbesserung der Erkennung von Log-Gruppen in komplexen Befehlszeilen.

## Sprint 12: Pfad-Robustheit der Completion
Ziel: Sicherstellen, dass die Completion auch funktioniert, wenn das Tool nicht im PATH ist.
- [x] Ermittlung des absoluten Pfads zum Executable in `CompletionCommand`.
- [x] Nutzung des absoluten Pfads innerhalb des generierten PowerShell-Skripts.
- [x] Registrierung des Completers für Dateinamen mit und ohne `.exe` Erweiterung.

## Sprint 13: Production Build (Single Binary)
Ziel: Erstellung eines eigenständigen Executables ohne .NET Abhängigkeiten.
- [x] Konfiguration des Projekts für `SelfContained` und `PublishSingleFile`.
- [x] Deaktivierung von Trimming zur Sicherstellung der Kompatibilität mit Spectre.Console.Cli.
- [x] Verifizierung des Single-File Builds.
- [x] Dokumentation der Build-Parameter.

## Sprint 14: Release Distribution
Ziel: Automatisierung der Ablage des fertigen Binaries.
- [x] Konfiguration des `PublishDir` in der Projektdatei für Release-Builds.
- [x] Sicherstellung, dass der `dist`-Ordner verwendet wird.
- [x] Verifizierung des Publish-Outputs.

## Sprint 15: Completion Priorisierung
Ziel: Verbesserung der UX durch gezielte Vervollständigung von Optionen.
- [x] Priorisierung von Optionen in der Completion-Logik, wenn das Präfix `-` oder `--` ist.
- [x] Unterdrückung von dynamischen Ressourcen-Vorschlägen (Groups/Streams) bei Options-Eingabe.

## Sprint 16: Dynamische Completion-Metadaten
Ziel: Erhöhung der Wartbarkeit durch dynamisches Laden von Befehlen und Optionen.
- [x] Implementierung von `_complete --type metadata` zur Ausgabe von Befehls- und Options-Informationen via Reflexion.
- [x] Anpassung von `CompletionCommand` zur Nutzung der dynamischen Metadaten im PowerShell-Skript.
- [x] Dynamische Erkennung von Optionen mit Werten für eine robustere Argument-Zählung.

## Sprint 17: Zentralisierung von Konstanten
Ziel: Verbesserung der Wartbarkeit durch Vermeidung von String-Duplikaten.
- [x] Einführung der Klasse `CommandNames` für Command-Name Konstanten.
- [x] Refactoring von `Program.cs`, `CompleteCommand.cs` und `CompletionCommand.cs` zur Nutzung der Konstanten.
