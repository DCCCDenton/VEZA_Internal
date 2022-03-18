using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace VEZA_Internal
{
    internal class AppViewModel : INotifyPropertyChanged
    {
        public Document _doc { get; set; }
        public UIDocument _uidoc { get; set; }

        private RelayCommand placeInstance;
        public RelayCommand PlaceInstance
        {
            get
            {
                return placeInstance ??
                    (placeInstance = new RelayCommand(obj =>
                    {

                        Get_Instance GetIns = new Get_Instance();
                        GetIns.GetInstance(_doc);
                    }));   
            }
        }


        private RelayCommand placeDimension;
        public RelayCommand PlaceDimension
        {
            get
            {
                return placeDimension ??
                    (placeDimension = new RelayCommand(obj =>
                    {

                        Get_Instance GetIns = new Get_Instance();
                        GetIns.CreateLinearDimension(_doc, _uidoc);
                    }));
            }
        }






        public AppViewModel(Document doc, UIDocument uidoc)
        {
            _doc = doc;
            _uidoc = uidoc;
        }





        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
