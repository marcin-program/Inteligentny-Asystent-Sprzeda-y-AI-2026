🚀 Instrukcja Uruchomienia 

## ✅ Status: APLIKACJA DZIAŁA!

**URL:** http://localhost:5000

---

## 📋 Co Zostało Zrobione

### 1. Naprawiono Problemy
- ✅ Kodowanie polskich znaków w CSV (ą, ę, ó, ł, ż, ź, ć, ń, ś)
- ✅ Język interfejsu ustawiony na polski (`<html lang="pl">`)
- ✅ Dodano 17 produktów do katalogu
- ✅ Wszystkie komentarze wyjaśniają "DLACZEGO"
- ✅ Dokumentacja README.md i AI_COLLABORATION.md
- ✅ Plik .gitignore chroni klucz API

### 2. Struktura Projektu
```
PetWorld/
├── src/
│   ├── PetWorld.Core/              # Encje domenowe
│   ├── PetWorld.Application/       # Interfejsy
│   ├── PetWorld.Infrastructure/    # AI + Baza danych
│   └── PetWorld.WebUI/             # Blazor UI
├── docker-compose.yml
├── Dockerfile
├── .env                            # Twój klucz OpenAI (CHRONIONY)
├── .gitignore                      # Zabezpiecza .env
├── README.md
├── AI_COLLABORATION.md
├── PODSUMOWANIE_REKRUTACJA.md
└── setup_final.ps1                 # Skrypt instalacyjny
```

---

## 🎯 Jak Uruchomić (dla Rekrutera)

### Wymagania:
- Docker Desktop

### Krok po kroku:

1. **Sklonuj repozytorium**
   ```bash
   git clone <twoj-link>
   cd PetWorld
   ```

2. **Skonfiguruj klucz OpenAI**

   Edytuj plik `.env`:
   ```env
   OPENAI_API_KEY=sk-proj-TWOJ_KLUCZ_TUTAJ
   ```

3. **Uruchom aplikację**
   ```bash
   docker compose up --build
   ```

4. **Otwórz przeglądarkę**
   ```
   http://localhost:5000
   ```

---

## 🧪 Jak Przetestować

### Test 1: Podstawowe Pytanie
**Pytanie:** "Jaka karma dla kota seniora?"

**Oczekiwana odpowiedź:** AI powinno polecić produkty z kategorii "Karma dla kotow" z katalogu.

### Test 2: Pytanie o Cenę
**Pytanie:** "Ile kosztuje Royal Canin Maxi Adult 15kg?"

**Oczekiwana odpowiedź:** "249.99 PLN" (dokładna cena z katalogu)

### Test 3: Weryfikacja Writer-Critic
**Pytanie:** "Czy macie karmę dla papug?"

**Oczekiwana odpowiedź:** AI powinno powiedzieć, że nie ma takiego produktu w ofercie (nie wymyśli produktu)

### Test 4: Sprawdzenie Logów
Po zadaniu pytania, rozwiń sekcję "View AI Decision Process" - zobaczysz:
- Odpowiedź Writer
- Weryfikację Critic
- Liczbę iteracji (1-3)

---

## 📊 Kluczowe Funkcje

### Writer-Critic Pattern
1. **Writer** generuje odpowiedź na podstawie katalogu produktów
2. **Critic** sprawdza poprawność (ceny, nazwy)
3. Jeśli Critic odrzuci → Writer poprawia (max 3 iteracje)
4. Tylko zatwierdzone odpowiedzi trafiają do użytkownika

### Context Injection (Grounding)
- Katalog produktów wstrzykiwany do System Promptu
- Zapobiega halucynacjom (wymyślaniu produktów/cen)
- Uproszczona wersja RAG (Retrieval Augmented Generation)

### Live Inventory
- Prawa strona interfejsu pokazuje aktualny katalog
- Użytkownik widzi, jakie dane ma AI
- Transparentność procesu

---

## 🔧 Zarządzanie Aplikacją

### Sprawdzenie Statusu
```bash
docker compose ps
```

### Wyświetlenie Logów
```bash
docker compose logs app -f
```

### Restart Aplikacji
```bash
docker compose restart app
```

### Zatrzymanie Aplikacji
```bash
docker compose down
```

### Pełne Przebudowanie
```bash
docker compose down
docker compose up --build
```

---

---

## 📈 Metryki Projektu

| Metryka | Wartość |
|---------|---------|
| Warstwy architektury | 4 (Onion) |
| Projekty .NET | 4 |
| Produkty w katalogu | 17 |
| Max iteracje Writer-Critic | 3 |
| Czas budowania Docker | ~20s |
| Czas odpowiedzi AI | 5-15s |
| Język interfejsu | Polski |

---



### Problem: "Aplikacja nie startuje"
**Rozwiązanie:**
```bash
docker compose down
docker compose up --build
```

### Problem: "Baza danych nie jest gotowa"
**Rozwiązanie:** Poczekaj 30 sekund - healthcheck czeka na MySQL

### Problem: "AI nie odpowiada"
**Rozwiązanie:** Sprawdź klucz OpenAI w pliku `.env`

### Problem: "Błąd 'port already in use'"
**Rozwiązanie:**
```bash
docker compose down
# Zmień port w docker-compose.yml: "5001:8080"
docker compose up
```
