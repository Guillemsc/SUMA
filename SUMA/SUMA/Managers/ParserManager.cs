using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SUMA.Managers
{
    delegate void ParseProcessTick(float progress);
    delegate void ParseFinished();

    class ParserManager : Singleton<ParserManager>
    {
        private bool parsing = false;

        void ParseLine(string line)
        {
            Fitxer f = Managers.DataManager.Instance.GetFitxer();
            
            int cursor_pos = 0;

            line = line.Replace("\n", "");
            line = line.Replace("\r", "");

            if (line[cursor_pos] == 'C')
            {
                ParseCapçalera(line, cursor_pos);
            }

            if (line[cursor_pos] == 'L')
            {
                Producte prod = f.NewProducte();
                ParseArticle(line, cursor_pos, ref prod);
            }

            if (line[cursor_pos] == 'E')
            {
                Producte prod = f.GetProducte();

                if (prod != null)
                {
                    CodiEan curr_ean = new CodiEan();

                    ParseCodiEan(line, cursor_pos, ref curr_ean);

                    prod.codis_ean.Add(curr_ean);
                    f.eans.Add(curr_ean);
                }
            }

            if (line[cursor_pos] == 'F')
            {
                f.NewProducte();
                ParseFinalFitxer(line, cursor_pos);
            }
        }

        System.Windows.Forms.Timer parse_timer = null;
        ParseProcessTick progress_callback = null;
        ParseFinished finish_callback = null;
        string[] async_text_lines;
        int curr_text_line = 0;
        public void ParseAsync(string file_name, string[] text_lines, ParseProcessTick progress_call = null, ParseFinished finish_call = null)
        {
            if(!parsing)
            {
                Fitxer f = DataManager.Instance.NewFitxer();
                f.nom = file_name;

                async_text_lines = text_lines;
                curr_text_line = 0;

                if (progress_call != null)
                    progress_callback += progress_call;

                if (finish_call != null)
                    finish_callback += finish_call;

                parse_timer = new System.Windows.Forms.Timer();
                parse_timer.Tick += new EventHandler(ParseAsyncTimeStep);
                parse_timer.Interval = 3; // in miliseconds
                parse_timer.Start();

                parsing = true;
            }
        }

        private void ParseAsyncTimeStep(Object stateInfo, EventArgs e)
        {
            for (int i = 0; i < 50; ++i)
            {
                if (curr_text_line < async_text_lines.Length)
                {
                    ParseLine(async_text_lines[curr_text_line]);
                    ++curr_text_line;

                    if (progress_callback != null)
                        progress_callback(GetParseProcess());
                }
                else
                {
                    parsing = false;
                    parse_timer.Stop();

                    if (finish_callback != null)
                        finish_callback();

                    break;
                }
            }
        }

        public float GetParseProcess()
        {
            float ret = 0;

            if (parsing)
            {
                if(async_text_lines.Length != 0)
                    ret = ((float)100 / (float)async_text_lines.Length) * curr_text_line;
            }

            return ret;
        }

        private void ParseCapçalera(string text, int cursor_pos)
        {
            cursor_pos += guide.tipus_registre;

            Fitxer f = Managers.DataManager.Instance.GetFitxer();

            // Tipus moneda --------------------------------------------------
            f.tipus_moneda = guide.GetTipusMoneda(text[cursor_pos]);

            cursor_pos += guide.tipus_moneda;

            // Data creacio fitxer -------------------------------------------
            string data_creacio_fitxer_any_str = "";
            string data_creacio_fitxer_mes_str = "";
            string data_creacio_fitxer_dia_str = "";
            string data_creacio_fitxer_hora_str = "";
            string data_creacio_fitxer_min_str = "";
            for (int i = 0; i < guide.data_creacio_fitxer + guide.hora_creacio_fitxer; ++i)
            {
                char curr_word = text[cursor_pos + i];

                if (i < 4)
                    data_creacio_fitxer_any_str += curr_word;

                else if (i < 6)
                    data_creacio_fitxer_mes_str += curr_word;

                else if (i < 8)
                    data_creacio_fitxer_dia_str += curr_word;

                else if (i < 10)
                    data_creacio_fitxer_hora_str += curr_word;

                else if (i < 12)
                    data_creacio_fitxer_min_str += curr_word;
            }

            int data_creacio_fitxer_any = 0;
            int.TryParse(data_creacio_fitxer_any_str, out data_creacio_fitxer_any);

            int data_creacio_fitxer_mes = 0;
            int.TryParse(data_creacio_fitxer_mes_str, out data_creacio_fitxer_mes);

            int data_creacio_fitxer_dia = 0;
            int.TryParse(data_creacio_fitxer_dia_str, out data_creacio_fitxer_dia);

            int data_creacio_fitxer_hora = 0;
            int.TryParse(data_creacio_fitxer_hora_str, out data_creacio_fitxer_hora);

            int data_creacio_fitxer_min = 0;
            int.TryParse(data_creacio_fitxer_min_str, out data_creacio_fitxer_min);

            f.data_creacio = new DateTime(data_creacio_fitxer_any, data_creacio_fitxer_mes,
                data_creacio_fitxer_dia, data_creacio_fitxer_hora, data_creacio_fitxer_min, 0);

            cursor_pos += guide.data_creacio_fitxer + guide.hora_creacio_fitxer;

            // Data importacio fitxer ----------------------------------------
            f.data_importacio = DateTime.Now;
        }

        private void ParseArticle(string text, int cursor_pos, ref Producte prod)
        {
            cursor_pos += guide.tipus_registre;

            // Codi article ----------------------------------------------
            prod.codi_article = "";
            for (int i = 0; i < guide.producte_codi_article; ++i)
            {
                prod.codi_article += text[i + cursor_pos];
            }

            cursor_pos += guide.producte_codi_article;

            // Marca de baixa ---------------------------------------------
            prod.marca_de_baixa = guide.GetMarcaDeBaixa(text[cursor_pos]);

            cursor_pos += guide.marca_de_baixa;

            // Descripcio -------------------------------------------------
            for (int i = 0; i < guide.descripcio; ++i)
            {
                prod.descripcio += text[i + cursor_pos];
            }

            cursor_pos += guide.descripcio;

            // Unitats caixa ----------------------------------------------
            string unitats_caixa_str = "";
            for (int i = 0; i < guide.unitats_caixa; ++i)
            {
                unitats_caixa_str += text[i + cursor_pos];
            }

            int unitats_caixa = 0;
            int.TryParse(unitats_caixa_str, out unitats_caixa);
            prod.unitats_caixa = unitats_caixa;

            cursor_pos += guide.unitats_caixa;

            // Unitats fraccio --------------------------------------------
            string unitats_fraccio_str = "";
            for (int i = 0; i < guide.unitats_fraccio; ++i)
            {
                unitats_fraccio_str += text[i + cursor_pos];
            }

            int unitats_fraccio = 0;
            int.TryParse(unitats_fraccio_str, out unitats_fraccio);
            prod.unitats_fraccio = unitats_fraccio;

            cursor_pos += guide.unitats_fraccio;

            // Marca de pes -----------------------------------------------
            prod.marca_de_pes = guide.GetMarcaDePes(text[cursor_pos]);

            cursor_pos += guide.marca_pes;

            // Preu unitari -----------------------------------------------
            string preu_unitari_str = "";
            string preu_unitari_enters_str = "";
            string preu_unitari_dec_str = "";
            for (int i = 0; i < guide.preu_unitari; ++i)
            {
                if (i < 6)
                    preu_unitari_enters_str += text[i + cursor_pos];
                else
                    preu_unitari_dec_str += text[i + cursor_pos];
            }
            preu_unitari_str += preu_unitari_enters_str + "." + preu_unitari_dec_str;

            double preu_unitari = 0.0f;
            double.TryParse(preu_unitari_str, NumberStyles.Float, CultureInfo.InvariantCulture, out preu_unitari);
            prod.preu_unitari = preu_unitari;

            cursor_pos += guide.preu_unitari;

            // Preu venda public recomanat ---------------------------------
            string preu_venda_public_str = "";
            string preu_venda_public_enters_str = "";
            string preu_venda_public_dec_str = "";
            for (int i = 0; i < guide.preu_venda_public_recomanat; ++i)
            {
                if (i < 6)
                    preu_venda_public_enters_str += text[i + cursor_pos];
                else
                    preu_venda_public_dec_str += text[i + cursor_pos];
            }
            preu_venda_public_str += preu_venda_public_enters_str + "." + preu_venda_public_dec_str;

            double preu_venta_public_recomanat = 0.0f;
            double.TryParse(preu_venda_public_str, NumberStyles.Float, CultureInfo.InvariantCulture, out preu_venta_public_recomanat);
            prod.preu_venta_public_recomanat = preu_venta_public_recomanat;

            cursor_pos += guide.preu_venda_public_recomanat;

            // Preu de fraccio --------------------------------------------
            string preu_fraccio_str = "";
            string preu_fraccio_enters_str = "";
            string preu_fraccio_dec_str = "";
            for (int i = 0; i < guide.preu_fraccio; ++i)
            {
                if (i < 6)
                    preu_fraccio_enters_str += text[i + cursor_pos];
                else
                    preu_fraccio_dec_str += text[i + cursor_pos];
            }
            preu_fraccio_str += preu_fraccio_enters_str + "." + preu_fraccio_dec_str;

            double preu_de_fraccio = 0.0f;
            double.TryParse(preu_fraccio_str, NumberStyles.Float, CultureInfo.InvariantCulture, out preu_de_fraccio);
            prod.preu_de_fraccio = preu_de_fraccio;

            cursor_pos += guide.preu_fraccio;

            // Tipus d'iva -----------------------------------------------
            prod.tipus_iva = guide.GetTipusIva(text[cursor_pos]);

            cursor_pos += guide.tipus_iva;

            // Codi de familia -------------------------------------------
            string codi_familia_str = "";
            for (int i = 0; i < guide.codi_familia; ++i)
            {
                codi_familia_str += text[i + cursor_pos];
            }

            int codi_familia = 0;
            int.TryParse(codi_familia_str, out codi_familia);
            prod.codi_familia = codi_familia;

            cursor_pos += guide.codi_familia;


            // Codi de subfamilia -------------------------------------------
            string codi_subfamilia_str = "";
            for (int i = 0; i < guide.codi_sub_familia; ++i)
            {
                codi_subfamilia_str += text[i + cursor_pos];
            }

            int codi_sub_familia = 0;
            int.TryParse(codi_subfamilia_str, out codi_sub_familia);
            prod.codi_sub_familia = codi_sub_familia;

            cursor_pos += guide.codi_sub_familia;

            // Unitat de mesura ---------------------------------------------
            prod.unitats_mesura = guide.GetUnitatsMesura(text[cursor_pos]);

            cursor_pos += guide.unitat_de_mesura;

            // Factor de conversio ------------------------------------------
            string factor_de_conversio_str = "";
            for (int i = 0; i < guide.factor_de_conversio; ++i)
            {
                factor_de_conversio_str += text[i + cursor_pos];
            }

            double factor_de_conversio = 0.0f;
            double.TryParse(factor_de_conversio_str, NumberStyles.Float, CultureInfo.InvariantCulture, out factor_de_conversio);
            prod.factor_de_conversio = factor_de_conversio;

            cursor_pos += guide.factor_de_conversio;
        }

        private void ParseCodiEan(string text, int cursor_pos, ref CodiEan ean)
        {
            cursor_pos += guide.tipus_registre;

            // Codi article ------------------------------------------------
            for(int i = 0; i < guide.ean_codi_article; ++i)
            {
                ean.codi_article += text[i + cursor_pos];
            }

            cursor_pos += guide.ean_codi_article;

            // Codi ean ----------------------------------------------------
            for (int i = 0; i < guide.codi_ean; ++i)
            {
                ean.codi_ean += text[i + cursor_pos];
            }

            cursor_pos += guide.codi_ean;
        }

        private void ParseFinalFitxer(string text, int cursor_pos)
        {
            cursor_pos += guide.tipus_registre;

            Fitxer f = Managers.DataManager.Instance.GetFitxer();

            // Quantitat registres -----------------------------------------
            string quantitat_registres_str = "";
            for(int i = 0; i < guide.quantitat_registres; ++i)
            {
                quantitat_registres_str += text[i + cursor_pos];
            }

            int.TryParse(quantitat_registres_str, out f.quantitat_registres);

            cursor_pos += guide.quantitat_registres;
        }

        public ParseGuide guide = new ParseGuide();
    }

    public class ParseGuide
    {
        public FitxerTipusMoneda GetTipusMoneda(char txt)
        {
            FitxerTipusMoneda ret = new FitxerTipusMoneda();

            if (txt == 'P')
                ret = FitxerTipusMoneda.PTES;

            if (txt == 'E')
                ret = FitxerTipusMoneda.EUROS;

            return ret;
        }

        public bool GetMarcaDeBaixa(char txt)
        {
            return txt == 'S' ? true : false;
        }

        public bool GetMarcaDePes(char txt)
        {
            return txt == 'S' ? true : false;
        }

        public ProducteTipusIva GetTipusIva(char txt)
        {
            ProducteTipusIva ret = new ProducteTipusIva();

            if (txt == 'S')
                ret = ProducteTipusIva.SUPER;

            if (txt == 'R')
                ret = ProducteTipusIva.RED;

            if (txt == 'N')
                ret = ProducteTipusIva.NORMAL;

            if (txt == 'E')
                ret = ProducteTipusIva.EXCEMPT;

            return ret;
        }

        public ProducteUnitatsMesura GetUnitatsMesura(char txt)
        {
            ProducteUnitatsMesura ret = new ProducteUnitatsMesura();

            if (txt == 'Q')
                ret = ProducteUnitatsMesura.GRAMS;

            if (txt == 'M')
                ret = ProducteUnitatsMesura.METRES;

            if (txt == 'L')
                ret = ProducteUnitatsMesura.LITRES;

            return ret;
        }

        public int tipus_registre = 1;
        public int tipus_moneda = 1;
        public int data_creacio_fitxer = 8;
        public int hora_creacio_fitxer = 4;

        public int producte_codi_article = 6;
        public int marca_de_baixa = 1;
        public int descripcio = 35;
        public int unitats_caixa = 4;
        public int unitats_fraccio = 4;
        public int marca_pes = 1;
        public int preu_unitari = 10;
        public int preu_venda_public_recomanat = 10;
        public int preu_fraccio = 10;
        public int tipus_iva = 1;
        public int codi_familia = 2;
        public int codi_sub_familia = 2;
        public int unitat_de_mesura = 1;
        public int factor_de_conversio = 6;
        public int reservat_factures_ampliacio = 200;

        public int ean_codi_article = 6;
        public int codi_ean = 13;

        public int fina_tipus_registre = 1;
        public int quantitat_registres = 6;
    }
}
