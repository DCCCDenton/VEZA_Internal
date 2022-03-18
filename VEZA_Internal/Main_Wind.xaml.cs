using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VEZA_Internal
{
    /// <summary>
    /// Interaction logic for Main_Wind.xaml
    /// </summary>
    /// 
    
    public partial class Main_Wind : Window
    {
        Document _doc;
        UIDocument _uidoc;
        public Main_Wind(Document doc, UIDocument uidoc)
        {
            _doc = doc;
            _uidoc = uidoc; 
            InitializeComponent();
            DataContext = new AppViewModel(_doc, _uidoc);
        }
    }
}
