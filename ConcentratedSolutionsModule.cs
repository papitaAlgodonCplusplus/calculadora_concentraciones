using System;
using System.Collections.Generic;
using System.Linq;
#pragma warning disable CS8618

namespace CalculadoraHidroponica.Modulos
{
    public enum NivelCompatibilidad
    {
        C = 0, // 100% compatible seco y en agua
        I = 1, // Incompatible seco y en agua
        E = 2, // Compatible solo en agua al momento de inyección
        L = 3, // Compatibilidad limitada - usar cantidades limitadas
        P = 4, // Genera calor - peligroso, agregar ácido al agua
        S = 5  // Solubilidad limitada
    }

    public class CompatibilidadFertilizante
    {
        public string Fertilizante1 { get; set; } = "";
        public string Fertilizante2 { get; set; } = "";
        public NivelCompatibilidad Compatibilidad { get; set; }
        public string Descripcion { get; set; } = "";
        public string Recomendacion { get; set; } = "";
    }

    public class SolubilidadFertilizante
    {
        public string Nombre { get; set; } = "";
        public string Formula { get; set; } = "";
        public double Solubilidad_0C { get; set; } // g/L a 0°C
        public double Solubilidad_20C { get; set; } // g/L a 20°C
        public double Solubilidad_40C { get; set; } // g/L a 40°C
        public double LimiteConcentracionSegura { get; set; } // 50% del límite de solubilidad
        public double ConcentracionMaximaRecomendada { get; set; } // 80% del límite seguro
    }

    public class FactorConcentracion
    {
        public string Descripcion { get; set; } = "";
        public int Factor { get; set; } // ej., 200 para 1:200
        public double Porcentaje { get; set; } // ej., 0.5% para 1:200
        public double TasaInyeccion_LporM3 { get; set; } // L/m³
        public string Aplicacion { get; set; } = ""; // Tipo de aplicación recomendada
    }

    public class DistribucionTanque
    {
        public int NumeroTanque { get; set; }
        public string EtiquetaTanque { get; set; } = "";
        public List<string> Fertilizantes { get; set; } = new List<string>();
        public List<string> Acidos { get; set; } = new List<string>();
        public Dictionary<string, double> Concentraciones_gL { get; set; } = new Dictionary<string, double>();
        public Dictionary<string, double> CantidadesFertilizante_kg { get; set; } = new Dictionary<string, double>();
        public double DensidadTotal_gL { get; set; }
        public double Volumen_L { get; set; }
        public List<string> AdvertenciasCompatibilidad { get; set; } = new List<string>();
        public List<string> AdvertenciasSolubilidad { get; set; } = new List<string>();
        public List<string> InstruccionesPreparacion { get; set; } = new List<string>();
        public double CostoEstimado { get; set; }
        public string ColorTanque { get; set; } = ""; // Para identificación visual
    }

    public class ReporteSolucionConcentrada
    {
        public int NumeroDeTanques { get; set; }
        public FactorConcentracion FactorConcentracion { get; set; } = new FactorConcentracion();
        public List<DistribucionTanque> Tanques { get; set; } = new List<DistribucionTanque>();
        public Dictionary<string, double> RequerimientosVolumen { get; set; } = new Dictionary<string, double>();
        public double CostoTotal { get; set; }
        public double CostoPorM3_SolucionDiluida { get; set; }
        public List<string> AdvertenciasCriticas { get; set; } = new List<string>();
        public List<string> RecomendacionesGenerales { get; set; } = new List<string>();
        public Dictionary<string, double> TotalesFertilizante_kg { get; set; } = new Dictionary<string, double>();
        public DateTime FechaCalculo { get; set; } = DateTime.Now;
    }

    public class ModuloSolucionesConcentradas
    {
        private Dictionary<string, SolubilidadFertilizante> solubilidadesFertilizantes;
        private Dictionary<(string, string), NivelCompatibilidad> matrizCompatibilidad;
        private List<FactorConcentracion> factoresConcentracion;
        private Dictionary<string, double> costosFertilizantes; // Costo por kg

        public ModuloSolucionesConcentradas()
        {
            InicializarSolubilidadesFertilizantes();
            InicializarMatrizCompatibilidad();
            InicializarFactoresConcentracion();
            InicializarCostosFertilizantes();
        }

