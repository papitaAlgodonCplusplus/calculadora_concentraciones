using System;
using System.Collections.Generic;
using System.Linq;

namespace NutrientSolutionCalculator
{
    public class Fertilizer
    {
        public string Name { get; set; }
        public double Purity { get; set; } // %
        public double MolecularWeight { get; set; }
        public Dictionary<string, double> Elements { get; set; } = new Dictionary<string, double>();
        public double Solubility { get; set; } // mg/L
        public double Cost { get; set; } // costo por kg
        
        public Fertilizer(string name, double purity, double molecularWeight)
        {
            Name = name;
            Purity = purity;
            MolecularWeight = molecularWeight;
        }
    }

    public class WaterAnalysis
    {
        public Dictionary<string, double> Elements_mgL { get; set; } = new Dictionary<string, double>();
        public Dictionary<string, double> Elements_mmolL { get; set; } = new Dictionary<string, double>();
        public Dictionary<string, double> Elements_meqL { get; set; } = new Dictionary<string, double>();
        public double pH { get; set; }
        public double EC { get; set; }
        public double HCO3 { get; set; }
    }

    public class FertilizerResult
    {
        public string Name { get; set; }
        public double Purity { get; set; }
        public double MolecularWeight { get; set; }
        public double Elem1MolWeight { get; set; }
        public double Elem2MolWeight { get; set; }
        public double SaltConcentration_mgL { get; set; }
        public double SaltConcentration_mmolL { get; set; }
        
        // Cationes (mg/L)
        public double Ca { get; set; }
        public double K { get; set; }
        public double Mg { get; set; }
        public double Na { get; set; }
        public double NH4 { get; set; }
        
        // Aniones (mg/L)
        public double NO3_N { get; set; }
        public double SO4_S { get; set; }
        public double Cl { get; set; }
        public double H2PO4_P { get; set; }
        public double HCO3 { get; set; }
        
        public double SumAnions { get; set; }
        public double CE { get; set; }
    }

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

    public class ElementData
    {
        public double AtomicWeight { get; set; }
        public int Valence { get; set; }
        public bool IsCation { get; set; }
    }

    public class NutrientCalculatorAdvanced
    {
        private List<Fertilizer> fertilizers;
        private WaterAnalysis waterAnalysis;
        private Dictionary<string, double> targetConcentrations;
        private Dictionary<string, ElementData> elementData;

        public NutrientCalculatorAdvanced()
        {
            InitializeFertilizers();
            InitializeElementData();
            targetConcentrations = new Dictionary<string, double>();
            waterAnalysis = new WaterAnalysis();
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
                ["N"] = new ElementData { AtomicWeight = 14.01, Valence = 1, IsCation = false },
                ["S"] = new ElementData { AtomicWeight = 32.06, Valence = 2, IsCation = false },
                ["P"] = new ElementData { AtomicWeight = 30.97, Valence = 1, IsCation = false },
                ["Cl"] = new ElementData { AtomicWeight = 35.45, Valence = 1, IsCation = false }
            };
        }

        private void InitializeFertilizers()
        {
            fertilizers = new List<Fertilizer>();

            // Ácido nítrico
            var nitricAcid = new Fertilizer("Ácido nítrico", 65, 62.00);
            nitricAcid.Elements["N"] = 14.01;
            fertilizers.Add(nitricAcid);

            // KH2PO4
            var kh2po4 = new Fertilizer("KH2PO4", 98, 136.19);
            kh2po4.Elements["K"] = 39.10;
            kh2po4.Elements["P"] = 30.97;
            fertilizers.Add(kh2po4);

            // Ca(NO3)2.2H2O
            var caNO3 = new Fertilizer("Ca(NO3)2.2H2O", 95, 200);
            caNO3.Elements["Ca"] = 40.08;
            caNO3.Elements["N"] = 28.01; // 2 átomos de N
            fertilizers.Add(caNO3);

            // KNO3
            var kno3 = new Fertilizer("KNO3", 98, 101.13);
            kno3.Elements["K"] = 39.10;
            kno3.Elements["N"] = 14.01;
            fertilizers.Add(kno3);

            // K2SO4
            var k2so4 = new Fertilizer("K2SO4", 98, 174.37);
            k2so4.Elements["K"] = 78.20; // 2 átomos de K
            k2so4.Elements["S"] = 32.06;
            fertilizers.Add(k2so4);

            // MgSO4.7H2O
            var mgso4 = new Fertilizer("MgSO4.7H2O", 98, 246.32);
            mgso4.Elements["Mg"] = 24.31;
            mgso4.Elements["S"] = 32.06;
            fertilizers.Add(mgso4);
        }

