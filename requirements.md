# Anforderungen: AWS CloudWatch Logs CLI Tool (cwlogs)

Dieses Dokument beschreibt die Anforderungen für das `cwlogs` CLI-Tool.

## 1. Zielsetzung
Ein einfaches Kommandozeilenwerkzeug (CLI), um AWS CloudWatch Logs zu durchsuchen, aufzulisten und in Echtzeit zu verfolgen (Tail).

## 2. Funktionale Anforderungen

### 2.1 AWS Authentifizierung
- Das Tool muss AWS Credentials automatisch aus den Standardquellen beziehen (Umgebungsvariablen, AWS CLI Profile in `~/.aws/credentials`).
- Unterstützung für Profile über einen optionalen Parameter (z.B. `--profile`).

### 2.2 Auflisten von Log-Gruppen
- Befehl zum Auflisten aller verfügbaren Log-Gruppen in der konfigurierten Region.

### 2.3 Auflisten von Log-Streams
- Befehl zum Auflisten der Log-Streams innerhalb einer spezifischen Log-Gruppe.

### 2.4 Anzeige von Logs
- Befehl zum Ausgeben der Log-Einträge einer Log-Gruppe.
- Optionaler Filter auf einen spezifischen Log-Stream.
- Formatierte Ausgabe auf der Konsole.

### 2.5 Tail-Funktionalität
- Kontinuierliches Abrufen neuer Log-Einträge ("Live-Tail").
- Ähnlich wie `tail -f`.

## 3. Nicht-funktionale Anforderungen
- **Technologie:** .NET 10 (C#).
- **Benutzerfreundlichkeit:** Intuitive Befehle, Hilfe-Texte.
- **Performance:** Effizientes Paging beim Abrufen großer Log-Mengen.

## 4. Geplante Befehle (Vorschlag)
- `cwlogs groups` - Listet Log-Gruppen auf.
- `cwlogs streams <group-name>` - Listet Streams einer Gruppe auf.
- `cwlogs fetch <group-name> [--stream <name|number>] [--limit <n>] [--sort <asc|desc>] [--single-line] [--raw]` - Zeigt Logs an.
- `cwlogs tail <group-name> [--stream <name|number>] [--limit <n>] [--single-line] [--raw]` - Folgt den Logs live.

## 5. Erweiterte Anforderungen (Sprint 6)
- **Tabellen-Design:** Spectre.Console Tabellen sollen standardmäßig keine Umrandung haben.
- **Stream-Auswahl:** Bei `fetch` und `tail` kann ein Stream per Name oder per Index (Zahl) ausgewählt werden. Eine Zahl `n` wählt die `n` neuesten Streams aus.
- **Limit:** Parameter `--limit` zur Begrenzung der Anzahl der abgerufenen Log-Einträge.
- **Sortierung:** Parameter `--sort` zur Steuerung der Sortierreihenfolge (`asc` oder `desc`).
- **Formatierung:**
    - `--single-line`: Log-Einträge werden einzeilig ausgegeben (entfernt Newlines innerhalb eines Eintrags).
    - `--raw`: Nur der Log-Text selbst wird ausgegeben, ohne Zeitstempel oder andere Präfixe des Tools.
    - `--clean`: Entfernt AWS-spezifische Präfixe (z. B. von Lambda: Timestamp, RequestID, Log-Level) aus der Nachricht.

## 6. Wartbarkeit & Clean Code
- Gemeinsame Logik für `fetch` und `tail` wird in einer Basisklasse (`LogBaseCommand`) gekapselt.
- Klare Trennung von Zuständigkeiten (Settings, Commands, Base-Logic).
