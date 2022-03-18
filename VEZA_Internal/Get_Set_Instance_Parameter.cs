using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEZA_Internal
{
    public class Get_Set_Instance_Parameter
    {
        public string Get_Parameter_Value_String(Element instance, string Param_Name)
        {
            Parameter param = instance.LookupParameter(Param_Name);
            string param_value = param.AsString();
            return param_value;
        }

        public double Get_Parameter_Value_Double(Element instance, string Param_Name)
        {
            Parameter param = instance.LookupParameter(Param_Name);
            double param_value = param.AsDouble();
            return param_value;
        }

        public void SetParameterValueByName(Document doc, Element new_instance, string Param_Name, string Parameter_Value)
        {
            using (Transaction t = new Transaction(doc))
            {
                Parameter param = new_instance.LookupParameter(Param_Name);
                t.Start("SetParameterValue");
                param.Set(Parameter_Value);
                t.Commit();
            }

        }
        public void SetParameterValueByName(Document doc, Element new_instance, string Param_Name, double Parameter_Value)
        {
            using (Transaction t = new Transaction(doc))
            {
                Parameter param = new_instance.LookupParameter(Param_Name);
                t.Start("SetParameterValue");
                param.Set(Parameter_Value);
                t.Commit();
            }
        }
    }
}
