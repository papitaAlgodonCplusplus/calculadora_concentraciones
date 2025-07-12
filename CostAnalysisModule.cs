using System;
using System.Collections.Generic;
using System.Linq;
#pragma warning disable CS8618

namespace HydroponicCalculator.Modules
{
    public class FertilizerCost
    {
        public string Name { get; set; }
        public string Supplier { get; set; }
        public double CostPerKg { get; set; }
        public string Currency { get; set; } = "USD";
        public double PackageSize_kg { get; set; }
        public double CostPerPackage { get; set; }
        public bool IsAvailable { get; set; } = true;
        public DateTime LastUpdated { get; set; } = DateTime.Now;
        public string Notes { get; set; }
    }

    public class NutrientCost
    {
        public string Nutrient { get; set; }
        public double CostPerKg_Nutrient { get; set; }
        public string CheapestSource { get; set; }
        public double TotalAmount_kg { get; set; }
        public double TotalCost { get; set; }
        public List<string> AlternativeSources { get; set; } = new List<string>();
    }

    public class SolutionCostAnalysis
    {
        public double TotalCost_Concentrated { get; set; }
        public double TotalCost_Diluted { get; set; }
        public double CostPerLiter_Concentrated { get; set; }
        public double CostPerLiter_Diluted { get; set; }
        public double CostPerM3_Diluted { get; set; }
        public Dictionary<string, double> CostByFertilizer { get; set; } = new Dictionary<string, double>();
        public Dictionary<string, double> CostByNutrient { get; set; } = new Dictionary<string, double>();
        public Dictionary<string, double> CostByTank { get; set; } = new Dictionary<string, double>();
        public Dictionary<string, double> PercentageByFertilizer { get; set; } = new Dictionary<string, double>();
        public Dictionary<string, double> PercentageByNutrient { get; set; } = new Dictionary<string, double>();
        public List<string> CostOptimizationSuggestions { get; set; } = new List<string>();
    }

    public class CostComparisonResult
    {
        public string FormulationName { get; set; }
        public double TotalCost { get; set; }
        public double CostPerM3 { get; set; }
        public List<string> FertilizersUsed { get; set; } = new List<string>();
        public Dictionary<string, double> NutrientDeviations { get; set; } = new Dictionary<string, double>();
        public double QualityScore { get; set; } // Based on how close to target concentrations
        public string Recommendation { get; set; }
    }

    public class CostAnalysisModule
    {
        private Dictionary<string, FertilizerCost> fertilizerCosts;
        private Dictionary<string, List<string>> nutrientSources; // Which fertilizers provide each nutrient

        public CostAnalysisModule()
        {
            InitializeFertilizerCosts();
            InitializeNutrientSources();
        }

