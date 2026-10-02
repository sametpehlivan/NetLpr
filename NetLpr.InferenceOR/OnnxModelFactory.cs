using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using NetLpr.Core.Log;
using NetLpr.Core.Values.Inference;
using Microsoft.ML.OnnxRuntime;

namespace NetLpr.InferenceOR
{
    public class OnnxModelFactory 
    {
        public ILogger _logger;
        public OnnxModelFactory(ILogger logger)
        {
            _logger = logger;
        }
        public InferenceSession CreateSingleThreadedSession(ModelConfig modelConfig)
        {
            var opts = new SessionOptions
            {
                IntraOpNumThreads = 1,
                InterOpNumThreads = 1,

                ExecutionMode = ExecutionMode.ORT_SEQUENTIAL,
                GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL
            };
            opts.AddSessionConfigEntry("session.intra_op.allow_spinning", "0");
            opts.AddSessionConfigEntry("session.inter_op.allow_spinning", "0");

            opts.AddSessionConfigEntry("session.use_arena", "1");
            opts.AddSessionConfigEntry("session.force_sequential_execution", "1");

            try
            {
                return new InferenceSession(modelConfig.ModelPath, opts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Single-threaded ONNX Session oluşturulamadı: {modelConfig.ModelPath}");
                throw;
            }
        }
    }
}