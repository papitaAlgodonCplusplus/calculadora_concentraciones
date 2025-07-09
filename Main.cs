using System;
using System.Collections.Generic;
using System.Linq;

namespace FertilizerCalculator
{
    // Clase para representar un fertilizante
    public class Fertilizer
    {
        public string Name { get; set; }
        public double Purity { get; set; } // %P
        public double MolecularWeight { get; set; } // Peso molecular
        public double ElementWeight { get; set; } // Peso del elemento
        public double Solubility { get; set; } // mg/L
        public double MillimolarSolubility { get; set; } // mmol/L
        
        // Concentraciones de cationes y aniones por unidad de fertilizante
        public double Ca { get; set; }
        public double K { get; set; }
        public double Mg { get; set; }
        public double Na { get; set; }
        public double NH4 { get; set; }
        public double NO3 { get; set; }
        public double N { get; set; }
        public double SO4 { get; set; }
        public double S { get; set; }
        public double Cl { get; set; }
        public double H2PO4 { get; set; }
        public double P { get; set; }
        public double HCO3 { get; set; }
    }

    // Clase para representar la concentración deseada
    public class DesiredConcentration
    {
        public double Ca { get; set; }
        public double K { get; set; }
        public double Mg { get; set; }
        public double Na { get; set; }
        public double NH4 { get; set; }
        public double NO3 { get; set; }
        public double N { get; set; }
        public double SO4 { get; set; }
        public double S { get; set; }
        public double Cl { get; set; }
        public double H2PO4 { get; set; }
        public double P { get; set; }
        public double HCO3 { get; set; }
    }

    // Clase para representar el aporte de iones del agua
    public class WaterIons
    {
        public double Ca { get; set; }
        public double K { get; set; }
        public double Mg { get; set; }
        public double Na { get; set; }
        public double NH4 { get; set; }
        public double NO3 { get; set; }
        public double N { get; set; }
        public double SO4 { get; set; }
        public double S { get; set; }
        public double Cl { get; set; }
        public double H2PO4 { get; set; }
        public double P { get; set; }
        public double HCO3 { get; set; }
    }

    // Clase para el resultado del cálculo
    public class CalculationResult
    {
        public string FertilizerName { get; set; }
        public double RequiredAmount { get; set; } // mg/L
        public double RequiredMmol { get; set; } // mmol/L
        public Dictionary<string, double> IonContribution { get; set; }
    }

    public class FertilizerCalculator
    {
        private List<Fertilizer> fertilizers;
        private WaterIons waterIons;

        public FertilizerCalculator()
        {
            InitializeFertilizers();
        }

        private void InitializeFertilizers()
        {
            fertilizers = new List<Fertilizer>
            {
                new Fertilizer
                {
                    Name = "Ácido nítrico",
                    Purity = 63,
                    MolecularWeight = 63.00,
                    ElementWeight = 61.02,
                    Solubility = 46.4,
                    MillimolarSolubility = 0,
                    NO3 = 28.3,
                    N = 6.39
                },
                new Fertilizer
                {
                    Name = "KH2PO4",
                    Purity = 100,
                    MolecularWeight = 136.1,
                    ElementWeight = 97,
                    Solubility = 100,
                    MillimolarSolubility = 1.321,
                    K = 51.71,
                    H2PO4 = 126.3,
                    P = 41
                },
                new Fertilizer
                {
                    Name = "Ca(NO3)2.2H2O",
                    Purity = 100,
                    MolecularWeight = 200,
                    ElementWeight = 40.08,
                    Solubility = 810,
                    MillimolarSolubility = 4.050,
                    Ca = 162,
                    NO3 = 502,
                    N = 114
                },
                new Fertilizer
                {
                    Name = "KNO3",
                    Purity = 100,
                    MolecularWeight = 101.1,
                    ElementWeight = 39.1,
                    Solubility = 267,
                    MillimolarSolubility = 2.641,
                    K = 103,
                    NO3 = 164,
                    N = 37
                },
                new Fertilizer
                {
                    Name = "K2SO4",
                    Purity = 100,
                    MolecularWeight = 174.3,
                    ElementWeight = 78.2,
                    Solubility = 178,
                    MillimolarSolubility = 1.021,
                    K = 80,
                    SO4 = 93,
                    S = 33
                },
                new Fertilizer
                {
                    Name = "MgSO4.7H2O",
                    Purity = 100,
                    MolecularWeight = 246.3,
                    ElementWeight = 24.31,
                    Solubility = 485,
                    MillimolarSolubility = 1.969,
                    Mg = 48,
                    SO4 = 159,
                    S = 63
                }
            };
        }

