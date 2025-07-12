using System;
using System.Collections.Generic;
using System.Linq;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.


public class DatosElemento
{
    public double PesoAtomico { get; set; }
    public int Valencia { get; set; }
    public bool EsCation { get; set; }
}

public class BalanceIonico
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

public class ResultadoFertilizante
{
    public string Nombre { get; set; }
    public double Pureza { get; set; } // Porcentaje de pureza
    public double PesoMolecular { get; set; } // Peso molecular en g/mol
    public double ConcentracionSal_mgL { get; set; } // Concentración de sal en mg/L
    public double ConcentracionSal_mmolL { get; set; } // Concentración de sal en mmol/L

    // Elementos aportados por el fertilizante
    public double Ca { get; set; }
    public double K { get; set; }
    public double Mg { get; set; }
    public double Na { get; set; }
    public double NH4 { get; set; }
    public double N { get; set; }
    public double P { get; set; }
    public double S { get; set; }
    public double Cl { get; set; }
    public double Fe { get; set; }
    public double Mn { get; set; }
    public double Zn { get; set; }
    public double Cu { get; set; }
    public double B { get; set; }
    public double Mo { get; set; }

    // Formas iónicas
    public double NO3 { get; set; } // Nitrato
    public double H2PO4 { get; set; } // Dihidrógeno fosfato
    public double SO4 { get; set; } // Sulfato

    public double HCO3 { get; set; } // Bicarbonato
}

public class Fertilizante
{
    public string Nombre { get; set; }
    public double Pureza { get; set; } // Porcentaje de pureza
    public double PesoMolecular { get; set; } // Peso molecular en g/mol
    public Dictionary<string, double> Elementos { get; set; } = new Dictionary<string, double>(); // Elementos y sus pesos atómicos
    public double Solubilidad { get; set; } // Solubilidad en g/L
    public double Costo { get; set; } // Costo por kg

    public Fertilizante(string nombre, double pureza, double pesoMolecular)
    {
        Nombre = nombre;
        Pureza = pureza;
        PesoMolecular = pesoMolecular;
    }
}

public class AnalisisAgua
{
    public Dictionary<string, double> Elementos_mgL { get; set; } = new Dictionary<string, double>();
    public Dictionary<string, double> Elementos_mmolL { get; set; } = new Dictionary<string, double>();
    public Dictionary<string, double> Elementos_meqL { get; set; } = new Dictionary<string, double>();
    public double pH { get; set; }
    public double CE { get; set; } // Conductividad eléctrica en mS/cm
    public double HCO3 { get; set; } // Bicarbonato en mg/L
}

namespace CalculadoraHidroponica.Modulos
{
    public class CalculadoraNutrientesAvanzada
    {
        private List<Fertilizante>? fertilizantes;
        private AnalisisAgua analisisAgua;
        private Dictionary<string, double> concentracionesObjetivo;
        private Dictionary<string, DatosElemento>? datosElementos;

        public CalculadoraNutrientesAvanzada()
        {
            InicializarFertilizantes();
            InicializarDatosElementos();
            concentracionesObjetivo = new Dictionary<string, double>();
            analisisAgua = new AnalisisAgua();
            EstablecerValoresPorDefecto();
        }

        private void EstablecerValoresPorDefecto()
        {
            // Establecer concentraciones objetivo: Ca, K, Mg, N, P, S
            concentracionesObjetivo["Ca"] = 172;
            concentracionesObjetivo["K"] = 260;
            concentracionesObjetivo["Mg"] = 50;
            concentracionesObjetivo["N"] = 150;
            concentracionesObjetivo["P"] = 45;
            concentracionesObjetivo["S"] = 108;

            // Establecer valores por defecto del análisis de agua (mg/L)
            analisisAgua.Elementos_mgL["Ca"] = 10;
            analisisAgua.Elementos_mgL["K"] = 2;
            analisisAgua.Elementos_mgL["Mg"] = 5;
            analisisAgua.Elementos_mgL["N"] = 0;
            analisisAgua.Elementos_mgL["P"] = 0;
            analisisAgua.Elementos_mgL["S"] = 0;
            analisisAgua.pH = 7.2;
            analisisAgua.CE = 0.5;
            analisisAgua.HCO3 = 77; // mg/L

            // Calcular mmol/L y meq/L para el agua
            foreach (var elemento in analisisAgua.Elementos_mgL)
            {
                if (datosElementos?.ContainsKey(elemento.Key) == true)
                {
                    analisisAgua.Elementos_mmolL[elemento.Key] = elemento.Value / datosElementos[elemento.Key].PesoAtomico;
                    analisisAgua.Elementos_meqL[elemento.Key] = analisisAgua.Elementos_mmolL[elemento.Key] * datosElementos[elemento.Key].Valencia;
                }
            }
        }

        public Dictionary<string, double> ObtenerConcentracionesObjetivo() => concentracionesObjetivo;
        public AnalisisAgua ObtenerAnalisisAgua() => analisisAgua;

        public void ActualizarConcentracionesObjetivo(Dictionary<string, double> nuevosObjetivos)
        {
            foreach (var objetivo in nuevosObjetivos)
            {
                concentracionesObjetivo[objetivo.Key] = objetivo.Value;
            }
        }

        public void ActualizarAnalisisAgua(AnalisisAgua nuevoAnalisisAgua)
        {
            analisisAgua = nuevoAnalisisAgua;
        }

        private void InicializarDatosElementos()
        {
            datosElementos = new Dictionary<string, DatosElemento>
            {
                // Cationes
                ["Ca"] = new DatosElemento { PesoAtomico = 40.08, Valencia = 2, EsCation = true },
                ["K"] = new DatosElemento { PesoAtomico = 39.10, Valencia = 1, EsCation = true },
                ["Mg"] = new DatosElemento { PesoAtomico = 24.31, Valencia = 2, EsCation = true },
                ["Na"] = new DatosElemento { PesoAtomico = 22.99, Valencia = 1, EsCation = true },
                ["NH4"] = new DatosElemento { PesoAtomico = 18.04, Valencia = 1, EsCation = true },

                // Aniones
                ["NO3"] = new DatosElemento { PesoAtomico = 62.00, Valencia = 1, EsCation = false }, // NO3-
                ["N"] = new DatosElemento { PesoAtomico = 14.01, Valencia = 1, EsCation = false },
                ["SO4"] = new DatosElemento { PesoAtomico = 96.06, Valencia = 2, EsCation = false }, // SO4=
                ["S"] = new DatosElemento { PesoAtomico = 32.06, Valencia = 2, EsCation = false },
                ["Cl"] = new DatosElemento { PesoAtomico = 35.45, Valencia = 1, EsCation = false }, // Cl-
                ["H2PO4"] = new DatosElemento { PesoAtomico = 96.99, Valencia = 1, EsCation = false }, // H2PO4-
                ["P"] = new DatosElemento { PesoAtomico = 30.97, Valencia = 1, EsCation = false },
                ["HCO3"] = new DatosElemento { PesoAtomico = 61.02, Valencia = 1, EsCation = false }, // HCO3-

                // Micronutrientes
                ["Fe"] = new DatosElemento { PesoAtomico = 55.85, Valencia = 2, EsCation = true },
                ["Mn"] = new DatosElemento { PesoAtomico = 54.94, Valencia = 2, EsCation = true },
                ["Zn"] = new DatosElemento { PesoAtomico = 65.38, Valencia = 2, EsCation = true },
                ["Cu"] = new DatosElemento { PesoAtomico = 63.55, Valencia = 2, EsCation = true },
                ["B"] = new DatosElemento { PesoAtomico = 10.81, Valencia = 3, EsCation = false },
                ["Mo"] = new DatosElemento { PesoAtomico = 95.96, Valencia = 6, EsCation = false }
            };
        }

