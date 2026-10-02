using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetLpr.Core.Services.Tracking
{
    public interface IKalmanFilter : IDisposable
    {

        void InitializeState(float x, float y, float s, float r);
        float[] Predict();
        void Correct(float x, float y, float s, float r);
        float[] GetState();
    }
}
