using System;
using System.Collections.Generic;
using System.Linq;
#pragma warning disable CS8618

namespace CalculadoraHidroponica.Modulos
{
    public class CostoFertilizante
    {
        public string Nombre { get; set; }
        public string Proveedor { get; set; }
        public double CostoPorKg { get; set; }
        public string Moneda { get; set; } = "USD";
        public double TamanoPaquete_kg { get; set; }
        public double CostoPorPaquete { get; set; }
        public bool EstaDisponible { get; set; } = true;
        public DateTime UltimaActualizacion { get; set; } = DateTime.Now;
        public string Notas { get; set; }
    }

    public class CostoNutriente
    {
        public string Nutriente { get; set; }
        public double CostoPorKg_Nutriente { get; set; }
        public string FuenteMasBarata { get; set; }
        public double CantidadTotal_kg { get; set; }
        public double CostoTotal { get; set; }
        public List<string> FuentesAlternativas { get; set; } = new List<string>();
    }

    public class AnalisisCostoSolucion
    {
        public double CostoTotal_Concentrada { get; set; }
        public double CostoTotal_Diluida { get; set; }
        public double CostoPorLitro_Concentrada { get; set; }
        public double CostoPorLitro_Diluida { get; set; }
        public double CostoPorM3_Diluida { get; set; }
        public Dictionary<string, double> CostoPorFertilizante { get; set; } = new Dictionary<string, double>();
        public Dictionary<string, double> CostoPorNutriente { get; set; } = new Dictionary<string, double>();
        public Dictionary<string, double> CostoPorTanque { get; set; } = new Dictionary<string, double>();
        public Dictionary<string, double> PorcentajePorFertilizante { get; set; } = new Dictionary<string, double>();
        public Dictionary<string, double> PorcentajePorNutriente { get; set; } = new Dictionary<string, double>();
        public List<string> SugerenciasOptimizacionCosto { get; set; } = new List<string>();
    }

    public class ResultadoComparacionCosto
    {
        public string NombreFormulacion { get; set; }
        public double CostoTotal { get; set; }
        public double CostoPorM3 { get; set; }
        public List<string> FertilizantesUsados { get; set; } = new List<string>();
        public Dictionary<string, double> DesviacionesNutrientes { get; set; } = new Dictionary<string, double>();
        public double PuntuacionCalidad { get; set; } // Basado en qué tan cerca está de las concentraciones objetivo
        public string Recomendacion { get; set; }
    }

    public class ModuloAnalisisCostos
    {
        private Dictionary<string, CostoFertilizante> costosFertilizantes;
        private Dictionary<string, List<string>> fuentesNutrientes; // Qué fertilizantes proporcionan cada nutriente

        public ModuloAnalisisCostos()
        {
            InicializarCostosFertilizantes();
            InicializarFuentesNutrientes();
        }

