using System;
using System.Collections.Generic;
using System.Linq;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

public class IonBalance
{
    public Dictionary<string, double> Aporte_mgL { get; set; } = new Dictionary<string, double>();
    public Dictionary<string, double> Aporte_mmolL { get; set; } = new Dictionary<string, double>();
    public Dictionary<string, double> Aporte_meqL { get; set; } = new Dictionary<string, double>();

    public Dictionary<string, double> Agua_mgL { get; set; } = new Dictionary<string, double>();
    public Dictionary<string, double> Agua_mmolL { get; set; } = new Dictionary<string, double>();
    public Dictionary<string, double> Agua_meqL { get; set; } = new Dictionary<string, double>();

    public Dictionary<string, double> Final_mgL { get; set; } = new Dictionary<string, double>();
    public Dictionary<string, double> Final_mmolL { get; set; } = new Dictionary<string, double>();
    public Dictionary<string, double> Final_meqL { get; set; } = new Dictionary<string, double>();
}
public class FertilizerResult
{
    public string Name { get; set; }
    public double Purity { get; set; } // Porcentaje de pureza
    public double MolecularWeight { get; set; } // Peso molecular en g/mol
    public double SaltConcentration_mgL { get; set; } // Concentración de sal en mg/L
    public double SaltConcentration_mmolL { get; set; } // Concentración de sal en mmol/L

    // Elementos aportados por el fertilizante
    public double Ca { get; set; }
    public double K { get; set; }
    public double Mg { get; set; }
    public double Na { get; set; }
    public double NH4 { get; set; }
    public double N { get; set; }
    public double P { get; set; }
    public double S { get; set; }
    public double Cl { get; set; }
    public double Fe { get; set; }
    public double Mn { get; set; }
    public double Zn { get; set; }
    public double Cu { get; set; }
    public double B { get; set; }
    public double Mo { get; set; }

    // Formas iónicas
    public double NO3 { get; set; } // Nitrato
    public double H2PO4 { get; set; } // Dihidrógeno fosfato
    public double SO4 { get; set; } // Sulfato

    public double HCO3 { get; set; } // Bicarbonato
}
public class Fertilizer
{
    public string Name { get; set; }
    public double Purity { get; set; } // Porcentaje de pureza
    public double MolecularWeight { get; set; } // Peso molecular en g/mol
    public Dictionary<string, double> Elements { get; set; } = new Dictionary<string, double>(); // Elementos y sus pesos atómicos
    public double Solubility { get; set; } // Solubilidad en g/L
    public double Cost { get; set; } // Costo por kg

    public Fertilizer(string name, double purity, double molecularWeight)
    {
        Name = name;
        Purity = purity;
        MolecularWeight = molecularWeight;
    }
}

public class ElementData
{
    public double AtomicWeight { get; set; } // Peso atómico en g/mol
    public int Valence { get; set; } // Carga iónica
    public bool IsCation { get; set; } // Indica si es un catión
}

public class WaterAnalysis
{
    public Dictionary<string, double> Elements_mgL { get; set; } = new Dictionary<string, double>();
    public Dictionary<string, double> Elements_mmolL { get; set; } = new Dictionary<string, double>();
    public Dictionary<string, double> Elements_meqL { get; set; } = new Dictionary<string, double>();
    public double pH { get; set; }
    public double EC { get; set; } // Conductividad eléctrica en mS/cm
    public double HCO3 { get; set; } // Bicarbonato en mg/L
}

namespace HydroponicCalculator.Modules
{
    public class NutrientCalculatorAdvanced
    {
        private List<Fertilizer>? fertilizers;
        private WaterAnalysis waterAnalysis;
        private Dictionary<string, double> targetConcentrations;
        private Dictionary<string, ElementData>? elementData;

        public NutrientCalculatorAdvanced()
        {
            InitializeFertilizers();
            InitializeElementData();
            targetConcentrations = new Dictionary<string, double>();
            waterAnalysis = new WaterAnalysis();
            SetDefaultValues();
        }

        private void SetDefaultValues()
        {
            // Set target concentrations: Ca, K, Mg, N, P, S
            targetConcentrations["Ca"] = 172;
            targetConcentrations["K"] = 260;
            targetConcentrations["Mg"] = 50;
            targetConcentrations["N"] = 150;
            targetConcentrations["P"] = 45;
            targetConcentrations["S"] = 108;

            // Set default water analysis values (mg/L)
            waterAnalysis.Elements_mgL["Ca"] = 10;
            waterAnalysis.Elements_mgL["K"] = 2;
            waterAnalysis.Elements_mgL["Mg"] = 5;
            waterAnalysis.Elements_mgL["N"] = 0;
            waterAnalysis.Elements_mgL["P"] = 0;
            waterAnalysis.Elements_mgL["S"] = 0;
            waterAnalysis.pH = 7.2;
            waterAnalysis.EC = 0.5;
            waterAnalysis.HCO3 = 77; // mg/L

            // Calculate mmol/L and meq/L for the water
            foreach (var element in waterAnalysis.Elements_mgL)
            {
                if (elementData?.ContainsKey(element.Key) == true)
                {
                    waterAnalysis.Elements_mmolL[element.Key] = element.Value / elementData[element.Key].AtomicWeight;
                    waterAnalysis.Elements_meqL[element.Key] = waterAnalysis.Elements_mmolL[element.Key] * elementData[element.Key].Valence;
                }
            }
        }

