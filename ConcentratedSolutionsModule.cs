using System;
using System.Collections.Generic;
using System.Linq;
#pragma warning disable CS8618
namespace HydroponicCalculator.Modules
{
    public enum CompatibilityLevel
    {
        C = 0, // 100% compatible dry and in water
        I = 1, // Incompatible dry and in water
        E = 2, // Compatible only in water at injection time
        L = 3, // Limited compatibility - use limited amounts
        P = 4, // Generates heat - dangerous, add acid to water
        S = 5  // Solubility limited
    }

    public class FertilizerCompatibility
    {
        public string Fertilizer1 { get; set; } = "";
        public string Fertilizer2 { get; set; } = "";
        public CompatibilityLevel Compatibility { get; set; }
        public string Description { get; set; } = "";
        public string Recommendation { get; set; } = "";
    }

    public class FertilizerSolubility
    {
        public string Name { get; set; } = "";
        public string Formula { get; set; } = "";
        public double Solubility_0C { get; set; } // g/L at 0°C
        public double Solubility_20C { get; set; } // g/L at 20°C
        public double Solubility_40C { get; set; } // g/L at 40°C
        public double SafeConcentrationLimit { get; set; } // 50% of solubility limit
        public double RecommendedMaxConcentration { get; set; } // 80% of safe limit
    }

    public class ConcentrationFactor
    {
        public string Description { get; set; } = "";
        public int Factor { get; set; } // e.g., 200 for 1:200
        public double Percentage { get; set; } // e.g., 0.5% for 1:200
        public double InjectionRate_LperM3 { get; set; } // L/m³
        public string Application { get; set; } = ""; // Recommended application type
    }

    public class TankDistribution
    {
        public int TankNumber { get; set; }
        public string TankLabel { get; set; } = "";
        public List<string> Fertilizers { get; set; } = new List<string>();
        public List<string> Acids { get; set; } = new List<string>();
        public Dictionary<string, double> Concentrations_gL { get; set; } = new Dictionary<string, double>();
        public Dictionary<string, double> FertilizerAmounts_kg { get; set; } = new Dictionary<string, double>();
        public double TotalDensity_gL { get; set; }
        public double Volume_L { get; set; }
        public List<string> CompatibilityWarnings { get; set; } = new List<string>();
        public List<string> SolubilityWarnings { get; set; } = new List<string>();
        public List<string> PreparationInstructions { get; set; } = new List<string>();
        public double EstimatedCost { get; set; }
        public string TankColor { get; set; } = ""; // For visual identification
    }

    public class ConcentratedSolutionReport
    {
        public int NumberOfTanks { get; set; }
        public ConcentrationFactor ConcentrationFactor { get; set; } = new ConcentrationFactor();
        public List<TankDistribution> Tanks { get; set; } = new List<TankDistribution>();
        public Dictionary<string, double> VolumeRequirements { get; set; } = new Dictionary<string, double>();
        public double TotalCost { get; set; }
        public double CostPerM3_DilutedSolution { get; set; }
        public List<string> CriticalWarnings { get; set; } = new List<string>();
        public List<string> GeneralRecommendations { get; set; } = new List<string>();
        public Dictionary<string, double> FertilizerTotals_kg { get; set; } = new Dictionary<string, double>();
        public DateTime CalculationDate { get; set; } = DateTime.Now;
    }

    public class ConcentratedSolutionsModule
    {
        private Dictionary<string, FertilizerSolubility> fertilizerSolubilities;
        private Dictionary<(string, string), CompatibilityLevel> compatibilityMatrix;
        private List<ConcentrationFactor> concentrationFactors;
        private Dictionary<string, double> fertilizerCosts; // Cost per kg

        public ConcentratedSolutionsModule()
        {
            InitializeFertilizerSolubilities();
            InitializeCompatibilityMatrix();
            InitializeConcentrationFactors();
            InitializeFertilizerCosts();
        }

