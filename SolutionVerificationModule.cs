using System;
using System.Collections.Generic;
using System.Linq;
#pragma warning disable CS8618

namespace HydroponicCalculator.Modules
{
    public class VerificationResult
    {
        public string Parameter { get; set; }
        public double TargetValue { get; set; }
        public double ActualValue { get; set; }
        public string Unit { get; set; }
        public double Deviation { get; set; }
        public double DeviationPercentage { get; set; }
        public string Status { get; set; } // "OK", "High", "Low", "Critical"
        public string StatusColor { get; set; } // "Green", "Yellow", "Red"
        public string Recommendation { get; set; }
        public double MinAcceptable { get; set; }
        public double MaxAcceptable { get; set; }
    }

    public class IonicRelationshipResult
    {
        public string RelationshipName { get; set; }
        public double ActualRatio { get; set; }
        public double TargetMin { get; set; }
        public double TargetMax { get; set; }
        public string Unit { get; set; }
        public string Status { get; set; }
        public string StatusColor { get; set; }
        public string Recommendation { get; set; }
        public string CropPhase { get; set; }
    }

    public class SolutionVerificationModule
    {
        private Dictionary<string, (double min, double max, double tolerance)> nutrientRanges;
        private Dictionary<string, (double min, double max)> physicalParameterRanges;
        private Dictionary<string, ElementData> elementData;

        public SolutionVerificationModule()
        {
            InitializeRanges();
            InitializeElementData();
        }

        private void InitializeRanges()
        {
            // Nutrient concentration ranges (mg/L) with 5% tolerance by default
            nutrientRanges = new Dictionary<string, (double min, double max, double tolerance)>
            {
                ["N"] = (100, 200, 0.05),
                ["P"] = (30, 50, 0.05),
                ["K"] = (200, 300, 0.05),
                ["Ca"] = (150, 200, 0.05),
                ["Mg"] = (40, 60, 0.05),
                ["S"] = (60, 120, 0.05),
                ["Fe"] = (1.0, 3.0, 0.10),
                ["Mn"] = (0.5, 1.0, 0.10),
                ["Zn"] = (0.2, 0.5, 0.10),
                ["Cu"] = (0.1, 0.3, 0.10),
                ["B"] = (0.3, 0.8, 0.10),
                ["Mo"] = (0.01, 0.05, 0.10)
            };

            // Physical parameter ranges
            physicalParameterRanges = new Dictionary<string, (double min, double max)>
            {
                ["pH"] = (5.5, 6.5),
                ["EC"] = (1.5, 2.5), // dS/m for nutrient solution
                ["Temperature"] = (18, 25) // °C
            };
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
                ["N"] = new ElementData { AtomicWeight = 14.01, Valence = 1, IsCation = false },
                ["SO4"] = new ElementData { AtomicWeight = 96.06, Valence = 2, IsCation = false },
                ["S"] = new ElementData { AtomicWeight = 32.06, Valence = 2, IsCation = false },
                ["Cl"] = new ElementData { AtomicWeight = 35.45, Valence = 1, IsCation = false },
                ["H2PO4"] = new ElementData { AtomicWeight = 96.99, Valence = 1, IsCation = false },
                ["P"] = new ElementData { AtomicWeight = 30.97, Valence = 1, IsCation = false },
                ["HCO3"] = new ElementData { AtomicWeight = 61.02, Valence = 1, IsCation = false }
            };
        }

