using System;
using System.Collections.Generic;
using System.Linq;
#pragma warning disable CS8618

namespace HydroponicCalculator.Modules
{
    public class WaterQualityParameters
    {
        public double pH { get; set; }
        public double EC { get; set; } // dS/m
        public double Temperature { get; set; } = 20.0; // °C
        public double HCO3 { get; set; } // mg/L
        public Dictionary<string, double> Elements_mgL { get; set; } = new Dictionary<string, double>();
        public Dictionary<string, double> Elements_mmolL { get; set; } = new Dictionary<string, double>();
        public Dictionary<string, double> Elements_meqL { get; set; } = new Dictionary<string, double>();
    }

    public class WaterQualityResult
    {
        public string Parameter { get; set; }
        public double Value { get; set; }
        public string Unit { get; set; }
        public string Status { get; set; } // "Optimal", "High", "Low"
        public string StatusColor { get; set; } // "Green", "Red", "Yellow"
        public double MinRange { get; set; }
        public double MaxRange { get; set; }
        public string Recommendation { get; set; }
    }

    public class WaterQualityIndices
    {
        public double RSC { get; set; } // Residual Sodium Carbonate
        public double SAR { get; set; } // Sodium Adsorption Ratio
        public double SARo { get; set; } // Corrected SAR
        public double PSI { get; set; } // Exchangeable Sodium Percentage
        public double ScottIndex { get; set; }
        public double CaMgRatio { get; set; }
        public double TotalSalts_Analyzed { get; set; }
        public double TotalSalts_Estimated { get; set; }
        public double SaltsDifference { get; set; }
        public double SaltsTolerance { get; set; }
    }

    public class WaterAnalysisModule
    {
        private Dictionary<string, ElementData> elementData;
        private Dictionary<string, (double min, double max)> optimalRanges;

        public WaterAnalysisModule()
        {
            InitializeElementData();
            InitializeOptimalRanges();
        }

        private void InitializeElementData()
        {
            elementData = new Dictionary<string, ElementData>
            {
                ["Ca"] = new ElementData { AtomicWeight = 40.08, Valence = 2, IsCation = true },
                ["K"] = new ElementData { AtomicWeight = 39.10, Valence = 1, IsCation = true },
                ["Mg"] = new ElementData { AtomicWeight = 24.31, Valence = 2, IsCation = true },
                ["Na"] = new ElementData { AtomicWeight = 22.99, Valence = 1, IsCation = true },
                ["NH4"] = new ElementData { AtomicWeight = 18.04, Valence = 1, IsCation = true },
                ["NO3"] = new ElementData { AtomicWeight = 62.00, Valence = 1, IsCation = false },
                ["SO4"] = new ElementData { AtomicWeight = 96.06, Valence = 2, IsCation = false },
                ["Cl"] = new ElementData { AtomicWeight = 35.45, Valence = 1, IsCation = false },
                ["H2PO4"] = new ElementData { AtomicWeight = 96.99, Valence = 1, IsCation = false },
                ["HCO3"] = new ElementData { AtomicWeight = 61.02, Valence = 1, IsCation = false }
            };
        }

        private void InitializeOptimalRanges()
        {
            optimalRanges = new Dictionary<string, (double min, double max)>
            {
                ["pH"] = (5.5, 6.5),
                ["EC"] = (0.1, 0.8),
                ["Ca"] = (0, 100),
                ["K"] = (0, 20),
                ["Mg"] = (0, 50),
                ["Na"] = (0, 50),
                ["NO3"] = (0, 10),
                ["SO4"] = (0, 200),
                ["Cl"] = (0, 100),
                ["HCO3"] = (30.5, 122)
            };
        }

        public WaterQualityParameters AnalyzeWater(WaterQualityParameters waterData)
        {
            // Convert units and calculate derived values
            foreach (var element in waterData.Elements_mgL)
            {
                if (elementData.ContainsKey(element.Key))
                {
                    var atomicWeight = elementData[element.Key].AtomicWeight;
                    var valence = elementData[element.Key].Valence;

                    // Convert mg/L to mmol/L
                    waterData.Elements_mmolL[element.Key] = element.Value / atomicWeight;

                    // Convert mmol/L to meq/L
                    waterData.Elements_meqL[element.Key] = waterData.Elements_mmolL[element.Key] * valence;
                }
            }

            return waterData;
        }

        public List<WaterQualityResult> EvaluateWaterQuality(WaterQualityParameters waterData)
        {
            var results = new List<WaterQualityResult>();

            // Evaluate pH
            results.Add(EvaluateParameter("pH", waterData.pH, "pH units", optimalRanges["pH"]));

            // Evaluate EC
            results.Add(EvaluateParameter("EC", waterData.EC, "dS/m", optimalRanges["EC"]));

            // Evaluate major elements
            foreach (var element in waterData.Elements_mgL)
            {
                if (optimalRanges.ContainsKey(element.Key))
                {
                    results.Add(EvaluateParameter(element.Key, element.Value, "mg/L", optimalRanges[element.Key]));
                }
            }

            return results;
        }