        private void InicializarFertilizantes()
        {
            fertilizantes = new List<Fertilizante>();

            // Ácido nítrico
            var acidoNitrico = new Fertilizante("Ácido nítrico", 65, 62.00);
            acidoNitrico.Elementos["N"] = 14.01;
            acidoNitrico.Solubilidad = 1000000; // Altamente soluble
            acidoNitrico.Costo = 1.20;
            fertilizantes.Add(acidoNitrico);

            // Ácido fosfórico
            var acidoFosforico = new Fertilizante("Ácido fosfórico", 85, 98.00);
            acidoFosforico.Elementos["P"] = 30.97;
            acidoFosforico.Solubilidad = 1000000; // Altamente soluble
            acidoFosforico.Costo = 1.50;
            fertilizantes.Add(acidoFosforico);

            // KH2PO4 - Fosfato Monopotásico
            var kh2po4 = new Fertilizante("KH2PO4", 98, 136.19);
            kh2po4.Elementos["K"] = 39.10;
            kh2po4.Elementos["P"] = 30.97;
            kh2po4.Solubilidad = 227; // g/L a 20°C
            kh2po4.Costo = 2.50;
            fertilizantes.Add(kh2po4);

            // Ca(NO3)2.2H2O - Nitrato de Calcio
            var caNO3 = new Fertilizante("Ca(NO3)2.2H2O", 95, 200.12);
            caNO3.Elementos["Ca"] = 40.08;
            caNO3.Elementos["N"] = 28.01; // 2 átomos de N
            caNO3.Solubilidad = 1200; // g/L a 20°C
            caNO3.Costo = 0.80;
            fertilizantes.Add(caNO3);

            // KNO3 - Nitrato de Potasio
            var kno3 = new Fertilizante("KNO3", 98, 101.13);
            kno3.Elementos["K"] = 39.10;
            kno3.Elementos["N"] = 14.01;
            kno3.Solubilidad = 335; // g/L a 20°C
            kno3.Costo = 1.20;
            fertilizantes.Add(kno3);

            // K2SO4 - Sulfato de Potasio
            var k2so4 = new Fertilizante("K2SO4", 98, 174.37);
            k2so4.Elementos["K"] = 78.20; // 2 átomos de K
            k2so4.Elementos["S"] = 32.06;
            k2so4.Solubilidad = 111; // g/L a 20°C
            k2so4.Costo = 1.50;
            fertilizantes.Add(k2so4);

            // MgSO4.7H2O - Sulfato de Magnesio (Sal de Epsom)
            var mgso4 = new Fertilizante("MgSO4.7H2O", 98, 246.32);
            mgso4.Elementos["Mg"] = 24.31;
            mgso4.Elementos["S"] = 32.06;
            mgso4.Solubilidad = 710; // g/L a 20°C
            mgso4.Costo = 0.60;
            fertilizantes.Add(mgso4);

            // NH4NO3 - Nitrato de Amonio
            var nh4no3 = new Fertilizante("NH4NO3", 99, 80.04);
            nh4no3.Elementos["N"] = 28.01; // 2 átomos de N
            nh4no3.Elementos["NH4"] = 18.04;
            nh4no3.Solubilidad = 1900; // g/L a 20°C
            nh4no3.Costo = 0.45;
            fertilizantes.Add(nh4no3);

            // (NH4)2SO4 - Sulfato de Amonio
            var nh4so4 = new Fertilizante("(NH4)2SO4", 99, 132.14);
            nh4so4.Elementos["N"] = 28.01; // 2 átomos de N
            nh4so4.Elementos["NH4"] = 36.08; // 2 átomos de NH4
            nh4so4.Elementos["S"] = 32.06;
            nh4so4.Solubilidad = 760; // g/L a 20°C
            nh4so4.Costo = 0.50;
            fertilizantes.Add(nh4so4);

            // KCl - Cloruro de Potasio
            var kcl = new Fertilizante("KCl", 99, 74.55);
            kcl.Elementos["K"] = 39.10;
            kcl.Elementos["Cl"] = 35.45;
            kcl.Solubilidad = 342; // g/L a 20°C
            kcl.Costo = 0.90;
            fertilizantes.Add(kcl);

            // CaCl2.2H2O - Cloruro de Calcio
            var cacl2 = new Fertilizante("CaCl2.2H2O", 99, 147.02);
            cacl2.Elementos["Ca"] = 40.08;
            cacl2.Elementos["Cl"] = 70.90; // 2 átomos de Cl
            cacl2.Solubilidad = 600; // g/L a 20°C
            cacl2.Costo = 0.70;
            fertilizantes.Add(cacl2);

            // NH4H2PO4 - Fosfato Monoamónico (MAP)
            var nh4h2po4 = new Fertilizante("NH4H2PO4", 98, 115.03);
            nh4h2po4.Elementos["N"] = 14.01;
            nh4h2po4.Elementos["NH4"] = 18.04;
            nh4h2po4.Elementos["P"] = 30.97;
            nh4h2po4.Solubilidad = 400; // g/L a 20°C
            nh4h2po4.Costo = 1.80;
            fertilizantes.Add(nh4h2po4);

            // (NH4)2HPO4 - Fosfato Diamónico (DAP)
            var nh4hpo4 = new Fertilizante("(NH4)2HPO4", 98, 132.06);
            nh4hpo4.Elementos["N"] = 28.01; // 2 átomos de N
            nh4hpo4.Elementos["NH4"] = 36.08; // 2 átomos de NH4
            nh4hpo4.Elementos["P"] = 30.97;
            nh4hpo4.Solubilidad = 575; // g/L a 0°C
            nh4hpo4.Costo = 1.90;
            fertilizantes.Add(nh4hpo4);

            // K2HPO4.3H2O - Fosfato Dipotásico
            var k2hpo4 = new Fertilizante("K2HPO4.3H2O", 98, 228.22);
            k2hpo4.Elementos["K"] = 78.20; // 2 átomos de K
            k2hpo4.Elementos["P"] = 30.97;
            k2hpo4.Solubilidad = 1590; // g/L a 0°C
            k2hpo4.Costo = 2.80;
            fertilizantes.Add(k2hpo4);

            // MgCl2.6H2O - Cloruro de Magnesio
            var mgcl2 = new Fertilizante("MgCl2.6H2O", 99, 203.30);
            mgcl2.Elementos["Mg"] = 24.31;
            mgcl2.Elementos["Cl"] = 70.90; // 2 átomos de Cl
            mgcl2.Solubilidad = 546; // g/L a 20°C
            mgcl2.Costo = 0.70;
            fertilizantes.Add(mgcl2);

            // === MICRONUTRIENTES ===

            // FeEDTA - Quelato de Hierro EDTA
            var feEDTA = new Fertilizante("FeEDTA", 99, 367.05);
            feEDTA.Elementos["Fe"] = 55.85;
            feEDTA.Solubilidad = 1000; // Altamente soluble
            feEDTA.Costo = 8.50;
            fertilizantes.Add(feEDTA);

            // FeSO4.7H2O - Sulfato de Hierro
            var feso4 = new Fertilizante("FeSO4.7H2O", 99, 278.02);
            feso4.Elementos["Fe"] = 55.85;
            feso4.Elementos["S"] = 32.06;
            feso4.Solubilidad = 260; // g/L a 20°C
            feso4.Costo = 1.80;
            fertilizantes.Add(feso4);

            // FeCl3.6H2O - Cloruro de Hierro
            var fecl3 = new Fertilizante("FeCl3.6H2O", 99, 270.30);
            fecl3.Elementos["Fe"] = 55.85;
            fecl3.Elementos["Cl"] = 106.35; // 3 átomos de Cl
            fecl3.Solubilidad = 920; // g/L a 20°C
            fecl3.Costo = 2.20;
            fertilizantes.Add(fecl3);

            // MnSO4.4H2O - Sulfato de Manganeso
            var mnso4 = new Fertilizante("MnSO4.4H2O", 99, 223.06);
            mnso4.Elementos["Mn"] = 54.94;
            mnso4.Elementos["S"] = 32.06;
            mnso4.Solubilidad = 1053; // g/L a 20°C
            mnso4.Costo = 2.80;
            fertilizantes.Add(mnso4);

            // MnCl2.4H2O - Cloruro de Manganeso
            var mncl2 = new Fertilizante("MnCl2.4H2O", 99, 197.91);
            mncl2.Elementos["Mn"] = 54.94;
            mncl2.Elementos["Cl"] = 70.90; // 2 átomos de Cl
            mncl2.Solubilidad = 638; // g/L a 20°C
            mncl2.Costo = 3.20;
            fertilizantes.Add(mncl2);

            // ZnSO4.7H2O - Sulfato de Zinc
            var znso4 = new Fertilizante("ZnSO4.7H2O", 99, 287.54);
            znso4.Elementos["Zn"] = 65.38;
            znso4.Elementos["S"] = 32.06;
            znso4.Solubilidad = 750; // g/L a 20°C
            znso4.Costo = 3.50;
            fertilizantes.Add(znso4);

            // ZnCl2 - Cloruro de Zinc
            var zncl2 = new Fertilizante("ZnCl2", 99, 136.28);
            zncl2.Elementos["Zn"] = 65.38;
            zncl2.Elementos["Cl"] = 70.90; // 2 átomos de Cl
            zncl2.Solubilidad = 4320; // g/L a 20°C
            zncl2.Costo = 4.80;
            fertilizantes.Add(zncl2);

            // CuSO4.5H2O - Sulfato de Cobre
            var cuso4 = new Fertilizante("CuSO4.5H2O", 99, 249.68);
            cuso4.Elementos["Cu"] = 63.55;
            cuso4.Elementos["S"] = 32.06;
            cuso4.Solubilidad = 316; // g/L a 20°C
            cuso4.Costo = 4.20;
            fertilizantes.Add(cuso4);

            // CuCl2.2H2O - Cloruro de Cobre
            var cucl2 = new Fertilizante("CuCl2.2H2O", 99, 170.48);
            cucl2.Elementos["Cu"] = 63.55;
            cucl2.Elementos["Cl"] = 70.90; // 2 átomos de Cl
            cucl2.Solubilidad = 730; // g/L a 20°C
            cucl2.Costo = 5.40;
            fertilizantes.Add(cucl2);

            // H3BO3 - Ácido Bórico
            var h3bo3 = new Fertilizante("H3BO3", 99, 61.83);
            h3bo3.Elementos["B"] = 10.81;
            h3bo3.Solubilidad = 63.5; // g/L a 20°C
            h3bo3.Costo = 3.20;
            fertilizantes.Add(h3bo3);

            // Na2B4O7.10H2O - Bórax
            var borax = new Fertilizante("Na2B4O7.10H2O", 99, 381.37);
            borax.Elementos["B"] = 43.24; // 4 átomos de B
            borax.Elementos["Na"] = 45.98; // 2 átomos de Na
            borax.Solubilidad = 47.2; // g/L a 20°C
            borax.Costo = 2.80;
            fertilizantes.Add(borax);

            // Na2MoO4.2H2O - Molibdato de Sodio
            var namoo4 = new Fertilizante("Na2MoO4.2H2O", 99, 241.95);
            namoo4.Elementos["Mo"] = 95.96;
            namoo4.Elementos["Na"] = 45.98; // 2 átomos de Na
            namoo4.Solubilidad = 840; // g/L a 20°C
            namoo4.Costo = 15.00;
            fertilizantes.Add(namoo4);

            // (NH4)6Mo7O24.4H2O - Molibdato de Amonio
            var nh4moo4 = new Fertilizante("(NH4)6Mo7O24.4H2O", 99, 1235.86);
            nh4moo4.Elementos["Mo"] = 671.72; // 7 átomos de Mo
            nh4moo4.Elementos["N"] = 84.06; // 6 átomos de N
            nh4moo4.Elementos["NH4"] = 108.24; // 6 átomos de NH4
            nh4moo4.Solubilidad = 430; // g/L a 20°C
            nh4moo4.Costo = 18.50;
            fertilizantes.Add(nh4moo4);

            // === QUELATOS ESPECIALIZADOS ===

            // FeEDDHA - Quelato de Hierro EDDHA (para suelos calcáreos)
            var feEDDHA = new Fertilizante("FeEDDHA", 99, 435.21);
            feEDDHA.Elementos["Fe"] = 55.85;
            feEDDHA.Solubilidad = 1000; // Altamente soluble
            feEDDHA.Costo = 12.50;
            fertilizantes.Add(feEDDHA);

            // FeDTPA - Quelato de Hierro DTPA
            var feDTPA = new Fertilizante("FeDTPA", 99, 445.19);
            feDTPA.Elementos["Fe"] = 55.85;
            feDTPA.Solubilidad = 1000; // Altamente soluble
            feDTPA.Costo = 10.20;
            fertilizantes.Add(feDTPA);

            // MnEDTA - Quelato de Manganeso EDTA
            var mnEDTA = new Fertilizante("MnEDTA", 99, 343.06);
            mnEDTA.Elementos["Mn"] = 54.94;
            mnEDTA.Solubilidad = 1000; // Altamente soluble
            mnEDTA.Costo = 9.80;
            fertilizantes.Add(mnEDTA);

            // ZnEDTA - Quelato de Zinc EDTA
            var znEDTA = new Fertilizante("ZnEDTA", 99, 353.59);
            znEDTA.Elementos["Zn"] = 65.38;
            znEDTA.Solubilidad = 1000; // Altamente soluble
            znEDTA.Costo = 11.50;
            fertilizantes.Add(znEDTA);

            // CuEDTA - Quelato de Cobre EDTA
            var cuEDTA = new Fertilizante("CuEDTA", 99, 351.76);
            cuEDTA.Elementos["Cu"] = 63.55;
            cuEDTA.Solubilidad = 1000; // Altamente soluble
            cuEDTA.Costo = 13.20;
            fertilizantes.Add(cuEDTA);

            // === FERTILIZANTES ESPECIALES ===

            // Ca(H2PO4)2.H2O - Superfosfato Simple
            var superfosfato = new Fertilizante("Ca(H2PO4)2.H2O", 95, 252.07);
            superfosfato.Elementos["Ca"] = 40.08;
            superfosfato.Elementos["P"] = 61.94; // 2 átomos de P
            superfosfato.Solubilidad = 18; // g/L a 20°C (baja solubilidad)
            superfosfato.Costo = 1.20;
            fertilizantes.Add(superfosfato);

            // CaSO4.2H2O - Sulfato de Calcio (Yeso)
            var yeso = new Fertilizante("CaSO4.2H2O", 99, 172.17);
            yeso.Elementos["Ca"] = 40.08;
            yeso.Elementos["S"] = 32.06;
            yeso.Solubilidad = 2.4; // g/L a 20°C (muy baja solubilidad)
            yeso.Costo = 0.25;
            fertilizantes.Add(yeso);

            // KOH - Hidróxido de Potasio (para ajuste de pH)
            var koh = new Fertilizante("KOH", 85, 56.11);
            koh.Elementos["K"] = 39.10;
            koh.Solubilidad = 1120; // g/L a 20°C
            koh.Costo = 2.80;
            fertilizantes.Add(koh);

            // K2CO3 - Carbonato de Potasio
            var k2co3 = new Fertilizante("K2CO3", 99, 138.21);
            k2co3.Elementos["K"] = 78.20; // 2 átomos de K
            k2co3.Solubilidad = 1120; // g/L a 20°C
            k2co3.Costo = 3.50;
            fertilizantes.Add(k2co3);

            // NaNO3 - Nitrato de Sodio (uso limitado en hidroponía)
            var nano3 = new Fertilizante("NaNO3", 99, 84.99);
            nano3.Elementos["Na"] = 22.99;
            nano3.Elementos["N"] = 14.01;
            nano3.Solubilidad = 921; // g/L a 20°C
            nano3.Costo = 0.65;
            fertilizantes.Add(nano3);

            // === FERTILIZANTES ORGÁNICOS SOLUBLES ===

            // Aminoácidos Quelados (representativo)
            var aminoacidos = new Fertilizante("Aminoácidos Quelados", 95, 200.00);
            aminoacidos.Elementos["N"] = 14.01; // Contenido variable de N
            aminoacidos.Solubilidad = 1000; // Altamente soluble
            aminoacidos.Costo = 8.90;
            fertilizantes.Add(aminoacidos);

            // Ácidos Húmicos Solubles
            var acHumicos = new Fertilizante("Ácidos Húmicos", 90, 300.00);
            acHumicos.Elementos["K"] = 39.10; // Usualmente como humato de potasio
            acHumicos.Solubilidad = 850; // Soluble en agua
            acHumicos.Costo = 6.20;
            fertilizantes.Add(acHumicos);

            // === FERTILIZANTES DE EMERGENCIA ===

            // MgO - Óxido de Magnesio (para emergencias de Mg)
            var mgo = new Fertilizante("MgO", 98, 40.30);
            mgo.Elementos["Mg"] = 24.31;
            mgo.Solubilidad = 0.086; // g/L a 20°C (muy baja)
            mgo.Costo = 1.10;
            fertilizantes.Add(mgo);

            // CaO - Óxido de Calcio (Cal viva)
            var cao = new Fertilizante("CaO", 95, 56.08);
            cao.Elementos["Ca"] = 40.08;
            cao.Solubilidad = 1.65; // g/L a 20°C (reacciona con agua)
            cao.Costo = 0.35;
            fertilizantes.Add(cao);

            // Ca(OH)2 - Hidróxido de Calcio (Cal apagada)
            var caoh2 = new Fertilizante("Ca(OH)2", 95, 74.09);
            caoh2.Elementos["Ca"] = 40.08;
            caoh2.Solubilidad = 1.73; // g/L a 20°C
            caoh2.Costo = 0.40;
            fertilizantes.Add(caoh2);
        }

