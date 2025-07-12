/*
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace NutrientSolutionCalculator
{
    public class Fertilizer
    {
        public string? Name { get; set; }
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
        public string? Name { get; set; }
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
        public double NO3 { get; set; }      // NO3-
        public double N { get; set; }        // N elemental
        public double SO4 { get; set; }      // SO4=
        public double S { get; set; }        // S elemental
        public double Cl { get; set; }       // Cl-
        public double H2PO4 { get; set; }    // H2PO4-
        public double P { get; set; }        // P elemental
        public double HCO3 { get; set; }     // HCO3-

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
                ["HCO3"] = new ElementData { AtomicWeight = 61.02, Valence = 1, IsCation = false } // HCO3-
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
            double nRequired = remainingNutrients.ContainsKey("N") ? remainingNutrients["N"] : 0;
            double caRequired = remainingNutrients.ContainsKey("Ca") ? remainingNutrients["Ca"] : 0;

            if (caRequired > 0)
            {
                // Calcular cuánto N aportaría Ca(NO3)2 para todo el Ca
                double nFromFullCaNO3 = caRequired * 28.01 * 0.95 / (40.08 * 200.0) * 200.0;

                if (nFromFullCaNO3 > nRequired * 1.5) // Si excede significativamente
                {
                    // Usar solo la cantidad de Ca(NO3)2 necesaria para el N
                    if (nRequired > 0)
                    {
                        double caNO3ForN = CalculateFertilizerForSecondaryElement("Ca(NO3)2.2H2O", "N", nRequired);
                        var caNO3Result = CreatePartialFertilizerResult("Ca(NO3)2.2H2O", caNO3ForN, nRequired, 0);
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
            double kRequired = remainingNutrients.ContainsKey("K") ? remainingNutrients["K"] : 0;
            double sRequired = remainingNutrients.ContainsKey("S") ? remainingNutrients["S"] : 0;

            if (kRequired > 0)
            {
                if (sRequired > 0)
                {
                    // Usar K2SO4 hasta donde alcance el azufre requerido
                    double k2so4ForS = CalculateFertilizerForSecondaryElement("K2SO4", "S", sRequired);
                    var k2so4Result = CreatePartialFertilizerResult("K2SO4", k2so4ForS, 0, sRequired);

                    if (k2so4Result.K <= kRequired)
                    {
                        results.Add(k2so4Result);
                        remainingNutrients["K"] = Math.Max(0, remainingNutrients["K"] - k2so4Result.K);
                        remainingNutrients["S"] = 0;
                    }
                    else
                    {
                        // Si K2SO4 da demasiado K, usar todo el K requerido
                        var result = CalculateFertilizer("K2SO4", "K", kRequired);
                        results.Add(result);
                        remainingNutrients["K"] = 0;
                    }
                }
                else
                {
                    // No se necesita azufre, usar K2SO4 normal
                    var result = CalculateFertilizer("K2SO4", "K", kRequired);
                    results.Add(result);
                    remainingNutrients["K"] = 0;
                }
            }

            return results;
        }

        private double CalculateFertilizerForSecondaryElement(string fertilizerName, string targetElement, double targetAmount)
        {
            if (fertilizers == null)
                throw new InvalidOperationException("Fertilizers list is not initialized.");
            var fertilizer = fertilizers.First(f => f.Name == fertilizerName);
            double elementMolWeight = fertilizer.Elements[targetElement];
            return targetAmount * fertilizer.MolecularWeight * 100 / (elementMolWeight * fertilizer.Purity);
        }

        private FertilizerResult CreatePartialFertilizerResult(string fertilizerName, double saltConcentration, double targetElement1, double targetElement2)
        {
            if (fertilizers == null)
                throw new InvalidOperationException("Fertilizers list is not initialized.");
            var fertilizer = fertilizers.First(f => f.Name == fertilizerName);
            var result = new FertilizerResult
            {
                Name = fertilizerName,
                Purity = fertilizer.Purity,
                MolecularWeight = fertilizer.MolecularWeight,
                SaltConcentration_mgL = saltConcentration,
                SaltConcentration_mmolL = saltConcentration / fertilizer.MolecularWeight
            };

            // Calcular aportes según el fertilizante
            switch (fertilizerName)
            {
                case "Ca(NO3)2.2H2O":
                    result.Elem1MolWeight = 40.08;
                    result.Elem2MolWeight = 28.01;
                    result.Ca = CalculateElementContribution(saltConcentration, fertilizer.MolecularWeight, 40.08, fertilizer.Purity);
                    result.N = targetElement1 > 0 ? targetElement1 : CalculateElementContribution(saltConcentration, fertilizer.MolecularWeight, 28.01, fertilizer.Purity);
                    break;

                case "K2SO4":
                    result.Elem1MolWeight = 78.20;
                    result.Elem2MolWeight = 32.06;
                    result.K = CalculateElementContribution(saltConcentration, fertilizer.MolecularWeight, 78.20, fertilizer.Purity);
                    result.S = targetElement2 > 0 ? targetElement2 : CalculateElementContribution(saltConcentration, fertilizer.MolecularWeight, 32.06, fertilizer.Purity);
                    break;
            }

            return result;
        }

        private FertilizerResult CalculateFertilizer(string fertilizerName, string targetElement, double targetAmount)
        {
            if (fertilizers == null)
                throw new InvalidOperationException("Fertilizers list is not initialized.");
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
                    result.P = targetAmount;
                    // Calcular H2PO4- desde P
                    result.H2PO4 = targetAmount * 96.99 / 30.97; // Conversión P a H2PO4-
                    break;

                case "Ca(NO3)2.2H2O":
                    result.Elem1MolWeight = 40.08; // Ca
                    result.Elem2MolWeight = 28.01; // 2N
                    result.Ca = targetAmount;
                    result.N = CalculateElementContribution(result.SaltConcentration_mgL, fertilizer.MolecularWeight, 28.01, fertilizer.Purity);
                    // Calcular NO3- desde N
                    result.NO3 = result.N * 62.00 / 14.01; // Conversión N a NO3-
                    break;

                case "MgSO4.7H2O":
                    result.Elem1MolWeight = 24.31; // Mg
                    result.Elem2MolWeight = 32.06; // S
                    result.Mg = targetAmount;
                    result.S = CalculateElementContribution(result.SaltConcentration_mgL, fertilizer.MolecularWeight, 32.06, fertilizer.Purity);
                    // Calcular SO4= desde S
                    result.SO4 = result.S * 96.06 / 32.06; // Conversión S a SO4=
                    break;

                case "KNO3":
                    result.Elem1MolWeight = 39.10; // K
                    result.Elem2MolWeight = 14.01; // N
                    result.K = CalculateElementContribution(result.SaltConcentration_mgL, fertilizer.MolecularWeight, 39.10, fertilizer.Purity);
                    result.N = targetAmount;
                    // Calcular NO3- desde N
                    result.NO3 = result.N * 62.00 / 14.01; // Conversión N a NO3-
                    break;

                case "K2SO4":
                    result.Elem1MolWeight = 78.20; // 2K
                    result.Elem2MolWeight = 32.06; // S
                    result.K = targetAmount;
                    result.S = CalculateElementContribution(result.SaltConcentration_mgL, fertilizer.MolecularWeight, 32.06, fertilizer.Purity);
                    // Calcular SO4= desde S
                    result.SO4 = result.S * 96.06 / 32.06; // Conversión S a SO4=
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
            string[] ions = { "Ca", "K", "Mg", "Na", "NH4", "NO3", "N", "SO4", "S", "Cl", "H2PO4", "P", "HCO3" };

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

                // Asignar valores del agua para elementos básicos (Ca, K, Mg, N, P, S)
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
            }

            // Calcular concentraciones finales y conversiones
            foreach (string ion in ions)
            {
                // Final = Aporte + Agua
                balance.Final_mgL[ion] = balance.Aporte_mgL[ion] + balance.Agua_mgL[ion];

                // Convertir a mmol/L y meq/L
                if (elementData != null && elementData.ContainsKey(ion))
                {
                    // Para aportes
                    balance.Aporte_mmolL[ion] = balance.Aporte_mgL[ion] / elementData[ion].AtomicWeight;
                    balance.Aporte_meqL[ion] = balance.Aporte_mmolL[ion] * elementData[ion].Valence;

                    // Para agua
                    balance.Agua_mmolL[ion] = balance.Agua_mgL[ion] / elementData[ion].AtomicWeight;
                    balance.Agua_meqL[ion] = balance.Agua_mmolL[ion] * elementData[ion].Valence;

                    // Para final
                    balance.Final_mmolL[ion] = balance.Final_mgL[ion] / elementData[ion].AtomicWeight;
                    balance.Final_meqL[ion] = balance.Final_mmolL[ion] * elementData[ion].Valence;
                }
            }

            return balance;
        }
    }

    // Windows Forms UI
    public partial class NutrientCalculatorForm : Form
    {
        private NutrientCalculatorAdvanced calculator;
        private TabControl? tabControl;
        private DataGridView? fertilizerGrid;
        private DataGridView? balanceGrid;
        private DataGridView? verificationGrid;
        private RichTextBox? summaryTextBox;
        private RichTextBox? configTextBox;

        public NutrientCalculatorForm()
        {
            calculator = new NutrientCalculatorAdvanced();
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(1200, 800);
            this.Text = "Calculadora de Soluciones Nutritivas";
            this.StartPosition = FormStartPosition.CenterScreen;

            // Create tab control
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F)
            };

            // Configuration Tab
            var configTab = new TabPage("Configuración");
            configTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 10F),
                ReadOnly = true,
                BackColor = Color.LightGray
            };
            configTab.Controls.Add(configTextBox);
            tabControl.TabPages.Add(configTab);

            // Fertilizers Tab
            var fertilizerTab = new TabPage("Fertilizantes");
            fertilizerGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                Font = new Font("Segoe UI", 8F)
            };
            fertilizerTab.Controls.Add(fertilizerGrid);
            tabControl.TabPages.Add(fertilizerTab);

            // Ion Balance Tab
            var balanceTab = new TabPage("Balance de Iones");
            balanceGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                Font = new Font("Segoe UI", 8F)
            };
            balanceTab.Controls.Add(balanceGrid);
            tabControl.TabPages.Add(balanceTab);

            // Verification Tab
            var verificationTab = new TabPage("Verificación");
            verificationGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                Font = new Font("Segoe UI", 9F)
            };
            verificationTab.Controls.Add(verificationGrid);
            tabControl.TabPages.Add(verificationTab);

            // Summary Tab
            var summaryTab = new TabPage("Resumen");
            summaryTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 10F),
                ReadOnly = true,
                BackColor = Color.AliceBlue
            };
            summaryTab.Controls.Add(summaryTextBox);
            tabControl.TabPages.Add(summaryTab);

            this.Controls.Add(tabControl);
        }

        private void LoadData()
        {
            var results = calculator.CalculateSolution();
            var balance = calculator.CalculateIonBalance(results);
            var targetConcentrations = calculator.GetTargetConcentrations();
            var waterAnalysis = calculator.GetWaterAnalysis();

            LoadConfigurationData(targetConcentrations, waterAnalysis);
            LoadFertilizerData(results);
            LoadBalanceData(balance);
            LoadVerificationData(targetConcentrations, balance);
        }

        private void LoadConfigurationData(Dictionary<string, double> targetConcentrations, WaterAnalysis waterAnalysis)
        {
            var config = new System.Text.StringBuilder();
            config.AppendLine("=== CONFIGURACIÓN DE LA SOLUCIÓN NUTRITIVA ===\n");

            config.AppendLine("CONCENTRACIONES OBJETIVO (mg/L):");
            foreach (var target in targetConcentrations)
            {
                config.AppendLine($"  {target.Key}: {target.Value} mg/L");
            }

            config.AppendLine("\nCOMPOSICIÓN DEL AGUA (mg/L):");
            foreach (var water in waterAnalysis.Elements_mgL)
            {
                config.AppendLine($"  {water.Key}: {water.Value} mg/L");
            }
            config.AppendLine($"  pH: {waterAnalysis.pH}");
            config.AppendLine($"  EC: {waterAnalysis.EC} dS/m");
            config.AppendLine($"  HCO3-: {waterAnalysis.HCO3} mg/L");

            config.AppendLine("\nNUTRIENTES FALTANTES A APORTAR:");
            foreach (var target in targetConcentrations)
            {
                double waterContent = waterAnalysis.Elements_mgL.ContainsKey(target.Key)
                    ? waterAnalysis.Elements_mgL[target.Key] : 0;
                double remaining = Math.Max(0, target.Value - waterContent);
                config.AppendLine($"  {target.Key}: {remaining:F1} mg/L");
            }

            if (configTextBox != null)
                configTextBox.Text = config.ToString();
        }

        private void LoadFertilizerData(List<FertilizerResult> results)
        {
            if (fertilizerGrid == null)
                return;
            fertilizerGrid.Columns.Clear();
            fertilizerGrid.Columns.Add("Name", "Fertilizante");
            fertilizerGrid.Columns.Add("Purity", "P%");
            fertilizerGrid.Columns.Add("MolWeight", "PM Sal");
            fertilizerGrid.Columns.Add("Elem1Weight", "PM E1");
            fertilizerGrid.Columns.Add("Elem2Weight", "PM E2");
            fertilizerGrid.Columns.Add("Concentration", "mg/L");
            fertilizerGrid.Columns.Add("mmolL", "mmol/L");
            fertilizerGrid.Columns.Add("Ca", "Ca");
            fertilizerGrid.Columns.Add("K", "K");
            fertilizerGrid.Columns.Add("Mg", "Mg");
            fertilizerGrid.Columns.Add("NH4", "NH4");
            fertilizerGrid.Columns.Add("NO3", "NO3-");
            fertilizerGrid.Columns.Add("N", "N");
            fertilizerGrid.Columns.Add("SO4", "SO4=");
            fertilizerGrid.Columns.Add("S", "S");
            fertilizerGrid.Columns.Add("H2PO4", "H2PO4-");
            fertilizerGrid.Columns.Add("P", "P");

            foreach (var result in results)
            {
                fertilizerGrid.Rows.Add(
                    result.Name,
                    result.Purity.ToString("F0"),
                    result.MolecularWeight.ToString("F2"),
                    result.Elem1MolWeight.ToString("F2"),
                    result.Elem2MolWeight.ToString("F2"),
                    result.SaltConcentration_mgL.ToString("F2"),
                    result.SaltConcentration_mmolL.ToString("F3"),
                    result.Ca.ToString("F1"),
                    result.K.ToString("F1"),
                    result.Mg.ToString("F1"),
                    result.NH4.ToString("F1"),
                    result.NO3.ToString("F1"),
                    result.N.ToString("F1"),
                    result.SO4.ToString("F1"),
                    result.S.ToString("F1"),
                    result.H2PO4.ToString("F1"),
                    result.P.ToString("F1")
                );
            }

            // Adjust column widths
            fertilizerGrid.Columns["Name"].Width = 120;
            for (int i = 1; i < fertilizerGrid.Columns.Count; i++)
            {
                fertilizerGrid.Columns[i].Width = 60;
            }
        }

        private void LoadBalanceData(IonBalance balance)
        {
            if (balanceGrid == null)
                return;
            balanceGrid.Columns.Clear();
            balanceGrid.Columns.Add("Element", "Elemento");
            balanceGrid.Columns.Add("AporteMg", "Aporte mg/L");
            balanceGrid.Columns.Add("AporteMmol", "Aporte mmol/L");
            balanceGrid.Columns.Add("AporteMeq", "Aporte meq/L");
            balanceGrid.Columns.Add("AguaMg", "Agua mg/L");
            balanceGrid.Columns.Add("AguaMmol", "Agua mmol/L");
            balanceGrid.Columns.Add("AguaMeq", "Agua meq/L");
            balanceGrid.Columns.Add("FinalMg", "Final mg/L");
            balanceGrid.Columns.Add("FinalMmol", "Final mmol/L");
            balanceGrid.Columns.Add("FinalMeq", "Final meq/L");

            // Add cations section header
            int rowIndex = balanceGrid.Rows.Add("=== CATIONES ===", "", "", "", "", "", "", "", "", "");
            balanceGrid.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightBlue;
            balanceGrid.Rows[rowIndex].DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            string[] cations = { "Ca", "K", "Mg", "Na", "NH4" };
            foreach (string cation in cations)
            {
                balanceGrid.Rows.Add(
                    cation,
                    balance.Aporte_mgL[cation].ToString("F1"),
                    balance.Aporte_mmolL[cation].ToString("F2"),
                    balance.Aporte_meqL[cation].ToString("F2"),
                    balance.Agua_mgL[cation].ToString("F1"),
                    balance.Agua_mmolL[cation].ToString("F2"),
                    balance.Agua_meqL[cation].ToString("F2"),
                    balance.Final_mgL[cation].ToString("F1"),
                    balance.Final_mmolL[cation].ToString("F2"),
                    balance.Final_meqL[cation].ToString("F2")
                );
            }

            // Add anions section header
            rowIndex = balanceGrid.Rows.Add("=== ANIONES ===", "", "", "", "", "", "", "", "", "");
            balanceGrid.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightGreen;
            balanceGrid.Rows[rowIndex].DefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            string[] anions = { "NO3", "N", "SO4", "S", "Cl", "H2PO4", "P", "HCO3" };
            foreach (string anion in anions)
            {
                balanceGrid.Rows.Add(
                    anion,
                    balance.Aporte_mgL[anion].ToString("F1"),
                    balance.Aporte_mmolL[anion].ToString("F2"),
                    balance.Aporte_meqL[anion].ToString("F2"),
                    balance.Agua_mgL[anion].ToString("F1"),
                    balance.Agua_mmolL[anion].ToString("F2"),
                    balance.Agua_meqL[anion].ToString("F2"),
                    balance.Final_mgL[anion].ToString("F1"),
                    balance.Final_mmolL[anion].ToString("F2"),
                    balance.Final_meqL[anion].ToString("F2")
                );
            }

            balanceGrid.Columns["Element"].Width = 100;
        }

        private void LoadVerificationData(Dictionary<string, double> targetConcentrations, IonBalance balance)
        {
            if (verificationGrid == null)
                return;
            verificationGrid.Columns.Clear();
            verificationGrid.Columns.Add("Element", "Elemento");
            verificationGrid.Columns.Add("Target", "Deseado (mg/L)");
            verificationGrid.Columns.Add("Obtained", "Obtenido (mg/L)");
            verificationGrid.Columns.Add("Difference", "Diferencia (mg/L)");
            verificationGrid.Columns.Add("Status", "Estado");

            foreach (var target in targetConcentrations)
            {
                double obtained = balance.Final_mgL.ContainsKey(target.Key) ? balance.Final_mgL[target.Key] : 0;
                double difference = obtained - target.Value;
                string status = Math.Abs(difference) <= target.Value * 0.05 ? "✓ OK" :
                               difference > 0 ? "↑ Alto" : "↓ Bajo";

                int rowIndex = verificationGrid.Rows.Add(
                    target.Key,
                    target.Value.ToString("F1"),
                    obtained.ToString("F1"),
                    difference.ToString("F1"),
                    status
                );

                // Color coding
                if (status.Contains("OK"))
                    verificationGrid.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightGreen;
                else if (status.Contains("Alto"))
                    verificationGrid.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightCoral;
                else if (status.Contains("Bajo"))
                    verificationGrid.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightYellow;
            }

            verificationGrid.Columns["Element"].Width = 100;
            verificationGrid.Columns["Status"].Width = 80;
        }
    }

    // Programa principal
    class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new NutrientCalculatorForm());
        }
    }
}
*/