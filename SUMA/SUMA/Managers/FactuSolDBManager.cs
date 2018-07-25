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

        public bool AddProductes(List<Managers.Producte> prod)
        {
            bool ret = false;

            if (prod != null)
            {
                if (connection != null)
                {
                    Managers.DBManager.Instance.OpenConexion(connection);

                    for (int i = 0; i < prod.Count; ++i)
                    {
                        Managers.Producte curr_prod = prod[i];

                        string check_command_text = "SELECT * FROM F_ART WHERE CODART" + curr_prod.codi_article;

                        OleDbCommand CheckCmd = new OleDbCommand(check_command_text, connection);
                        OleDbDataReader CheckData = Managers.DBManager.Instance.ExecuteReader(connection, CheckCmd);

                        //string command = "INSERT INTO F_ART(";

                        //command += ", [CODART]";
                        //command += ", [DESART]";
                        //command += ", [TIVART]";
                        //command += ", [PCOART]";
                        //command += ", [UPPART]";

                        //command += ")VALUES(";

                        //command += ", @CODART";
                        //command += ", @DESART";
                        //command += ", @TIVART";
                        //command += ", @PCOART";
                        //command += ", @UPPART";

                        //command += ")";

                        //OleDbCommand Cmd = new OleDbCommand(command, connection);
                        //Cmd.Parameters.Add("@CODART", OleDbType.VarChar, 13).Value = curr_prod.codi_article;
                        //Cmd.Parameters.Add("@DESART", OleDbType.VarChar, 50).Value = curr_prod.descripcio; ;
                        //Cmd.Parameters.Add("@TIVART", OleDbType.Integer).Value = curr_prod.tipus_iva;
                        //Cmd.Parameters.Add("@PCOART", OleDbType.Double).Value = curr_prod.preu_unitari;
                        //Cmd.Parameters.Add("@UPPART", OleDbType.Integer).Value = curr_prod.unitats_caixa;
 
                        //ret = Managers.DBManager.Instance.ExecuteQuery(connection, Cmd);

                        //if (!ret)
                        //    break;

                        //string command2 = "INSERT INTO F_LFTA(";

                        //command2 += ", [TARLTA]";
                        //command2 += ", [ARTLTA]";
                        //command2 += ", [PRELTA]";

                        //command2 += ")VALUES(";
                               
                        //command2 += ", @TARLTA";
                        //command2 += ", @ARTLTA";
                        //command2 += ", @PRELTA";

                        //OleDbCommand Cmd2 = new OleDbCommand(command2, connection);
                        //Cmd2.Parameters.Add("@TARLTA", OleDbType.Integer).Value = 1;
                        //Cmd2.Parameters.Add("@ARTLTA", OleDbType.VarChar, 13).Value = curr_prod.codi_article;
                        //Cmd2.Parameters.Add("@PRELTA", OleDbType.Double, 50).Value = curr_prod.preu_venta_public_recomanat;

                        //ret = Managers.DBManager.Instance.ExecuteQuery(connection, Cmd2);

                        //if (!ret)
                        //    break;
                    }

                    Managers.DBManager.Instance.CloseConexion(connection);

                }
            }
            return ret;
        }

        public bool AddEans(List<Managers.CodiEan> eans)
        {
            bool ret = false;

            if (eans != null)
            {
                if (connection != null)
                {
                    Managers.DBManager.Instance.OpenConexion(connection);

                    for (int i = 0; i < eans.Count; ++i)
                    {
                        Managers.CodiEan curr_ean = eans[i];

                        string command = "INSERT INTO F_EAN(";

                        command += ", [ARTEAN]";
                        command += ", [EANEAN]";

                        command += ")VALUES(";

                        command += ", @ARTEAN";
                        command += ", @EANEAN";

                        command += ")";

                        OleDbCommand Cmd = new OleDbCommand(command, connection);
                        Cmd.Parameters.Add("@ARTEAN", OleDbType.VarChar, 13).Value = curr_ean.codi_article;
                        Cmd.Parameters.Add("@EANEAN", OleDbType.VarChar, 50).Value = curr_ean.codi_ean; ;

                        ret = Managers.DBManager.Instance.ExecuteQuery(connection, Cmd);

                        if (!ret)
                            break;
                    }

                    Managers.DBManager.Instance.CloseConexion(connection);

                }
            }
            return ret;
        }

    }
}