        public void GetUserInput()
        {
            Console.WriteLine("=== CALCULADORA DE SOLUCIÓN NUTRITIVA ===\n");
            
            Console.WriteLine("Ingrese las concentraciones deseadas (mg/L):");
            
            string[] elements = { "Ca", "K", "Mg", "N", "P", "S" };
            
            foreach (string element in elements)
            {
                Console.Write($"{element}: ");
                if (double.TryParse(Console.ReadLine(), out double concentration))
                {
                    targetConcentrations[element] = concentration;
                }
                else
                {
                    Console.WriteLine("Valor inválido, se usará 0");
                    targetConcentrations[element] = 0;
                }
            }

            // Datos del agua (simplificado para el ejemplo)
            Console.WriteLine("\nIngrese datos del agua (mg/L) - presione Enter para usar valores por defecto:");
            
            Console.Write("Ca en agua (default: 10): ");
            string input = Console.ReadLine();
            waterAnalysis.Elements_mgL["Ca"] = string.IsNullOrEmpty(input) ? 10 : double.Parse(input);
            
            Console.Write("K en agua (default: 2): ");
            input = Console.ReadLine();
            waterAnalysis.Elements_mgL["K"] = string.IsNullOrEmpty(input) ? 2 : double.Parse(input);
            
            Console.Write("Mg en agua (default: 5): ");
            input = Console.ReadLine();
            waterAnalysis.Elements_mgL["Mg"] = string.IsNullOrEmpty(input) ? 5 : double.Parse(input);

            // Calcular mmol/L y meq/L para el agua
            foreach (var element in waterAnalysis.Elements_mgL)
            {
                if (elementData.ContainsKey(element.Key))
                {
                    waterAnalysis.Elements_mmolL[element.Key] = element.Value / elementData[element.Key].AtomicWeight;
                    waterAnalysis.Elements_meqL[element.Key] = waterAnalysis.Elements_mmolL[element.Key] * elementData[element.Key].Valence;
                }
            }
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

            // Secuencia de cálculo como en el Excel
            
            // 1. KH2PO4 para fósforo
            if (remainingNutrients.ContainsKey("P") && remainingNutrients["P"] > 0)
            {
                var result = CalculateFertilizer("KH2PO4", "P", remainingNutrients["P"]);
                results.Add(result);
                
                // Actualizar K restante
                double kFromPhosphate = result.K;
                if (remainingNutrients.ContainsKey("K"))
                    remainingNutrients["K"] = Math.Max(0, remainingNutrients["K"] - kFromPhosphate);
            }

            // 2. Ca(NO3)2.2H2O para calcio
            if (remainingNutrients.ContainsKey("Ca") && remainingNutrients["Ca"] > 0)
            {
                var result = CalculateFertilizer("Ca(NO3)2.2H2O", "Ca", remainingNutrients["Ca"]);
                results.Add(result);
                
                // Actualizar N restante
                double nFromCalcium = result.NO3_N;
                if (remainingNutrients.ContainsKey("N"))
                    remainingNutrients["N"] = Math.Max(0, remainingNutrients["N"] - nFromCalcium);
            }

            // 3. MgSO4.7H2O para magnesio
            if (remainingNutrients.ContainsKey("Mg") && remainingNutrients["Mg"] > 0)
            {
                var result = CalculateFertilizer("MgSO4.7H2O", "Mg", remainingNutrients["Mg"]);
                results.Add(result);
                
                // Actualizar S restante
                double sFromMagnesium = result.SO4_S;
                if (remainingNutrients.ContainsKey("S"))
                    remainingNutrients["S"] = Math.Max(0, remainingNutrients["S"] - sFromMagnesium);
            }

            // 4. KNO3 para nitrógeno restante
            if (remainingNutrients.ContainsKey("N") && remainingNutrients["N"] > 0)
            {
                var result = CalculateFertilizer("KNO3", "N", remainingNutrients["N"]);
                results.Add(result);
                
                // Actualizar K restante
                double kFromNitrate = result.K;
                if (remainingNutrients.ContainsKey("K"))
                    remainingNutrients["K"] = Math.Max(0, remainingNutrients["K"] - kFromNitrate);
            }

            // 5. K2SO4 para potasio restante
            if (remainingNutrients.ContainsKey("K") && remainingNutrients["K"] > 0)
            {
                var result = CalculateFertilizer("K2SO4", "K", remainingNutrients["K"]);
                results.Add(result);
            }

            return results;
        }

