using System;
using System.Collections.Generic;
using System.Linq;
#pragma warning disable CS8618

namespace CalculadoraHidroponica.Modulos
{
    public class ParametrosCalidadAgua
    {
        public double pH { get; set; }
        public double CE { get; set; } // dS/m
        public double Temperatura { get; set; } = 20.0; // °C
        public double HCO3 { get; set; } // mg/L
        public Dictionary<string, double> Elementos_mgL { get; set; } = new Dictionary<string, double>();
        public Dictionary<string, double> Elementos_mmolL { get; set; } = new Dictionary<string, double>();
        public Dictionary<string, double> Elementos_meqL { get; set; } = new Dictionary<string, double>();
    }

    public class ResultadoCalidadAgua
    {
        public string Parametro { get; set; }
        public double Valor { get; set; }
        public string Unidad { get; set; }
        public string Estado { get; set; } // "Óptimo", "Alto", "Bajo"
        public string ColorEstado { get; set; } // "Verde", "Rojo", "Amarillo"
        public double RangoMinimo { get; set; }
        public double RangoMaximo { get; set; }
        public string Recomendacion { get; set; }
    }

    public class IndicesCalidadAgua
    {
        public double RSC { get; set; } // Carbonato de Sodio Residual
        public double SAR { get; set; } // Relación de Adsorción de Sodio
        public double SARo { get; set; } // SAR corregido
        public double PSI { get; set; } // Porcentaje de Sodio Intercambiable
        public double IndiceBScott { get; set; }
        public double RatioCaMg { get; set; }
        public double SalesTotales_Analizadas { get; set; }
        public double SalesTotales_Estimadas { get; set; }
        public double DiferenciaSales { get; set; }
        public double ToleranciasSales { get; set; }
    }

    public class ModuloAnalisisAgua
    {
        private Dictionary<string, DatosElemento> datosElementos;
        private Dictionary<string, (double min, double max)> rangosOptimos;

        public ModuloAnalisisAgua()
        {
            InicializarDatosElementos();
            InicializarRangosOptimos();
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
                ["SO4"] = new DatosElemento { PesoAtomico = 96.06, Valencia = 2, EsCation = false },
                ["Cl"] = new DatosElemento { PesoAtomico = 35.45, Valencia = 1, EsCation = false },
                ["H2PO4"] = new DatosElemento { PesoAtomico = 96.99, Valencia = 1, EsCation = false },
                ["HCO3"] = new DatosElemento { PesoAtomico = 61.02, Valencia = 1, EsCation = false }
            };
        }

        private void InicializarRangosOptimos()
        {
            rangosOptimos = new Dictionary<string, (double min, double max)>
            {
                ["pH"] = (5.5, 6.5),
                ["CE"] = (0.1, 0.8),
                ["Ca"] = (0, 100),
                ["K"] = (0, 20),
                ["Mg"] = (0, 50),
                ["Na"] = (0, 50),
                ["NO3"] = (0, 10),
                ["SO4"] = (0, 200),
                ["Cl"] = (0, 100),
                ["HCO3"] = (30.5, 122)
            };
        }

        public ParametrosCalidadAgua AnalizarAgua(ParametrosCalidadAgua datosAgua)
        {
            // Convertir unidades y calcular valores derivados
            foreach (var elemento in datosAgua.Elementos_mgL)
            {
                if (datosElementos.ContainsKey(elemento.Key))
                {
                    var pesoAtomico = datosElementos[elemento.Key].PesoAtomico;
                    var valencia = datosElementos[elemento.Key].Valencia;

                    // Convertir mg/L a mmol/L
                    datosAgua.Elementos_mmolL[elemento.Key] = elemento.Value / pesoAtomico;

                    // Convertir mmol/L a meq/L
                    datosAgua.Elementos_meqL[elemento.Key] = datosAgua.Elementos_mmolL[elemento.Key] * valencia;
                }
            }

            return datosAgua;
        }

        public List<ResultadoCalidadAgua> EvaluarCalidadAgua(ParametrosCalidadAgua datosAgua)
        {
            var resultados = new List<ResultadoCalidadAgua>();

            // Evaluar pH
            resultados.Add(EvaluarParametro("pH", datosAgua.pH, "unidades pH", rangosOptimos["pH"]));

            // Evaluar CE
            resultados.Add(EvaluarParametro("CE", datosAgua.CE, "dS/m", rangosOptimos["CE"]));

            // Evaluar elementos principales
            foreach (var elemento in datosAgua.Elementos_mgL)
            {
                if (rangosOptimos.ContainsKey(elemento.Key))
                {
                    resultados.Add(EvaluarParametro(elemento.Key, elemento.Value, "mg/L", rangosOptimos[elemento.Key]));
                }
            }

            return resultados;
        }