        private void InicializarSolubilidadesFertilizantes()
        {
            solubilidadesFertilizantes = new Dictionary<string, SolubilidadFertilizante>
            {
                ["NH4NO3"] = new SolubilidadFertilizante
                {
                    Nombre = "Nitrato de Amonio",
                    Formula = "NH4NO3",
                    Solubilidad_0C = 1800,
                    Solubilidad_20C = 1900,
                    Solubilidad_40C = 2190,
                    LimiteConcentracionSegura = 950, // 50% de solubilidad a 20°C
                    ConcentracionMaximaRecomendada = 760 // 80% del límite seguro
                },
                ["(NH4)2SO4"] = new SolubilidadFertilizante
                {
                    Nombre = "Sulfato de Amonio",
                    Formula = "(NH4)2SO4",
                    Solubilidad_0C = 700,
                    Solubilidad_20C = 760,
                    Solubilidad_40C = 760,
                    LimiteConcentracionSegura = 380,
                    ConcentracionMaximaRecomendada = 304
                },
                ["Ca(NO3)2.2H2O"] = new SolubilidadFertilizante
                {
                    Nombre = "Nitrato de Calcio",
                    Formula = "Ca(NO3)2·2H2O",
                    Solubilidad_0C = 1200,
                    Solubilidad_20C = 1200,
                    Solubilidad_40C = 1200,
                    LimiteConcentracionSegura = 600,
                    ConcentracionMaximaRecomendada = 480
                },
                ["KNO3"] = new SolubilidadFertilizante
                {
                    Nombre = "Nitrato de Potasio",
                    Formula = "KNO3",
                    Solubilidad_0C = 130,
                    Solubilidad_20C = 335,
                    Solubilidad_40C = 630,
                    LimiteConcentracionSegura = 167,
                    ConcentracionMaximaRecomendada = 134
                },
                ["NH4H2PO4"] = new SolubilidadFertilizante
                {
                    Nombre = "Fosfato Monoamónico",
                    Formula = "NH4H2PO4",
                    Solubilidad_0C = 225,
                    Solubilidad_20C = 400,
                    Solubilidad_40C = 818,
                    LimiteConcentracionSegura = 200,
                    ConcentracionMaximaRecomendada = 160
                },
                ["(NH4)2HPO4"] = new SolubilidadFertilizante
                {
                    Nombre = "Fosfato Diamónico",
                    Formula = "(NH4)2HPO4",
                    Solubilidad_0C = 575,
                    Solubilidad_20C = 400,
                    Solubilidad_40C = 818,
                    LimiteConcentracionSegura = 200,
                    ConcentracionMaximaRecomendada = 160
                },
                ["KH2PO4"] = new SolubilidadFertilizante
                {
                    Nombre = "Fosfato Monopotásico",
                    Formula = "KH2PO4",
                    Solubilidad_0C = 143,
                    Solubilidad_20C = 227,
                    Solubilidad_40C = 339,
                    LimiteConcentracionSegura = 113,
                    ConcentracionMaximaRecomendada = 90
                },
                ["K2HPO4.3H2O"] = new SolubilidadFertilizante
                {
                    Nombre = "Fosfato Dipotásico",
                    Formula = "K2HPO4·3H2O",
                    Solubilidad_0C = 1590,
                    Solubilidad_20C = 2125,
                    Solubilidad_40C = 2125,
                    LimiteConcentracionSegura = 1062,
                    ConcentracionMaximaRecomendada = 850
                },
                ["KCl"] = new SolubilidadFertilizante
                {
                    Nombre = "Cloruro de Potasio",
                    Formula = "KCl",
                    Solubilidad_0C = 282,
                    Solubilidad_20C = 342,
                    Solubilidad_40C = 403,
                    LimiteConcentracionSegura = 171,
                    ConcentracionMaximaRecomendada = 137
                },
                ["K2SO4"] = new SolubilidadFertilizante
                {
                    Nombre = "Sulfato de Potasio",
                    Formula = "K2SO4",
                    Solubilidad_0C = 74,
                    Solubilidad_20C = 111,
                    Solubilidad_40C = 148,
                    LimiteConcentracionSegura = 55,
                    ConcentracionMaximaRecomendada = 44
                },
                ["MgSO4.7H2O"] = new SolubilidadFertilizante
                {
                    Nombre = "Sulfato de Magnesio",
                    Formula = "MgSO4·7H2O",
                    Solubilidad_0C = 710,
                    Solubilidad_20C = 710,
                    Solubilidad_40C = 710,
                    LimiteConcentracionSegura = 355,
                    ConcentracionMaximaRecomendada = 284
                },
                ["MgCl2.6H2O"] = new SolubilidadFertilizante
                {
                    Nombre = "Cloruro de Magnesio",
                    Formula = "MgCl2·6H2O",
                    Solubilidad_0C = 528,
                    Solubilidad_20C = 546,
                    Solubilidad_40C = 575,
                    LimiteConcentracionSegura = 273,
                    ConcentracionMaximaRecomendada = 218
                },
                ["CaCl2.6H2O"] = new SolubilidadFertilizante
                {
                    Nombre = "Cloruro de Calcio",
                    Formula = "CaCl2·6H2O",
                    Solubilidad_0C = 600,
                    Solubilidad_20C = 600,
                    Solubilidad_40C = 600,
                    LimiteConcentracionSegura = 300,
                    ConcentracionMaximaRecomendada = 240
                },
                ["FeSO4.7H2O"] = new SolubilidadFertilizante
                {
                    Nombre = "Sulfato de Hierro",
                    Formula = "FeSO4·7H2O",
                    Solubilidad_0C = 155,
                    Solubilidad_20C = 260,
                    Solubilidad_40C = 650,
                    LimiteConcentracionSegura = 130,
                    ConcentracionMaximaRecomendada = 104
                },
                ["CuSO4.5H2O"] = new SolubilidadFertilizante
                {
                    Nombre = "Sulfato de Cobre",
                    Formula = "CuSO4·5H2O",
                    Solubilidad_0C = 316,
                    Solubilidad_20C = 316,
                    Solubilidad_40C = 316,
                    LimiteConcentracionSegura = 158,
                    ConcentracionMaximaRecomendada = 126
                },
                ["MnSO4.4H2O"] = new SolubilidadFertilizante
                {
                    Nombre = "Sulfato de Manganeso",
                    Formula = "MnSO4·4H2O",
                    Solubilidad_0C = 1053,
                    Solubilidad_20C = 1053,
                    Solubilidad_40C = 1053,
                    LimiteConcentracionSegura = 526,
                    ConcentracionMaximaRecomendada = 421
                },
                ["ZnSO4.7H2O"] = new SolubilidadFertilizante
                {
                    Nombre = "Sulfato de Zinc",
                    Formula = "ZnSO4·7H2O",
                    Solubilidad_0C = 750,
                    Solubilidad_20C = 750,
                    Solubilidad_40C = 750,
                    LimiteConcentracionSegura = 375,
                    ConcentracionMaximaRecomendada = 300
                },
                ["H3BO3"] = new SolubilidadFertilizante
                {
                    Nombre = "Ácido Bórico",
                    Formula = "H3BO3",
                    Solubilidad_0C = 63.5,
                    Solubilidad_20C = 63.5,
                    Solubilidad_40C = 63.5,
                    LimiteConcentracionSegura = 31.7,
                    ConcentracionMaximaRecomendada = 25.4
                },
                ["FeEDTA"] = new SolubilidadFertilizante
                {
                    Nombre = "Quelato de Hierro EDTA",
                    Formula = "FeEDTA",
                    Solubilidad_0C = 1000,
                    Solubilidad_20C = 1000,
                    Solubilidad_40C = 1000,
                    LimiteConcentracionSegura = 500,
                    ConcentracionMaximaRecomendada = 400
                },
                ["Na2MoO4.2H2O"] = new SolubilidadFertilizante
                {
                    Nombre = "Molibdato de Sodio",
                    Formula = "Na2MoO4·2H2O",
                    Solubilidad_0C = 840,
                    Solubilidad_20C = 840,
                    Solubilidad_40C = 840,
                    LimiteConcentracionSegura = 420,
                    ConcentracionMaximaRecomendada = 336
                }
            };
        }