        public List<ResultadoFertilizante> CalcularSolucion()
        {
            var resultados = new List<ResultadoFertilizante>();
            var nutrientesRestantes = new Dictionary<string, double>();

            // Calcular nutrientes faltantes
            foreach (var objetivo in concentracionesObjetivo)
            {
                double contenidoEnAgua = analisisAgua.Elementos_mgL.ContainsKey(objetivo.Key)
                    ? analisisAgua.Elementos_mgL[objetivo.Key] : 0;
                nutrientesRestantes[objetivo.Key] = Math.Max(0, objetivo.Value - contenidoEnAgua);
            }

            // Algoritmo optimizado para minimizar excesos
            resultados = CalcularSolucionOptimizada(nutrientesRestantes);

            return resultados;
        }

        private List<ResultadoFertilizante> CalcularSolucionOptimizada(Dictionary<string, double> nutrientesRestantes)
        {
            var resultados = new List<ResultadoFertilizante>();

            // 1. KH2PO4 para fósforo (siempre primero)
            if (nutrientesRestantes.ContainsKey("P") && nutrientesRestantes["P"] > 0)
            {
                var resultado = CalcularFertilizante("KH2PO4", "P", nutrientesRestantes["P"]);
                resultados.Add(resultado);

                // Actualizar K restante
                if (nutrientesRestantes.ContainsKey("K"))
                    nutrientesRestantes["K"] = Math.Max(0, nutrientesRestantes["K"] - resultado.K);
            }

            // 2. Decisión inteligente para Calcio
            double nRequerido = nutrientesRestantes.GetValueOrDefault("N", 0);
            double caRequerido = nutrientesRestantes.GetValueOrDefault("Ca", 0);

            if (caRequerido > 0)
            {
                // Calcular cuánto N aportaría Ca(NO3)2 para todo el Ca
                double nDeCaNO3Completo = CalcularContribucionNutriente("Ca(NO3)2.2H2O", "N", caRequerido, "Ca");

                if (nDeCaNO3Completo > nRequerido * 1.5) // Si excede significativamente
                {
                    // Usar solo la cantidad de Ca(NO3)2 necesaria para el N
                    if (nRequerido > 0)
                    {
                        var resultadoCaNO3 = CalcularFertilizante("Ca(NO3)2.2H2O", "N", nRequerido);
                        resultados.Add(resultadoCaNO3);

                        nutrientesRestantes["N"] = 0;
                        nutrientesRestantes["Ca"] = Math.Max(0, nutrientesRestantes["Ca"] - resultadoCaNO3.Ca);
                    }
                }
                else
                {
                    // Usar Ca(NO3)2 para todo el calcio
                    var resultado = CalcularFertilizante("Ca(NO3)2.2H2O", "Ca", caRequerido);
                    resultados.Add(resultado);

                    nutrientesRestantes["N"] = Math.Max(0, nutrientesRestantes["N"] - resultado.N);
                    nutrientesRestantes["Ca"] = 0;
                }
            }

            // 3. MgSO4.7H2O para magnesio
            if (nutrientesRestantes.ContainsKey("Mg") && nutrientesRestantes["Mg"] > 0)
            {
                var resultado = CalcularFertilizante("MgSO4.7H2O", "Mg", nutrientesRestantes["Mg"]);
                resultados.Add(resultado);

                nutrientesRestantes["Mg"] = 0;
                if (nutrientesRestantes.ContainsKey("S"))
                    nutrientesRestantes["S"] = Math.Max(0, nutrientesRestantes["S"] - resultado.S);
            }

            // 4. KNO3 para nitrógeno restante (si queda)
            if (nutrientesRestantes.ContainsKey("N") && nutrientesRestantes["N"] > 0)
            {
                var resultado = CalcularFertilizante("KNO3", "N", nutrientesRestantes["N"]);
                resultados.Add(resultado);

                nutrientesRestantes["N"] = 0;
                if (nutrientesRestantes.ContainsKey("K"))
                    nutrientesRestantes["K"] = Math.Max(0, nutrientesRestantes["K"] - resultado.K);
            }

            // 5. Decisión inteligente para Potasio restante
            double kRequerido = nutrientesRestantes.GetValueOrDefault("K", 0);
            double sRequerido = nutrientesRestantes.GetValueOrDefault("S", 0);

            if (kRequerido > 0)
            {
                if (sRequerido > 0)
                {
                    // Evaluar si K2SO4 puede cubrir ambos
                    double k2so4ParaS = CalcularCantidadFertilizante("K2SO4", "S", sRequerido);
                    double kDeK2SO4 = CalcularContribucionElemento(k2so4ParaS, "K2SO4", "K");

                    if (kDeK2SO4 <= kRequerido * 1.2) // Si no excede mucho el K requerido
                    {
                        var resultado = CalcularFertilizante("K2SO4", "S", sRequerido);
                        resultados.Add(resultado);
                        nutrientesRestantes["S"] = 0;
                        nutrientesRestantes["K"] = Math.Max(0, nutrientesRestantes["K"] - resultado.K);
                    }
                    else
                    {
                        // Usar K2SO4 para todo el K requerido
                        var resultado = CalcularFertilizante("K2SO4", "K", kRequerido);
                        resultados.Add(resultado);
                        nutrientesRestantes["K"] = 0;
                        nutrientesRestantes["S"] = Math.Max(0, nutrientesRestantes["S"] - resultado.S);
                    }
                }
                else
                {
                    // No se necesita azufre, usar K2SO4 para K
                    var resultado = CalcularFertilizante("K2SO4", "K", kRequerido);
                    resultados.Add(resultado);
                    nutrientesRestantes["K"] = 0;
                }
            }

            // 6. Completar calcio restante con CaCl2 si es necesario
            if (nutrientesRestantes.GetValueOrDefault("Ca", 0) > 0)
            {
                var resultado = CalcularFertilizante("CaCl2.2H2O", "Ca", nutrientesRestantes["Ca"]);
                resultados.Add(resultado);
                nutrientesRestantes["Ca"] = 0;
            }

            // 7. Completar potasio restante con KCl si es necesario
            if (nutrientesRestantes.GetValueOrDefault("K", 0) > 0)
            {
                var resultado = CalcularFertilizante("KCl", "K", nutrientesRestantes["K"]);
                resultados.Add(resultado);
                nutrientesRestantes["K"] = 0;
            }

            // 8. Agregar micronutrientes
            resultados.AddRange(CalcularMicronutrientes());

            return resultados;
        }

