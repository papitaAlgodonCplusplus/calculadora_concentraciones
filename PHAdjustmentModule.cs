using System;
using System.Collections.Generic;
#pragma warning disable CS8618

namespace CalculadoraHidroponica.Modulos
{
    public class DatosAcido
    {
        public string Nombre { get; set; }
        public double Pureza { get; set; } // %
        public double Densidad { get; set; } // g/L
        public double PesoMolecular { get; set; }
        public int Valencia { get; set; }
        public string ElementoProporcionado { get; set; } // N o P
        public double PesoMolecularElemento { get; set; }
    }

    public class ResultadoCalculoAcido
    {
        public string NombreAcido { get; set; }
        public double VolumenAcido_mlL { get; set; } // ml de ácido por L de agua
        public double ConcentracionHidrogeno_mgL { get; set; }
        public double ConcentracionHidrogeno_mmolL { get; set; }
        public double ContribucionNutriente_mgL { get; set; } // N o P contribuido
        public string TipoNutriente { get; set; } // "N" o "P"
        public double BicarbonatosANeutralizar { get; set; }
        public double BicarbonatosRestantes { get; set; } = 30.5; // Siempre dejar 30.5 mg/L como buffer
    }

    public class ResultadoTitracion
    {
        public List<ReplicaTitracion> Replicas { get; set; } = new List<ReplicaTitracion>();
        public double VolumenAcidoPromedio_mlL { get; set; }
        public double BicarbonatosCalculados_mgL { get; set; }
        public double BicarbonatosTotales_mgL { get; set; }
    }

    public class ReplicaTitracion
    {
        public int NumeroReplica { get; set; }
        public double VolumenAgua_L { get; set; }
        public double PHInicial { get; set; }
        public double PHFinal { get; set; }
        public double AcidoUsado_ml { get; set; }
        public double AcidoPorLitro_mlL { get; set; }
    }

    public class ModuloAjustePH
    {
        private Dictionary<string, DatosAcido> acidos;
        private const double BICARBONATOS_BUFFER = 30.5; // mg/L para dejar sin neutralizar
        private const double UMBRAL_BICARBONATOS = 122.0; // mg/L (2 mmol/L)

        public ModuloAjustePH()
        {
            InicializarAcidos();
        }

        private void InicializarAcidos()
        {
            acidos = new Dictionary<string, DatosAcido>
            {
                ["HNO3"] = new DatosAcido
                {
                    Nombre = "Ácido Nítrico",
                    Pureza = 65.0,
                    Densidad = 1400.0,
                    PesoMolecular = 63.01,
                    Valencia = 1,
                    ElementoProporcionado = "N",
                    PesoMolecularElemento = 14.01
                },
                ["H3PO4"] = new DatosAcido
                {
                    Nombre = "Ácido Fosfórico",
                    Pureza = 85.0,
                    Densidad = 1685.0,
                    PesoMolecular = 98.00,
                    Valencia = 3,
                    ElementoProporcionado = "P",
                    PesoMolecularElemento = 30.97
                }
            };
        }

        public string SeleccionarEstrategiaAcido(double hco3_mgL, double pObjetivo_mgL, double pActual_mgL)
        {
            double pNecesario = pObjetivo_mgL - pActual_mgL;

            if (hco3_mgL > UMBRAL_BICARBONATOS)
            {
                // Bicarbonatos altos: Usar ácido fosfórico primero, luego ácido nítrico
                return "EstrategiaDosPasos";
            }
            else
            {
                if (pNecesario > 0)
                {
                    // Bicarbonatos bajos pero necesita P: Podría usar ácido fosfórico
                    return "OpcionAcidoFosforico";
                }
                else
                {
                    // Bicarbonatos bajos: Usar solo ácido nítrico
                    return "SoloAcidoNitrico";
                }
            }
        }

        public List<ResultadoCalculoAcido> CalcularRequerimientoAcido_MetodoIncrossi(
            double hco3_mgL,
            double pHObjetivo,
            double pObjetivo_mgL = 0,
            double pActual_mgL = 0)
        {
            var resultados = new List<ResultadoCalculoAcido>();
            string estrategia = SeleccionarEstrategiaAcido(hco3_mgL, pObjetivo_mgL, pActual_mgL);

            switch (estrategia)
            {
                case "EstrategiaDosPasos":
                    resultados.AddRange(CalcularAjusteAcidoDosPasos(hco3_mgL, pHObjetivo, pObjetivo_mgL, pActual_mgL));
                    break;

                case "OpcionAcidoFosforico":
                    resultados.Add(CalcularAjusteAcidoUnico("H3PO4", hco3_mgL, pHObjetivo, pObjetivo_mgL, pActual_mgL));
                    break;

                case "SoloAcidoNitrico":
                default:
                    resultados.Add(CalcularAjusteAcidoUnico("HNO3", hco3_mgL, pHObjetivo));
                    break;
            }

            return resultados;
        }

