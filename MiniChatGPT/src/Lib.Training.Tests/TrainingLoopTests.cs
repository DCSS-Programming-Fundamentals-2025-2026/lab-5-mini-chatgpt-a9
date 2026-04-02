using NUnit.Framework;
using Lib.Training;
using Lib.Training.Configuration;
using Lib.Training.Metrics;
using Lib.Training.Scheduling;
using System;

namespace Lib.Training.Tests
{
    // Test doubles (fake implementations)
    public class FakeBatchProvider : IBatchProvider
    {
        private int _batchCount = 0;
        public int BatchesReturned { get; private set; } = 0;

        public Batch? GetNextBatch()
        {
            if (_batchCount >= 100) // Max 100 batches per run
                return null;

            _batchCount++;
            BatchesReturned++;
            return new Batch
            {
                Tokens = new[] { "test" },
                Context = new double[] { 1.0 },
                Target = new double[] { 0.5 }
            };
        }

        public void Reset() => _batchCount = 0;
    }

    public class FakeNGramModel : INGramModel
    {
        public int TrainCallCount { get; private set; } = 0;

        public void Train(string[] tokens)
        {
            TrainCallCount++;
        }
    }

    public class FakeNeuralNetworkModel : INeuralNetworkModel
    {
        public int TrainStepCallCount { get; private set; } = 0;

        public double TrainStep(double[] context, double[] target, float learningRate)
        {
            TrainStepCallCount++;
            return 0.5; // Fixed loss value for testing
        }
    }

    public class FakeTrainingMetrics : TrainingMetrics
    {
        public int RecordEpochCallCount { get; private set; } = 0;
        public List<(int epoch, double loss)> RecordedEpochs { get; } = new();

        public override void RecordEpoch(int epoch, double averageLoss)
        {
            RecordEpochCallCount++;
            RecordedEpochs.Add((epoch, averageLoss));
        }
    }

    public class FakeCheckpointScheduler : CheckpointScheduler
    {
        public int CheckAndSaveCallCount { get; private set; } = 0;
        public List<(int epoch, ILanguageModel model)> SavedCheckpoints { get; } = new();

        public override void CheckAndSave(int epoch, ILanguageModel model)
        {
            CheckAndSaveCallCount++;
            SavedCheckpoints.Add((epoch, model));
        }
    }

    [TestFixture]
    public class TrainingLoopTests
    {
        private TrainingConfig? _config;
        private FakeBatchProvider? _batchProvider;
        private FakeTrainingMetrics? _metrics;
        private FakeCheckpointScheduler? _scheduler;

        [SetUp]
        public void Setup()
        {
            _config = new TrainingConfig { Epochs = 2, StepsPerEpoch = 5, LearningRate = 0.01f };
            _batchProvider = new FakeBatchProvider();
            _metrics = new FakeTrainingMetrics();
            _scheduler = new FakeCheckpointScheduler();
        }

        [Test]
        public void NGramTrainingLoop_CallsTrain_And_TriggersScheduler()
        {
            // Arrange
            var ngramModel = new FakeNGramModel();
            var loop = new TrainingLoopImpl(
                ngramModel,
                _batchProvider!,
                _config!,
                _metrics!,
                _scheduler!);

            // Act
            loop.Run();

            // Assert
            Assert.That(ngramModel.TrainCallCount, Is.EqualTo(10),
                "NGram model Train should be called 10 times (2 epochs × 5 steps per epoch)");
            Assert.That(_scheduler!.CheckAndSaveCallCount, Is.EqualTo(2),
                "Checkpoint scheduler should be called 2 times (once per epoch)");
            Assert.That(_metrics!.RecordEpochCallCount, Is.EqualTo(2),
                "Metrics should record 2 epochs");
        }

        [Test]
        public void NeuralNetworkTrainingLoop_AccumulatesLoss()
        {
            // Arrange
            var nnModel = new FakeNeuralNetworkModel();
            var loop = new TrainingLoopImpl(
                nnModel,
                _batchProvider!,
                _config!,
                _metrics!,
                null);

            // Act
            loop.Run();

            // Assert
            Assert.That(nnModel.TrainStepCallCount, Is.EqualTo(10),
                "Neural network TrainStep should be called 10 times (2 epochs × 5 steps per epoch)");
            Assert.That(_metrics!.RecordEpochCallCount, Is.EqualTo(2),
                "Metrics should record 2 epochs");

            // Verify that metrics recorded the correct average loss (0.5 for all batches)
            Assert.That(_metrics!.RecordedEpochs[0].loss, Is.EqualTo(0.5),
                "Average loss for first epoch should be 0.5");
            Assert.That(_metrics!.RecordedEpochs[1].loss, Is.EqualTo(0.5),
                "Average loss for second epoch should be 0.5");
        }
    }
}