        public Dictionary<string, double> GetTargetConcentrations() => targetConcentrations;
        public WaterAnalysis GetWaterAnalysis() => waterAnalysis;

        public void UpdateTargetConcentrations(Dictionary<string, double> newTargets)
        {
            foreach (var target in newTargets)
            {
                targetConcentrations[target.Key] = target.Value;
            }
        }

        public void UpdateWaterAnalysis(WaterAnalysis newWaterAnalysis)
        {
            waterAnalysis = newWaterAnalysis;
        }

        private void InitializeElementData()
        {
            elementData = new Dictionary<string, ElementData>
            {
                // Cationes
                ["Ca"] = new ElementData { AtomicWeight = 40.08, Valence = 2, IsCation = true },
                ["K"] = new ElementData { AtomicWeight = 39.10, Valence = 1, IsCation = true },
                ["Mg"] = new ElementData { AtomicWeight = 24.31, Valence = 2, IsCation = true },
                ["Na"] = new ElementData { AtomicWeight = 22.99, Valence = 1, IsCation = true },
                ["NH4"] = new ElementData { AtomicWeight = 18.04, Valence = 1, IsCation = true },

                // Aniones
                ["NO3"] = new ElementData { AtomicWeight = 62.00, Valence = 1, IsCation = false }, // NO3-
                ["N"] = new ElementData { AtomicWeight = 14.01, Valence = 1, IsCation = false },
                ["SO4"] = new ElementData { AtomicWeight = 96.06, Valence = 2, IsCation = false }, // SO4=
                ["S"] = new ElementData { AtomicWeight = 32.06, Valence = 2, IsCation = false },
                ["Cl"] = new ElementData { AtomicWeight = 35.45, Valence = 1, IsCation = false }, // Cl-
                ["H2PO4"] = new ElementData { AtomicWeight = 96.99, Valence = 1, IsCation = false }, // H2PO4-
                ["P"] = new ElementData { AtomicWeight = 30.97, Valence = 1, IsCation = false },
                ["HCO3"] = new ElementData { AtomicWeight = 61.02, Valence = 1, IsCation = false }, // HCO3-

                // Micronutrients
                ["Fe"] = new ElementData { AtomicWeight = 55.85, Valence = 2, IsCation = true },
                ["Mn"] = new ElementData { AtomicWeight = 54.94, Valence = 2, IsCation = true },
                ["Zn"] = new ElementData { AtomicWeight = 65.38, Valence = 2, IsCation = true },
                ["Cu"] = new ElementData { AtomicWeight = 63.55, Valence = 2, IsCation = true },
                ["B"] = new ElementData { AtomicWeight = 10.81, Valence = 3, IsCation = false },
                ["Mo"] = new ElementData { AtomicWeight = 95.96, Valence = 6, IsCation = false }
            };
        }

