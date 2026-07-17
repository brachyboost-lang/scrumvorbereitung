# ScrumPrep

Windows-Lernapp (WPF, .NET 8) zur Vorbereitung auf das **PSM I Assessment** (Professional
Scrum Master I, Scrum.org). Enthält 200 englische Übungsfragen auf Basis des Scrum Guide 2020
mit Erklärungen — die echte Prüfung hat 80 Fragen in 60 Minuten, Bestehensgrenze 85 %.

## Voraussetzungen

- Windows
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (`dotnet --list-sdks` zeigt installierte Versionen)

## Starten

```powershell
git clone https://github.com/brachyboost-lang/scrumvorbereitung.git
cd scrumvorbereitung
dotnet run --project ScrumPrep
```

Alternativ die Solution [ScrumPrep.slnx](ScrumPrep.slnx) in Visual Studio öffnen und starten,
oder nach einem `dotnet build` direkt die Exe ausführen:
`ScrumPrep\bin\Debug\net8.0-windows\ScrumPrep.exe`

## Modi

| Modus | Beschreibung |
|---|---|
| Exam Simulation | 80 Zufallsfragen, 60-Minuten-Timer, Auswertung mit 85-%-Grenze wie in der echten Prüfung |
| Learn Mode | Alle Fragen in Zufallsreihenfolge, sofortige Auflösung mit Erklärung |
| Topic Training | Wie Learn Mode, gefiltert auf ein Themengebiet |
| Mistake Training | Wiederholt Fragen, deren letzte Antwort falsch war, bis sie einmal richtig beantwortet wurden |
| Statistics | Sitzungshistorie und Trefferquote pro Thema |

## Daten

- **Fragenkatalog:** [ScrumPrep/Data/questions.json](ScrumPrep/Data/questions.json) — wird beim
  Start gelesen. Neue Fragen einfach anhängen: eindeutige `id`, `category` (einer der vier
  bestehenden Namen), `text`, `options`, `correct` (Liste der richtigen Options-Indizes, ab 0),
  `explanation`. Mehrere Indizes in `correct` machen die Frage automatisch zur Mehrfachauswahl.
- **Lernfortschritt:** wird unter `%APPDATA%\ScrumPrep\history.json` gespeichert und kann in der
  App über *Statistics → Clear History* zurückgesetzt werden.

## Hinweis

Die Fragen sind selbst formuliert und keine echten Prüfungsfragen. Als Gegenprobe vor der
echten Prüfung lohnt das offizielle [Scrum Open Assessment](https://www.scrum.org/open-assessments)
von Scrum.org.
