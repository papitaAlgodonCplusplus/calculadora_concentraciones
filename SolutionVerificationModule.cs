using System;
using System.Collections.Generic;
using System.Linq;
#pragma warning disable CS8618

namespace CalculadoraHidroponica.Modulos
{
    public class ResultadoVerificacion
    {
        public string Parametro { get; set; }
        public double ValorObjetivo { get; set; }
        public double ValorReal { get; set; }
        public string Unidad { get; set; }
        public double Desviacion { get; set; }
        public double PorcentajeDesviacion { get; set; }
        public string Estado { get; set; } // "OK", "Alto", "Bajo", "Crítico"
        public string ColorEstado { get; set; } // "Verde", "Amarillo", "Rojo"
        public string Recomendacion { get; set; }
        public double MinimoAceptable { get; set; }
        public double MaximoAceptable { get; set; }
    }

    public class ResultadoRelacionIonica
    {
        public string NombreRelacion { get; set; }
        public double RelacionReal { get; set; }
        public double ObjetivoMinimo { get; set; }
        public double ObjetivoMaximo { get; set; }
        public string Unidad { get; set; }
        public string Estado { get; set; }
        public string ColorEstado { get; set; }
        public string Recomendacion { get; set; }
        public string FaseCultivo { get; set; }
    }

    public class ModuloVerificacionSolucion
    {
        private Dictionary<string, (double min, double max, double tolerancia)> rangosNutrientes;
        private Dictionary<string, (double min, double max)> rangosParametrosFisicos;
        private Dictionary<string, DatosElemento> datosElementos;

        public ModuloVerificacionSolucion()
        {
            InicializarRangos();
            InicializarDatosElementos();
        }

        private void InicializarRangos()
        {
            // Rangos de concentración de nutrientes (mg/L) con tolerancia del 5% por defecto
            rangosNutrientes = new Dictionary<string, (double min, double max, double tolerancia)>
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

            // Rangos de parámetros físicos
            rangosParametrosFisicos = new Dictionary<string, (double min, double max)>
            {
                ["pH"] = (5.5, 6.5),
                ["CE"] = (1.5, 2.5), // dS/m para solución nutritiva
                ["Temperatura"] = (18, 25) // °C
            };
        }

        private void InicializarDatosElementos()
        {
            datosElementos = new Dictionary<string, DatosElemento>
            {
                ["Ca"] = new DatosElemento { PesoAtomico = 40.08, Valencia = 2, EsCation = true },
                ["K"] = new DatosElemento { PesoAtomico = 39.10, Valencia = 1, EsCation = true },
                ["Mg"] = new DatosElemento { PesoAtomico = 24.31, Valencia = 2, EsCation = true },
                ["Na"] = new DatosElemento { PesoAtomico = 22.99, Valencia = 1, EsCation = true },
                ["NH4"] = new DatosElemento { PesoAtomico = 18.04, Valencia = 1, EsCation = true },
                ["NO3"] = new DatosElemento { PesoAtomico = 62.00, Valencia = 1, EsCation = false },
                ["N"] = new DatosElemento { PesoAtomico = 14.01, Valencia = 1, EsCation = false },
                ["SO4"] = new DatosElemento { PesoAtomico = 96.06, Valencia = 2, EsCation = false },
                ["S"] = new DatosElemento { PesoAtomico = 32.06, Valencia = 2, EsCation = false },
                ["Cl"] = new DatosElemento { PesoAtomico = 35.45, Valencia = 1, EsCation = false },
                ["H2PO4"] = new DatosElemento { PesoAtomico = 96.99, Valencia = 1, EsCation = false },
                ["P"] = new DatosElemento { PesoAtomico = 30.97, Valencia = 1, EsCation = false },
                ["HCO3"] = new DatosElemento { PesoAtomico = 61.02, Valencia = 1, EsCation = false }
            };
        }

