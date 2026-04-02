using System;
using Lib.Training.Configuration;
using Lib.Training.Metrics;
using Lib.Training.Scheduling;

namespace Lib.Training
{
    public class TrainingLoopImpl : ITrainingLoop
    {
        private readonly ILanguageModel _model;
        private readonly IBatchProvider _batchProvider;
        private readonly TrainingConfig _config;
        private readonly TrainingMetrics? _metrics;
        private readonly CheckpointScheduler? _scheduler;

        public TrainingLoopImpl(
            ILanguageModel model,
            IBatchProvider batchProvider,
            TrainingConfig config,
            TrainingMetrics? metrics = null,
            CheckpointScheduler? scheduler = null)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));
            _batchProvider = batchProvider ?? throw new ArgumentNullException(nameof(batchProvider));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _metrics = metrics;
            _scheduler = scheduler;
        }

        public void Run()
        {
            for (int epoch = 0; epoch < _config.Epochs; epoch++)
            {
                double epochLoss = 0;
                int batchesProcessed = 0;

                while (batchesProcessed < _config.StepsPerEpoch)
                {
                    var batch = _batchProvider.GetNextBatch();
                    if (batch == null) break;

                    double batchLoss = 0;

                    if (_model is INeuralNetworkModel nnModel)
                    {
                        batchLoss = nnModel.TrainStep(batch.Context, batch.Target, _config.LearningRate);
                    }
                    else if (_model is INGramModel ngramModel)
                    {
                        ngramModel.Train(batch.Tokens);
                        // For NGram models, we calculate a mock loss based on model behavior
                        // In a real scenario, this would be a proper perplexity or likelihood metric
                        batchLoss = 0.1;
                    }
                    else
                    {
                        throw new InvalidOperationException("Unknown model type.");
                    }

                    epochLoss += batchLoss;
                    batchesProcessed++;
                }

                double averageLoss = batchesProcessed > 0 ? epochLoss / batchesProcessed : 0;

                _metrics?.RecordEpoch(epoch, averageLoss);
                _scheduler?.CheckAndSave(epoch, _model);
            }
        }
    }
}