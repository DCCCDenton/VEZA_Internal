using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;

namespace VEZA_Internal
{
    [TransactionAttribute(TransactionMode.Manual)]  //активация транзакции
    [RegenerationAttribute(RegenerationOption.Manual)]// активация регенрации 
    public class Main_Class : IExternalCommand
    {
        
        public Result Execute(ExternalCommandData externalCommand, ref string message, ElementSet elements)
        {
            Document doc = externalCommand.Application.ActiveUIDocument.Document;
            UIDocument uidoc = externalCommand.Application.ActiveUIDocument;
            Main_Wind userWind = new Main_Wind(doc, uidoc);
            userWind.ShowDialog();
            return Result.Succeeded;
        }
    }
}