        private void InicializarMatrizCompatibilidad()
        {
            matrizCompatibilidad = new Dictionary<(string, string), NivelCompatibilidad>();

            // Definir combinaciones incompatibles clave basadas en la tabla de compatibilidad de la presentación
            var paresIncompatibles = new List<(string, string)>
            {
                ("Ca(NO3)2.2H2O", "K2SO4"),
                ("Ca(NO3)2.2H2O", "MgSO4.7H2O"),
                ("Ca(NO3)2.2H2O", "(NH4)2SO4"),
                ("Ca(NO3)2.2H2O", "NH4H2PO4"),
                ("Ca(NO3)2.2H2O", "KH2PO4"),
                ("Ca(NO3)2.2H2O", "(NH4)2HPO4")
            };

            // Pares de compatibilidad limitada
            var paresCompatibilidadLimitada = new List<(string, string)>
            {
                ("KNO3", "KH2PO4"),
                ("NH4NO3", "NH4H2PO4"),
                ("K2SO4", "KH2PO4")
            };

            // Compatible solo en agua
            var paresSoloEnAgua = new List<(string, string)>
            {
                ("MgSO4.7H2O", "KH2PO4"),
                ("(NH4)2SO4", "KH2PO4"),
                ("FeSO4.7H2O", "Ca(NO3)2.2H2O")
            };

            // Lista de todos los fertilizantes
            var fertilizantes = solubilidadesFertilizantes.Keys.ToList();

            // Inicializar todos como compatibles por defecto
            foreach (var fert1 in fertilizantes)
            {
                foreach (var fert2 in fertilizantes)
                {
                    if (fert1 != fert2)
                    {
                        matrizCompatibilidad[(fert1, fert2)] = NivelCompatibilidad.C;
                    }
                }
            }

            // Establecer pares incompatibles
            foreach (var par in paresIncompatibles)
            {
                matrizCompatibilidad[(par.Item1, par.Item2)] = NivelCompatibilidad.I;
                matrizCompatibilidad[(par.Item2, par.Item1)] = NivelCompatibilidad.I;
            }

            // Establecer pares de compatibilidad limitada
            foreach (var par in paresCompatibilidadLimitada)
            {
                matrizCompatibilidad[(par.Item1, par.Item2)] = NivelCompatibilidad.L;
                matrizCompatibilidad[(par.Item2, par.Item1)] = NivelCompatibilidad.L;
            }

            // Establecer pares de compatibilidad solo en agua
            foreach (var par in paresSoloEnAgua)
            {
                matrizCompatibilidad[(par.Item1, par.Item2)] = NivelCompatibilidad.E;
                matrizCompatibilidad[(par.Item2, par.Item1)] = NivelCompatibilidad.E;
            }
        }

        private void InicializarFactoresConcentracion()
        {
            factoresConcentracion = new List<FactorConcentracion>
            {
                new FactorConcentracion { Descripcion = "1:40", Factor = 40, Porcentaje = 2.5, TasaInyeccion_LporM3 = 25, Aplicacion = "Sistemas de alta precisión" },
                new FactorConcentracion { Descripcion = "1:50", Factor = 50, Porcentaje = 2.0, TasaInyeccion_LporM3 = 20, Aplicacion = "Sistemas hidropónicos estándar" },
                new FactorConcentracion { Descripcion = "1:66", Factor = 66, Porcentaje = 1.5, TasaInyeccion_LporM3 = 15, Aplicacion = "Sistemas de concentración media" },
                new FactorConcentracion { Descripcion = "1:100", Factor = 100, Porcentaje = 1.0, TasaInyeccion_LporM3 = 10, Aplicacion = "Sistemas de propósito general" },
                new FactorConcentracion { Descripcion = "1:133", Factor = 133, Porcentaje = 0.75, TasaInyeccion_LporM3 = 7.5, Aplicacion = "Sistemas de baja concentración" },
                new FactorConcentracion { Descripcion = "1:200", Factor = 200, Porcentaje = 0.5, TasaInyeccion_LporM3 = 5, Aplicacion = "Sistemas de alto volumen" },
                new FactorConcentracion { Descripcion = "1:400", Factor = 400, Porcentaje = 0.25, TasaInyeccion_LporM3 = 2.5, Aplicacion = "Sistemas de muy alto volumen" }
            };
        }

        private void InicializarCostosFertilizantes()
        {
            costosFertilizantes = new Dictionary<string, double>
            {
                ["NH4NO3"] = 0.45,
                ["(NH4)2SO4"] = 0.50,
                ["Ca(NO3)2.2H2O"] = 0.80,
                ["KNO3"] = 1.20,
                ["NH4H2PO4"] = 1.80,
                ["(NH4)2HPO4"] = 1.90,
                ["KH2PO4"] = 2.50,
                ["K2HPO4.3H2O"] = 2.80,
                ["KCl"] = 0.60,
                ["K2SO4"] = 1.50,
                ["MgSO4.7H2O"] = 0.60,
                ["MgCl2.6H2O"] = 0.70,
                ["CaCl2.6H2O"] = 0.70,
                ["FeSO4.7H2O"] = 1.80,
                ["CuSO4.5H2O"] = 4.20,
                ["MnSO4.4H2O"] = 2.80,
                ["ZnSO4.7H2O"] = 3.50,
                ["H3BO3"] = 3.20,
                ["FeEDTA"] = 8.50,
                ["Na2MoO4.2H2O"] = 15.00
            };
        }

        public int CalcularFactorConcentracion(double flujoTotal_Lh, double flujoInyeccion_Lh)
        {
            if (flujoInyeccion_Lh <= 0) return 100; // Factor por defecto
            return (int)Math.Round(flujoTotal_Lh / flujoInyeccion_Lh);
        }

        public FactorConcentracion ObtenerFactorConcentracion(int factor)
        {
            return factoresConcentracion.FirstOrDefault(cf => cf.Factor == factor) ??
                   new FactorConcentracion { Descripcion = $"1:{factor}", Factor = factor, Porcentaje = 100.0 / factor, TasaInyeccion_LporM3 = 1000.0 / factor };
        }

        public List<FactorConcentracion> ObtenerFactoresConcentracionDisponibles()
        {
            return new List<FactorConcentracion>(factoresConcentracion);
        }

