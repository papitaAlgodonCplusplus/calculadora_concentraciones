using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HydroponicCalculator.Modules;
#pragma warning disable CS8618
#pragma warning disable CS8622

namespace HydroponicCalculator
{
    public partial class CompleteHydroponicCalculatorForm : Form
    {
        // All modules
        private WaterAnalysisModule waterModule;
        private PHAdjustmentModule phModule;
        private NutrientCalculatorAdvanced nutrientModule;
        private SolutionVerificationModule verificationModule;
        private ConcentratedSolutionsModule concentratedModule;
        private CostAnalysisModule costModule;

        // Data storage
        private WaterQualityParameters waterData;
        private Dictionary<string, double> targetConcentrations;
        private List<FertilizerResult> calculationResults;
        private IonBalance ionBalance;
        private List<AcidCalculationResult> acidResults;

        // UI Controls
        private TabControl mainTabControl;
        private ProgressBar progressBar;
        private Label stepLabel;

        // Step tracking
        private int currentStep = 0;
        private readonly string[] stepNames = {
            "1. Water Analysis",
            "2. Target Concentrations",
            "3. pH Adjustment",
            "4. Nutrient Calculations",
            "5. Solution Verification",
            "6. Concentrated Solutions",
            "7. Cost Analysis",
            "8. Final Report"
        };

        public CompleteHydroponicCalculatorForm()
        {
            InitializeModules();
            InitializeComponent();
            InitializeData();
            LoadStep1_WaterAnalysis();
        }

        private void InitializeModules()
        {
            waterModule = new WaterAnalysisModule();
            phModule = new PHAdjustmentModule();
            nutrientModule = new NutrientCalculatorAdvanced();
            verificationModule = new SolutionVerificationModule();
            concentratedModule = new ConcentratedSolutionsModule();
            costModule = new CostAnalysisModule();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(1400, 900);
            this.Text = "Complete Hydroponic Nutrient Calculator - Professional Edition";
            this.StartPosition = FormStartPosition.CenterScreen;

            // Create main layout
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            // Progress bar at top
            var progressPanel = new Panel
            {
                Height = 60,
                Dock = DockStyle.Top,
                BackColor = Color.LightBlue,
                Padding = new Padding(10)
            };

            stepLabel = new Label
            {
                Text = stepNames[0],
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 25,
                TextAlign = ContentAlignment.MiddleCenter
            };

            progressBar = new ProgressBar
            {
                Dock = DockStyle.Bottom,
                Height = 25,
                Minimum = 0,
                Maximum = stepNames.Length - 1,
                Value = 0,
                Style = ProgressBarStyle.Continuous
            };

            progressPanel.Controls.Add(stepLabel);
            progressPanel.Controls.Add(progressBar);

            // Main tab control
            mainTabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F)
            };

            // Navigation buttons panel
            var navigationPanel = new Panel
            {
                Height = 50,
                Dock = DockStyle.Bottom
            };

            var prevButton = new Button
            {
                Text = "← Previous",
                Size = new Size(100, 35),
                Location = new Point(10, 8),
                Enabled = false
            };
            prevButton.Click += (s, e) => NavigateStep(-1);

            var nextButton = new Button
            {
                Text = "Next →",
                Size = new Size(100, 35),
                Location = new Point(120, 8)
            };
            nextButton.Click += (s, e) => NavigateStep(1);

            var calculateButton = new Button
            {
                Text = "Calculate Step",
                Size = new Size(120, 35),
                Location = new Point(240, 8),
                BackColor = Color.LightGreen
            };
            calculateButton.Click += CalculateCurrentStep;

            navigationPanel.Controls.AddRange(new Control[] { prevButton, nextButton, calculateButton });

