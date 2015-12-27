using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;
using LuckyRotorPlate.Model;

namespace LuckyRotorPlate.DataService
{
    /// <summary>
    /// xml 读写类
    /// </summary>
    class XmlHelper
    {
        public static ObservableCollection<RealThing> getRealThings(String fileName, 
            ObservableCollection<RealThing> thingsForExclude)
        {
            ObservableCollection<RealThing> things = new ObservableCollection<RealThing>();

            XmlDocument xmldoc = new XmlDocument();
            XmlElement root;
            RealThing tmpThing;
            String name;
            double value;
            String remark;

            if (File.Exists(fileName))
            {
                xmldoc.Load(fileName);
                root = xmldoc.DocumentElement;
                foreach (XmlElement node in root.ChildNodes)
                {
                    name = node.GetElementsByTagName("name").Count > 0 ? node.GetElementsByTagName("name").Item(0).InnerText : String.Empty;
                    value = node.GetElementsByTagName("value").Count > 0 ? 
                        Convert.ToDouble(node.GetElementsByTagName("value").Item(0).InnerText) : 1;
                    remark = node.GetElementsByTagName("remark").Count > 0 ? 
                        node.GetElementsByTagName("remark").Item(0).InnerText : String.Empty;
                    tmpThing = new RealThing(name, value, remark);
                    if (null != thingsForExclude && thingsForExclude.Count > 0 && node.GetElementsByTagName("excludes").Count > 0)
                    {
                        XmlNode excludes = node.GetElementsByTagName("excludes").Item(0);
                        foreach (XmlElement n in excludes.ChildNodes)
                        {
                            String tname = n.GetElementsByTagName("name").Count > 0 ? 
                                n.GetElementsByTagName("name").Item(0).InnerText : String.Empty;
                            foreach (RealThing t in thingsForExclude)
                            {
                                if (t.Name.Equals(tname) && !tmpThing.ExcludeThing.Contains(t))
                                {
                                    tmpThing.ExcludeThing.Add(t);
                                }
                            }
                        }
                    }
                    things.Add(tmpThing);
                }
            }

            return things;
        }

        public static void SaveRealThings(String fileName, ObservableCollection<RealThing> things)
        {
            XmlDocument xmldoc = new XmlDocument();
            XmlElement root;
            XmlElement tmpThing, name, value, remark;

            xmldoc.LoadXml(@"<?xml version='1.0' encoding='utf-8' ?><things></things>");
            root = xmldoc.DocumentElement;
            foreach (RealThing thing in things)
            {
                tmpThing = xmldoc.CreateElement("thing");
                name = xmldoc.CreateElement("name");
                name.InnerText = thing.Name;
                value = xmldoc.CreateElement("value");
                value.InnerText = thing.Value.ToString();
                remark = xmldoc.CreateElement("remark");
                remark.InnerText = thing.Remark;
                XmlElement excludes = xmldoc.CreateElement("excludes");
                foreach (RealThing t in thing.ExcludeThing)
                {
                    XmlElement tNode = xmldoc.CreateElement("thing");
                    XmlElement nameNode = xmldoc.CreateElement("name");
                    nameNode.InnerText = t.Name;
                    tNode.AppendChild(nameNode);
                    excludes.AppendChild(tNode);
                }

                tmpThing.AppendChild(name);
                tmpThing.AppendChild(value);
                tmpThing.AppendChild(remark);
                tmpThing.AppendChild(excludes);
                root.AppendChild(tmpThing);
            }

            xmldoc.Save(fileName);
        }

