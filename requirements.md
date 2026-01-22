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
- `cwlogs streams [<group-name>] [-g|--group <group-name>]` - Listet Streams einer Gruppe auf.
- `cwlogs fetch [<group-name>] [-g|--group <group-name>] [-s|--stream <name|number>] [--limit <n>] [--sort <asc|desc>] [--single-line] [--raw] [--clean]` - Zeigt Logs an.
- `cwlogs tail [<group-name>] [-g|--group <group-name>] [-s|--stream <name|number>] [--limit <n>] [--single-line] [--raw] [--clean]` - Folgt den Logs live.

## 5. Erweiterte Anforderungen (Sprint 6)
- **Tabellen-Design:** Spectre.Console Tabellen sollen standardmäßig keine Umrandung haben.
- **Stream-Auswahl:** Bei `fetch` and `tail` kann ein Stream per Name oder per Index (Zahl) ausgewählt werden. Eine Zahl `n` wählt die `n` neuesten Streams aus.
- **Limit:** Parameter `--limit` zur Begrenzung der Anzahl der abgerufenen Log-Einträge.
- **Sortierung:** Parameter `--sort` zur Steuerung der Sortierreihenfolge (`asc` oder `desc`).
- **Formatierung:**
    - `--single-line`: Log-Einträge werden einzeilig ausgegeben (entfernt Newlines innerhalb eines Eintrags).
    - `--raw`: Nur der Log-Text selbst wird ausgegeben, ohne Zeitstempel oder andere Präfixe des Tools.
    - `--clean`: Entfernt AWS-spezifische Präfixe (z. B. von Lambda: Timestamp, RequestID, Log-Level) aus der Nachricht.

## 6. Robuste Completion (Sprint 11)
- Einführung der `--group` Option für `streams`, `fetch` und `tail`.
- Die completion nutzt bevorzugt `--group` für die Identifizierung der Log-Gruppe.
- Die Positionsargumente bleiben als Alias erhalten, werden aber in der Completion-Logik robuster behandelt.
- Wenn die Eingabe mit `-` oder `--` beginnt, werden nur Optionen vervollständigt (Vermeidung von LogGroup/Stream Vorschlägen).

## 7. Auto-Completion (Sprint 9)
- Unterstützung für PowerShell 7.
- Vervollständigung von Befehlen und Optionen.
- Dynamische Vervollständigung von Log-Gruppen-Namen.
- Dynamische Vervollständigung von Log-Stream-Namen bei Verwendung der `--stream` Option.
- Generierung des Completion-Scripts über den Befehl `cwlogs completion`.

## 9. Deployment (Sprint 13 & 14)
- Das Tool soll als eigenständige Binärdatei (Single Binary) verteilt werden können.
- Ziel: Keine Abhängigkeit von einer vorinstallierten .NET Runtime auf dem Zielsystem (Self-Contained).
- Optimierung: Single-File Executable für einfache Handhabung.
- **Release-Verzeichnis:** Bei einem Production-Build (Release) soll das fertige Binary automatisch in einem `dist`-Ordner im Projektverzeichnis abgelegt werden.
