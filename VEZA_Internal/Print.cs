using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEZA_Internal
{
    internal class Print
    {
        public void AutoPrintPDF(Document doc)
        {
            Options options = new Options();

            PrintManager printManager = doc.PrintManager;
            printManager.PrintRange = PrintRange.Select;
            printManager.Apply();
            printManager.SelectNewPrintDriver("Adobe PDF");
            printManager.Apply();
            printManager.CombinedFile = false;
            printManager.Apply();
            printManager.PrintToFile = true;
            printManager.Apply();
            try
            {
                printManager.SubmitPrint();

            }
            catch
            {

            }
        }

        public void AutoExportDWG(Document doc)
        {
            DWGExportOptions options = DWGExportOptions.GetPredefinedOptions(doc, "main");
            options.FileVersion = (ACADVersion)(22);
            options.MergedViews = true;
            ICollection<ElementId> views = new List<ElementId>();
            var list = new FilteredElementCollector(doc).OfClass(typeof(ViewSheet)).Where(a => a.Name == "Без имени").First() as ViewSheet;
            views.Add(list.Id);
            doc.Export(@"c:\temp", @"temp.dwg", views, options);
        }
    }
}