        public static ObservableCollection<Solution> getSolutions(String fileName)
        {
            ObservableCollection<Solution> suos = new ObservableCollection<Solution>();
            XmlDocument xmldoc = new XmlDocument();
            XmlElement root;

            if (File.Exists(fileName))
            {
                xmldoc.Load(fileName);
                root = xmldoc.DocumentElement;
                foreach (XmlElement suoNode in root.ChildNodes)
                {
                    Solution suo = new Solution();
                    suo.Name = suoNode.GetElementsByTagName("name").Count > 0 ?
                        suoNode.GetElementsByTagName("name").Item(0).InnerText : String.Empty;
                    suo.Drap = suoNode.GetElementsByTagName("drap").Count > 0 ?
                        double.Parse(suoNode.GetElementsByTagName("drap").Item(0).InnerText) : 0.002;
                    suo.Way = suoNode.GetElementsByTagName("way").Count > 0 ? 
                        suoNode.GetElementsByTagName("way").Item(0).InnerText : String.Empty;
                    suo.StepList = new ObservableCollection<StepInfo>();
                    if (suoNode.GetElementsByTagName("steps").Count > 0)
                    {
                        XmlElement stepsNode = (XmlElement)suoNode.GetElementsByTagName("steps").Item(0);
                        foreach (XmlElement stepNode in stepsNode)
                        {
                            StepInfo step = new StepInfo();
                            step.Name = stepNode.GetElementsByTagName("name").Count > 0 ?
                                stepNode.GetElementsByTagName("name").Item(0).InnerText : String.Empty;
                            step.SortWay = stepNode.GetElementsByTagName("sort").Count > 0 ?
                                stepNode.GetElementsByTagName("sort").Item(0).InnerText : String.Empty;
                            step.Pies = new ObservableCollection<RealThing>();
                            if (stepNode.GetElementsByTagName("pies").Count > 0)
                            {
                                XmlNode piesNode = stepNode.GetElementsByTagName("pies").Item(0);
                                getThingsFromNode(step.Pies, piesNode, false);
                            }
                            step.Players = new ObservableCollection<RealThing>();
                            if (stepNode.GetElementsByTagName("players").Count > 0)
                            {
                                XmlNode playersNode = stepNode.GetElementsByTagName("players").Item(0);
                                getThingsFromNode(step.Players, playersNode, true);
                            }
                            step.ExcludePies = new ObservableCollection<RealThing>();
                            if (stepNode.GetElementsByTagName("globalExcludes").Count > 0)
                            {
                                XmlNode excludesNode = stepNode.GetElementsByTagName("globalExcludes").Item(0);
                                getThingsFromNode(step.ExcludePies, excludesNode, false);
                            }
                            suo.StepList.Add(step);
                        }
                    }
                    suos.Add(suo);
                }
            }
            return suos;
        }

        private static void getThingsFromNode(ObservableCollection<RealThing> things , XmlNode piesNode, bool isPlayerNode)
        {
            foreach (XmlElement resultNode in piesNode)
            {
                String name = resultNode.GetElementsByTagName("name").Count > 0 ?
                    resultNode.GetElementsByTagName("name").Item(0).InnerText : String.Empty;
                double value = resultNode.GetElementsByTagName("value").Count > 0 ?
                    Convert.ToDouble(resultNode.GetElementsByTagName("value").Item(0).InnerText) : 1;
                String remark = resultNode.GetElementsByTagName("remark").Count > 0 ?
                    resultNode.GetElementsByTagName("remark").Item(0).InnerText : String.Empty;
                RealThing tmpThing;
                if (isPlayerNode)
                {
                    tmpThing = new RealThing(name, value, remark);
                    String obtainedGift = resultNode.GetElementsByTagName("obtainedGift").Count > 0 ?
                        resultNode.GetElementsByTagName("obtainedGift").Item(0).InnerText : String.Empty;
                    tmpThing.ObtainedGift = obtainedGift;
                }
                else
                {
                    tmpThing = new RealThing(name, value, remark);
                }
                if (resultNode.GetElementsByTagName("excludes").Count > 0)
                {
                    XmlNode excludes = resultNode.GetElementsByTagName("excludes").Item(0);
                    foreach (XmlElement n in excludes.ChildNodes)
                    {
                        String tname = n.GetElementsByTagName("name").Count > 0 ?
                            n.GetElementsByTagName("name").Item(0).InnerText : String.Empty;
                        RealThing t = new RealThing(tname,1,String.Empty);
                        tmpThing.ExcludeThing.Add(t);
                    }
                }
                things.Add(tmpThing);
            }
        }