        private void InitializeFertilizers()
        {
            fertilizers = new List<Fertilizer>();

            // Ácido nítrico
            var nitricAcid = new Fertilizer("Ácido nítrico", 65, 62.00);
            nitricAcid.Elements["N"] = 14.01;
            nitricAcid.Solubility = 1000000; // Highly soluble
            nitricAcid.Cost = 1.20;
            fertilizers.Add(nitricAcid);

            // Ácido fosfórico
            var phosphoricAcid = new Fertilizer("Ácido fosfórico", 85, 98.00);
            phosphoricAcid.Elements["P"] = 30.97;
            phosphoricAcid.Solubility = 1000000; // Highly soluble
            phosphoricAcid.Cost = 1.50;
            fertilizers.Add(phosphoricAcid);

            // KH2PO4
            var kh2po4 = new Fertilizer("KH2PO4", 98, 136.19);
            kh2po4.Elements["K"] = 39.10;
            kh2po4.Elements["P"] = 30.97;
            kh2po4.Solubility = 227; // g/L at 20°C
            kh2po4.Cost = 2.50;
            fertilizers.Add(kh2po4);

            // Ca(NO3)2.2H2O
            var caNO3 = new Fertilizer("Ca(NO3)2.2H2O", 95, 200.12);
            caNO3.Elements["Ca"] = 40.08;
            caNO3.Elements["N"] = 28.01; // 2 átomos de N
            caNO3.Solubility = 1200; // g/L at 20°C
            caNO3.Cost = 0.80;
            fertilizers.Add(caNO3);

            // KNO3
            var kno3 = new Fertilizer("KNO3", 98, 101.13);
            kno3.Elements["K"] = 39.10;
            kno3.Elements["N"] = 14.01;
            kno3.Solubility = 335; // g/L at 20°C
            kno3.Cost = 1.20;
            fertilizers.Add(kno3);

            // K2SO4
            var k2so4 = new Fertilizer("K2SO4", 98, 174.37);
            k2so4.Elements["K"] = 78.20; // 2 átomos de K
            k2so4.Elements["S"] = 32.06;
            k2so4.Solubility = 111; // g/L at 20°C
            k2so4.Cost = 1.50;
            fertilizers.Add(k2so4);

            // MgSO4.7H2O
            var mgso4 = new Fertilizer("MgSO4.7H2O", 98, 246.32);
            mgso4.Elements["Mg"] = 24.31;
            mgso4.Elements["S"] = 32.06;
            mgso4.Solubility = 710; // g/L at 20°C
            mgso4.Cost = 0.60;
            fertilizers.Add(mgso4);

            // NH4NO3
            var nh4no3 = new Fertilizer("NH4NO3", 99, 80.04);
            nh4no3.Elements["N"] = 28.01; // 2 átomos de N
            nh4no3.Elements["NH4"] = 18.04;
            nh4no3.Solubility = 1900; // g/L at 20°C
            nh4no3.Cost = 0.45;
            fertilizers.Add(nh4no3);

            // (NH4)2SO4
            var nh4so4 = new Fertilizer("(NH4)2SO4", 99, 132.14);
            nh4so4.Elements["N"] = 28.01; // 2 átomos de N
            nh4so4.Elements["NH4"] = 36.08; // 2 átomos de NH4
            nh4so4.Elements["S"] = 32.06;
            nh4so4.Solubility = 760; // g/L at 20°C
            nh4so4.Cost = 0.50;
            fertilizers.Add(nh4so4);

            // KCl
            var kcl = new Fertilizer("KCl", 99, 74.55);
            kcl.Elements["K"] = 39.10;
            kcl.Elements["Cl"] = 35.45;
            kcl.Solubility = 342; // g/L at 20°C
            kcl.Cost = 0.90;
            fertilizers.Add(kcl);

            // CaCl2.2H2O
            var cacl2 = new Fertilizer("CaCl2.2H2O", 99, 147.02);
            cacl2.Elements["Ca"] = 40.08;
            cacl2.Elements["Cl"] = 70.90; // 2 átomos de Cl
            cacl2.Solubility = 600; // g/L at 20°C
            cacl2.Cost = 0.70;
            fertilizers.Add(cacl2);

            // Micronutrientes
            // FeEDTA
            var feEDTA = new Fertilizer("FeEDTA", 99, 367.05);
            feEDTA.Elements["Fe"] = 55.85;
            feEDTA.Solubility = 1000; // Highly soluble
            feEDTA.Cost = 8.50;
            fertilizers.Add(feEDTA);

            // FeSO4.7H2O
            var feso4 = new Fertilizer("FeSO4.7H2O", 99, 278.02);
            feso4.Elements["Fe"] = 55.85;
            feso4.Elements["S"] = 32.06;
            feso4.Solubility = 260; // g/L at 20°C
            feso4.Cost = 1.80;
            fertilizers.Add(feso4);

            // MnSO4.4H2O
            var mnso4 = new Fertilizer("MnSO4.4H2O", 99, 223.06);
            mnso4.Elements["Mn"] = 54.94;
            mnso4.Elements["S"] = 32.06;
            mnso4.Solubility = 1053; // g/L at 20°C
            mnso4.Cost = 2.80;
            fertilizers.Add(mnso4);

            // ZnSO4.7H2O
            var znso4 = new Fertilizer("ZnSO4.7H2O", 99, 287.54);
            znso4.Elements["Zn"] = 65.38;
            znso4.Elements["S"] = 32.06;
            znso4.Solubility = 750; // g/L at 20°C
            znso4.Cost = 3.50;
            fertilizers.Add(znso4);

            // CuSO4.5H2O
            var cuso4 = new Fertilizer("CuSO4.5H2O", 99, 249.68);
            cuso4.Elements["Cu"] = 63.55;
            cuso4.Elements["S"] = 32.06;
            cuso4.Solubility = 316; // g/L at 20°C
            cuso4.Cost = 4.20;
            fertilizers.Add(cuso4);

            // H3BO3
            var h3bo3 = new Fertilizer("H3BO3", 99, 61.83);
            h3bo3.Elements["B"] = 10.81;
            h3bo3.Solubility = 63.5; // g/L at 20°C
            h3bo3.Cost = 3.20;
            fertilizers.Add(h3bo3);

            // Na2MoO4.2H2O
            var namoo4 = new Fertilizer("Na2MoO4.2H2O", 99, 241.95);
            namoo4.Elements["Mo"] = 95.96;
            namoo4.Elements["Na"] = 45.98; // 2 átomos de Na
            namoo4.Solubility = 840; // g/L at 20°C
            namoo4.Cost = 15.00;
            fertilizers.Add(namoo4);
        }

