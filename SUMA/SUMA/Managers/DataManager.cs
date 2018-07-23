using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SUMA.Managers
{
    class DataManager : Singleton<DataManager>
    {
        public Fitxer NewFitxer()
        {
            fitxer = new Fitxer();
            return fitxer;
        }

        public Fitxer GetFitxer()
        {
            return fitxer;
        }

        private Fitxer fitxer = new Fitxer();
    }

    public enum FitxerTipusMoneda
    {
        PTES,
        EUROS,
    }

    public class Fitxer
    {
        public Producte NewProducte()
        {
            if (curr_prod != null)
            {
                productes.Add(curr_prod);
                curr_prod = null;
            }

            curr_prod = new Producte();
            return curr_prod;
        }

        public Producte GetProducte()
        {
            return curr_prod;
        }

        public FitxerTipusMoneda tipus_moneda;
        public DateTime data_creacio;
        public DateTime data_importacio;
        public int quantitat_registres = 0;

        Producte curr_prod = null;
        public List<Producte> productes = new List<Producte>();
    }

    public enum ProducteTipusIva
    {
        SUPER, RED, NORMAL, EXCEMPT,
    }

    public enum ProducteUnitatsMesura
    {
        GRAMS, METRES, LITRES,
    }

    public class Producte
    {
        public int codi_article = 0;
        public bool marca_de_baixa = false;
        public string descripcio { get; set; }
        public int                   unitats_caixa = 0;
        public int                   unitats_fraccio = 0;
        public string                marca_de_pes;
        public double                preu_unitari = 0.0f;
        public double                preu_venta_public_recomanat = 0.0f;
        public double                preu_de_fraccio = 0.0f;
        public ProducteTipusIva      tipus_iva;
        public int                   codi_familia = 0;
        public int                   codi_sub_familia = 0;
        public ProducteUnitatsMesura unitats_mesura;
        public double                factor_de_conversio = 0.0f;
        public string                reservat_futures_ampliacions;

        public List<CodiEan>         codis_ean = new List<CodiEan>();
    }

    public class CodiEan
    {
        public int codi_article = 0;
        public int codi_ean = 0;
    }
}