        public List<VerificationResult> VerifyNutrientConcentrations(
            Dictionary<string, double> targetConcentrations,
            Dictionary<string, double> actualConcentrations)
        {
            var results = new List<VerificationResult>();

            foreach (var target in targetConcentrations)
            {
                string nutrient = target.Key;
                double targetValue = target.Value;
                double actualValue = actualConcentrations.GetValueOrDefault(nutrient, 0);

                if (nutrientRanges.ContainsKey(nutrient))
                {
                    var range = nutrientRanges[nutrient];
                    var result = new VerificationResult
                    {
                        Parameter = nutrient,
                        TargetValue = targetValue,
                        ActualValue = actualValue,
                        Unit = "mg/L",
                        Deviation = actualValue - targetValue,
                        DeviationPercentage = targetValue > 0 ? Math.Abs(actualValue - targetValue) / targetValue * 100 : 0,
                        MinAcceptable = targetValue * (1 - range.tolerance),
                        MaxAcceptable = targetValue * (1 + range.tolerance)
                    };

                    // Determine status
                    if (actualValue >= result.MinAcceptable && actualValue <= result.MaxAcceptable)
                    {
                        result.Status = "OK";
                        result.StatusColor = "Green";
                        result.Recommendation = "Concentration within acceptable range";
                    }
                    else if (actualValue > result.MaxAcceptable)
                    {
                        result.Status = actualValue > targetValue * 1.2 ? "Critical" : "High";
                        result.StatusColor = result.Status == "Critical" ? "Red" : "Orange";
                        result.Recommendation = $"Concentration too high. Reduce fertilizer input or increase dilution.";
                    }
                    else
                    {
                        result.Status = actualValue < targetValue * 0.8 ? "Critical" : "Low";
                        result.StatusColor = result.Status == "Critical" ? "Red" : "Yellow";
                        result.Recommendation = $"Concentration too low. Increase fertilizer input.";
                    }

                    results.Add(result);
                }
            }

            return results;
        }

        public List<VerificationResult> VerifyPhysicalParameters(
            double pH, double ec, double temperature = 20.0)
        {
            var results = new List<VerificationResult>();

            // Verify pH
            var phRange = physicalParameterRanges["pH"];
            results.Add(new VerificationResult
            {
                Parameter = "pH",
                ActualValue = pH,
                Unit = "pH units",
                MinAcceptable = phRange.min,
                MaxAcceptable = phRange.max,
                Status = pH >= phRange.min && pH <= phRange.max ? "OK" :
                        pH > phRange.max ? "High" : "Low",
                StatusColor = pH >= phRange.min && pH <= phRange.max ? "Green" : "Red",
                Recommendation = pH >= phRange.min && pH <= phRange.max ?
                    "pH within optimal range" :
                    pH > phRange.max ? "pH too high - increase acid addition" :
                    "pH too low - reduce acid addition"
            });

            // Verify EC
            var ecRange = physicalParameterRanges["EC"];
            results.Add(new VerificationResult
            {
                Parameter = "EC",
                ActualValue = ec,
                Unit = "dS/m",
                MinAcceptable = ecRange.min,
                MaxAcceptable = ecRange.max,
                Status = ec >= ecRange.min && ec <= ecRange.max ? "OK" :
                        ec > ecRange.max ? "High" : "Low",
                StatusColor = ec >= ecRange.min && ec <= ecRange.max ? "Green" : "Red",
                Recommendation = ec >= ecRange.min && ec <= ecRange.max ?
                    "EC within optimal range" :
                    ec > ecRange.max ? "EC too high - dilute solution" :
                    "EC too low - increase fertilizer concentration"
            });

            return results;
        }

        public Dictionary<string, double> VerifyIonBalance(Dictionary<string, double> finalConcentrations_meqL)
        {
            var results = new Dictionary<string, double>();

            double sumCations = 0;
            double sumAnions = 0;

            foreach (var ion in finalConcentrations_meqL)
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
            results["Tolerance"] = Math.Min(sumCations, sumAnions) * 0.1; // 10% tolerance

            return results;
        }