        private List<ResultadoFertilizante> CalcularMicronutrientes()
        {
            var resultadosMicros = new List<ResultadoFertilizante>();

            // Micronutrientes con concentraciones típicas
            var objetivosMicros = new Dictionary<string, double>
            {
                ["Fe"] = 1.0,
                ["Mn"] = 0.5,
                ["Zn"] = 0.2,
                ["Cu"] = 0.1,
                ["B"] = 0.5,
                ["Mo"] = 0.01
            };

            // Fuentes preferidas para cada micronutriente
            var fuentesPreferidas = new Dictionary<string, string>
            {
                ["Fe"] = "FeEDTA",
                ["Mn"] = "MnSO4.4H2O",
                ["Zn"] = "ZnSO4.7H2O",
                ["Cu"] = "CuSO4.5H2O",
                ["B"] = "H3BO3",
                ["Mo"] = "Na2MoO4.2H2O"
            };

            foreach (var micro in objetivosMicros)
            {
                string fuente = fuentesPreferidas[micro.Key];
                double contenidoEnAgua = analisisAgua.Elementos_mgL.GetValueOrDefault(micro.Key, 0);
                double necesario = Math.Max(0, micro.Value - contenidoEnAgua);

                if (necesario > 0)
                {
                    var resultado = CalcularFertilizante(fuente, micro.Key, necesario);
                    resultadosMicros.Add(resultado);
                }
            }

            return resultadosMicros;
        }

        private double CalcularContribucionNutriente(string nombreFertilizante, string nutrienteACalcular, double cantidadObjetivo, string nutrientePrimario)
        {
            if (fertilizantes == null) return 0;
            var fertilizante = fertilizantes.FirstOrDefault(f => f.Nombre == nombreFertilizante);
            if (fertilizante == null) return 0;

            // Calcular cantidad de fertilizante necesaria para el nutriente primario
            double pmElementoPrimario = fertilizante.Elementos[nutrientePrimario];
            double cantidadFertilizante = cantidadObjetivo * fertilizante.PesoMolecular * 100.0 / (pmElementoPrimario * fertilizante.Pureza);

            // Calcular contribución del nutriente secundario
            if (fertilizante.Elementos.ContainsKey(nutrienteACalcular))
            {
                double pmElementoSecundario = fertilizante.Elementos[nutrienteACalcular];
                return cantidadFertilizante * pmElementoSecundario * (fertilizante.Pureza / 100.0) / fertilizante.PesoMolecular;
            }

            return 0;
        }

        private double CalcularCantidadFertilizante(string nombreFertilizante, string elementoObjetivo, double cantidadObjetivo)
        {
            if (fertilizantes == null) return 0;
            var fertilizante = fertilizantes.FirstOrDefault(f => f.Nombre == nombreFertilizante);
            if (fertilizante == null) return 0;

            double pesoMolElemento = fertilizante.Elementos[elementoObjetivo];
            return cantidadObjetivo * fertilizante.PesoMolecular * 100.0 / (pesoMolElemento * fertilizante.Pureza);
        }

        private double CalcularContribucionElemento(double cantidadFertilizante, string nombreFertilizante, string elemento)
        {
            if (fertilizantes == null) return 0;
            var fertilizante = fertilizantes.FirstOrDefault(f => f.Nombre == nombreFertilizante);
            if (fertilizante == null || !fertilizante.Elementos.ContainsKey(elemento)) return 0;

            double pesoMolElemento = fertilizante.Elementos[elemento];
            return cantidadFertilizante * pesoMolElemento * (fertilizante.Pureza / 100.0) / fertilizante.PesoMolecular;
        }

        private ResultadoFertilizante CalcularFertilizante(string nombreFertilizante, string elementoObjetivo, double cantidadObjetivo)
        {
            if (fertilizantes == null)
                throw new InvalidOperationException("La lista de fertilizantes no está inicializada.");

            var fertilizante = fertilizantes.FirstOrDefault(f => f.Nombre == nombreFertilizante);
            if (fertilizante == null)
                throw new InvalidOperationException($"Fertilizante {nombreFertilizante} no encontrado.");

            var resultado = new ResultadoFertilizante
            {
                Nombre = nombreFertilizante,
                Pureza = fertilizante.Pureza,
                PesoMolecular = fertilizante.PesoMolecular
            };

            // Calcular concentración de sal necesaria
            // Fórmula: Concentración_sal = objetivo_mg/L × PM_sal × 100 / (PM_elemento × pureza%)
            double pesoMolElemento = fertilizante.Elementos[elementoObjetivo];
            resultado.ConcentracionSal_mgL = cantidadObjetivo * fertilizante.PesoMolecular * 100.0 / (pesoMolElemento * fertilizante.Pureza);
            resultado.ConcentracionSal_mmolL = resultado.ConcentracionSal_mgL / fertilizante.PesoMolecular;

            // Calcular aportes de cada elemento según el fertilizante
            CalcularTodasContribucionesElementos(resultado, fertilizante);

            // Asegurar que el elemento objetivo tenga el valor correcto
            EstablecerValorElementoObjetivo(resultado, elementoObjetivo, cantidadObjetivo);

            return resultado;
        }

        private void CalcularTodasContribucionesElementos(ResultadoFertilizante resultado, Fertilizante fertilizante)
        {
            foreach (var elemento in fertilizante.Elementos)
            {
                double contribucion = CalcularContribucionElemento(resultado.ConcentracionSal_mgL, fertilizante.PesoMolecular, elemento.Value, fertilizante.Pureza);

                switch (elemento.Key)
                {
                    case "Ca": resultado.Ca = contribucion; break;
                    case "K": resultado.K = contribucion; break;
                    case "Mg": resultado.Mg = contribucion; break;
                    case "Na": resultado.Na = contribucion; break;
                    case "NH4": resultado.NH4 = contribucion; break;
                    case "N": resultado.N = contribucion; break;
                    case "P": resultado.P = contribucion; break;
                    case "S": resultado.S = contribucion; break;
                    case "Cl": resultado.Cl = contribucion; break;
                    case "Fe": resultado.Fe = contribucion; break;
                    case "Mn": resultado.Mn = contribucion; break;
                    case "Zn": resultado.Zn = contribucion; break;
                    case "Cu": resultado.Cu = contribucion; break;
                    case "B": resultado.B = contribucion; break;
                    case "Mo": resultado.Mo = contribucion; break;
                }
            }

            // Calcular formas iónicas
            if (resultado.N > 0)
            {
                resultado.NO3 = resultado.N * 62.00 / 14.01; // Conversión N a NO3-
            }

            if (resultado.P > 0)
            {
                resultado.H2PO4 = resultado.P * 96.99 / 30.97; // Conversión P a H2PO4-
            }

            if (resultado.S > 0)
            {
                resultado.SO4 = resultado.S * 96.06 / 32.06; // Conversión S a SO4=
            }
        }

