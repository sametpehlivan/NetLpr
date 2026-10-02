using System;
using System.Collections.Generic;
using System.Text;

namespace NetLpr.Core.Values.Preprocessing
{
    public class Mean
    {
        public float B { get; private set; }
        public float G { get; private set; }
        public float R { get; private set; }
        public float A { get; private set; }
        public Mean(float valB, float valG, float valR, float valA = 0)
        {
            B = valB;
            G = valG;
            R = valR;
            A = valA;
        }
    }
    public class Std
    {
        public float B { get; private set; }
        public float G { get; private set; }
        public float R { get; private set; }
        public float A { get; private set; }
        public Std(float valB, float valG, float valR, float valA = 0)
        {
            B = valB;
            G = valG;
            R = valR;
            A = valA;
        }
    }
    public class Normalize
    {
        public static readonly Normalize IMAGE_NET = new Normalize(
                    new Mean(0.406f, 0.456f, 0.485f, 0f),
                    new Std(0.225f, 0.224f, 0.229f, 1f)
                );

        public static readonly Normalize MIN_MAX = new Normalize(
            new Mean(0f, 0f, 0f, 0f),
            new Std(1f, 1f, 1f, 1f)
        );

        public static readonly Normalize NONE = new Normalize(
            new Mean(1f, 1f, 1f, 1f),
            new Std(0f, 0f, 0f, 0f)
        );
        public Mean Mean { get; private set; }
        public Std Std { get; private set; }
        public Normalize(Mean mean, Std std)
        {
            Mean = mean;
            Std = std;
        }
    }
}
