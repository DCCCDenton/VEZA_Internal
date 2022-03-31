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
                        GetIns.GetInstanceDuctAccessory(_doc);
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

        private RelayCommand deleteViewPort;
        public RelayCommand DeleteViewPort
        {
            get
            {
                return deleteViewPort ??
                    (deleteViewPort = new RelayCommand(obj =>
                    {

                        Get_Instance GetIns = new Get_Instance();
                        GetIns.DeleteViewPort(_doc);
                    }));
            }
        }

        private RelayCommand printPDF;
        public RelayCommand PrintPDF
        {
            get
            {
                return printPDF ??
                    (printPDF = new RelayCommand(obj =>
                    {

                        Print pr = new Print();
                        pr.AutoPrintPDF(_doc);
                    }));
            }
        }

        private RelayCommand exportDWG;
        public RelayCommand ExportDWG
        {
            get
            {
                return exportDWG ??
                    (exportDWG = new RelayCommand(obj =>
                    {

                        Print pr = new Print();
                        pr.AutoExportDWG(_doc);
                    }));
            }
        }

        private RelayCommand famAnalitic;
        public RelayCommand FamAnalitic
        {
            get
            {
                return famAnalitic ??
                    (famAnalitic = new RelayCommand(obj =>
                    {

                        Fam_Analitic f_A = new Fam_Analitic();
                        f_A.FamAnalitic(_doc);
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
