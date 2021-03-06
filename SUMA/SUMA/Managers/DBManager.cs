﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.OleDb;
using System.Data;
using System.Windows;

namespace SUMA.Managers
{
    class DBManager : Singleton<DBManager>
    {
        public OleDbConnection ConnectToDataSource(string path)
        {
            OleDbConnection ret = null;

            string connection_string = @"Provider=Microsoft.ACE.OLEDB.12.0; Data Source=" + path + ";";

            try
            {
                ret = new OleDbConnection(connection_string);
            }
            catch (System.Data.OleDb.OleDbException exception)
            {
                MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(string.Format(exception.Message),
                "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            catch (System.InvalidOperationException exception)
            {
                MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(string.Format(exception.Message),
                "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }

            OpenConexion(ret);

            return ret;
        }

        public bool ExecuteQuery(OleDbConnection connection, OleDbCommand cmd)
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

        public OleDbDataReader ExecuteReader(OleDbConnection connection, OleDbCommand cmd)
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
                    catch (System.InvalidOperationException exception)
                    {
                        MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(string.Format(exception.Message),
                        "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    }
                }
            }

            return ret;
        }

        public void OpenConexion(OleDbConnection connection)
        {
            if (connection != null)
            {
                try
                {
                    if(connection.State != ConnectionState.Open)
                        connection.Open();
                }
                catch (System.Data.OleDb.OleDbException exception)
                {
                    MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(string.Format(exception.Message),
                    "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
                catch (System.InvalidOperationException exception)
                {
                    MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(string.Format(exception.Message),
                    "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }

        public void CloseConexion(OleDbConnection connection)
        {
            if (connection != null)
            {
                try
                {
                    if(connection.State == ConnectionState.Open)
                        connection.Close();
                }
                catch (System.Data.OleDb.OleDbException exception)
                {
                    MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show(string.Format(exception.Message),
                    "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }
    }
}