        public List<FertilizerResult> CalculateSolution()
        {
            var results = new List<FertilizerResult>();
            var remainingNutrients = new Dictionary<string, double>();

            // Calcular nutrientes faltantes
            foreach (var target in targetConcentrations)
            {
                double waterContent = waterAnalysis.Elements_mgL.ContainsKey(target.Key)
                    ? waterAnalysis.Elements_mgL[target.Key] : 0;
                remainingNutrients[target.Key] = Math.Max(0, target.Value - waterContent);
            }

            // Algoritmo optimizado para minimizar excesos
            results = CalculateOptimizedSolution(remainingNutrients);

            return results;
        }

        private List<FertilizerResult> CalculateOptimizedSolution(Dictionary<string, double> remainingNutrients)
        {
            var results = new List<FertilizerResult>();

            // 1. KH2PO4 para fósforo (siempre primero)
            if (remainingNutrients.ContainsKey("P") && remainingNutrients["P"] > 0)
            {
                var result = CalculateFertilizer("KH2PO4", "P", remainingNutrients["P"]);
                results.Add(result);

                // Actualizar K restante
                if (remainingNutrients.ContainsKey("K"))
                    remainingNutrients["K"] = Math.Max(0, remainingNutrients["K"] - result.K);
            }

            // 2. Decisión inteligente para Calcio
            double nRequired = remainingNutrients.GetValueOrDefault("N", 0);
            double caRequired = remainingNutrients.GetValueOrDefault("Ca", 0);

            if (caRequired > 0)
            {
                // Calcular cuánto N aportaría Ca(NO3)2 para todo el Ca
                double nFromFullCaNO3 = CalculateNutrientContribution("Ca(NO3)2.2H2O", "N", caRequired, "Ca");

                if (nFromFullCaNO3 > nRequired * 1.5) // Si excede significativamente
                {
                    // Usar solo la cantidad de Ca(NO3)2 necesaria para el N
                    if (nRequired > 0)
                    {
                        var caNO3Result = CalculateFertilizer("Ca(NO3)2.2H2O", "N", nRequired);
                        results.Add(caNO3Result);

                        remainingNutrients["N"] = 0;
                        remainingNutrients["Ca"] = Math.Max(0, remainingNutrients["Ca"] - caNO3Result.Ca);
                    }
                }
                else
                {
                    // Usar Ca(NO3)2 para todo el calcio
                    var result = CalculateFertilizer("Ca(NO3)2.2H2O", "Ca", caRequired);
                    results.Add(result);

                    remainingNutrients["N"] = Math.Max(0, remainingNutrients["N"] - result.N);
                    remainingNutrients["Ca"] = 0;
                }
            }

            // 3. MgSO4.7H2O para magnesio
            if (remainingNutrients.ContainsKey("Mg") && remainingNutrients["Mg"] > 0)
            {
                var result = CalculateFertilizer("MgSO4.7H2O", "Mg", remainingNutrients["Mg"]);
                results.Add(result);

                remainingNutrients["Mg"] = 0;
                if (remainingNutrients.ContainsKey("S"))
                    remainingNutrients["S"] = Math.Max(0, remainingNutrients["S"] - result.S);
            }

            // 4. KNO3 para nitrógeno restante (si queda)
            if (remainingNutrients.ContainsKey("N") && remainingNutrients["N"] > 0)
            {
                var result = CalculateFertilizer("KNO3", "N", remainingNutrients["N"]);
                results.Add(result);

                remainingNutrients["N"] = 0;
                if (remainingNutrients.ContainsKey("K"))
                    remainingNutrients["K"] = Math.Max(0, remainingNutrients["K"] - result.K);
            }

            // 5. Decisión inteligente para Potasio restante
            double kRequired = remainingNutrients.GetValueOrDefault("K", 0);
            double sRequired = remainingNutrients.GetValueOrDefault("S", 0);

            if (kRequired > 0)
            {
                if (sRequired > 0)
                {
                    // Evaluar si K2SO4 puede cubrir ambos
                    double k2so4ForS = CalculateFertilizerAmount("K2SO4", "S", sRequired);
                    double kFromK2SO4 = CalculateElementContribution(k2so4ForS, "K2SO4", "K");

                    if (kFromK2SO4 <= kRequired * 1.2) // Si no excede mucho el K requerido
                    {
                        var result = CalculateFertilizer("K2SO4", "S", sRequired);
                        results.Add(result);
                        remainingNutrients["S"] = 0;
                        remainingNutrients["K"] = Math.Max(0, remainingNutrients["K"] - result.K);
                    }
                    else
                    {
                        // Usar K2SO4 para todo el K requerido
                        var result = CalculateFertilizer("K2SO4", "K", kRequired);
                        results.Add(result);
                        remainingNutrients["K"] = 0;
                        remainingNutrients["S"] = Math.Max(0, remainingNutrients["S"] - result.S);
                    }
                }
                else
                {
                    // No se necesita azufre, usar K2SO4 para K
                    var result = CalculateFertilizer("K2SO4", "K", kRequired);
                    results.Add(result);
                    remainingNutrients["K"] = 0;
                }
            }

            // 6. Completar calcio restante con CaCl2 si es necesario
            if (remainingNutrients.GetValueOrDefault("Ca", 0) > 0)
            {
                var result = CalculateFertilizer("CaCl2.2H2O", "Ca", remainingNutrients["Ca"]);
                results.Add(result);
                remainingNutrients["Ca"] = 0;
            }

            // 7. Completar potasio restante con KCl si es necesario
            if (remainingNutrients.GetValueOrDefault("K", 0) > 0)
            {
                var result = CalculateFertilizer("KCl", "K", remainingNutrients["K"]);
                results.Add(result);
                remainingNutrients["K"] = 0;
            }

            // 8. Agregar micronutrientes
            results.AddRange(CalculateMicronutrients());

            return results;
        }

