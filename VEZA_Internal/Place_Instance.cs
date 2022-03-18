using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEZA_Internal
{
    public class Place_Instance
    {
        public Element PlaceNewFamilyInstance(Document doc, XYZ location, string F_Name, string F_Type)
        {
            FilteredElementCollector Filter_FamSym = new FilteredElementCollector(doc);
            FamilySymbol FamSym = Filter_FamSym.OfClass(typeof(FamilySymbol)).Cast<FamilySymbol>().Where(x => x.FamilyName == F_Name && x.Name == F_Type).FirstOrDefault();
            StructuralType structuralType = 0;
            using (Transaction t = new Transaction(doc))
            {
                t.Start("Place_Instance");
                FamilyInstance new_instance = doc.Create.NewFamilyInstance(location, FamSym, structuralType);
                t.Commit();
                return new_instance;
            }
        }
    }
}