        private FertilizerResult CalculateFertilizer(string fertilizerName, string targetElement, double targetAmount)
        {
            var fertilizer = fertilizers.First(f => f.Name == fertilizerName);
            var result = new FertilizerResult
            {
                Name = fertilizerName,
                Purity = fertilizer.Purity,
                MolecularWeight = fertilizer.MolecularWeight
            };

            // Calcular concentración de sal necesaria
            // Fórmula: Concentración_sal = target_mg/L × PM_sal × 100 / (PM_elemento × pureza%)
            double elementMolWeight = fertilizer.Elements[targetElement];
            result.SaltConcentration_mgL = targetAmount * fertilizer.MolecularWeight * 100 / 
                                          (elementMolWeight * fertilizer.Purity);
            
            result.SaltConcentration_mmolL = result.SaltConcentration_mgL / fertilizer.MolecularWeight;

            // Calcular aportes de cada elemento
            switch (fertilizerName)
            {
                case "KH2PO4":
                    result.Elem1MolWeight = 39.10; // K
                    result.Elem2MolWeight = 30.97; // P
                    result.K = CalculateElementContribution(result.SaltConcentration_mgL, fertilizer.MolecularWeight, 39.10, fertilizer.Purity);
                    result.H2PO4_P = targetAmount;
                    break;
                    
                case "Ca(NO3)2.2H2O":
                    result.Elem1MolWeight = 40.08; // Ca
                    result.Elem2MolWeight = 28.01; // 2N
                    result.Ca = targetAmount;
                    result.NO3_N = CalculateElementContribution(result.SaltConcentration_mgL, fertilizer.MolecularWeight, 28.01, fertilizer.Purity);
                    break;
                    
                case "MgSO4.7H2O":
                    result.Elem1MolWeight = 24.31; // Mg
                    result.Elem2MolWeight = 32.06; // S
                    result.Mg = targetAmount;
                    result.SO4_S = CalculateElementContribution(result.SaltConcentration_mgL, fertilizer.MolecularWeight, 32.06, fertilizer.Purity);
                    break;
                    
                case "KNO3":
                    result.Elem1MolWeight = 39.10; // K
                    result.Elem2MolWeight = 14.01; // N
                    result.K = CalculateElementContribution(result.SaltConcentration_mgL, fertilizer.MolecularWeight, 39.10, fertilizer.Purity);
                    result.NO3_N = targetAmount;
                    break;
                    
                case "K2SO4":
                    result.Elem1MolWeight = 78.20; // 2K
                    result.Elem2MolWeight = 32.06; // S
                    result.K = targetAmount;
                    result.SO4_S = CalculateElementContribution(result.SaltConcentration_mgL, fertilizer.MolecularWeight, 32.06, fertilizer.Purity);
                    break;
            }

            return result;
        }

        private double CalculateElementContribution(double saltConcentration, double saltMolWeight, double elementMolWeight, double purity)
        {
            return saltConcentration * elementMolWeight * (purity / 100) / saltMolWeight;
        }

