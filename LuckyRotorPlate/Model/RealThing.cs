using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace LuckyRotorPlate.Model
{
    public class RealThing : INotifyPropertyChanged, ICloneable
    {
        private String name;
        private double value;
        private String remark;
        private String obtainedGift;

        private ObservableCollection<RealThing> excludeThing;

        public event PropertyChangedEventHandler PropertyChanged;

        public RealThing(String name, double value, String remark)
        {
            this.name = name;
            this.value = value;
            this.remark = remark;
            excludeThing = new ObservableCollection<RealThing>();
            excludeThing.CollectionChanged += 
                new System.Collections.Specialized.NotifyCollectionChangedEventHandler(excludeThing_CollectionChanged);
        }

        void excludeThing_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //关联添加删除
            if (null != e.NewItems)
            {
                for (int i = 0;i < e.NewItems.Count; i++)
                {
                    RealThing thing = e.NewItems[i] as RealThing;
                    if (!thing.ExcludeThing.Contains(this) && !thing.ExcludeThingContainName(this))
                    {
                        thing.ExcludeThing.Add(this);
                    }
                }
            }
            if (null != e.OldItems)
            {
                for (int i = 0; i < e.OldItems.Count; i++)
                {
                    RealThing thing = e.OldItems[i] as RealThing;
                    if (thing.ExcludeThing.Contains(this) || thing.ExcludeThingContainName(this))
                    {
                        thing.ExcludeThing.Remove(this);
                    }
                }
            }
        }

        public bool ExcludeThingContainName(RealThing thing)
        {
            bool rst = false;

            foreach (RealThing t in excludeThing)
            {
                if (t.name.Equals(thing.Name))
                {
                    rst = true;
                    break;
                }
            }

            return rst;
        }

        /// <summary>
        /// 获得的礼品
        /// </summary>
        public String ObtainedGift
        {
            get { return obtainedGift; }
            set
            {
                obtainedGift = value;
                OnPropertyChanged("ObtainedGift");
            }
        }

        public override string ToString()
        {
            return name.ToString();
        }

        /// <summary>
        /// 显示名称
        /// </summary>
        public String Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }

        /// <summary>
        /// (所占)值
        /// </summary>
        public double Value
        {
            get { return this.value; }
            set
            {
                this.value = value;
                OnPropertyChanged("Value");
            }
        }

        /// <summary>
        /// 备注
        /// </summary>
        public String Remark
        {
            get { return remark; }
            set
            {
                remark = value;
                OnPropertyChanged("Remark");
            }
        }

        public ObservableCollection<RealThing> ExcludeThing
        {
            get { return excludeThing; }
            set
            {
                excludeThing = value;
                OnPropertyChanged("ExcludeThing");
                excludeThing.CollectionChanged += 
                    new System.Collections.Specialized.NotifyCollectionChangedEventHandler(excludeThing_CollectionChanged);
            }
        }

        protected void OnPropertyChanged(string info)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(info));
            }
        }

        #region ICloneable 成员

        public object Clone()
        {
            RealThing clone = new RealThing(Name, Value, Remark);
            foreach (RealThing t in ExcludeThing)
            {
                clone.ExcludeThing.Add(new RealThing(t.Name, t.Value, t.Remark));
            }

            return clone;
        }

        #endregion
    }

}