        private List<ResultadoCalculoAcido> CalcularAjusteAcidoDosPasos(
            double hco3_mgL,
            double pHObjetivo,
            double pObjetivo_mgL,
            double pActual_mgL)
        {
            var resultados = new List<ResultadoCalculoAcido>();

            // Paso 1: Usar ácido fosfórico para proporcionar P necesario
            double pNecesario = pObjetivo_mgL - pActual_mgL;
            if (pNecesario > 0)
            {
                var resultadoFosforico = CalcularAcidoFosforicoParaFosforo(pNecesario);
                resultados.Add(resultadoFosforico);

                // Calcular bicarbonatos restantes después del ácido fosfórico
                double hco3Restante = hco3_mgL - resultadoFosforico.BicarbonatosANeutralizar;

                // Paso 2: Usar ácido nítrico para bicarbonatos restantes
                if (hco3Restante > BICARBONATOS_BUFFER)
                {
                    var resultadoNitrico = CalcularAjusteAcidoUnico("HNO3", hco3Restante, pHObjetivo);
                    resultados.Add(resultadoNitrico);
                }
            }
            else
            {
                // Si no se necesita P, solo usar ácido nítrico
                resultados.Add(CalcularAjusteAcidoUnico("HNO3", hco3_mgL, pHObjetivo));
            }

            return resultados;
        }

        private ResultadoCalculoAcido CalcularAcidoFosforicoParaFosforo(double pNecesario_mgL)
        {
            var acido = acidos["H3PO4"];

            // Calcular concentración de ácido necesaria para proporcionar P
            // Fórmula: P = Acido_mgL × ElementoPM × Pureza / AcidoPM × 100
            // Reorganizada: Acido_mgL = P × AcidoPM × 100 / (ElementoPM × Pureza)
            double concentracionAcido_mgL = pNecesario_mgL * acido.PesoMolecular * 100.0 /
                                         (acido.PesoMolecularElemento * acido.Pureza);

            // Calcular volumen de ácido: Q = ConcentracionAcido / (Valencia × Densidad × Pureza/100)
            double volumenAcido_mlL = concentracionAcido_mgL / (acido.Valencia * acido.Densidad * (acido.Pureza / 100.0));

            // Calcular cuánto bicarbonato neutraliza esto
            double hNeutralizado = volumenAcido_mlL * acido.Valencia * acido.Densidad * (acido.Pureza / 100.0);

            return new ResultadoCalculoAcido
            {
                NombreAcido = acido.Nombre,
                VolumenAcido_mlL = volumenAcido_mlL,
                ConcentracionHidrogeno_mgL = hNeutralizado,
                ConcentracionHidrogeno_mmolL = hNeutralizado / acido.PesoMolecular,
                ContribucionNutriente_mgL = pNecesario_mgL,
                TipoNutriente = "P",
                BicarbonatosANeutralizar = hNeutralizado,
                BicarbonatosRestantes = BICARBONATOS_BUFFER
            };
        }

