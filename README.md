#  Inteligentny Asystent Sprzedaży

> **Inteligentny Asystent Sprzedaży AI 2026**: System produkcyjny z deterministyczną walidacją i architekturą enterprise

Inteligentny asystent sprzedaży dla sklepu , implementujący wzorzec **Writer-Critic** eliminujący halucynacje cenowe i zapewniający faktyczną poprawność odpowiedzi.

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker)](https://www.docker.com/)
[![AI](https://img.shields.io/badge/AI-Semantic%20Kernel-00A4EF)](https://github.com/microsoft/semantic-kernel)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

---

## 📖 Dokumentacja

- **[INSTRUKCJA URUCHOMIENIA](INSTRUKCJA_URUCHOMIENIA.md)** - Szczegółowy przewodnik

---

### 1. To Nie Jest "Zabawka" - To System Produkcyjny

✅ **Determinizm** - Wymuszony format JSON, pełna kontrola nad odpowiedziami AI
✅ **Governance** - Audytowalne logi, pełna transparentność procesu
✅ **Bezpieczeństwo** - Klucze API w zmiennych środowiskowych, izolacja sekretów
✅ **Skalowalność** - Docker, healthchecks, DbContextFactory dla Blazor Server

### 2. Wzorzec Writer-Critic (Agentic AI)

```
Użytkownik → Writer (generuje odpowiedź) → Critic (weryfikuje fakty)
                ↓                                    ↓
         Jeśli błąd ← ← ← ← ← ← ← ← ← ← ← ← ← Feedback
                ↓
         Max 3 iteracje
                ↓
         Zatwierdzona odpowiedź → Użytkownik
```

**Dlaczego to ważne?**
- AI samo się sprawdza (Quality Assurance)
- Eliminacja halucynacji cenowych
- Transparentność procesu (logi Writer/Critic)

### 3. Context Injection (Grounding)

Model językowy **nie ma dostępu do bazy danych**. Dlatego:
- Wstrzykuję katalog produktów bezpośrednio do promptu
- AI operuje na **faktach z bazy SQL**, nie na wiedzy treningowej
- To uproszczona wersja RAG (Retrieval Augmented Generation)

---

## 🏗️ Architektura

Zbudowana w **Onion Architecture** (Clean Architecture) dla łatwej konserwacji i testowalności:

- **Core Layer:** Encje domenowe (ChatSession, Product, AgentLog)
- **Application Layer:** Interfejsy biznesowe (IAgentService)
- **Infrastructure Layer:** Implementacja AI (Semantic Kernel) + Baza danych (EF Core + MySQL)
- **WebUI Layer:** Interfejs użytkownika (Blazor Server)

```
PetWorld/
├── src/
│   ├── PetWorld.Core/              # Czyste encje domenowe
│   ├── PetWorld.Application/       # Interfejsy (Dependency Inversion)
│   ├── PetWorld.Infrastructure/    # AI + Baza danych
│   └── PetWorld.WebUI/             # Blazor Server UI
├── docker-compose.yml
├── Dockerfile
└── .env                            # Klucze API (CHRONIONE)
```

---

## 🚀 Szybki Start (Wymagany Docker)

### Krok 1: Sklonuj repozytorium
```bash
git clone https://github.com/twoj-username/petworld-ai.git
cd petworld-ai
```

### Krok 2: Skonfiguruj klucz OpenAI
Edytuj plik `.env`:
```env
OPENAI_API_KEY=sk-proj-TWOJ_KLUCZ_TUTAJ
```

### Krok 3: Uruchom aplikację
```bash
docker compose up --build
```

### Krok 4: Otwórz w przeglądarce
```
http://localhost:5000
```

**Gotowe!** Aplikacja działa z pełnym systemem Writer-Critic.

---


## 🔒 Bezpieczeństwo

### Zarządzanie Sekretami
- ✅ Klucze API w zmiennych środowiskowych (NIGDY w kodzie)
- ✅ Plik `.env` w `.gitignore` (chroniony przed wyciekiem)
- ✅ Przykładowy `.env.example` dla nowych użytkowników




---

## 🛠️ Zarządzanie Aplikacją

### Uruchomienie bez Dockera (development lokalny)
```bash
dotnet run --project src/PetWorld.WebUI
```

### Wyświetlenie logów
```bash
docker compose logs -f app
```

### Zatrzymanie aplikacji
```bash
docker compose down
```

### Restart
```bash
docker compose restart app
```

---

## 🚀 Przyszłe Rozszerzenia

- [ ] **Vector Search (RAG)** - Dla katalogów >10 000 produktów
- [ ] **A/B Testing** - Testowanie różnych promptów
- [ ] **Telemetria** - Application Insights dla monitoringu AI
- [ ] **Cache** - Redis dla często zadawanych pytań
- [ ] **Multi-language** - Wsparcie dla wielu języków

---


## 📄 Licencja

MIT License

