
using System.Drawing;
using NetLpr.Core.Values.Inference;
using OpenCvSharp;

namespace NetLpr.ImageProcessingCv
{

    public class PreprocessToTensorPointer
    {
        public static unsafe void PreprocessAndTransformTensor<T>(Image wrapper, void* tensorPointer, ModelConfig modelConfig,RectangleF? roi = null) where T : unmanaged
        {
            if (wrapper.IsDisposedOrEmpty())
                return;
            if (typeof(T) != typeof(float) && typeof(T) != typeof(byte))
            {
                return;
            }
            Mat? image = null;
            try
            {

                image = PreprocessingUtil.PreprocessImage(wrapper.GetPixelFormat(),wrapper.Mat, modelConfig, roi);
                if (modelConfig.ImageTensorInfo.ImageTensorLayoutType.Equals(ImageTensorLayoutType.BCHW))
                {
                    MatToTensorBCHW<T>(image.Data.ToPointer(), tensorPointer, image.Width, image.Height);
                    return;
                }
                if (modelConfig.ImageTensorInfo.ImageTensorLayoutType.Equals(ImageTensorLayoutType.BHWC))
                {
                    MatToTensorBHWC<T>(image.Data.ToPointer(), tensorPointer, image.Width, image.Height, image.Step());
                    return;
                }
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                image?.Dispose();
            }


        }
        private unsafe static void MatToTensorBCHW<T>(void* imagePointer,void* tensorPointer,int width,int height) where T : unmanaged
        {
            long size = (long)width * height * 1 * sizeof(T);

            Buffer.MemoryCopy(
                imagePointer,
                tensorPointer,
                size,
                size);
        }
        private unsafe static void MatToTensorBHWC<T>(void* imgPointer,void* tensorPtr,int width,int height,long srcStep) where T : unmanaged
        {
            long size = (long)width * height * 3 * sizeof(T);

            Buffer.MemoryCopy(
                imgPointer,
                tensorPtr,
                size,
                size);
        }

    }
}