        public void SetWaterIons(WaterIons ions)
        {
            waterIons = ions;
        }

        // Método principal para calcular fertilizantes
        public List<CalculationResult> CalculateFertilizers(DesiredConcentration desired)
        {
            var results = new List<CalculationResult>();

            // Calcular iones faltantes (deseados - aporte del agua)
            var neededIons = new DesiredConcentration
            {
                Ca = Math.Max(0, desired.Ca - (waterIons?.Ca ?? 0)),
                K = Math.Max(0, desired.K - (waterIons?.K ?? 0)),
                Mg = Math.Max(0, desired.Mg - (waterIons?.Mg ?? 0)),
                Na = Math.Max(0, desired.Na - (waterIons?.Na ?? 0)),
                NH4 = Math.Max(0, desired.NH4 - (waterIons?.NH4 ?? 0)),
                NO3 = Math.Max(0, desired.NO3 - (waterIons?.NO3 ?? 0)),
                N = Math.Max(0, desired.N - (waterIons?.N ?? 0)),
                SO4 = Math.Max(0, desired.SO4 - (waterIons?.SO4 ?? 0)),
                S = Math.Max(0, desired.S - (waterIons?.S ?? 0)),
                Cl = Math.Max(0, desired.Cl - (waterIons?.Cl ?? 0)),
                H2PO4 = Math.Max(0, desired.H2PO4 - (waterIons?.H2PO4 ?? 0)),
                P = Math.Max(0, desired.P - (waterIons?.P ?? 0)),
                HCO3 = Math.Max(0, desired.HCO3 - (waterIons?.HCO3 ?? 0))
            };

            // Algoritmo de optimización simple para cada fertilizante
            foreach (var fertilizer in fertilizers)
            {
                var result = CalculateFertilizerAmount(fertilizer, neededIons);
                if (result.RequiredAmount > 0)
                {
                    results.Add(result);
                }
            }

            return results;
        }