        public List<DistribucionTanque> DistribuirFertilizantes(
            Dictionary<string, double> concentracionesFertilizantes_mgL,
            List<string> tiposAcidos,
            int numeroDeTanques,
            int factorConcentracion,
            double volumenTanque_L = 200)
        {
            var tanques = new List<DistribucionTanque>();

            // Inicializar tanques con colores para identificación
            var coloresTanques = new[] { "Azul", "Verde", "Rojo", "Amarillo", "Naranja", "Púrpura", "Marrón", "Rosa" };

            for (int i = 0; i < numeroDeTanques; i++)
            {
                tanques.Add(new DistribucionTanque
                {
                    NumeroTanque = i + 1,
                    EtiquetaTanque = ObtenerEtiquetaTanque(i),
                    Volumen_L = volumenTanque_L,
                    ColorTanque = coloresTanques[i % coloresTanques.Length]
                });
            }

            // Estrategia basada en el número de tanques
            switch (numeroDeTanques)
            {
                case 2:
                    DistribuirEnDosTanques(tanques, concentracionesFertilizantes_mgL, tiposAcidos, factorConcentracion);
                    break;
                case 3:
                    DistribuirEnTresTanques(tanques, concentracionesFertilizantes_mgL, tiposAcidos, factorConcentracion);
                    break;
                case 4:
                    DistribuirEnCuatroTanques(tanques, concentracionesFertilizantes_mgL, tiposAcidos, factorConcentracion);
                    break;
                default:
                    DistribuirEnMultiplesTanques(tanques, concentracionesFertilizantes_mgL, tiposAcidos, factorConcentracion);
                    break;
            }

            // Verificar compatibilidad y solubilidad para cada tanque
            foreach (var tanque in tanques)
            {
                VerificarCompatibilidadTanque(tanque);
                VerificarSolubilidadTanque(tanque, factorConcentracion);
                CalcularDensidadTanque(tanque);
                CalcularCostoTanque(tanque);
                GenerarInstruccionesPreparacion(tanque);
            }

            return tanques;
        }

        private void DistribuirEnDosTanques(List<DistribucionTanque> tanques, Dictionary<string, double> fertilizantes, List<string> acidos, int factorConcentracion)
        {
            // Tanque A: Fosfatos, Potasio, Magnesio, Micronutrientes (evitar calcio)
            var tanqueA = tanques[0];
            tanqueA.EtiquetaTanque = "A - NPK + Micros";

            var fertilizantesTanqueA = new[] { "KH2PO4", "KNO3", "K2SO4", "MgSO4.7H2O", "FeSO4.7H2O", "FeEDTA", "MnSO4.4H2O", "ZnSO4.7H2O", "CuSO4.5H2O", "H3BO3", "Na2MoO4.2H2O" };
            foreach (var fert in fertilizantesTanqueA)
            {
                if (fertilizantes.ContainsKey(fert))
                {
                    tanqueA.Fertilizantes.Add(fert);
                    double cantidadConcentrada = fertilizantes[fert] * factorConcentracion / 1000.0; // Convertir a g/L
                    tanqueA.Concentraciones_gL[fert] = cantidadConcentrada;
                    tanqueA.CantidadesFertilizante_kg[fert] = (cantidadConcentrada * tanqueA.Volumen_L) / 1000.0; // Convertir a kg
                }
            }

            // Tanque B: Calcio y Ácidos (separado de fosfatos y sulfatos)
            var tanqueB = tanques[1];
            tanqueB.EtiquetaTanque = "B - Calcio + Ácidos";

            var fertilizantesTanqueB = new[] { "Ca(NO3)2.2H2O", "CaCl2.6H2O" };
            foreach (var fert in fertilizantesTanqueB)
            {
                if (fertilizantes.ContainsKey(fert))
                {
                    tanqueB.Fertilizantes.Add(fert);
                    double cantidadConcentrada = fertilizantes[fert] * factorConcentracion / 1000.0;
                    tanqueB.Concentraciones_gL[fert] = cantidadConcentrada;
                    tanqueB.CantidadesFertilizante_kg[fert] = (cantidadConcentrada * tanqueB.Volumen_L) / 1000.0;
                }
            }

            tanqueB.Acidos.AddRange(acidos);
        }

        private void DistribuirEnTresTanques(List<DistribucionTanque> tanques, Dictionary<string, double> fertilizantes, List<string> acidos, int factorConcentracion)
        {
            // Tanque A: Fosfatos y compuestos de potasio compatibles
            var tanqueA = tanques[0];
            tanqueA.EtiquetaTanque = "A - Fosfatos + K";

            var fertilizantesTanqueA = new[] { "KH2PO4", "KNO3", "K2SO4", "MgSO4.7H2O" };
            foreach (var fert in fertilizantesTanqueA)
            {
                if (fertilizantes.ContainsKey(fert))
                {
                    tanqueA.Fertilizantes.Add(fert);
                    double cantidadConcentrada = fertilizantes[fert] * factorConcentracion / 1000.0;
                    tanqueA.Concentraciones_gL[fert] = cantidadConcentrada;
                    tanqueA.CantidadesFertilizante_kg[fert] = (cantidadConcentrada * tanqueA.Volumen_L) / 1000.0;
                }
            }

            // Tanque B: Micronutrientes
            var tanqueB = tanques[1];
            tanqueB.EtiquetaTanque = "B - Micronutrientes";

            var fertilizantesTanqueB = new[] { "FeSO4.7H2O", "FeEDTA", "MnSO4.4H2O", "ZnSO4.7H2O", "CuSO4.5H2O", "H3BO3", "Na2MoO4.2H2O" };
            foreach (var fert in fertilizantesTanqueB)
            {
                if (fertilizantes.ContainsKey(fert))
                {
                    tanqueB.Fertilizantes.Add(fert);
                    double cantidadConcentrada = fertilizantes[fert] * factorConcentracion / 1000.0;
                    tanqueB.Concentraciones_gL[fert] = cantidadConcentrada;
                    tanqueB.CantidadesFertilizante_kg[fert] = (cantidadConcentrada * tanqueB.Volumen_L) / 1000.0;
                }
            }

            // Tanque C: Calcio y Ácidos
            var tanqueC = tanques[2];
            tanqueC.EtiquetaTanque = "C - Calcio + Ácidos";

            var fertilizantesTanqueC = new[] { "Ca(NO3)2.2H2O", "CaCl2.6H2O" };
            foreach (var fert in fertilizantesTanqueC)
            {
                if (fertilizantes.ContainsKey(fert))
                {
                    tanqueC.Fertilizantes.Add(fert);
                    double cantidadConcentrada = fertilizantes[fert] * factorConcentracion / 1000.0;
                    tanqueC.Concentraciones_gL[fert] = cantidadConcentrada;
                    tanqueC.CantidadesFertilizante_kg[fert] = (cantidadConcentrada * tanqueC.Volumen_L) / 1000.0;
                }
            }

            tanqueC.Acidos.AddRange(acidos);
        }