        public List<ResultadoVerificacion> VerificarConcentracionesNutrientes(
            Dictionary<string, double> concentracionesObjetivo,
            Dictionary<string, double> concentracionesReales)
        {
            var resultados = new List<ResultadoVerificacion>();

            foreach (var objetivo in concentracionesObjetivo)
            {
                string nutriente = objetivo.Key;
                double valorObjetivo = objetivo.Value;
                double valorReal = concentracionesReales.GetValueOrDefault(nutriente, 0);

                if (rangosNutrientes.ContainsKey(nutriente))
                {
                    var rango = rangosNutrientes[nutriente];
                    var resultado = new ResultadoVerificacion
                    {
                        Parametro = nutriente,
                        ValorObjetivo = valorObjetivo,
                        ValorReal = valorReal,
                        Unidad = "mg/L",
                        Desviacion = valorReal - valorObjetivo,
                        PorcentajeDesviacion = valorObjetivo > 0 ? Math.Abs(valorReal - valorObjetivo) / valorObjetivo * 100 : 0,
                        MinimoAceptable = valorObjetivo * (1 - rango.tolerancia),
                        MaximoAceptable = valorObjetivo * (1 + rango.tolerancia)
                    };

                    // Determinar estado
                    if (valorReal >= resultado.MinimoAceptable && valorReal <= resultado.MaximoAceptable)
                    {
                        resultado.Estado = "OK";
                        resultado.ColorEstado = "Verde";
                        resultado.Recomendacion = "Concentración dentro del rango aceptable";
                    }
                    else if (valorReal > resultado.MaximoAceptable)
                    {
                        resultado.Estado = valorReal > valorObjetivo * 1.2 ? "Crítico" : "Alto";
                        resultado.ColorEstado = resultado.Estado == "Crítico" ? "Rojo" : "Naranja";
                        resultado.Recomendacion = $"Concentración demasiado alta. Reducir aporte de fertilizante o aumentar dilución.";
                    }
                    else
                    {
                        resultado.Estado = valorReal < valorObjetivo * 0.8 ? "Crítico" : "Bajo";
                        resultado.ColorEstado = resultado.Estado == "Crítico" ? "Rojo" : "Amarillo";
                        resultado.Recomendacion = $"Concentración demasiado baja. Aumentar aporte de fertilizante.";
                    }

                    resultados.Add(resultado);
                }
            }

            return resultados;
        }

        public List<ResultadoVerificacion> VerificarParametrosFisicos(
            double pH, double ce, double temperatura = 20.0)
        {
            var resultados = new List<ResultadoVerificacion>();

            // Verificar pH
            var rangoPH = rangosParametrosFisicos["pH"];
            resultados.Add(new ResultadoVerificacion
            {
                Parametro = "pH",
                ValorReal = pH,
                Unidad = "unidades pH",
                MinimoAceptable = rangoPH.min,
                MaximoAceptable = rangoPH.max,
                Estado = pH >= rangoPH.min && pH <= rangoPH.max ? "OK" :
                        pH > rangoPH.max ? "Alto" : "Bajo",
                ColorEstado = pH >= rangoPH.min && pH <= rangoPH.max ? "Verde" : "Rojo",
                Recomendacion = pH >= rangoPH.min && pH <= rangoPH.max ?
                    "pH dentro del rango óptimo" :
                    pH > rangoPH.max ? "pH demasiado alto - aumentar adición de ácido" :
                    "pH demasiado bajo - reducir adición de ácido"
            });

            // Verificar CE
            var rangoCE = rangosParametrosFisicos["CE"];
            resultados.Add(new ResultadoVerificacion
            {
                Parametro = "CE",
                ValorReal = ce,
                Unidad = "dS/m",
                MinimoAceptable = rangoCE.min,
                MaximoAceptable = rangoCE.max,
                Estado = ce >= rangoCE.min && ce <= rangoCE.max ? "OK" :
                        ce > rangoCE.max ? "Alto" : "Bajo",
                ColorEstado = ce >= rangoCE.min && ce <= rangoCE.max ? "Verde" : "Rojo",
                Recomendacion = ce >= rangoCE.min && ce <= rangoCE.max ?
                    "CE dentro del rango óptimo" :
                    ce > rangoCE.max ? "CE demasiado alta - diluir solución" :
                    "CE demasiado baja - aumentar concentración de fertilizantes"
            });

            return resultados;
        }

