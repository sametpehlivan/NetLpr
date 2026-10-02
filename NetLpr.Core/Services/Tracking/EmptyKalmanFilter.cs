using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetLpr.Core.Services.Tracking
{
    public class EmptyKalmanFilter : IKalmanFilter
    {
        float x = 0;
        float y = 0;
        float s = 0; 
        float r = 0;
        public void Correct(float x, float y, float s, float r)
        {
            this.x = x;
            this.y = y;
            this.s = s;
            this.r = r;

        }

        public void Dispose()
        {
            
        }

        public float[] GetState()
        {
            return new float[] { x,y,s,r};
        }

        public void InitializeState(float x, float y, float s, float r)
        {
            this.x=x;
            this.y=y;
            this.s=s;
            this.r=r;
        }

        public float[] Predict()
        {
            return GetState();
        }
    }
}
