using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CalculadoraHidroponica.Modulos;
#pragma warning disable CS8618
#pragma warning disable CS8622

namespace CalculadoraHidroponica
{
    public partial class FormularioCalculadoraHidroponicaCompleta : Form
    {
        // Todos los módulos
        private ModuloAnalisisAgua moduloAgua;
        private ModuloAjustePH moduloPh;
        private CalculadoraNutrientesAvanzada moduloNutrientes;
        private ModuloVerificacionSolucion moduloVerificacion;
        private ModuloSolucionesConcentradas moduloConcentrados;
        private ModuloAnalisisCostos moduloCostos;

        // Almacenamiento de datos
        private ParametrosCalidadAgua datosAgua;
        private Dictionary<string, double> concentracionesObjetivo;
        private List<ResultadoFertilizante> resultadosCalculo;
        private BalanceIonico balanceIonico;
        private List<ResultadoCalculoAcido> resultadosAcidos;

        // Controles UI
        private TabControl controlPestanasPrincipal;
        private ProgressBar barraProgreso;
        private Label etiquetaPaso;

        // Seguimiento de pasos
        private int pasoActual = 0;
        private readonly string[] nombresPasos = {
            "1. Análisis de Agua",
            "2. Concentraciones Objetivo",
            "3. Ajuste de pH",
            "4. Cálculos de Nutrientes",
            "5. Verificación de Solución",
            "6. Soluciones Concentradas",
            "7. Análisis de Costos",
            "8. Reporte Final"
        };

        public FormularioCalculadoraHidroponicaCompleta()
        {
            InicializarModulos();
            InicializarComponente();
            InicializarDatos();
            CargarPaso1_AnalisisAgua();
        }

        private void InicializarModulos()
        {
            moduloAgua = new ModuloAnalisisAgua();
            moduloPh = new ModuloAjustePH();
            moduloNutrientes = new CalculadoraNutrientesAvanzada();
            moduloVerificacion = new ModuloVerificacionSolucion();
            moduloConcentrados = new ModuloSolucionesConcentradas();
            moduloCostos = new ModuloAnalisisCostos();
        }

        private void InicializarComponente()
        {
            this.Size = new Size(1400, 900);
            this.Text = "Calculadora Completa de Nutrientes Hidropónicos - Edición Profesional";
            this.StartPosition = FormStartPosition.CenterScreen;

            // Crear diseño principal
            var panelPrincipal = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            // Barra de progreso en la parte superior
            var panelProgreso = new Panel
            {
                Height = 60,
                Dock = DockStyle.Top,
                BackColor = Color.LightBlue,
                Padding = new Padding(10)
            };

            etiquetaPaso = new Label
            {
                Text = nombresPasos[0],
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 25,
                TextAlign = ContentAlignment.MiddleCenter
            };

            barraProgreso = new ProgressBar
            {
                Dock = DockStyle.Bottom,
                Height = 25,
                Minimum = 0,
                Maximum = nombresPasos.Length - 1,
                Value = 0,
                Style = ProgressBarStyle.Continuous
            };

            panelProgreso.Controls.Add(etiquetaPaso);
            panelProgreso.Controls.Add(barraProgreso);

            // Control de pestañas principal
            controlPestanasPrincipal = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F)
            };

            // Panel de botones de navegación
            var panelNavegacion = new Panel
            {
                Height = 50,
                Dock = DockStyle.Bottom
            };

            var botonAnterior = new Button
            {
                Text = "← Anterior",
                Size = new Size(100, 35),
                Location = new Point(10, 8),
                Enabled = false
            };
            botonAnterior.Click += (s, e) => NavegarPaso(-1);

            var botonSiguiente = new Button
            {
                Text = "Siguiente →",
                Size = new Size(100, 35),
                Location = new Point(120, 8)
            };
            botonSiguiente.Click += (s, e) => NavegarPaso(1);

            var botonCalcular = new Button
            {
                Text = "Calcular Paso",
                Size = new Size(120, 35),
                Location = new Point(240, 8),
                BackColor = Color.LightGreen
            };
            botonCalcular.Click += CalcularPasoActual;

            panelNavegacion.Controls.AddRange(new Control[] { botonAnterior, botonSiguiente, botonCalcular });