        public IonBalance CalculateIonBalance(List<FertilizerResult> results)
        {
            var balance = new IonBalance();
            
            // Inicializar diccionarios
            string[] ions = { "Ca", "K", "Mg", "Na", "NH4", "NO3_N", "SO4_S", "Cl", "H2PO4_P", "HCO3" };
            
            foreach (string ion in ions)
            {
                // Inicializar todos los diccionarios
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
                string elementKey = ion.Replace("_", "").Replace("NO3", "N").Replace("SO4", "S").Replace("H2PO4", "P");
                if (waterAnalysis.Elements_mgL.ContainsKey(elementKey))
                {
                    balance.Agua_mgL[ion] = waterAnalysis.Elements_mgL[elementKey];
                }
            }

            // Sumar aportes de fertilizantes
            foreach (var result in results)
            {
                balance.Aporte_mgL["Ca"] += result.Ca;
                balance.Aporte_mgL["K"] += result.K;
                balance.Aporte_mgL["Mg"] += result.Mg;
                balance.Aporte_mgL["NH4"] += result.NH4;
                balance.Aporte_mgL["NO3_N"] += result.NO3_N;
                balance.Aporte_mgL["SO4_S"] += result.SO4_S;
                balance.Aporte_mgL["H2PO4_P"] += result.H2PO4_P;
            }

            // Calcular concentraciones finales y conversiones
            foreach (string ion in ions)
            {
                // Final = Aporte + Agua
                balance.Final_mgL[ion] = balance.Aporte_mgL[ion] + balance.Agua_mgL[ion];
                
                // Convertir a mmol/L y meq/L
                string elementKey = ion.Replace("_", "").Replace("NO3", "N").Replace("SO4", "S").Replace("H2PO4", "P");
                if (elementData.ContainsKey(elementKey))
                {
                    // Para aportes
                    balance.Aporte_mmolL[ion] = balance.Aporte_mgL[ion] / elementData[elementKey].AtomicWeight;
                    balance.Aporte_meqL[ion] = balance.Aporte_mmolL[ion] * elementData[elementKey].Valence;
                    
                    // Para agua
                    balance.Agua_mmolL[ion] = balance.Agua_mgL[ion] / elementData[elementKey].AtomicWeight;
                    balance.Agua_meqL[ion] = balance.Agua_mmolL[ion] * elementData[elementKey].Valence;
                    
                    // Para final
                    balance.Final_mmolL[ion] = balance.Final_mgL[ion] / elementData[elementKey].AtomicWeight;
                    balance.Final_meqL[ion] = balance.Final_mmolL[ion] * elementData[elementKey].Valence;
                }
            }

            return balance;
        }