        private void DistribuirEnCuatroTanques(List<DistribucionTanque> tanques, Dictionary<string, double> fertilizantes, List<string> acidos, int factorConcentracion)
        {
            // Tanque A: Fosfatos y algo de Potasio
            var tanqueA = tanques[0];
            tanqueA.EtiquetaTanque = "A - Fosfatos";

            var fertilizantesTanqueA = new[] { "KH2PO4", "NH4H2PO4", "(NH4)2HPO4", "K2HPO4.3H2O" };
            foreach (var fert in fertilizantesTanqueA)
            {
                if (fertilizantes.ContainsKey(fert))
                {
                    tanqueA.Fertilizantes.Add(fert);
                    double cantidadConcentrada = fertilizantes[fert] * factorConcentracion / 1000.0;
                    tanqueA.Concentraciones_gL[fert] = cantidadConcentrada;
                    tanqueA.CantidadesFertilizante_kg[fert] = (cantidadConcentrada * tanqueA.Volumen_L) / 1000.0;
                }
            }

            // Tanque B: Potasio y Magnesio
            var tanqueB = tanques[1];
            tanqueB.EtiquetaTanque = "B - K + Mg";

            var fertilizantesTanqueB = new[] { "KNO3", "K2SO4", "KCl", "MgSO4.7H2O", "MgCl2.6H2O" };
            foreach (var fert in fertilizantesTanqueB)
            {
                if (fertilizantes.ContainsKey(fert))
                {
                    tanqueB.Fertilizantes.Add(fert);
                    double cantidadConcentrada = fertilizantes[fert] * factorConcentracion / 1000.0;
                    tanqueB.Concentraciones_gL[fert] = cantidadConcentrada;
                    tanqueB.CantidadesFertilizante_kg[fert] = (cantidadConcentrada * tanqueB.Volumen_L) / 1000.0;
                }
            }

            // Tanque C: Calcio
            var tanqueC = tanques[2];
            tanqueC.EtiquetaTanque = "C - Calcio";

            var fertilizantesTanqueC = new[] { "Ca(NO3)2.2H2O", "CaCl2.6H2O" };
            foreach (var fert in fertilizantesTanqueC)
            {
                if (fertilizantes.ContainsKey(fert))
                {
                    tanqueC.Fertilizantes.Add(fert);
                    double cantidadConcentrada = fertilizantes[fert] * factorConcentracion / 1000.0;
                    tanqueC.Concentraciones_gL[fert] = cantidadConcentrada;
                    tanqueC.CantidadesFertilizante_kg[fert] = (cantidadConcentrada * tanqueC.Volumen_L) / 1000.0;
                }
            }

            // Tanque D: Micronutrientes y Ácidos
            var tanqueD = tanques[3];
            tanqueD.EtiquetaTanque = "D - Micros + Ácidos";

            var fertilizantesTanqueD = new[] { "FeSO4.7H2O", "FeEDTA", "MnSO4.4H2O", "ZnSO4.7H2O", "CuSO4.5H2O", "H3BO3", "Na2MoO4.2H2O" };
            foreach (var fert in fertilizantesTanqueD)
            {
                if (fertilizantes.ContainsKey(fert))
                {
                    tanqueD.Fertilizantes.Add(fert);
                    double cantidadConcentrada = fertilizantes[fert] * factorConcentracion / 1000.0;
                    tanqueD.Concentraciones_gL[fert] = cantidadConcentrada;
                    tanqueD.CantidadesFertilizante_kg[fert] = (cantidadConcentrada * tanqueD.Volumen_L) / 1000.0;
                }
            }

            tanqueD.Acidos.AddRange(acidos);
        }

        private void DistribuirEnMultiplesTanques(List<DistribucionTanque> tanques, Dictionary<string, double> fertilizantes, List<string> acidos, int factorConcentracion)
        {
            // Para 5+ tanques, distribuir cada grupo principal de fertilizantes por separado
            int indiceTanque = 0;

            // Agrupar fertilizantes por compatibilidad
            var gruposFertilizantes = new[]
            {
                new { Nombre = "Nitratos", Fertilizantes = new[] { "NH4NO3", "Ca(NO3)2.2H2O", "KNO3" } },
                new { Nombre = "Fosfatos", Fertilizantes = new[] { "KH2PO4", "NH4H2PO4", "(NH4)2HPO4" } },
                new { Nombre = "Sulfatos", Fertilizantes = new[] { "K2SO4", "MgSO4.7H2O", "(NH4)2SO4" } },
                new { Nombre = "Cloruros", Fertilizantes = new[] { "KCl", "CaCl2.6H2O", "MgCl2.6H2O" } },
                new { Nombre = "Micronutrientes", Fertilizantes = new[] { "FeSO4.7H2O", "FeEDTA", "MnSO4.4H2O", "ZnSO4.7H2O", "CuSO4.5H2O", "H3BO3", "Na2MoO4.2H2O" } }
            };

            foreach (var grupo in gruposFertilizantes)
            {
                if (indiceTanque >= tanques.Count - 1) break; // Reservar último tanque para ácidos

                var tanque = tanques[indiceTanque];
                tanque.EtiquetaTanque = $"{(char)('A' + indiceTanque)} - {grupo.Nombre}";

                foreach (var fert in grupo.Fertilizantes)
                {
                    if (fertilizantes.ContainsKey(fert))
                    {
                        tanque.Fertilizantes.Add(fert);
                        double cantidadConcentrada = fertilizantes[fert] * factorConcentracion / 1000.0;
                        tanque.Concentraciones_gL[fert] = cantidadConcentrada;
                        tanque.CantidadesFertilizante_kg[fert] = (cantidadConcentrada * tanque.Volumen_L) / 1000.0;
                    }
                }

                if (tanque.Fertilizantes.Any())
                {
                    indiceTanque++;
                }
            }

            // Último tanque para ácidos
            if (tanques.Count > 0 && acidos.Any())
            {
                var tanqueAcido = tanques[tanques.Count - 1];
                tanqueAcido.EtiquetaTanque = "Tanque de Ácidos";
                tanqueAcido.Acidos.AddRange(acidos);
            }
        }