        private void EstablecerValorElementoObjetivo(ResultadoFertilizante resultado, string elementoObjetivo, double cantidadObjetivo)
        {
            switch (elementoObjetivo)
            {
                case "Ca": resultado.Ca = cantidadObjetivo; break;
                case "K": resultado.K = cantidadObjetivo; break;
                case "Mg": resultado.Mg = cantidadObjetivo; break;
                case "N": resultado.N = cantidadObjetivo; resultado.NO3 = cantidadObjetivo * 62.00 / 14.01; break;
                case "P": resultado.P = cantidadObjetivo; resultado.H2PO4 = cantidadObjetivo * 96.99 / 30.97; break;
                case "S": resultado.S = cantidadObjetivo; resultado.SO4 = cantidadObjetivo * 96.06 / 32.06; break;
                case "Fe": resultado.Fe = cantidadObjetivo; break;
                case "Mn": resultado.Mn = cantidadObjetivo; break;
                case "Zn": resultado.Zn = cantidadObjetivo; break;
                case "Cu": resultado.Cu = cantidadObjetivo; break;
                case "B": resultado.B = cantidadObjetivo; break;
                case "Mo": resultado.Mo = cantidadObjetivo; break;
            }
        }

        private double CalcularContribucionElemento(double concentracionSal, double pesoMolSal, double pesoMolElemento, double pureza)
        {
            return concentracionSal * pesoMolElemento * (pureza / 100.0) / pesoMolSal;
        }

        public BalanceIonico CalcularBalanceIonico(List<ResultadoFertilizante> resultados)
        {
            var balance = new BalanceIonico();

            // Inicializar diccionarios
            string[] iones = { "Ca", "K", "Mg", "Na", "NH4", "NO3", "N", "SO4", "S", "Cl", "H2PO4", "P", "HCO3", "Fe", "Mn", "Zn", "Cu", "B", "Mo" };

            foreach (string ion in iones)
            {
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
                if (analisisAgua.Elementos_mgL.ContainsKey(ion))
                {
                    balance.Agua_mgL[ion] = analisisAgua.Elementos_mgL[ion];
                }
            }

            // Agregar bicarbonatos del agua
            balance.Agua_mgL["HCO3"] = analisisAgua.HCO3;

            // Sumar aportes de fertilizantes
            foreach (var resultado in resultados)
            {
                // Cationes
                balance.Aporte_mgL["Ca"] += resultado.Ca;
                balance.Aporte_mgL["K"] += resultado.K;
                balance.Aporte_mgL["Mg"] += resultado.Mg;
                balance.Aporte_mgL["Na"] += resultado.Na;
                balance.Aporte_mgL["NH4"] += resultado.NH4;

                // Aniones - tanto la forma iónica como elemental
                balance.Aporte_mgL["NO3"] += resultado.NO3;
                balance.Aporte_mgL["N"] += resultado.N;
                balance.Aporte_mgL["SO4"] += resultado.SO4;
                balance.Aporte_mgL["S"] += resultado.S;
                balance.Aporte_mgL["Cl"] += resultado.Cl;
                balance.Aporte_mgL["H2PO4"] += resultado.H2PO4;
                balance.Aporte_mgL["P"] += resultado.P;
                balance.Aporte_mgL["HCO3"] += resultado.HCO3;

                // Micronutrientes
                balance.Aporte_mgL["Fe"] += resultado.Fe;
                balance.Aporte_mgL["Mn"] += resultado.Mn;
                balance.Aporte_mgL["Zn"] += resultado.Zn;
                balance.Aporte_mgL["Cu"] += resultado.Cu;
                balance.Aporte_mgL["B"] += resultado.B;
                balance.Aporte_mgL["Mo"] += resultado.Mo;
            }

            // Calcular concentraciones finales y conversiones
            foreach (string ion in iones)
            {
                // Final = Aporte + Agua
                balance.Final_mgL[ion] = balance.Aporte_mgL[ion] + balance.Agua_mgL[ion];

                // Convertir a mmol/L y meq/L
                if (datosElementos != null && datosElementos.ContainsKey(ion))
                {
                    var pesoAtomico = datosElementos[ion].PesoAtomico;
                    var valencia = datosElementos[ion].Valencia;

                    // Para aportes
                    balance.Aporte_mmolL[ion] = balance.Aporte_mgL[ion] / pesoAtomico;
                    balance.Aporte_meqL[ion] = balance.Aporte_mmolL[ion] * valencia;

                    // Para agua
                    balance.Agua_mmolL[ion] = balance.Agua_mgL[ion] / pesoAtomico;
                    balance.Agua_meqL[ion] = balance.Agua_mmolL[ion] * valencia;

                    // Para final
                    balance.Final_mmolL[ion] = balance.Final_mgL[ion] / pesoAtomico;
                    balance.Final_meqL[ion] = balance.Final_mmolL[ion] * valencia;
                }
            }

            return balance;
        }

        public Dictionary<string, double> VerificarBalanceIonico(BalanceIonico balance)
        {
            var verificacion = new Dictionary<string, double>();

            // Calcular suma de cationes y aniones
            double sumaCationes = 0;
            double sumaAniones = 0;

            var cationes = new[] { "Ca", "K", "Mg", "Na", "NH4", "Fe", "Mn", "Zn", "Cu" };
            var aniones = new[] { "NO3", "SO4", "Cl", "H2PO4", "HCO3", "B", "Mo" };

            foreach (var cation in cationes)
            {
                sumaCationes += balance.Final_meqL.GetValueOrDefault(cation, 0);
            }

            foreach (var anion in aniones)
            {
                sumaAniones += balance.Final_meqL.GetValueOrDefault(anion, 0);
            }

            verificacion["SumaCationes_meqL"] = sumaCationes;
            verificacion["SumaAniones_meqL"] = sumaAniones;
            verificacion["Diferencia_meqL"] = Math.Abs(sumaCationes - sumaAniones);
            verificacion["DiferenciaPorcentual"] = sumaCationes > 0 ? (verificacion["Diferencia_meqL"] / sumaCationes) * 100 : 0;
            verificacion["EstaBalanceado"] = verificacion["DiferenciaPorcentual"] <= 10.0 ? 1 : 0;
            verificacion["Tolerancia_meqL"] = Math.Min(sumaCationes, sumaAniones) * 0.1; // 10% de tolerancia

            // Calcular relaciones importantes
            double ca_meq = balance.Final_meqL.GetValueOrDefault("Ca", 0);
            double k_meq = balance.Final_meqL.GetValueOrDefault("K", 0);
            double mg_meq = balance.Final_meqL.GetValueOrDefault("Mg", 0);

            if (ca_meq > 0 && mg_meq > 0)
            {
                verificacion["RelacionK_Ca"] = k_meq / ca_meq;
                verificacion["RelacionCa_Mg"] = ca_meq / mg_meq;
            }

            // Calcular CE estimada (método simplificado)
            verificacion["CE_Estimada_dSm"] = sumaCationes * 0.1; // Método aproximado

            return verificacion;
        }

        public Dictionary<string, object> AnalisisNutricional(BalanceIonico balance)
        {
            var analisis = new Dictionary<string, object>();

            // Análisis de macronutrientes
            var macronutrientes = new Dictionary<string, object>();
            var macros = new[] { "N", "P", "K", "Ca", "Mg", "S" };

            foreach (var macro in macros)
            {
                double objetivo = concentracionesObjetivo.GetValueOrDefault(macro, 0);
                double final = balance.Final_mgL.GetValueOrDefault(macro, 0);
                double desviacion = final - objetivo;
                double desviacionPorcentual = objetivo > 0 ? (desviacion / objetivo) * 100 : 0;

                macronutrientes[macro] = new
                {
                    Objetivo_mgL = objetivo,
                    Final_mgL = final,
                    DelAgua_mgL = balance.Agua_mgL.GetValueOrDefault(macro, 0),
                    DeFertilizantes_mgL = balance.Aporte_mgL.GetValueOrDefault(macro, 0),
                    Desviacion_mgL = desviacion,
                    DesviacionPorcentual = desviacionPorcentual,
                    Estado = Math.Abs(desviacionPorcentual) <= 5 ? "Óptimo" :
                            Math.Abs(desviacionPorcentual) <= 10 ? "Aceptable" :
                            Math.Abs(desviacionPorcentual) <= 20 ? "Precaución" : "Crítico"
                };
            }
            analisis["Macronutrientes"] = macronutrientes;

            // Análisis de micronutrientes
            var micronutrientes = new Dictionary<string, object>();
            var micros = new[] { "Fe", "Mn", "Zn", "Cu", "B", "Mo" };
            var objetivosMicros = new Dictionary<string, double>
            {
                ["Fe"] = 1.0,
                ["Mn"] = 0.5,
                ["Zn"] = 0.2,
                ["Cu"] = 0.1,
                ["B"] = 0.5,
                ["Mo"] = 0.01
            };

            foreach (var micro in micros)
            {
                double objetivo = objetivosMicros.GetValueOrDefault(micro, 0);
                double final = balance.Final_mgL.GetValueOrDefault(micro, 0);
                double desviacion = final - objetivo;
                double desviacionPorcentual = objetivo > 0 ? (desviacion / objetivo) * 100 : 0;

                micronutrientes[micro] = new
                {
                    Objetivo_mgL = objetivo,
                    Final_mgL = final,
                    DeFertilizantes_mgL = balance.Aporte_mgL.GetValueOrDefault(micro, 0),
                    Desviacion_mgL = desviacion,
                    DesviacionPorcentual = desviacionPorcentual,
                    Estado = Math.Abs(desviacionPorcentual) <= 10 ? "Óptimo" :
                            Math.Abs(desviacionPorcentual) <= 25 ? "Aceptable" : "Revisar"
                };
            }
            analisis["Micronutrientes"] = micronutrientes;

            // Análisis de formas iónicas
            var formasIonicas = new Dictionary<string, object>();

            // NH4/N total
            double nh4_mg = balance.Final_mgL.GetValueOrDefault("NH4", 0);
            double nTotal_mg = balance.Final_mgL.GetValueOrDefault("N", 0);
            double nh4_equivalente = nh4_mg * 14.01 / 18.04; // Convertir NH4 a equivalente de N
            double porcentajeNH4 = nTotal_mg > 0 ? (nh4_equivalente / nTotal_mg) * 100 : 0;

            formasIonicas["NH4_PorcentajeDelN"] = new
            {
                PorcentajeNH4 = porcentajeNH4,
                Limite = 20.0,
                Estado = porcentajeNH4 <= 20 ? "Seguro" : "Alto - Riesgo Fitotoxicidad",
                Recomendacion = porcentajeNH4 <= 20 ? "Nivel aceptable de amonio" : "Reducir fuentes de NH4+"
            };

            analisis["FormastIonicas"] = formasIonicas;

            return analisis;
        }