        public void DisplayResults(List<FertilizerResult> results, IonBalance balance)
        {
            Console.Clear();
            Console.WriteLine("=== RESULTADOS DE SOLUCIÓN NUTRITIVA ===\n");

            // Tabla 1: Fertilizantes
            Console.WriteLine("TABLA 1: FERTILIZANTES");
            Console.WriteLine(new string('=', 150));
            Console.WriteLine($"{"FERTILIZANTE",-20} {"P%",-5} {"PM Sal",-8} {"PM E1",-8} {"PM E2",-8} {"mg/L",-10} {"mmol/L",-8} {"Ca",-6} {"K",-6} {"Mg",-6} {"NH4",-6} {"NO3-N",-8} {"SO4-S",-8} {"H2PO4-P",-10}");
            Console.WriteLine(new string('-', 150));

            foreach (var result in results)
            {
                Console.WriteLine($"{result.Name,-20} {result.Purity,-5:F0} {result.MolecularWeight,-8:F2} {result.Elem1MolWeight,-8:F2} {result.Elem2MolWeight,-8:F2} " +
                                $"{result.SaltConcentration_mgL,-10:F2} {result.SaltConcentration_mmolL,-8:F3} {result.Ca,-6:F1} {result.K,-6:F1} {result.Mg,-6:F1} " +
                                $"{result.NH4,-6:F1} {result.NO3_N,-8:F1} {result.SO4_S,-8:F1} {result.H2PO4_P,-10:F1}");
            }

            Console.WriteLine("\n\nTABLA 2: BALANCE DE IONES");
            Console.WriteLine(new string('=', 120));
            
            // Encabezados
            Console.WriteLine($"{"ELEMENTO",-12} {"--- APORTE ---",-30} {"--- AGUA ---",-25} {"--- FINAL ---",-25}");
            Console.WriteLine($"{"",12} {"mg/L",-8} {"mmol/L",-8} {"meq/L",-8} {"mg/L",-8} {"mmol/L",-8} {"meq/L",-8} {"mg/L",-8} {"mmol/L",-8} {"meq/L",-8}");
            Console.WriteLine(new string('-', 120));

            // Cationes
            Console.WriteLine("CATIONES:");
            string[] cations = { "Ca", "K", "Mg", "Na", "NH4" };
            foreach (string cation in cations)
            {
                Console.WriteLine($"{cation,-12} {balance.Aporte_mgL[cation],-8:F1} {balance.Aporte_mmolL[cation],-8:F2} {balance.Aporte_meqL[cation],-8:F2} " +
                                $"{balance.Agua_mgL[cation],-8:F1} {balance.Agua_mmolL[cation],-8:F2} {balance.Agua_meqL[cation],-8:F2} " +
                                $"{balance.Final_mgL[cation],-8:F1} {balance.Final_mmolL[cation],-8:F2} {balance.Final_meqL[cation],-8:F2}");
            }

            Console.WriteLine("\nANIONES:");
            string[] anions = { "NO3_N", "SO4_S", "Cl", "H2PO4_P", "HCO3" };
            foreach (string anion in anions)
            {
                Console.WriteLine($"{anion,-12} {balance.Aporte_mgL[anion],-8:F1} {balance.Aporte_mmolL[anion],-8:F2} {balance.Aporte_meqL[anion],-8:F2} " +
                                $"{balance.Agua_mgL[anion],-8:F1} {balance.Agua_mmolL[anion],-8:F2} {balance.Agua_meqL[anion],-8:F2} " +
                                $"{balance.Final_mgL[anion],-8:F1} {balance.Final_mmolL[anion],-8:F2} {balance.Final_meqL[anion],-8:F2}");
            }

            // Verificación vs concentraciones deseadas
            Console.WriteLine("\n\nVERIFICACIÓN VS CONCENTRACIONES DESEADAS:");
            Console.WriteLine(new string('=', 60));
            Console.WriteLine($"{"Elemento",-12} {"Deseado",-12} {"Obtenido",-12} {"Diferencia",-12}");
            Console.WriteLine(new string('-', 60));

            foreach (var target in targetConcentrations)
            {
                string key = target.Key;
                if (key == "N") key = "NO3_N";
                if (key == "S") key = "SO4_S";
                if (key == "P") key = "H2PO4_P";
                
                double obtained = 0;
                if (balance.Final_mgL.ContainsKey(key))
                {
                    obtained = balance.Final_mgL[key];
                }
                else if (balance.Final_mgL.ContainsKey(target.Key))
                {
                    obtained = balance.Final_mgL[target.Key];
                }
                
                double difference = obtained - target.Value;
                
                Console.WriteLine($"{target.Key,-12} {target.Value,-12:F1} {obtained,-12:F1} {difference,-12:F1}");
            }
        }
    }

    // Programa principal
    class Program
    {
        static void Main(string[] args)
        {
            var calculator = new NutrientCalculatorAdvanced();
            
            // Obtener input del usuario
            calculator.GetUserInput();
            
            // Calcular solución
            var results = calculator.CalculateSolution();
            
            // Calcular balance de iones
            var balance = calculator.CalculateIonBalance(results);
            
            // Mostrar resultados
            calculator.DisplayResults(results, balance);
            
            Console.WriteLine("\nPresione cualquier tecla para salir...");
            Console.ReadKey();
        }
    }
}