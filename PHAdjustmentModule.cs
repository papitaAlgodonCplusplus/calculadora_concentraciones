using System;
using System.Collections.Generic;
#pragma warning disable CS8618
namespace HydroponicCalculator.Modules
{
    public class AcidData
    {
        public string Name { get; set; }
        public double Purity { get; set; } // %
        public double Density { get; set; } // g/L
        public double MolecularWeight { get; set; }
        public int Valence { get; set; }
        public string ElementProvided { get; set; } // N or P
        public double ElementMolecularWeight { get; set; }
    }

    public class AcidCalculationResult
    {
        public string AcidName { get; set; }
        public double AcidVolume_mlL { get; set; } // ml of acid per L of water
        public double HydrogenConcentration_mgL { get; set; }
        public double HydrogenConcentration_mmolL { get; set; }
        public double NutrientContribution_mgL { get; set; } // N or P contributed
        public string NutrientType { get; set; } // "N" or "P"
        public double BicarbonatesToNeutralize { get; set; }
        public double BicarbonatesRemaining { get; set; } = 30.5; // Always leave 30.5 mg/L as buffer
    }

    public class TitrationResult
    {
        public List<TitrationReplication> Replications { get; set; } = new List<TitrationReplication>();
        public double AverageAcidVolume_mlL { get; set; }
        public double CalculatedBicarbonates_mgL { get; set; }
        public double TotalBicarbonates_mgL { get; set; }
    }

    public class TitrationReplication
    {
        public int ReplicationNumber { get; set; }
        public double WaterVolume_L { get; set; }
        public double InitialPH { get; set; }
        public double FinalPH { get; set; }
        public double AcidUsed_ml { get; set; }
        public double AcidPerLiter_mlL { get; set; }
    }

    public class PHAdjustmentModule
    {
        private Dictionary<string, AcidData> acids;
        private const double BUFFER_BICARBONATES = 30.5; // mg/L to leave unneutralized
        private const double BICARBONATE_THRESHOLD = 122.0; // mg/L (2 mmol/L)

        public PHAdjustmentModule()
        {
            InitializeAcids();
        }

        private void InitializeAcids()
        {
            acids = new Dictionary<string, AcidData>
            {
                ["HNO3"] = new AcidData
                {
                    Name = "Nitric Acid",
                    Purity = 65.0,
                    Density = 1400.0,
                    MolecularWeight = 63.01,
                    Valence = 1,
                    ElementProvided = "N",
                    ElementMolecularWeight = 14.01
                },
                ["H3PO4"] = new AcidData
                {
                    Name = "Phosphoric Acid",
                    Purity = 85.0,
                    Density = 1685.0,
                    MolecularWeight = 98.00,
                    Valence = 3,
                    ElementProvided = "P",
                    ElementMolecularWeight = 30.97
                }
            };
        }

        public string SelectAcidStrategy(double hco3_mgL, double targetP_mgL, double currentP_mgL)
        {
            double neededP = targetP_mgL - currentP_mgL;

            if (hco3_mgL > BICARBONATE_THRESHOLD)
            {
                // High bicarbonates: Use phosphoric acid first, then nitric acid
                return "TwoStepStrategy";
            }
            else
            {
                if (neededP > 0)
                {
                    // Low bicarbonates but need P: Could use phosphoric acid
                    return "PhosphoricAcidOption";
                }
                else
                {
                    // Low bicarbonates: Use nitric acid only
                    return "NitricAcidOnly";
                }
            }
        }