        private void InitializeFertilizerCosts()
        {
            fertilizerCosts = new Dictionary<string, FertilizerCost>
            {
                ["KH2PO4"] = new FertilizerCost
                {
                    Name = "Monopotassium Phosphate",
                    Supplier = "Generic",
                    CostPerKg = 2.50,
                    PackageSize_kg = 25,
                    CostPerPackage = 62.50,
                    IsAvailable = true,
                    Notes = "High quality source for P and K"
                },
                ["Ca(NO3)2.2H2O"] = new FertilizerCost
                {
                    Name = "Calcium Nitrate",
                    Supplier = "Generic",
                    CostPerKg = 0.80,
                    PackageSize_kg = 25,
                    CostPerPackage = 20.00,
                    IsAvailable = true,
                    Notes = "Primary Ca and N source"
                },
                ["KNO3"] = new FertilizerCost
                {
                    Name = "Potassium Nitrate",
                    Supplier = "Generic",
                    CostPerKg = 1.20,
                    PackageSize_kg = 25,
                    CostPerPackage = 30.00,
                    IsAvailable = true,
                    Notes = "Dual K and N source"
                },
                ["K2SO4"] = new FertilizerCost
                {
                    Name = "Potassium Sulfate",
                    Supplier = "Generic",
                    CostPerKg = 1.50,
                    PackageSize_kg = 25,
                    CostPerPackage = 37.50,
                    IsAvailable = true,
                    Notes = "K and S source, low chloride"
                },
                ["MgSO4.7H2O"] = new FertilizerCost
                {
                    Name = "Magnesium Sulfate",
                    Supplier = "Generic",
                    CostPerKg = 0.60,
                    PackageSize_kg = 25,
                    CostPerPackage = 15.00,
                    IsAvailable = true,
                    Notes = "Epsom salt, Mg and S source"
                },
                ["NH4NO3"] = new FertilizerCost
                {
                    Name = "Ammonium Nitrate",
                    Supplier = "Generic",
                    CostPerKg = 0.45,
                    PackageSize_kg = 50,
                    CostPerPackage = 22.50,
                    IsAvailable = true,
                    Notes = "Cheap N source, use with caution"
                },
                ["(NH4)2SO4"] = new FertilizerCost
                {
                    Name = "Ammonium Sulfate",
                    Supplier = "Generic",
                    CostPerKg = 0.50,
                    PackageSize_kg = 50,
                    CostPerPackage = 25.00,
                    IsAvailable = true,
                    Notes = "N and S source"
                },
                ["FeSO4.7H2O"] = new FertilizerCost
                {
                    Name = "Iron Sulfate",
                    Supplier = "Generic",
                    CostPerKg = 1.80,
                    PackageSize_kg = 5,
                    CostPerPackage = 9.00,
                    IsAvailable = true,
                    Notes = "Iron source, may need chelation"
                },
                ["FeEDTA"] = new FertilizerCost
                {
                    Name = "Iron EDTA Chelate",
                    Supplier = "Generic",
                    CostPerKg = 8.50,
                    PackageSize_kg = 5,
                    CostPerPackage = 42.50,
                    IsAvailable = true,
                    Notes = "Chelated iron, more expensive but stable"
                },
                ["H3BO3"] = new FertilizerCost
                {
                    Name = "Boric Acid",
                    Supplier = "Generic",
                    CostPerKg = 3.20,
                    PackageSize_kg = 5,
                    CostPerPackage = 16.00,
                    IsAvailable = true,
                    Notes = "Boron source"
                },
                ["MnSO4.4H2O"] = new FertilizerCost
                {
                    Name = "Manganese Sulfate",
                    Supplier = "Generic",
                    CostPerKg = 2.80,
                    PackageSize_kg = 5,
                    CostPerPackage = 14.00,
                    IsAvailable = true,
                    Notes = "Manganese source"
                },
                ["ZnSO4.7H2O"] = new FertilizerCost
                {
                    Name = "Zinc Sulfate",
                    Supplier = "Generic",
                    CostPerKg = 3.50,
                    PackageSize_kg = 5,
                    CostPerPackage = 17.50,
                    IsAvailable = true,
                    Notes = "Zinc source"
                },
                ["CuSO4.5H2O"] = new FertilizerCost
                {
                    Name = "Copper Sulfate",
                    Supplier = "Generic",
                    CostPerKg = 4.20,
                    PackageSize_kg = 5,
                    CostPerPackage = 21.00,
                    IsAvailable = true,
                    Notes = "Copper source"
                }
            };
        }

        private void InitializeNutrientSources()
        {
            nutrientSources = new Dictionary<string, List<string>>
            {
                ["N"] = new List<string> { "Ca(NO3)2.2H2O", "KNO3", "NH4NO3", "(NH4)2SO4" },
                ["P"] = new List<string> { "KH2PO4", "NH4H2PO4" },
                ["K"] = new List<string> { "KH2PO4", "KNO3", "K2SO4", "KCl" },
                ["Ca"] = new List<string> { "Ca(NO3)2.2H2O", "CaCl2" },
                ["Mg"] = new List<string> { "MgSO4.7H2O", "MgCl2" },
                ["S"] = new List<string> { "K2SO4", "MgSO4.7H2O", "(NH4)2SO4", "FeSO4.7H2O" },
                ["Fe"] = new List<string> { "FeSO4.7H2O", "FeEDTA", "FeCl3" },
                ["B"] = new List<string> { "H3BO3" },
                ["Mn"] = new List<string> { "MnSO4.4H2O" },
                ["Zn"] = new List<string> { "ZnSO4.7H2O" },
                ["Cu"] = new List<string> { "CuSO4.5H2O" },
                ["Mo"] = new List<string> { "Na2MoO4", "(NH4)6Mo7O24" }
            };
        }

