using Lib.Training.Configuration;
using Lib.Training.Metrics;

namespace Lib.Training
{
    public interface ITrainingLoop
    {
        TrainingMetrics Train(dynamic model, dynamic batchProvider, TrainingConfig config);
    }
}