using System.Collections.Generic;

namespace Lib.Training.Metrics
{
    public class TrainingMetrics
    {
        public int TotalEpochsCompleted { get; set; }
        public double FinalAverageLoss { get; set; }

        public Dictionary<int, double> LossHistory { get; set; } = new Dictionary<int, double>();
    }
}