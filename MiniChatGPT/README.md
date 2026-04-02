# MiniChatGPT - Lib.Training

**MiniChatGPT (Lib.Training)** — це C#-бібліотека для платформи **.NET 8.0**, яка надає гнучкий фреймворк для створення та запуску тренувальних циклів (training loops) для мовних моделей. Бібліотека підтримує як базові моделі на основі N-Gram, так і прості нейронні мережі (Neural Networks).

## 🛠 Реалізовані покращення (Refactoring)

У проєкті було виконано глибокий рефакторинг згідно з кращими практиками:

- **Безпечна типізація (Type Safety):** Видалено ключове слово `dynamic`. [cite_start]Впроваджено строгі інтерфейси `ILanguageModel`, `IBatchProvider`, `INGramModel` та `INeuralNetworkModel` для перевірки типів під час компіляції.
- [cite_start]**Конфігурований цикл (Data-driven Limits):** Замість жорстко закодованих лімітів (наприклад, 100 батчів) цикл тепер спирається на конфігурацію `StepsPerEpoch` у класі `TrainingConfig`[cite: 221].
- [cite_start]**Реальна логіка навчання:** Тренувальний цикл коректно викликає методи `Train` (для N-Gram) або `TrainStep` (для NN), накопичує loss та записує його за допомогою метрик[cite: 222, 227].
- [cite_start]**Повноцінні тести:** Додано проєкт із unit-тестами для перевірки правильності викликів планувальника та накопичення метрик.

## 📂 Структура проєкту

```text
MiniChatGPT/
├── Lib.Training.A9.sln                # Основний Solution-файл
├── README.md                          # Документація проєкту
└── src/
    ├── Lib.Training/                  # Основна бібліотека
    │   ├── Lib.Training.csproj
    │   ├── CoreTypes.cs               # Базові типи (Batch, IBatchProvider, інтерфейси моделей)
    │   ├── ITrainingLoop.cs           # Інтерфейс тренувального циклу
    │   ├── TrainingLoop.cs            # Фабрика для створення циклу
    │   ├── TrainingLoopImpl.cs        # Головна реалізація логіки навчання
    │   ├── Configuration/
    │   │   └── TrainingConfig.cs      # Параметри навчання (Epochs, StepsPerEpoch, LearningRate)
    │   ├── Metrics/
    │   │   └── TrainingMetrics.cs     # Клас для запису показників (loss)
    │   └── Scheduling/
    │       └── CheckpointScheduler.cs # Клас збереження чекпоінтів
    │
    └── Lib.Training.Tests/            # Проєкт із юніт-тестами (NUnit / Moq)
        └── ...                        # Файли тестів та залежності

Як зібрати та запустити

Для роботи з проєктом вам знадобиться .NET 8.0 SDK.

1. Збірка проєкту:
Відкрийте термінал у кореневій папці з файлом .sln і виконайте:
dotnet build

2. Запуск тестів:
Щоб перевірити коректність логіки навчання, виконайте:
dotnet test

Приклад використання
using Lib.Training;
using Lib.Training.Configuration;
using Lib.Training.Metrics;
using Lib.Training.Scheduling;

// 1. Налаштування гіперпараметрів навчання
var config = new TrainingConfig
{
    Epochs = 10,
    StepsPerEpoch = 100, // Управляє кількістю кроків за епоху
    LearningRate = 0.001f
};

// 2. Ініціалізація власних компонентів клієнта
ILanguageModel model = new MyNeuralNetworkModel();
IBatchProvider batchProvider = new MyDatasetProvider();
var metrics = new TrainingMetrics();
var scheduler = new CheckpointScheduler();

// 3. Створення тренувального циклу через фабрику
var trainingLoop = TrainingLoop.CreateDefault(
    model,
    batchProvider,
    config,
    metrics,
    scheduler
);

// 4. Запуск навчання
trainingLoop.Run();

API Довідник

TrainingConfig
Клас конфігурації містить параметри:

Epochs: Кількість загальних циклів по даним (за замовчуванням: 10).

StepsPerEpoch: Кількість батчів, які обробляються за 1 епоху (за замовчуванням: 100).

LearningRate: Крок оновлення ваг моделі (за замовчуванням: 0.001).


Інтерфейси моделей (CoreTypes.cs)
ILanguageModel: Загальний базовий інтерфейс.

INGramModel: Для лінгвістичних моделей N-Gram (метод Train(string[] tokens)).

INeuralNetworkModel: Для нейромереж (метод TrainStep(double[] context, double[] target, float learningRate)), що повертає loss.


Batch
Клас представлення одного пакета даних:

Tokens: Масив строкових токенів.

Context: Вхідний масив числових даних (double[]).

Target: Очікуваний масив значень (double[]).


Test Coverage (Покриття тестами)
Проєкт Lib.Training.Tests містить перевірки, які гарантують:

N-Gram модель коректно викликає метод Train задану конфігурацією кількість разів, а CheckpointScheduler зберігає стан.

Модель Neural Network успішно використовує LearningRate під час викликів TrainStep та коректно накопичує loss для TrainingMetrics.

Викидання помилок при спробі передати в TrainingLoopImpl невідомий тип моделі.
```
