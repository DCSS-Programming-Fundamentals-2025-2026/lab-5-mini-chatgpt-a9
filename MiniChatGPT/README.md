# MiniChatGPT - Lib.Training

> Гнучкий C# фреймворк для тренування мовних моделей на .NET 8.0

---

## Ключові особливості

### Безпечна типізація (Type Safety)

Видалено ключове слово `dynamic`. Впроваджено строгі інтерфейси `ILanguageModel`, `IBatchProvider`, `INGramModel` та `INeuralNetworkModel` для перевірки типів під час компіляції.

### Конфігурований цикл (Data-driven Limits)

Замість жорстко закодованих лімітів (наприклад, 100 батчів) цикл тепер спирається на конфігурацію `StepsPerEpoch` у класі `TrainingConfig`.

### Реальна логіка навчання

Тренувальний цикл коректно викликає методи `Train` (для N-Gram) або `TrainStep` (для NN), накопичує loss та записує його за допомогою метрик.

### Повноцінні unit-тести

Додано проєкт із 2+ unit-тестами для перевірки правильності викликів планувальника та накопичення метрик, без залежностей на Moq.

---

## 📂 Структура проєкту

```
MiniChatGPT/
├── Lib.Training.A9.sln                   # Основний Solution-файл
├── README.md                             # Документація
└── src/
    ├── Lib.Training/                     # Основна бібліотека
    │   ├── Lib.Training.csproj
    │   ├── CoreTypes.cs                  # Базові типи та інтерфейси
    │   ├── ITrainingLoop.cs              # Інтерфейс тренування
    │   ├── TrainingLoop.cs               # Фабрика для створення циклу
    │   ├── TrainingLoopImpl.cs            # Основна логіка навчання
    │   ├── Configuration/
    │   │   └── TrainingConfig.cs         # Параметри (Epochs, StepsPerEpoch, LearningRate)
    │   ├── Metrics/
    │   │   └── TrainingMetrics.cs        # Запис метрик (loss)
    │   └── Scheduling/
    │       └── CheckpointScheduler.cs    # Збереження чекпоінтів
    │
    └── Lib.Training.Tests/               # Unit-тести (NUnit)
        ├── Lib.Training.Tests.csproj
        └── TrainingLoopTests.cs          # Тести з fake об'єктами
```

---

## Швидкий старт

### Вимоги

- **.NET 8.0 SDK** або новіше

### Збірка проєкту

```bash
dotnet build
```

### Запуск тестів

```bash
dotnet test
```

---

## Приклад використання

```csharp
using Lib.Training;
using Lib.Training.Configuration;
using Lib.Training.Metrics;
using Lib.Training.Scheduling;

// 1️⃣ Налаштування гіперпараметрів навчання
var config = new TrainingConfig
{
    Epochs = 10,
    StepsPerEpoch = 100,      // Кількість батчів за епоху
    LearningRate = 0.001f
};

// 2️⃣ Ініціалізація компонентів
ILanguageModel model = new MyNeuralNetworkModel();
IBatchProvider batchProvider = new MyDatasetProvider();
var metrics = new TrainingMetrics();
var scheduler = new CheckpointScheduler();

// 3️⃣ Створення тренувального циклу
var trainingLoop = TrainingLoop.CreateDefault(
    model,
    batchProvider,
    config,
    metrics,
    scheduler
);

// 4️⃣ Запуск навчання
trainingLoop.Run();
```

---

## API Довідник

### TrainingConfig

Параметри конфігурації для навчання:

| Параметр        | Тип     | За замовчуванням | Опис                             |
| --------------- | ------- | ---------------- | -------------------------------- |
| `Epochs`        | `int`   | 10               | Кількість повних циклів по даним |
| `StepsPerEpoch` | `int`   | 100              | Кількість батчів за епоху        |
| `LearningRate`  | `float` | 0.001            | Крок оновлення ваг моделі        |

### Інтерфейси моделей

#### `ILanguageModel`

Базовий інтерфейс для всіх мовних моделей.

#### `INGramModel : ILanguageModel`

Для N-Gram моделей:

```csharp
void Train(string[] tokens);
```

#### `INeuralNetworkModel : ILanguageModel`

Для нейромереж:

```csharp
double TrainStep(double[] context, double[] target, float learningRate);
```

### Batch

Структура для представлення даних:

```csharp
public class Batch
{
    public string[] Tokens { get; set; }      // Масив токенів
    public double[] Context { get; set; }     // Вхідні дані (числові)
    public double[] Target { get; set; }      // Очікувані значення
}
```

---

## Покриття тестами

Проєкт включає **2 основні юніт-тести**:

### ✅ NGramTrainingLoop_CallsTrain_And_TriggersScheduler

- Перевіряє, що N-Gram модель викликає `Train()` потрібну кількість разів
- Перевіряє, що `CheckpointScheduler` викликається один раз за епоху
- Перевіряє запис метрик для кожної епохи

### ✅ NeuralNetworkTrainingLoop_AccumulatesLoss

- Перевіряє коректне накопичення loss значень
- Перевіряє використання `LearningRate` із конфігурації
- Перевіряє запис усереднених loss значень у метрики

**Технологія тестування:**

- Використовуються **прості fake об'єкти** замість фреймворків мокування
- Реалізовано 5 fake класів: `FakeBatchProvider`, `FakeNGramModel`, `FakeNeuralNetworkModel`, `FakeTrainingMetrics`, `FakeCheckpointScheduler`
- Мінімальна залежність від зовнішніх пакетів

---

## Розширення

Для створення власної моделі просто реалізуйте один з інтерфейсів:

### Приклад: N-Gram модель

```csharp
public class MyNGramModel : INGramModel
{
    public void Train(string[] tokens)
    {
        // Ваша логіка навчання
    }
}
```

### Приклад: Нейронна мережа

```csharp
public class MyNeuralNetwork : INeuralNetworkModel
{
    public double TrainStep(double[] context, double[] target, float learningRate)
    {
        // Ваша логіка навчання
        return calculatedLoss;
    }
}
```

---

## Результати тестування

```
    Складання успішно завершено 
    Попереджень: 0 
    Помилок: 0

    Тести пройдено: 2/2 
    Тривалість: 10 ms
```


```