        private void InitializeFertilizerSolubilities()
        {
            fertilizerSolubilities = new Dictionary<string, FertilizerSolubility>
            {
                ["NH4NO3"] = new FertilizerSolubility
                {
                    Name = "Ammonium Nitrate",
                    Formula = "NH4NO3",
                    Solubility_0C = 1800,
                    Solubility_20C = 1900,
                    Solubility_40C = 2190,
                    SafeConcentrationLimit = 950, // 50% of 20°C solubility
                    RecommendedMaxConcentration = 760 // 80% of safe limit
                },
                ["(NH4)2SO4"] = new FertilizerSolubility
                {
                    Name = "Ammonium Sulfate",
                    Formula = "(NH4)2SO4",
                    Solubility_0C = 700,
                    Solubility_20C = 760,
                    Solubility_40C = 760,
                    SafeConcentrationLimit = 380,
                    RecommendedMaxConcentration = 304
                },
                ["Ca(NO3)2.2H2O"] = new FertilizerSolubility
                {
                    Name = "Calcium Nitrate",
                    Formula = "Ca(NO3)2·2H2O",
                    Solubility_0C = 1200,
                    Solubility_20C = 1200,
                    Solubility_40C = 1200,
                    SafeConcentrationLimit = 600,
                    RecommendedMaxConcentration = 480
                },
                ["KNO3"] = new FertilizerSolubility
                {
                    Name = "Potassium Nitrate",
                    Formula = "KNO3",
                    Solubility_0C = 130,
                    Solubility_20C = 335,
                    Solubility_40C = 630,
                    SafeConcentrationLimit = 167,
                    RecommendedMaxConcentration = 134
                },
                ["NH4H2PO4"] = new FertilizerSolubility
                {
                    Name = "Monoammonium Phosphate",
                    Formula = "NH4H2PO4",
                    Solubility_0C = 225,
                    Solubility_20C = 400,
                    Solubility_40C = 818,
                    SafeConcentrationLimit = 200,
                    RecommendedMaxConcentration = 160
                },
                ["(NH4)2HPO4"] = new FertilizerSolubility
                {
                    Name = "Diammonium Phosphate",
                    Formula = "(NH4)2HPO4",
                    Solubility_0C = 575,
                    Solubility_20C = 400,
                    Solubility_40C = 818,
                    SafeConcentrationLimit = 200,
                    RecommendedMaxConcentration = 160
                },
                ["KH2PO4"] = new FertilizerSolubility
                {
                    Name = "Monopotassium Phosphate",
                    Formula = "KH2PO4",
                    Solubility_0C = 143,
                    Solubility_20C = 227,
                    Solubility_40C = 339,
                    SafeConcentrationLimit = 113,
                    RecommendedMaxConcentration = 90
                },
                ["K2HPO4.3H2O"] = new FertilizerSolubility
                {
                    Name = "Dipotassium Phosphate",
                    Formula = "K2HPO4·3H2O",
                    Solubility_0C = 1590,
                    Solubility_20C = 2125,
                    Solubility_40C = 2125,
                    SafeConcentrationLimit = 1062,
                    RecommendedMaxConcentration = 850
                },
                ["KCl"] = new FertilizerSolubility
                {
                    Name = "Potassium Chloride",
                    Formula = "KCl",
                    Solubility_0C = 282,
                    Solubility_20C = 342,
                    Solubility_40C = 403,
                    SafeConcentrationLimit = 171,
                    RecommendedMaxConcentration = 137
                },
                ["K2SO4"] = new FertilizerSolubility
                {
                    Name = "Potassium Sulfate",
                    Formula = "K2SO4",
                    Solubility_0C = 74,
                    Solubility_20C = 111,
                    Solubility_40C = 148,
                    SafeConcentrationLimit = 55,
                    RecommendedMaxConcentration = 44
                },
                ["MgSO4.7H2O"] = new FertilizerSolubility
                {
                    Name = "Magnesium Sulfate",
                    Formula = "MgSO4·7H2O",
                    Solubility_0C = 710,
                    Solubility_20C = 710,
                    Solubility_40C = 710,
                    SafeConcentrationLimit = 355,
                    RecommendedMaxConcentration = 284
                },
                ["MgCl2.6H2O"] = new FertilizerSolubility
                {
                    Name = "Magnesium Chloride",
                    Formula = "MgCl2·6H2O",
                    Solubility_0C = 528,
                    Solubility_20C = 546,
                    Solubility_40C = 575,
                    SafeConcentrationLimit = 273,
                    RecommendedMaxConcentration = 218
                },
                ["CaCl2.6H2O"] = new FertilizerSolubility
                {
                    Name = "Calcium Chloride",
                    Formula = "CaCl2·6H2O",
                    Solubility_0C = 600,
                    Solubility_20C = 600,
                    Solubility_40C = 600,
                    SafeConcentrationLimit = 300,
                    RecommendedMaxConcentration = 240
                },
                ["FeSO4.7H2O"] = new FertilizerSolubility
                {
                    Name = "Iron Sulfate",
                    Formula = "FeSO4·7H2O",
                    Solubility_0C = 155,
                    Solubility_20C = 260,
                    Solubility_40C = 650,
                    SafeConcentrationLimit = 130,
                    RecommendedMaxConcentration = 104
                },
                ["CuSO4.5H2O"] = new FertilizerSolubility
                {
                    Name = "Copper Sulfate",
                    Formula = "CuSO4·5H2O",
                    Solubility_0C = 316,
                    Solubility_20C = 316,
                    Solubility_40C = 316,
                    SafeConcentrationLimit = 158,
                    RecommendedMaxConcentration = 126
                },
                ["MnSO4.4H2O"] = new FertilizerSolubility
                {
                    Name = "Manganese Sulfate",
                    Formula = "MnSO4·4H2O",
                    Solubility_0C = 1053,
                    Solubility_20C = 1053,
                    Solubility_40C = 1053,
                    SafeConcentrationLimit = 526,
                    RecommendedMaxConcentration = 421
                },
                ["ZnSO4.7H2O"] = new FertilizerSolubility
                {
                    Name = "Zinc Sulfate",
                    Formula = "ZnSO4·7H2O",
                    Solubility_0C = 750,
                    Solubility_20C = 750,
                    Solubility_40C = 750,
                    SafeConcentrationLimit = 375,
                    RecommendedMaxConcentration = 300
                },
                ["H3BO3"] = new FertilizerSolubility
                {
                    Name = "Boric Acid",
                    Formula = "H3BO3",
                    Solubility_0C = 63.5,
                    Solubility_20C = 63.5,
                    Solubility_40C = 63.5,
                    SafeConcentrationLimit = 31.7,
                    RecommendedMaxConcentration = 25.4
                },
                ["FeEDTA"] = new FertilizerSolubility
                {
                    Name = "Iron EDTA Chelate",
                    Formula = "FeEDTA",
                    Solubility_0C = 1000,
                    Solubility_20C = 1000,
                    Solubility_40C = 1000,
                    SafeConcentrationLimit = 500,
                    RecommendedMaxConcentration = 400
                },
                ["Na2MoO4.2H2O"] = new FertilizerSolubility
                {
                    Name = "Sodium Molybdate",
                    Formula = "Na2MoO4·2H2O",
                    Solubility_0C = 840,
                    Solubility_20C = 840,
                    Solubility_40C = 840,
                    SafeConcentrationLimit = 420,
                    RecommendedMaxConcentration = 336
                }
            };
        }