        private List<FertilizerResult> CalculateMicronutrients()
        {
            var microResults = new List<FertilizerResult>();

            // Micronutrientes con concentraciones típicas
            var microTargets = new Dictionary<string, double>
            {
                ["Fe"] = 1.0,
                ["Mn"] = 0.5,
                ["Zn"] = 0.2,
                ["Cu"] = 0.1,
                ["B"] = 0.5,
                ["Mo"] = 0.01
            };

            // Fuentes preferidas para cada micronutriente
            var preferredSources = new Dictionary<string, string>
            {
                ["Fe"] = "FeEDTA",
                ["Mn"] = "MnSO4.4H2O",
                ["Zn"] = "ZnSO4.7H2O",
                ["Cu"] = "CuSO4.5H2O",
                ["B"] = "H3BO3",
                ["Mo"] = "Na2MoO4.2H2O"
            };

            foreach (var micro in microTargets)
            {
                string source = preferredSources[micro.Key];
                double waterContent = waterAnalysis.Elements_mgL.GetValueOrDefault(micro.Key, 0);
                double needed = Math.Max(0, micro.Value - waterContent);

                if (needed > 0)
                {
                    var result = CalculateFertilizer(source, micro.Key, needed);
                    microResults.Add(result);
                }
            }

            return microResults;
        }

        private double CalculateNutrientContribution(string fertilizerName, string nutrientToCalculate, double targetAmount, string primaryNutrient)
        {
            if (fertilizers == null) return 0;
            var fertilizer = fertilizers.FirstOrDefault(f => f.Name == fertilizerName);
            if (fertilizer == null) return 0;

            // Calcular cantidad de fertilizante necesaria para el nutriente primario
            double primaryElementMW = fertilizer.Elements[primaryNutrient];
            double fertilizerAmount = targetAmount * fertilizer.MolecularWeight * 100.0 / (primaryElementMW * fertilizer.Purity);

            // Calcular contribución del nutriente secundario
            if (fertilizer.Elements.ContainsKey(nutrientToCalculate))
            {
                double secondaryElementMW = fertilizer.Elements[nutrientToCalculate];
                return fertilizerAmount * secondaryElementMW * (fertilizer.Purity / 100.0) / fertilizer.MolecularWeight;
            }

            return 0;
        }

        private double CalculateFertilizerAmount(string fertilizerName, string targetElement, double targetAmount)
        {
            if (fertilizers == null) return 0;
            var fertilizer = fertilizers.FirstOrDefault(f => f.Name == fertilizerName);
            if (fertilizer == null) return 0;

            double elementMolWeight = fertilizer.Elements[targetElement];
            return targetAmount * fertilizer.MolecularWeight * 100.0 / (elementMolWeight * fertilizer.Purity);
        }

        private double CalculateElementContribution(double fertilizerAmount, string fertilizerName, string element)
        {
            if (fertilizers == null) return 0;
            var fertilizer = fertilizers.FirstOrDefault(f => f.Name == fertilizerName);
            if (fertilizer == null || !fertilizer.Elements.ContainsKey(element)) return 0;

            double elementMolWeight = fertilizer.Elements[element];
            return fertilizerAmount * elementMolWeight * (fertilizer.Purity / 100.0) / fertilizer.MolecularWeight;
        }