        public List<IonicRelationshipResult> VerifyIonicRelationships(
            Dictionary<string, double> concentrations_meqL,
            Dictionary<string, double> concentrations_mmolL,
            Dictionary<string, double> concentrations_mgL,
            string cropPhase = "General")
        {
            var results = new List<IonicRelationshipResult>();

            // K:Ca:Mg ratio in meq/L (typical target: 4:4:1 to 6:4:2)
            double k_meq = concentrations_meqL.GetValueOrDefault("K", 0);
            double ca_meq = concentrations_meqL.GetValueOrDefault("Ca", 0);
            double mg_meq = concentrations_meqL.GetValueOrDefault("Mg", 0);

            if (ca_meq > 0 && mg_meq > 0)
            {
                double k_ca_ratio = k_meq / ca_meq;
                double ca_mg_ratio = ca_meq / mg_meq;

                results.Add(new IonicRelationshipResult
                {
                    RelationshipName = "K:Ca ratio",
                    ActualRatio = k_ca_ratio,
                    TargetMin = 0.8,
                    TargetMax = 1.5,
                    Unit = "meq/L ratio",
                    Status = k_ca_ratio >= 0.8 && k_ca_ratio <= 1.5 ? "OK" : "Imbalanced",
                    StatusColor = k_ca_ratio >= 0.8 && k_ca_ratio <= 1.5 ? "Green" : "Orange",
                    Recommendation = k_ca_ratio >= 0.8 && k_ca_ratio <= 1.5 ?
                        "K:Ca ratio balanced" :
                        k_ca_ratio > 1.5 ? "Too much K relative to Ca" : "Too much Ca relative to K",
                    CropPhase = cropPhase
                });

                results.Add(new IonicRelationshipResult
                {
                    RelationshipName = "Ca:Mg ratio",
                    ActualRatio = ca_mg_ratio,
                    TargetMin = 2.0,
                    TargetMax = 4.0,
                    Unit = "meq/L ratio",
                    Status = ca_mg_ratio >= 2.0 && ca_mg_ratio <= 4.0 ? "OK" : "Imbalanced",
                    StatusColor = ca_mg_ratio >= 2.0 && ca_mg_ratio <= 4.0 ? "Green" : "Orange",
                    Recommendation = ca_mg_ratio >= 2.0 && ca_mg_ratio <= 4.0 ?
                        "Ca:Mg ratio balanced" :
                        ca_mg_ratio > 4.0 ? "Too much Ca relative to Mg" : "Too much Mg relative to Ca",
                    CropPhase = cropPhase
                });
            }

            // N/K ratio in mmol/L (typical: 1.0-1.5 for vegetative, >1.5 for generative)
            double n_mmol = concentrations_mmolL.GetValueOrDefault("N", 0);
            double k_mmol = concentrations_mmolL.GetValueOrDefault("K", 0);

            if (k_mmol > 0)
            {
                double n_k_ratio_mmol = n_mmol / k_mmol;
                double targetMin = cropPhase == "Vegetative" ? 1.0 : 1.5;
                double targetMax = cropPhase == "Vegetative" ? 1.5 : 2.5;

                results.Add(new IonicRelationshipResult
                {
                    RelationshipName = "N/K ratio (mmol/L)",
                    ActualRatio = n_k_ratio_mmol,
                    TargetMin = targetMin,
                    TargetMax = targetMax,
                    Unit = "mmol/L ratio",
                    Status = n_k_ratio_mmol >= targetMin && n_k_ratio_mmol <= targetMax ? "OK" : "Imbalanced",
                    StatusColor = n_k_ratio_mmol >= targetMin && n_k_ratio_mmol <= targetMax ? "Green" : "Orange",
                    Recommendation = n_k_ratio_mmol >= targetMin && n_k_ratio_mmol <= targetMax ?
                        $"N/K ratio optimal for {cropPhase.ToLower()} growth" :
                        n_k_ratio_mmol > targetMax ? $"High N/K ratio - promotes {(cropPhase == "Vegetative" ? "generative" : "excessive vegetative")} growth" :
                        $"Low N/K ratio - promotes {(cropPhase == "Vegetative" ? "generative" : "vegetative")} growth",
                    CropPhase = cropPhase
                });
            }

            // N/K ratio in mg/L (typical: 1.0-1.5 for leaf crops, >1.5 for fruit crops)
            double n_mg = concentrations_mgL.GetValueOrDefault("N", 0);
            double k_mg = concentrations_mgL.GetValueOrDefault("K", 0);

            if (k_mg > 0)
            {
                double n_k_ratio_mg = n_mg / k_mg;
                double targetMin_mg = cropPhase == "Vegetative" ? 1.0 : 1.5;
                double targetMax_mg = cropPhase == "Vegetative" ? 1.5 : 2.5;

                results.Add(new IonicRelationshipResult
                {
                    RelationshipName = "N/K ratio (mg/L)",
                    ActualRatio = n_k_ratio_mg,
                    TargetMin = targetMin_mg,
                    TargetMax = targetMax_mg,
                    Unit = "mg/L ratio",
                    Status = n_k_ratio_mg >= targetMin_mg && n_k_ratio_mg <= targetMax_mg ? "OK" : "Imbalanced",
                    StatusColor = n_k_ratio_mg >= targetMin_mg && n_k_ratio_mg <= targetMax_mg ? "Green" : "Orange",
                    Recommendation = n_k_ratio_mg >= targetMin_mg && n_k_ratio_mg <= targetMax_mg ?
                        $"N/K ratio suitable for {cropPhase.ToLower()} phase" :
                        n_k_ratio_mg > targetMax_mg ? "High N/K - favors vegetative growth" :
                        "Low N/K - favors generative growth",
                    CropPhase = cropPhase
                });
            }

            // NH4/NO3 ratio (should be < 20% of total N)
            double nh4_mg = concentrations_mgL.GetValueOrDefault("NH4", 0);
            double no3_mg = concentrations_mgL.GetValueOrDefault("NO3", 0);
            double totalN_ionic = (nh4_mg * 14.01 / 18.04) + (no3_mg * 14.01 / 62.00); // Convert to N equivalents

            if (totalN_ionic > 0)
            {
                double nh4_percentage = (nh4_mg * 14.01 / 18.04) / totalN_ionic * 100;

                results.Add(new IonicRelationshipResult
                {
                    RelationshipName = "NH4 percentage of total N",
                    ActualRatio = nh4_percentage,
                    TargetMin = 0,
                    TargetMax = 20,
                    Unit = "%",
                    Status = nh4_percentage <= 20 ? "OK" : "High",
                    StatusColor = nh4_percentage <= 20 ? "Green" : "Red",
                    Recommendation = nh4_percentage <= 20 ?
                        "NH4 levels safe" :
                        "NH4 levels too high - can be toxic to plants",
                    CropPhase = cropPhase
                });
            }

            return results;
        }

