using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEZA_Internal
{
    internal class Get_Instance
    {

        public Element GetInstance(Document doc)
        {
            ISet<ElementId> simIds;
            List<int> FamilyName = new List<int>();
            string famName = null;
            string typeName = null;
            var families = new FilteredElementCollector(doc).OfClass(typeof(Family)).ToList();
            Place_Instance pls_ins = new Place_Instance();
            XYZ elmLoc = new XYZ();
            Element elm = null;
            FamilySymbol FamSym = null;
            foreach (Family fam in families)
            {

                var catId = fam.FamilyCategoryId.IntegerValue;
                if (catId == -2001140)
                {
                    famName = fam.Name;
                    simIds = fam.GetFamilySymbolIds();


                    foreach (ElementId symId in simIds)
                    {
                        FamSym = doc.GetElement(symId) as FamilySymbol;
                        if (FamSym.IsActive)
                        {
                            typeName = doc.GetElement(symId).Name;
                            elm = pls_ins.PlaceNewFamilyInstance(doc, elmLoc, famName, typeName);
                        }

                    }
                }
            }
            return elm;
        }


        public void CreateLinearDimension(Document doc, UIDocument uidoc)
        {

            using (Transaction t = new Transaction(doc))
            {
                Element elm = GetInstance(doc);
                Get_Set_Instance_Parameter param = new Get_Set_Instance_Parameter();
                double A = param.Get_Parameter_Value_Double(elm, "ADSK_Размер_Длина");
                A = Math.Round(A, 2);
                double B = param.Get_Parameter_Value_Double(elm, "ADSK_Размер_Ширина");
                B = Math.Round(B, 2);
                List<double> dimension = new List<double> { A, B };
                t.Start("CreateLinearDimension");
                
                BoundingBoxXYZ Bbox = null;
                FamilyInstance famIns = elm as FamilyInstance;
                Options opt = new Options();
                opt.ComputeReferences = true;
                opt.IncludeNonVisibleObjects = false;
                GeometryElement famInsRef = famIns.get_Geometry(opt);
                Bbox = famInsRef.GetBoundingBox();
                GeometryElement instanceGeometryElement = null;
                foreach (GeometryObject geoObj in famInsRef)
                {
                    GeometryInstance geom = geoObj as GeometryInstance;
                    if (null != geom)
                    {
                        instanceGeometryElement = geom.GetInstanceGeometry();
                        foreach (var sol in instanceGeometryElement)
                        {
                            if (sol is Solid)
                            {
                                Solid s = sol as Solid;
                                Bbox = s.GetBoundingBox();
                                DimLine(doc, Bbox, dimension);
                            }
                        }
                    }
                }
                t.Commit();
            }
        }

        private void DimLine(Document doc, BoundingBoxXYZ Bbox, List<double> dimension)
        {
            XYZ MinPoint = Bbox.Min;
            XYZ MaxPoint = Bbox.Max;
            XYZ startPoint1 = new XYZ(MinPoint.X, MinPoint.Y, 0);
            XYZ endPoint1 = new XYZ(MaxPoint.X, MinPoint.Y, 0);
            XYZ startPoint2 = new XYZ(MinPoint.X, MaxPoint.Y, 0);
            XYZ endPoint2 = new XYZ(MaxPoint.X, MaxPoint.Y, 0);
            XYZ startPoint3 = new XYZ(MinPoint.X, MinPoint.Y, MaxPoint.Z);
            XYZ endPoint3 = new XYZ(MaxPoint.X, MinPoint.Y, MaxPoint.Z);
            XYZ dimStartPoint1 = new XYZ(MinPoint.X-1, MinPoint.Y - 1, 0);
            XYZ dimEndPoint1 = new XYZ(MinPoint.X - 1, MaxPoint.Y + 1, 0);
            XYZ dimEndPoint2 = new XYZ(MaxPoint.X + 1, MinPoint.Y - 1, 0);
            XYZ dimEndPoint3 = new XYZ(MinPoint.X + 1, MinPoint.Y + 1, MaxPoint.Z);
            bool validMinPoint = XYZ.IsWithinLengthLimits(MinPoint);
            bool validMaxPoint = XYZ.IsWithinLengthLimits(MaxPoint);

            if (validMinPoint == true && validMaxPoint == true)
            {
                
                Line geomLine1 = Line.CreateBound(startPoint1, endPoint1);             
                Line geomLine2 = Line.CreateBound(startPoint2, endPoint2);
                Line geomLine3 = Line.CreateBound(startPoint1, startPoint2);               
                Line geomLine4 = Line.CreateBound(endPoint1, endPoint2);
                Line dimLine1 = Line.CreateBound(dimStartPoint1, dimEndPoint1);
                Line dimLine2 = Line.CreateBound(dimStartPoint1, dimEndPoint2);
                
                double L1 = geomLine1.Length;
                L1 = Math.Round(L1, 2);
                double L2 = geomLine3.Length;
                L2 = Math.Round(L2, 2);
                Plane geomPlane1 = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, new XYZ(0, 0, 0));
                SketchPlane sketch1 = SketchPlane.Create(doc, geomPlane1);

                if ((L1 == dimension[0] | L1 == dimension[1]) && (L2 == dimension[0] | L2 == dimension[1]))
                {
                    ModelLine line1 = doc.Create.NewModelCurve(geomLine1, sketch1) as ModelLine;
                    ModelLine line2 = doc.Create.NewModelCurve(geomLine2, sketch1) as ModelLine;
                    ModelLine line3 = doc.Create.NewModelCurve(geomLine3, sketch1) as ModelLine;
                    ModelLine line4 = doc.Create.NewModelCurve(geomLine4, sketch1) as ModelLine;
                    ReferenceArray references1 = new ReferenceArray();
                    references1.Append(line1.GeometryCurve.Reference);
                    references1.Append(line2.GeometryCurve.Reference);
                    doc.Create.NewDimension(doc.ActiveView, dimLine1, references1);

                    ReferenceArray references2 = new ReferenceArray();
                    references2.Append(line3.GeometryCurve.Reference);
                    references2.Append(line4.GeometryCurve.Reference);
                    doc.Create.NewDimension(doc.ActiveView, dimLine2, references2);
                }


                
            }
        }
    }
}

