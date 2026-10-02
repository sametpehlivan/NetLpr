using System.Drawing;
using NetLpr.Core.Extensions;
using NetLpr.Core.Log;
using NetLpr.Core.Models;
using NetLpr.Core.Values.Inference;
using OpenVinoSharp;

namespace NetLpr.InferenceOV.InferenceModels
{
    public class OpenVinoPlateDetModel : OpenVinoModel<float> 
    {

        private ILogger _logger;
        public OpenVinoPlateDetModel(ILogger logger, OpenVinoInferencePoolFactory poolFactory, ModelConfig modelConfig) : base(logger, poolFactory, modelConfig) 
        {
            _logger = logger;
        }
        protected override InferenceResult? ResponseToDetectionResult(InferRequest inferenceRequest, Size originalSize)
        {
           return ResponseToDetectionResults(inferenceRequest,originalSize).OrderByDescending(x => x.Score).FirstOrDefault();
        }
        protected override List<InferenceResult> ResponseToDetectionResults(InferRequest inferenceRequest, Size originalSize)
        {
            try
            {
                var config = ModelConfig.GetInferenceModelResponseConfig<PlateDetectionResponseConfigs>();
                if (config == null)
                    return new();

                var outputTensor = inferenceRequest.get_output_tensor(0);
                float[] data = outputTensor.get_data<float>((int)outputTensor.get_size());
                var results = new List<InferenceResult>();
                var threshold = config.Threshold;
                int foundObjects = 0;
                int rowCount = data.Length / config.ColumnCount;
                for (int i = 0; i < rowCount; i++)
                {
                    int cls = (int)data[i * config.ColumnCount + 5];
                    float conf = data[i * config.ColumnCount + 6];
                    if (conf >= threshold &&  config.ValidChildClasses.ContainsKey(cls))
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
            }catch(Exception e)
            {
                _logger.LogError(e, "YOLPlateDetectionSession: response cannot parse.");
                return new();
            }
        }
    }
}