        private string ObtenerEtiquetaTanque(int indice)
        {
            return indice < 8 ? ((char)('A' + indice)).ToString() : $"Tanque {indice + 1}";
        }

        private void VerificarCompatibilidadTanque(DistribucionTanque tanque)
        {
            var fertilizantes = tanque.Fertilizantes;

            for (int i = 0; i < fertilizantes.Count; i++)
            {
                for (int j = i + 1; j < fertilizantes.Count; j++)
                {
                    var fert1 = fertilizantes[i];
                    var fert2 = fertilizantes[j];

                    if (matrizCompatibilidad.ContainsKey((fert1, fert2)))
                    {
                        var compatibilidad = matrizCompatibilidad[(fert1, fert2)];

                        switch (compatibilidad)
                        {
                            case NivelCompatibilidad.I:
                                tanque.AdvertenciasCompatibilidad.Add($"🚫 INCOMPATIBLE: {fert1} y {fert2} no se pueden mezclar - precipitarán");
                                break;
                            case NivelCompatibilidad.E:
                                tanque.AdvertenciasCompatibilidad.Add($"⚠️ PRECAUCIÓN: {fert1} y {fert2} compatibles solo en agua al momento de inyección");
                                break;
                            case NivelCompatibilidad.L:
                                tanque.AdvertenciasCompatibilidad.Add($"⚠️ LIMITADO: {fert1} y {fert2} tienen compatibilidad limitada - usar cantidades reducidas");
                                break;
                            case NivelCompatibilidad.P:
                                tanque.AdvertenciasCompatibilidad.Add($"🔥 PELIGROSO: {fert1} y {fert2} generan calor - agregar ácido al agua cuidadosamente");
                                break;
                        }
                    }
                }
            }
        }

        private void VerificarSolubilidadTanque(DistribucionTanque tanque, int factorConcentracion)
        {
            foreach (var fertilizante in tanque.Concentraciones_gL)
            {
                if (solubilidadesFertilizantes.ContainsKey(fertilizante.Key))
                {
                    var solubilidad = solubilidadesFertilizantes[fertilizante.Key];
                    var concentracion = fertilizante.Value;

                    if (concentracion > solubilidad.LimiteConcentracionSegura)
                    {
                        tanque.AdvertenciasSolubilidad.Add(
                            $"🚫 RIESGO DE SOLUBILIDAD: {fertilizante.Key} concentración ({concentracion:F1} g/L) " +
                            $"excede el límite seguro ({solubilidad.LimiteConcentracionSegura:F1} g/L). " +
                            $"Solubilidad máxima: {solubilidad.Solubilidad_20C:F1} g/L a 20°C. " +
                            $"Recomendación: Reducir factor de concentración por debajo de 1:{factorConcentracion}"
                        );
                    }
                    else if (concentracion > solubilidad.ConcentracionMaximaRecomendada)
                    {
                        tanque.AdvertenciasSolubilidad.Add(
                            $"⚠️ PRECAUCIÓN: {fertilizante.Key} concentración ({concentracion:F1} g/L) " +
                            $"se acerca al límite seguro. Máximo recomendado: {solubilidad.ConcentracionMaximaRecomendada:F1} g/L"
                        );
                    }
                }
            }
        }

        private void CalcularDensidadTanque(DistribucionTanque tanque)
        {
            tanque.DensidadTotal_gL = tanque.Concentraciones_gL.Values.Sum();
        }

        private void CalcularCostoTanque(DistribucionTanque tanque)
        {
            double costoTotal = 0;

            foreach (var fertilizante in tanque.CantidadesFertilizante_kg)
            {
                if (costosFertilizantes.ContainsKey(fertilizante.Key))
                {
                    costoTotal += fertilizante.Value * costosFertilizantes[fertilizante.Key];
                }
            }

            tanque.CostoEstimado = costoTotal;
        }

        private void GenerarInstruccionesPreparacion(DistribucionTanque tanque)
        {
            tanque.InstruccionesPreparacion.Clear();

            tanque.InstruccionesPreparacion.Add($"🏷️ Tanque {tanque.EtiquetaTanque} ({tanque.ColorTanque}) - {tanque.Volumen_L}L");
            tanque.InstruccionesPreparacion.Add("📋 Secuencia de preparación:");
            tanque.InstruccionesPreparacion.Add("1. Llenar tanque con agua limpia al 80% de capacidad");
            tanque.InstruccionesPreparacion.Add("2. Iniciar sistema de mezcla/circulación");

            // Agregar fertilizantes en orden de solubilidad (más soluble primero)
            var fertilizantesOrdenados = tanque.Fertilizantes
                .Where(f => solubilidadesFertilizantes.ContainsKey(f))
                .OrderByDescending(f => solubilidadesFertilizantes[f].Solubilidad_20C)
                .ToList();

            int paso = 3;
            foreach (var fertilizante in fertilizantesOrdenados)
            {
                if (tanque.CantidadesFertilizante_kg.ContainsKey(fertilizante))
                {
                    double cantidad = tanque.CantidadesFertilizante_kg[fertilizante];
                    tanque.InstruccionesPreparacion.Add($"{paso}. Agregar {cantidad:F2} kg de {fertilizante} lentamente mientras se mezcla");
                    tanque.InstruccionesPreparacion.Add($"   Esperar disolución completa antes de la siguiente adición");
                    paso++;
                }
            }

            // Agregar ácidos al final
            foreach (var acido in tanque.Acidos)
            {
                tanque.InstruccionesPreparacion.Add($"{paso}. AGREGAR CUIDADOSAMENTE {acido} (SIEMPRE ácido al agua, nunca agua al ácido)");
                paso++;
            }

            tanque.InstruccionesPreparacion.Add($"{paso}. Llenar al volumen final ({tanque.Volumen_L}L)");
            tanque.InstruccionesPreparacion.Add($"{paso + 1}. Verificar pH y CE antes del uso");
            tanque.InstruccionesPreparacion.Add("⚠️ Seguridad: Usar guantes, gafas y asegurar buena ventilación");
        }

