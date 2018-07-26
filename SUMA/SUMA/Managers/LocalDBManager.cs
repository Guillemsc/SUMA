using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.OleDb;
using System.Data;
using System.Windows;

namespace SUMA.Managers
{
    delegate void LocalProductAndEanAddProcessTick(float progress);
    delegate void LocalProductAndEanAddFinished();

    class LocalDBManager : Singleton<LocalDBManager>
    {
        private string connection_string = "";
        private OleDbConnection connection = null;

        public void SetDataBasePath(string set)
        {
            connection_string = @"Provider=Microsoft.ACE.OLEDB.12.0; Data Source=" + set + ";";
            connection = new OleDbConnection(connection_string);
        }

        public bool ClearDataBase()
        {
            bool ret = false;
            
            Managers.DBManager.Instance.OpenConexion(connection);

            OleDbCommand Cmd = new OleDbCommand("DELETE FROM ArticlesMagsa", connection);
            OleDbCommand Cmd2 = new OleDbCommand("DELETE FROM BarresMagsa", connection);
            OleDbCommand Cmd3 = new OleDbCommand("DELETE FROM RegistreImportacio", connection);

            if (Managers.DBManager.Instance.ExecuteQuery(connection, Cmd) &&
               Managers.DBManager.Instance.ExecuteQuery(connection, Cmd2) &&
               Managers.DBManager.Instance.ExecuteQuery(connection, Cmd3))
                ret = true;

            Managers.DBManager.Instance.CloseConexion(connection);
            
            return ret;
        }

        public bool LoadDataBase()
        {
            bool ret = false;

            if (connection != null)
            {
                Managers.DBManager.Instance.OpenConexion(connection);

                Fitxer f = DataManager.Instance.NewFitxer();

                OleDbCommand Cmd = new OleDbCommand("SELECT * FROM ArticlesMagsa", connection);

                OleDbDataReader data = Managers.DBManager.Instance.ExecuteReader(connection, Cmd);

                if (data != null)
                {
                    while (data.Read())
                    {
                        Producte prod = new Producte();

                        prod.codi_article = data.GetString(1);
                        prod.marca_de_baixa = ParserManager.Instance.guide.GetMarcaDeBaixa(data.GetString(2)[0]);
                        prod.descripcio = data.GetString(3);
                        prod.unitats_caixa = Convert.ToInt32(data.GetValue(4));
                        prod.unitats_fraccio = Convert.ToInt32(data.GetValue(5));
                        prod.marca_de_pes = ParserManager.Instance.guide.GetMarcaDePes(data.GetString(6)[0]);
                        prod.preu_unitari = data.GetDouble(7);
                        prod.preu_venta_public_recomanat = data.GetDouble(8);
                        prod.preu_de_fraccio = data.GetDouble(9);
                        prod.tipus_iva = ParserManager.Instance.guide.GetTipusIva(data.GetString(10)[0]);
                        prod.codi_familia = Convert.ToInt32(data.GetValue(11));
                        prod.codi_sub_familia = Convert.ToInt32(data.GetValue(12));
                        prod.unitats_mesura = ParserManager.Instance.guide.GetUnitatsMesura(data.GetString(13)[0]);
                        int factor_de_conversio = 0;
                        prod.factor_de_conversio = 0; int.TryParse(data.GetString(14), out factor_de_conversio);
                        prod.factor_de_conversio = factor_de_conversio;
                        prod.unitats_caixa = Convert.ToInt32(data.GetValue(15));

                        f.productes.Add(prod);
                    }

                    ret = true;
                }

                OleDbCommand Cmd2 = new OleDbCommand("SELECT * FROM BarresMagsa", connection);

                OleDbDataReader data2 = Managers.DBManager.Instance.ExecuteReader(connection, Cmd2);

                if (data2 != null)
                {
                    while (data2.Read())
                    {
                        CodiEan ean = new CodiEan();

                        ean.codi_article = data2.GetString(1);
                        ean.codi_ean = data2.GetString(2);

                        f.eans.Add(ean);                
                    }

                    f.MakeEanProducteRelations();

                    ret = true;
                }

                OleDbCommand Cmd3 = new OleDbCommand("SELECT * FROM RegistreImportacio", connection);

                OleDbDataReader data3 = Managers.DBManager.Instance.ExecuteReader(connection, Cmd3);

                if(data3 != null)
                {
                    while (data3.Read())
                    {
                        f.nom = data3.GetString(0);
                        f.data_importacio = data3.GetDateTime(1);
                    }
                }

                Managers.DBManager.Instance.CloseConexion(connection);
            }
            return ret;
        }