        private void InicializarCostosFertilizantes()
        {
            costosFertilizantes = new Dictionary<string, CostoFertilizante>
            {
                ["KH2PO4"] = new CostoFertilizante
                {
                    Nombre = "Fosfato Monopotásico",
                    Proveedor = "Genérico",
                    CostoPorKg = 2.50,
                    TamanoPaquete_kg = 25,
                    CostoPorPaquete = 62.50,
                    EstaDisponible = true,
                    Notas = "Fuente de alta calidad para P y K"
                },
                ["Ca(NO3)2.2H2O"] = new CostoFertilizante
                {
                    Nombre = "Nitrato de Calcio",
                    Proveedor = "Genérico",
                    CostoPorKg = 0.80,
                    TamanoPaquete_kg = 25,
                    CostoPorPaquete = 20.00,
                    EstaDisponible = true,
                    Notas = "Fuente principal de Ca y N"
                },
                ["KNO3"] = new CostoFertilizante
                {
                    Nombre = "Nitrato de Potasio",
                    Proveedor = "Genérico",
                    CostoPorKg = 1.20,
                    TamanoPaquete_kg = 25,
                    CostoPorPaquete = 30.00,
                    EstaDisponible = true,
                    Notas = "Fuente dual de K y N"
                },
                ["K2SO4"] = new CostoFertilizante
                {
                    Nombre = "Sulfato de Potasio",
                    Proveedor = "Genérico",
                    CostoPorKg = 1.50,
                    TamanoPaquete_kg = 25,
                    CostoPorPaquete = 37.50,
                    EstaDisponible = true,
                    Notas = "Fuente de K y S, bajo cloruro"
                },
                ["MgSO4.7H2O"] = new CostoFertilizante
                {
                    Nombre = "Sulfato de Magnesio",
                    Proveedor = "Genérico",
                    CostoPorKg = 0.60,
                    TamanoPaquete_kg = 25,
                    CostoPorPaquete = 15.00,
                    EstaDisponible = true,
                    Notas = "Sal de Epsom, fuente de Mg y S"
                },
                ["NH4NO3"] = new CostoFertilizante
                {
                    Nombre = "Nitrato de Amonio",
                    Proveedor = "Genérico",
                    CostoPorKg = 0.45,
                    TamanoPaquete_kg = 50,
                    CostoPorPaquete = 22.50,
                    EstaDisponible = true,
                    Notas = "Fuente barata de N, usar con precaución"
                },
                ["(NH4)2SO4"] = new CostoFertilizante
                {
                    Nombre = "Sulfato de Amonio",
                    Proveedor = "Genérico",
                    CostoPorKg = 0.50,
                    TamanoPaquete_kg = 50,
                    CostoPorPaquete = 25.00,
                    EstaDisponible = true,
                    Notas = "Fuente de N y S"
                },
                ["FeSO4.7H2O"] = new CostoFertilizante
                {
                    Nombre = "Sulfato de Hierro",
                    Proveedor = "Genérico",
                    CostoPorKg = 1.80,
                    TamanoPaquete_kg = 5,
                    CostoPorPaquete = 9.00,
                    EstaDisponible = true,
                    Notas = "Fuente de hierro, puede necesitar quelación"
                },
                ["FeEDTA"] = new CostoFertilizante
                {
                    Nombre = "Quelato de Hierro EDTA",
                    Proveedor = "Genérico",
                    CostoPorKg = 8.50,
                    TamanoPaquete_kg = 5,
                    CostoPorPaquete = 42.50,
                    EstaDisponible = true,
                    Notas = "Hierro quelado, más caro pero estable"
                },
                ["H3BO3"] = new CostoFertilizante
                {
                    Nombre = "Ácido Bórico",
                    Proveedor = "Genérico",
                    CostoPorKg = 3.20,
                    TamanoPaquete_kg = 5,
                    CostoPorPaquete = 16.00,
                    EstaDisponible = true,
                    Notas = "Fuente de boro"
                },
                ["MnSO4.4H2O"] = new CostoFertilizante
                {
                    Nombre = "Sulfato de Manganeso",
                    Proveedor = "Genérico",
                    CostoPorKg = 2.80,
                    TamanoPaquete_kg = 5,
                    CostoPorPaquete = 14.00,
                    EstaDisponible = true,
                    Notas = "Fuente de manganeso"
                },
                ["ZnSO4.7H2O"] = new CostoFertilizante
                {
                    Nombre = "Sulfato de Zinc",
                    Proveedor = "Genérico",
                    CostoPorKg = 3.50,
                    TamanoPaquete_kg = 5,
                    CostoPorPaquete = 17.50,
                    EstaDisponible = true,
                    Notas = "Fuente de zinc"
                },
                ["CuSO4.5H2O"] = new CostoFertilizante
                {
                    Nombre = "Sulfato de Cobre",
                    Proveedor = "Genérico",
                    CostoPorKg = 4.20,
                    TamanoPaquete_kg = 5,
                    CostoPorPaquete = 21.00,
                    EstaDisponible = true,
                    Notas = "Fuente de cobre"
                }
            };
        }

