using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetLpr.Core.Values.Inference
{
    public class OcrResponseConfig 
    {
        private static readonly string[] regions = [
            "region.albania",
            "region.andorra",
            "region.argentina",
            "region.armenia",
            "region.australia",
            "region.austria",
            "region.azerbaijan",
            "region.bahrain",
            "region.belarus",
            "region.belgium",
            "region.bosnia_and_herzegovina",
            "region.brazil",
            "region.bulgaria",
            "region.cambodia",
            "region.canada",
            "region.croatia",
            "region.cyprus",
            "region.czech_republic",
            "region.denmark",
            "region.estonia",
            "region.finland",
            "region.france",
            "region.georgia",
            "region.germany",
            "region.gibraltar",
            "region.greece",
            "region.guernsey",
            "region.hungary",
            "region.iceland",
            "region.indonesia",
            "region.ireland",
            "region.israel",
            "region.italy",
            "region.latvia",
            "region.liechtenstein",
            "region.lithuania",
            "region.luxembourg",
            "region.malaysia",
            "region.malta",
            "region.mexico",
            "region.moldova",
            "region.monaco",
            "region.montenegro",
            "region.netherlands",
            "region.new_zealand",
            "region.north_macedonia",
            "region.norway",
            "region.poland",
            "region.portugal",
            "region.qatar",
            "region.romania",
            "region.san_marino",
            "region.serbia",
            "region.singapore",
            "region.slovakia",
            "region.slovenia",
            "region.spain",
            "region.sweden",
            "region.switzerland",
            "region.thailand",
            "region.turkey",
            "region.united_states",
            "region.ukraine",
            "region.united_kingdom",
            "region.vietnam",
            "region.unknown"
        ];
        public float Threshold { get; set; }

        public string[] Regions { get
            {
                return regions;
            }
        }
        public bool IsRegionExistsOnInference { get; }
        public int MaxSlotSize { get; }
        public char PaddingChar { get; }
        public string OcrAlphabet { get; }

        public OcrResponseConfig(int maxSlotSize,char paddingChar, string ocrAlphabet, float threshold = 0.5f, bool isRegionExistsOnInference = true)
        {
            Threshold = threshold;
            MaxSlotSize = maxSlotSize;
            PaddingChar = paddingChar;
            OcrAlphabet = ocrAlphabet;
            IsRegionExistsOnInference = isRegionExistsOnInference;
        }
    }
}
