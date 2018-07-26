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
    delegate void ProductAddProcessTick(float progress);
    delegate void ProductAddFinished();
    delegate void EanAddProcessTick(float progress);
    delegate void EanAddFinished();

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
                        add_productes_timer.Stop();

                        string message = "El producte amb codi: " + prod.codi_article + " ja esta a la base de dades, vol reemplacar-lo?";
                        MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(string.Format(message), "Conflicte", System.Windows.MessageBoxButton.YesNoCancel, System.Windows.MessageBoxImage.Warning);

                        add_productes_timer.Start();

                        if (messageBoxResult == MessageBoxResult.Yes)
                        {
                            sobreescriure = true;
                        }
                        else if(messageBoxResult == MessageBoxResult.Cancel)
                        {
                            ret = false;
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
                    add_eans_timer.Stop();

                    string message = "El producte amb codi: " + prod.codi_article + " no està a la base de dades, vol incloure'l?";
                    MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(string.Format(message), "Conflicte", System.Windows.MessageBoxButton.YesNoCancel, System.Windows.MessageBoxImage.Warning);

                    add_eans_timer.Start();

                    if (messageBoxResult == MessageBoxResult.Yes)
                    {
                        add_product = true;
                    }
                    else if (messageBoxResult == MessageBoxResult.Cancel)
                    {
                        ret = false;
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

            return ret;
        }

        private System.Windows.Forms.Timer add_productes_timer = null;
        private List<Managers.Producte> prods_to_add = null;
        private int curr_prod = 0;
        private bool adding_productes = false;
        private ProductAddProcessTick on_product_add_tick;
        private ProductAddFinished on_product_add_finished;
        private EanAddProcessTick on_product_ean_add_tick;
        private EanAddFinished on_product_ean_add_finished;
        private bool product_succes_msg = true;
        public void AddProductesAsync(List<Managers.Producte> prod, ProductAddProcessTick product_add_tick, ProductAddFinished product_add_finished, EanAddProcessTick on_ean_add_tick, EanAddFinished ean_add_finished, bool succes_message = true)
        {
            if (!adding_productes)
            {
                on_product_add_tick -= product_add_tick;
                on_product_add_tick += product_add_tick;
                on_product_add_finished -= product_add_finished;
                on_product_add_finished += product_add_finished;

                on_product_ean_add_tick -= on_ean_add_tick;
                on_product_ean_add_tick += on_ean_add_tick;

                on_product_ean_add_finished -= ean_add_finished;
                on_product_ean_add_finished += ean_add_finished;

                on_product_ean_add_finished -= ProductEanAddFinished;
                on_product_ean_add_finished += ProductEanAddFinished;

                curr_prod = 0;
                prods_to_add = prod;
                product_succes_msg = succes_message;
                add_productes_timer = new System.Windows.Forms.Timer();
                add_productes_timer.Tick += new EventHandler(AddProductesAsyncTimeStep);
                add_productes_timer.Interval = 5; // in miliseconds
                add_productes_timer.Start();

                adding_productes = true;
            }
        }

        private void AddProductesAsyncTimeStep(Object stateInfo, EventArgs e)
        {
            if (adding_productes)
            {
                for (int i = 0; i < 13; ++i)
                {
                    bool succes = false;

                    if (curr_prod < prods_to_add.Count)
                    {
                        succes = AddProducte(prods_to_add[curr_prod]);
                        ++curr_prod;

                        if (on_product_add_tick != null)
                            on_product_add_tick(GetProductAddProcess());

                        if (!succes)
                        {
                            adding_productes = false;
                            add_productes_timer.Stop();
                            on_product_ean_add_finished -= ProductEanAddFinished;

                            if (on_product_add_finished != null)
                                on_product_add_finished();

                            string message = "La introducció de Productes no s'ha pogut finalitzar";
                            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(string.Format(message), "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);

                            Managers.DBManager.Instance.CloseConexion(connection);

                            break;
                        }
                    }
                    else
                    {
                        adding_productes = false;
                        add_productes_timer.Stop();
                        on_product_ean_add_finished -= ProductEanAddFinished;

                        if (on_product_add_finished != null)
                            on_product_add_finished();

                        AddEansAsync(prods_to_add, on_product_ean_add_tick, on_product_ean_add_finished, false);

                        Managers.DBManager.Instance.CloseConexion(connection);

                        break;
                    }
                }
            }
        }

        private void ProductEanAddFinished()
        {
            if (product_succes_msg)
            {
                string message = "Introducció de Productes finalitzada amb èxit";
                MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(string.Format(message), "Èxit", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

                on_product_ean_add_finished -= ProductEanAddFinished;
            }
        }

        private float GetProductAddProcess()
        {
            float ret = 0;

            if (adding_productes)
            {
                if (prods_to_add.Count != 0)
                    ret = ((float)100 / (float)prods_to_add.Count) * curr_prod;
            }

            return ret;
        }

        private System.Windows.Forms.Timer add_eans_timer = null;
        private List<Managers.Producte> eans_to_add = null;
        private int curr_ean = 0;
        private int curr_prod_ean = 0;
        private bool curr_product_not_added = false;
        private bool adding_eans = false;
        private EanAddProcessTick on_ean_add_tick;
        private EanAddFinished on_ean_add_finished;
        private bool ean_succes_msg = true;
        public void AddEansAsync(List<Managers.Producte> prod, EanAddProcessTick ean_add_tick, EanAddFinished ean_add_finished, bool succes_message = true)
        {
            if (!adding_eans)
            {
                on_ean_add_tick -= ean_add_tick;
                on_ean_add_tick += ean_add_tick;
                on_ean_add_finished -= ean_add_finished;
                on_ean_add_finished += ean_add_finished;

                curr_ean = 0;
                curr_prod_ean = 0;
                eans_to_add = prod;
                ean_succes_msg = succes_message;
                add_eans_timer = new System.Windows.Forms.Timer();
                add_eans_timer.Tick += new EventHandler(AddEansAsyncTimeStep);
                add_eans_timer.Interval = 5; // in miliseconds
                add_eans_timer.Start();

                adding_eans = true;
            }
        }

        private void AddEansAsyncTimeStep(Object stateInfo, EventArgs e)
        {
            if (adding_eans)
            {
                for (int i = 0; i < 13; ++i)
                {
                    if (curr_prod_ean < eans_to_add.Count)
                    {
                        bool succes = false;
                        if (eans_to_add[curr_prod_ean].codis_ean.Count > curr_ean)
                        {
                            if(!curr_product_not_added)
                                succes = AddEan(eans_to_add[curr_prod_ean], eans_to_add[curr_prod_ean].codis_ean[curr_ean], out curr_product_not_added);

                            ++curr_ean;

                            if (!succes)
                            {
                                adding_eans = false;
                                add_eans_timer.Stop();

                                if (on_ean_add_finished != null)
                                    on_ean_add_finished();

                                Managers.DBManager.Instance.CloseConexion(connection);

                                string message = "La introducció de Eans no s'ha pogut finalitzar";
                                MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(string.Format(message), "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);

                                break;
                            }
                        }
                        else
                        {
                            curr_product_not_added = false;
                            curr_ean = 0;
                            ++curr_prod_ean;
                        }

                        if (on_ean_add_tick != null)
                            on_ean_add_tick(GetEansAddProcess());
                    }
                    else
                    {
                        adding_eans = false;
                        add_eans_timer.Stop();

                        if (on_ean_add_finished != null)
                            on_ean_add_finished();

                        Managers.DBManager.Instance.CloseConexion(connection);

                        if (ean_succes_msg)
                        {
                            string message = "Introducció de Eans finalitzada amb èxit";
                            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(string.Format(message), "Èxit", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                        }

                        break;
                    }
                }
            }
        }

        private float GetEansAddProcess()
        {
            float ret = 0;

            if (adding_eans)
            {
                if (eans_to_add.Count != 0)
                    ret = ((float)100 / (float)eans_to_add.Count) * curr_prod_ean;
            }

            return ret;
        }
    }
}