        private void InitializeCompatibilityMatrix()
        {
            compatibilityMatrix = new Dictionary<(string, string), CompatibilityLevel>();

            // Define key incompatible combinations based on the presentation compatibility table
            var incompatiblePairs = new List<(string, string)>
            {
                ("Ca(NO3)2.2H2O", "K2SO4"),
                ("Ca(NO3)2.2H2O", "MgSO4.7H2O"),
                ("Ca(NO3)2.2H2O", "(NH4)2SO4"),
                ("Ca(NO3)2.2H2O", "NH4H2PO4"),
                ("Ca(NO3)2.2H2O", "KH2PO4"),
                ("Ca(NO3)2.2H2O", "(NH4)2HPO4")
            };

            // Limited compatibility pairs
            var limitedCompatibilityPairs = new List<(string, string)>
            {
                ("KNO3", "KH2PO4"),
                ("NH4NO3", "NH4H2PO4"),
                ("K2SO4", "KH2PO4")
            };

            // Compatible only in water pairs
            var waterOnlyPairs = new List<(string, string)>
            {
                ("MgSO4.7H2O", "KH2PO4"),
                ("(NH4)2SO4", "KH2PO4"),
                ("FeSO4.7H2O", "Ca(NO3)2.2H2O")
            };

            // All fertilizers list
            var fertilizers = fertilizerSolubilities.Keys.ToList();

            // Initialize all as compatible by default
            foreach (var fert1 in fertilizers)
            {
                foreach (var fert2 in fertilizers)
                {
                    if (fert1 != fert2)
                    {
                        compatibilityMatrix[(fert1, fert2)] = CompatibilityLevel.C;
                    }
                }
            }

            // Set incompatible pairs
            foreach (var pair in incompatiblePairs)
            {
                compatibilityMatrix[(pair.Item1, pair.Item2)] = CompatibilityLevel.I;
                compatibilityMatrix[(pair.Item2, pair.Item1)] = CompatibilityLevel.I;
            }

            // Set limited compatibility pairs
            foreach (var pair in limitedCompatibilityPairs)
            {
                compatibilityMatrix[(pair.Item1, pair.Item2)] = CompatibilityLevel.L;
                compatibilityMatrix[(pair.Item2, pair.Item1)] = CompatibilityLevel.L;
            }

            // Set water-only compatibility pairs
            foreach (var pair in waterOnlyPairs)
            {
                compatibilityMatrix[(pair.Item1, pair.Item2)] = CompatibilityLevel.E;
                compatibilityMatrix[(pair.Item2, pair.Item1)] = CompatibilityLevel.E;
            }
        }

        private void InitializeConcentrationFactors()
        {
            concentrationFactors = new List<ConcentrationFactor>
            {
                new ConcentrationFactor { Description = "1:40", Factor = 40, Percentage = 2.5, InjectionRate_LperM3 = 25, Application = "High precision systems" },
                new ConcentrationFactor { Description = "1:50", Factor = 50, Percentage = 2.0, InjectionRate_LperM3 = 20, Application = "Standard hydroponic systems" },
                new ConcentrationFactor { Description = "1:66", Factor = 66, Percentage = 1.5, InjectionRate_LperM3 = 15, Application = "Medium concentration systems" },
                new ConcentrationFactor { Description = "1:100", Factor = 100, Percentage = 1.0, InjectionRate_LperM3 = 10, Application = "General purpose systems" },
                new ConcentrationFactor { Description = "1:133", Factor = 133, Percentage = 0.75, InjectionRate_LperM3 = 7.5, Application = "Low concentration systems" },
                new ConcentrationFactor { Description = "1:200", Factor = 200, Percentage = 0.5, InjectionRate_LperM3 = 5, Application = "High volume systems" },
                new ConcentrationFactor { Description = "1:400", Factor = 400, Percentage = 0.25, InjectionRate_LperM3 = 2.5, Application = "Very high volume systems" }
            };
        }

        private void InitializeFertilizerCosts()
        {
            fertilizerCosts = new Dictionary<string, double>
            {
                ["NH4NO3"] = 0.45,
                ["(NH4)2SO4"] = 0.50,
                ["Ca(NO3)2.2H2O"] = 0.80,
                ["KNO3"] = 1.20,
                ["NH4H2PO4"] = 1.80,
                ["(NH4)2HPO4"] = 1.90,
                ["KH2PO4"] = 2.50,
                ["K2HPO4.3H2O"] = 2.80,
                ["KCl"] = 0.60,
                ["K2SO4"] = 1.50,
                ["MgSO4.7H2O"] = 0.60,
                ["MgCl2.6H2O"] = 0.70,
                ["CaCl2.6H2O"] = 0.70,
                ["FeSO4.7H2O"] = 1.80,
                ["CuSO4.5H2O"] = 4.20,
                ["MnSO4.4H2O"] = 2.80,
                ["ZnSO4.7H2O"] = 3.50,
                ["H3BO3"] = 3.20,
                ["FeEDTA"] = 8.50,
                ["Na2MoO4.2H2O"] = 15.00
            };
        }

        public int CalculateConcentrationFactor(double totalFlow_Lh, double injectionFlow_Lh)
        {
            if (injectionFlow_Lh <= 0) return 100; // Default factor
            return (int)Math.Round(totalFlow_Lh / injectionFlow_Lh);
        }

