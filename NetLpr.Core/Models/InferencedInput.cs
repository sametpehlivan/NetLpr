using System.Drawing;
using NetLpr.Core.Models;

namespace NetLpr.Core.Datas
{

    public class InferencedInput : IDisposable
    {
        public string SourceId { get; private set; }
        private int _disposed = 0;
        public bool IsProcessed { get; private set; } = false;
        public List<InferenceResult> DetectionResults { get; } = new();
        public BaseImage? Image { get; set; }
        public StreamAnalysesInfo StreamAnalysesInfo { get; }
        public InferencedInput(string sourceId,StreamAnalysesInfo streamAnalysesInfo) { 
            SourceId = sourceId;
            StreamAnalysesInfo = streamAnalysesInfo; 

        }
        public void MarkProcessed()
        {
            IsProcessed = true;
        }
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1)
                return;
            try
            {
                if (Image != null && !Image.IsDisposedOrEmpty())
                {
                    Image.Dispose();
                }
                GC.SuppressFinalize(this);

            }
            catch (Exception e)
            {
                throw new ArgumentException("Image: Dispose Edilemedi!",e);
            }
        }
    }
}