        private FertilizerResult CalculateFertilizer(string fertilizerName, string targetElement, double targetAmount)
        {
            if (fertilizers == null)
                throw new InvalidOperationException("Fertilizers list is not initialized.");

            var fertilizer = fertilizers.FirstOrDefault(f => f.Name == fertilizerName);
            if (fertilizer == null)
                throw new InvalidOperationException($"Fertilizer {fertilizerName} not found.");

            var result = new FertilizerResult
            {
                Name = fertilizerName,
                Purity = fertilizer.Purity,
                MolecularWeight = fertilizer.MolecularWeight
            };

            // Calcular concentración de sal necesaria
            // Fórmula: Concentración_sal = target_mg/L × PM_sal × 100 / (PM_elemento × pureza%)
            double elementMolWeight = fertilizer.Elements[targetElement];
            result.SaltConcentration_mgL = targetAmount * fertilizer.MolecularWeight * 100.0 / (elementMolWeight * fertilizer.Purity);
            result.SaltConcentration_mmolL = result.SaltConcentration_mgL / fertilizer.MolecularWeight;

            // Calcular aportes de cada elemento según el fertilizante
            CalculateAllElementContributions(result, fertilizer);

            // Asegurar que el elemento objetivo tenga el valor correcto
            SetTargetElementValue(result, targetElement, targetAmount);

            return result;
        }

        private void CalculateAllElementContributions(FertilizerResult result, Fertilizer fertilizer)
        {
            foreach (var element in fertilizer.Elements)
            {
                double contribution = CalculateElementContribution(result.SaltConcentration_mgL, fertilizer.MolecularWeight, element.Value, fertilizer.Purity);

                switch (element.Key)
                {
                    case "Ca": result.Ca = contribution; break;
                    case "K": result.K = contribution; break;
                    case "Mg": result.Mg = contribution; break;
                    case "Na": result.Na = contribution; break;
                    case "NH4": result.NH4 = contribution; break;
                    case "N": result.N = contribution; break;
                    case "P": result.P = contribution; break;
                    case "S": result.S = contribution; break;
                    case "Cl": result.Cl = contribution; break;
                    case "Fe": result.Fe = contribution; break;
                    case "Mn": result.Mn = contribution; break;
                    case "Zn": result.Zn = contribution; break;
                    case "Cu": result.Cu = contribution; break;
                    case "B": result.B = contribution; break;
                    case "Mo": result.Mo = contribution; break;
                }
            }

            // Calcular formas iónicas
            if (result.N > 0)
            {
                result.NO3 = result.N * 62.00 / 14.01; // Conversión N a NO3-
            }

            if (result.P > 0)
            {
                result.H2PO4 = result.P * 96.99 / 30.97; // Conversión P a H2PO4-
            }

            if (result.S > 0)
            {
                result.SO4 = result.S * 96.06 / 32.06; // Conversión S a SO4=
            }
        }

        private void SetTargetElementValue(FertilizerResult result, string targetElement, double targetAmount)
        {
            switch (targetElement)
            {
                case "Ca": result.Ca = targetAmount; break;
                case "K": result.K = targetAmount; break;
                case "Mg": result.Mg = targetAmount; break;
                case "N": result.N = targetAmount; result.NO3 = targetAmount * 62.00 / 14.01; break;
                case "P": result.P = targetAmount; result.H2PO4 = targetAmount * 96.99 / 30.97; break;
                case "S": result.S = targetAmount; result.SO4 = targetAmount * 96.06 / 32.06; break;
                case "Fe": result.Fe = targetAmount; break;
                case "Mn": result.Mn = targetAmount; break;
                case "Zn": result.Zn = targetAmount; break;
                case "Cu": result.Cu = targetAmount; break;
                case "B": result.B = targetAmount; break;
                case "Mo": result.Mo = targetAmount; break;
            }
        }

        private double CalculateElementContribution(double saltConcentration, double saltMolWeight, double elementMolWeight, double purity)
        {
            return saltConcentration * elementMolWeight * (purity / 100.0) / saltMolWeight;
        }

