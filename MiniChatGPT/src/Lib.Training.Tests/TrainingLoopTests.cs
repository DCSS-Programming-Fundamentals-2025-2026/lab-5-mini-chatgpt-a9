using NUnit.Framework;
using Moq;
using Lib.Training;
using Lib.Training.Configuration;
using Lib.Training.Metrics;
using Lib.Training.Scheduling;
using System;

namespace Lib.Training.Tests
{
    [TestFixture]
    public class TrainingLoopTests
    {
        private TrainingConfig? _config;
        private Mock<IBatchProvider>? _mockBatchProvider;
        private Mock<TrainingMetrics>? _mockMetrics;
        private Mock<CheckpointScheduler>? _mockScheduler;

        [SetUp]
        public void Setup()
        {
            _config = new TrainingConfig { Epochs = 2, StepsPerEpoch = 5, LearningRate = 0.01f };

            _mockBatchProvider = new Mock<IBatchProvider>();
            _mockBatchProvider.Setup(b => b.GetNextBatch()).Returns(new Batch
            {
                Tokens = new[] { "test" },
                Context = new double[] { 1.0 },
                Target = new double[] { 0.5 }
            });

            _mockMetrics = new Mock<TrainingMetrics>();
            _mockScheduler = new Mock<CheckpointScheduler>();
        }

        [Test]
        public void NGramTrainingLoop_CallsTrain_And_TriggersScheduler()
        {
            // Arrange
            var mockModel = new Mock<INGramModel>();
            var loop = new TrainingLoopImpl(
                mockModel.Object,
                _mockBatchProvider!.Object,
                _config!,
                _mockMetrics!.Object,
                _mockScheduler!.Object);

            // Act
            loop.Run();

            // Assert
            mockModel.Verify(m => m.Train(It.IsAny<string[]>()), Times.Exactly(10));
            _mockScheduler.Verify(s => s.CheckAndSave(It.IsAny<int>(), mockModel.Object), Times.Exactly(2));
            _mockMetrics.Verify(m => m.RecordEpoch(It.IsAny<int>(), It.IsAny<double>()), Times.Exactly(2));
        }

        [Test]
        public void NeuralNetworkTrainingLoop_AccumulatesLoss()
        {
            // Arrange
            var mockModel = new Mock<INeuralNetworkModel>();
            mockModel.Setup(m => m.TrainStep(It.IsAny<double[]>(), It.IsAny<double[]>(), It.IsAny<float>()))
                     .Returns(0.5);

            var loop = new TrainingLoopImpl(
                mockModel.Object,
                _mockBatchProvider!.Object,
                _config!,
                _mockMetrics!.Object,
                null);

            // Act
            loop.Run();

            // Assert
            mockModel.Verify(m => m.TrainStep(It.IsAny<double[]>(), It.IsAny<double[]>(), _config!.LearningRate), Times.Exactly(10));
            _mockMetrics!.Verify(m => m.RecordEpoch(It.IsAny<int>(), 0.5), Times.Exactly(2));
        }
    }
}