        public Dictionary<string, double> CalcularRequerimientosVolumen(
            Dictionary<string, double> concentracionesFertilizantes_mgL,
            int factorConcentracion,
            double volumenTanque_L,
            double volumenDiluidoObjetivo_L)
        {
            var resultados = new Dictionary<string, double>();

            // Calcular solución concentrada total necesaria
            double volumenConcentradoNecesario_L = volumenDiluidoObjetivo_L / factorConcentracion;
            resultados["VolumenConcentradoNecesario_L"] = volumenConcentradoNecesario_L;

            // Calcular número de preparaciones de tanque necesarias
            double preparacionesTanqueNecesarias = Math.Ceiling(volumenConcentradoNecesario_L / volumenTanque_L);
            resultados["PreparacionesTanqueNecesarias"] = preparacionesTanqueNecesarias;

            // Calcular cantidades totales de fertilizantes necesarias
            foreach (var fertilizante in concentracionesFertilizantes_mgL)
            {
                double cantidadTotal_kg = (fertilizante.Value * volumenConcentradoNecesario_L * factorConcentracion) / 1000000.0;
                resultados[$"{fertilizante.Key}_Total_kg"] = cantidadTotal_kg;
            }

            resultados["VolumenTanque_L"] = volumenTanque_L;
            resultados["VolumenDiluidoObjetivo_L"] = volumenDiluidoObjetivo_L;
            resultados["FactorConcentracion"] = factorConcentracion;
            resultados["TasaInyeccion_LporM3"] = 1000.0 / factorConcentracion;

            return resultados;
        }

        public ReporteSolucionConcentrada GenerarReporteCompleto(
            Dictionary<string, double> concentracionesFertilizantes_mgL,
            List<string> tiposAcidos,
            int numeroDeTanques,
            int factorConcentracion,
            double volumenTanque_L,
            double volumenDiluidoObjetivo_L)
        {
            var reporte = new ReporteSolucionConcentrada();

            // Parámetros básicos
            reporte.NumeroDeTanques = numeroDeTanques;
            reporte.FactorConcentracion = ObtenerFactorConcentracion(factorConcentracion);

            // Distribuir fertilizantes
            reporte.Tanques = DistribuirFertilizantes(concentracionesFertilizantes_mgL, tiposAcidos, numeroDeTanques, factorConcentracion, volumenTanque_L);

            // Calcular requerimientos de volumen
            reporte.RequerimientosVolumen = CalcularRequerimientosVolumen(concentracionesFertilizantes_mgL, factorConcentracion, volumenTanque_L, volumenDiluidoObjetivo_L);

            // Calcular costos
            reporte.CostoTotal = reporte.Tanques.Sum(t => t.CostoEstimado);
            reporte.CostoPorM3_SolucionDiluida = reporte.CostoTotal / (volumenDiluidoObjetivo_L / 1000.0);

            // Calcular totales de fertilizantes
            foreach (var tanque in reporte.Tanques)
            {
                foreach (var fertilizante in tanque.CantidadesFertilizante_kg)
                {
                    if (!reporte.TotalesFertilizante_kg.ContainsKey(fertilizante.Key))
                        reporte.TotalesFertilizante_kg[fertilizante.Key] = 0;
                    reporte.TotalesFertilizante_kg[fertilizante.Key] += fertilizante.Value;
                }
            }

            // Generar advertencias y recomendaciones
            GenerarAdvertenciasYRecomendaciones(reporte);

            return reporte;
        }

        private void GenerarAdvertenciasYRecomendaciones(ReporteSolucionConcentrada reporte)
        {
            // Advertencias críticas
            foreach (var tanque in reporte.Tanques)
            {
                reporte.AdvertenciasCriticas.AddRange(tanque.AdvertenciasCompatibilidad.Where(w => w.Contains("INCOMPATIBLE")));
                reporte.AdvertenciasCriticas.AddRange(tanque.AdvertenciasSolubilidad.Where(w => w.Contains("RIESGO DE SOLUBILIDAD")));
            }

            // Recomendaciones generales
            reporte.RecomendacionesGenerales.Add("Siempre preparar soluciones frescas y usar dentro de 2-3 días");
            reporte.RecomendacionesGenerales.Add("Almacenar soluciones concentradas en lugares frescos y oscuros");
            reporte.RecomendacionesGenerales.Add("Verificar pH y CE de la solución diluida final antes de la aplicación");
            reporte.RecomendacionesGenerales.Add("Mantener el equipo de inyección regularmente para asegurar proporciones de dilución precisas");

            if (reporte.FactorConcentracion.Factor > 200)
            {
                reporte.RecomendacionesGenerales.Add("Factor de alta concentración - monitorear precipitación durante almacenamiento");
            }

            if (reporte.NumeroDeTanques == 2)
            {
                reporte.RecomendacionesGenerales.Add("Sistema de dos tanques: Asegurar que los ácidos estén correctamente separados de las fuentes de calcio");
            }

            // Sugerencias de optimización de costos
            if (reporte.CostoPorM3_SolucionDiluida > 5.0)
            {
                reporte.RecomendacionesGenerales.Add("Considerar compras al por mayor de fertilizantes para reducir costos");
            }
        }

        public List<string> ValidarFactorConcentracionParaSolubilidad(
            Dictionary<string, double> concentracionesFertilizantes_mgL,
            int factorConcentracion)
        {
            var advertencias = new List<string>();

            foreach (var fertilizante in concentracionesFertilizantes_mgL)
            {
                if (solubilidadesFertilizantes.ContainsKey(fertilizante.Key))
                {
                    var solubilidad = solubilidadesFertilizantes[fertilizante.Key];
                    double cantidadConcentrada_gL = fertilizante.Value * factorConcentracion / 1000.0;

                    if (cantidadConcentrada_gL > solubilidad.LimiteConcentracionSegura)
                    {
                        advertencias.Add($"⚠️ {fertilizante.Key}: Factor de concentración 1:{factorConcentracion} " +
                                       $"resulta en {cantidadConcentrada_gL:F1} g/L, excediendo el límite seguro de {solubilidad.LimiteConcentracionSegura:F1} g/L");
                    }
                }
            }

            return advertencias;
        }