        private ResultadoCalidadAgua EvaluarParametro(string parametro, double valor, string unidad, (double min, double max) rango)
        {
            var resultado = new ResultadoCalidadAgua
            {
                Parametro = parametro,
                Valor = valor,
                Unidad = unidad,
                RangoMinimo = rango.min,
                RangoMaximo = rango.max
            };

            if (valor >= rango.min && valor <= rango.max)
            {
                resultado.Estado = "Óptimo";
                resultado.ColorEstado = "Verde";
                resultado.Recomendacion = "Dentro del rango óptimo";
            }
            else if (valor > rango.max)
            {
                resultado.Estado = "Alto";
                resultado.ColorEstado = "Rojo";
                resultado.Recomendacion = $"Concentración demasiado alta. Considere tratamiento o dilución.";
            }
            else
            {
                resultado.Estado = "Bajo";
                resultado.ColorEstado = "Amarillo";
                resultado.Recomendacion = $"Concentración baja. Monitorear durante fertilización.";
            }

            return resultado;
        }

        public IndicesCalidadAgua CalcularIndicesCalidadAgua(ParametrosCalidadAgua datosAgua)
        {
            var indices = new IndicesCalidadAgua();

            // Obtener valores en meq/L
            double ca_meq = datosAgua.Elementos_meqL.GetValueOrDefault("Ca", 0);
            double mg_meq = datosAgua.Elementos_meqL.GetValueOrDefault("Mg", 0);
            double na_meq = datosAgua.Elementos_meqL.GetValueOrDefault("Na", 0);
            double k_meq = datosAgua.Elementos_meqL.GetValueOrDefault("K", 0);
            double hco3_meq = datosAgua.Elementos_meqL.GetValueOrDefault("HCO3", 0);
            double so4_meq = datosAgua.Elementos_meqL.GetValueOrDefault("SO4", 0);
            double cl_meq = datosAgua.Elementos_meqL.GetValueOrDefault("Cl", 0);

            // Carbonato de Sodio Residual (RSC) = (HCO3 + CO3) - (Ca + Mg)
            indices.RSC = hco3_meq - (ca_meq + mg_meq);

            // Relación de Adsorción de Sodio (SAR) = Na / sqrt((Ca + Mg)/2)
            if ((ca_meq + mg_meq) > 0)
            {
                indices.SAR = na_meq / Math.Sqrt((ca_meq + mg_meq) / 2.0);
            }

            // SAR corregido (SARo) - cálculo simplificado
            indices.SARo = indices.SAR * (1 + (8.4 - datosAgua.pH));

            // Porcentaje de Sodio Intercambiable (PSI) = Na / (Ca + Mg + Na + K) * 100
            double totalCationes = ca_meq + mg_meq + na_meq + k_meq;
            if (totalCationes > 0)
            {
                indices.PSI = (na_meq / totalCationes) * 100.0;
            }

            // Índice de Scott = (Cl + SO4) / (HCO3 + CO3)
            if (hco3_meq > 0)
            {
                indices.IndiceBScott = (cl_meq + so4_meq) / hco3_meq;
            }

            // Relación Ca/Mg
            if (mg_meq > 0)
            {
                indices.RatioCaMg = ca_meq / mg_meq;
            }

            // Calcular sales totales
            indices.SalesTotales_Analizadas = CalcularSalesTotales(datosAgua);
            indices.SalesTotales_Estimadas = datosAgua.CE * 640; // mg/L, aproximación
            indices.DiferenciaSales = Math.Abs(indices.SalesTotales_Analizadas - indices.SalesTotales_Estimadas);
            indices.ToleranciasSales = indices.SalesTotales_Estimadas * 0.1; // Tolerancia del 10%

            return indices;
        }

        private double CalcularSalesTotales(ParametrosCalidadAgua datosAgua)
        {
            double total = 0;
            foreach (var elemento in datosAgua.Elementos_mgL)
            {
                total += elemento.Value;
            }
            return total;
        }

        public Dictionary<string, double> VerificarBalanceIonico(ParametrosCalidadAgua datosAgua)
        {
            var resultados = new Dictionary<string, double>();

            double sumaCationes = 0;
            double sumaAniones = 0;

            foreach (var ion in datosAgua.Elementos_meqL)
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

            return resultados;
        }
    }
}