        public ConcentrationFactor GetConcentrationFactor(int factor)
        {
            return concentrationFactors.FirstOrDefault(cf => cf.Factor == factor) ??
                   new ConcentrationFactor { Description = $"1:{factor}", Factor = factor, Percentage = 100.0 / factor, InjectionRate_LperM3 = 1000.0 / factor };
        }

        public List<ConcentrationFactor> GetAvailableConcentrationFactors()
        {
            return new List<ConcentrationFactor>(concentrationFactors);
        }

        public List<TankDistribution> DistributeFertilizers(
            Dictionary<string, double> fertilizerConcentrations_mgL,
            List<string> acidTypes,
            int numberOfTanks,
            int concentrationFactor,
            double tankVolume_L = 200)
        {
            var tanks = new List<TankDistribution>();

            // Initialize tanks with colors for identification
            var tankColors = new[] { "Blue", "Green", "Red", "Yellow", "Orange", "Purple", "Brown", "Pink" };

            for (int i = 0; i < numberOfTanks; i++)
            {
                tanks.Add(new TankDistribution
                {
                    TankNumber = i + 1,
                    TankLabel = GetTankLabel(i),
                    Volume_L = tankVolume_L,
                    TankColor = tankColors[i % tankColors.Length]
                });
            }

            // Strategy based on number of tanks
            switch (numberOfTanks)
            {
                case 2:
                    DistributeInTwoTanks(tanks, fertilizerConcentrations_mgL, acidTypes, concentrationFactor);
                    break;
                case 3:
                    DistributeInThreeTanks(tanks, fertilizerConcentrations_mgL, acidTypes, concentrationFactor);
                    break;
                case 4:
                    DistributeInFourTanks(tanks, fertilizerConcentrations_mgL, acidTypes, concentrationFactor);
                    break;
                default:
                    DistributeInMultipleTanks(tanks, fertilizerConcentrations_mgL, acidTypes, concentrationFactor);
                    break;
            }

            // Check compatibility and solubility for each tank
            foreach (var tank in tanks)
            {
                CheckTankCompatibility(tank);
                CheckTankSolubility(tank, concentrationFactor);
                CalculateTankDensity(tank);
                CalculateTankCost(tank);
                GeneratePreparationInstructions(tank);
            }

            return tanks;
        }

        private void DistributeInTwoTanks(List<TankDistribution> tanks, Dictionary<string, double> fertilizers, List<string> acids, int concentrationFactor)
        {
            // Tank A: Phosphates, Potassium, Magnesium, Micronutrients (avoid calcium)
            var tankA = tanks[0];
            tankA.TankLabel = "A - NPK + Micros";

            var tankAFertilizers = new[] { "KH2PO4", "KNO3", "K2SO4", "MgSO4.7H2O", "FeSO4.7H2O", "FeEDTA", "MnSO4.4H2O", "ZnSO4.7H2O", "CuSO4.5H2O", "H3BO3", "Na2MoO4.2H2O" };
            foreach (var fert in tankAFertilizers)
            {
                if (fertilizers.ContainsKey(fert))
                {
                    tankA.Fertilizers.Add(fert);
                    double concentratedAmount = fertilizers[fert] * concentrationFactor / 1000.0; // Convert to g/L
                    tankA.Concentrations_gL[fert] = concentratedAmount;
                    tankA.FertilizerAmounts_kg[fert] = (concentratedAmount * tankA.Volume_L) / 1000.0; // Convert to kg
                }
            }

            // Tank B: Calcium and Acids (separate from phosphates and sulfates)
            var tankB = tanks[1];
            tankB.TankLabel = "B - Calcium + Acids";

            var tankBFertilizers = new[] { "Ca(NO3)2.2H2O", "CaCl2.6H2O" };
            foreach (var fert in tankBFertilizers)
            {
                if (fertilizers.ContainsKey(fert))
                {
                    tankB.Fertilizers.Add(fert);
                    double concentratedAmount = fertilizers[fert] * concentrationFactor / 1000.0;
                    tankB.Concentrations_gL[fert] = concentratedAmount;
                    tankB.FertilizerAmounts_kg[fert] = (concentratedAmount * tankB.Volume_L) / 1000.0;
                }
            }

            tankB.Acids.AddRange(acids);
        }

        private void DistributeInThreeTanks(List<TankDistribution> tanks, Dictionary<string, double> fertilizers, List<string> acids, int concentrationFactor)
        {
            // Tank A: Phosphates and compatible potassium compounds
            var tankA = tanks[0];
            tankA.TankLabel = "A - Phosphates + K";

            var tankAFertilizers = new[] { "KH2PO4", "KNO3", "K2SO4", "MgSO4.7H2O" };
            foreach (var fert in tankAFertilizers)
            {
                if (fertilizers.ContainsKey(fert))
                {
                    tankA.Fertilizers.Add(fert);
                    double concentratedAmount = fertilizers[fert] * concentrationFactor / 1000.0;
                    tankA.Concentrations_gL[fert] = concentratedAmount;
                    tankA.FertilizerAmounts_kg[fert] = (concentratedAmount * tankA.Volume_L) / 1000.0;
                }
            }

            // Tank B: Micronutrients
            var tankB = tanks[1];
            tankB.TankLabel = "B - Micronutrients";

            var tankBFertilizers = new[] { "FeSO4.7H2O", "FeEDTA", "MnSO4.4H2O", "ZnSO4.7H2O", "CuSO4.5H2O", "H3BO3", "Na2MoO4.2H2O" };
            foreach (var fert in tankBFertilizers)
            {
                if (fertilizers.ContainsKey(fert))
                {
                    tankB.Fertilizers.Add(fert);
                    double concentratedAmount = fertilizers[fert] * concentrationFactor / 1000.0;
                    tankB.Concentrations_gL[fert] = concentratedAmount;
                    tankB.FertilizerAmounts_kg[fert] = (concentratedAmount * tankB.Volume_L) / 1000.0;
                }
            }

            // Tank C: Calcium and Acids
            var tankC = tanks[2];
            tankC.TankLabel = "C - Calcium + Acids";

            var tankCFertilizers = new[] { "Ca(NO3)2.2H2O", "CaCl2.6H2O" };
            foreach (var fert in tankCFertilizers)
            {
                if (fertilizers.ContainsKey(fert))
                {
                    tankC.Fertilizers.Add(fert);
                    double concentratedAmount = fertilizers[fert] * concentrationFactor / 1000.0;
                    tankC.Concentrations_gL[fert] = concentratedAmount;
                    tankC.FertilizerAmounts_kg[fert] = (concentratedAmount * tankC.Volume_L) / 1000.0;
                }
            }

            tankC.Acids.AddRange(acids);
        }

