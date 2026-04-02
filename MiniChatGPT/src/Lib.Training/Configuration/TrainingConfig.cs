namespace Lib.Training.Configuration
{
    public class TrainingConfig
    {
        public int Epochs { get; set; } = 10;
        public int StepsPerEpoch { get; set; } = 100;
        public float LearningRate { get; set; } = 0.001f;
    }
}