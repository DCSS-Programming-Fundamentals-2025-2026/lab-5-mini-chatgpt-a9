using System;

namespace Lib.Training
{
    public class Batch
    {
        public string[] Tokens { get; set; } = Array.Empty<string>();
        public double[] Context { get; set; } = Array.Empty<double>();
        public double[] Target { get; set; } = Array.Empty<double>();
    }

    public interface IBatchProvider
    {
        Batch? GetNextBatch();
    }

    public interface ILanguageModel { }

    public interface INGramModel : ILanguageModel
    {
        void Train(string[] tokens);
    }

    public interface INeuralNetworkModel : ILanguageModel
    {
        double TrainStep(double[] context, double[] target, float learningRate);
    }
}