        public List<AcidCalculationResult> CalculateAcidRequirement_IncrossiMethod(
            double hco3_mgL,
            double targetPH,
            double targetP_mgL = 0,
            double currentP_mgL = 0)
        {
            var results = new List<AcidCalculationResult>();
            string strategy = SelectAcidStrategy(hco3_mgL, targetP_mgL, currentP_mgL);

            switch (strategy)
            {
                case "TwoStepStrategy":
                    results.AddRange(CalculateTwoStepAcidAdjustment(hco3_mgL, targetPH, targetP_mgL, currentP_mgL));
                    break;

                case "PhosphoricAcidOption":
                    results.Add(CalculateSingleAcidAdjustment("H3PO4", hco3_mgL, targetPH, targetP_mgL, currentP_mgL));
                    break;

                case "NitricAcidOnly":
                default:
                    results.Add(CalculateSingleAcidAdjustment("HNO3", hco3_mgL, targetPH));
                    break;
            }

            return results;
        }

        private List<AcidCalculationResult> CalculateTwoStepAcidAdjustment(
            double hco3_mgL,
            double targetPH,
            double targetP_mgL,
            double currentP_mgL)
        {
            var results = new List<AcidCalculationResult>();

            // Step 1: Use phosphoric acid to provide needed P
            double neededP = targetP_mgL - currentP_mgL;
            if (neededP > 0)
            {
                var phosphoricResult = CalculatePhosphoricAcidForPhosphorus(neededP);
                results.Add(phosphoricResult);

                // Calculate remaining bicarbonates after phosphoric acid
                double remainingHCO3 = hco3_mgL - phosphoricResult.BicarbonatesToNeutralize;

                // Step 2: Use nitric acid for remaining bicarbonates
                if (remainingHCO3 > BUFFER_BICARBONATES)
                {
                    var nitricResult = CalculateSingleAcidAdjustment("HNO3", remainingHCO3, targetPH);
                    results.Add(nitricResult);
                }
            }
            else
            {
                // If no P needed, just use nitric acid
                results.Add(CalculateSingleAcidAdjustment("HNO3", hco3_mgL, targetPH));
            }

            return results;
        }

        private AcidCalculationResult CalculatePhosphoricAcidForPhosphorus(double neededP_mgL)
        {
            var acid = acids["H3PO4"];

            // Calculate acid concentration needed to provide P
            // Formula: P = Acid_mgL × ElementMW × Purity / AcidMW × 100
            // Rearranged: Acid_mgL = P × AcidMW × 100 / (ElementMW × Purity)
            double acidConc_mgL = neededP_mgL * acid.MolecularWeight * 100.0 /
                                 (acid.ElementMolecularWeight * acid.Purity);

            // Calculate acid volume: Q = AcidConc / (Valence × Density × Purity/100)
            double acidVolume_mlL = acidConc_mgL / (acid.Valence * acid.Density * (acid.Purity / 100.0));

            // Calculate how much bicarbonate this neutralizes
            double hNeutralized = acidVolume_mlL * acid.Valence * acid.Density * (acid.Purity / 100.0);

            return new AcidCalculationResult
            {
                AcidName = acid.Name,
                AcidVolume_mlL = acidVolume_mlL,
                HydrogenConcentration_mgL = hNeutralized,
                HydrogenConcentration_mmolL = hNeutralized / acid.MolecularWeight,
                NutrientContribution_mgL = neededP_mgL,
                NutrientType = "P",
                BicarbonatesToNeutralize = hNeutralized,
                BicarbonatesRemaining = BUFFER_BICARBONATES
            };
        }