        public bool AddRegistre(Managers.Fitxer fitx)
        {
            bool ret = false;

            if (fitx != null)
            {
                Managers.DBManager.Instance.OpenConexion(connection);

                string command = "INSERT INTO RegistreImportacio(";

                command += "[NomFitxer]";
                command += ", [DataImport]";

                command += ")VALUES(";

                command += "@NomFitxer";
                command += ", @DataImport";

                command += ")";

                OleDbCommand Cmd = new OleDbCommand(command, connection);
                Cmd.Parameters.Add("@NomFitxer", OleDbType.VarChar, 30).Value = fitx.nom;
                Cmd.Parameters.Add("@DataImport", OleDbType.VarChar, 30).Value = fitx.data_importacio.ToString();

                ret = Managers.DBManager.Instance.ExecuteQuery(connection, Cmd);

                Managers.DBManager.Instance.CloseConexion(connection);
            }

            return ret;
        }

        public bool AddProducte(Managers.Producte curr_prod)
        {
            bool ret = false;

            if (curr_prod != null)
            {
                if (connection != null)
                {
                    Managers.DBManager.Instance.OpenConexion(connection);

                    string data_actual = DateTime.Now.ToShortTimeString();
                    
                    string command = "INSERT INTO ArticlesMagsa(";

                    command += "[Linia]";
                    command += ", [Codi]";
                    command += ", [Baixa]";
                    command += ", [Descripcio]";
                    command += ", [Caixa]";
                    command += ", [Fraccio]";
                    command += ", [Pes]";
                    command += ", [P_Cost]";
                    command += ", [P_Recomanat]";
                    command += ", [P_Fraccio]";
                    command += ", [Iva]";
                    command += ", [Familia]";
                    command += ", [SubFamilia]";
                    command += ", [Mesura (L)(M)(Q)]";
                    command += ", [FactorConversio]";
                    command += ", [U/C]";
                    command += ", [Data]";

                    command += ")VALUES(";

                    command += "@Linia";
                    command += ", @Codi";
                    command += ", @Baixa";
                    command += ", @Descripcio";
                    command += ", @Caixa";
                    command += ", @Fraccio";
                    command += ", @Pes";
                    command += ", @P_Cost";
                    command += ", @P_Recomanat";
                    command += ", @P_Fraccio";
                    command += ", @Iva";
                    command += ", @Familia";
                    command += ", @SubFamilia";
                    command += ", @Mesura";
                    command += ", @FactorConversio";
                    command += ", @UC";
                    command += ", @Data";

                    command += ")";

                    OleDbCommand Cmd = new OleDbCommand(command, connection);
                    Cmd.Parameters.Add("@Linia", OleDbType.VarChar, 1).Value = "L";
                    Cmd.Parameters.Add("@Codi", OleDbType.VarChar, 6).Value = curr_prod.codi_article;
                    Cmd.Parameters.Add("@Baixa", OleDbType.VarChar, 1).Value = curr_prod.marca_de_baixa_str;
                    Cmd.Parameters.Add("@Descripcio", OleDbType.VarChar, 35).Value = curr_prod.descripcio;
                    Cmd.Parameters.Add("@Caixa", OleDbType.Integer).Value = curr_prod.unitats_caixa;
                    Cmd.Parameters.Add("@Fraccio", OleDbType.Integer).Value = curr_prod.unitats_fraccio;
                    Cmd.Parameters.Add("@Pes", OleDbType.VarChar, 1).Value = curr_prod.marca_de_pes_str;
                    Cmd.Parameters.Add("@P_Cost", OleDbType.Double).Value = curr_prod.preu_unitari;
                    Cmd.Parameters.Add("@P_Recomanat", OleDbType.Double).Value = curr_prod.preu_venta_public_recomanat;
                    Cmd.Parameters.Add("@P_Fraccio", OleDbType.Double).Value = curr_prod.preu_de_fraccio;
                    Cmd.Parameters.Add("@Iva", OleDbType.VarChar, 1).Value = curr_prod.tipus_iva;
                    Cmd.Parameters.Add("@Familia", OleDbType.VarChar, 2).Value = curr_prod.codi_familia;
                    Cmd.Parameters.Add("@SubFamilia", OleDbType.VarChar, 2).Value = curr_prod.codi_sub_familia;
                    Cmd.Parameters.Add("@Mesura", OleDbType.VarChar, 1).Value = curr_prod.unitats_mesura;
                    Cmd.Parameters.Add("@FactorConversio", OleDbType.VarChar, 6).Value = curr_prod.factor_de_conversio;
                    Cmd.Parameters.Add("@UC", OleDbType.Integer).Value = curr_prod.unitats_caixa;
                    Cmd.Parameters.Add("@Data", OleDbType.Date).Value = data_actual;

                    ret = Managers.DBManager.Instance.ExecuteQuery(connection, Cmd);                   
                }
            }

            return ret;
        }

