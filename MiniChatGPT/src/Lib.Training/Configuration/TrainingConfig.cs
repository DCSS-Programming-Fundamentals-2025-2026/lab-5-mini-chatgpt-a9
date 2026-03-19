namespace Lib.Training.Configuration
{
    public class TrainingConfig
    {
        public int Epochs { get; set; } = 10;
        public int CheckpointCadence { get; set; } = 5;
        public float LearningRate { get; set; } = 0.05f;
        public int BatchSize { get; set; } = 32;
        public int BlockSize { get; set; } = 8;
    }
}