        public IonBalance CalculateIonBalance(List<FertilizerResult> results)
        {
            var balance = new IonBalance();

            // Inicializar diccionarios
            string[] ions = { "Ca", "K", "Mg", "Na", "NH4", "NO3", "N", "SO4", "S", "Cl", "H2PO4", "P", "HCO3", "Fe", "Mn", "Zn", "Cu", "B", "Mo" };

            foreach (string ion in ions)
            {
                balance.Aporte_mgL[ion] = 0;
                balance.Aporte_mmolL[ion] = 0;
                balance.Aporte_meqL[ion] = 0;

                balance.Agua_mgL[ion] = 0;
                balance.Agua_mmolL[ion] = 0;
                balance.Agua_meqL[ion] = 0;

                balance.Final_mgL[ion] = 0;
                balance.Final_mmolL[ion] = 0;
                balance.Final_meqL[ion] = 0;

                // Asignar valores del agua para elementos básicos
                if (waterAnalysis.Elements_mgL.ContainsKey(ion))
                {
                    balance.Agua_mgL[ion] = waterAnalysis.Elements_mgL[ion];
                }
            }

            // Sumar aportes de fertilizantes
            foreach (var result in results)
            {
                balance.Aporte_mgL["Ca"] += result.Ca;
                balance.Aporte_mgL["K"] += result.K;
                balance.Aporte_mgL["Mg"] += result.Mg;
                balance.Aporte_mgL["Na"] += result.Na;
                balance.Aporte_mgL["NH4"] += result.NH4;

                // Aniones - tanto la forma iónica como elemental
                balance.Aporte_mgL["NO3"] += result.NO3;
                balance.Aporte_mgL["N"] += result.N;
                balance.Aporte_mgL["SO4"] += result.SO4;
                balance.Aporte_mgL["S"] += result.S;
                balance.Aporte_mgL["Cl"] += result.Cl;
                balance.Aporte_mgL["H2PO4"] += result.H2PO4;
                balance.Aporte_mgL["P"] += result.P;
                balance.Aporte_mgL["HCO3"] += result.HCO3;

                // Micronutrientes
                balance.Aporte_mgL["Fe"] += result.Fe;
                balance.Aporte_mgL["Mn"] += result.Mn;
                balance.Aporte_mgL["Zn"] += result.Zn;
                balance.Aporte_mgL["Cu"] += result.Cu;
                balance.Aporte_mgL["B"] += result.B;
                balance.Aporte_mgL["Mo"] += result.Mo;
            }

            // Calcular concentraciones finales y conversiones
            foreach (string ion in ions)
            {
                // Final = Aporte + Agua
                balance.Final_mgL[ion] = balance.Aporte_mgL[ion] + balance.Agua_mgL[ion];

                // Convertir a mmol/L y meq/L
                if (elementData != null && elementData.ContainsKey(ion))
                {
                    var atomicWeight = elementData[ion].AtomicWeight;
                    var valence = elementData[ion].Valence;

                    // Para aportes
                    balance.Aporte_mmolL[ion] = balance.Aporte_mgL[ion] / atomicWeight;
                    balance.Aporte_meqL[ion] = balance.Aporte_mmolL[ion] * valence;

                    // Para agua
                    balance.Agua_mmolL[ion] = balance.Agua_mgL[ion] / atomicWeight;
                    balance.Agua_meqL[ion] = balance.Agua_mmolL[ion] * valence;

                    // Para final
                    balance.Final_mmolL[ion] = balance.Final_mgL[ion] / atomicWeight;
                    balance.Final_meqL[ion] = balance.Final_mmolL[ion] * valence;
                }
            }

            return balance;
        }

        public List<Fertilizer> GetAvailableFertilizers()
        {
            return fertilizers?.ToList() ?? new List<Fertilizer>();
        }

        public Dictionary<string, double> CalculateEC(IonBalance ionBalance)
        {
            var ecData = new Dictionary<string, double>();

            // Calcular CE usando diferentes métodos
            // Método 1: Suma de cationes × 0.1
            double sumCations_meq = 0;
            var cations = new[] { "Ca", "K", "Mg", "Na", "NH4", "Fe", "Mn", "Zn", "Cu" };
            foreach (var cation in cations)
            {
                if (elementData != null && elementData.ContainsKey(cation) && elementData[cation].IsCation)
                {
                    sumCations_meq += ionBalance.Final_meqL[cation];
                }
            }
            ecData["EC_FromCations"] = sumCations_meq * 0.1;

            // Método 2: Estimación desde sales totales
            double totalSalts_mgL = 0;
            foreach (var salt in ionBalance.Final_mgL)
            {
                totalSalts_mgL += salt.Value;
            }
            ecData["EC_FromSalts"] = totalSalts_mgL / 640.0; // Conversión aproximada

            // Método 3: Fórmula de Sonneveld
            ecData["EC_Sonneveld"] = 0.095 * sumCations_meq + 0.19;

            return ecData;
        }