        public bool AddEan(Managers.CodiEan curr_ean)
        {
            bool ret = true;

            if (connection != null)
            {
                Managers.DBManager.Instance.OpenConexion(connection);

                string command = "INSERT INTO BarresMagsa(";

                command += "[Linia]";
                command += ", [Codi]";
                command += ", [Barres]";

                command += ")VALUES(";

                command += "@Linia";
                command += ", @Codi";
                command += ", @Barres";

                command += ")";

                OleDbCommand Cmd = new OleDbCommand(command, connection);
                Cmd.Parameters.Add("@Linia", OleDbType.VarChar, 1).Value = "E";
                Cmd.Parameters.Add("@Codi", OleDbType.VarChar, 6).Value = curr_ean.codi_article;
                Cmd.Parameters.Add("@Barres", OleDbType.VarChar, 13).Value = curr_ean.codi_ean;

                ret = Managers.DBManager.Instance.ExecuteQuery(connection, Cmd);
            }

            return ret;
        }


        private System.Windows.Forms.Timer add_products_and_eans_timer = null;
        private List<Managers.Producte> products_to_add = null;
        private List<Managers.CodiEan> eans_to_add = null;
        private int curr_ean = 0;
        private int curr_prod = 0;
        private bool adding_products_and_eans = false;
        private LocalProductAndEanAddProcessTick on_product_and_ean_add_tick;
        private LocalProductAndEanAddFinished on_product_and_ean_add_finished;
        public void AddProductesAndEansAsync(List<Managers.Producte> prods, List<Managers.CodiEan> eans, 
            LocalProductAndEanAddProcessTick product_and_ean_add_tick, LocalProductAndEanAddFinished product_and_ean_add_finished)
        {
            if(!adding_products_and_eans)
            {
                on_product_and_ean_add_tick -= product_and_ean_add_tick;
                on_product_and_ean_add_tick += product_and_ean_add_tick;
                on_product_and_ean_add_finished -= product_and_ean_add_finished;
                on_product_and_ean_add_finished += product_and_ean_add_finished;

                curr_ean = 0;
                curr_prod = 0;
                products_to_add = prods;
                eans_to_add = eans;

                add_products_and_eans_timer = new System.Windows.Forms.Timer();
                add_products_and_eans_timer.Tick += new EventHandler(AddProductsAndEansAsyncTimeStep);
                add_products_and_eans_timer.Interval = 5; // in miliseconds
                add_products_and_eans_timer.Start();

                adding_products_and_eans = true;
            }
        }

