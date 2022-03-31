using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEZA_Internal
{
    internal class Fam_Analitic
    {
        public void FamAnalitic(Document doc)
        {
            Search search = new Search(); 
            List<string> famList = search.SearchRFA();
            string filePath = "C:/Temp/temp.txt";
            string tmp = null;
            using (StreamWriter writer = new StreamWriter(filePath, false))
            {
                writer.WriteLine(tmp);
            }

            foreach (string familyPath in famList)
            {
                Family fam = LoadFamily(doc, familyPath);
                List<FamilySymbol> familySymbols = GetFamilySymbol(doc, fam);
                Element elm = PlaceInstance(doc, familySymbols);
                byte famType = GetFamType(elm, familySymbols);   
                if (famType == 0)
                {
                    using (StreamWriter writer = new StreamWriter(filePath, true))
                    {
                        writer.WriteLine(familyPath);
                    }
                }
                using (Transaction t = new Transaction(doc))
                {
                    t.Start("Удаление семейства");

                    doc.Delete(elm.Id);
                    doc.Delete(fam.Id);
                    t.Commit();
                }
                    
            }

        }
        private Family LoadFamily(Document doc, string familyPath)
        {         
            Family family = null;       
            using (Transaction t = new Transaction(doc))
            {
                t.Start("Загрузка семейства");
                doc.LoadFamily(familyPath, out family);
                t.Commit();
            }
            return family;  
        }

        private List<FamilySymbol> GetFamilySymbol(Document doc, Family family)
        {
            FamilySymbol famSym = null;
            List<FamilySymbol> familySymbols = new List<FamilySymbol>();
            using (Transaction t = new Transaction(doc))
            {
                t.Start("Загрузка элемента");
                foreach (ElementId symId in family.GetFamilySymbolIds())
                {
                    famSym = doc.GetElement(symId) as FamilySymbol;
                    familySymbols.Add(famSym);
                    famSym.Activate();
                    break;
                }              
                t.Commit();
                return familySymbols;
            }
        }

        private Element PlaceInstance(Document doc, List<FamilySymbol> familySymbols)
        {
            StructuralType structuralType = 0;
            XYZ elmLoc = new XYZ(0, 0, 0);
            Element elm = null;
            using (Transaction t = new Transaction(doc))
            {
                t.Start("Загрузка элемента");
                foreach (FamilySymbol famSym in familySymbols)
                {
                    elm = doc.Create.NewFamilyInstance(elmLoc, famSym, structuralType);
                    break;
                }              
                t.Commit();
            }

            return elm;
        }

        private byte GetFamType(Element elm, List<FamilySymbol> familySymbols)
        {
            Get_Set_Instance_Parameter param = new Get_Set_Instance_Parameter();
            byte famType = 0;
            bool ValidA = param.IsParameterValid(elm, "ADSK_Размер_Длина");
            bool ValidB = param.IsParameterValid(elm, "ADSK_Размер_Ширина");
            bool ValidC = param.IsParameterValid(elm, "ADSK_Размер_Высота");
            bool ValidD = param.IsParameterValid(elm, "ADSK_Размер_Диаметр");
            bool ValidE = param.IsFamSymParameterValid(familySymbols[0], "L");

            if (ValidA == true && ValidB == true && ValidC == true)
            {
                famType = 1;
            }

            if (ValidA == true && ValidD == true)
            {
                famType = 2;
            }

            if (ValidD == true && ValidE == true)
            {
                famType = 3;
            }
            return famType;
        }

        private void WriteToFile()
        {

        }
    }

}
