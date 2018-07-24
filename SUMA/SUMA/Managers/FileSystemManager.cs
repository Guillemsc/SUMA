using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SUMA.Managers
{
    class FileSystemManager : Singleton<FileSystemManager>
    {
        public string LoadFileDialog(string extension)
        {
            string ret = null;

            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = extension + " files (*. " + extension + ")|*." + extension;
            dialog.FilterIndex = 1;
            dialog.RestoreDirectory = true;

            if(dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                ret = dialog.FileName;
            
            return ret;
        }

        public string[] GetFileDataText(string path)
        {
            string[] ret;

            ret = File.ReadAllLines(path, Encoding.GetEncoding("iso-8859-1"));

            return ret;
        }
    }
}
