using NetLpr.Core.Log;
using NetLpr.Core.Services.Inference.Pipelines;
using NetLpr.Core.Services.Inference.Pipelines.Ocr;
using NetLpr.Core.Services.Inference.Pipelines.Plate;
using NetLpr.Core.Services.InferenceModels.Sessions;
using NetLpr.Core.Values.Inference;
using NetLpr.Core.Values.Preprocessing;
using NetLpr.Core.Values.Request;
using NetLpr.InferenceOR;
using NetLpr.InferenceOR.InferenceModels;
using NetLpr.InferenceOV;
using NetLpr.InferenceOV.InferenceModels;


namespace NetLpr.Boot.Node
{
    public class PipelineFactory
    {

        public static IInferencePipeline? CreatePipelineFromOv(ILogger logger ,OpenVinoInferencePoolFactory poolFactory)
        {

            return CreatePlateBasedPipelineFromOv(logger, poolFactory);

        }
        public static IInferencePipeline? CreatePipelineFromOr(ILogger logger, OnnxModelFactory modelFactory)
        {

            return CreatePlateBasedPipelineFromOr(logger, modelFactory);

        }
        private static IInferencePipeline? CreatePlateBasedPipelineFromOv(ILogger logger, OpenVinoInferencePoolFactory poolFactory)
        {
            var inferencePipeline = new PlateBasedPipeline(
                logger,
                GetPlateDetectionModelFromOv(logger, poolFactory,.5f),
                 new PlateOcrWrapper(
                        logger,
                        GetOcrModelFromOv(logger, poolFactory)
                    )
            );
            return inferencePipeline;
        }

        private static IInferencePipeline? CreatePlateBasedPipelineFromOr(ILogger logger, OnnxModelFactory modelFactory)
        {
            var inferencePipeline = new PlateBasedPipeline(
                logger,
                GetPlateDetectionModelFromOr(logger, modelFactory, .5f),
                 new PlateOcrWrapper(
                        logger,
                        GetOcrModelFromOr(logger, modelFactory)
                    )
            );
            return inferencePipeline;
        }


        private static InferenceModel GetPlateDetectionModelFromOv(ILogger logger, OpenVinoInferencePoolFactory poolFactory,float threshold )
        {


            return new OpenVinoPlateDetModel(
                logger,
                poolFactory,
                GetPlateDetConfig(threshold)
            );
        }
        private static InferenceModel GetPlateDetectionModelFromOr(ILogger logger, OnnxModelFactory modelFactory, float threshold)
        {


            return new OnnxPlateDetectionSession(
                logger,
                modelFactory,
                GetPlateDetConfig(threshold)
            );
        }
        private static InferenceModel GetOcrModelFromOv(ILogger logger, OpenVinoInferencePoolFactory poolFactory)
        {
            

            return new OpenVinoOcrModel(
                logger,
                poolFactory,
                GetOcrConfig()
            );
        }
        private static InferenceModel GetOcrModelFromOr(ILogger logger, OnnxModelFactory modelFactory)
        {


            return new OnnxOcrSession(
                logger,
                modelFactory,
                GetOcrConfig()
            );
        }
        private static ModelConfig GetOcrConfig()
        {
            return new ModelConfig(
                maxProcessorCount: 4,
                modelPath: Path.GetFullPath("./inference-models/ocr/ocr-2_n_int8_64x128.onnx"),
                imageTensorName: "input",
                tensorInfo: new ImageTensorInfo(ImageTensorLayoutType.BHWC, 1, 3, 128, 64),
                normalize: Normalize.NONE,
                paddingColor: PaddingColor.Gray,
                pixelFormat: PixelFormat.Rgb24,
                scalingPolicy: ScalingPolicy.Stretch,
                responseConfig: new OcrResponseConfig(10, '_', "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ_", threshold: 0.5f)
            );
        }
        private static ModelConfig GetPlateDetConfig(float threshold)
        {
            return new ModelConfig(
                maxProcessorCount: 4,
                modelPath: Path.GetFullPath("./inference-models/ld/ld_n_fp16_384x384.onnx"),
                imageTensorName: "images",
                tensorInfo: new ImageTensorInfo(ImageTensorLayoutType.BCHW, 1, 3, 384, 384),
                normalize: Normalize.MIN_MAX,
                paddingColor: PaddingColor.Gray,
                pixelFormat: PixelFormat.Rgb24,
                scalingPolicy: ScalingPolicy.Stretch,
                responseConfig: new PlateDetectionResponseConfigs(
                    validChildClasses: new Dictionary<int, string>()
                    {
                        { 0,  "plate" }
                    },
                    threshold: threshold
                )
            ); 
        }
    }
}