        private ResultadoCalculoAcido CalcularAjusteAcidoUnico(
            string tipoAcido,
            double hco3_mgL,
            double pHObjetivo,
            double pObjetivo_mgL = 0,
            double pActual_mgL = 0)
        {
            var acido = acidos[tipoAcido];

            // Método Incrossi: H+ = [HCO3-] / (1 + 10^(pH - 6.35))
            double bicarbonatoANeutralizar = Math.Max(0, hco3_mgL - BICARBONATOS_BUFFER);
            double hPlusRequerido_mgL = bicarbonatoANeutralizar / (1 + Math.Pow(10, pHObjetivo - 6.35));

            // Calcular volumen de ácido: Q = H+ × PM / (n × D × P/100)
            double volumenAcido_mlL = hPlusRequerido_mgL * acido.PesoMolecular /
                                   (acido.Valencia * acido.Densidad * (acido.Pureza / 100.0));

            // Cálculo alternativo sin PM para mg/L directo
            double volumenAcido_mlL_directo = hPlusRequerido_mgL /
                                          (acido.Valencia * acido.Densidad * (acido.Pureza / 100.0));

            // Calcular contribución de nutriente si aplica
            double contribucionNutriente = 0;
            if (tipoAcido == "HNO3")
            {
                // Calcular contribución de N: N = VolumenAcido × Densidad × Pureza × ElementoPM / AcidoPM
                contribucionNutriente = volumenAcido_mlL * acido.Densidad * (acido.Pureza / 100.0) *
                                     acido.PesoMolecularElemento / acido.PesoMolecular;
            }
            else if (tipoAcido == "H3PO4")
            {
                contribucionNutriente = pObjetivo_mgL - pActual_mgL;
            }

            return new ResultadoCalculoAcido
            {
                NombreAcido = acido.Nombre,
                VolumenAcido_mlL = volumenAcido_mlL_directo,
                ConcentracionHidrogeno_mgL = hPlusRequerido_mgL,
                ConcentracionHidrogeno_mmolL = hPlusRequerido_mgL / acido.PesoMolecular,
                ContribucionNutriente_mgL = contribucionNutriente,
                TipoNutriente = acido.ElementoProporcionado,
                BicarbonatosANeutralizar = bicarbonatoANeutralizar,
                BicarbonatosRestantes = BICARBONATOS_BUFFER
            };
        }

        public ResultadoTitracion CalcularDesdeTitracion(List<ReplicaTitracion> replicas, string tipoAcido = "HNO3")
        {
            var acido = acidos[tipoAcido];
            var resultado = new ResultadoTitracion { Replicas = replicas };

            // Calcular volumen promedio de ácido
            double volumenTotalAcido = 0;
            foreach (var replica in replicas)
            {
                replica.AcidoPorLitro_mlL = replica.AcidoUsado_ml / replica.VolumenAgua_L;
                volumenTotalAcido += replica.AcidoPorLitro_mlL;
            }
            resultado.VolumenAcidoPromedio_mlL = volumenTotalAcido / replicas.Count;

            // Calcular concentración de bicarbonato desde volumen de ácido
            // H+ = Q × n × D × P/100
            double hPlus_mgL = resultado.VolumenAcidoPromedio_mlL * acido.Valencia *
                              acido.Densidad * (acido.Pureza / 100.0);

            resultado.BicarbonatosCalculados_mgL = hPlus_mgL;
            resultado.BicarbonatosTotales_mgL = resultado.BicarbonatosCalculados_mgL + BICARBONATOS_BUFFER;

            return resultado;
        }

        public Dictionary<string, double> ObtenerPropiedadesAcido(string tipoAcido)
        {
            if (!acidos.ContainsKey(tipoAcido))
                return new Dictionary<string, double>();

            var acido = acidos[tipoAcido];
            return new Dictionary<string, double>
            {
                ["Pureza"] = acido.Pureza,
                ["Densidad"] = acido.Densidad,
                ["PesoMolecular"] = acido.PesoMolecular,
                ["Valencia"] = acido.Valencia,
                ["PesoMolecularElemento"] = acido.PesoMolecularElemento
            };
        }

        public bool RequiereAjusteAcido(double pHAgua, double pHObjetivo = 6.0)
        {
            return pHAgua > pHObjetivo;
        }

        public string ObtenerRecomendacionAcido(double hco3_mgL, double pActual_mgL, double pObjetivo_mgL)
        {
            string estrategia = SeleccionarEstrategiaAcido(hco3_mgL, pObjetivo_mgL, pActual_mgL);

            switch (estrategia)
            {
                case "EstrategiaDosPasos":
                    return $"Bicarbonatos altos ({hco3_mgL:F1} mg/L > 122 mg/L). " +
                           "Recomendado: Usar ácido fosfórico primero para requerimientos de P, luego ácido nítrico para ajuste de pH restante.";

                case "OpcionAcidoFosforico":
                    return $"Bicarbonatos moderados ({hco3_mgL:F1} mg/L) y P necesario. " +
                           "Opción: Usar ácido fosfórico para proporcionar P y ajustar pH simultáneamente.";

                case "SoloAcidoNitrico":
                default:
                    return $"Bicarbonatos bajos ({hco3_mgL:F1} mg/L < 122 mg/L). " +
                           "Recomendado: Usar ácido nítrico para ajuste de pH.";
            }
        }
    }
}