        public Dictionary<string, double> VerificarBalanceIonico(Dictionary<string, double> concentracionesFinales_meqL)
        {
            var resultados = new Dictionary<string, double>();

            double sumaCationes = 0;
            double sumaAniones = 0;

            foreach (var ion in concentracionesFinales_meqL)
            {
                if (datosElementos.ContainsKey(ion.Key))
                {
                    if (datosElementos[ion.Key].EsCation)
                        sumaCationes += ion.Value;
                    else
                        sumaAniones += ion.Value;
                }
            }

            resultados["SumaCationes"] = sumaCationes;
            resultados["SumaAniones"] = sumaAniones;
            resultados["Diferencia"] = Math.Abs(sumaCationes - sumaAniones);
            resultados["DiferenciaPorcentual"] = sumaCationes > 0 ? (resultados["Diferencia"] / sumaCationes) * 100.0 : 0;
            resultados["EstaBalanceado"] = resultados["DiferenciaPorcentual"] <= 10.0 ? 1 : 0;
            resultados["Tolerancia"] = Math.Min(sumaCationes, sumaAniones) * 0.1; // Tolerancia del 10%

            return resultados;
        }

        public List<ResultadoRelacionIonica> VerificarRelacionesIonicas(
            Dictionary<string, double> concentraciones_meqL,
            Dictionary<string, double> concentraciones_mmolL,
            Dictionary<string, double> concentraciones_mgL,
            string faseCultivo = "General")
        {
            var resultados = new List<ResultadoRelacionIonica>();

            // Relación K:Ca:Mg en meq/L (objetivo típico: 4:4:1 a 6:4:2)
            double k_meq = concentraciones_meqL.GetValueOrDefault("K", 0);
            double ca_meq = concentraciones_meqL.GetValueOrDefault("Ca", 0);
            double mg_meq = concentraciones_meqL.GetValueOrDefault("Mg", 0);

            if (ca_meq > 0 && mg_meq > 0)
            {
                double relacionK_Ca = k_meq / ca_meq;
                double relacionCa_Mg = ca_meq / mg_meq;

                resultados.Add(new ResultadoRelacionIonica
                {
                    NombreRelacion = "Relación K:Ca",
                    RelacionReal = relacionK_Ca,
                    ObjetivoMinimo = 0.8,
                    ObjetivoMaximo = 1.5,
                    Unidad = "relación meq/L",
                    Estado = relacionK_Ca >= 0.8 && relacionK_Ca <= 1.5 ? "OK" : "Desbalanceado",
                    ColorEstado = relacionK_Ca >= 0.8 && relacionK_Ca <= 1.5 ? "Verde" : "Naranja",
                    Recomendacion = relacionK_Ca >= 0.8 && relacionK_Ca <= 1.5 ?
                        "Relación K:Ca balanceada" :
                        relacionK_Ca > 1.5 ? "Demasiado K en relación al Ca" : "Demasiado Ca en relación al K",
                    FaseCultivo = faseCultivo
                });

                resultados.Add(new ResultadoRelacionIonica
                {
                    NombreRelacion = "Relación Ca:Mg",
                    RelacionReal = relacionCa_Mg,
                    ObjetivoMinimo = 2.0,
                    ObjetivoMaximo = 4.0,
                    Unidad = "relación meq/L",
                    Estado = relacionCa_Mg >= 2.0 && relacionCa_Mg <= 4.0 ? "OK" : "Desbalanceado",
                    ColorEstado = relacionCa_Mg >= 2.0 && relacionCa_Mg <= 4.0 ? "Verde" : "Naranja",
                    Recomendacion = relacionCa_Mg >= 2.0 && relacionCa_Mg <= 4.0 ?
                        "Relación Ca:Mg balanceada" :
                        relacionCa_Mg > 4.0 ? "Demasiado Ca en relación al Mg" : "Demasiado Mg en relación al Ca",
                    FaseCultivo = faseCultivo
                });
            }

            // Relación N/K en mmol/L (típico: 1.0-1.5 para vegetativo, >1.5 para generativo)
            double n_mmol = concentraciones_mmolL.GetValueOrDefault("N", 0);
            double k_mmol = concentraciones_mmolL.GetValueOrDefault("K", 0);

            if (k_mmol > 0)
            {
                double relacionN_K_mmol = n_mmol / k_mmol;
                double objetivoMin = faseCultivo == "Vegetativo" ? 1.0 : 1.5;
                double objetivoMax = faseCultivo == "Vegetativo" ? 1.5 : 2.5;

                resultados.Add(new ResultadoRelacionIonica
                {
                    NombreRelacion = "Relación N/K (mmol/L)",
                    RelacionReal = relacionN_K_mmol,
                    ObjetivoMinimo = objetivoMin,
                    ObjetivoMaximo = objetivoMax,
                    Unidad = "relación mmol/L",
                    Estado = relacionN_K_mmol >= objetivoMin && relacionN_K_mmol <= objetivoMax ? "OK" : "Desbalanceado",
                    ColorEstado = relacionN_K_mmol >= objetivoMin && relacionN_K_mmol <= objetivoMax ? "Verde" : "Naranja",
                    Recomendacion = relacionN_K_mmol >= objetivoMin && relacionN_K_mmol <= objetivoMax ?
                        $"Relación N/K óptima para crecimiento {faseCultivo.ToLower()}" :
                        relacionN_K_mmol > objetivoMax ? $"Relación N/K alta - promueve crecimiento {(faseCultivo == "Vegetativo" ? "generativo" : "vegetativo excesivo")}" :
                        $"Relación N/K baja - promueve crecimiento {(faseCultivo == "Vegetativo" ? "generativo" : "vegetativo")}",
                    FaseCultivo = faseCultivo
                });
            }

            // Relación N/K en mg/L (típico: 1.0-1.5 para cultivos de hoja, >1.5 para cultivos de fruto)
            double n_mg = concentraciones_mgL.GetValueOrDefault("N", 0);
            double k_mg = concentraciones_mgL.GetValueOrDefault("K", 0);

            if (k_mg > 0)
            {
                double relacionN_K_mg = n_mg / k_mg;
                double objetivoMin_mg = faseCultivo == "Vegetativo" ? 1.0 : 1.5;
                double objetivoMax_mg = faseCultivo == "Vegetativo" ? 1.5 : 2.5;

                resultados.Add(new ResultadoRelacionIonica
                {
                    NombreRelacion = "Relación N/K (mg/L)",
                    RelacionReal = relacionN_K_mg,
                    ObjetivoMinimo = objetivoMin_mg,
                    ObjetivoMaximo = objetivoMax_mg,
                    Unidad = "relación mg/L",
                    Estado = relacionN_K_mg >= objetivoMin_mg && relacionN_K_mg <= objetivoMax_mg ? "OK" : "Desbalanceado",
                    ColorEstado = relacionN_K_mg >= objetivoMin_mg && relacionN_K_mg <= objetivoMax_mg ? "Verde" : "Naranja",
                    Recomendacion = relacionN_K_mg >= objetivoMin_mg && relacionN_K_mg <= objetivoMax_mg ?
                        $"Relación N/K adecuada para fase {faseCultivo.ToLower()}" :
                        relacionN_K_mg > objetivoMax_mg ? "N/K alto - favorece crecimiento vegetativo" :
                        "N/K bajo - favorece crecimiento generativo",
                    FaseCultivo = faseCultivo
                });
            }

            // Relación NH4/NO3 (debe ser < 20% del N total)
            double nh4_mg = concentraciones_mgL.GetValueOrDefault("NH4", 0);
            double no3_mg = concentraciones_mgL.GetValueOrDefault("NO3", 0);
            double nTotalIonico = (nh4_mg * 14.01 / 18.04) + (no3_mg * 14.01 / 62.00); // Convertir a equivalentes de N

            if (nTotalIonico > 0)
            {
                double porcentajeNh4 = (nh4_mg * 14.01 / 18.04) / nTotalIonico * 100;

                resultados.Add(new ResultadoRelacionIonica
                {
                    NombreRelacion = "Porcentaje NH4 del N total",
                    RelacionReal = porcentajeNh4,
                    ObjetivoMinimo = 0,
                    ObjetivoMaximo = 20,
                    Unidad = "%",
                    Estado = porcentajeNh4 <= 20 ? "OK" : "Alto",
                    ColorEstado = porcentajeNh4 <= 20 ? "Verde" : "Rojo",
                    Recomendacion = porcentajeNh4 <= 20 ?
                        "Niveles de NH4 seguros" :
                        "Niveles de NH4 demasiado altos - pueden ser tóxicos para las plantas",
                    FaseCultivo = faseCultivo
                });
            }

            return resultados;
        }