        private void AddProductsAndEansAsyncTimeStep(Object stateInfo, EventArgs e)
        {
            if(adding_products_and_eans)
            {
                for(int i = 0; i < 13; ++i)
                {
                    if(products_to_add.Count > curr_prod)
                    {
                        AddProducte(products_to_add[curr_prod]);
                        ++curr_prod;

                        if (on_product_and_ean_add_tick != null)
                            on_product_and_ean_add_tick(GetProductAndEansAddProcess());
                    }
                    else if(eans_to_add.Count > curr_ean)
                    {
                        AddEan(eans_to_add[curr_ean]);
                        ++curr_ean;

                        if (on_product_and_ean_add_tick != null)
                            on_product_and_ean_add_tick(GetProductAndEansAddProcess());
                    }
                    else
                    {
                        adding_products_and_eans = false;
                        add_products_and_eans_timer.Stop();

                        if (on_product_and_ean_add_finished != null)
                            on_product_and_ean_add_finished();

                        Managers.DBManager.Instance.CloseConexion(connection);
                    }
                }
            }
        }

        private float GetProductAndEansAddProcess()
        {
            float ret = 0;

            if (adding_products_and_eans)
            {
                float products_per = 0;
                float eans_per = 0;

                if (products_to_add.Count != 0)
                    products_per = ((float)100 / (float)products_to_add.Count) * curr_prod;

                if (eans_to_add.Count != 0)
                    eans_per = ((float)100 / (float)eans_to_add.Count) * curr_ean;

                ret = (products_per * 0.5f) + (eans_per * 0.5f);
            }

            return ret;
        }

        //public bool AddProductes(List<Managers.Producte> prod)
        //{
        //    bool ret = false;

        //    if (prod != null)
        //    {
        //        if (connection != null)
        //        {
        //            Managers.DBManager.Instance.OpenConexion(connection);

        //            string data_actual = DateTime.Now.ToShortTimeString();

        //            for (int i = 0; i < prod.Count; ++i)
        //            {
        //                Managers.Producte curr_prod = prod[i];

        //                string command = "INSERT INTO ArticlesMagsa(";

        //                command += "[Linia]";
        //                command += ", [Codi]";
        //                command += ", [Baixa]";
        //                command += ", [Descripcio]";
        //                command += ", [Caixa]";
        //                command += ", [Fraccio]";
        //                command += ", [Pes]";
        //                command += ", [P_Cost]";
        //                command += ", [P_Recomanat]";
        //                command += ", [P_Fraccio]";
        //                command += ", [Iva]";
        //                command += ", [Familia]";
        //                command += ", [SubFamilia]";
        //                command += ", [Mesura (L)(M)(Q)]";
        //                command += ", [FactorConversio]";
        //                command += ", [U/C]";
        //                command += ", [Data]";

        //                command += ")VALUES(";

        //                command += "@Linia";
        //                command += ", @Codi";
        //                command += ", @Baixa";
        //                command += ", @Descripcio";
        //                command += ", @Caixa";
        //                command += ", @Fraccio";
        //                command += ", @Pes";
        //                command += ", @P_Cost";
        //                command += ", @P_Recomanat";
        //                command += ", @P_Fraccio";
        //                command += ", @Iva";
        //                command += ", @Familia";
        //                command += ", @SubFamilia";
        //                command += ", @Mesura";
        //                command += ", @FactorConversio";
        //                command += ", @UC";
        //                command += ", @Data";

        //                command += ")";