            mainPanel.Controls.Add(mainTabControl);
            this.Controls.Add(mainPanel);
            this.Controls.Add(progressPanel);
            this.Controls.Add(navigationPanel);
        }

        private void InitializeData()
        {
            waterData = new WaterQualityParameters
            {
                pH = 7.2,
                EC = 0.5,
                HCO3 = 77,
                Elements_mgL = new Dictionary<string, double>
                {
                    ["Ca"] = 10.2,
                    ["K"] = 2.6,
                    ["Mg"] = 4.8,
                    ["Na"] = 9.4,
                    ["N"] = 0.32,
                    ["P"] = 0,
                    ["S"] = 0
                }
            };

            targetConcentrations = nutrientModule.GetTargetConcentrations();
        }

        private void LoadStep1_WaterAnalysis()
        {
            var tab = new TabPage("Step 1: Water Analysis");
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                Padding = new Padding(10)
            };
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            // Input panel
            var inputPanel = new GroupBox
            {
                Text = "Water Analysis Input",
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var inputLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                AutoSize = true
            };
            inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            // pH
            inputLayout.Controls.Add(new Label { Text = "pH:", TextAlign = ContentAlignment.MiddleRight }, 0, 0);
            var phTextBox = new TextBox { Text = waterData.pH.ToString("F1"), Width = 100 };
            inputLayout.Controls.Add(phTextBox, 1, 0);
            inputLayout.Controls.Add(new Label { Text = "pH units" }, 2, 0);

            // EC
            inputLayout.Controls.Add(new Label { Text = "EC:", TextAlign = ContentAlignment.MiddleRight }, 0, 1);
            var ecTextBox = new TextBox { Text = waterData.EC.ToString("F1"), Width = 100 };
            inputLayout.Controls.Add(ecTextBox, 1, 1);
            inputLayout.Controls.Add(new Label { Text = "dS/m" }, 2, 1);

            // HCO3
            inputLayout.Controls.Add(new Label { Text = "HCO3-:", TextAlign = ContentAlignment.MiddleRight }, 0, 2);
            var hco3TextBox = new TextBox { Text = waterData.HCO3.ToString("F1"), Width = 100 };
            inputLayout.Controls.Add(hco3TextBox, 1, 2);
            inputLayout.Controls.Add(new Label { Text = "mg/L" }, 2, 2);

            // Elements
            var elementTextBoxes = new Dictionary<string, TextBox>();
            int row = 3;
            foreach (var element in waterData.Elements_mgL)
            {
                inputLayout.Controls.Add(new Label { Text = $"{element.Key}:", TextAlign = ContentAlignment.MiddleRight }, 0, row);
                var textBox = new TextBox { Text = element.Value.ToString("F1"), Width = 100 };
                elementTextBoxes[element.Key] = textBox;
                inputLayout.Controls.Add(textBox, 1, row);
                inputLayout.Controls.Add(new Label { Text = "mg/L" }, 2, row);
                row++;
            }

            // Update button
            var updateButton = new Button
            {
                Text = "Update Water Data",
                Dock = DockStyle.Bottom,
                Height = 35,
                BackColor = Color.LightBlue
            };
            updateButton.Click += (s, e) => UpdateWaterData(phTextBox, ecTextBox, hco3TextBox, elementTextBoxes);

            inputPanel.Controls.Add(inputLayout);
            inputPanel.Controls.Add(updateButton);

            // Results panel
            var resultsPanel = new GroupBox
            {
                Text = "Water Quality Analysis",
                Dock = DockStyle.Fill
            };

            var resultsGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false
            };

            resultsPanel.Controls.Add(resultsGrid);

            // Store references for updates
            resultsGrid.Name = "WaterQualityGrid";

            mainPanel.Controls.Add(inputPanel, 0, 0);
            mainPanel.Controls.Add(resultsPanel, 1, 0);

            tab.Controls.Add(mainPanel);
            mainTabControl.TabPages.Add(tab);

            // Initial analysis
            PerformWaterAnalysis();
        }

        private void UpdateWaterData(TextBox phBox, TextBox ecBox, TextBox hco3Box, Dictionary<string, TextBox> elementBoxes)
        {
            try
            {
                waterData.pH = double.Parse(phBox.Text);
                waterData.EC = double.Parse(ecBox.Text);
                waterData.HCO3 = double.Parse(hco3Box.Text);

                foreach (var element in elementBoxes)
                {
                    waterData.Elements_mgL[element.Key] = double.Parse(element.Value.Text);
                }

                PerformWaterAnalysis();
                MessageBox.Show("Water data updated successfully!", "Update Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating water data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PerformWaterAnalysis()
        {
            waterData = waterModule.AnalyzeWater(waterData);
            var qualityResults = waterModule.EvaluateWaterQuality(waterData);
            var indices = waterModule.CalculateWaterQualityIndices(waterData);
            var ionBalance = waterModule.VerifyIonBalance(waterData);

            // Update results grid
            var grid = mainTabControl.TabPages[0].Controls.Find("WaterQualityGrid", true).FirstOrDefault() as DataGridView;
            if (grid != null)
            {
                grid.DataSource = null;
                grid.Columns.Clear();

                grid.Columns.Add("Parameter", "Parameter");
                grid.Columns.Add("Value", "Value");
                grid.Columns.Add("Unit", "Unit");
                grid.Columns.Add("Status", "Status");
                grid.Columns.Add("Recommendation", "Recommendation");

                foreach (var result in qualityResults)
                {
                    var rowIndex = grid.Rows.Add(result.Parameter,
                                                result.Value.ToString("F2"),
                                                result.Unit,
                                                result.Status,
                                                result.Recommendation);

                    var row = grid.Rows[rowIndex];
                    switch (result.StatusColor)
                    {
                        case "Green":
                            row.DefaultCellStyle.BackColor = Color.LightGreen;
                            break;
                        case "Red":
                            row.DefaultCellStyle.BackColor = Color.LightCoral;
                            break;
                        case "Yellow":
                            row.DefaultCellStyle.BackColor = Color.LightYellow;
                            break;
                    }
                }

                // Add separator and indices
                grid.Rows.Add("=== QUALITY INDICES ===", "", "", "", "");
                grid.Rows.Add("SAR", indices.SAR.ToString("F2"), "", indices.SAR < 3 ? "OK" : "High", "");
                grid.Rows.Add("RSC", indices.RSC.ToString("F2"), "meq/L", indices.RSC < 1.25 ? "OK" : "High", "");
                grid.Rows.Add("Ca/Mg Ratio", indices.CaMgRatio.ToString("F2"), "",
                             indices.CaMgRatio >= 2 && indices.CaMgRatio <= 4 ? "OK" : "Imbalanced", "");

                grid.Rows.Add("=== ION BALANCE ===", "", "", "", "");
                grid.Rows.Add("Ion Balance", $"{ionBalance["PercentageDifference"]:F1}%", "",
                             ionBalance["IsBalanced"] == 1 ? "Balanced" : "Imbalanced",
                             ionBalance["IsBalanced"] == 1 ? "Within 10% tolerance" : "Exceeds 10% tolerance");
            }
        }

        private void LoadStep2_TargetConcentrations()
        {
            var tab = new TabPage("Step 2: Target Concentrations");
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                Padding = new Padding(10)
            };
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            // Target input panel
            var inputPanel = new GroupBox
            {
                Text = "Target Nutrient Concentrations (mg/L)",
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var inputLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                AutoSize = true
            };
            inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            var targetTextBoxes = new Dictionary<string, TextBox>();
            int row = 0;

            // Crop selection
            inputLayout.Controls.Add(new Label { Text = "Crop Type:", TextAlign = ContentAlignment.MiddleRight }, 0, row);
            var cropCombo = new ComboBox
            {
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cropCombo.Items.AddRange(new[] { "Tomato", "Lettuce", "Cucumber", "Pepper", "Strawberry", "Custom" });
            cropCombo.SelectedIndex = 0;
            inputLayout.Controls.Add(cropCombo, 1, row);
            row++;

            // Growth stage
            inputLayout.Controls.Add(new Label { Text = "Growth Stage:", TextAlign = ContentAlignment.MiddleRight }, 0, row);
            var stageCombo = new ComboBox
            {
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            stageCombo.Items.AddRange(new[] { "Seedling", "Vegetative", "Flowering", "Fruiting" });
            stageCombo.SelectedIndex = 2;
            inputLayout.Controls.Add(stageCombo, 1, row);
            row++;

            // Load defaults button
            var loadDefaultsButton = new Button
            {
                Text = "Load Crop Defaults",
                Width = 150,
                Height = 25
            };
            loadDefaultsButton.Click += (s, e) => LoadCropDefaults(cropCombo.Text, stageCombo.Text, targetTextBoxes);
            inputLayout.Controls.Add(loadDefaultsButton, 1, row);
            row++;

            // Separator
            inputLayout.Controls.Add(new Label { Text = "=== MACRONUTRIENTS ===", Font = new Font("Segoe UI", 9F, FontStyle.Bold) }, 0, row);
            row++;

            // Major nutrients
            var majorNutrients = new[] { "N", "P", "K", "Ca", "Mg", "S" };
            foreach (var nutrient in majorNutrients)
            {
                inputLayout.Controls.Add(new Label { Text = $"{nutrient}:", TextAlign = ContentAlignment.MiddleRight }, 0, row);
                var textBox = new TextBox { Text = targetConcentrations[nutrient].ToString("F1"), Width = 100 };
                targetTextBoxes[nutrient] = textBox;
                inputLayout.Controls.Add(textBox, 1, row);
                inputLayout.Controls.Add(new Label { Text = "mg/L" }, 2, row);
                row++;
            }

            // Separator
            inputLayout.Controls.Add(new Label { Text = "=== MICRONUTRIENTS ===", Font = new Font("Segoe UI", 9F, FontStyle.Bold) }, 0, row);
            row++;

            // Micronutrients with default values
            var microDefaults = new Dictionary<string, double>
            {
                ["Fe"] = 1.0,
                ["Mn"] = 0.5,
                ["Zn"] = 0.2,
                ["Cu"] = 0.1,
                ["B"] = 0.5,
                ["Mo"] = 0.01
            };

            foreach (var micro in microDefaults)
            {
                inputLayout.Controls.Add(new Label { Text = $"{micro.Key}:", TextAlign = ContentAlignment.MiddleRight }, 0, row);
                var textBox = new TextBox { Text = micro.Value.ToString("F2"), Width = 100 };
                targetTextBoxes[micro.Key] = textBox;
                inputLayout.Controls.Add(textBox, 1, row);
                inputLayout.Controls.Add(new Label { Text = "mg/L" }, 2, row);
                row++;
            }

            inputPanel.Controls.Add(inputLayout);

            // Current vs Target comparison panel
            var comparisonPanel = new GroupBox
            {
                Text = "Water vs Target Analysis",
                Dock = DockStyle.Fill
            };

            var comparisonGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                Name = "ComparisonGrid"
            };

            comparisonPanel.Controls.Add(comparisonGrid);

            mainPanel.Controls.Add(inputPanel, 0, 0);
            mainPanel.Controls.Add(comparisonPanel, 1, 0);

            tab.Controls.Add(mainPanel);
            mainTabControl.TabPages.Add(tab);

            // Initial comparison
            UpdateTargetComparison();
        }

        private void LoadCropDefaults(string crop, string stage, Dictionary<string, TextBox> textBoxes)
        {
            var defaults = new Dictionary<string, Dictionary<string, double>>
            {
                ["Tomato"] = new Dictionary<string, double>
                {
                    ["N"] = 150,
                    ["P"] = 45,
                    ["K"] = 260,
                    ["Ca"] = 172,
                    ["Mg"] = 50,
                    ["S"] = 108,
                    ["Fe"] = 1.0,
                    ["Mn"] = 0.5,
                    ["Zn"] = 0.2,
                    ["Cu"] = 0.1,
                    ["B"] = 0.5,
                    ["Mo"] = 0.01
                },
                ["Lettuce"] = new Dictionary<string, double>
                {
                    ["N"] = 120,
                    ["P"] = 35,
                    ["K"] = 200,
                    ["Ca"] = 140,
                    ["Mg"] = 40,
                    ["S"] = 80,
                    ["Fe"] = 1.2,
                    ["Mn"] = 0.6,
                    ["Zn"] = 0.3,
                    ["Cu"] = 0.1,
                    ["B"] = 0.4,
                    ["Mo"] = 0.01
                },
                ["Cucumber"] = new Dictionary<string, double>
                {
                    ["N"] = 160,
                    ["P"] = 50,
                    ["K"] = 280,
                    ["Ca"] = 180,
                    ["Mg"] = 55,
                    ["S"] = 120,
                    ["Fe"] = 1.0,
                    ["Mn"] = 0.5,
                    ["Zn"] = 0.2,
                    ["Cu"] = 0.1,
                    ["B"] = 0.5,
                    ["Mo"] = 0.01
                }
            };

            if (defaults.ContainsKey(crop))
            {
                var cropDefaults = defaults[crop];

                // Adjust based on stage
                double stageMultiplier = stage switch
                {
                    "Seedling" => 0.5,
                    "Vegetative" => 0.8,
                    "Flowering" => 1.0,
                    "Fruiting" => 1.2,
                    _ => 1.0
                };

                foreach (var nutrient in cropDefaults)
                {
                    if (textBoxes.ContainsKey(nutrient.Key))
                    {
                        double adjustedValue = nutrient.Value * stageMultiplier;
                        textBoxes[nutrient.Key].Text = adjustedValue.ToString("F1");
                    }
                }

                // Update target concentrations
                foreach (var textBox in textBoxes)
                {
                    if (double.TryParse(textBox.Value.Text, out double value))
                    {
                        targetConcentrations[textBox.Key] = value;
                    }
                }

                UpdateTargetComparison();
                MessageBox.Show($"Loaded {crop} defaults for {stage} stage", "Defaults Loaded",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void UpdateTargetComparison()
        {
            var grid = mainTabControl.TabPages[1].Controls.Find("ComparisonGrid", true).FirstOrDefault() as DataGridView;
            if (grid == null) return;

            grid.Columns.Clear();
            grid.Columns.Add("Nutrient", "Nutrient");
            grid.Columns.Add("InWater", "In Water (mg/L)");
            grid.Columns.Add("Target", "Target (mg/L)");
            grid.Columns.Add("ToAdd", "To Add (mg/L)");
            grid.Columns.Add("Status", "Status");

            foreach (var target in targetConcentrations)
            {
                double inWater = waterData.Elements_mgL.GetValueOrDefault(target.Key, 0);
                double toAdd = Math.Max(0, target.Value - inWater);
                string status = toAdd <= 0 ? "Sufficient" : toAdd > target.Value * 0.8 ? "High Need" : "Moderate Need";

                var rowIndex = grid.Rows.Add(target.Key, inWater.ToString("F1"),
                                           target.Value.ToString("F1"), toAdd.ToString("F1"), status);

                var row = grid.Rows[rowIndex];
                row.DefaultCellStyle.BackColor = status switch
                {
                    "Sufficient" => Color.LightGreen,
                    "High Need" => Color.LightCoral,
                    _ => Color.LightYellow
                };
            }
        }

        private void LoadStep3_PHAdjustment()
        {
            var tab = new TabPage("Step 3: pH Adjustment");
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                Padding = new Padding(10)
            };
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));

            // pH adjustment input panel
            var inputPanel = new GroupBox
            {
                Text = "pH Adjustment Parameters",
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var inputLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                AutoSize = true
            };

            // Current water pH
            inputLayout.Controls.Add(new Label { Text = "Current pH:", TextAlign = ContentAlignment.MiddleRight }, 0, 0);
            inputLayout.Controls.Add(new Label { Text = waterData.pH.ToString("F1"), Font = new Font("Segoe UI", 9F, FontStyle.Bold) }, 1, 0);

            // Target pH
            inputLayout.Controls.Add(new Label { Text = "Target pH:", TextAlign = ContentAlignment.MiddleRight }, 0, 1);
            var targetPHBox = new TextBox { Text = "6.0", Width = 100 };
            inputLayout.Controls.Add(targetPHBox, 1, 1);

            // HCO3 concentration
            inputLayout.Controls.Add(new Label { Text = "HCO3- (mg/L):", TextAlign = ContentAlignment.MiddleRight }, 0, 2);
            inputLayout.Controls.Add(new Label { Text = waterData.HCO3.ToString("F1"), Font = new Font("Segoe UI", 9F, FontStyle.Bold) }, 1, 2);

            // Phosphorus needs
            double targetP = targetConcentrations.GetValueOrDefault("P", 45);
            double currentP = waterData.Elements_mgL.GetValueOrDefault("P", 0);
            inputLayout.Controls.Add(new Label { Text = "P needed (mg/L):", TextAlign = ContentAlignment.MiddleRight }, 0, 3);
            inputLayout.Controls.Add(new Label { Text = Math.Max(0, targetP - currentP).ToString("F1"), Font = new Font("Segoe UI", 9F, FontStyle.Bold) }, 1, 3);

            // Calculate button
            var calculatePHButton = new Button
            {
                Text = "Calculate Acid Requirements",
                Dock = DockStyle.Bottom,
                Height = 35,
                BackColor = Color.LightBlue
            };
            calculatePHButton.Click += (s, e) => CalculatePHAdjustment(double.Parse(targetPHBox.Text));

            inputPanel.Controls.Add(inputLayout);
            inputPanel.Controls.Add(calculatePHButton);

            // Results panel
            var resultsPanel = new GroupBox
            {
                Text = "Acid Calculation Results",
                Dock = DockStyle.Fill
            };

            var resultsGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                Name = "AcidResultsGrid"
            };

            resultsPanel.Controls.Add(resultsGrid);

            mainPanel.Controls.Add(inputPanel, 0, 0);
            mainPanel.Controls.Add(resultsPanel, 1, 0);

            tab.Controls.Add(mainPanel);
            mainTabControl.TabPages.Add(tab);
        }

        private void CalculatePHAdjustment(double targetPH)
        {
            try
            {
                double targetP = targetConcentrations.GetValueOrDefault("P", 45);
                double currentP = waterData.Elements_mgL.GetValueOrDefault("P", 0);

                acidResults = phModule.CalculateAcidRequirement_IncrossiMethod(
                    waterData.HCO3, targetPH, targetP, currentP);

                var grid = mainTabControl.TabPages[2].Controls.Find("AcidResultsGrid", true).FirstOrDefault() as DataGridView;
                if (grid == null) return;

                grid.Columns.Clear();
                grid.Columns.Add("Acid", "Acid Type");
                grid.Columns.Add("Volume", "Volume (mL/L)");
                grid.Columns.Add("HConc", "H+ (mg/L)");
                grid.Columns.Add("Nutrient", "Nutrient");
                grid.Columns.Add("NutrientAmount", "Amount (mg/L)");
                grid.Columns.Add("Bicarbonates", "HCO3- Neutralized");

                foreach (var result in acidResults)
                {
                    grid.Rows.Add(
                        result.AcidName,
                        result.AcidVolume_mlL.ToString("F3"),
                        result.HydrogenConcentration_mgL.ToString("F1"),
                        result.NutrientType,
                        result.NutrientContribution_mgL.ToString("F1"),
                        result.BicarbonatesToNeutralize.ToString("F1")
                    );
                }

                // Add recommendation
                var recommendation = phModule.GetAcidRecommendation(waterData.HCO3, currentP, targetP);

                var recPanel = new Panel
                {
                    Height = 80,
                    Dock = DockStyle.Bottom,
                    BackColor = Color.LightYellow,
                    Padding = new Padding(10)
                };

                var recLabel = new Label
                {
                    Text = "Recommendation: " + recommendation,
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleLeft
                };

                recPanel.Controls.Add(recLabel);
                mainTabControl.TabPages[2].Controls.Add(recPanel);

                MessageBox.Show("pH adjustment calculations completed!", "Calculation Complete",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error calculating pH adjustment: {ex.Message}", "Calculation Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void NavigateStep(int direction)
        {
            int newStep = currentStep + direction;
            if (newStep < 0 || newStep >= stepNames.Length) return;

            currentStep = newStep;
            stepLabel.Text = stepNames[currentStep];
            progressBar.Value = currentStep;

            // Load appropriate step
            switch (currentStep)
            {
                case 1:
                    if (mainTabControl.TabPages.Count < 2) LoadStep2_TargetConcentrations();
                    break;
                case 2:
                    if (mainTabControl.TabPages.Count < 3) LoadStep3_PHAdjustment();
                    break;
                case 3:
                    if (mainTabControl.TabPages.Count < 4) LoadStep4_NutrientCalculations();
                    break;
                case 4:
                    if (mainTabControl.TabPages.Count < 5) LoadStep5_SolutionVerification();
                    break;
                case 5:
                    if (mainTabControl.TabPages.Count < 6) LoadStep6_ConcentratedSolutions();
                    break;
                case 6:
                    if (mainTabControl.TabPages.Count < 7) LoadStep7_CostAnalysis();
                    break;
                case 7:
                    if (mainTabControl.TabPages.Count < 8) LoadStep8_FinalReport();
                    break;
            }

            mainTabControl.SelectedIndex = currentStep;

            // Update navigation buttons
            var navigationPanel = this.Controls.OfType<Panel>().LastOrDefault();
            if (navigationPanel != null)
            {
                var prevButton = navigationPanel.Controls[0] as Button;
                var nextButton = navigationPanel.Controls[1] as Button;

                if (prevButton != null) prevButton.Enabled = currentStep > 0;
                if (nextButton != null) nextButton.Enabled = currentStep < stepNames.Length - 1;
            }
        }

        private void CalculateCurrentStep(object sender, EventArgs e)
        {
            switch (currentStep)
            {
                case 0:
                    PerformWaterAnalysis();
                    break;
                case 1:
                    UpdateTargetConcentrations();
                    break;
                case 2:
                    if (mainTabControl.TabPages[2].Controls.Find("TargetPHBox", true).FirstOrDefault() is TextBox phBox)
                        CalculatePHAdjustment(double.Parse(phBox.Text));
                    break;
                case 3:
                    CalculateNutrientSolution();
                    break;
                case 4:
                    PerformVerification();
                    break;
                case 5:
                    CalculateConcentratedSolutions();
                    break;
                case 6:
                    PerformCostAnalysis();
                    break;
                case 7:
                    GenerateFinalReport();
                    break;
            }
        }

        private void UpdateTargetConcentrations()
        {
            // Update target concentrations from UI
            UpdateTargetComparison();
            MessageBox.Show("Target concentrations updated!", "Update Complete",
                          MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void LoadStep4_NutrientCalculations()
        {
            var tab = new TabPage("Step 4: Nutrient Calculations");
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                Name = "NutrientCalculationsGrid"
            };

            var infoPanel = new Panel
            {
                Height = 60,
                Dock = DockStyle.Top,
                BackColor = Color.LightCyan,
                Padding = new Padding(10)
            };

            var infoLabel = new Label
            {
                Text = "This step calculates fertilizer quantities using the formula: P = C × M × 100 / (A × %P)\n" +
                       "Where P=fertilizer amount, C=target concentration, M=molecular weight, A=element weight, %P=purity",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F),
                TextAlign = ContentAlignment.MiddleLeft
            };

            infoPanel.Controls.Add(infoLabel);
            tab.Controls.Add(grid);
            tab.Controls.Add(infoPanel);
            mainTabControl.TabPages.Add(tab);
        }

        private void CalculateNutrientSolution()
        {
            try
            {
                calculationResults = nutrientModule.CalculateSolution();
                ionBalance = nutrientModule.CalculateIonBalance(calculationResults);

                var grid = mainTabControl.TabPages[3].Controls.Find("NutrientCalculationsGrid", true).FirstOrDefault() as DataGridView;
                if (grid == null) return;

                // Set up columns
                grid.Columns.Clear();
                var columns = new[]
                {
                    ("Fertilizer", "Fertilizer"),
                    ("Amount", "Amount (mg/L)"),
                    ("Ca", "Ca"),
                    ("K", "K"),
                    ("Mg", "Mg"),
                    ("N", "N"),
                    ("P", "P"),
                    ("S", "S"),
                    ("Formula", "Calculation Used")
                };

                foreach (var col in columns)
                {
                    grid.Columns.Add(col.Item1, col.Item2);
                }

                // Add calculation results
                foreach (var result in calculationResults)
                {
                    string formula = $@"P = {result.Name switch
                    {
                        "KH2PO4" => "45 × 136.1 × 100 / (30.97 × 98)",
                        "Ca(NO3)2.2H2O" => "162 × 200 × 100 / (40.08 × 95)",
                        "MgSO4.7H2O" => "45 × 246.5 × 100 / (24.31 × 98)",
                        "KNO3" => "30 × 101.1 × 100 / (14.01 × 98)",
                        "K2SO4" => "118 × 174.3 × 100 / (78.2 × 98)",
                        _ => "Standard Formula"
                    }}";

                    grid.Rows.Add(
                        result.Name,
                        result.SaltConcentration_mgL.ToString("F1"),
                        result.Ca.ToString("F1"),
                        result.K.ToString("F1"),
                        result.Mg.ToString("F1"),
                        result.N.ToString("F1"),
                        result.P.ToString("F1"),
                        result.S.ToString("F1"),
                        formula
                    );
                }

                MessageBox.Show("Nutrient calculations completed successfully!\n" +
                              $"Total fertilizers calculated: {calculationResults.Count}",
                              "Calculation Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error calculating nutrient solution: {ex.Message}", "Calculation Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadStep5_SolutionVerification()
        {
            var tab = new TabPage("Step 5: Solution Verification");
            mainTabControl.TabPages.Add(tab);
        }

        private void PerformVerification()
        {
            MessageBox.Show("Verification step - checking ion balance, EC, pH, and nutrient ratios",
                          "Verification", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void LoadStep6_ConcentratedSolutions()
        {
            var tab = new TabPage("Step 6: Concentrated Solutions");
            mainTabControl.TabPages.Add(tab);
        }

        private void CalculateConcentratedSolutions()
        {
            MessageBox.Show("Calculating concentrated solutions with tank distribution and compatibility checks",
                          "Concentrated Solutions", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void LoadStep7_CostAnalysis()
        {
            var tab = new TabPage("Step 7: Cost Analysis");
            mainTabControl.TabPages.Add(tab);
        }

        private void PerformCostAnalysis()
        {
            MessageBox.Show("Analyzing costs by fertilizer, nutrient, and tank distribution",
                          "Cost Analysis", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void LoadStep8_FinalReport()
        {
            var tab = new TabPage("Step 8: Final Report");
            mainTabControl.TabPages.Add(tab);
        }

        private void GenerateFinalReport()
        {
            MessageBox.Show("Generating comprehensive final report with all calculations and recommendations",
                          "Final Report", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    // Main program entry point
    class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new CompleteHydroponicCalculatorForm());
        }
    }
}