        public double CalcularCE(Dictionary<string, double> concentraciones_meqL)
        {
            // Cálculo simplificado de CE: CE ≈ 0.1 × suma de cationes (meq/L)
            double sumaCationes = 0;
            foreach (var ion in concentraciones_meqL)
            {
                if (datosElementos.ContainsKey(ion.Key) && datosElementos[ion.Key].EsCation)
                {
                    sumaCationes += ion.Value;
                }
            }
            return sumaCationes * 0.1; // dS/m
        }

        public Dictionary<string, object> GenerarResumenVerificacion(
            List<ResultadoVerificacion> resultadosNutrientes,
            List<ResultadoVerificacion> resultadosFisicos,
            List<ResultadoRelacionIonica> resultadosRelaciones,
            Dictionary<string, double> balanceIonico)
        {
            var resumen = new Dictionary<string, object>();

            // Contar tipos de estado
            int verificacionesTotales = resultadosNutrientes.Count + resultadosFisicos.Count + resultadosRelaciones.Count;
            int conteoOK = resultadosNutrientes.Count(r => r.Estado == "OK") +
                         resultadosFisicos.Count(r => r.Estado == "OK") +
                         resultadosRelaciones.Count(r => r.Estado == "OK");
            int conteoAdvertencias = resultadosNutrientes.Count(r => r.Estado == "Alto" || r.Estado == "Bajo") +
                              resultadosFisicos.Count(r => r.Estado == "Alto" || r.Estado == "Bajo") +
                              resultadosRelaciones.Count(r => r.Estado == "Desbalanceado");
            int conteoCriticos = resultadosNutrientes.Count(r => r.Estado == "Crítico") +
                               resultadosFisicos.Count(r => r.Estado == "Crítico");

            resumen["VerificacionesTotales"] = verificacionesTotales;
            resumen["ConteoOK"] = conteoOK;
            resumen["ConteoAdvertencias"] = conteoAdvertencias;
            resumen["ConteoCriticos"] = conteoCriticos;
            resumen["EstadoGeneral"] = conteoCriticos > 0 ? "Crítico" : conteoAdvertencias > 0 ? "Advertencia" : "OK";
            resumen["ColorGeneral"] = conteoCriticos > 0 ? "Rojo" : conteoAdvertencias > 0 ? "Naranja" : "Verde";
            resumen["TasaExito"] = verificacionesTotales > 0 ? (double)conteoOK / verificacionesTotales * 100 : 0;

            // Estado del balance iónico
            resumen["EstadoBalanceIonico"] = balanceIonico["EstaBalanceado"] == 1 ? "Balanceado" : "Desbalanceado";
            resumen["PorcentajeBalanceIonico"] = balanceIonico["DiferenciaPorcentual"];

            // Problemas críticos
            var problemasCriticos = new List<string>();
            problemasCriticos.AddRange(resultadosNutrientes.Where(r => r.Estado == "Crítico").Select(r => $"{r.Parametro}: {r.Recomendacion}"));
            problemasCriticos.AddRange(resultadosFisicos.Where(r => r.Estado == "Crítico").Select(r => $"{r.Parametro}: {r.Recomendacion}"));
            if (balanceIonico["EstaBalanceado"] == 0)
            {
                problemasCriticos.Add($"Balance iónico: {balanceIonico["DiferenciaPorcentual"]:F1}% de diferencia excede la tolerancia del 10%");
            }
            resumen["ProblemasCriticos"] = problemasCriticos;

            return resumen;
        }
    }
}