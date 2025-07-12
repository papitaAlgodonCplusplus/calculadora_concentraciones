using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
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

        // Agregar estos métodos a la clase FormularioCalculadoraHidroponicaCompleta

        private void CargarPaso5_VerificacionSolucion()
        {
            var pestana = new TabPage("Paso 5: Verificación de Solución");
            var panelPrincipal = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(10)
            };
            panelPrincipal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            panelPrincipal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            panelPrincipal.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            panelPrincipal.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            // Panel de verificación de concentraciones
            var panelConcentraciones = new GroupBox
            {
                Text = "Verificación de Concentraciones",
                Dock = DockStyle.Fill
            };
            var rejillaConcentraciones = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                Name = "RejillaVerificacionConcentraciones"
            };
            panelConcentraciones.Controls.Add(rejillaConcentraciones);

            // Panel de parámetros físicos
            var panelFisicos = new GroupBox
            {
                Text = "Parámetros Físicos",
                Dock = DockStyle.Fill
            };
            var rejillaFisicos = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                Name = "RejillaParametrosFisicos"
            };
            panelFisicos.Controls.Add(rejillaFisicos);

            // Panel de relaciones iónicas
            var panelRelaciones = new GroupBox
            {
                Text = "Relaciones Iónicas",
                Dock = DockStyle.Fill
            };
            var rejillaRelaciones = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                Name = "RejillaRelacionesIonicas"
            };
            panelRelaciones.Controls.Add(rejillaRelaciones);

            // Panel de balance iónico
            var panelBalance = new GroupBox
            {
                Text = "Balance Iónico",
                Dock = DockStyle.Fill
            };
            var rejillaBalance = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                Name = "RejillaBalanceIonico"
            };
            panelBalance.Controls.Add(rejillaBalance);

            panelPrincipal.Controls.Add(panelConcentraciones, 0, 0);
            panelPrincipal.Controls.Add(panelFisicos, 1, 0);
            panelPrincipal.Controls.Add(panelRelaciones, 0, 1);
            panelPrincipal.Controls.Add(panelBalance, 1, 1);

            pestana.Controls.Add(panelPrincipal);
            controlPestanasPrincipal.TabPages.Add(pestana);
        }

        private void RealizarVerificacion()
        {
            try
            {
                if (resultadosCalculo == null || balanceIonico == null)
                {
                    MessageBox.Show("Primero debe completar los cálculos de nutrientes.", "Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Calcular concentraciones finales
                var concentracionesFinales = new Dictionary<string, double>();
                foreach (var resultado in resultadosCalculo)
                {
                    if (!concentracionesFinales.ContainsKey("N")) concentracionesFinales["N"] = 0;
                    if (!concentracionesFinales.ContainsKey("P")) concentracionesFinales["P"] = 0;
                    if (!concentracionesFinales.ContainsKey("K")) concentracionesFinales["K"] = 0;
                    if (!concentracionesFinales.ContainsKey("Ca")) concentracionesFinales["Ca"] = 0;
                    if (!concentracionesFinales.ContainsKey("Mg")) concentracionesFinales["Mg"] = 0;
                    if (!concentracionesFinales.ContainsKey("S")) concentracionesFinales["S"] = 0;
                    if (!concentracionesFinales.ContainsKey("Fe")) concentracionesFinales["Fe"] = 0;

                    concentracionesFinales["N"] += resultado.N;
                    concentracionesFinales["P"] += resultado.P;
                    concentracionesFinales["K"] += resultado.K;
                    concentracionesFinales["Ca"] += resultado.Ca;
                    concentracionesFinales["Mg"] += resultado.Mg;
                    concentracionesFinales["S"] += resultado.S;
                    concentracionesFinales["Fe"] += resultado.Fe;
                }

                // Sumar aportes del agua
                foreach (var elemento in datosAgua.Elementos_mgL)
                {
                    if (concentracionesFinales.ContainsKey(elemento.Key))
                        concentracionesFinales[elemento.Key] += elemento.Value;
                }

                // Verificar concentraciones
                var resultadosConcentraciones = moduloVerificacion.VerificarConcentracionesNutrientes(
                    concentracionesObjetivo, concentracionesFinales);

                // Calcular CE estimada
                double ceEstimada = balanceIonico.Final_meqL.Values.Sum() * 0.1;

                // Verificar parámetros físicos
                var resultadosFisicos = moduloVerificacion.VerificarParametrosFisicos(
                    datosAgua.pH, ceEstimada, datosAgua.Temperatura);

                // Verificar relaciones iónicas
                var resultadosRelaciones = moduloVerificacion.VerificarRelacionesIonicas(
                    balanceIonico.Final_meqL, balanceIonico.Final_mmolL, balanceIonico.Final_mgL, "General");

                // Verificar balance iónico
                var verificacionBalance = moduloVerificacion.VerificarBalanceIonico(balanceIonico.Final_meqL);

                // Mostrar resultados en las rejillas
                MostrarResultadosVerificacion(resultadosConcentraciones, resultadosFisicos,
                                            resultadosRelaciones, verificacionBalance);

                MessageBox.Show("¡Verificación de solución completada con éxito!", "Verificación Completa",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error durante la verificación: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MostrarResultadosVerificacion(
            List<ResultadoVerificacion> concentraciones,
            List<ResultadoVerificacion> fisicos,
            List<ResultadoRelacionIonica> relaciones,
            Dictionary<string, double> balance)
        {
            // Rejilla de concentraciones
            var rejillaConc = controlPestanasPrincipal.TabPages[4].Controls.Find("RejillaVerificacionConcentraciones", true)
                              .FirstOrDefault() as DataGridView;
            if (rejillaConc != null)
            {
                rejillaConc.Columns.Clear();
                rejillaConc.Columns.Add("Nutriente", "Nutriente");
                rejillaConc.Columns.Add("Objetivo", "Objetivo (mg/L)");
                rejillaConc.Columns.Add("Real", "Real (mg/L)");
                rejillaConc.Columns.Add("Desviacion", "Desviación (%)");
                rejillaConc.Columns.Add("Estado", "Estado");

                foreach (var resultado in concentraciones)
                {
                    var indice = rejillaConc.Rows.Add(
                        resultado.Parametro,
                        resultado.ValorObjetivo.ToString("F1"),
                        resultado.ValorReal.ToString("F1"),
                        resultado.PorcentajeDesviacion.ToString("F1") + "%",
                        resultado.Estado
                    );

                    var fila = rejillaConc.Rows[indice];
                    fila.DefaultCellStyle.BackColor = resultado.ColorEstado switch
                    {
                        "Verde" => Color.LightGreen,
                        "Rojo" => Color.LightCoral,
                        "Amarillo" => Color.LightYellow,
                        "Naranja" => Color.Orange,
                        _ => Color.White
                    };
                }
            }

            // Rejilla de parámetros físicos
            var rejillaFis = controlPestanasPrincipal.TabPages[4].Controls.Find("RejillaParametrosFisicos", true)
                             .FirstOrDefault() as DataGridView;
            if (rejillaFis != null)
            {
                rejillaFis.Columns.Clear();
                rejillaFis.Columns.Add("Parametro", "Parámetro");
                rejillaFis.Columns.Add("Valor", "Valor");
                rejillaFis.Columns.Add("Unidad", "Unidad");
                rejillaFis.Columns.Add("Estado", "Estado");
                rejillaFis.Columns.Add("Recomendacion", "Recomendación");

                foreach (var resultado in fisicos)
                {
                    var indice = rejillaFis.Rows.Add(
                        resultado.Parametro,
                        resultado.ValorReal.ToString("F2"),
                        resultado.Unidad,
                        resultado.Estado,
                        resultado.Recomendacion
                    );

                    var fila = rejillaFis.Rows[indice];
                    fila.DefaultCellStyle.BackColor = resultado.ColorEstado switch
                    {
                        "Verde" => Color.LightGreen,
                        "Rojo" => Color.LightCoral,
                        _ => Color.White
                    };
                }
            }

            // Rejilla de relaciones
            var rejillaRel = controlPestanasPrincipal.TabPages[4].Controls.Find("RejillaRelacionesIonicas", true)
                             .FirstOrDefault() as DataGridView;
            if (rejillaRel != null)
            {
                rejillaRel.Columns.Clear();
                rejillaRel.Columns.Add("Relacion", "Relación");
                rejillaRel.Columns.Add("Valor", "Valor");
                rejillaRel.Columns.Add("Rango", "Rango Óptimo");
                rejillaRel.Columns.Add("Estado", "Estado");

                foreach (var relacion in relaciones)
                {
                    var indice = rejillaRel.Rows.Add(
                        relacion.NombreRelacion,
                        relacion.RelacionReal.ToString("F2"),
                        $"{relacion.ObjetivoMinimo:F1} - {relacion.ObjetivoMaximo:F1}",
                        relacion.Estado
                    );

                    var fila = rejillaRel.Rows[indice];
                    fila.DefaultCellStyle.BackColor = relacion.ColorEstado switch
                    {
                        "Verde" => Color.LightGreen,
                        "Naranja" => Color.Orange,
                        _ => Color.White
                    };
                }
            }

            // Rejilla de balance iónico
            var rejillaBal = controlPestanasPrincipal.TabPages[4].Controls.Find("RejillaBalanceIonico", true)
                             .FirstOrDefault() as DataGridView;
            if (rejillaBal != null)
            {
                rejillaBal.Columns.Clear();
                rejillaBal.Columns.Add("Parametro", "Parámetro");
                rejillaBal.Columns.Add("Valor", "Valor");
                rejillaBal.Columns.Add("Unidad", "Unidad");
                rejillaBal.Columns.Add("Estado", "Estado");

                rejillaBal.Rows.Add("Suma Cationes", balance["SumaCationes"].ToString("F2"), "meq/L", "");
                rejillaBal.Rows.Add("Suma Aniones", balance["SumaAniones"].ToString("F2"), "meq/L", "");
                rejillaBal.Rows.Add("Diferencia", balance["Diferencia"].ToString("F2"), "meq/L", "");

                var indiceDif = rejillaBal.Rows.Add("Diferencia %", balance["DiferenciaPorcentual"].ToString("F1") + "%", "",
                                      balance["EstaBalanceado"] == 1 ? "Balanceado" : "Desbalanceado");

                var filaDif = rejillaBal.Rows[indiceDif];
                filaDif.DefaultCellStyle.BackColor = balance["EstaBalanceado"] == 1 ? Color.LightGreen : Color.LightCoral;
            }
        }

        private void CargarPaso6_SolucionesConcentradas()
        {
            var pestana = new TabPage("Paso 6: Soluciones Concentradas");
            var panelPrincipal = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                Padding = new Padding(10)
            };
            panelPrincipal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            panelPrincipal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));

            // Panel de configuración
            var panelConfig = new GroupBox
            {
                Text = "Configuración de Concentración",
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var disenoConfig = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                AutoSize = true
            };

            // Factor de concentración
            disenoConfig.Controls.Add(new Label { Text = "Factor de Concentración:", TextAlign = ContentAlignment.MiddleRight }, 0, 0);
            var comboFactor = new ComboBox
            {
                Width = 100,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Name = "ComboFactorConcentracion"
            };
            comboFactor.Items.AddRange(new[] { "1:50", "1:100", "1:200" });
            comboFactor.SelectedIndex = 1; // 1:100 por defecto
            disenoConfig.Controls.Add(comboFactor, 1, 0);

            // Número de tanques
            disenoConfig.Controls.Add(new Label { Text = "Número de Tanques:", TextAlign = ContentAlignment.MiddleRight }, 0, 1);
            var comboTanques = new ComboBox
            {
                Width = 100,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Name = "ComboNumeroTanques"
            };
            comboTanques.Items.AddRange(new[] { "2", "3", "4" });
            comboTanques.SelectedIndex = 0; // 2 tanques por defecto
            disenoConfig.Controls.Add(comboTanques, 1, 1);

            // Volumen por tanque
            disenoConfig.Controls.Add(new Label { Text = "Volumen por Tanque (L):", TextAlign = ContentAlignment.MiddleRight }, 0, 2);
            var cajaVolumen = new TextBox { Text = "200", Width = 100, Name = "CajaVolumenTanque" };
            disenoConfig.Controls.Add(cajaVolumen, 1, 2);

            panelConfig.Controls.Add(disenoConfig);

            // Panel de resultados
            var panelResultados = new GroupBox
            {
                Text = "Distribución de Tanques",
                Dock = DockStyle.Fill
            };

            var rejillaConcentrados = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                Name = "RejillaSolucionesConcentradas"
            };

            panelResultados.Controls.Add(rejillaConcentrados);

            panelPrincipal.Controls.Add(panelConfig, 0, 0);
            panelPrincipal.Controls.Add(panelResultados, 1, 0);

            pestana.Controls.Add(panelPrincipal);
            controlPestanasPrincipal.TabPages.Add(pestana);
        }

        private void CalcularSolucionesConcentradas()
        {
            try
            {
                if (resultadosCalculo == null)
                {
                    MessageBox.Show("Primero debe completar los cálculos de nutrientes.", "Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Obtener configuración
                var comboFactor = controlPestanasPrincipal.TabPages[5].Controls.Find("ComboFactorConcentracion", true)
                                  .FirstOrDefault() as ComboBox;
                var comboTanques = controlPestanasPrincipal.TabPages[5].Controls.Find("ComboNumeroTanques", true)
                                   .FirstOrDefault() as ComboBox;
                var cajaVolumen = controlPestanasPrincipal.TabPages[5].Controls.Find("CajaVolumenTanque", true)
                                  .FirstOrDefault() as TextBox;

                if (comboFactor == null || comboTanques == null || cajaVolumen == null) return;

                int factor = int.Parse(comboFactor.Text.Split(':')[1]);
                int numeroTanques = int.Parse(comboTanques.Text);
                double volumenTanque = double.Parse(cajaVolumen.Text);

                // Convertir resultados a concentraciones de fertilizantes
                var concentracionesFertilizantes = new Dictionary<string, double>();
                foreach (var resultado in resultadosCalculo)
                {
                    concentracionesFertilizantes[resultado.Nombre] = resultado.ConcentracionSal_mgL;
                }

                // Obtener ácidos si existen
                var acidos = new List<string>();
                if (resultadosAcidos != null)
                {
                    acidos.AddRange(resultadosAcidos.Select(a => a.NombreAcido));
                }

                // Generar distribución
                var tanques = moduloConcentrados.DistribuirFertilizantes(
                    concentracionesFertilizantes, acidos, numeroTanques, factor, volumenTanque);

                // Mostrar resultados
                MostrarSolucionesConcentradas(tanques, factor);

                MessageBox.Show("¡Soluciones concentradas calculadas con éxito!", "Cálculo Completo",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al calcular soluciones concentradas: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MostrarSolucionesConcentradas(List<DistribucionTanque> tanques, int factor)
        {
            var rejilla = controlPestanasPrincipal.TabPages[5].Controls.Find("RejillaSolucionesConcentradas", true)
                          .FirstOrDefault() as DataGridView;
            if (rejilla == null) return;

            rejilla.Columns.Clear();
            rejilla.Columns.Add("Tanque", "Tanque");
            rejilla.Columns.Add("Fertilizantes", "Fertilizantes");
            rejilla.Columns.Add("Concentracion", "Concentración Total (g/L)");
            rejilla.Columns.Add("Costo", "Costo ($)");
            rejilla.Columns.Add("Advertencias", "Advertencias");

            foreach (var tanque in tanques)
            {
                string fertilizantes = string.Join(", ", tanque.Fertilizantes);
                string advertencias = "";

                if (tanque.AdvertenciasCompatibilidad.Any())
                    advertencias += "⚠️ Compatibilidad ";
                if (tanque.AdvertenciasSolubilidad.Any())
                    advertencias += "⚠️ Solubilidad ";

                var indice = rejilla.Rows.Add(
                    tanque.EtiquetaTanque,
                    fertilizantes,
                    tanque.DensidadTotal_gL.ToString("F1"),
                    tanque.CostoEstimado.ToString("F2"),
                    advertencias
                );

                var fila = rejilla.Rows[indice];
                if (advertencias.Contains("⚠️"))
                    fila.DefaultCellStyle.BackColor = Color.LightYellow;
            }
        }

        private void CargarPaso7_AnalisisCostos()
        {
            var pestana = new TabPage("Paso 7: Análisis de Costos");
            var panelPrincipal = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(10)
            };
            panelPrincipal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            panelPrincipal.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            panelPrincipal.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            panelPrincipal.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            // Panel de costos por fertilizante
            var panelFertilizantes = new GroupBox
            {
                Text = "Costos por Fertilizante",
                Dock = DockStyle.Fill
            };
            var rejillaFertilizantes = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                Name = "RejillaCostosFertilizantes"
            };
            panelFertilizantes.Controls.Add(rejillaFertilizantes);

            // Panel de costos por nutriente
            var panelNutrientes = new GroupBox
            {
                Text = "Costos por Nutriente",
                Dock = DockStyle.Fill
            };
            var rejillaNutrientes = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                Name = "RejillaCostosNutrientes"
            };
            panelNutrientes.Controls.Add(rejillaNutrientes);

            // Panel de resumen de costos
            var panelResumen = new GroupBox
            {
                Text = "Resumen de Costos",
                Dock = DockStyle.Fill
            };
            var rejillaResumen = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                Name = "RejillaResumenCostos"
            };
            panelResumen.Controls.Add(rejillaResumen);

            // Panel de configuración de área
            var panelConfigArea = new GroupBox
            {
                Text = "Configuración de Área",
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var disenoArea = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                AutoSize = true
            };

            disenoArea.Controls.Add(new Label { Text = "Área Total (m²):", TextAlign = ContentAlignment.MiddleRight }, 0, 0);
            var cajaArea = new TextBox { Text = "100", Width = 100, Name = "CajaAreaTotal" };
            disenoArea.Controls.Add(cajaArea, 1, 0);

            disenoArea.Controls.Add(new Label { Text = "Aplicaciones/Año:", TextAlign = ContentAlignment.MiddleRight }, 0, 1);
            var cajaAplicaciones = new TextBox { Text = "365", Width = 100, Name = "CajaAplicacionesAno" };
            disenoArea.Controls.Add(cajaAplicaciones, 1, 1);

            panelConfigArea.Controls.Add(disenoArea);

            panelPrincipal.Controls.Add(panelFertilizantes, 0, 0);
            panelPrincipal.Controls.Add(panelNutrientes, 1, 0);
            panelPrincipal.Controls.Add(panelResumen, 0, 1);
            panelPrincipal.Controls.Add(panelConfigArea, 1, 1);

            pestana.Controls.Add(panelPrincipal);
            controlPestanasPrincipal.TabPages.Add(pestana);
        }

        private void RealizarAnalisisCostos()
        {
            try
            {
                if (resultadosCalculo == null)
                {
                    MessageBox.Show("Primero debe completar los cálculos de nutrientes.", "Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Obtener configuración de área
                var cajaArea = controlPestanasPrincipal.TabPages[6].Controls.Find("CajaAreaTotal", true)
                               .FirstOrDefault() as TextBox;
                var cajaAplicaciones = controlPestanasPrincipal.TabPages[6].Controls.Find("CajaAplicacionesAno", true)
                                       .FirstOrDefault() as TextBox;

                double areaTotal = double.Parse(cajaArea?.Text ?? "100");
                double aplicacionesAno = double.Parse(cajaAplicaciones?.Text ?? "365");

                // Calcular cantidades de fertilizantes por 1000L
                var cantidadesFertilizantes = new Dictionary<string, double>();
                foreach (var resultado in resultadosCalculo)
                {
                    cantidadesFertilizantes[resultado.Nombre] = resultado.ConcentracionSal_mgL / 1000.0; // kg por 1000L
                }

                // Calcular análisis de costos
                var analisisCostos = moduloCostos.CalcularCostoSolucion(
                    cantidadesFertilizantes, 1000, 1000, 1); // 1000L concentrado = 1000L diluido (factor 1:1)

                // Calcular costos por nutriente
                var costosNutrientes = moduloCostos.AnalizarCostosNutrientes(concentracionesObjetivo, 1000);

                // Generar reporte completo
                var reporteCostos = moduloCostos.GenerarReporteCostos(
                    analisisCostos, costosNutrientes, areaTotal, aplicacionesAno);

                // Mostrar resultados
                MostrarAnalisisCostos(analisisCostos, costosNutrientes, reporteCostos);

                MessageBox.Show("¡Análisis de costos completado con éxito!", "Análisis Completo",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error durante el análisis de costos: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MostrarAnalisisCostos(
            AnalisisCostoSolucion analisisCostos,
            Dictionary<string, CostoNutriente> costosNutrientes,
            Dictionary<string, object> reporteCostos)
        {
            // Rejilla de costos por fertilizante
            var rejillaFert = controlPestanasPrincipal.TabPages[6].Controls.Find("RejillaCostosFertilizantes", true)
                              .FirstOrDefault() as DataGridView;
            if (rejillaFert != null)
            {
                rejillaFert.Columns.Clear();
                rejillaFert.Columns.Add("Fertilizante", "Fertilizante");
                rejillaFert.Columns.Add("Costo", "Costo ($)");
                rejillaFert.Columns.Add("Porcentaje", "% del Total");

                foreach (var costo in analisisCostos.CostoPorFertilizante.OrderByDescending(c => c.Value))
                {
                    double porcentaje = analisisCostos.PorcentajePorFertilizante.GetValueOrDefault(costo.Key, 0);
                    rejillaFert.Rows.Add(costo.Key, costo.Value.ToString("F3"), porcentaje.ToString("F1") + "%");
                }
            }

            // Rejilla de costos por nutriente
            var rejillaNut = controlPestanasPrincipal.TabPages[6].Controls.Find("RejillaCostosNutrientes", true)
                             .FirstOrDefault() as DataGridView;
            if (rejillaNut != null)
            {
                rejillaNut.Columns.Clear();
                rejillaNut.Columns.Add("Nutriente", "Nutriente");
                rejillaNut.Columns.Add("CostoPorKg", "Costo/kg ($)");
                rejillaNut.Columns.Add("CostoTotal", "Costo Total ($)");
                rejillaNut.Columns.Add("FuenteBarata", "Fuente Más Barata");

                foreach (var nutriente in costosNutrientes.OrderByDescending(n => n.Value.CostoTotal))
                {
                    rejillaNut.Rows.Add(
                        nutriente.Key,
                        nutriente.Value.CostoPorKg_Nutriente.ToString("F2"),
                        nutriente.Value.CostoTotal.ToString("F3"),
                        nutriente.Value.FuenteMasBarata
                    );
                }
            }

            // Rejilla de resumen
            var rejillaRes = controlPestanasPrincipal.TabPages[6].Controls.Find("RejillaResumenCostos", true)
                             .FirstOrDefault() as DataGridView;
            if (rejillaRes != null)
            {
                rejillaRes.Columns.Clear();
                rejillaRes.Columns.Add("Concepto", "Concepto");
                rejillaRes.Columns.Add("Valor", "Valor");
                rejillaRes.Columns.Add("Unidad", "Unidad");

                rejillaRes.Rows.Add("Costo por Aplicación", reporteCostos["CostoTotal_PorAplicacion"].ToString(), "$");
                rejillaRes.Rows.Add("Costo por m³", reporteCostos["CostoPorM3_Diluida"].ToString(), "$/m³");
                rejillaRes.Rows.Add("Costo por m² por Aplicación", reporteCostos["CostoPorM2_PorAplicacion"].ToString(), "$/m²");
                rejillaRes.Rows.Add("Costo Anual Total", reporteCostos["CostoAnual_Total"].ToString(), "$");
                rejillaRes.Rows.Add("Costo Anual por m²", reporteCostos["CostoAnual_PorM2"].ToString(), "$/m²/año");
            }
        }

        private void CargarPaso8_ReporteFinal()
        {
            var pestana = new TabPage("Paso 8: Reporte Final");
            var panelPrincipal = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(10)
            };
            panelPrincipal.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panelPrincipal.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Panel de botones de exportación
            var panelBotones = new Panel
            {
                Height = 60,
                Dock = DockStyle.Top,
                Padding = new Padding(10)
            };

            var botonGenerarReporte = new Button
            {
                Text = "Generar Reporte Completo",
                Size = new Size(180, 35),
                Location = new Point(10, 10),
                BackColor = Color.LightGreen
            };
            botonGenerarReporte.Click += (s, e) => GenerarReporteFinal();

            var botonExportarExcel = new Button
            {
                Text = "Exportar a Excel",
                Size = new Size(120, 35),
                Location = new Point(200, 10),
                BackColor = Color.LightBlue
            };
            botonExportarExcel.Click += (s, e) => ExportarAExcel();

            var botonGuardarPDF = new Button
            {
                Text = "Guardar PDF",
                Size = new Size(120, 35),
                Location = new Point(330, 10),
                BackColor = Color.LightCoral
            };
            botonGuardarPDF.Click += (s, e) => GuardarPDF();

            panelBotones.Controls.AddRange(new Control[] { botonGenerarReporte, botonExportarExcel, botonGuardarPDF });

            // Panel de reporte
            var panelReporte = new GroupBox
            {
                Text = "Reporte Final Integral",
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var cajaTextoReporte = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9F),
                ReadOnly = true,
                Name = "CajaTextoReporte"
            };

            panelReporte.Controls.Add(cajaTextoReporte);

            panelPrincipal.Controls.Add(panelBotones, 0, 0);
            panelPrincipal.Controls.Add(panelReporte, 0, 1);

            pestana.Controls.Add(panelPrincipal);
            controlPestanasPrincipal.TabPages.Add(pestana);
        }

        private void GenerarReporteFinal()
        {
            try
            {
                var cajaReporte = controlPestanasPrincipal.TabPages[7].Controls.Find("CajaTextoReporte", true)
                                  .FirstOrDefault() as RichTextBox;
                if (cajaReporte == null) return;

                var reporte = new StringBuilder();

                // Encabezado
                reporte.AppendLine("╔══════════════════════════════════════════════════════════════════════════════╗");
                reporte.AppendLine("║              REPORTE INTEGRAL DE SOLUCIÓN NUTRITIVA HIDROPÓNICA              ║");
                reporte.AppendLine("╚══════════════════════════════════════════════════════════════════════════════╝");
                reporte.AppendLine();
                reporte.AppendLine($"Fecha de Generación: {DateTime.Now:dd/MM/yyyy HH:mm}");
                reporte.AppendLine($"Software: Calculadora Hidropónica Profesional v1.0");
                reporte.AppendLine();

                // Resumen ejecutivo
                reporte.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
                reporte.AppendLine("                                RESUMEN EJECUTIVO");
                reporte.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
                reporte.AppendLine();

                if (resultadosCalculo != null)
                {
                    reporte.AppendLine($"• Total de fertilizantes calculados: {resultadosCalculo.Count}");
                    reporte.AppendLine($"• Concentración total de sales: {resultadosCalculo.Sum(r => r.ConcentracionSal_mgL):F1} mg/L");
                }

                if (balanceIonico != null)
                {
                    var verificacionBalance = moduloVerificacion?.VerificarBalanceIonico(balanceIonico.Final_meqL);
                    if (verificacionBalance != null)
                    {
                        reporte.AppendLine($"• Balance iónico: {(verificacionBalance["EstaBalanceado"] == 1 ? "✓ Balanceado" : "✗ Desbalanceado")} " +
                                         $"({verificacionBalance["DiferenciaPorcentual"]:F1}% diferencia)");
                    }
                }

                reporte.AppendLine();

                // 1. Análisis de agua
                reporte.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
                reporte.AppendLine("                            1. ANÁLISIS DE AGUA");
                reporte.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
                reporte.AppendLine();

                if (datosAgua != null)
                {
                    reporte.AppendLine("Parámetros del Agua:");
                    reporte.AppendLine($"  • pH: {datosAgua.pH:F1}");
                    reporte.AppendLine($"  • CE: {datosAgua.CE:F1} dS/m");
                    reporte.AppendLine($"  • HCO3-: {datosAgua.HCO3:F1} mg/L");
                    reporte.AppendLine();

                    reporte.AppendLine("Elementos en el Agua (mg/L):");
                    foreach (var elemento in datosAgua.Elementos_mgL.OrderBy(e => e.Key))
                    {
                        reporte.AppendLine($"  • {elemento.Key}: {elemento.Value:F1}");
                    }
                }

                reporte.AppendLine();

                // 2. Concentraciones objetivo
                reporte.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
                reporte.AppendLine("                        2. CONCENTRACIONES OBJETIVO");
                reporte.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
                reporte.AppendLine();

                if (concentracionesObjetivo != null)
                {
                    reporte.AppendLine("Macronutrientes (mg/L):");
                    var macros = new[] { "N", "P", "K", "Ca", "Mg", "S" };
                    foreach (var macro in macros)
                    {
                        if (concentracionesObjetivo.ContainsKey(macro))
                            reporte.AppendLine($"  • {macro}: {concentracionesObjetivo[macro]:F1}");
                    }
                }

                reporte.AppendLine();

                // 3. Ajuste de pH
                reporte.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
                reporte.AppendLine("                            3. AJUSTE DE pH");
                reporte.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
                reporte.AppendLine();

                if (resultadosAcidos != null && resultadosAcidos.Any())
                {
                    reporte.AppendLine("Requerimientos de Ácido:");
                    foreach (var acido in resultadosAcidos)
                    {
                        reporte.AppendLine($"  • {acido.NombreAcido}: {acido.VolumenAcido_mlL:F3} mL/L");
                        reporte.AppendLine($"    - Contribución de {acido.TipoNutriente}: {acido.ContribucionNutriente_mgL:F1} mg/L");
                        reporte.AppendLine($"    - Bicarbonatos a neutralizar: {acido.BicarbonatosANeutralizar:F1} mg/L");
                    }
                }
                else
                {
                    reporte.AppendLine("No se requiere ajuste de pH.");
                }

                reporte.AppendLine();

                // 4. Cálculos de fertilizantes
                reporte.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
                reporte.AppendLine("                        4. CÁLCULOS DE FERTILIZANTES");
                reporte.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
                reporte.AppendLine();

                if (resultadosCalculo != null)
                {
                    reporte.AppendLine("Fertilizantes Requeridos (mg/L):");
                    reporte.AppendLine();
                    foreach (var resultado in resultadosCalculo.OrderByDescending(r => r.ConcentracionSal_mgL))
                    {
                        reporte.AppendLine($"  {resultado.Nombre}: {resultado.ConcentracionSal_mgL:F1} mg/L");
                        reporte.AppendLine($"    Contribuciones de nutrientes:");
                        if (resultado.N > 0) reporte.AppendLine($"      - N: {resultado.N:F1} mg/L");
                        if (resultado.P > 0) reporte.AppendLine($"      - P: {resultado.P:F1} mg/L");
                        if (resultado.K > 0) reporte.AppendLine($"      - K: {resultado.K:F1} mg/L");
                        if (resultado.Ca > 0) reporte.AppendLine($"      - Ca: {resultado.Ca:F1} mg/L");
                        if (resultado.Mg > 0) reporte.AppendLine($"      - Mg: {resultado.Mg:F1} mg/L");
                        if (resultado.S > 0) reporte.AppendLine($"      - S: {resultado.S:F1} mg/L");
                        if (resultado.Fe > 0) reporte.AppendLine($"      - Fe: {resultado.Fe:F2} mg/L");
                        reporte.AppendLine();
                    }
                }

                // 5. Balance iónico
                reporte.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
                reporte.AppendLine("                            5. BALANCE IÓNICO");
                reporte.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
                reporte.AppendLine();

                if (balanceIonico != null)
                {
                    reporte.AppendLine("Concentraciones Finales (mg/L):");
                    var elementosBalance = new[] { "N", "P", "K", "Ca", "Mg", "S" };
                    foreach (var elemento in elementosBalance)
                    {
                        if (balanceIonico.Final_mgL.ContainsKey(elemento))
                        {
                            double objetivo = concentracionesObjetivo?.GetValueOrDefault(elemento, 0) ?? 0;
                            double final = balanceIonico.Final_mgL[elemento];
                            double desviacion = objetivo > 0 ? ((final - objetivo) / objetivo) * 100 : 0;

                            reporte.AppendLine($"  • {elemento}: {final:F1} mg/L (Objetivo: {objetivo:F1}, Desviación: {desviacion:F1}%)");
                        }
                    }

                    reporte.AppendLine();
                    reporte.AppendLine("Balance Cationes vs Aniones (meq/L):");
                    double sumaCationes = 0, sumaAniones = 0;

                    var cationes = new[] { "Ca", "K", "Mg", "Na", "NH4" };
                    var aniones = new[] { "NO3", "SO4", "Cl", "H2PO4", "HCO3" };

                    foreach (var cation in cationes)
                    {
                        if (balanceIonico.Final_meqL.ContainsKey(cation))
                            sumaCationes += balanceIonico.Final_meqL[cation];
                    }

                    foreach (var anion in aniones)
                    {
                        if (balanceIonico.Final_meqL.ContainsKey(anion))
                            sumaAniones += balanceIonico.Final_meqL[anion];
                    }

                    reporte.AppendLine($"  • Suma de cationes: {sumaCationes:F2} meq/L");
                    reporte.AppendLine($"  • Suma de aniones: {sumaAniones:F2} meq/L");
                    reporte.AppendLine($"  • Diferencia: {Math.Abs(sumaCationes - sumaAniones):F2} meq/L");

                    double diferenciaPorcentual = sumaCationes > 0 ? (Math.Abs(sumaCationes - sumaAniones) / sumaCationes) * 100 : 0;
                    reporte.AppendLine($"  • Diferencia porcentual: {diferenciaPorcentual:F1}%");
                    reporte.AppendLine($"  • Estado: {(diferenciaPorcentual <= 10 ? "✓ Balanceado" : "✗ Desbalanceado")}");
                }

                reporte.AppendLine();

                // 6. Instrucciones de preparación
                reporte.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
                reporte.AppendLine("                        6. INSTRUCCIONES DE PREPARACIÓN");
                reporte.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
                reporte.AppendLine();

                reporte.AppendLine("Para preparar 1000 litros de solución nutritiva:");
                reporte.AppendLine();
                reporte.AppendLine("1. Llenar el tanque con 800L de agua limpia");
                reporte.AppendLine("2. Iniciar el sistema de circulación/mezclado");
                reporte.AppendLine("3. Agregar fertilizantes en el siguiente orden:");
                reporte.AppendLine();

                if (resultadosCalculo != null)
                {
                    int paso = 1;
                    foreach (var resultado in resultadosCalculo.OrderBy(r => r.Nombre))
                    {
                        double cantidadPor1000L = resultado.ConcentracionSal_mgL; // mg/L
                        double cantidadKg = cantidadPor1000L / 1000.0; // Convertir a kg

                        reporte.AppendLine($"   {paso}. {resultado.Nombre}: {cantidadKg:F3} kg ({cantidadPor1000L:F1} mg/L)");
                        reporte.AppendLine($"      Esperar disolución completa antes de la siguiente adición");
                        paso++;
                    }
                }

                if (resultadosAcidos != null && resultadosAcidos.Any())
                {
                    reporte.AppendLine();
                    reporte.AppendLine("4. Agregar ácidos (SIEMPRE ácido al agua, nunca al revés):");
                    foreach (var acido in resultadosAcidos)
                    {
                        double volumenPor1000L = acido.VolumenAcido_mlL * 1000; // mL por 1000L
                        reporte.AppendLine($"   • {acido.NombreAcido}: {volumenPor1000L:F1} mL");
                    }
                }

                reporte.AppendLine();
                reporte.AppendLine("5. Completar el volumen a 1000L");
                reporte.AppendLine("6. Verificar pH final (objetivo: 5.8-6.2)");
                reporte.AppendLine("7. Verificar CE final (objetivo: 1.8-2.5 dS/m)");
                reporte.AppendLine();

                // 7. Medidas de seguridad
                reporte.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
                reporte.AppendLine("                           7. MEDIDAS DE SEGURIDAD");
                reporte.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
                reporte.AppendLine();

                reporte.AppendLine("IMPORTANTE - Equipos de Protección Personal (EPP):");
                reporte.AppendLine("  • Gafas de seguridad");
                reporte.AppendLine("  • Guantes de nitrilo resistentes a químicos");
                reporte.AppendLine("  • Delantal o ropa protectora");
                reporte.AppendLine("  • Mascarilla para polvo (al manejar fertilizantes secos)");
                reporte.AppendLine();

                reporte.AppendLine("Precauciones durante la preparación:");
                reporte.AppendLine("  • Asegurar ventilación adecuada del área de trabajo");
                reporte.AppendLine("  • Mantener agua limpia disponible para lavado de emergencia");
                reporte.AppendLine("  • No comer, beber o fumar durante la preparación");
                reporte.AppendLine("  • Agregar fertilizantes lentamente mientras se mezcla");
                reporte.AppendLine("  • Al usar ácidos: SIEMPRE agregar ácido al agua, NUNCA agua al ácido");
                reporte.AppendLine("  • Lavar manos y equipo después del uso");
                reporte.AppendLine();

                // Pie de página
                reporte.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
                reporte.AppendLine("                                 FIN DEL REPORTE");
                reporte.AppendLine("═══════════════════════════════════════════════════════════════════════════════");
                reporte.AppendLine();
                reporte.AppendLine("Este reporte ha sido generado automáticamente por el software de cálculo");
                reporte.AppendLine("de soluciones nutritivas hidropónicas. Verificar siempre los resultados");
                reporte.AppendLine("antes de la aplicación práctica.");

                cajaReporte.Text = reporte.ToString();

                MessageBox.Show("¡Reporte final generado con éxito!", "Reporte Completo",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar el reporte final: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportarAExcel()
        {
            MessageBox.Show("Funcionalidad de exportación a Excel estará disponible en la próxima versión.",
                          "Exportar Excel", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void GuardarPDF()
        {
            try
            {
                var cajaReporte = controlPestanasPrincipal.TabPages[7].Controls.Find("CajaTextoReporte", true)
                                  .FirstOrDefault() as RichTextBox;
                if (cajaReporte == null || string.IsNullOrEmpty(cajaReporte.Text))
                {
                    MessageBox.Show("Primero debe generar el reporte final.", "Error",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var saveDialog = new SaveFileDialog
                {
                    Filter = "Archivos de texto (*.txt)|*.txt|Todos los archivos (*.*)|*.*",
                    DefaultExt = "txt",
                    FileName = $"Reporte_Hidroponico_{DateTime.Now:yyyyMMdd_HHmm}.txt"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    System.IO.File.WriteAllText(saveDialog.FileName, cajaReporte.Text, System.Text.Encoding.UTF8);
                    MessageBox.Show($"Reporte guardado exitosamente en:\n{saveDialog.FileName}",
                                  "Guardado Exitoso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar el archivo: {ex.Message}", "Error de Guardado",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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