        public SolutionCostAnalysis CalculateSolutionCost(
            Dictionary<string, double> fertilizerAmounts_kg,
            double concentratedVolume_L,
            double dilutedVolume_L,
            int concentrationFactor)
        {
            var analysis = new SolutionCostAnalysis();

            // Calculate cost by fertilizer
            double totalCost = 0;
            foreach (var fertilizer in fertilizerAmounts_kg)
            {
                if (fertilizerCosts.ContainsKey(fertilizer.Key))
                {
                    double cost = fertilizer.Value * fertilizerCosts[fertilizer.Key].CostPerKg;
                    analysis.CostByFertilizer[fertilizer.Key] = cost;
                    totalCost += cost;
                }
            }

            analysis.TotalCost_Concentrated = totalCost;
            analysis.TotalCost_Diluted = totalCost; // Same total cost, just different volumes
            analysis.CostPerLiter_Concentrated = concentratedVolume_L > 0 ? totalCost / concentratedVolume_L : 0;
            analysis.CostPerLiter_Diluted = dilutedVolume_L > 0 ? totalCost / dilutedVolume_L : 0;
            analysis.CostPerM3_Diluted = analysis.CostPerLiter_Diluted * 1000;

            // Calculate percentages by fertilizer
            if (totalCost > 0)
            {
                foreach (var cost in analysis.CostByFertilizer)
                {
                    analysis.PercentageByFertilizer[cost.Key] = (cost.Value / totalCost) * 100;
                }
            }

            return analysis;
        }

        public Dictionary<string, NutrientCost> AnalyzeNutrientCosts(
            Dictionary<string, double> targetNutrients_mgL,
            double volume_L)
        {
            var nutrientCosts = new Dictionary<string, NutrientCost>();

            foreach (var nutrient in targetNutrients_mgL)
            {
                if (nutrientSources.ContainsKey(nutrient.Key))
                {
                    var sources = nutrientSources[nutrient.Key];
                    var cheapestCost = FindCheapestNutrientSource(nutrient.Key, nutrient.Value, volume_L, sources);

                    nutrientCosts[nutrient.Key] = new NutrientCost
                    {
                        Nutrient = nutrient.Key,
                        CostPerKg_Nutrient = cheapestCost.costPerKg,
                        CheapestSource = cheapestCost.source,
                        TotalAmount_kg = (nutrient.Value * volume_L) / 1000000, // Convert mg to kg
                        TotalCost = cheapestCost.totalCost,
                        AlternativeSources = sources.Where(s => s != cheapestCost.source).ToList()
                    };
                }
            }

            return nutrientCosts;
        }

        private (double costPerKg, string source, double totalCost) FindCheapestNutrientSource(
            string nutrient, double concentration_mgL, double volume_L, List<string> sources)
        {
            double cheapestCostPerKg = double.MaxValue;
            string cheapestSource = "";
            double cheapestTotalCost = double.MaxValue;

            foreach (var source in sources)
            {
                if (fertilizerCosts.ContainsKey(source))
                {
                    var fertilizerCost = fertilizerCosts[source];
                    double nutrientContent = GetNutrientContentPercentage(source, nutrient);

                    if (nutrientContent > 0)
                    {
                        double costPerKgNutrient = fertilizerCost.CostPerKg / (nutrientContent / 100.0);
                        double totalNutrientNeeded_kg = (concentration_mgL * volume_L) / 1000000.0;
                        double totalCost = totalNutrientNeeded_kg * costPerKgNutrient;

                        if (costPerKgNutrient < cheapestCostPerKg)
                        {
                            cheapestCostPerKg = costPerKgNutrient;
                            cheapestSource = source;
                            cheapestTotalCost = totalCost;
                        }
                    }
                }
            }

            return (cheapestCostPerKg, cheapestSource, cheapestTotalCost);
        }

        private double GetNutrientContentPercentage(string fertilizer, string nutrient)
        {
            // Simplified nutrient content percentages (in reality, these would be from a database)
            var contents = new Dictionary<(string, string), double>
            {
                { ("KH2PO4", "P"), 22.8 },
                { ("KH2PO4", "K"), 28.7 },
                { ("Ca(NO3)2.2H2O", "Ca"), 19.0 },
                { ("Ca(NO3)2.2H2O", "N"), 15.5 },
                { ("KNO3", "K"), 38.7 },
                { ("KNO3", "N"), 13.9 },
                { ("K2SO4", "K"), 44.9 },
                { ("K2SO4", "S"), 18.4 },
                { ("MgSO4.7H2O", "Mg"), 9.9 },
                { ("MgSO4.7H2O", "S"), 13.0 },
                { ("NH4NO3", "N"), 35.0 },
                { ("(NH4)2SO4", "N"), 21.2 },
                { ("(NH4)2SO4", "S"), 24.3 },
                { ("FeSO4.7H2O", "Fe"), 20.1 },
                { ("FeEDTA", "Fe"), 13.0 },
                { ("H3BO3", "B"), 17.5 },
                { ("MnSO4.4H2O", "Mn"), 32.5 },
                { ("ZnSO4.7H2O", "Zn"), 22.7 },
                { ("CuSO4.5H2O", "Cu"), 25.5 }
            };

            return contents.GetValueOrDefault((fertilizer, nutrient), 0);
        }

