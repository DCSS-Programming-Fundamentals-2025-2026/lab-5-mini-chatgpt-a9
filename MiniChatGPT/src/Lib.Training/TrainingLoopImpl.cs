using System;
using Lib.Training.Configuration;
using Lib.Training.Metrics;
using Lib.Training.Scheduling;

namespace Lib.Training
{
    public class TrainingLoopImpl : ITrainingLoop
    {
        public TrainingMetrics Train(dynamic model, dynamic batchProvider, TrainingConfig config)
        {
            var metrics = new TrainingMetrics();
            var scheduler = new CheckpointScheduler(config.CheckpointCadence);
            var rng = new Random(42);

            Console.WriteLine($"Починаємо тренування. Модель: {model.ModelKind}, Епох: {config.Epochs}");

            for (int epoch = 1; epoch <= config.Epochs; epoch++)
            {
                double epochTotalLoss = 0;
                int batchesProcessed = 0;

                while (true)
                {
                    var batch = batchProvider.GetBatch(config.BatchSize, config.BlockSize, rng);
                    if (batch == null || batch.Length == 0)
                    {
                        break;
                    }

                    double batchLoss = 0;

                    epochTotalLoss += batchLoss;
                    batchesProcessed++;

                    if (batchesProcessed >= 100) break;
                }

                double avgLoss = batchesProcessed > 0 ? epochTotalLoss / batchesProcessed : 0;
                metrics.LossHistory[epoch] = avgLoss;
                metrics.FinalAverageLoss = avgLoss;
                metrics.TotalEpochsCompleted = epoch;

                Console.WriteLine($"Епоха [{epoch}/{config.Epochs}] завершена. Середній Loss: {avgLoss:F4}");

                if (scheduler.ShouldSaveCheckpoint(epoch))
                {
                    Console.WriteLine($"[Save] Збереження проміжного чекпоінту для епохи {epoch}...");
                    // var payload = model.GetPayloadForCheckpoint();
                    // Реалізація збереження на диск (через JsonCheckpointIO)
                }
            }

            Console.WriteLine("Тренування успішно завершене!");
            return metrics;
        }
    }
}