        public static void SaveSolutions(String fileName, ObservableCollection<Solution> solutions)
        {
            XmlDocument xmldoc = new XmlDocument();
            XmlElement root;

            xmldoc.LoadXml(@"<?xml version='1.0' encoding='utf-8' ?><solutions></solutions>");
            root = xmldoc.DocumentElement;
            foreach (Solution suo in solutions)
            {
                XmlElement suoNode = xmldoc.CreateElement("solution");
                XmlElement suoName = xmldoc.CreateElement("name");
                suoName.InnerText = suo.Name;
                suoNode.AppendChild(suoName);
                XmlElement suoDrap = xmldoc.CreateElement("drap");
                suoDrap.InnerText = suo.Drap.ToString();
                suoNode.AppendChild(suoDrap);
                XmlElement way = xmldoc.CreateElement("way");
                way.InnerText = suo.Way;
                suoNode.AppendChild(way);
                XmlElement stepsNode = xmldoc.CreateElement("steps");
                foreach (StepInfo step in suo.StepList)
                {
                    XmlElement stepNode = xmldoc.CreateElement("step");
                    XmlElement stepName = xmldoc.CreateElement("name");
                    stepName.InnerText = step.Name;
                    stepNode.AppendChild(stepName);
                    XmlElement stepSortWay = xmldoc.CreateElement("sort");
                    stepSortWay.InnerText = step.SortWay;
                    stepNode.AppendChild(stepSortWay);
                    saveNode(xmldoc, step.Pies, stepNode, "pies", false);
                    saveNode(xmldoc, step.Players, stepNode, "players", true);
                    saveNode(xmldoc, step.ExcludePies, stepNode, "globalExcludes", false);

                    stepsNode.AppendChild(stepNode);
                }
                suoNode.AppendChild(stepsNode);

                root.AppendChild(suoNode);
            }

            xmldoc.Save(fileName);
        }

        private static void saveNode(XmlDocument xmldoc, ObservableCollection<RealThing> things, XmlElement stepNode, 
            String nodeName, bool isPlayerNode)
        {
            XmlElement thingsNode = xmldoc.CreateElement(nodeName);
            foreach (RealThing thing in things)
            {
                XmlElement thingNode = xmldoc.CreateElement("thing");
                XmlElement name = xmldoc.CreateElement("name");
                name.InnerText = thing.Name;
                thingNode.AppendChild(name);
                XmlElement value = xmldoc.CreateElement("value");
                value.InnerText = thing.Value.ToString();
                thingNode.AppendChild(value);
                XmlElement remark = xmldoc.CreateElement("remark");
                remark.InnerText = thing.Remark;
                thingNode.AppendChild(remark);
                if (isPlayerNode)
                {
                    XmlElement obtainedGift = xmldoc.CreateElement("obtainedGift");
                    obtainedGift.InnerText = thing.ObtainedGift;
                    thingNode.AppendChild(obtainedGift);
                }
                XmlElement excludes = xmldoc.CreateElement("excludes");
                foreach (RealThing t in thing.ExcludeThing)
                {
                    XmlElement tNode = xmldoc.CreateElement("thing");
                    XmlElement nameNode = xmldoc.CreateElement("name");
                    nameNode.InnerText = t.Name;
                    tNode.AppendChild(nameNode);
                    excludes.AppendChild(tNode);
                }
                thingNode.AppendChild(excludes);

                thingsNode.AppendChild(thingNode);
            }
            stepNode.AppendChild(thingsNode);
        }
    }
}