        //                OleDbCommand Cmd = new OleDbCommand(command, connection);
        //                Cmd.Parameters.Add("@Linia", OleDbType.VarChar, 1).Value = "L";
        //                Cmd.Parameters.Add("@Codi", OleDbType.VarChar, 6).Value = curr_prod.codi_article;
        //                Cmd.Parameters.Add("@Baixa", OleDbType.VarChar, 1).Value = curr_prod.marca_de_baixa_str;
        //                Cmd.Parameters.Add("@Descripcio", OleDbType.VarChar, 35).Value = curr_prod.descripcio;
        //                Cmd.Parameters.Add("@Caixa", OleDbType.Integer).Value = curr_prod.unitats_caixa;
        //                Cmd.Parameters.Add("@Fraccio", OleDbType.Integer).Value = curr_prod.unitats_fraccio;
        //                Cmd.Parameters.Add("@Pes", OleDbType.VarChar, 1).Value = curr_prod.marca_de_pes_str;
        //                Cmd.Parameters.Add("@P_Cost", OleDbType.Double).Value = curr_prod.preu_unitari;
        //                Cmd.Parameters.Add("@P_Recomanat", OleDbType.Double).Value = curr_prod.preu_venta_public_recomanat;
        //                Cmd.Parameters.Add("@P_Fraccio", OleDbType.Double).Value = curr_prod.preu_de_fraccio;
        //                Cmd.Parameters.Add("@Iva", OleDbType.VarChar, 1).Value = curr_prod.tipus_iva;
        //                Cmd.Parameters.Add("@Familia", OleDbType.VarChar, 2).Value = curr_prod.codi_familia;
        //                Cmd.Parameters.Add("@SubFamilia", OleDbType.VarChar, 2).Value = curr_prod.codi_sub_familia;
        //                Cmd.Parameters.Add("@Mesura", OleDbType.VarChar, 1).Value = curr_prod.unitats_mesura;
        //                Cmd.Parameters.Add("@FactorConversio", OleDbType.VarChar, 6).Value = curr_prod.factor_de_conversio;
        //                Cmd.Parameters.Add("@UC", OleDbType.Integer).Value = curr_prod.unitats_caixa;
        //                Cmd.Parameters.Add("@Data", OleDbType.Date).Value = data_actual;

        //                ret = Managers.DBManager.Instance.ExecuteQuery(connection, Cmd);

        //                if (!ret)
        //                    break;
        //            }

        //            Managers.DBManager.Instance.CloseConexion(connection);

        //        }
        //    }
        //    return ret;
        //}

        //public bool AddEans(List<Managers.CodiEan> ean)
        //{
        //    bool ret = true;

        //    if (connection != null)
        //    {
        //        Managers.DBManager.Instance.OpenConexion(connection);

        //        for (int i = 0; i < ean.Count; ++i)
        //        {
        //            Managers.CodiEan curr_ean = ean[i];

        //            string command = "INSERT INTO BarresMagsa(";

        //            command += "[Linia]";
        //            command += ", [Codi]";
        //            command += ", [Barres]";

        //            command += ")VALUES(";

        //            command += "@Linia";
        //            command += ", @Codi";
        //            command += ", @Barres";

        //            command += ")";

        //            OleDbCommand Cmd = new OleDbCommand(command, connection);
        //            Cmd.Parameters.Add("@Linia", OleDbType.VarChar, 1).Value = "E";
        //            Cmd.Parameters.Add("@Codi", OleDbType.VarChar, 6).Value = curr_ean.codi_article;
        //            Cmd.Parameters.Add("@Barres", OleDbType.VarChar, 13).Value = curr_ean.codi_ean;

        //            ret = Managers.DBManager.Instance.ExecuteQuery(connection, Cmd);

        //            if (!ret)
        //                break;
        //        }

        //        Managers.DBManager.Instance.CloseConexion(connection);
        //    }

        //    return ret;
        //}
    }
}