        public List<CostComparisonResult> CompareFormulations(
            List<Dictionary<string, double>> formulations,
            Dictionary<string, double> targetConcentrations,
            double volume_L)
        {
            var comparisons = new List<CostComparisonResult>();

            for (int i = 0; i < formulations.Count; i++)
            {
                var formulation = formulations[i];
                var comparison = new CostComparisonResult
                {
                    FormulationName = $"Formulation {i + 1}",
                    FertilizersUsed = formulation.Keys.ToList()
                };

                // Calculate total cost
                double totalCost = 0;
                foreach (var fertilizer in formulation)
                {
                    if (fertilizerCosts.ContainsKey(fertilizer.Key))
                    {
                        totalCost += (fertilizer.Value * volume_L / 1000) * fertilizerCosts[fertilizer.Key].CostPerKg;
                    }
                }

                comparison.TotalCost = totalCost;
                comparison.CostPerM3 = totalCost / (volume_L / 1000);

                // Calculate quality score based on target deviations
                double qualityScore = CalculateQualityScore(formulation, targetConcentrations);
                comparison.QualityScore = qualityScore;

                // Generate recommendation
                comparison.Recommendation = GenerateFormulationRecommendation(comparison);

                comparisons.Add(comparison);
            }

            return comparisons.OrderBy(c => c.CostPerM3).ToList();
        }

        private double CalculateQualityScore(Dictionary<string, double> formulation, Dictionary<string, double> targets)
        {
            // Simplified quality scoring - in reality would calculate actual nutrient delivery
            double totalDeviation = 0;
            int nutrientCount = 0;

            foreach (var target in targets)
            {
                // This is a simplified calculation - would need actual nutrient content calculation
                double actualDelivery = EstimateNutrientDelivery(formulation, target.Key);
                double deviation = Math.Abs(actualDelivery - target.Value) / target.Value;
                totalDeviation += deviation;
                nutrientCount++;
            }

            double averageDeviation = nutrientCount > 0 ? totalDeviation / nutrientCount : 1.0;
            return Math.Max(0, 100 - (averageDeviation * 100)); // Score out of 100
        }

        private double EstimateNutrientDelivery(Dictionary<string, double> formulation, string nutrient)
        {
            double totalDelivery = 0;

            foreach (var fertilizer in formulation)
            {
                double content = GetNutrientContentPercentage(fertilizer.Key, nutrient);
                totalDelivery += fertilizer.Value * (content / 100.0);
            }

            return totalDelivery;
        }

        private string GenerateFormulationRecommendation(CostComparisonResult comparison)
        {
            if (comparison.QualityScore >= 95 && comparison.CostPerM3 <= 10)
                return "Excellent: High quality and low cost";
            else if (comparison.QualityScore >= 90)
                return "Good: Meets nutrient targets well";
            else if (comparison.CostPerM3 <= 5)
                return "Economical: Low cost but check nutrient balance";
            else if (comparison.QualityScore < 80)
                return "Poor: Significant nutrient deviations";
            else
                return "Acceptable: Moderate cost and quality";
        }

        public List<string> GenerateCostOptimizationSuggestions(
            SolutionCostAnalysis analysis,
            Dictionary<string, double> fertilizerAmounts)
        {
            var suggestions = new List<string>();

            // Find most expensive fertilizers
            var expensiveFertilizers = analysis.PercentageByFertilizer
                .Where(f => f.Value > 30)
                .OrderByDescending(f => f.Value);

            foreach (var expensive in expensiveFertilizers)
            {
                suggestions.Add($"{expensive.Key} represents {expensive.Value:F1}% of total cost. " +
                               "Consider alternative sources or suppliers.");
            }

            // Check for expensive micronutrients
            var micronutrients = new[] { "FeEDTA", "MnSO4.4H2O", "ZnSO4.7H2O", "CuSO4.5H2O", "H3BO3" };
            var expensiveMicros = analysis.CostByFertilizer
                .Where(f => micronutrients.Contains(f.Key) &&
                           analysis.PercentageByFertilizer[f.Key] > 10);

            if (expensiveMicros.Any())
            {
                suggestions.Add("Micronutrients represent significant cost. " +
                               "Consider using pre-mixed micronutrient blends or chelated forms.");
            }

            // Suggest bulk purchasing
            foreach (var fertilizer in fertilizerAmounts.Where(f => f.Value > 50))
            {
                suggestions.Add($"Large quantity of {fertilizer.Key} needed ({fertilizer.Value:F1} kg). " +
                               "Consider bulk purchasing for better rates.");
            }

            // General cost reduction suggestions
            suggestions.Add("Compare prices from multiple suppliers regularly.");
            suggestions.Add("Consider seasonal purchasing during low-demand periods.");
            suggestions.Add("Evaluate generic vs. branded fertilizers for cost savings.");

            return suggestions;
        }

