using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEZA_Internal
{
    internal class Search
    {
        public List<string> SearchRFA()
        {
            string[] allFoundFiles = Directory.GetFiles("C:/Работа/Veza/Families/Вентиляторы/71000_ВОД-ДУ/71001_ВОД-ДУ_У2_01", "*.rfa", SearchOption.AllDirectories);
            List<string> foundFiles = new List<string>();
            foreach (string file in allFoundFiles)
            {
                foundFiles.Add(file);
            }
        return foundFiles;
        }
    }
}
