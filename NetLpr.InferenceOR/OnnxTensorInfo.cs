using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NetLpr.Core.Values.Inference;
using NetLpr.Core.Values.Preprocessing;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace NetLpr.InferenceOR
{
    public class OnnxTensorInfo<T> : IDisposable where T : unmanaged
    {
        public List<NamedOnnxValue> Tensors { get; private set; }
        public T[] ArrayData { get; private set; }
        public InferenceSession ModelSession { get; private set; }
        public OnnxTensorInfo(ModelConfig modelConfig,InferenceSession model)
        {
            ModelSession = model;
            int elementCount = modelConfig.ImageTensorInfo.Batch *
                               modelConfig.ImageTensorInfo.Channel *
                               modelConfig.ImageTensorInfo.Width *
                               modelConfig.ImageTensorInfo.Height;

            ArrayData = new T[elementCount];
            Tensors = new List<NamedOnnxValue>()
            {
                NamedOnnxValue.CreateFromTensor(
                    modelConfig.ImageTensorName, 
                    new DenseTensor<T>(ArrayData, modelConfig.GetImageTensorDimensionInt()))
            };
        }


        public unsafe T* GetBufferPointer()
        {
            return (T*)Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(ArrayData));
        }

        public void Dispose()
        {
            ModelSession.Dispose();
        }
    }
   
    
}