        private void DistributeInFourTanks(List<TankDistribution> tanks, Dictionary<string, double> fertilizers, List<string> acids, int concentrationFactor)
        {
            // Tank A: Phosphates and some Potassium
            var tankA = tanks[0];
            tankA.TankLabel = "A - Phosphates";

            var tankAFertilizers = new[] { "KH2PO4", "NH4H2PO4", "(NH4)2HPO4", "K2HPO4.3H2O" };
            foreach (var fert in tankAFertilizers)
            {
                if (fertilizers.ContainsKey(fert))
                {
                    tankA.Fertilizers.Add(fert);
                    double concentratedAmount = fertilizers[fert] * concentrationFactor / 1000.0;
                    tankA.Concentrations_gL[fert] = concentratedAmount;
                    tankA.FertilizerAmounts_kg[fert] = (concentratedAmount * tankA.Volume_L) / 1000.0;
                }
            }

            // Tank B: Potassium and Magnesium
            var tankB = tanks[1];
            tankB.TankLabel = "B - K + Mg";

            var tankBFertilizers = new[] { "KNO3", "K2SO4", "KCl", "MgSO4.7H2O", "MgCl2.6H2O" };
            foreach (var fert in tankBFertilizers)
            {
                // Continuation from DistributeInFourTanks method - Tank B section
                if (fertilizers.ContainsKey(fert))
                {
                    tankB.Fertilizers.Add(fert);
                    double concentratedAmount = fertilizers[fert] * concentrationFactor / 1000.0;
                    tankB.Concentrations_gL[fert] = concentratedAmount;
                    tankB.FertilizerAmounts_kg[fert] = (concentratedAmount * tankB.Volume_L) / 1000.0;
                }
            }

            // Tank C: Calcium
            var tankC = tanks[2];
            tankC.TankLabel = "C - Calcium";

            var tankCFertilizers = new[] { "Ca(NO3)2.2H2O", "CaCl2.6H2O" };
            foreach (var fert in tankCFertilizers)
            {
                if (fertilizers.ContainsKey(fert))
                {
                    tankC.Fertilizers.Add(fert);
                    double concentratedAmount = fertilizers[fert] * concentrationFactor / 1000.0;
                    tankC.Concentrations_gL[fert] = concentratedAmount;
                    tankC.FertilizerAmounts_kg[fert] = (concentratedAmount * tankC.Volume_L) / 1000.0;
                }
            }

            // Tank D: Micronutrients and Acids
            var tankD = tanks[3];
            tankD.TankLabel = "D - Micros + Acids";

            var tankDFertilizers = new[] { "FeSO4.7H2O", "FeEDTA", "MnSO4.4H2O", "ZnSO4.7H2O", "CuSO4.5H2O", "H3BO3", "Na2MoO4.2H2O" };
            foreach (var fert in tankDFertilizers)
            {
                if (fertilizers.ContainsKey(fert))
                {
                    tankD.Fertilizers.Add(fert);
                    double concentratedAmount = fertilizers[fert] * concentrationFactor / 1000.0;
                    tankD.Concentrations_gL[fert] = concentratedAmount;
                    tankD.FertilizerAmounts_kg[fert] = (concentratedAmount * tankD.Volume_L) / 1000.0;
                }
            }

            tankD.Acids.AddRange(acids);
        }

        private void DistributeInMultipleTanks(List<TankDistribution> tanks, Dictionary<string, double> fertilizers, List<string> acids, int concentrationFactor)
        {
            // For 5+ tanks, distribute each major fertilizer group separately
            int tankIndex = 0;

            // Group fertilizers by compatibility
            var fertilizerGroups = new[]
            {
                new { Name = "Nitrates", Fertilizers = new[] { "NH4NO3", "Ca(NO3)2.2H2O", "KNO3" } },
                new { Name = "Phosphates", Fertilizers = new[] { "KH2PO4", "NH4H2PO4", "(NH4)2HPO4" } },
                new { Name = "Sulfates", Fertilizers = new[] { "K2SO4", "MgSO4.7H2O", "(NH4)2SO4" } },
                new { Name = "Chlorides", Fertilizers = new[] { "KCl", "CaCl2.6H2O", "MgCl2.6H2O" } },
                new { Name = "Micronutrients", Fertilizers = new[] { "FeSO4.7H2O", "FeEDTA", "MnSO4.4H2O", "ZnSO4.7H2O", "CuSO4.5H2O", "H3BO3", "Na2MoO4.2H2O" } }
            };

            foreach (var group in fertilizerGroups)
            {
                if (tankIndex >= tanks.Count - 1) break; // Reserve last tank for acids

                var tank = tanks[tankIndex];
                tank.TankLabel = $"{(char)('A' + tankIndex)} - {group.Name}";

                foreach (var fert in group.Fertilizers)
                {
                    if (fertilizers.ContainsKey(fert))
                    {
                        tank.Fertilizers.Add(fert);
                        double concentratedAmount = fertilizers[fert] * concentrationFactor / 1000.0;
                        tank.Concentrations_gL[fert] = concentratedAmount;
                        tank.FertilizerAmounts_kg[fert] = (concentratedAmount * tank.Volume_L) / 1000.0;
                    }
                }

                if (tank.Fertilizers.Any())
                {
                    tankIndex++;
                }
            }

            // Last tank for acids
            if (tanks.Count > 0 && acids.Any())
            {
                var acidTank = tanks[tanks.Count - 1];
                acidTank.TankLabel = "Acid Tank";
                acidTank.Acids.AddRange(acids);
            }
        }

