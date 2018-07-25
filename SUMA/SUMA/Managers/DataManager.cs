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
         public void MakeEanProducteRelations()
        {
            for(int i = 0; i < productes.Count; ++i)
            {
                productes[i].codis_ean.Clear();
            }

            for(int i = 0; i < eans.Count; ++i)
            {
                Producte prod = GetProductePerCodiArticle(eans[i].codi_article);

                if (prod != null)
                {
                    prod.codis_ean.Add(eans[i]);
                }
            }
        }

        private Producte GetProductePerCodiArticle(string codi_article)
        {
            Producte ret = null;

            for (int i = 0; i < productes.Count; ++i)
            {
                if(productes[i].codi_article == codi_article)
                {
                    ret = productes[i];
                    break;
                }
            }

            return ret;
        }

        public string nom { get; set; }

        public FitxerTipusMoneda tipus_moneda;
        public DateTime data_creacio;
        public DateTime data_importacio;
        public int quantitat_registres = 0;

        Producte curr_prod = null;
        CodiEan  curr_ean = null;
        public List<Producte> productes = new List<Producte>();
        public List<CodiEan>  eans= new List<CodiEan>();
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
        public string                codi_article { get; set; }
        public bool                  marca_de_baixa { get; set; }
        public string                marca_de_baixa_str { get { return marca_de_baixa ? "S" : "N"; } }
        public string                descripcio { get; set; }
        public int                   unitats_caixa { get; set; }
        public int                   unitats_fraccio { get; set; }
        public bool                  marca_de_pes { get; set; }
        public string                marca_de_pes_str { get { return marca_de_pes ? "S" : "N"; } }
        public double                preu_unitari { get; set; }
        public double                preu_venta_public_recomanat { get; set; }
        public double                preu_de_fraccio { get; set; }
        public ProducteTipusIva      tipus_iva { get; set; }
        public int                   codi_familia { get; set; }
        public int                   codi_sub_familia { get; set; }
        public ProducteUnitatsMesura unitats_mesura { get; set; }
        public double                factor_de_conversio { get; set; }
        public string                reservat_futures_ampliacions { get; set; }

        public List<CodiEan>         codis_ean = new List<CodiEan>();
    }

    public class CodiEan
    {
        public string codi_article { get; set; }
        public string codi_ean { get; set; }
    }
}