        private AcidCalculationResult CalculateSingleAcidAdjustment(
            string acidType,
            double hco3_mgL,
            double targetPH,
            double targetP_mgL = 0,
            double currentP_mgL = 0)
        {
            var acid = acids[acidType];

            // Incrossi Method: H+ = [HCO3-] / (1 + 10^(pH - 6.35))
            double bicarbonateToNeutralize = Math.Max(0, hco3_mgL - BUFFER_BICARBONATES);
            double hPlusRequired_mgL = bicarbonateToNeutralize / (1 + Math.Pow(10, targetPH - 6.35));

            // Calculate acid volume: Q = H+ × MW / (n × D × P/100)
            double acidVolume_mlL = hPlusRequired_mgL * acid.MolecularWeight /
                                   (acid.Valence * acid.Density * (acid.Purity / 100.0));

            // Alternative calculation without MW for direct mg/L
            double acidVolume_mlL_direct = hPlusRequired_mgL /
                                          (acid.Valence * acid.Density * (acid.Purity / 100.0));

            // Calculate nutrient contribution if applicable
            double nutrientContribution = 0;
            if (acidType == "HNO3")
            {
                // Calculate N contribution: N = AcidVolume × Density × Purity × ElementMW / AcidMW
                nutrientContribution = acidVolume_mlL * acid.Density * (acid.Purity / 100.0) *
                                     acid.ElementMolecularWeight / acid.MolecularWeight;
            }
            else if (acidType == "H3PO4")
            {
                nutrientContribution = targetP_mgL - currentP_mgL;
            }

            return new AcidCalculationResult
            {
                AcidName = acid.Name,
                AcidVolume_mlL = acidVolume_mlL_direct,
                HydrogenConcentration_mgL = hPlusRequired_mgL,
                HydrogenConcentration_mmolL = hPlusRequired_mgL / acid.MolecularWeight,
                NutrientContribution_mgL = nutrientContribution,
                NutrientType = acid.ElementProvided,
                BicarbonatesToNeutralize = bicarbonateToNeutralize,
                BicarbonatesRemaining = BUFFER_BICARBONATES
            };
        }

        public TitrationResult CalculateFromTitration(List<TitrationReplication> replications, string acidType = "HNO3")
        {
            var acid = acids[acidType];
            var result = new TitrationResult { Replications = replications };

            // Calculate average acid volume
            double totalAcidVolume = 0;
            foreach (var rep in replications)
            {
                rep.AcidPerLiter_mlL = rep.AcidUsed_ml / rep.WaterVolume_L;
                totalAcidVolume += rep.AcidPerLiter_mlL;
            }
            result.AverageAcidVolume_mlL = totalAcidVolume / replications.Count;

            // Calculate bicarbonate concentration from acid volume
            // H+ = Q × n × D × P/100
            double hPlus_mgL = result.AverageAcidVolume_mlL * acid.Valence *
                              acid.Density * (acid.Purity / 100.0);

            result.CalculatedBicarbonates_mgL = hPlus_mgL;
            result.TotalBicarbonates_mgL = result.CalculatedBicarbonates_mgL + BUFFER_BICARBONATES;

            return result;
        }

        public Dictionary<string, double> GetAcidProperties(string acidType)
        {
            if (!acids.ContainsKey(acidType))
                return new Dictionary<string, double>();

            var acid = acids[acidType];
            return new Dictionary<string, double>
            {
                ["Purity"] = acid.Purity,
                ["Density"] = acid.Density,
                ["MolecularWeight"] = acid.MolecularWeight,
                ["Valence"] = acid.Valence,
                ["ElementMolecularWeight"] = acid.ElementMolecularWeight
            };
        }

        public bool RequiresAcidAdjustment(double waterPH, double targetPH = 6.0)
        {
            return waterPH > targetPH;
        }

        public string GetAcidRecommendation(double hco3_mgL, double currentP_mgL, double targetP_mgL)
        {
            string strategy = SelectAcidStrategy(hco3_mgL, targetP_mgL, currentP_mgL);

            switch (strategy)
            {
                case "TwoStepStrategy":
                    return $"High bicarbonates ({hco3_mgL:F1} mg/L > 122 mg/L). " +
                           "Recommended: Use phosphoric acid first for P requirements, then nitric acid for remaining pH adjustment.";

                case "PhosphoricAcidOption":
                    return $"Moderate bicarbonates ({hco3_mgL:F1} mg/L) and P needed. " +
                           "Option: Use phosphoric acid to provide P and adjust pH simultaneously.";

                case "NitricAcidOnly":
                default:
                    return $"Low bicarbonates ({hco3_mgL:F1} mg/L < 122 mg/L). " +
                           "Recommended: Use nitric acid for pH adjustment.";
            }
        }
    }
}