        private CalculationResult CalculateFertilizerAmount(Fertilizer fertilizer, DesiredConcentration needed)
        {
            var result = new CalculationResult
            {
                FertilizerName = fertilizer.Name,
                IonContribution = new Dictionary<string, double>()
            };

            // Determinar cuál ion es el limitante para este fertilizante
            double requiredAmount = 0;
            string limitingIon = "";

            // Buscar el ion que requiere más cantidad del fertilizante
            var ionRequirements = new Dictionary<string, double>();

            if (fertilizer.Ca > 0 && needed.Ca > 0)
                ionRequirements["Ca"] = needed.Ca / fertilizer.Ca;
            if (fertilizer.K > 0 && needed.K > 0)
                ionRequirements["K"] = needed.K / fertilizer.K;
            if (fertilizer.Mg > 0 && needed.Mg > 0)
                ionRequirements["Mg"] = needed.Mg / fertilizer.Mg;
            if (fertilizer.Na > 0 && needed.Na > 0)
                ionRequirements["Na"] = needed.Na / fertilizer.Na;
            if (fertilizer.NH4 > 0 && needed.NH4 > 0)
                ionRequirements["NH4"] = needed.NH4 / fertilizer.NH4;
            if (fertilizer.NO3 > 0 && needed.NO3 > 0)
                ionRequirements["NO3"] = needed.NO3 / fertilizer.NO3;
            if (fertilizer.N > 0 && needed.N > 0)
                ionRequirements["N"] = needed.N / fertilizer.N;
            if (fertilizer.SO4 > 0 && needed.SO4 > 0)
                ionRequirements["SO4"] = needed.SO4 / fertilizer.SO4;
            if (fertilizer.S > 0 && needed.S > 0)
                ionRequirements["S"] = needed.S / fertilizer.S;
            if (fertilizer.Cl > 0 && needed.Cl > 0)
                ionRequirements["Cl"] = needed.Cl / fertilizer.Cl;
            if (fertilizer.H2PO4 > 0 && needed.H2PO4 > 0)
                ionRequirements["H2PO4"] = needed.H2PO4 / fertilizer.H2PO4;
            if (fertilizer.P > 0 && needed.P > 0)
                ionRequirements["P"] = needed.P / fertilizer.P;
            if (fertilizer.HCO3 > 0 && needed.HCO3 > 0)
                ionRequirements["HCO3"] = needed.HCO3 / fertilizer.HCO3;

            if (ionRequirements.Count > 0)
            {
                // Tomar el ion que requiere menos cantidad (más restrictivo)
                var minRequirement = ionRequirements.OrderBy(kvp => kvp.Value).First();
                requiredAmount = minRequirement.Value;
                limitingIon = minRequirement.Key;
            }

            result.RequiredAmount = requiredAmount;
            result.RequiredMmol = requiredAmount / fertilizer.MolecularWeight * 1000;

            // Calcular la contribución de cada ion
            result.IonContribution["Ca"] = fertilizer.Ca * requiredAmount;
            result.IonContribution["K"] = fertilizer.K * requiredAmount;
            result.IonContribution["Mg"] = fertilizer.Mg * requiredAmount;
            result.IonContribution["Na"] = fertilizer.Na * requiredAmount;
            result.IonContribution["NH4"] = fertilizer.NH4 * requiredAmount;
            result.IonContribution["NO3"] = fertilizer.NO3 * requiredAmount;
            result.IonContribution["N"] = fertilizer.N * requiredAmount;
            result.IonContribution["SO4"] = fertilizer.SO4 * requiredAmount;
            result.IonContribution["S"] = fertilizer.S * requiredAmount;
            result.IonContribution["Cl"] = fertilizer.Cl * requiredAmount;
            result.IonContribution["H2PO4"] = fertilizer.H2PO4 * requiredAmount;
            result.IonContribution["P"] = fertilizer.P * requiredAmount;
            result.IonContribution["HCO3"] = fertilizer.HCO3 * requiredAmount;

            return result;
        }

