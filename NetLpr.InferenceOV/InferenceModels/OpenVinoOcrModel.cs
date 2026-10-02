using System.Drawing;
using NetLpr.Core;
using NetLpr.Core.Log;
using NetLpr.Core.Models;
using NetLpr.Core.Values.Inference;
using OpenVinoSharp;


namespace NetLpr.InferenceOV.InferenceModels
{
    public class OpenVinoOcrModel : OpenVinoModel<byte>
    {

        private ILogger _logger;
        public OpenVinoOcrModel(ILogger logger, OpenVinoInferencePoolFactory poolFactory, ModelConfig modelConfig) : base(logger,poolFactory,modelConfig) {
            _logger = logger;
        }
        protected override InferenceResult ResponseToDetectionResult(InferRequest inferenceRequest, Size originalSize)
        {
            try
            {

                var ocrConfig = ModelConfig.GetInferenceModelResponseConfig<OcrResponseConfig>();
                
                if(ocrConfig == null)
                    return InferenceResult.Empty;

                var threshold = ocrConfig.Threshold;
                int numClasses = ocrConfig.OcrAlphabet.Length;
                var outputTensor = inferenceRequest.get_output_tensor(0);
                float[] data = outputTensor.get_data<float>((int)outputTensor.get_size());
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
                if(finalConf >= threshold)
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
                        var regionTensor = inferenceRequest.get_output_tensor(1);
                        float[] regionsResult = regionTensor.get_data<float>((int)regionTensor.get_size());
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
                    return inferenceResponse;
                }
                else 
                    return InferenceResult.Empty;
            }
            catch (Exception ex) 
            {
                _logger.LogError("CCTPOCRSession: response cannot parse!", ex);
                return InferenceResult.Empty;
            }
        }

        protected override List<InferenceResult> ResponseToDetectionResults(InferRequest result, Size originalSize)
        {
            return new()
            {
                ResponseToDetectionResult(result,originalSize)
            };
        }
    }
}
