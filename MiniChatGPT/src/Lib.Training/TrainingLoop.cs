using Lib.Training.Configuration;
using Lib.Training.Metrics;

namespace Lib.Training
{
    public static class TrainingLoop
    {
        public static ITrainingLoop CreateDefault()
        {
            return new TrainingLoopImpl();
        }
    }
}