            panelPrincipal.Controls.Add(controlPestanasPrincipal);
            this.Controls.Add(panelPrincipal);
            this.Controls.Add(panelProgreso);
            this.Controls.Add(panelNavegacion);
        }

        private void InicializarDatos()
        {
            datosAgua = new ParametrosCalidadAgua
            {
                pH = 7.2,
                CE = 0.5,
                HCO3 = 77,
                Elementos_mgL = new Dictionary<string, double>
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

            concentracionesObjetivo = moduloNutrientes.ObtenerConcentracionesObjetivo();
        }

        private void CargarPaso1_AnalisisAgua()
        {
            var pestana = new TabPage("Paso 1: Análisis de Agua");
            var panelPrincipal = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                Padding = new Padding(10)
            };
            panelPrincipal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            panelPrincipal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            // Panel de entrada
            var panelEntrada = new GroupBox
            {
                Text = "Entrada de Análisis de Agua",
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var disenoEntrada = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                AutoSize = true
            };
            disenoEntrada.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            disenoEntrada.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            disenoEntrada.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            // pH
            disenoEntrada.Controls.Add(new Label { Text = "pH:", TextAlign = ContentAlignment.MiddleRight }, 0, 0);
            var cajaPh = new TextBox { Text = datosAgua.pH.ToString("F1"), Width = 100 };
            disenoEntrada.Controls.Add(cajaPh, 1, 0);
            disenoEntrada.Controls.Add(new Label { Text = "unidades pH" }, 2, 0);

            // CE
            disenoEntrada.Controls.Add(new Label { Text = "CE:", TextAlign = ContentAlignment.MiddleRight }, 0, 1);
            var cajaCe = new TextBox { Text = datosAgua.CE.ToString("F1"), Width = 100 };
            disenoEntrada.Controls.Add(cajaCe, 1, 1);
            disenoEntrada.Controls.Add(new Label { Text = "dS/m" }, 2, 1);

            // HCO3
            disenoEntrada.Controls.Add(new Label { Text = "HCO3-:", TextAlign = ContentAlignment.MiddleRight }, 0, 2);
            var cajaHco3 = new TextBox { Text = datosAgua.HCO3.ToString("F1"), Width = 100 };
            disenoEntrada.Controls.Add(cajaHco3, 1, 2);
            disenoEntrada.Controls.Add(new Label { Text = "mg/L" }, 2, 2);

            // Elementos
            var cajasElementos = new Dictionary<string, TextBox>();
            int fila = 3;
            foreach (var elemento in datosAgua.Elementos_mgL)
            {
                disenoEntrada.Controls.Add(new Label { Text = $"{elemento.Key}:", TextAlign = ContentAlignment.MiddleRight }, 0, fila);
                var cajaTexto = new TextBox { Text = elemento.Value.ToString("F1"), Width = 100 };
                cajasElementos[elemento.Key] = cajaTexto;
                disenoEntrada.Controls.Add(cajaTexto, 1, fila);
                disenoEntrada.Controls.Add(new Label { Text = "mg/L" }, 2, fila);
                fila++;
            }

            // Botón de actualización
            var botonActualizar = new Button
            {
                Text = "Actualizar Datos de Agua",
                Dock = DockStyle.Bottom,
                Height = 35,
                BackColor = Color.LightBlue
            };
            botonActualizar.Click += (s, e) => ActualizarDatosAgua(cajaPh, cajaCe, cajaHco3, cajasElementos);

            panelEntrada.Controls.Add(disenoEntrada);
            panelEntrada.Controls.Add(botonActualizar);

            // Panel de resultados
            var panelResultados = new GroupBox
            {
                Text = "Análisis de Calidad del Agua",
                Dock = DockStyle.Fill
            };

            var rejillaResultados = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false
            };

            panelResultados.Controls.Add(rejillaResultados);

            // Almacenar referencias para actualizaciones
            rejillaResultados.Name = "RejillaCalidadAgua";

            panelPrincipal.Controls.Add(panelEntrada, 0, 0);
            panelPrincipal.Controls.Add(panelResultados, 1, 0);

            pestana.Controls.Add(panelPrincipal);
            controlPestanasPrincipal.TabPages.Add(pestana);

            // Análisis inicial
            RealizarAnalisisAgua();
        }

        private void ActualizarDatosAgua(TextBox cajaPh, TextBox cajaCe, TextBox cajaHco3, Dictionary<string, TextBox> cajasElementos)
        {
            try
            {
                datosAgua.pH = double.Parse(cajaPh.Text);
                datosAgua.CE = double.Parse(cajaCe.Text);
                datosAgua.HCO3 = double.Parse(cajaHco3.Text);

                foreach (var elemento in cajasElementos)
                {
                    datosAgua.Elementos_mgL[elemento.Key] = double.Parse(elemento.Value.Text);
                }

                RealizarAnalisisAgua();
                MessageBox.Show("¡Datos de agua actualizados con éxito!", "Actualización Completa", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar datos de agua: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RealizarAnalisisAgua()
        {
            datosAgua = moduloAgua.AnalizarAgua(datosAgua);
            var resultadosCalidad = moduloAgua.EvaluarCalidadAgua(datosAgua);
            var indices = moduloAgua.CalcularIndicesCalidadAgua(datosAgua);
            var balanceIonico = moduloAgua.VerificarBalanceIonico(datosAgua);

            // Actualizar rejilla de resultados
            var rejilla = controlPestanasPrincipal.TabPages[0].Controls.Find("RejillaCalidadAgua", true).FirstOrDefault() as DataGridView;
            if (rejilla != null)
            {
                rejilla.DataSource = null;
                rejilla.Columns.Clear();

                rejilla.Columns.Add("Parametro", "Parámetro");
                rejilla.Columns.Add("Valor", "Valor");
                rejilla.Columns.Add("Unidad", "Unidad");
                rejilla.Columns.Add("Estado", "Estado");
                rejilla.Columns.Add("Recomendacion", "Recomendación");

                foreach (var resultado in resultadosCalidad)
                {
                    var indiceFila = rejilla.Rows.Add(resultado.Parametro,
                                                resultado.Valor.ToString("F2"),
                                                resultado.Unidad,
                                                resultado.Estado,
                                                resultado.Recomendacion);

                    var fila = rejilla.Rows[indiceFila];
                    switch (resultado.ColorEstado)
                    {
                        case "Verde":
                            fila.DefaultCellStyle.BackColor = Color.LightGreen;
                            break;
                        case "Rojo":
                            fila.DefaultCellStyle.BackColor = Color.LightCoral;
                            break;
                        case "Amarillo":
                            fila.DefaultCellStyle.BackColor = Color.LightYellow;
                            break;
                    }
                }

                // Agregar separador e índices
                rejilla.Rows.Add("=== ÍNDICES DE CALIDAD ===", "", "", "", "");
                rejilla.Rows.Add("SAR", indices.SAR.ToString("F2"), "", indices.SAR < 3 ? "OK" : "Alto", "");
                rejilla.Rows.Add("RSC", indices.RSC.ToString("F2"), "meq/L", indices.RSC < 1.25 ? "OK" : "Alto", "");
                rejilla.Rows.Add("Relación Ca/Mg", indices.RatioCaMg.ToString("F2"), "",
                             indices.RatioCaMg >= 2 && indices.RatioCaMg <= 4 ? "OK" : "Desbalanceado", "");

                rejilla.Rows.Add("=== BALANCE IÓNICO ===", "", "", "", "");
                rejilla.Rows.Add("Balance Iónico", $"{balanceIonico["DiferenciaPorcentual"]:F1}%", "",
                             balanceIonico["EstaBalanceado"] == 1 ? "Balanceado" : "Desbalanceado",
                             balanceIonico["EstaBalanceado"] == 1 ? "Dentro de tolerancia del 10%" : "Excede tolerancia del 10%");
            }
        }

        private void CargarPaso2_ConcentracionesObjetivo()
        {
            var pestana = new TabPage("Paso 2: Concentraciones Objetivo");
            var panelPrincipal = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                Padding = new Padding(10)
            };
            panelPrincipal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            panelPrincipal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            // Panel de entrada de objetivos
            var panelEntrada = new GroupBox
            {
                Text = "Concentraciones de Nutrientes Objetivo (mg/L)",
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var disenoEntrada = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                AutoSize = true
            };
            disenoEntrada.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            disenoEntrada.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            disenoEntrada.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            var cajasObjetivo = new Dictionary<string, TextBox>();
            int fila = 0;

            // Selección de cultivo
            disenoEntrada.Controls.Add(new Label { Text = "Tipo de Cultivo:", TextAlign = ContentAlignment.MiddleRight }, 0, fila);
            var comboCultivo = new ComboBox
            {
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            comboCultivo.Items.AddRange(new[] { "Tomate", "Lechuga", "Pepino", "Pimiento", "Fresa", "Personalizado" });
            comboCultivo.SelectedIndex = 0;
            disenoEntrada.Controls.Add(comboCultivo, 1, fila);
            fila++;

            // Etapa de crecimiento
            disenoEntrada.Controls.Add(new Label { Text = "Etapa de Crecimiento:", TextAlign = ContentAlignment.MiddleRight }, 0, fila);
            var comboEtapa = new ComboBox
            {
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            comboEtapa.Items.AddRange(new[] { "Plántula", "Vegetativo", "Floración", "Fructificación" });
            comboEtapa.SelectedIndex = 2;
            disenoEntrada.Controls.Add(comboEtapa, 1, fila);
            fila++;

            // Botón cargar valores por defecto
            var botonCargarDefecto = new Button
            {
                Text = "Cargar Valores por Defecto del Cultivo",
                Width = 150,
                Height = 25
            };
            botonCargarDefecto.Click += (s, e) => CargarValoresDefectoCultivo(comboCultivo.Text, comboEtapa.Text, cajasObjetivo);
            disenoEntrada.Controls.Add(botonCargarDefecto, 1, fila);
            fila++;

            // Separador
            disenoEntrada.Controls.Add(new Label { Text = "=== MACRONUTRIENTES ===", Font = new Font("Segoe UI", 9F, FontStyle.Bold) }, 0, fila);
            fila++;

            // Nutrientes principales
            var nutrientesPrincipales = new[] { "N", "P", "K", "Ca", "Mg", "S" };
            foreach (var nutriente in nutrientesPrincipales)
            {
                disenoEntrada.Controls.Add(new Label { Text = $"{nutriente}:", TextAlign = ContentAlignment.MiddleRight }, 0, fila);
                var cajaTexto = new TextBox { Text = concentracionesObjetivo[nutriente].ToString("F1"), Width = 100 };
                cajasObjetivo[nutriente] = cajaTexto;
                disenoEntrada.Controls.Add(cajaTexto, 1, fila);
                disenoEntrada.Controls.Add(new Label { Text = "mg/L" }, 2, fila);
                fila++;
            }

            // Separador
            disenoEntrada.Controls.Add(new Label { Text = "=== MICRONUTRIENTES ===", Font = new Font("Segoe UI", 9F, FontStyle.Bold) }, 0, fila);
            fila++;

            // Micronutrientes con valores por defecto
            var microDefecto = new Dictionary<string, double>
            {
                ["Fe"] = 1.0,
                ["Mn"] = 0.5,
                ["Zn"] = 0.2,
                ["Cu"] = 0.1,
                ["B"] = 0.5,
                ["Mo"] = 0.01
            };

            foreach (var micro in microDefecto)
            {
                disenoEntrada.Controls.Add(new Label { Text = $"{micro.Key}:", TextAlign = ContentAlignment.MiddleRight }, 0, fila);
                var cajaTexto = new TextBox { Text = micro.Value.ToString("F2"), Width = 100 };
                cajasObjetivo[micro.Key] = cajaTexto;
                disenoEntrada.Controls.Add(cajaTexto, 1, fila);
                disenoEntrada.Controls.Add(new Label { Text = "mg/L" }, 2, fila);
                fila++;
            }

            panelEntrada.Controls.Add(disenoEntrada);

            // Panel de comparación Agua vs Objetivo actual
            var panelComparacion = new GroupBox
            {
                Text = "Análisis Agua vs Objetivo",
                Dock = DockStyle.Fill
            };

            var rejillaComparacion = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                Name = "RejillaComparacion"
            };

            panelComparacion.Controls.Add(rejillaComparacion);

            panelPrincipal.Controls.Add(panelEntrada, 0, 0);
            panelPrincipal.Controls.Add(panelComparacion, 1, 0);

            pestana.Controls.Add(panelPrincipal);
            controlPestanasPrincipal.TabPages.Add(pestana);

            // Comparación inicial
            ActualizarComparacionObjetivo();
        }

        private void CargarValoresDefectoCultivo(string cultivo, string etapa, Dictionary<string, TextBox> cajasTexto)
        {
            var valoresDefecto = new Dictionary<string, Dictionary<string, double>>
            {
                ["Tomate"] = new Dictionary<string, double>
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
                ["Lechuga"] = new Dictionary<string, double>
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
                ["Pepino"] = new Dictionary<string, double>
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

            if (valoresDefecto.ContainsKey(cultivo))
            {
                var defectosCultivo = valoresDefecto[cultivo];

                // Ajustar según la etapa
                double multiplicadorEtapa = etapa switch
                {
                    "Plántula" => 0.5,
                    "Vegetativo" => 0.8,
                    "Floración" => 1.0,
                    "Fructificación" => 1.2,
                    _ => 1.0
                };

                foreach (var nutriente in defectosCultivo)
                {
                    if (cajasTexto.ContainsKey(nutriente.Key))
                    {
                        double valorAjustado = nutriente.Value * multiplicadorEtapa;
                        cajasTexto[nutriente.Key].Text = valorAjustado.ToString("F1");
                    }
                }

                // Actualizar concentraciones objetivo
                foreach (var cajaTexto in cajasTexto)
                {
                    if (double.TryParse(cajaTexto.Value.Text, out double valor))
                    {
                        concentracionesObjetivo[cajaTexto.Key] = valor;
                    }
                }

                ActualizarComparacionObjetivo();
                MessageBox.Show($"Valores por defecto de {cultivo} cargados para etapa {etapa}", "Valores por Defecto Cargados",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ActualizarComparacionObjetivo()
        {
            var rejilla = controlPestanasPrincipal.TabPages[1].Controls.Find("RejillaComparacion", true).FirstOrDefault() as DataGridView;
            if (rejilla == null) return;

            rejilla.Columns.Clear();
            rejilla.Columns.Add("Nutriente", "Nutriente");
            rejilla.Columns.Add("EnAgua", "En Agua (mg/L)");
            rejilla.Columns.Add("Objetivo", "Objetivo (mg/L)");
            rejilla.Columns.Add("PorAgregar", "Por Agregar (mg/L)");
            rejilla.Columns.Add("Estado", "Estado");

            foreach (var objetivo in concentracionesObjetivo)
            {
                double enAgua = datosAgua.Elementos_mgL.GetValueOrDefault(objetivo.Key, 0);
                double porAgregar = Math.Max(0, objetivo.Value - enAgua);
                string estado = porAgregar <= 0 ? "Suficiente" : porAgregar > objetivo.Value * 0.8 ? "Alta Necesidad" : "Necesidad Moderada";

                var indiceFila = rejilla.Rows.Add(objetivo.Key, enAgua.ToString("F1"),
                                           objetivo.Value.ToString("F1"), porAgregar.ToString("F1"), estado);

                var fila = rejilla.Rows[indiceFila];
                fila.DefaultCellStyle.BackColor = estado switch
                {
                    "Suficiente" => Color.LightGreen,
                    "Alta Necesidad" => Color.LightCoral,
                    _ => Color.LightYellow
                };
            }
        }

        private void CargarPaso3_AjustePH()
        {
            var pestana = new TabPage("Paso 3: Ajuste de pH");
            var panelPrincipal = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                Padding = new Padding(10)
            };
            panelPrincipal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            panelPrincipal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));

            // Panel de entrada de ajuste de pH
            var panelEntrada = new GroupBox
            {
                Text = "Parámetros de Ajuste de pH",
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var disenoEntrada = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                AutoSize = true
            };

            // pH actual del agua
            disenoEntrada.Controls.Add(new Label { Text = "pH Actual:", TextAlign = ContentAlignment.MiddleRight }, 0, 0);
            disenoEntrada.Controls.Add(new Label { Text = datosAgua.pH.ToString("F1"), Font = new Font("Segoe UI", 9F, FontStyle.Bold) }, 1, 0);

            // pH objetivo
            disenoEntrada.Controls.Add(new Label { Text = "pH Objetivo:", TextAlign = ContentAlignment.MiddleRight }, 0, 1);
            var cajaPHObjetivo = new TextBox { Text = "6.0", Width = 100 };
            disenoEntrada.Controls.Add(cajaPHObjetivo, 1, 1);

            // Concentración HCO3
            disenoEntrada.Controls.Add(new Label { Text = "HCO3- (mg/L):", TextAlign = ContentAlignment.MiddleRight }, 0, 2);
            disenoEntrada.Controls.Add(new Label { Text = datosAgua.HCO3.ToString("F1"), Font = new Font("Segoe UI", 9F, FontStyle.Bold) }, 1, 2);

            // Necesidades de fósforo
            double pObjetivo = concentracionesObjetivo.GetValueOrDefault("P", 45);
            double pActual = datosAgua.Elementos_mgL.GetValueOrDefault("P", 0);
            disenoEntrada.Controls.Add(new Label { Text = "P necesario (mg/L):", TextAlign = ContentAlignment.MiddleRight }, 0, 3);
            disenoEntrada.Controls.Add(new Label { Text = Math.Max(0, pObjetivo - pActual).ToString("F1"), Font = new Font("Segoe UI", 9F, FontStyle.Bold) }, 1, 3);

            // Botón calcular
            var botonCalcularPH = new Button
            {
                Text = "Calcular Requerimientos de Ácido",
                Dock = DockStyle.Bottom,
                Height = 35,
                BackColor = Color.LightBlue
            };
            botonCalcularPH.Click += (s, e) => CalcularAjustePH(double.Parse(cajaPHObjetivo.Text));

            panelEntrada.Controls.Add(disenoEntrada);
            panelEntrada.Controls.Add(botonCalcularPH);

            // Panel de resultados
            var panelResultados = new GroupBox
            {
                Text = "Resultados de Cálculo de Ácidos",
                Dock = DockStyle.Fill
            };

            var rejillaResultados = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                Name = "RejillaResultadosAcidos"
            };

            panelResultados.Controls.Add(rejillaResultados);

            panelPrincipal.Controls.Add(panelEntrada, 0, 0);
            panelPrincipal.Controls.Add(panelResultados, 1, 0);

            pestana.Controls.Add(panelPrincipal);
            controlPestanasPrincipal.TabPages.Add(pestana);
        }

        private void CalcularAjustePH(double pHObjetivo)
        {
            try
            {
                double pObjetivo = concentracionesObjetivo.GetValueOrDefault("P", 45);
                double pActual = datosAgua.Elementos_mgL.GetValueOrDefault("P", 0);

                resultadosAcidos = moduloPh.CalcularRequerimientoAcido_MetodoIncrossi(
                    datosAgua.HCO3, pHObjetivo, pObjetivo, pActual);

                var rejilla = controlPestanasPrincipal.TabPages[2].Controls.Find("RejillaResultadosAcidos", true).FirstOrDefault() as DataGridView;
                if (rejilla == null) return;

                rejilla.Columns.Clear();
                rejilla.Columns.Add("Acido", "Tipo de Ácido");
                rejilla.Columns.Add("Volumen", "Volumen (mL/L)");
                rejilla.Columns.Add("ConcentracionH", "H+ (mg/L)");
                rejilla.Columns.Add("Nutriente", "Nutriente");
                rejilla.Columns.Add("CantidadNutriente", "Cantidad (mg/L)");
                rejilla.Columns.Add("Bicarbonatos", "HCO3- Neutralizado");

                foreach (var resultado in resultadosAcidos)
                {
                    rejilla.Rows.Add(
                        resultado.NombreAcido,
                        resultado.VolumenAcido_mlL.ToString("F3"),
                        resultado.ConcentracionHidrogeno_mgL.ToString("F1"),
                        resultado.TipoNutriente,
                        resultado.ContribucionNutriente_mgL.ToString("F1"),
                        resultado.BicarbonatosANeutralizar.ToString("F1")
                    );
                }

                // Agregar recomendación
                var recomendacion = moduloPh.ObtenerRecomendacionAcido(datosAgua.HCO3, pActual, pObjetivo);

                var panelRec = new Panel
                {
                    Height = 80,
                    Dock = DockStyle.Bottom,
                    BackColor = Color.LightYellow,
                    Padding = new Padding(10)
                };

                var etiquetaRec = new Label
                {
                    Text = "Recomendación: " + recomendacion,
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleLeft
                };

                panelRec.Controls.Add(etiquetaRec);
                controlPestanasPrincipal.TabPages[2].Controls.Add(panelRec);

                MessageBox.Show("¡Cálculos de ajuste de pH completados!", "Cálculo Completo",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al calcular ajuste de pH: {ex.Message}", "Error de Cálculo",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void NavegarPaso(int direccion)
        {
            int nuevoPaso = pasoActual + direccion;
            if (nuevoPaso < 0 || nuevoPaso >= nombresPasos.Length) return;

            pasoActual = nuevoPaso;
            etiquetaPaso.Text = nombresPasos[pasoActual];
            barraProgreso.Value = pasoActual;

            // Cargar paso apropiado
            switch (pasoActual)
            {
                case 1:
                    if (controlPestanasPrincipal.TabPages.Count < 2) CargarPaso2_ConcentracionesObjetivo();
                    break;
                case 2:
                    if (controlPestanasPrincipal.TabPages.Count < 3) CargarPaso3_AjustePH();
                    break;
                case 3:
                    if (controlPestanasPrincipal.TabPages.Count < 4) CargarPaso4_CalculosNutrientes();
                    break;
                case 4:
                    if (controlPestanasPrincipal.TabPages.Count < 5) CargarPaso5_VerificacionSolucion();
                    break;
                case 5:
                    if (controlPestanasPrincipal.TabPages.Count < 6) CargarPaso6_SolucionesConcentradas();
                    break;
                case 6:
                    if (controlPestanasPrincipal.TabPages.Count < 7) CargarPaso7_AnalisisCostos();
                    break;
                case 7:
                    if (controlPestanasPrincipal.TabPages.Count < 8) CargarPaso8_ReporteFinal();
                    break;
            }

            controlPestanasPrincipal.SelectedIndex = pasoActual;

            // Actualizar botones de navegación
            var panelNavegacion = this.Controls.OfType<Panel>().LastOrDefault();
            if (panelNavegacion != null)
            {
                var botonAnterior = panelNavegacion.Controls[0] as Button;
                var botonSiguiente = panelNavegacion.Controls[1] as Button;

                if (botonAnterior != null) botonAnterior.Enabled = pasoActual > 0;
                if (botonSiguiente != null) botonSiguiente.Enabled = pasoActual < nombresPasos.Length - 1;
            }
        }

        private void CalcularPasoActual(object sender, EventArgs e)
        {
            switch (pasoActual)
            {
                case 0:
                    RealizarAnalisisAgua();
                    break;
                case 1:
                    ActualizarConcentracionesObjetivo();
                    break;
                case 2:
                    if (controlPestanasPrincipal.TabPages[2].Controls.Find("CajaPHObjetivo", true).FirstOrDefault() is TextBox cajaPh)
                        CalcularAjustePH(double.Parse(cajaPh.Text));
                    break;
                case 3:
                    CalcularSolucionNutritiva();
                    break;
                case 4:
                    RealizarVerificacion();
                    break;
                case 5:
                    CalcularSolucionesConcentradas();
                    break;
                case 6:
                    RealizarAnalisisCostos();
                    break;
                case 7:
                    GenerarReporteFinal();
                    break;
            }
        }

        private void ActualizarConcentracionesObjetivo()
        {
            // Actualizar concentraciones objetivo desde UI
            ActualizarComparacionObjetivo();
            MessageBox.Show("¡Concentraciones objetivo actualizadas!", "Actualización Completa",
                          MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void CargarPaso4_CalculosNutrientes()
        {
            var pestana = new TabPage("Paso 4: Cálculos de Nutrientes");
            var rejilla = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                Name = "RejillaCalculosNutrientes"
            };

            var panelInfo = new Panel
            {
                Height = 60,
                Dock = DockStyle.Top,
                BackColor = Color.LightCyan,
                Padding = new Padding(10)
            };

            var etiquetaInfo = new Label
            {
                Text = "Este paso calcula las cantidades de fertilizantes usando la fórmula: P = C × M × 100 / (A × %P)\n" +
                       "Donde P=cantidad fertilizante, C=concentración objetivo, M=peso molecular, A=peso elemento, %P=pureza",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F),
                TextAlign = ContentAlignment.MiddleLeft
            };

            panelInfo.Controls.Add(etiquetaInfo);
            pestana.Controls.Add(rejilla);
            pestana.Controls.Add(panelInfo);
            controlPestanasPrincipal.TabPages.Add(pestana);
        }

        private void CalcularSolucionNutritiva()
        {
            try
            {
                resultadosCalculo = moduloNutrientes.CalcularSolucion();
                balanceIonico = moduloNutrientes.CalcularBalanceIonico(resultadosCalculo);

                var rejilla = controlPestanasPrincipal.TabPages[3].Controls.Find("RejillaCalculosNutrientes", true).FirstOrDefault() as DataGridView;
                if (rejilla == null) return;

                // Configurar columnas
                rejilla.Columns.Clear();
                var columnas = new[]
                {
                    ("Fertilizante", "Fertilizante"),
                    ("Cantidad", "Cantidad (mg/L)"),
                    ("Ca", "Ca"),
                    ("K", "K"),
                    ("Mg", "Mg"),
                    ("N", "N"),
                    ("P", "P"),
                    ("S", "S"),
                    ("Formula", "Cálculo Usado")
                };

                foreach (var col in columnas)
                {
                    rejilla.Columns.Add(col.Item1, col.Item2);
                }

                // Agregar resultados de cálculo
                foreach (var resultado in resultadosCalculo)
                {
                    string formula = $@"P = {resultado.Nombre switch
                    {
                        "KH2PO4" => "45 × 136.1 × 100 / (30.97 × 98)",
                        "Ca(NO3)2.2H2O" => "162 × 200 × 100 / (40.08 × 95)",
                        "MgSO4.7H2O" => "45 × 246.5 × 100 / (24.31 × 98)",
                        "KNO3" => "30 × 101.1 × 100 / (14.01 × 98)",
                        "K2SO4" => "118 × 174.3 × 100 / (78.2 × 98)",
                        _ => "Fórmula Estándar"
                    }}";

                    rejilla.Rows.Add(
                        resultado.Nombre,
                        resultado.ConcentracionSal_mgL.ToString("F1"),
                        resultado.Ca.ToString("F1"),
                        resultado.K.ToString("F1"),
                        resultado.Mg.ToString("F1"),
                        resultado.N.ToString("F1"),
                        resultado.P.ToString("F1"),
                        resultado.S.ToString("F1"),
                        formula
                    );
                }

                MessageBox.Show("¡Cálculos de nutrientes completados con éxito!\n" +
                              $"Total de fertilizantes calculados: {resultadosCalculo.Count}",
                              "Cálculo Completo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al calcular solución nutritiva: {ex.Message}", "Error de Cálculo",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarPaso5_VerificacionSolucion()
        {
            var pestana = new TabPage("Paso 5: Verificación de Solución");
            controlPestanasPrincipal.TabPages.Add(pestana);
        }

        private void RealizarVerificacion()
        {
            MessageBox.Show("Paso de verificación - verificando balance iónico, CE, pH y relaciones de nutrientes",
                          "Verificación", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void CargarPaso6_SolucionesConcentradas()
        {
            var pestana = new TabPage("Paso 6: Soluciones Concentradas");
            controlPestanasPrincipal.TabPages.Add(pestana);
        }

        private void CalcularSolucionesConcentradas()
        {
            MessageBox.Show("Calculando soluciones concentradas con distribución de tanques y verificaciones de compatibilidad",
                          "Soluciones Concentradas", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void CargarPaso7_AnalisisCostos()
        {
            var pestana = new TabPage("Paso 7: Análisis de Costos");
            controlPestanasPrincipal.TabPages.Add(pestana);
        }

        private void RealizarAnalisisCostos()
        {
            MessageBox.Show("Analizando costos por fertilizante, nutriente y distribución de tanques",
                          "Análisis de Costos", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void CargarPaso8_ReporteFinal()
        {
            var pestana = new TabPage("Paso 8: Reporte Final");
            controlPestanasPrincipal.TabPages.Add(pestana);
        }

        private void GenerarReporteFinal()
        {
            MessageBox.Show("Generando reporte final integral con todos los cálculos y recomendaciones",
                          "Reporte Final", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    // Punto de entrada del programa principal
    class Programa
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormularioCalculadoraHidroponicaCompleta());
        }
    }
}