        private WaterQualityResult EvaluateParameter(string parameter, double value, string unit, (double min, double max) range)
        {
            var result = new WaterQualityResult
            {
                Parameter = parameter,
                Value = value,
                Unit = unit,
                MinRange = range.min,
                MaxRange = range.max
            };

            if (value >= range.min && value <= range.max)
            {
                result.Status = "Optimal";
                result.StatusColor = "Green";
                result.Recommendation = "Within optimal range";
            }
            else if (value > range.max)
            {
                result.Status = "High";
                result.StatusColor = "Red";
                result.Recommendation = $"Concentration too high. Consider treatment or dilution.";
            }
            else
            {
                result.Status = "Low";
                result.StatusColor = "Yellow";
                result.Recommendation = $"Concentration low. Monitor during fertilization.";
            }

            return result;
        }

        public WaterQualityIndices CalculateWaterQualityIndices(WaterQualityParameters waterData)
        {
            var indices = new WaterQualityIndices();

            // Get values in meq/L
            double ca_meq = waterData.Elements_meqL.GetValueOrDefault("Ca", 0);
            double mg_meq = waterData.Elements_meqL.GetValueOrDefault("Mg", 0);
            double na_meq = waterData.Elements_meqL.GetValueOrDefault("Na", 0);
            double k_meq = waterData.Elements_meqL.GetValueOrDefault("K", 0);
            double hco3_meq = waterData.Elements_meqL.GetValueOrDefault("HCO3", 0);
            double so4_meq = waterData.Elements_meqL.GetValueOrDefault("SO4", 0);
            double cl_meq = waterData.Elements_meqL.GetValueOrDefault("Cl", 0);

            // Residual Sodium Carbonate (RSC) = (HCO3 + CO3) - (Ca + Mg)
            indices.RSC = hco3_meq - (ca_meq + mg_meq);

            // Sodium Adsorption Ratio (SAR) = Na / sqrt((Ca + Mg)/2)
            if ((ca_meq + mg_meq) > 0)
            {
                indices.SAR = na_meq / Math.Sqrt((ca_meq + mg_meq) / 2.0);
            }

            // Corrected SAR (SARo) - simplified calculation
            indices.SARo = indices.SAR * (1 + (8.4 - waterData.pH));

            // Exchangeable Sodium Percentage (PSI) = Na / (Ca + Mg + Na + K) * 100
            double totalCations = ca_meq + mg_meq + na_meq + k_meq;
            if (totalCations > 0)
            {
                indices.PSI = (na_meq / totalCations) * 100.0;
            }

            // Scott Index = (Cl + SO4) / (HCO3 + CO3)
            if (hco3_meq > 0)
            {
                indices.ScottIndex = (cl_meq + so4_meq) / hco3_meq;
            }

            // Ca/Mg Ratio
            if (mg_meq > 0)
            {
                indices.CaMgRatio = ca_meq / mg_meq;
            }

            // Calculate total salts
            indices.TotalSalts_Analyzed = CalculateTotalSalts(waterData);
            indices.TotalSalts_Estimated = waterData.EC * 640; // mg/L, approximation
            indices.SaltsDifference = Math.Abs(indices.TotalSalts_Analyzed - indices.TotalSalts_Estimated);
            indices.SaltsTolerance = indices.TotalSalts_Estimated * 0.1; // 10% tolerance

            return indices;
        }

        private double CalculateTotalSalts(WaterQualityParameters waterData)
        {
            double total = 0;
            foreach (var element in waterData.Elements_mgL)
            {
                total += element.Value;
            }
            return total;
        }

        public Dictionary<string, double> VerifyIonBalance(WaterQualityParameters waterData)
        {
            var results = new Dictionary<string, double>();

            double sumCations = 0;
            double sumAnions = 0;

            foreach (var ion in waterData.Elements_meqL)
            {
                if (elementData.ContainsKey(ion.Key))
                {
                    if (elementData[ion.Key].IsCation)
                        sumCations += ion.Value;
                    else
                        sumAnions += ion.Value;
                }
            }

            results["SumCations"] = sumCations;
            results["SumAnions"] = sumAnions;
            results["Difference"] = Math.Abs(sumCations - sumAnions);
            results["PercentageDifference"] = sumCations > 0 ? (results["Difference"] / sumCations) * 100.0 : 0;
            results["IsBalanced"] = results["PercentageDifference"] <= 10.0 ? 1 : 0;

            return results;
        }
    }

    public class ElementData
    {
        public double AtomicWeight { get; set; }
        public int Valence { get; set; }
        public bool IsCation { get; set; }
    }
}