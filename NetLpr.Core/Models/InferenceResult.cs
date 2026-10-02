using System.Collections.Concurrent;
using NetLpr.Core.Values.Inference;

namespace NetLpr.Core.Models
{

    public class InferenceResult
    {
        public static readonly InferenceResult Empty = new InferenceResult();
        public static string INFORMATION_PLATE_DETECTON_INFERENCE_KEY { get; private set; } = "INFORMATION_PLATE_DETECTON_INFERENCE_KEY";
        public static string INFORMATION_PLATE_OCR_INFERENCE_KEY { get; private set; } = "INFORMATION_PLATE_OCR_INFERENCE_KEY";

        private ConcurrentDictionary<string, object> Informations = new();
        public string TrackId { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string ReelLabel { get; set; } = string.Empty;
        public int ClassId { get; set; } = int.MinValue;
        public float Score { get; set; } = 0f;
        public int X1 { get; set; } = 0;
        public int Y1 { get; set; } = 0;
        public int X2 { get; set; } = 0;
        public int Y2 { get; set; } = 0;

        public string GetLabel()
        {
            return ReelLabel ?? string.Empty;
        }
        public float CalculateArea()
        {
            return (X2 - X1) * (Y2 - Y1);
        }
        public override string ToString()
        {
            return $"[{X1},{Y1},{X2},{Y2}] conf={Score:F4} class={Label}";
        }
        public T? GetInformation<T>(string key)
        {
            key = $"{key}_{typeof(T)}";
            _ = Informations.TryGetValue(key, out var value);
            if (value != null)
            {
                return (T)value;
            }
            return default;
        }
        public List<(double x, double y)> GetPoints()
        {
            return [
                (X1, Y1),
                (X2, Y1),
                (X2, Y2),
                (X1, Y2)
            ];
        }
        public void AddInformation<T>(string key, T? value)
        {
            if (value != null)
            {
                key = $"{key}_{typeof(T)}";
                Informations.AddOrUpdate(key, value, (_, _) => value!);
            }
        }
        public void ClearInformations()
        {
            Informations.Clear();
        }
        public void CalculateAndInitiateOriginalBox(CalculateOriginalBoxInfo info)
        {
            double scaleX = info.ScaleX != 0 ? info.ScaleX : 1.0;
            double scaleY = info.ScaleY != 0 ? info.ScaleY : 1.0;

            double newX1 = (X1 - info.PaddingX) / scaleX;
            double newY1 = (Y1 - info.PaddingY) / scaleY;
            double newX2 = (X2 - info.PaddingX) / scaleX;
            double newY2 = (Y2 - info.PaddingY) / scaleY;

            X1 = Math.Max(0, (int)Math.Round(newX1));
            Y1 = Math.Max(0, (int)Math.Round(newY1));
            X2 = Math.Min(info.OriginalSize.Width - 1, (int)Math.Round(newX2));
            Y2 = Math.Min(info.OriginalSize.Height - 1, (int)Math.Round(newY2));
            if (X2 < X1)
                (X1, X2) = (X2, X1);
            if (Y2 < Y1)
                (Y1, Y2) = (Y2, Y1);
        }






 





    }
}
