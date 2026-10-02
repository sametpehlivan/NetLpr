using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetLpr.Core.Log;
using NetLpr.Core.Models;
using NetLpr.Core.Values.Inference;
using Microsoft.ML.OnnxRuntime;

namespace NetLpr.InferenceOR.InferenceModels
{
    public class OnnxOcrSession : OnnxModelSession<byte>
    {
        public OnnxOcrSession(ILogger logger, OnnxModelFactory modelFactory, ModelConfig modelConfig) : base(logger, modelFactory, modelConfig)
        {
        }

        protected override InferenceResult ResponseToDetectionResult(IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results, Size originalSize)
        {
            try
            {
                var ocrConfig = ModelConfig.GetInferenceModelResponseConfig<OcrResponseConfig>();

                if (ocrConfig == null)
                    return InferenceResult.Empty;

                var threshold = ocrConfig.Threshold;
                int numClasses = ocrConfig.OcrAlphabet.Length;

                // ONNX Runtime: İlk çıktı Tensor'ünü alma
                var outputValue = results.ElementAt(0);
                if (outputValue == null)
                    return InferenceResult.Empty;


                var tensor = outputValue.AsTensor<float>();
                var data = tensor.ToArray();
                float totalConf = 0f;
                int validCharCount = 0;
                char[] chars = new char[ocrConfig.MaxSlotSize];
                float[] charConfs = new float[ocrConfig.MaxSlotSize];

                for (int t = 0; t < ocrConfig.MaxSlotSize; t++)
                {
                    int offset = t * numClasses;
                    int maxIdx = 0;
                    float maxVal = data[offset];

                    for (int c = 1; c < numClasses; c++)
                    {
                        if (data[offset + c] > maxVal)
                        {
                            maxVal = data[offset + c];
                            maxIdx = c;
                        }
                    }

                    chars[t] = ocrConfig.OcrAlphabet[maxIdx];
                    charConfs[t] = maxVal;

                    if (chars[t] != ocrConfig.PaddingChar)
                    {
                        totalConf += maxVal;
                        validCharCount++;
                    }
                }

                string plate = new string(chars);
                float finalConf = validCharCount > 0 ? totalConf / validCharCount : 0.0f;

                if (finalConf >= threshold)
                {
                    InferenceResult inferenceResponse = new()
                    {
                        Label = plate,
                        ReelLabel = plate,
                        Score = finalConf,
                        ClassId = 0
                    };

                    if (ocrConfig.IsRegionExistsOnInference)
                    {
                        // ONNX Runtime: İkinci çıktı (Region Tensor) alma
                        var regionValue = results.ElementAtOrDefault(1);
                        if (regionValue != null)
                        {
                            var regionsResult = regionValue.AsTensor<float>().ToArray();
                            int index = 0;
                            float regionConf = 0f;

                            for (int i = 0; i < regionsResult.Length; i++)
                            {
                                if (regionsResult[i] > regionConf)
                                {
                                    index = i;
                                    regionConf = regionsResult[i];
                                }
                            }

                            inferenceResponse.AddInformation("region", ocrConfig.Regions[index]);
                            inferenceResponse.AddInformation("region_conf", regionConf);
                        }
                    }

                    return inferenceResponse;
                }
                else
                {
                    return InferenceResult.Empty;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("CCTPOCRSession: response cannot parse!", ex);
                return InferenceResult.Empty;
            }
        }

        protected override List<InferenceResult> ResponseToDetectionResults(IDisposableReadOnlyCollection<DisposableNamedOnnxValue> result, Size originalSize)
        {
            return new List<InferenceResult> { ResponseToDetectionResult(result, originalSize) };
        }
    }
}