        public double CalculateEC(Dictionary<string, double> concentrations_meqL)
        {
            // Simplified EC calculation: EC ≈ 0.1 × sum of cations (meq/L)
            double sumCations = 0;
            foreach (var ion in concentrations_meqL)
            {
                if (elementData.ContainsKey(ion.Key) && elementData[ion.Key].IsCation)
                {
                    sumCations += ion.Value;
                }
            }
            return sumCations * 0.1; // dS/m
        }

        public Dictionary<string, object> GenerateVerificationSummary(
            List<VerificationResult> nutrientResults,
            List<VerificationResult> physicalResults,
            List<IonicRelationshipResult> relationshipResults,
            Dictionary<string, double> ionBalance)
        {
            var summary = new Dictionary<string, object>();

            // Count status types
            int totalChecks = nutrientResults.Count + physicalResults.Count + relationshipResults.Count;
            int okCount = nutrientResults.Count(r => r.Status == "OK") +
                         physicalResults.Count(r => r.Status == "OK") +
                         relationshipResults.Count(r => r.Status == "OK");
            int warningCount = nutrientResults.Count(r => r.Status == "High" || r.Status == "Low") +
                              physicalResults.Count(r => r.Status == "High" || r.Status == "Low") +
                              relationshipResults.Count(r => r.Status == "Imbalanced");
            int criticalCount = nutrientResults.Count(r => r.Status == "Critical") +
                               physicalResults.Count(r => r.Status == "Critical");

            summary["TotalChecks"] = totalChecks;
            summary["OKCount"] = okCount;
            summary["WarningCount"] = warningCount;
            summary["CriticalCount"] = criticalCount;
            summary["OverallStatus"] = criticalCount > 0 ? "Critical" : warningCount > 0 ? "Warning" : "OK";
            summary["OverallColor"] = criticalCount > 0 ? "Red" : warningCount > 0 ? "Orange" : "Green";
            summary["SuccessRate"] = totalChecks > 0 ? (double)okCount / totalChecks * 100 : 0;

            // Ion balance status
            summary["IonBalanceStatus"] = ionBalance["IsBalanced"] == 1 ? "Balanced" : "Imbalanced";
            summary["IonBalancePercentage"] = ionBalance["PercentageDifference"];

            // Critical issues
            var criticalIssues = new List<string>();
            criticalIssues.AddRange(nutrientResults.Where(r => r.Status == "Critical").Select(r => $"{r.Parameter}: {r.Recommendation}"));
            criticalIssues.AddRange(physicalResults.Where(r => r.Status == "Critical").Select(r => $"{r.Parameter}: {r.Recommendation}"));
            if (ionBalance["IsBalanced"] == 0)
            {
                criticalIssues.Add($"Ion balance: {ionBalance["PercentageDifference"]:F1}% difference exceeds 10% tolerance");
            }
            summary["CriticalIssues"] = criticalIssues;

            return summary;
        }
    }
}