        private string GetTankLabel(int index)
        {
            return index < 8 ? ((char)('A' + index)).ToString() : $"Tank {index + 1}";
        }

        private void CheckTankCompatibility(TankDistribution tank)
        {
            var fertilizers = tank.Fertilizers;

            for (int i = 0; i < fertilizers.Count; i++)
            {
                for (int j = i + 1; j < fertilizers.Count; j++)
                {
                    var fert1 = fertilizers[i];
                    var fert2 = fertilizers[j];

                    if (compatibilityMatrix.ContainsKey((fert1, fert2)))
                    {
                        var compatibility = compatibilityMatrix[(fert1, fert2)];

                        switch (compatibility)
                        {
                            case CompatibilityLevel.I:
                                tank.CompatibilityWarnings.Add($"🚫 INCOMPATIBLE: {fert1} and {fert2} cannot be mixed - will precipitate");
                                break;
                            case CompatibilityLevel.E:
                                tank.CompatibilityWarnings.Add($"⚠️ CAUTION: {fert1} and {fert2} compatible only in water at injection time");
                                break;
                            case CompatibilityLevel.L:
                                tank.CompatibilityWarnings.Add($"⚠️ LIMITED: {fert1} and {fert2} have limited compatibility - use reduced amounts");
                                break;
                            case CompatibilityLevel.P:
                                tank.CompatibilityWarnings.Add($"🔥 DANGEROUS: {fert1} and {fert2} generate heat - add acid to water carefully");
                                break;
                        }
                    }
                }
            }
        }

        private void CheckTankSolubility(TankDistribution tank, int concentrationFactor)
        {
            foreach (var fertilizer in tank.Concentrations_gL)
            {
                if (fertilizerSolubilities.ContainsKey(fertilizer.Key))
                {
                    var solubility = fertilizerSolubilities[fertilizer.Key];
                    var concentration = fertilizer.Value;

                    if (concentration > solubility.SafeConcentrationLimit)
                    {
                        tank.SolubilityWarnings.Add(
                            $"🚫 SOLUBILITY RISK: {fertilizer.Key} concentration ({concentration:F1} g/L) " +
                            $"exceeds safe limit ({solubility.SafeConcentrationLimit:F1} g/L). " +
                            $"Maximum solubility: {solubility.Solubility_20C:F1} g/L at 20°C. " +
                            $"Recommendation: Reduce concentration factor below 1:{concentrationFactor}"
                        );
                    }
                    else if (concentration > solubility.RecommendedMaxConcentration)
                    {
                        tank.SolubilityWarnings.Add(
                            $"⚠️ CAUTION: {fertilizer.Key} concentration ({concentration:F1} g/L) " +
                            $"approaches safe limit. Recommended max: {solubility.RecommendedMaxConcentration:F1} g/L"
                        );
                    }
                }
            }
        }

        private void CalculateTankDensity(TankDistribution tank)
        {
            tank.TotalDensity_gL = tank.Concentrations_gL.Values.Sum();
        }

        private void CalculateTankCost(TankDistribution tank)
        {
            double totalCost = 0;

            foreach (var fertilizer in tank.FertilizerAmounts_kg)
            {
                if (fertilizerCosts.ContainsKey(fertilizer.Key))
                {
                    totalCost += fertilizer.Value * fertilizerCosts[fertilizer.Key];
                }
            }

            tank.EstimatedCost = totalCost;
        }

        private void GeneratePreparationInstructions(TankDistribution tank)
        {
            tank.PreparationInstructions.Clear();

            tank.PreparationInstructions.Add($"🏷️ Tank {tank.TankLabel} ({tank.TankColor}) - {tank.Volume_L}L");
            tank.PreparationInstructions.Add("📋 Preparation sequence:");
            tank.PreparationInstructions.Add("1. Fill tank with clean water to 80% capacity");
            tank.PreparationInstructions.Add("2. Start mixing/circulation system");

            // Add fertilizers in order of solubility (most soluble first)
            var orderedFertilizers = tank.Fertilizers
                .Where(f => fertilizerSolubilities.ContainsKey(f))
                .OrderByDescending(f => fertilizerSolubilities[f].Solubility_20C)
                .ToList();

            int step = 3;
            foreach (var fertilizer in orderedFertilizers)
            {
                if (tank.FertilizerAmounts_kg.ContainsKey(fertilizer))
                {
                    double amount = tank.FertilizerAmounts_kg[fertilizer];
                    tank.PreparationInstructions.Add($"{step}. Add {amount:F2} kg of {fertilizer} slowly while mixing");
                    tank.PreparationInstructions.Add($"   Wait for complete dissolution before next addition");
                    step++;
                }
            }

            // Add acids last
            foreach (var acid in tank.Acids)
            {
                tank.PreparationInstructions.Add($"{step}. CAREFULLY add {acid} (ALWAYS acid to water, never water to acid)");
                step++;
            }

            tank.PreparationInstructions.Add($"{step}. Fill to final volume ({tank.Volume_L}L)");
            tank.PreparationInstructions.Add($"{step + 1}. Check pH and EC before use");
            tank.PreparationInstructions.Add("⚠️ Safety: Wear gloves, goggles, and ensure good ventilation");
        }