        // Continuación de la clase con métodos adicionales...

        public List<Fertilizante> ObtenerFertilizantesPorCategoria(string categoria)
        {
            if (fertilizantes == null) return new List<Fertilizante>();

            return categoria.ToLower() switch
            {
                "macronutrientes" => fertilizantes.Where(f =>
                    f.Elementos.ContainsKey("N") || f.Elementos.ContainsKey("P") ||
                    f.Elementos.ContainsKey("K") || f.Elementos.ContainsKey("Ca") ||
                    f.Elementos.ContainsKey("Mg") || f.Elementos.ContainsKey("S")).ToList(),

                "micronutrientes" => fertilizantes.Where(f =>
                    f.Elementos.ContainsKey("Fe") || f.Elementos.ContainsKey("Mn") ||
                    f.Elementos.ContainsKey("Zn") || f.Elementos.ContainsKey("Cu") ||
                    f.Elementos.ContainsKey("B") || f.Elementos.ContainsKey("Mo")).ToList(),

                "quelatos" => fertilizantes.Where(f =>
                    f.Nombre.Contains("EDTA") || f.Nombre.Contains("EDDHA") ||
                    f.Nombre.Contains("DTPA")).ToList(),

                "acidos" => fertilizantes.Where(f =>
                    f.Nombre.Contains("Ácido") || f.Nombre.Contains("ácido")).ToList(),

                "solubles" => fertilizantes.Where(f => f.Solubilidad > 100).ToList(),

                "economicos" => fertilizantes.Where(f => f.Costo < 2.0).OrderBy(f => f.Costo).ToList(),

                _ => fertilizantes.ToList()
            };
        }

        public Dictionary<string, List<string>> ObtenerAlternativasFertilizantes()
        {
            var alternativas = new Dictionary<string, List<string>>();

            // Alternativas para Nitrógeno
            alternativas["N"] = new List<string>
            {
                "Ca(NO3)2.2H2O", "KNO3", "NH4NO3", "(NH4)2SO4",
                "NH4H2PO4", "(NH4)2HPO4", "NaNO3", "Ácido nítrico"
            };

            // Alternativas para Fósforo
            alternativas["P"] = new List<string>
            {
                "KH2PO4", "NH4H2PO4", "(NH4)2HPO4", "K2HPO4.3H2O",
                "Ca(H2PO4)2.H2O", "Ácido fosfórico"
            };

            // Alternativas para Potasio
            alternativas["K"] = new List<string>
            {
                "KNO3", "K2SO4", "KCl", "KH2PO4", "K2HPO4.3H2O",
                "KOH", "K2CO3"
            };

            // Alternativas para Calcio
            alternativas["Ca"] = new List<string>
            {
                "Ca(NO3)2.2H2O", "CaCl2.2H2O", "CaSO4.2H2O",
                "Ca(H2PO4)2.H2O", "CaO", "Ca(OH)2"
            };

            // Alternativas para Magnesio
            alternativas["Mg"] = new List<string>
            {
                "MgSO4.7H2O", "MgCl2.6H2O", "MgO"
            };

            // Alternativas para Azufre
            alternativas["S"] = new List<string>
            {
                "MgSO4.7H2O", "K2SO4", "(NH4)2SO4", "CaSO4.2H2O",
                "FeSO4.7H2O", "MnSO4.4H2O", "ZnSO4.7H2O", "CuSO4.5H2O"
            };

            // Alternativas para Hierro
            alternativas["Fe"] = new List<string>
            {
                "FeEDTA", "FeSO4.7H2O", "FeCl3.6H2O",
                "FeEDDHA", "FeDTPA"
            };

            // Alternativas para Manganeso
            alternativas["Mn"] = new List<string>
            {
                "MnSO4.4H2O", "MnCl2.4H2O", "MnEDTA"
            };

            // Alternativas para Zinc
            alternativas["Zn"] = new List<string>
            {
                "ZnSO4.7H2O", "ZnCl2", "ZnEDTA"
            };

            // Alternativas para Cobre
            alternativas["Cu"] = new List<string>
            {
                "CuSO4.5H2O", "CuCl2.2H2O", "CuEDTA"
            };

            // Alternativas para Boro
            alternativas["B"] = new List<string>
            {
                "H3BO3", "Na2B4O7.10H2O"
            };

            // Alternativas para Molibdeno
            alternativas["Mo"] = new List<string>
            {
                "Na2MoO4.2H2O", "(NH4)6Mo7O24.4H2O"
            };

            return alternativas;
        }

        public Dictionary<string, string> ObtenerRecomendacionesUso()
        {
            var recomendaciones = new Dictionary<string, string>();

            recomendaciones["Ca(NO3)2.2H2O"] = "Fertilizante base para calcio y nitrógeno. Muy soluble. Separar de sulfatos y fosfatos en tanques concentrados.";
            recomendaciones["KH2PO4"] = "Excelente fuente de fósforo y potasio. Usar siempre como primera opción para P. Compatible con la mayoría de fertilizantes.";
            recomendaciones["KNO3"] = "Fuente limpia de K y N. Ideal para ajustes finales. Puede cristalizar a bajas temperaturas.";
            recomendaciones["MgSO4.7H2O"] = "Sal de Epsom. Muy económica y soluble. Fuente principal de magnesio y azufre.";
            recomendaciones["FeEDTA"] = "Quelato estable hasta pH 6.5. Más caro que FeSO4 pero más eficiente. Usar en sistemas hidropónicos.";
            recomendaciones["FeSO4.7H2O"] = "Económico pero se oxida rápidamente. Usar en tanque separado. Requiere pH ácido para estabilidad.";
            recomendaciones["K2SO4"] = "Fuente de K y S libre de cloro. Ideal para cultivos sensibles al Cl. Solubilidad limitada.";
            recomendaciones["NH4NO3"] = "Fuente muy económica de N. Cuidado con toxicidad por NH4. Máximo 20% del N total.";
            recomendaciones["(NH4)2SO4"] = "Aporta N y S. Acidifica el medio. Usar con moderación por contenido de amonio.";
            recomendaciones["KCl"] = "Fuente económica de K pero aporta cloro. Evitar en cultivos sensibles (tomate, pepino).";
            recomendaciones["CaCl2.2H2O"] = "Alternativa al nitrato de calcio. Útil cuando se quiere Ca sin N adicional.";

            // Fosfatos especializados
            recomendaciones["NH4H2PO4"] = "MAP - Buena fuente de N y P. Acidifica la solución. Cuidado con el NH4.";
            recomendaciones["(NH4)2HPO4"] = "DAP - Alto contenido de N. Usar solo si se necesita mucho N y P juntos.";
            recomendaciones["K2HPO4.3H2O"] = "Fosfato dipotásico. Para cuando se necesita mucho K y algo de P. Muy soluble.";
            recomendaciones["Ca(H2PO4)2.H2O"] = "Superfosfato. Baja solubilidad. Solo para aplicaciones específicas en suelo.";

            // Micronutrientes - Sulfatos
            recomendaciones["MnSO4.4H2O"] = "Fuente estándar de Mn. Soluble y económica. Compatible con la mayoría de mezclas.";
            recomendaciones["ZnSO4.7H2O"] = "Fuente común de Zn. Buena solubilidad. Puede precipitar en pH alto.";
            recomendaciones["CuSO4.5H2O"] = "Fuente tradicional de Cu. Usar con precaución - el Cu es fitotóxico en exceso.";

            // Micronutrientes - Cloruros
            recomendaciones["MnCl2.4H2O"] = "Alternativa al sulfato. Más soluble pero aporta cloruro.";
            recomendaciones["ZnCl2"] = "Muy soluble pero aporta Cl. Solo usar si se tolera el cloruro.";
            recomendaciones["CuCl2.2H2O"] = "Más soluble que el sulfato. Evaluar tolerancia al Cl del cultivo.";
            recomendaciones["FeCl3.6H2O"] = "Alternativa económica. Se hidroliza fácilmente - usar en pH ácido.";

            // Quelatos especializados
            recomendaciones["FeEDDHA"] = "Quelato más estable para pH alto (hasta 9.0). Muy caro pero efectivo en suelos calcáreos.";
            recomendaciones["FeDTPA"] = "Estabilidad intermedia (hasta pH 7.5). Buen compromiso costo-efectividad.";
            recomendaciones["MnEDTA"] = "Quelato de Mn para pH alto. Previene deficiencias en sistemas alcalinos.";
            recomendaciones["ZnEDTA"] = "Quelato de Zn muy estable. Ideal para corrección rápida de deficiencias.";
            recomendaciones["CuEDTA"] = "Quelato de Cu. Reduce riesgo de fitotoxicidad comparado con sulfatos.";

            // Boro
            recomendaciones["H3BO3"] = "Ácido bórico. Fuente más común de B. Soluble pero no en exceso. Fácil de sobre-dosificar.";
            recomendaciones["Na2B4O7.10H2O"] = "Bórax. Alternativa al ácido bórico. Aporta sodio - considerar en análisis iónico.";

            // Molibdeno
            recomendaciones["Na2MoO4.2H2O"] = "Molibdato de sodio. Muy soluble. Aporta Na - considerar en balance iónico.";
            recomendaciones["(NH4)6Mo7O24.4H2O"] = "Molibdato de amonio. Aporta N adicional. Más económico por unidad de Mo.";

            // Ácidos
            recomendaciones["Ácido nítrico"] = "Para ajuste de pH y aporte de N. Muy corrosivo - manejar con extrema precaución.";
            recomendaciones["Ácido fosfórico"] = "Ajuste de pH + aporte de P. Menos corrosivo que nítrico. Ideal para aguas con bicarbonatos altos.";

            // Fertilizantes especiales
            recomendaciones["CaSO4.2H2O"] = "Yeso. Muy poco soluble. Solo para aplicaciones de liberación lenta en sustrato.";
            recomendaciones["MgO"] = "Óxido de magnesio. Muy poco soluble. Solo para correcciones de pH en sustrato.";
            recomendaciones["KOH"] = "Hidróxido de potasio. Para subir pH y aportar K. Muy cáustico - usar con precaución.";
            recomendaciones["K2CO3"] = "Carbonato de potasio. Alternativa menos cáustica al KOH para subir pH.";
            recomendaciones["NaNO3"] = "Nitrato de sodio. Evitar en hidroponía - el Na se acumula. Solo para emergencias.";
            recomendaciones["CaO"] = "Cal viva. Reacciona violentamente con agua. Solo para preparación de sustratos.";
            recomendaciones["Ca(OH)2"] = "Cal apagada. Para subir pH en sustratos. No usar en soluciones nutritivas.";

            // Fertilizantes orgánicos
            recomendaciones["Aminoácidos Quelados"] = "Bioestimulante + quelación. Mejora absorción de nutrientes. Costoso pero efectivo.";
            recomendaciones["Ácidos Húmicos"] = "Mejorador de sustrato. Aumenta CIC y retención de nutrientes. Usar como complemento.";

            // Cloruros de magnesio
            recomendaciones["MgCl2.6H2O"] = "Alternativa al sulfato de Mg. Más soluble pero aporta Cl. Evaluar tolerancia del cultivo.";

            return recomendaciones;
        }