        // Método para calcular la solución nutritiva completa
        public void CalculateNutrientSolution(DesiredConcentration desired)
        {
            var results = CalculateFertilizers(desired);

            Console.WriteLine("=== CÁLCULO DE SOLUCIÓN NUTRITIVA ===\n");
            Console.WriteLine($"{"Fertilizante",-20} {"Cantidad (mg/L)",-15} {"Cantidad (mmol/L)",-18}");
            Console.WriteLine(new string('-', 55));

            foreach (var result in results)
            {
                Console.WriteLine($"{result.FertilizerName,-20} {result.RequiredAmount,-15:F2} {result.RequiredMmol,-18:F3}");
            }

            Console.WriteLine("\n=== APORTE DE IONES POR FERTILIZANTE ===\n");
            
            foreach (var result in results)
            {
                if (result.RequiredAmount > 0)
                {
                    Console.WriteLine($"\n{result.FertilizerName} ({result.RequiredAmount:F2} mg/L):");
                    foreach (var ion in result.IonContribution)
                    {
                        if (ion.Value > 0.01)
                        {
                            Console.WriteLine($"  {ion.Key}: {ion.Value:F2} mg/L");
                        }
                    }
                }
            }

            // Calcular totales
            Console.WriteLine("\n=== CONCENTRACIÓN FINAL TOTAL ===\n");
            var totalContribution = new Dictionary<string, double>();
            
            foreach (var result in results)
            {
                foreach (var ion in result.IonContribution)
                {
                    if (!totalContribution.ContainsKey(ion.Key))
                        totalContribution[ion.Key] = 0;
                    totalContribution[ion.Key] += ion.Value;
                }
            }

            // Sumar aporte del agua
            if (waterIons != null)
            {
                totalContribution["Ca"] += waterIons.Ca;
                totalContribution["K"] += waterIons.K;
                totalContribution["Mg"] += waterIons.Mg;
                totalContribution["Na"] += waterIons.Na;
                totalContribution["NH4"] += waterIons.NH4;
                totalContribution["NO3"] += waterIons.NO3;
                totalContribution["N"] += waterIons.N;
                totalContribution["SO4"] += waterIons.SO4;
                totalContribution["S"] += waterIons.S;
                totalContribution["Cl"] += waterIons.Cl;
                totalContribution["H2PO4"] += waterIons.H2PO4;
                totalContribution["P"] += waterIons.P;
                totalContribution["HCO3"] += waterIons.HCO3;
            }

            Console.WriteLine($"{"Ion",-8} {"Deseado",-10} {"Obtenido",-10} {"Diferencia",-12}");
            Console.WriteLine(new string('-', 42));
            
            Console.WriteLine($"{"Ca",-8} {desired.Ca,-10:F2} {totalContribution["Ca"],-10:F2} {totalContribution["Ca"] - desired.Ca,-12:F2}");
            Console.WriteLine($"{"K",-8} {desired.K,-10:F2} {totalContribution["K"],-10:F2} {totalContribution["K"] - desired.K,-12:F2}");
            Console.WriteLine($"{"Mg",-8} {desired.Mg,-10:F2} {totalContribution["Mg"],-10:F2} {totalContribution["Mg"] - desired.Mg,-12:F2}");
            Console.WriteLine($"{"P",-8} {desired.P,-10:F2} {totalContribution["P"],-10:F2} {totalContribution["P"] - desired.P,-12:F2}");
            Console.WriteLine($"{"N",-8} {desired.N,-10:F2} {totalContribution["N"],-10:F2} {totalContribution["N"] - desired.N,-12:F2}");
            Console.WriteLine($"{"S",-8} {desired.S,-10:F2} {totalContribution["S"],-10:F2} {totalContribution["S"] - desired.S,-12:F2}");
        }
    }

    // Programa principal de ejemplo
    class Program
    {
        static void Main(string[] args)
        {
            var calculator = new FertilizerCalculator();

            // Definir el aporte de iones del agua (basado en la imagen 1)
            var waterIons = new WaterIons
            {
                Ca = 162,
                K = 257,
                Mg = 46,
                Na = 0,
                NH4 = 0,
                NO3 = 673,
                N = 152,
                SO4 = 316,
                S = 106,
                Cl = 0,
                H2PO4 = 145,
                P = 46,
                HCO3 = 0
            };

            calculator.SetWaterIons(waterIons);

            // Definir concentraciones deseadas (ejemplo)
            var desiredConcentration = new DesiredConcentration
            {
                Ca = 200,
                K = 300,
                Mg = 60,
                Na = 10,
                NH4 = 20,
                NO3 = 800,
                N = 180,
                SO4 = 400,
                S = 130,
                Cl = 50,
                H2PO4 = 200,
                P = 60,
                HCO3 = 30
            };

            // Realizar el cálculo
            calculator.CalculateNutrientSolution(desiredConcentration);

            Console.WriteLine("\nPresione cualquier tecla para salir...");
            Console.ReadKey();
        }
    }
}