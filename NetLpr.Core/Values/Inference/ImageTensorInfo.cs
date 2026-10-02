using System;
using System.Collections.Generic;
using System.Text;

namespace NetLpr.Core.Values.Inference
{
    public record class ImageTensorInfo(ImageTensorLayoutType ImageTensorLayoutType, int Batch,int Channel,int Width,int Height)
    {
        
    }

}