        public Dictionary<string, object> GenerateCalculationSummary(List<FertilizerResult> results, IonBalance ionBalance)
        {
            var summary = new Dictionary<string, object>();

            // Información básica
            summary["TotalFertilizers"] = results.Count;
            summary["TotalSaltConcentration_mgL"] = results.Sum(r => r.SaltConcentration_mgL);

            // Verificación de objetivos
            var achievements = new Dictionary<string, object>();
            foreach (var target in targetConcentrations)
            {
                double achieved = ionBalance.Final_mgL.GetValueOrDefault(target.Key, 0);
                double deviation = Math.Abs(achieved - target.Value);
                double deviationPercent = target.Value > 0 ? (deviation / target.Value) * 100 : 0;

                achievements[target.Key] = new
                {
                    Target = target.Value,
                    Achieved = achieved,
                    Deviation = deviation,
                    DeviationPercent = deviationPercent,
                    Status = deviationPercent <= 5 ? "Excellent" :
                            deviationPercent <= 10 ? "Good" :
                            deviationPercent <= 20 ? "Acceptable" : "Poor"
                };
            }
            summary["NutrientAchievements"] = achievements;

            // Balance iónico
            double sumCations = 0;
            double sumAnions = 0;
            var cations = new[] { "Ca", "K", "Mg", "Na", "NH4" };
            var anions = new[] { "NO3", "SO4", "Cl", "H2PO4", "HCO3" };

            foreach (var cation in cations)
            {
                sumCations += ionBalance.Final_meqL.GetValueOrDefault(cation, 0);
            }
            foreach (var anion in anions)
            {
                sumAnions += ionBalance.Final_meqL.GetValueOrDefault(anion, 0);
            }

            summary["IonBalance"] = new
            {
                SumCations_meqL = sumCations,
                SumAnions_meqL = sumAnions,
                Difference_meqL = Math.Abs(sumCations - sumAnions),
                PercentageDifference = sumCations > 0 ? Math.Abs(sumCations - sumAnions) / sumCations * 100 : 0,
                IsBalanced = sumCations > 0 ? Math.Abs(sumCations - sumAnions) / sumCations <= 0.1 : false
            };

            // CE estimada
            var ecData = CalculateEC(ionBalance);
            summary["EstimatedEC"] = ecData;

            // Costos estimados
            double totalCost = 0;
            var fertilizerCosts = new Dictionary<string, double>();
            foreach (var result in results)
            {
                if (fertilizers != null)
                {
                    var fertilizer = fertilizers.FirstOrDefault(f => f.Name == result.Name);
                    if (fertilizer != null)
                    {
                        double cost = (result.SaltConcentration_mgL / 1000.0) * fertilizer.Cost; // Cost per liter of concentrated solution
                        fertilizerCosts[result.Name] = cost;
                        totalCost += cost;
                    }
                }
            }
            summary["CostAnalysis"] = new
            {
                TotalCost_PerLiter = totalCost,
                CostByFertilizer = fertilizerCosts
            };

            return summary;
        }

        public List<string> GetOptimizationRecommendations(List<FertilizerResult> results, IonBalance ionBalance)
        {
            var recommendations = new List<string>();

            // Verificar excesos significativos
            foreach (var target in targetConcentrations)
            {
                double achieved = ionBalance.Final_mgL.GetValueOrDefault(target.Key, 0);
                double excess = achieved - target.Value;
                if (excess > target.Value * 0.2) // Más del 20% de exceso
                {
                    recommendations.Add($"EXCESO: {target.Key} está {excess:F1} mg/L por encima del objetivo. Considere reducir fertilizantes que aportan {target.Key}.");
                }
                else if (excess < -target.Value * 0.1) // Más del 10% de déficit
                {
                    recommendations.Add($"DÉFICIT: {target.Key} está {Math.Abs(excess):F1} mg/L por debajo del objetivo. Considere fertilizantes adicionales.");
                }
            }

            // Verificar solubilidad
            foreach (var result in results)
            {
                if (fertilizers != null)
                {
                    var fertilizer = fertilizers.FirstOrDefault(f => f.Name == result.Name);
                    if (fertilizer != null && result.SaltConcentration_mgL > fertilizer.Solubility * 0.8)
                    {
                        recommendations.Add($"SOLUBILIDAD: {result.Name} se aproxima al límite de solubilidad. Considere dividir en múltiples tanques.");
                    }
                }
            }

            // Verificar balance iónico
            var ionBalanceData = (dynamic)GenerateCalculationSummary(results, ionBalance)["IonBalance"];
            if (ionBalanceData.PercentageDifference > 10)
            {
                recommendations.Add($"BALANCE IÓNICO: Diferencia de {ionBalanceData.PercentageDifference:F1}% entre cationes y aniones excede el 10% recomendado.");
            }

            // Recomendaciones de optimización
            recommendations.Add("GENERAL: Verifique pH final de la solución (objetivo: 5.5-6.5).");
            recommendations.Add("GENERAL: Monitoree CE final (objetivo típico: 1.5-2.5 dS/m para solución nutritiva).");

            if (results.Any(r => r.Name?.Contains("NH4") == true))
            {
                recommendations.Add("ADVERTENCIA: Presencia de amonio (NH4+). Mantenga por debajo del 20% del nitrógeno total.");
            }

            return recommendations;
        }
    }
}