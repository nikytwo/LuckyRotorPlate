using System;
using System.Collections.ObjectModel;

namespace LuckyRotorPlate.Model
{
    public class Solution
    {
        private ObservableCollection<StepInfo> stepList;
        private String way;
        private String name;
        private double drap;

        /// <summary>
        /// 阻力系数
        /// </summary>
        public double Drap
        {
            get { return drap; }
            set { drap = value; }
        }

        public Solution(String name, ObservableCollection<StepInfo> steps)
        {
            this.name = name;
            this.drap = 0.002;
            this.stepList = steps;
        }

        public Solution()
        {
            this.stepList = new ObservableCollection<StepInfo>();
        }

        public String Name
        {
            get { return name; }
            set { name = value; }
        }

        public ObservableCollection<StepInfo> StepList
        {
            get { return stepList; }
            set { stepList = value; }
        }

        public String Way
        {
            get { return way; }
            set { way = value; }
        }
    }

    public class StepInfo
    {
        private ObservableCollection<RealThing> pies;
        private ObservableCollection<RealThing> players;
        private ObservableCollection<RealThing> excludePies;
        private String sortWay;
        private String name;

        public StepInfo(String name, ObservableCollection<RealThing> pies, ObservableCollection<RealThing> players,
            ObservableCollection<RealThing> results, ObservableCollection<RealThing> excludePies)
        {
            this.name = name;
            this.pies = pies;
            this.players = players;
            this.excludePies = excludePies;
        }

        public StepInfo()
        {
            this.pies = new ObservableCollection<RealThing>();
            this.players = new ObservableCollection<RealThing>();
            this.excludePies = new ObservableCollection<RealThing>();
        }

        public String Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// 转盘排序方式
        /// </summary>
        public String SortWay
        {
            get { return sortWay; }
            set { sortWay = value; }
        }

        /// <summary>
        /// 转盘圆饼信息
        /// </summary>
        public ObservableCollection<RealThing> Pies
        {
            get { return pies; }
            set { pies = value; }
        }

        /// <summary>
        /// 参与转转盘的人
        /// </summary>
        public ObservableCollection<RealThing> Players
        {
            get { return players; }
            set { players = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<RealThing> ExcludePies
        {
            get { return excludePies; }
            set { excludePies = value; }
        }

        public static bool ThingsContainName(String name, ObservableCollection<RealThing> Things)
        {
            bool rst = false;
            foreach (RealThing t in Things)
            {
                if (t.Name.Equals(name))
                {
                    rst = true;
                    break;
                }
            }
            return rst;
        }

        public static bool ThingsContainNameAndValue(String name, double value, ObservableCollection<RealThing> Things)
        {
            bool rst = false;
            foreach (RealThing t in Things)
            {
                if (t.Name.Equals(name) && t.Value == value)
                {
                    rst = true;
                    break;
                }
            }
            return rst;
        }

        public bool PiesContainNameAndValue(String name, double value)
        {
            return ThingsContainNameAndValue(name, value, pies);
        }

        public bool PlayersContainNameAndValue(String name, double value)
        {
            return ThingsContainNameAndValue(name, value, players);
        }
    }
}
