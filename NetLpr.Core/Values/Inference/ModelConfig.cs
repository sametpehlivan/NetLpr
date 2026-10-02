using System.Drawing;
using NetLpr.Core.Values.Preprocessing;

namespace NetLpr.Core.Values.Inference
{

    public record ModelConfig
    {
        public int MaxProcessorCount { get; }
        public Normalize Normalize { get; }
        public string ModelPath { get; }
        public string ImageTensorName { get; }
        public PaddingColor PaddingColor { get; }
        public ImageTensorInfo ImageTensorInfo { get; }
        public Size ImageSize { get; }
        public PixelFormat PixelFormat { get; }
        public ScalingPolicy ScalingPolicy { get; }
        public object InferenceResponseConfig { get;  }
        public ModelConfig(
            int maxProcessorCount,
            string modelPath,
            string imageTensorName,
            ImageTensorInfo tensorInfo,
            Normalize normalize,
            PaddingColor paddingColor,
            PixelFormat pixelFormat,
            ScalingPolicy scalingPolicy,
            object responseConfig
        )
        {
            MaxProcessorCount = maxProcessorCount;
            Normalize = normalize;
            ModelPath = modelPath;
            ImageTensorName = imageTensorName;
            PaddingColor = paddingColor;
            ImageTensorInfo = tensorInfo;
            ImageSize = new Size(tensorInfo.Width, tensorInfo.Height);
            PixelFormat = pixelFormat;
            InferenceResponseConfig = responseConfig;
            ScalingPolicy = scalingPolicy;

        }
        public void Validate()
        {
            var errors = new List<string>();
            if (GetImageTensorDimensionInt().Count() == 0)
                errors.Add("modelConfig.error.modelShapeCannotBeNull");
            if (!File.Exists(ModelPath))
                errors.Add("modelConfig.error.modelPathNotFound");
        }


        public int[] GetImageTensorDimensionInt()
        {
            return ImageTensorInfo.ImageTensorLayoutType switch
            {

                ImageTensorLayoutType.BHWC => new int[] { ImageTensorInfo.Batch, ImageTensorInfo.Height, ImageTensorInfo.Width, ImageTensorInfo.Channel },
                _ => new int[] { ImageTensorInfo.Batch, ImageTensorInfo.Channel, ImageTensorInfo.Width, ImageTensorInfo.Height }
            };
        }
        public T? GetInferenceModelResponseConfig<T>() where T : class
        {
            if (InferenceResponseConfig is T config)
                return config;
            return null;
        }

    }



}
