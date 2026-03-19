namespace Lib.Training.Scheduling
{
    public class CheckpointScheduler
    {
        private readonly int _cadence;

        public CheckpointScheduler(int cadence)
        {
            _cadence = cadence > 0 ? cadence : 1;
        }

        public bool ShouldSaveCheckpoint(int currentEpoch)
        {
            return currentEpoch > 0 && currentEpoch % _cadence == 0;
        }
    }
}