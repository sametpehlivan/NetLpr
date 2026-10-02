using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetLpr.Core.Extensions;
using NetLpr.Core.Log;
using NetLpr.Core.Models;
using NetLpr.Core.Values.Inference;
using Microsoft.ML.OnnxRuntime;

namespace NetLpr.InferenceOR.InferenceModels
{
    public class OnnxPlateDetectionSession : OnnxModelSession<float>
    {
        public OnnxPlateDetectionSession(ILogger logger, OnnxModelFactory modelFactory, ModelConfig modelConfig) : base(logger, modelFactory, modelConfig)
        {
        }

        protected override InferenceResult? ResponseToDetectionResult(IDisposableReadOnlyCollection<DisposableNamedOnnxValue> result, Size originalSize)
        {
            return ResponseToDetectionResults(result, originalSize).OrderByDescending(x => x.Score).FirstOrDefault();

        }

        protected override List<InferenceResult> ResponseToDetectionResults(IDisposableReadOnlyCollection<DisposableNamedOnnxValue> result, Size originalSize)
        {
            try
            {
                var config = ModelConfig.GetInferenceModelResponseConfig<PlateDetectionResponseConfigs>();
                if (config == null)
                    return new();

                var outputTensor = result.First();
                float[] data = outputTensor.AsTensor<float>().ToArray();
                var results = new List<InferenceResult>();
                var threshold = config.Threshold;
                int foundObjects = 0;
                int rowCount = data.Length / config.ColumnCount;
                for (int i = 0; i < rowCount; i++)
                {
                    int cls = (int)data[i * config.ColumnCount + 5];
                    float conf = data[i * config.ColumnCount + 6];
                    if (conf >= threshold && config.ValidChildClasses.ContainsKey(cls))
                    {
                        results.Add(new InferenceResult
                        {
                            X1 = (int)data[i * config.ColumnCount + 1],
                            Y1 = (int)data[i * config.ColumnCount + 2],
                            X2 = (int)data[i * config.ColumnCount + 3],
                            Y2 = (int)data[i * config.ColumnCount + 4],
                            Score = conf,
                            Label = config.GenericClassName,
                            ReelLabel = config.ValidChildClasses[cls],
                            ClassId = cls,
                        });
                        foundObjects++;
                        if (foundObjects >= config.DetectMaxObjectPerFrame)
                            break;
                    }
                }
                return results
                    .CalculateOriginalBox(
                        originalSize,
                        ModelConfig.ImageSize,
                        ModelConfig.ScalingPolicy
                    )
                    .OrderByDescending(x => x.Score)
                    .ToList();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "YOLPlateDetectionSession: response cannot parse.");
                return new();
            }
        }
    }
}