        public int SugerirFactorConcentracionOptimo(Dictionary<string, double> concentracionesFertilizantes_mgL)
        {
            int factorMaximoSeguro = 400; // Comenzar con el máximo

            foreach (var fertilizante in concentracionesFertilizantes_mgL)
            {
                if (solubilidadesFertilizantes.ContainsKey(fertilizante.Key))
                {
                    var solubilidad = solubilidadesFertilizantes[fertilizante.Key];

                    // Calcular factor de concentración máximo seguro para este fertilizante
                    int factorSeguro = (int)(solubilidad.ConcentracionMaximaRecomendada * 1000.0 / fertilizante.Value);
                    factorMaximoSeguro = Math.Min(factorMaximoSeguro, factorSeguro);
                }
            }

            // Redondear hacia abajo al factor de concentración estándar más cercano
            var factoresEstandar = factoresConcentracion.Select(cf => cf.Factor).OrderBy(f => f).ToList();
            return factoresEstandar.Where(f => f <= factorMaximoSeguro).LastOrDefault();
        }

        public Dictionary<string, object> ObtenerResumenTanques(List<DistribucionTanque> tanques)
        {
            var resumen = new Dictionary<string, object>();

            resumen["TotalTanques"] = tanques.Count;
            resumen["TotalTiposFertilizantes"] = tanques.SelectMany(t => t.Fertilizantes).Distinct().Count();
            resumen["VolumenTotal_L"] = tanques.Sum(t => t.Volumen_L);
            resumen["CostoTotal"] = tanques.Sum(t => t.CostoEstimado);
            resumen["DensidadPromedio_gL"] = tanques.Average(t => t.DensidadTotal_gL);
            resumen["TotalAdvertenciasCompatibilidad"] = tanques.Sum(t => t.AdvertenciasCompatibilidad.Count);
            resumen["TotalAdvertenciasSolubilidad"] = tanques.Sum(t => t.AdvertenciasSolubilidad.Count);

            // Detalles de tanques
            var detallesTanques = tanques.Select(t => new
            {
                Etiqueta = t.EtiquetaTanque,
                CantidadFertilizantes = t.Fertilizantes.Count,
                Densidad_gL = t.DensidadTotal_gL,
                Costo = t.CostoEstimado,
                TieneAdvertencias = t.AdvertenciasCompatibilidad.Any() || t.AdvertenciasSolubilidad.Any()
            }).ToList();

            resumen["DetallesTanques"] = detallesTanques;

            return resumen;
        }

        // Métodos utilitarios adicionales para operaciones avanzadas
        public Dictionary<string, double> CalcularTasasInyeccion(
            Dictionary<string, double> concentracionesTanque_gL,
            int factorConcentracion,
            double flujoObjetivo_Lh)
        {
            var tasasInyeccion = new Dictionary<string, double>();

            foreach (var tanque in concentracionesTanque_gL)
            {
                // Calcular tasa de inyección necesaria para este tanque
                double tasaInyeccion_Lh = flujoObjetivo_Lh / factorConcentracion;
                tasasInyeccion[tanque.Key] = tasaInyeccion_Lh;
            }

            return tasasInyeccion;
        }

        public List<string> GenerarListaCompras(Dictionary<string, double> totalesFertilizantes_kg)
        {
            var listaCompras = new List<string>();

            listaCompras.Add("=== LISTA DE COMPRAS DE FERTILIZANTES ===");
            listaCompras.Add("");

            double costoTotal = 0;

            foreach (var fertilizante in totalesFertilizantes_kg.OrderBy(f => f.Key))
            {
                double costo = costosFertilizantes.GetValueOrDefault(fertilizante.Key, 0) * fertilizante.Value;
                costoTotal += costo;

                listaCompras.Add($"• {fertilizante.Key}: {fertilizante.Value:F2} kg (${costo:F2})");

                // Agregar fórmula química si está disponible
                if (solubilidadesFertilizantes.ContainsKey(fertilizante.Key))
                {
                    listaCompras.Add($"  Fórmula: {solubilidadesFertilizantes[fertilizante.Key].Formula}");
                }
            }

            listaCompras.Add("");
            listaCompras.Add($"COSTO TOTAL ESTIMADO: ${costoTotal:F2}");
            listaCompras.Add("");
            listaCompras.Add("Notas:");
            listaCompras.Add("- Los precios son estimados y pueden variar según el proveedor");
            listaCompras.Add("- Considere descuentos por volumen para grandes cantidades");
            listaCompras.Add("- Verificar fechas de vencimiento en micronutrientes");

            return listaCompras;
        }

        public Dictionary<string, object> AnalizarEquilibrioNutrientes(
            Dictionary<string, double> concentracionesFinales_mgL,
            Dictionary<string, double> concentracionesObjetivo_mgL)
        {
            var analisis = new Dictionary<string, object>();
            var desviaciones = new Dictionary<string, double>();
            var desviacionesPorcentuales = new Dictionary<string, double>();

            foreach (var objetivo in concentracionesObjetivo_mgL)
            {
                double real = concentracionesFinales_mgL.GetValueOrDefault(objetivo.Key, 0);
                double desviacion = real - objetivo.Value;
                double desviacionPorcentual = objetivo.Value > 0 ? (desviacion / objetivo.Value) * 100 : 0;

                desviaciones[objetivo.Key] = desviacion;
                desviacionesPorcentuales[objetivo.Key] = desviacionPorcentual;
            }

            analisis["Desviaciones_mgL"] = desviaciones;
            analisis["DesviacionesPorcentuales"] = desviacionesPorcentuales;
            analisis["MaximaDesviacion"] = desviaciones.Values.Max(Math.Abs);
            analisis["DesviacionAbsolutaPromedio"] = desviaciones.Values.Average(Math.Abs);
            analisis["DentroTolerancia"] = desviacionesPorcentuales.Values.All(d => Math.Abs(d) <= 5.0);

            return analisis;
        }
    }
}