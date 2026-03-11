# Pragmatischer Claude Code Einsatz für C# Entwickler auf Mac

Begleitendes Repository zum Blog-Artikel **"Pragmatischer Claude Code Einsatz für C# Entwickler auf Mac"**.

Dieses Repository enthält Beispiel-Skills, Custom Commands, Subagent-Konfigurationen und PRD-Vorlagen, die im Artikel beschrieben werden. Die Dateien sind als Anregung und Ausgangspunkt für eigene Anpassungen gedacht -- nicht zur direkten Verwendung ohne projektspezifische Anpassung.

## Inhaltsverzeichnis

```
├── commands/            # Custom Commands für Claude Code
├── skills/              # Skills (Wissen & Kontext für das LLM)
├── agents/              # Subagent-Konfigurationen
└── prds/                # PRD-Vorlagen und Beispiele
```

## Custom Commands

Benutzerdefinierte Slash-Commands, die den Claude Code Prompt um zusätzlichen Kontext erweitern.

| Datei | Beschreibung |
|---|---|
| [`commands/generate-prd.md`](commands/generate-prd.md) | `/generate-prd` -- Erzeugt aus einer Feature-Beschreibung ein vollständiges Product Requirements Document (PRD) |
| [`commands/execute-prd.md`](commands/execute-prd.md) | `/execute-prd` -- Startet die Implementierung eines Features auf Basis eines PRD |
| [`commands/go.md`](commands/go.md) | `/go` -- Shortcut-Command, der wiederkehrende MCP-Referenzen und CLAUDE.md-Kontextinstruktionen kapselt |

## Skills

Skills erweitern Claude Code mit Wissen zu bestimmten Tools, Frameworks, Code-Standards und Patterns. Jeder Skill besteht aus einer `SKILL.md` und optionalen Referenz- bzw. Beispieldateien.

| Skill | Beschreibung |
|---|---|
| [`skills/dotnet-code-standards/`](skills/dotnet-code-standards/) | C# Code-Standards -- Naming Conventions, Guard Clauses, moderne C# 12-14 Features (Primary Constructors, Collection Expressions etc.), Logging, Testing und Formatierungsvorgaben. Inkl. Beispieldateien. |
| [`skills/dotnet-colocated-localization/`](skills/dotnet-colocated-localization/) | Lokalisierung in ASP.NET Core mit co-located Resource Files und URL-basierter Spracherkennung. Inkl. Templates und Infrastruktur-Klassen. |
| [`skills/directus-dotnet-repository/`](skills/directus-dotnet-repository/) | Directus Headless CMS Integration -- Filtering, Pagination, CRUD-Operationen und Authentifizierung mit einem eigenen C# Client. |
| [`skills/evanto-paypal-client/`](skills/evanto-paypal-client/) | PayPal-Integration in ASP.NET -- Einmalzahlungen, Subscriptions, SEPA, Webhooks und Fee-Berechnung. |
| [`skills/project-xy-design-guide/`](skills/project-xy-design-guide/) | Beispiel eines projektspezifischen UI-Design-Guides -- Tailwind CSS Klassen, Farbschema, Komponentenstruktur. |
| [`skills/dotnet-security-owasp/`](skills/dotnet-security-owasp/) | OWASP Top 10 (2021) Sicherheitsleitfaden -- .NET-spezifische Mitigations für Injection, Broken Access Control, XSS, SSRF, veraltete APIs (CAS, BinaryFormatter, .NET Remoting) und Security-Header-Konfiguration. Dient als Wissensschicht für den `owasp-security-audit` Subagenten. |

## Subagents

Spezialisierte, autonom arbeitende Agents für klar eingegrenzte Aufgaben.

| Datei | Beschreibung |
|---|---|
| [`agents/seo-optimization-expert.md`](agents/seo-optimization-expert.md) | SEO-Subagent (Sonnet) -- Technische SEO-Audits, On-Page-Optimierungen und automatisierte Verifikation via Playwright. |
| [`agents/owasp-security-audit.md`](agents/owasp-security-audit.md) | Autonomer OWASP-Security-Auditor -- Systematischer Scan der .NET-Codebasis auf OWASP-Top-10-Verletzungen mit strukturiertem Findings-Report (Schweregrad, Datei-Positionen, Behebungshinweise). Referenziert den `dotnet-security-owasp` Skill als Wissensquelle. |

## PRDs (Product Requirements Documents)

Vorlagen und Beispiele für den PRD-basierten Entwicklungsworkflow.

| Datei | Beschreibung |
|---|---|
| [`prds/templates/prd-base-csharp.md`](prds/templates/prd-base-csharp.md) | PRD-Vorlage, optimiert für C# / .NET Projekte |
| [`prds/templates/prd-base-python.md`](prds/templates/prd-base-python.md) | PRD-Vorlage für Python Projekte |
| [`prds/samples/simple-prd-prompt.md`](prds/samples/simple-prd-prompt.md) | Beispiel: Einfacher Eingabe-Prompt |
| [`prds/samples/simple-prd-generated.md`](prds/samples/simple-prd-generated.md) | Beispiel: Daraus generiertes PRD |
| [`prds/samples/complex-prd-prompt.md`](prds/samples/complex-prd-prompt.md) | Beispiel: Komplexer Eingabe-Prompt |
| [`prds/samples/complex-prd-generated.md`](prds/samples/complex-prd-generated.md) | Beispiel: Daraus generiertes PRD |

## Empfohlener Workflow

1. **Projekt aufsetzen:** `claude /init` im Projektverzeichnis, CLAUDE.md befüllen
2. **PRD erstellen:** `/generate-prd prds/mein-feature-prompt.md`
3. **PRD reviewen:** Inhalt, Code-Standards und Performance prüfen
4. **Implementieren:** `/execute-prd prds/mein-feature-prd.md`
5. **CLAUDE.md aktualisieren:** Nach jedem umgesetzten Feature

Details zum Workflow und zur Konfiguration finden sich im Blog-Artikel.

## Voraussetzungen

- .NET SDK (aktuelle LTS oder STS Version)
- VS Code mit C# Dev Kit Extension
- Node.js (für Claude Code und MCP Server)
- [Claude Code](https://code.claude.com/docs/en/getting-started) (`npm install -g @anthropic-ai/claude-code`)

## Lizenz und Hinweis

Die Dateien in diesem Repository dienen als Beispiele und Vorlagen. Sie sind projektspezifisch entstanden und sollten vor der Verwendung an die eigenen Anforderungen angepasst werden.

---

evanto media GmbH | 2026