        public Dictionary<string, string> ObtenerPrecaucionesSeguridad()
        {
            var precauciones = new Dictionary<string, string>();

            // Ácidos - Máxima precaución
            precauciones["Ácido nítrico"] = "⚠️ PELIGRO EXTREMO: Muy corrosivo. Usar gafas, guantes de nitrilo, delantal. Ventilación obligatoria. SIEMPRE ácido al agua.";
            precauciones["Ácido fosfórico"] = "⚠️ CORROSIVO: Menos peligroso que nítrico pero requiere EPP completo. Puede causar quemaduras severas.";

            // Bases fuertes
            precauciones["KOH"] = "⚠️ MUY CÁUSTICO: Hidróxido muy alcalino. Causa quemaduras instantáneas. EPP completo obligatorio.";
            precauciones["CaO"] = "⚠️ PELIGROSO: Cal viva reacciona violentamente con agua generando calor extremo. Protección respiratoria.";
            precauciones["Ca(OH)2"] = "⚠️ IRRITANTE: Cal apagada. Polvo muy irritante para vías respiratorias y ojos. Usar mascarilla.";

            // Fertilizantes con amonio
            precauciones["NH4NO3"] = "⚠️ OXIDANTE: Nitrato de amonio es oxidante fuerte. Mantener alejado de combustibles. Almacenar en seco.";
            precauciones["(NH4)2SO4"] = "⚠️ IRRITANTE: Polvo irritante. El NH4 puede ser fitotóxico en concentraciones altas.";
            precauciones["NH4H2PO4"] = "⚠️ PRECAUCIÓN: MAP puede generar calor al disolverse. Agregar lentamente mientras se agita.";
            precauciones["(NH4)2HPO4"] = "⚠️ PRECAUCIÓN: DAP muy alcalino en solución. Puede precipitar micronutrientes.";

            // Cloruros - Corrosión
            precauciones["CaCl2.2H2O"] = "⚠️ CORROSIVO: Acelera corrosión de metales. Usar equipos resistentes. Higroscópico - almacenar seco.";
            precauciones["MgCl2.6H2O"] = "⚠️ CORROSIVO: Muy corrosivo para metales. Evitar contacto con hierro/acero sin protección.";
            precauciones["KCl"] = "⚠️ CORROSIVO: Facilita corrosión. Puede cristalizar en tuberías a bajas temperaturas.";
            precauciones["ZnCl2"] = "⚠️ CORROSIVO E IRRITANTE: Muy corrosivo y tóxico. Usar guantes, evitar inhalación.";
            precauciones["CuCl2.2H2O"] = "⚠️ TÓXICO: Compuesto de cobre tóxico. Evitar inhalación y contacto con piel.";
            precauciones["FeCl3.6H2O"] = "⚠️ CORROSIVO: Mancha permanentemente. Muy corrosivo para metales y textiles.";

            // Sulfatos de micronutrientes
            precauciones["CuSO4.5H2O"] = "⚠️ TÓXICO: Sulfato de cobre es fungicida y fitotóxico en exceso. Usar guantes, evitar inhalación.";
            precauciones["ZnSO4.7H2O"] = "⚠️ IRRITANTE: Puede causar irritación de piel y ojos. El Zn es tóxico en exceso.";
            precauciones["MnSO4.4H2O"] = "⚠️ IRRITANTE: Polvo irritante. El Mn puede ser neurotóxico en concentraciones muy altas.";
            precauciones["FeSO4.7H2O"] = "⚠️ SE OXIDA: Se oxida rápidamente al aire. Almacenar en recipientes herméticos. Mancha permanentemente.";

            // Boro - Toxicidad
            precauciones["H3BO3"] = "⚠️ TÓXICO: Ácido bórico es tóxico por ingestión. Margen estrecho entre deficiencia y toxicidad.";
            precauciones["Na2B4O7.10H2O"] = "⚠️ TÓXICO: Bórax tóxico por ingestión. Fácil de sobre-dosificar. Usar balanza de precisión.";

            // Molibdeno
            precauciones["Na2MoO4.2H2O"] = "⚠️ TÓXICO EN EXCESO: Mo es tóxico para rumiantes en exceso. Dosis muy pequeñas - usar balanza analítica.";
            precauciones["(NH4)6Mo7O24.4H2O"] = "⚠️ TÓXICO: Compuesto de Mo. Usar cantidades muy pequeñas. Almacenar en lugar seco.";

            // Quelatos
            precauciones["FeEDTA"] = "✓ SEGURO: Quelato seguro. Baja toxicidad. Puede manchar - usar guantes.";
            precauciones["FeEDDHA"] = "⚠️ COSTOSO: Producto muy caro. Evitar desperdicios. No tóxico pero usar con precisión.";
            precauciones["FeDTPA"] = "✓ SEGURO: Quelato estable y seguro. Mínima toxicidad.";

            // Fertilizantes seguros
            precauciones["Ca(NO3)2.2H2O"] = "✓ SEGURO: Fertilizante muy seguro. No tóxico. Puede cristalizar - mantener en solución.";
            precauciones["KH2PO4"] = "✓ SEGURO: Muy seguro de manejar. Grado alimentario. No presenta riesgos especiales.";
            precauciones["KNO3"] = "✓ SEGURO: Fertilizante seguro. Puede cristalizar a bajas temperaturas.";
            precauciones["MgSO4.7H2O"] = "✓ SEGURO: Sal de Epsom. Grado farmacéutico disponible. Muy seguro.";
            precauciones["K2SO4"] = "✓ SEGURO: Fertilizante seguro. No presenta riesgos especiales de manejo.";

            // Recomendaciones generales de seguridad
            precauciones["GENERAL"] = @"
📋 PRECAUCIONES GENERALES:
• Usar siempre EPP: Gafas, guantes de nitrilo, ropa protectora
• Ventilación adecuada en área de preparación
• Balanza de precisión para micronutrientes (±0.1g mínimo)
• Agua limpia para lavado de emergencia
• Botiquín de primeros auxilios
• Fichas de seguridad (MSDS) de todos los productos
• Nunca mezclar productos en seco
• Siempre agregar fertilizantes al agua, no al revés
• Mantener registro de cantidades usadas
• Almacenar en lugar seco, fresco y ventilado
• Etiquetar todos los recipientes
• No comer, beber o fumar durante la preparación";

            return precauciones;
        }