        public Dictionary<string, object> GenerateCostReport(
            SolutionCostAnalysis costAnalysis,
            Dictionary<string, NutrientCost> nutrientCosts,
            double totalArea_m2,
            double applicationsPerYear = 365)
        {
            var report = new Dictionary<string, object>();

            // Summary costs
            report["TotalCost_PerApplication"] = costAnalysis.TotalCost_Diluted;
            report["CostPerM3_Diluted"] = costAnalysis.CostPerM3_Diluted;
            report["CostPerM2_PerApplication"] = totalArea_m2 > 0 ? costAnalysis.TotalCost_Diluted / totalArea_m2 : 0;
            report["AnnualCost_Total"] = costAnalysis.TotalCost_Diluted * applicationsPerYear;
            report["AnnualCost_PerM2"] = totalArea_m2 > 0 ?
                (costAnalysis.TotalCost_Diluted * applicationsPerYear) / totalArea_m2 : 0;

            // Cost breakdown by fertilizer
            var fertilizerBreakdown = costAnalysis.CostByFertilizer
                .OrderByDescending(f => f.Value)
                .ToDictionary(f => f.Key, f => new
                {
                    Cost = f.Value,
                    Percentage = costAnalysis.PercentageByFertilizer.GetValueOrDefault(f.Key, 0)
                });
            report["CostByFertilizer"] = fertilizerBreakdown;

            // Cost breakdown by nutrient
            var nutrientBreakdown = nutrientCosts
                .OrderByDescending(n => n.Value.TotalCost)
                .ToDictionary(n => n.Key, n => new
                {
                    CostPerKg_Nutrient = n.Value.CostPerKg_Nutrient,
                    TotalCost = n.Value.TotalCost,
                    CheapestSource = n.Value.CheapestSource,
                    Amount_kg = n.Value.TotalAmount_kg
                });
            report["CostByNutrient"] = nutrientBreakdown;

            // Cost efficiency metrics
            report["MostExpensiveNutrient"] = nutrientCosts.OrderByDescending(n => n.Value.CostPerKg_Nutrient).FirstOrDefault().Key;
            report["CheapestNutrient"] = nutrientCosts.OrderBy(n => n.Value.CostPerKg_Nutrient).FirstOrDefault().Key;
            report["AverageNutrientCost_PerKg"] = nutrientCosts.Values.Average(n => n.CostPerKg_Nutrient);

            // Optimization suggestions
            report["OptimizationSuggestions"] = costAnalysis.CostOptimizationSuggestions;

            return report;
        }

        public void UpdateFertilizerCost(string fertilizerName, double newCostPerKg, string supplier = "")
        {
            if (fertilizerCosts.ContainsKey(fertilizerName))
            {
                fertilizerCosts[fertilizerName].CostPerKg = newCostPerKg;
                fertilizerCosts[fertilizerName].LastUpdated = DateTime.Now;
                if (!string.IsNullOrEmpty(supplier))
                {
                    fertilizerCosts[fertilizerName].Supplier = supplier;
                }
            }
            else
            {
                // Add new fertilizer cost
                fertilizerCosts[fertilizerName] = new FertilizerCost
                {
                    Name = fertilizerName,
                    Supplier = supplier ?? "Unknown",
                    CostPerKg = newCostPerKg,
                    IsAvailable = true,
                    LastUpdated = DateTime.Now
                };
            }
        }

        public Dictionary<string, FertilizerCost> GetAllFertilizerCosts()
        {
            return new Dictionary<string, FertilizerCost>(fertilizerCosts);
        }

        public List<string> GetAlternativeFertilizers(string nutrient)
        {
            return nutrientSources.GetValueOrDefault(nutrient, new List<string>());
        }

        public double CalculateBreakEvenVolume(double setupCost, double costPerM3_Current, double costPerM3_Alternative)
        {
            if (Math.Abs(costPerM3_Current - costPerM3_Alternative) < 0.001)
                return double.MaxValue; // No savings possible

            return setupCost / Math.Abs(costPerM3_Current - costPerM3_Alternative);
        }
    }
}