        public Dictionary<string, double> CalculateVolumeRequirements(
            Dictionary<string, double> fertilizerConcentrations_mgL,
            int concentrationFactor,
            double tankVolume_L,
            double targetDilutedVolume_L)
        {
            var results = new Dictionary<string, double>();

            // Calculate total concentrated solution needed
            double concentratedVolumeNeeded_L = targetDilutedVolume_L / concentrationFactor;
            results["ConcentratedVolumeNeeded_L"] = concentratedVolumeNeeded_L;

            // Calculate number of tank preparations needed
            double tankPreparationsNeeded = Math.Ceiling(concentratedVolumeNeeded_L / tankVolume_L);
            results["TankPreparationsNeeded"] = tankPreparationsNeeded;

            // Calculate total fertilizer amounts needed
            foreach (var fertilizer in fertilizerConcentrations_mgL)
            {
                double totalAmount_kg = (fertilizer.Value * concentratedVolumeNeeded_L * concentrationFactor) / 1000000.0;
                results[$"{fertilizer.Key}_Total_kg"] = totalAmount_kg;
            }

            results["TankVolume_L"] = tankVolume_L;
            results["TargetDilutedVolume_L"] = targetDilutedVolume_L;
            results["ConcentrationFactor"] = concentrationFactor;
            results["InjectionRate_LperM3"] = 1000.0 / concentrationFactor;

            return results;
        }

        public ConcentratedSolutionReport GenerateCompleteReport(
            Dictionary<string, double> fertilizerConcentrations_mgL,
            List<string> acidTypes,
            int numberOfTanks,
            int concentrationFactor,
            double tankVolume_L,
            double targetDilutedVolume_L)
        {
            var report = new ConcentratedSolutionReport();

            // Basic parameters
            report.NumberOfTanks = numberOfTanks;
            report.ConcentrationFactor = GetConcentrationFactor(concentrationFactor);

            // Distribute fertilizers
            report.Tanks = DistributeFertilizers(fertilizerConcentrations_mgL, acidTypes, numberOfTanks, concentrationFactor, tankVolume_L);

            // Calculate volume requirements
            report.VolumeRequirements = CalculateVolumeRequirements(fertilizerConcentrations_mgL, concentrationFactor, tankVolume_L, targetDilutedVolume_L);

            // Calculate costs
            report.TotalCost = report.Tanks.Sum(t => t.EstimatedCost);
            report.CostPerM3_DilutedSolution = report.TotalCost / (targetDilutedVolume_L / 1000.0);

            // Calculate fertilizer totals
            foreach (var tank in report.Tanks)
            {
                foreach (var fertilizer in tank.FertilizerAmounts_kg)
                {
                    if (!report.FertilizerTotals_kg.ContainsKey(fertilizer.Key))
                        report.FertilizerTotals_kg[fertilizer.Key] = 0;
                    report.FertilizerTotals_kg[fertilizer.Key] += fertilizer.Value;
                }
            }

            // Generate warnings and recommendations
            GenerateWarningsAndRecommendations(report);

            return report;
        }

        private void GenerateWarningsAndRecommendations(ConcentratedSolutionReport report)
        {
            // Critical warnings
            foreach (var tank in report.Tanks)
            {
                report.CriticalWarnings.AddRange(tank.CompatibilityWarnings.Where(w => w.Contains("INCOMPATIBLE")));
                report.CriticalWarnings.AddRange(tank.SolubilityWarnings.Where(w => w.Contains("SOLUBILITY RISK")));
            }

            // General recommendations
            report.GeneralRecommendations.Add("Always prepare fresh solutions and use within 2-3 days");
            report.GeneralRecommendations.Add("Store concentrated solutions in cool, dark places");
            report.GeneralRecommendations.Add("Check pH and EC of final diluted solution before application");
            report.GeneralRecommendations.Add("Maintain injection equipment regularly to ensure accurate dilution ratios");

            if (report.ConcentrationFactor.Factor > 200)
            {
                report.GeneralRecommendations.Add("High concentration factor - monitor for precipitation during storage");
            }

            if (report.NumberOfTanks == 2)
            {
                report.GeneralRecommendations.Add("Two-tank system: Ensure acids are properly separated from calcium sources");
            }

            // Cost optimization suggestions
            if (report.CostPerM3_DilutedSolution > 5.0)
            {
                report.GeneralRecommendations.Add("Consider bulk purchasing of fertilizers to reduce costs");
            }
        }

        public List<string> ValidateConcentrationFactorForSolubility(
            Dictionary<string, double> fertilizerConcentrations_mgL,
            int concentrationFactor)
        {
            var warnings = new List<string>();

            foreach (var fertilizer in fertilizerConcentrations_mgL)
            {
                if (fertilizerSolubilities.ContainsKey(fertilizer.Key))
                {
                    var solubility = fertilizerSolubilities[fertilizer.Key];
                    double concentratedAmount_gL = fertilizer.Value * concentrationFactor / 1000.0;

                    if (concentratedAmount_gL > solubility.SafeConcentrationLimit)
                    {
                        warnings.Add($"⚠️ {fertilizer.Key}: Concentration factor 1:{concentrationFactor} " +
                                   $"results in {concentratedAmount_gL:F1} g/L, exceeding safe limit of {solubility.SafeConcentrationLimit:F1} g/L");
                    }
                }
            }

            return warnings;
        }

