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
    class LocalDBManager : Singleton<LocalDBManager>
    {
        string connection_string = "";
        OleDbConnection connection = null;

        public void SetDataBasePath(string set)
        {
            connection_string = @"Provider=Microsoft.ACE.OLEDB.12.0; Data Source=" + set + ";";
            connection = new OleDbConnection(connection_string);
        }

        private bool ExecuteQuery(OleDbCommand cmd)
        {
            bool ret = false;

            if (connection != null)
            {
                if (cmd != null)
                {
                    try
                    {
                        cmd.ExecuteNonQuery();

                        ret = true;
                    }
                    catch (System.Data.OleDb.OleDbException exception)
                    {
                        string errors = "S'ha de tancar el fitxer Acces";
                        MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(string.Format(exception.Message),
                       "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);

                        ret = false;
                    }
                    catch (System.InvalidOperationException exception)
                    {
                        MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(string.Format(exception.Message),
                        "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    }
                }
            }

            return ret;
        }

        private OleDbDataReader ExecuteReader(OleDbCommand cmd)
        {
            OleDbDataReader ret = null;

            if (connection != null)
            {
                if (cmd != null)
                {
                    try
                    {
                        ret = cmd.ExecuteReader();
                    }
                    catch (System.Data.OleDb.OleDbException exception)
                    {
                        MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(string.Format(exception.Message),
                       "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    }
                    catch(System.InvalidOperationException exception)
                    {
                        MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(string.Format(exception.Message),
                        "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    }
                }
            }

            return ret;
        }

        private void OpenConexion()
        {
            if (connection != null)
            {
                try
                {
                    connection.Open();
                }
                catch (System.Data.OleDb.OleDbException exception)
                {
                    MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(string.Format(exception.Message),
                    "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }   
        }    

        private void CloseConexion()
        {
            if (connection != null)
            {
                try
                {
                    connection.Close();
                }
                catch (System.Data.OleDb.OleDbException exception)
                {
                    MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(string.Format(exception.Message),
                    "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }


        public bool ClearDataBase()
        {
            bool ret = false;

            if (connection != null)
            {
                OpenConexion();

                OleDbCommand Cmd = new OleDbCommand("DELETE FROM ArticlesMagsa", connection);
                OleDbCommand Cmd2 = new OleDbCommand("DELETE FROM BarresMagsa", connection);
                OleDbCommand Cmd3 = new OleDbCommand("DELETE FROM RegistreImportacio", connection);

                if (ExecuteQuery(Cmd) && ExecuteQuery(Cmd2) && ExecuteQuery(Cmd3))
                    ret = true;

                CloseConexion();
            }

            return ret;
        }

        public bool LoadDataBase()
        {
            bool ret = false;

            if (connection != null)
            {
                OpenConexion();

                Fitxer f = DataManager.Instance.NewFitxer();

                OleDbCommand Cmd = new OleDbCommand("SELECT * FROM ArticlesMagsa", connection);

                OleDbDataReader data = ExecuteReader(Cmd);

                if (data != null)
                {
                    while (data.Read())
                    {
                        Producte prod = f.NewProducte();

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
                    }

                    f.NewProducte();

                    ret = true;
                }

                OleDbCommand Cmd2 = new OleDbCommand("SELECT * FROM BarresMagsa", connection);

                OleDbDataReader data2 = ExecuteReader(Cmd2);

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

                OleDbDataReader data3 = ExecuteReader(Cmd3);

                if(data3 != null)
                {
                    while (data3.Read())
                    {
                        f.nom = data3.GetString(0);
                        f.data_importacio = data3.GetDateTime(1);
                    }
                }

                CloseConexion();
            }
            return ret;
        }

        public bool AddRegistre(Managers.Fitxer fitx)
        {
            bool ret = false;

            if (fitx != null)
            {
                OpenConexion();

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

                ret = ExecuteQuery(Cmd);

                CloseConexion();
            }

            return ret;
        }

        public bool AddProductes(List<Managers.Producte> prod)
        {
            bool ret = false;

            if (prod != null)
            {
                if (connection != null)
                {
                    OpenConexion();

                    string data_actual = DateTime.Now.ToShortTimeString();

                    for (int i = 0; i < prod.Count; ++i)
                    {
                        Managers.Producte curr_prod = prod[i];

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
                        Cmd.Parameters.Add("@P_Cost", OleDbType.Decimal).Value = curr_prod.preu_unitari.ToString().Replace(",", ".");
                        Cmd.Parameters.Add("@P_Recomanat", OleDbType.Decimal).Value = curr_prod.preu_venta_public_recomanat.ToString().Replace(",", ".");
                        Cmd.Parameters.Add("@P_Fraccio", OleDbType.Decimal).Value = curr_prod.preu_de_fraccio.ToString().Replace(",", ".");
                        Cmd.Parameters.Add("@Iva", OleDbType.VarChar, 1).Value = curr_prod.tipus_iva;
                        Cmd.Parameters.Add("@Familia", OleDbType.VarChar, 2).Value = curr_prod.codi_familia;
                        Cmd.Parameters.Add("@SubFamilia", OleDbType.VarChar, 2).Value = curr_prod.codi_sub_familia;
                        Cmd.Parameters.Add("@Mesura", OleDbType.VarChar, 1).Value = curr_prod.unitats_mesura;
                        Cmd.Parameters.Add("@FactorConversio", OleDbType.VarChar, 6).Value = curr_prod.factor_de_conversio;
                        Cmd.Parameters.Add("@UC", OleDbType.Integer).Value = curr_prod.unitats_caixa;
                        Cmd.Parameters.Add("@Data", OleDbType.Date).Value = data_actual;

                        ret = ExecuteQuery(Cmd);

                        if (!ret)
                            break;
                    }

                    CloseConexion();

                }
            }
            return ret;
        }

        public bool AddEans(List<Managers.CodiEan> ean)
        {
            bool ret = true;

            if (connection != null)
            {
                OpenConexion();

                //var s = ean.GroupBy(a => a.codi_ean);

                //int co = 0;
                //foreach(var p in s)
                //{
                //    if (p.Count() > 1)
                //    {
                //        int a = 0;
                //    }

                //    ++co;
                //}

                for (int i = 0; i < ean.Count; ++i)
                {
                    Managers.CodiEan curr_ean = ean[i];

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

                    ret = ExecuteQuery(Cmd);

                    if (!ret)
                        break;
                }

                CloseConexion();

            }

            return ret;
        }
    }
}
