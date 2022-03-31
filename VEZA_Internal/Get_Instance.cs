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

        public Element GetInstanceDuctAccessory(Document doc)
        {
            ISet<ElementId> simIds;
            List<int> FamilyName = new List<int>();
            List<List<double>> listDimension = new List<List<double>>();
            List<BoundingBoxXYZ> listBbox = new List<BoundingBoxXYZ>();
            string famName = null;
            string typeName = null;
            byte famType = 0;
            var families = new FilteredElementCollector(doc).OfClass(typeof(Family)).ToList();
            Element elm = null;
            FamilySymbol FamSym = null;
            double x = 0;
            foreach (Family fam in families)
            {
                var catId = fam.FamilyCategoryId.IntegerValue;
                if (catId == -2008016)
                {
                    famName = fam.Name;
                    simIds = fam.GetFamilySymbolIds();
                    List<double> dimension = new List<double>();
                    foreach (ElementId symId in simIds)
                    {
                        elm = PlaceInstance(doc, symId, famName, x);
                        famType = GetFamType(elm);
                        dimension = GetFamInsDimension(elm, famType);
                        if (listDimension.Count != 0)
                        {
                            if (DimensionCompare(listDimension, dimension) == true)
                            {
                                DeleteElement(doc, elm);
                            }
                            if (DimensionCompare(listDimension, dimension) == false)
                            {
                                listDimension.Add(dimension);
                                x += 20;
                            }
                        }
                        if (listDimension.Count == 0)
                        {
                            listDimension.Add(dimension);
                            x += 20;
                        }
                    }
                }
            }
            return elm;
        }

        //catId == -2001140 | 


        public void CreateLinearDimension(Document doc, UIDocument uidoc)
        {
            using (Transaction t = new Transaction(doc))
            {
                Element elm = GetInstanceDuctAccessory(doc);
                XYZ minPoint = null;
                XYZ maxPoint = null;
                byte famType = GetFamType(elm);
                List<double> dimension = GetFamInsDimension(elm, famType);
                if (dimension.Count != 0)
                {
                    FamilyInstance famIns = elm as FamilyInstance;
                    Options opt = new Options();
                    opt.ComputeReferences = true;
                    opt.IncludeNonVisibleObjects = true;
                    GeometryElement famInsRef = famIns.get_Geometry(opt);
                    foreach (GeometryObject geoObj in famInsRef)
                    {
                        GeometryInstance geom = geoObj as GeometryInstance;
                        if (null != geom)
                        {
                            minPoint = GetMinPoint(elm, geom, dimension, famType);
                            maxPoint = GetMaxPoint(minPoint, dimension);
                            CreateDimension(doc, uidoc, minPoint, maxPoint);
                        }
                    }
                }

            }
        }

        private double BboxLenght(BoundingBoxXYZ Bbox)
        {
            XYZ MinPoint = Bbox.Min;
            XYZ MaxPoint = Bbox.Max;
            XYZ startPoint1 = new XYZ(MinPoint.X, MinPoint.Y, MinPoint.Z);
            XYZ endPoint1 = new XYZ(MaxPoint.X, MinPoint.Y, MinPoint.Z);
            XYZ startPoint2 = new XYZ(MinPoint.X, MaxPoint.Y, MinPoint.Z);
            XYZ endPoint2 = new XYZ(MaxPoint.X, MaxPoint.Y, MinPoint.Z);
            XYZ startPoint3 = new XYZ(MinPoint.X, MinPoint.Y, MaxPoint.Z);
            XYZ endPoint3 = new XYZ(MaxPoint.X, MinPoint.Y, MaxPoint.Z);
            Line geomLine1 = Line.CreateBound(startPoint1, endPoint1);
            Line geomLine2 = Line.CreateBound(startPoint1, startPoint2);
            Line geomLine3 = Line.CreateBound(startPoint1, startPoint3);
            double A = geomLine1.Length;
            double B = geomLine2.Length;
            double C = geomLine3.Length;
            double bboxLenght = Math.Sqrt(Math.Pow(A, 2) + Math.Pow(B, 2) + Math.Pow(C, 2));
            return bboxLenght;
        }

        private List<double> GetFamInsDimension(Element elm, byte famType)
        {
            Get_Set_Instance_Parameter param = new Get_Set_Instance_Parameter();
            List<double> dimension = new List<double>();
            double A = 0;
            double B = 0;
            double C = 0;
            double D = 0;
            if (famType == 1)
            {
                A = param.GetParameterValueDouble(elm, "ADSK_Размер_Длина");
                A = Math.Round(A, 2);
                B = param.GetParameterValueDouble(elm, "ADSK_Размер_Ширина");
                B = Math.Round(B, 2);
                C = param.GetParameterValueDouble(elm, "ADSK_Размер_Высота");
                C = Math.Round(C, 2);
                dimension = new List<double> { A, B, C };
            }
            if (famType == 2)
            {
                A = param.GetParameterValueDouble(elm, "ADSK_Размер_Длина");
                A = Math.Round(A, 2);
                B = param.GetParameterValueDouble(elm, "ADSK_Размер_Диаметр");
                B = Math.Round(B, 2);
                C = param.GetParameterValueDouble(elm, "ADSK_Размер_Диаметр");
                C = Math.Round(C, 2);
                dimension = new List<double> { A, B, C };
            }
            return dimension;
        }

        private byte GetFamType(Element elm)
        {
            Get_Set_Instance_Parameter param = new Get_Set_Instance_Parameter();
            byte famType = 0;
            bool ValidA = param.IsParameterValid(elm, "ADSK_Размер_Длина");
            bool ValidB = param.IsParameterValid(elm, "ADSK_Размер_Ширина");
            bool ValidC = param.IsParameterValid(elm, "ADSK_Размер_Высота");
            bool ValidD = param.IsParameterValid(elm, "ADSK_Размер_Диаметр");

            if (ValidA == true && ValidB == true && ValidC == true)
            {
                famType = 1;
            }
            if (ValidA == true && ValidD == true)
            {
                famType = 2;
            }
            
            return famType;
        }

        private XYZ GetMinPoint(Element elm, GeometryInstance geom, List<double> dimension, byte famType)
        {
            GeometryElement instanceGeometryElement = geom.GetInstanceGeometry();
            double bboxLenghtDim = Math.Sqrt(Math.Pow(dimension[0], 2) + Math.Pow(dimension[1], 2) + Math.Pow(dimension[2], 2));
            double minPointX = double.MaxValue;
            double minPointY = double.MaxValue;
            double minPointZ = double.MaxValue;
            XYZ minPoint = new XYZ();
            if (famType == 1)
            {
                foreach (var sol in instanceGeometryElement)
                {
                    if (sol is Solid)
                    {
                        Solid s = sol as Solid;
                        BoundingBoxXYZ bbox = s.GetBoundingBox();
                        double bboxLenght = BboxLenght(bbox);
                        if (bboxLenght <= bboxLenghtDim)
                        {
                            double PointX = bbox.Min.X;
                            if (PointX < minPointX)
                            {
                                minPointX = PointX;
                            }
                            double PointY = bbox.Min.Y;
                            if (PointY < minPointY)
                            {
                                minPointY = PointY;
                            }
                            double PointZ = bbox.Min.Z;
                            if (PointZ < minPointZ)
                            {
                                minPointZ = PointZ;
                            }
                        }
                        minPoint = new XYZ(minPointX, minPointY, 0);                      
                    }
                }
            }
            if (famType == 2)
            {
                minPointX = -(dimension[0] / 2);
                minPointY = -(dimension[1] / 2);
                minPointZ = -(dimension[2] / 2);
                minPoint = new XYZ(minPointX, minPointY, minPointZ);
            }
            return minPoint;
        }
        private XYZ GetMaxPoint(XYZ minPoint, List<double> dimension)
        {
            double maxPointX = minPoint.X + dimension[0];
            double maxPointY = minPoint.Y + dimension[1];
            double maxPointZ = minPoint.Z + dimension[2];
            XYZ maxPoint = new XYZ(maxPointX, maxPointY, maxPointZ);
            return maxPoint;
        }

        private void CreateDimension(Document doc, UIDocument uidoc, XYZ minPoint, XYZ maxPoint)
        {
            using (Transaction t = new Transaction(doc))
            {
                var vid = new FilteredElementCollector(doc).OfClass(typeof(ViewPlan)).Where(a => a.Name == "Вид сверху").First() as ViewPlan;
                var list = new FilteredElementCollector(doc).OfClass(typeof(ViewSheet)).Where(a => a.Name == "Без имени").First() as ViewSheet;
                ElementId vidId = vid.Id;
                t.Start("CreateLinearDimension");
                XYZ startPoint1 = new XYZ(minPoint.X, minPoint.Y, minPoint.Z);
                XYZ endPoint1 = new XYZ(maxPoint.X, minPoint.Y, minPoint.Z);
                XYZ startPoint2 = new XYZ(minPoint.X, maxPoint.Y, minPoint.Z);
                XYZ endPoint2 = new XYZ(maxPoint.X, maxPoint.Y, minPoint.Z);
                XYZ startPoint3 = new XYZ(minPoint.X, minPoint.Y, maxPoint.Z);
                XYZ endPoint3 = new XYZ(maxPoint.X, minPoint.Y, maxPoint.Z);
                XYZ dimStartPoint1 = new XYZ(minPoint.X - 1, minPoint.Y - 1, minPoint.Z);
                XYZ dimStartPoint2 = new XYZ(minPoint.X - 1, minPoint.Y, minPoint.Z);
                XYZ dimEndPoint1 = new XYZ(minPoint.X - 1, maxPoint.Y + 1, minPoint.Z);
                XYZ dimEndPoint2 = new XYZ(maxPoint.X + 1, minPoint.Y - 1, minPoint.Z);
                XYZ dimEndPoint3 = new XYZ(minPoint.X - 1, minPoint.Y, maxPoint.Z);
                Line geomLine1 = Line.CreateBound(startPoint1, endPoint1);
                Line geomLine2 = Line.CreateBound(startPoint2, endPoint2);
                Line geomLine3 = Line.CreateBound(startPoint1, startPoint2);
                Line geomLine4 = Line.CreateBound(endPoint1, endPoint2);
                Line geomLine5 = Line.CreateBound(startPoint3, endPoint3);
                Line geomLine6 = Line.CreateBound(startPoint1, startPoint3);
                Line dimLine1 = Line.CreateBound(dimStartPoint1, dimEndPoint1);
                Line dimLine2 = Line.CreateBound(dimStartPoint1, dimEndPoint2);
                Line dimLine3 = Line.CreateBound(dimStartPoint2, dimEndPoint3);
                Plane geomPlane1 = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, new XYZ(0, 0, minPoint.Z));
                Plane geomPlane2 = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, new XYZ(0, 0, maxPoint.Z));
                Plane geomPlane3 = Plane.CreateByNormalAndOrigin(XYZ.BasisY, new XYZ(0, minPoint.Y, 0));
                SketchPlane sketch1 = SketchPlane.Create(doc, geomPlane1);
                SketchPlane sketch2 = SketchPlane.Create(doc, geomPlane2);
                SketchPlane sketch3 = SketchPlane.Create(doc, geomPlane3);
                ModelLine line1 = doc.Create.NewModelCurve(geomLine1, sketch1) as ModelLine;
                ModelLine line2 = doc.Create.NewModelCurve(geomLine2, sketch1) as ModelLine;
                ModelLine line3 = doc.Create.NewModelCurve(geomLine3, sketch1) as ModelLine;
                ModelLine line4 = doc.Create.NewModelCurve(geomLine4, sketch1) as ModelLine;
                ModelLine line5 = doc.Create.NewModelCurve(geomLine5, sketch2) as ModelLine;
                ModelLine line6 = doc.Create.NewModelCurve(dimLine3, sketch3) as ModelLine;

                ReferenceArray references1 = new ReferenceArray();
                references1.Append(line1.GeometryCurve.Reference);
                references1.Append(line2.GeometryCurve.Reference);
                doc.Create.NewDimension(vid, dimLine1, references1);

                ReferenceArray references2 = new ReferenceArray();
                references2.Append(line3.GeometryCurve.Reference);
                references2.Append(line4.GeometryCurve.Reference);
                doc.Create.NewDimension(vid, dimLine2, references2);

                XYZ placePoint1 = new XYZ(0.5, 0.3, 0);
                Viewport.Create(doc, list.Id, vidId, placePoint1);

                t.Commit();

                var razrez = new FilteredElementCollector(doc).OfClass(typeof(View)).Where(a => a.Name == "Разрез 1").First() as View;

                t.Start("CreateLinearDimension");

                ReferenceArray references3 = new ReferenceArray();
                references3.Append(line5.GeometryCurve.Reference);
                references3.Append(line1.GeometryCurve.Reference);
                doc.Create.NewDimension(razrez, line6.GeometryCurve as Line, references3);

                XYZ placePoint2 = new XYZ(1.3, 0.3, 0);
                Viewport.Create(doc, list.Id, razrez.Id, placePoint2);

                t.Commit();
            }
        }

        public void DeleteViewPort(Document doc)
        {
            var listViewPort = new FilteredElementCollector(doc).OfClass(typeof(Viewport)).WhereElementIsNotElementType().ToList();
            using (Transaction t = new Transaction(doc))
            { 
                foreach (Element item in listViewPort)
                {
                    t.Start("Delete");
                    doc.Delete(item.Id);
                    t.Commit();
                }
            }
        }

        public void DeleteElement(Document doc, Element elm)
        {
            using (Transaction t = new Transaction(doc))
            {

                t.Start("Delete");
                doc.Delete(elm.Id);
                t.Commit();

            }
        }

        private bool DimensionCompare(List<List<double>> listDimension, List<double> dimension)
        {
            bool eq = false;
            foreach (List<double> dim in listDimension)
            {
                if (dim[0] == dimension[0] && dim[1] == dimension[1] && dim[2] == dimension[2])
                {
                    eq = true;
                }
            }
            return eq;
        }

        private bool BboxCompare(List<BoundingBoxXYZ> listBbox, BoundingBoxXYZ bbox)
        {
            bool eq = false;
            foreach (BoundingBoxXYZ box in listBbox)
            {
                if (box.Min == bbox.Min && box.Max == bbox.Max)
                {
                    eq = true;
                }
            }
            return eq;
        }

        private Element PlaceInstance(Document doc, ElementId symId, string famName, double x)
        {
            Place_Instance pls_ins = new Place_Instance();
            Element elm = null;
            FamilySymbol FamSym = doc.GetElement(symId) as FamilySymbol;
            using (Transaction t = new Transaction(doc))
            {
                if (!FamSym.IsActive)
                {
                    t.Start("GetInstance");
                    FamSym.Activate();
                    t.Commit();
                }
                string typeName = doc.GetElement(symId).Name;
                XYZ elmLoc = new XYZ(x, 0, 0);
                elm = pls_ins.PlaceNewFamilyInstance(doc, elmLoc, famName, typeName);
            }
            return elm;
        }
    }
}

