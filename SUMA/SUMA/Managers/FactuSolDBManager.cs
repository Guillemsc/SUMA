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
    class FactuSolDBManager : Singleton<FactuSolDBManager>
    {
        private string connection_string = "";
        private OleDbConnection connection = null;

        public void SetDataBasePath(string set)
        {
            connection_string = @"Provider=Microsoft.ACE.OLEDB.12.0; Data Source=" + set + ";";
            connection = new OleDbConnection(connection_string);
        }

        public bool AddProducte(Managers.Producte prod)
        {
            bool ret = false;

            if (prod != null)
            {
                if (connection != null)
                {
                    Managers.DBManager.Instance.OpenConexion(connection);

                    string check_command_text = "SELECT * FROM F_ART WHERE CODART ='" + prod.codi_article + "'";

                    OleDbCommand CheckCmd = new OleDbCommand(check_command_text, connection);
                    OleDbDataReader CheckData = Managers.DBManager.Instance.ExecuteReader(connection, CheckCmd);

                    bool already_exists = false;
                    bool sobreescriure = false;

                    if (CheckData != null)
                    {
                        if (CheckData.Read())
                            already_exists = true;

                        ret = true;
                    }

                    if (ret && already_exists)
                    {
                        string message = "El producte amb codi: " + prod.codi_article + " ja esta a la base de dades, vol reemplacar-lo?";
                        MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(string.Format(message), "Conflicte", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning);

                        if (messageBoxResult == MessageBoxResult.Yes)
                        {
                            sobreescriure = true;
                        }
                    }

                    if (ret)
                    {
                        if (sobreescriure || !already_exists)
                        {
                            if (sobreescriure)
                            {
                                string command3 = "DELETE FROM F_ART WHERE CODART ='" + prod.codi_article + "'";

                                OleDbCommand Cmd3 = new OleDbCommand(command3, connection);
                                ret = Managers.DBManager.Instance.ExecuteQuery(connection, Cmd3);
                            }

                            if (ret)
                            {
                                string command = "INSERT INTO F_ART(";

                                command += " [CODART]";
                                command += ", [DESART]";
                                command += ", [TIVART]";
                                command += ", [PCOART]";
                                command += ", [UPPART]";

                                command += ")VALUES(";

                                command += " @CODART";
                                command += ", @DESART";
                                command += ", @TIVART";
                                command += ", @PCOART";
                                command += ", @UPPART";

                                command += ")";

                                OleDbCommand Cmd = new OleDbCommand(command, connection);
                                Cmd.Parameters.Add("@CODART", OleDbType.VarChar, 13).Value = prod.codi_article;
                                Cmd.Parameters.Add("@DESART", OleDbType.VarChar, 50).Value = prod.descripcio; ;
                                Cmd.Parameters.Add("@TIVART", OleDbType.Integer).Value = prod.tipus_iva;
                                Cmd.Parameters.Add("@PCOART", OleDbType.Double).Value = prod.preu_unitari;
                                Cmd.Parameters.Add("@UPPART", OleDbType.Integer).Value = prod.unitats_caixa;

                                ret = Managers.DBManager.Instance.ExecuteQuery(connection, Cmd);

                                if (ret)
                                {
                                    string command4 = "DELETE FROM F_LTA WHERE ARTLTA ='" + prod.codi_article + "'";

                                    OleDbCommand Cmd4 = new OleDbCommand(command4, connection);
                                    ret = Managers.DBManager.Instance.ExecuteQuery(connection, Cmd4);

                                    if (ret)
                                    {
                                        string command2 = "INSERT INTO F_LTA(";

                                        command2 += "[TARLTA]";
                                        command2 += ", [ARTLTA]";
                                        command2 += ", [PRELTA]";

                                        command2 += ")VALUES(";

                                        command2 += "@TARLTA";
                                        command2 += ", @ARTLTA";
                                        command2 += ", @PRELTA";

                                        command2 += ")";

                                        OleDbCommand Cmd2 = new OleDbCommand(command2, connection);
                                        Cmd2.Parameters.Add("@TARLTA", OleDbType.Integer).Value = 1;
                                        Cmd2.Parameters.Add("@ARTLTA", OleDbType.VarChar, 13).Value = prod.codi_article;
                                        Cmd2.Parameters.Add("@PRELTA", OleDbType.Double, 50).Value = prod.preu_venta_public_recomanat;

                                        ret = Managers.DBManager.Instance.ExecuteQuery(connection, Cmd2);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return ret;
        }

        public bool AddEan(Managers.Producte prod, Managers.CodiEan ean, out bool product_not_added)
        {
            bool ret = false;

            product_not_added = false;

            if (prod != null && ean != null)
            {
                Managers.DBManager.Instance.OpenConexion(connection);

                string command4 = "DELETE FROM F_EAN WHERE EANEAN ='" + ean.codi_ean + "'";

                OleDbCommand Cmd4 = new OleDbCommand(command4, connection);
                ret = Managers.DBManager.Instance.ExecuteQuery(connection, Cmd4);

                bool has_product = false;
                bool add_product = false;
                if (ret)
                {
                    string check_product_text = "SELECT * FROM F_ART WHERE CODART ='" + prod.codi_article + "'";

                    OleDbCommand CheckCmd2 = new OleDbCommand(check_product_text, connection);
                    OleDbDataReader CheckData2 = Managers.DBManager.Instance.ExecuteReader(connection, CheckCmd2);

                    if (CheckData2 != null)
                    {
                        if (CheckData2.Read())
                            has_product = true;

                        ret = true;
                    }
                }

                if (!has_product)
                {
                    string message = "El producte amb codi: " + prod.codi_article + " no està a la base de dades, vol incloure'l?";
                    MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(string.Format(message), "Conflicte", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning);

                    if (messageBoxResult == MessageBoxResult.Yes)
                    {
                        add_product = true;
                    }
                    else
                    {
                        product_not_added = true;
                    }

                    if(add_product)
                    {
                        ret = AddProducte(prod);

                        has_product = true;
                    }
                }

                if (ret && has_product)
                {
                    string command = "INSERT INTO F_EAN(";

                    command += "[ARTEAN]";
                    command += ", [EANEAN]";

                    command += ")VALUES(";

                    command += "@ARTEAN";
                    command += ", @EANEAN";

                    command += ")";

                    OleDbCommand Cmd = new OleDbCommand(command, connection);
                    Cmd.Parameters.Add("@ARTEAN", OleDbType.VarChar, 13).Value = ean.codi_article;
                    Cmd.Parameters.Add("@EANEAN", OleDbType.VarChar, 50).Value = ean.codi_ean; ;

                    ret = Managers.DBManager.Instance.ExecuteQuery(connection, Cmd);
                }

            }

            Managers.DBManager.Instance.CloseConexion(connection);

            return ret;
        }

        public bool AddProductes(List<Managers.Producte> prod, bool succes_message = true)
        {
            bool ret = false;

            if (prod != null)
            {
                if (connection != null)
                {
                    for (int i = 0; i < prod.Count; ++i)
                    {
                        ret = AddProducte(prod[i]);

                        if (!ret)
                            break;
                    }

                    ret = AddEans(prod, false);

                    if(ret && succes_message)
                    {
                        string message = "Introducció de Productes finalitzada amb èxit";
                        MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(string.Format(message), "Èxit", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    }

                }
            }
            return ret;
        }

        public bool AddEans(List<Managers.Producte> prod, bool succes_missage = true)
        {
            bool ret = false;

            if (prod != null)
            {
                if (connection != null)
                {
                    Managers.DBManager.Instance.OpenConexion(connection);

                    for (int p = 0; p < prod.Count; ++p)
                    {
                        Managers.Producte curr_prod = prod[p];

                        for (int i = 0; i < curr_prod.codis_ean.Count; ++i)
                        {
                            bool product_not_added = false;

                            ret = AddEan(curr_prod, curr_prod.codis_ean[i], out product_not_added);

                            if (product_not_added)
                                break;

                            if (!ret)
                                break;
                        }
                    }

                    Managers.DBManager.Instance.CloseConexion(connection);

                    if(ret && succes_missage)
                    {
                        string message = "Introducció de EANS finalitzada amb èxit";
                        MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(string.Format(message), "Èxit", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    }

                }
            }
            return ret;
        }

    }
}
