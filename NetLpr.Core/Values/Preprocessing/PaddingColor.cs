namespace NetLpr.Core.Values.Preprocessing
{
    public readonly struct PaddingColor 
    {
        public int R { get; }
        public int G { get; }
        public int B { get; }

        public PaddingColor(int r, int g, int b,int a)
        {
            R = r; G = g; B = b;
        }
        public static readonly PaddingColor Gray = new PaddingColor(114, 114, 114, 255);

    }
}
