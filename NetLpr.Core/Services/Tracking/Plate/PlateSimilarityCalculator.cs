using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetLpr.Core.Services.Tracking.Plate
{
    internal class PlateSimilarityCalculator
    {
        private static readonly Dictionary<(char, char), double> Penalties = new()
        {

            { ('0', 'O'), 0.25 }, { ('0', 'Q'), 0.35 }, { ('0', 'D'), 0.35 }, { ('0', 'C'), 0.40 }, { ('0', '6'), 0.45 },
            { ('O', 'Q'), 0.30 }, { ('O', 'D'), 0.35 }, { ('O', 'C'), 0.40 },
            { ('D', 'Q'), 0.40 },
            { ('6', 'G'), 0.40 }, { ('C', 'G'), 0.35 }, { ('9', 'Q'), 0.45 },

      
            { ('1', 'I'), 0.25 }, { ('1', '7'), 0.40 }, { ('1', 'T'), 0.40 }, { ('1', 'J'), 0.45 }, { ('1', 'L'), 0.45 },
            { ('I', 'T'), 0.35 }, { ('I', 'L'), 0.40 }, { ('I', 'J'), 0.45 },

 
            { ('8', 'B'), 0.30 }, { ('8', '3'), 0.35 }, { ('8', 'S'), 0.40 }, { ('B', 'R'), 0.35 }, { ('3', 'B'), 0.40 },
            { ('5', 'S'), 0.30 }, { ('E', 'F'), 0.35 }, { ('P', 'R'), 0.35 }, { ('F', 'P'), 0.40 },

            { ('2', 'Z'), 0.30 }, { ('7', 'Z'), 0.45 }, { ('4', 'A'), 0.35 },
            { ('U', 'V'), 0.35 }, { ('V', 'W'), 0.40 }, { ('M', 'N'), 0.40 }, { ('X', 'Y'), 0.45 }
        };

        public static double GetCharPenalty(char c1, char c2)
        {
            if (c1 == c2) return 0.0;

   
            var key = c1 < c2 ? (c1, c2) : (c2, c1);

            if (Penalties.TryGetValue(key, out double penalty))
            {
                return penalty;
            }

            return 1.0; 
        }

        public static double CalculateWeightedSimilarity(string plate1, string plate2)
        {
            plate1 ??= string.Empty;
            plate2 ??= string.Empty;

            int len1 = plate1.Length;
            int len2 = plate2.Length;

            if (len1 == 0 && len2 == 0) return 100.0;


            double[,] dp = new double[len1 + 1, len2 + 1];

            for (int i = 0; i <= len1; i++) dp[i, 0] = i;
            for (int j = 0; j <= len2; j++) dp[0, j] = j;

            for (int i = 1; i <= len1; i++)
            {
                for (int j = 1; j <= len2; j++)
                {
                    double cost = GetCharPenalty(plate1[i - 1], plate2[j - 1]);

                    dp[i, j] = Math.Min(
                        Math.Min(dp[i - 1, j] + 1.0, dp[i, j - 1] + 1.0), 
                        dp[i - 1, j - 1] + cost                          
                    );
                }
            }

            double totalDistance = dp[len1, len2];
            int maxLen = Math.Max(len1, len2);

            double similarity = Math.Max(0.0, (1.0 - (totalDistance / maxLen)) * 100.0);
            return similarity;
        }
    }
}