        private void InicializarFuentesNutrientes()
        {
            fuentesNutrientes = new Dictionary<string, List<string>>
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

        public AnalisisCostoSolucion CalcularCostoSolucion(
            Dictionary<string, double> cantidadesFertilizantes_kg,
            double volumenConcentrado_L,
            double volumenDiluido_L,
            int factorConcentracion)
        {
            var analisis = new AnalisisCostoSolucion();

            // Calcular costo por fertilizante
            double costoTotal = 0;
            foreach (var fertilizante in cantidadesFertilizantes_kg)
            {
                if (costosFertilizantes.ContainsKey(fertilizante.Key))
                {
                    double costo = fertilizante.Value * costosFertilizantes[fertilizante.Key].CostoPorKg;
                    analisis.CostoPorFertilizante[fertilizante.Key] = costo;
                    costoTotal += costo;
                }
            }

            analisis.CostoTotal_Concentrada = costoTotal;
            analisis.CostoTotal_Diluida = costoTotal; // Mismo costo total, solo volúmenes diferentes
            analisis.CostoPorLitro_Concentrada = volumenConcentrado_L > 0 ? costoTotal / volumenConcentrado_L : 0;
            analisis.CostoPorLitro_Diluida = volumenDiluido_L > 0 ? costoTotal / volumenDiluido_L : 0;
            analisis.CostoPorM3_Diluida = analisis.CostoPorLitro_Diluida * 1000;

            // Calcular porcentajes por fertilizante
            if (costoTotal > 0)
            {
                foreach (var costo in analisis.CostoPorFertilizante)
                {
                    analisis.PorcentajePorFertilizante[costo.Key] = (costo.Value / costoTotal) * 100;
                }
            }

            return analisis;
        }

        public Dictionary<string, CostoNutriente> AnalizarCostosNutrientes(
            Dictionary<string, double> nutrientesObjetivo_mgL,
            double volumen_L)
        {
            var costosNutrientes = new Dictionary<string, CostoNutriente>();

            foreach (var nutriente in nutrientesObjetivo_mgL)
            {
                if (fuentesNutrientes.ContainsKey(nutriente.Key))
                {
                    var fuentes = fuentesNutrientes[nutriente.Key];
                    var costoMasBarato = EncontrarFuenteNutrienteMasBarata(nutriente.Key, nutriente.Value, volumen_L, fuentes);

                    costosNutrientes[nutriente.Key] = new CostoNutriente
                    {
                        Nutriente = nutriente.Key,
                        CostoPorKg_Nutriente = costoMasBarato.costoPorKg,
                        FuenteMasBarata = costoMasBarato.fuente,
                        CantidadTotal_kg = (nutriente.Value * volumen_L) / 1000000, // Convertir mg a kg
                        CostoTotal = costoMasBarato.costoTotal,
                        FuentesAlternativas = fuentes.Where(s => s != costoMasBarato.fuente).ToList()
                    };
                }
            }

            return costosNutrientes;
        }

        private (double costoPorKg, string fuente, double costoTotal) EncontrarFuenteNutrienteMasBarata(
            string nutriente, double concentracion_mgL, double volumen_L, List<string> fuentes)
        {
            double costoPorKgMasBarato = double.MaxValue;
            string fuenteMasBarata = "";
            double costoTotalMasBarato = double.MaxValue;

            foreach (var fuente in fuentes)
            {
                if (costosFertilizantes.ContainsKey(fuente))
                {
                    var costoFertilizante = costosFertilizantes[fuente];
                    double contenidoNutriente = ObtenerPorcentajeContenidoNutriente(fuente, nutriente);

                    if (contenidoNutriente > 0)
                    {
                        double costoPorKgNutriente = costoFertilizante.CostoPorKg / (contenidoNutriente / 100.0);
                        double nutrienteTotalNecesario_kg = (concentracion_mgL * volumen_L) / 1000000.0;
                        double costoTotal = nutrienteTotalNecesario_kg * costoPorKgNutriente;

                        if (costoPorKgNutriente < costoPorKgMasBarato)
                        {
                            costoPorKgMasBarato = costoPorKgNutriente;
                            fuenteMasBarata = fuente;
                            costoTotalMasBarato = costoTotal;
                        }
                    }
                }
            }

            return (costoPorKgMasBarato, fuenteMasBarata, costoTotalMasBarato);
        }

        private double ObtenerPorcentajeContenidoNutriente(string fertilizante, string nutriente)
        {
            // Porcentajes simplificados de contenido de nutrientes (en realidad, estos vendrían de una base de datos)
            var contenidos = new Dictionary<(string, string), double>
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

            return contenidos.GetValueOrDefault((fertilizante, nutriente), 0);
        }

        public List<ResultadoComparacionCosto> CompararFormulaciones(
            List<Dictionary<string, double>> formulaciones,
            Dictionary<string, double> concentracionesObjetivo,
            double volumen_L)
        {
            var comparaciones = new List<ResultadoComparacionCosto>();

            for (int i = 0; i < formulaciones.Count; i++)
            {
                var formulacion = formulaciones[i];
                var comparacion = new ResultadoComparacionCosto
                {
                    NombreFormulacion = $"Formulación {i + 1}",
                    FertilizantesUsados = formulacion.Keys.ToList()
                };

                // Calcular costo total
                double costoTotal = 0;
                foreach (var fertilizante in formulacion)
                {
                    if (costosFertilizantes.ContainsKey(fertilizante.Key))
                    {
                        costoTotal += (fertilizante.Value * volumen_L / 1000) * costosFertilizantes[fertilizante.Key].CostoPorKg;
                    }
                }

                comparacion.CostoTotal = costoTotal;
                comparacion.CostoPorM3 = costoTotal / (volumen_L / 1000);

                // Calcular puntuación de calidad basada en desviaciones objetivo
                double puntuacionCalidad = CalcularPuntuacionCalidad(formulacion, concentracionesObjetivo);
                comparacion.PuntuacionCalidad = puntuacionCalidad;

                // Generar recomendación
                comparacion.Recomendacion = GenerarRecomendacionFormulacion(comparacion);

                comparaciones.Add(comparacion);
            }

            return comparaciones.OrderBy(c => c.CostoPorM3).ToList();
        }

        private double CalcularPuntuacionCalidad(Dictionary<string, double> formulacion, Dictionary<string, double> objetivos)
        {
            // Puntuación de calidad simplificada - en realidad calcularía la entrega real de nutrientes
            double desviacionTotal = 0;
            int conteoNutrientes = 0;

            foreach (var objetivo in objetivos)
            {
                // Este es un cálculo simplificado - necesitaría cálculo real del contenido de nutrientes
                double entregaReal = EstimarEntregaNutriente(formulacion, objetivo.Key);
                double desviacion = Math.Abs(entregaReal - objetivo.Value) / objetivo.Value;
                desviacionTotal += desviacion;
                conteoNutrientes++;
            }

            double desviacionPromedio = conteoNutrientes > 0 ? desviacionTotal / conteoNutrientes : 1.0;
            return Math.Max(0, 100 - (desviacionPromedio * 100)); // Puntuación sobre 100
        }

        private double EstimarEntregaNutriente(Dictionary<string, double> formulacion, string nutriente)
        {
            double entregaTotal = 0;

            foreach (var fertilizante in formulacion)
            {
                double contenido = ObtenerPorcentajeContenidoNutriente(fertilizante.Key, nutriente);
                entregaTotal += fertilizante.Value * (contenido / 100.0);
            }

            return entregaTotal;
        }

        private string GenerarRecomendacionFormulacion(ResultadoComparacionCosto comparacion)
        {
            if (comparacion.PuntuacionCalidad >= 95 && comparacion.CostoPorM3 <= 10)
                return "Excelente: Alta calidad y bajo costo";
            else if (comparacion.PuntuacionCalidad >= 90)
                return "Buena: Cumple bien los objetivos de nutrientes";
            else if (comparacion.CostoPorM3 <= 5)
                return "Económica: Bajo costo pero verificar balance de nutrientes";
            else if (comparacion.PuntuacionCalidad < 80)
                return "Deficiente: Desviaciones significativas de nutrientes";
            else
                return "Aceptable: Costo y calidad moderados";
        }

        public List<string> GenerarSugerenciasOptimizacionCosto(
            AnalisisCostoSolucion analisis,
            Dictionary<string, double> cantidadesFertilizantes)
        {
            var sugerencias = new List<string>();

            // Encontrar fertilizantes más caros
            var fertilizantesCaros = analisis.PorcentajePorFertilizante
                .Where(f => f.Value > 30)
                .OrderByDescending(f => f.Value);

            foreach (var caro in fertilizantesCaros)
            {
                sugerencias.Add($"{caro.Key} representa el {caro.Value:F1}% del costo total. " +
                               "Considere fuentes alternativas o proveedores.");
            }

            // Verificar micronutrientes caros
            var micronutrientes = new[] { "FeEDTA", "MnSO4.4H2O", "ZnSO4.7H2O", "CuSO4.5H2O", "H3BO3" };
            var microsCostosos = analisis.CostoPorFertilizante
                .Where(f => micronutrientes.Contains(f.Key) &&
                           analisis.PorcentajePorFertilizante[f.Key] > 10);

            if (microsCostosos.Any())
            {
                sugerencias.Add("Los micronutrientes representan un costo significativo. " +
                               "Considere usar mezclas de micronutrientes premezcladas o formas queladas.");
            }

            // Sugerir compras al por mayor
            foreach (var fertilizante in cantidadesFertilizantes.Where(f => f.Value > 50))
            {
                sugerencias.Add($"Gran cantidad de {fertilizante.Key} necesaria ({fertilizante.Value:F1} kg). " +
                               "Considere compras al por mayor para mejores precios.");
            }

            // Sugerencias generales de reducción de costos
            sugerencias.Add("Compare precios de múltiples proveedores regularmente.");
            sugerencias.Add("Considere compras estacionales durante períodos de baja demanda.");
            sugerencias.Add("Evalúe fertilizantes genéricos vs. de marca para ahorros en costos.");

            return sugerencias;
        }

        public Dictionary<string, object> GenerarReporteCostos(
            AnalisisCostoSolucion analisisCostos,
            Dictionary<string, CostoNutriente> costosNutrientes,
            double areaTotal_m2,
            double aplicacionesPorAno = 365)
        {
            var reporte = new Dictionary<string, object>();

            // Costos resumen
            reporte["CostoTotal_PorAplicacion"] = analisisCostos.CostoTotal_Diluida;
            reporte["CostoPorM3_Diluida"] = analisisCostos.CostoPorM3_Diluida;
            reporte["CostoPorM2_PorAplicacion"] = areaTotal_m2 > 0 ? analisisCostos.CostoTotal_Diluida / areaTotal_m2 : 0;
            reporte["CostoAnual_Total"] = analisisCostos.CostoTotal_Diluida * aplicacionesPorAno;
            reporte["CostoAnual_PorM2"] = areaTotal_m2 > 0 ?
                (analisisCostos.CostoTotal_Diluida * aplicacionesPorAno) / areaTotal_m2 : 0;

            // Desglose de costos por fertilizante
            var desgloseFertilizantes = analisisCostos.CostoPorFertilizante
                .OrderByDescending(f => f.Value)
                .ToDictionary(f => f.Key, f => new
                {
                    Costo = f.Value,
                    Porcentaje = analisisCostos.PorcentajePorFertilizante.GetValueOrDefault(f.Key, 0)
                });
            reporte["CostoPorFertilizante"] = desgloseFertilizantes;

            // Desglose de costos por nutriente
            var desgloseNutrientes = costosNutrientes
                .OrderByDescending(n => n.Value.CostoTotal)
                .ToDictionary(n => n.Key, n => new
                {
                    CostoPorKg_Nutriente = n.Value.CostoPorKg_Nutriente,
                    CostoTotal = n.Value.CostoTotal,
                    FuenteMasBarata = n.Value.FuenteMasBarata,
                    Cantidad_kg = n.Value.CantidadTotal_kg
                });
            reporte["CostoPorNutriente"] = desgloseNutrientes;

            // Métricas de eficiencia de costos
            reporte["NutrienteMasCaro"] = costosNutrientes.OrderByDescending(n => n.Value.CostoPorKg_Nutriente).FirstOrDefault().Key;
            reporte["NutrienteMasBarato"] = costosNutrientes.OrderBy(n => n.Value.CostoPorKg_Nutriente).FirstOrDefault().Key;
            reporte["CostoPromedioNutriente_PorKg"] = costosNutrientes.Values.Average(n => n.CostoPorKg_Nutriente);

            // Sugerencias de optimización
            reporte["SugerenciasOptimizacion"] = analisisCostos.SugerenciasOptimizacionCosto;

            return reporte;
        }

        public void ActualizarCostoFertilizante(string nombreFertilizante, double nuevoCostoPorKg, string proveedor = "")
        {
            if (costosFertilizantes.ContainsKey(nombreFertilizante))
            {
                costosFertilizantes[nombreFertilizante].CostoPorKg = nuevoCostoPorKg;
                costosFertilizantes[nombreFertilizante].UltimaActualizacion = DateTime.Now;
                if (!string.IsNullOrEmpty(proveedor))
                {
                    costosFertilizantes[nombreFertilizante].Proveedor = proveedor;
                }
            }
            else
            {
                // Agregar nuevo costo de fertilizante
                costosFertilizantes[nombreFertilizante] = new CostoFertilizante
                {
                    Nombre = nombreFertilizante,
                    Proveedor = proveedor ?? "Desconocido",
                    CostoPorKg = nuevoCostoPorKg,
                    EstaDisponible = true,
                    UltimaActualizacion = DateTime.Now
                };
            }
        }

        public Dictionary<string, CostoFertilizante> ObtenerTodosCostosFertilizantes()
        {
            return new Dictionary<string, CostoFertilizante>(costosFertilizantes);
        }

        public List<string> ObtenerFertilizantesAlternativos(string nutriente)
        {
            return fuentesNutrientes.GetValueOrDefault(nutriente, new List<string>());
        }

        public double CalcularVolumenEquilibrio(double costoConfiguracion, double costoPorM3_Actual, double costoPorM3_Alternativo)
        {
            if (Math.Abs(costoPorM3_Actual - costoPorM3_Alternativo) < 0.001)
                return double.MaxValue; // No es posible ahorrar

            return costoConfiguracion / Math.Abs(costoPorM3_Actual - costoPorM3_Alternativo);
        }
    }
}