        public Dictionary<string, Dictionary<string, double>> ObtenerTablaIncompatibilidades()
        {
            var incompatibilidades = new Dictionary<string, Dictionary<string, double>>();

            // Niveles: 0=Compatible, 1=Precaución, 2=Incompatible, 3=Peligroso

            // Calcio vs Sulfatos y Fosfatos
            incompatibilidades["Ca(NO3)2.2H2O"] = new Dictionary<string, double>
            {
                ["K2SO4"] = 2, // Precipita CaSO4
                ["MgSO4.7H2O"] = 2, // Precipita CaSO4
                ["(NH4)2SO4"] = 2, // Precipita CaSO4
                ["KH2PO4"] = 2, // Precipita Ca3(PO4)2
                ["NH4H2PO4"] = 2, // Precipita Ca3(PO4)2
                ["(NH4)2HPO4"] = 2, // Precipita Ca3(PO4)2
                ["Ácido fosfórico"] = 2, // Precipita fosfatos de Ca
                ["FeSO4.7H2O"] = 1, // Puede precipitar FePO4
                ["MnSO4.4H2O"] = 1, // Precaución con sulfatos
                ["ZnSO4.7H2O"] = 1, // Precaución con sulfatos
                ["CuSO4.5H2O"] = 1 // Precaución con sulfatos
            };

            // Hierro vs pH alto y fosfatos
            incompatibilidades["FeSO4.7H2O"] = new Dictionary<string, double>
            {
                ["Ca(NO3)2.2H2O"] = 1, // Puede precipitar en pH alto
                ["KOH"] = 3, // Precipita Fe(OH)3 - muy peligroso
                ["Ca(OH)2"] = 3, // Precipita Fe(OH)3
                ["K2CO3"] = 2, // Puede precipitar carbonatos
                ["(NH4)2HPO4"] = 2, // DAP eleva pH, precipita Fe
                ["KH2PO4"] = 1, // Precaución con fosfatos
                ["Ácido fosfórico"] = 0 // Compatible - mantiene pH ácido
            };

            // Ácidos vs Bases
            incompatibilidades["Ácido nítrico"] = new Dictionary<string, double>
            {
                ["KOH"] = 3, // Reacción violenta - PELIGROSO
                ["Ca(OH)2"] = 3, // Reacción violenta
                ["CaO"] = 3, // Reacción extremadamente violenta
                ["K2CO3"] = 2, // Reacción efervescente
                ["(NH4)2HPO4"] = 1, // DAP es básico
                ["Na2B4O7.10H2O"] = 1 // Bórax es básico
            };

            incompatibilidades["Ácido fosfórico"] = new Dictionary<string, double>
            {
                ["KOH"] = 3, // Reacción violenta
                ["Ca(OH)2"] = 3, // Reacción violenta + precipitados
                ["CaO"] = 3, // Reacción extremadamente violenta
                ["Ca(NO3)2.2H2O"] = 2, // Precipita fosfatos de Ca
                ["CaCl2.2H2O"] = 2 // Precipita fosfatos de Ca
            };

            // Sulfatos vs Calcio
            incompatibilidades["K2SO4"] = new Dictionary<string, double>
            {
                ["Ca(NO3)2.2H2O"] = 2, // Precipita CaSO4
                ["CaCl2.2H2O"] = 2, // Precipita CaSO4
                ["Ca(OH)2"] = 2 // Precipita CaSO4
            };

            incompatibilidades["MgSO4.7H2O"] = new Dictionary<string, double>
            {
                ["Ca(NO3)2.2H2O"] = 2, // Precipita CaSO4
                ["CaCl2.2H2O"] = 2, // Precipita CaSO4
                ["KOH"] = 2, // Precipita Mg(OH)2
                ["Ca(OH)2"] = 2 // Precipita CaSO4 y Mg(OH)2
            };

            // Fosfatos vs Calcio y Hierro
            incompatibilidades["KH2PO4"] = new Dictionary<string, double>
            {
                ["Ca(NO3)2.2H2O"] = 2, // Precipita Ca3(PO4)2
                ["CaCl2.2H2O"] = 2, // Precipita Ca3(PO4)2
                ["FeSO4.7H2O"] = 1, // Puede precipitar FePO4
                ["MnSO4.4H2O"] = 1, // Puede precipitar Mn3(PO4)2
                ["ZnSO4.7H2O"] = 1 // Puede precipitar Zn3(PO4)2
            };

            // Amonio vs pH alto
            incompatibilidades["NH4NO3"] = new Dictionary<string, double>
            {
                ["KOH"] = 2, // Libera NH3 gaseoso
                ["Ca(OH)2"] = 2, // Libera NH3 gaseoso
                ["K2CO3"] = 1 // Puede liberar NH3
            };

            incompatibilidades["(NH4)2SO4"] = new Dictionary<string, double>
            {
                ["KOH"] = 2, // Libera NH3 gaseoso
                ["Ca(OH)2"] = 2, // Libera NH3 + precipita CaSO4
                ["Ca(NO3)2.2H2O"] = 2 // Precipita CaSO4
            };

            // Micronutrientes vs pH alto
            incompatibilidades["CuSO4.5H2O"] = new Dictionary<string, double>
            {
                ["KOH"] = 3, // Precipita Cu(OH)2 tóxico
                ["Ca(OH)2"] = 3, // Precipita Cu(OH)2
                ["(NH4)2HPO4"] = 2, // DAP eleva pH
                ["K2CO3"] = 2 // Precipita carbonatos
            };

            incompatibilidades["ZnSO4.7H2O"] = new Dictionary<string, double>
            {
                ["KOH"] = 2, // Precipita Zn(OH)2
                ["Ca(OH)2"] = 2, // Precipita Zn(OH)2 y CaSO4
                ["(NH4)2HPO4"] = 1, // Precaución con pH alto
                ["KH2PO4"] = 1 // Puede precipitar fosfatos
            };

            // Boro vs pH extremos
            incompatibilidades["H3BO3"] = new Dictionary<string, double>
            {
                ["KOH"] = 1, // Forma boratos
                ["Ca(OH)2"] = 1, // Forma boratos de Ca
                ["CaO"] = 2 // Reacción fuerte
            };

            return incompatibilidades;
        }

        public List<string> GenerarAdvertenciasCompatibilidad(List<string> fertilizantesSeleccionados)
        {
            var advertencias = new List<string>();
            var incompatibilidades = ObtenerTablaIncompatibilidades();

            for (int i = 0; i < fertilizantesSeleccionados.Count; i++)
            {
                for (int j = i + 1; j < fertilizantesSeleccionados.Count; j++)
                {
                    string fert1 = fertilizantesSeleccionados[i];
                    string fert2 = fertilizantesSeleccionados[j];

                    // Verificar en ambas direcciones
                    double nivelIncompatibilidad = 0;

                    if (incompatibilidades.ContainsKey(fert1) && incompatibilidades[fert1].ContainsKey(fert2))
                    {
                        nivelIncompatibilidad = incompatibilidades[fert1][fert2];
                    }
                    else if (incompatibilidades.ContainsKey(fert2) && incompatibilidades[fert2].ContainsKey(fert1))
                    {
                        nivelIncompatibilidad = incompatibilidades[fert2][fert1];
                    }

                    switch (nivelIncompatibilidad)
                    {
                        case 3:
                            advertencias.Add($"🚫 PELIGROSO: {fert1} + {fert2} - Reacción violenta. NUNCA mezclar.");
                            break;
                        case 2:
                            advertencias.Add($"⛔ INCOMPATIBLE: {fert1} + {fert2} - Forman precipitados. Usar tanques separados.");
                            break;
                        case 1:
                            advertencias.Add($"⚠️ PRECAUCIÓN: {fert1} + {fert2} - Compatibilidad limitada. Monitorear pH y precipitados.");
                            break;
                    }
                }
            }

            // Advertencias adicionales específicas
            if (fertilizantesSeleccionados.Any(f => f.Contains("NH4")) &&
                fertilizantesSeleccionados.Count(f => f.Contains("NH4")) > 1)
            {
                advertencias.Add("⚠️ AMONIO ALTO: Múltiples fuentes de NH4+. Mantener <20% del N total para evitar fitotoxicidad.");
            }

            if (fertilizantesSeleccionados.Any(f => f.Contains("Cl")) &&
                fertilizantesSeleccionados.Count(f => f.Contains("Cl")) > 1)
            {
                advertencias.Add("⚠️ CLORURO ALTO: Múltiples fuentes de Cl-. Verificar tolerancia del cultivo al cloruro.");
            }

            if (fertilizantesSeleccionados.Any(f => f.Contains("Cu")) ||
                fertilizantesSeleccionados.Any(f => f.Contains("Zn")))
            {
                advertencias.Add("⚠️ MICRONUTRIENTES: Cu y Zn son fitotóxicos en exceso. Usar balanza de precisión (±0.1g).");
            }

            return advertencias;
        }

        public Dictionary<string, object> GenerarResumenFormulacion(List<ResultadoFertilizante> resultados)
        {
            var resumen = new Dictionary<string, object>();

            // Estadísticas básicas
            resumen["TotalFertilizantes"] = resultados.Count;
            resumen["ConcentracionTotalSales"] = resultados.Sum(r => r.ConcentracionSal_mgL);

            // Costo total estimado
            double costoTotal = 0;
            if (fertilizantes != null)
            {
                foreach (var resultado in resultados)
                {
                    var fertilizante = fertilizantes.FirstOrDefault(f => f.Nombre == resultado.Nombre);
                    if (fertilizante != null)
                    {
                        costoTotal += (resultado.ConcentracionSal_mgL / 1000.0) * fertilizante.Costo;
                    }
                }
            }
            resumen["CostoEstimadoPorLitro"] = costoTotal;

            // Categorización de fertilizantes
            var categorizacion = new Dictionary<string, List<string>>();
            categorizacion["Macronutrientes"] = new List<string>();
            categorizacion["Micronutrientes"] = new List<string>();
            categorizacion["Quelatos"] = new List<string>();
            categorizacion["Ácidos"] = new List<string>();

            foreach (var resultado in resultados)
            {
                if (resultado.Nombre.Contains("EDTA") || resultado.Nombre.Contains("EDDHA") || resultado.Nombre.Contains("DTPA"))
                {
                    categorizacion["Quelatos"].Add(resultado.Nombre);
                }
                else if (resultado.Nombre.Contains("Ácido") || resultado.Nombre.Contains("ácido"))
                {
                    categorizacion["Ácidos"].Add(resultado.Nombre);
                }
                else if (resultado.Fe > 0 || resultado.Mn > 0 || resultado.Zn > 0 ||
                         resultado.Cu > 0 || resultado.B > 0 || resultado.Mo > 0)
                {
                    categorizacion["Micronutrientes"].Add(resultado.Nombre);
                }
                else
                {
                    categorizacion["Macronutrientes"].Add(resultado.Nombre);
                }
            }
            resumen["Categorizacion"] = categorizacion;

            // Nivel de complejidad
            string complejidad = resultados.Count switch
            {
                <= 5 => "Básica",
                <= 8 => "Intermedia",
                <= 12 => "Avanzada",
                _ => "Muy Compleja"
            };
            resumen["NivelComplejidad"] = complejidad;

            // Advertencias de compatibilidad
            var nombresUtilizados = resultados.Select(r => r.Nombre).ToList();
            var advertenciasCompatibilidad = GenerarAdvertenciasCompatibilidad(nombresUtilizados);
            resumen["AdvertenciasCompatibilidad"] = advertenciasCompatibilidad;

            return resumen;
        }
    }
}