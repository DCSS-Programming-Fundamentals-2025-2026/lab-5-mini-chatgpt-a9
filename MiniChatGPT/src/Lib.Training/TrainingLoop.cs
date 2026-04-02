using Lib.Training.Configuration;
using Lib.Training.Metrics;
using Lib.Training.Scheduling;

namespace Lib.Training
{
    public static class TrainingLoop
    {
        public static ITrainingLoop CreateDefault(
            ILanguageModel model,
            IBatchProvider batchProvider,
            TrainingConfig config,
            TrainingMetrics? metrics = null,
            CheckpointScheduler? scheduler = null)
        {
            return new TrainingLoopImpl(model, batchProvider, config, metrics, scheduler);
        }
    }
}