        public int SuggestOptimalConcentrationFactor(Dictionary<string, double> fertilizerConcentrations_mgL)
        {
            int maxSafeFactor = 400; // Start with maximum

            foreach (var fertilizer in fertilizerConcentrations_mgL)
            {
                if (fertilizerSolubilities.ContainsKey(fertilizer.Key))
                {
                    var solubility = fertilizerSolubilities[fertilizer.Key];

                    // Calculate maximum safe concentration factor for this fertilizer
                    int safeFactor = (int)(solubility.RecommendedMaxConcentration * 1000.0 / fertilizer.Value);
                    maxSafeFactor = Math.Min(maxSafeFactor, safeFactor);
                }
            }

            // Round down to nearest standard concentration factor
            var standardFactors = concentrationFactors.Select(cf => cf.Factor).OrderBy(f => f).ToList();
            return standardFactors.Where(f => f <= maxSafeFactor).LastOrDefault();
        }

        public Dictionary<string, object> GetTankSummary(List<TankDistribution> tanks)
        {
            var summary = new Dictionary<string, object>();

            summary["TotalTanks"] = tanks.Count;
            summary["TotalFertilizerTypes"] = tanks.SelectMany(t => t.Fertilizers).Distinct().Count();
            summary["TotalVolume_L"] = tanks.Sum(t => t.Volume_L);
            summary["TotalCost"] = tanks.Sum(t => t.EstimatedCost);
            summary["AverageDensity_gL"] = tanks.Average(t => t.TotalDensity_gL);
            summary["TotalCompatibilityWarnings"] = tanks.Sum(t => t.CompatibilityWarnings.Count);
            summary["TotalSolubilityWarnings"] = tanks.Sum(t => t.SolubilityWarnings.Count);

            // Tank details
            var tankDetails = tanks.Select(t => new
            {
                Label = t.TankLabel,
                FertilizerCount = t.Fertilizers.Count,
                Density_gL = t.TotalDensity_gL,
                Cost = t.EstimatedCost,
                HasWarnings = t.CompatibilityWarnings.Any() || t.SolubilityWarnings.Any()
            }).ToList();

            summary["TankDetails"] = tankDetails;

            return summary;
        }

        // Additional utility methods for advanced operations
        public Dictionary<string, double> CalculateInjectionRates(
            Dictionary<string, double> tankConcentrations_gL,
            int concentrationFactor,
            double targetFlow_Lh)
        {
            var injectionRates = new Dictionary<string, double>();

            foreach (var tank in tankConcentrations_gL)
            {
                // Calculate injection rate needed for this tank
                double injectionRate_Lh = targetFlow_Lh / concentrationFactor;
                injectionRates[tank.Key] = injectionRate_Lh;
            }

            return injectionRates;
        }

        public List<string> GenerateShoppingList(Dictionary<string, double> fertilizerTotals_kg)
        {
            var shoppingList = new List<string>();

            shoppingList.Add("=== FERTILIZER SHOPPING LIST ===");
            shoppingList.Add("");

            double totalCost = 0;

            foreach (var fertilizer in fertilizerTotals_kg.OrderBy(f => f.Key))
            {
                double cost = fertilizerCosts.GetValueOrDefault(fertilizer.Key, 0) * fertilizer.Value;
                totalCost += cost;

                shoppingList.Add($"• {fertilizer.Key}: {fertilizer.Value:F2} kg (${cost:F2})");

                // Add chemical formula if available
                if (fertilizerSolubilities.ContainsKey(fertilizer.Key))
                {
                    shoppingList.Add($"  Formula: {fertilizerSolubilities[fertilizer.Key].Formula}");
                }
            }

            shoppingList.Add("");
            shoppingList.Add($"TOTAL ESTIMATED COST: ${totalCost:F2}");
            shoppingList.Add("");
            shoppingList.Add("Notes:");
            shoppingList.Add("- Prices are estimates and may vary by supplier");
            shoppingList.Add("- Consider bulk discounts for large quantities");
            shoppingList.Add("- Check expiration dates on micronutrients");

            return shoppingList;
        }

        public Dictionary<string, object> AnalyzeNutrientBalance(
            Dictionary<string, double> finalConcentrations_mgL,
            Dictionary<string, double> targetConcentrations_mgL)
        {
            var analysis = new Dictionary<string, object>();
            var deviations = new Dictionary<string, double>();
            var percentDeviations = new Dictionary<string, double>();

            foreach (var target in targetConcentrations_mgL)
            {
                double actual = finalConcentrations_mgL.GetValueOrDefault(target.Key, 0);
                double deviation = actual - target.Value;
                double percentDeviation = target.Value > 0 ? (deviation / target.Value) * 100 : 0;

                deviations[target.Key] = deviation;
                percentDeviations[target.Key] = percentDeviation;
            }

            analysis["Deviations_mgL"] = deviations;
            analysis["PercentDeviations"] = percentDeviations;
            analysis["MaxDeviation"] = deviations.Values.Max(Math.Abs);
            analysis["AverageAbsDeviation"] = deviations.Values.Average(Math.Abs);
            analysis["WithinTolerance"] = percentDeviations.Values.All(d => Math.Abs(d) <= 5.0);

            return analysis;
        }
    }
}