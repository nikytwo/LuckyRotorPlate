using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using LuckyRotorPlate.Model;

namespace LuckyRotorPlate
{
    /// <summary>
    /// AddThing.xaml 的交互逻辑
    /// </summary>
    public partial class AddThing : Window
    {
        private RealThing thing;
        private bool isAdd = true;

        public bool IsAdd
        {
            get { return isAdd; }
            private set { isAdd = value; }
        }
        private ObservableCollection<RealThing> thingsForExcludes;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="th">待编辑的对象，创建新对象请设置为 null</param>
        /// <param name="thingsForExcludes">例外的筛选列表对象</param>
        public AddThing(RealThing th, ObservableCollection<RealThing> thingsForExcludes)
        {
            InitializeComponent();

            thing = th;
            if (null == thingsForExcludes)
            {
                //
            }
            this.thingsForExcludes = thingsForExcludes;
            initThing();
        }

        private void initThing()
        {
            if (null != thing)
            {
                isAdd = false;
                this.InputName.Text = thing.Name;
                this.InputValue.Text = thing.Value.ToString();
                this.InputRemark.Text = thing.Remark;
                foreach (RealThing t in thing.ExcludeThing)
                {
                    this.ExcludeList.Items.Add(t.Name);
                }
                this.Title = "编辑" + Title;
            }
            else
            {
                this.Title = "添加" + Title;
            }
        }

        public RealThing Thing
        {
            get { return thing; }
            set 
            { 
                thing = value;
                initThing();
            }
        }

        public ObservableCollection<RealThing> ThingsForExcludes
        {
            get { return thingsForExcludes; }
            set { thingsForExcludes = value; }
        }

        private void AddExclude_Click(object sender, RoutedEventArgs e)
        {
            //显示例外的列表并添加
            SelectList sl;
            sl = new SelectList(thingsForExcludes);
            sl.Owner = this;
            sl.ShowDialog();
            if (null != sl.Things)
            {
                foreach (RealThing thing in sl.Things)
                {
                    if (!ExcludeList.Items.Contains(thing.Name) && !thing.Name.Equals(string.Empty))
                    {
                        ExcludeList.Items.Add(thing.Name);
                    }
                }
            }
        }

        private void SubmitThing(object sender, RoutedEventArgs e)
        {
            if (SaveThing())
            {
                this.Close();
            }
        }

        private bool SaveThing()
        {
            bool rst = false;
            ObservableCollection<RealThing> excList;

            //设置全局集合
            excList = thingsForExcludes;

            //验证
            double value;
            if (!string.Empty.Equals(this.InputName.Text) && double.TryParse(this.InputValue.Text, out value) && value > 0)
            {
                //设置信息
                if (isAdd)
                {
                    thing = new RealThing(this.InputName.Text, double.Parse(this.InputValue.Text), this.InputRemark.Text);
                }
                else
                {
                    thing.Name = this.InputName.Text;
                    thing.Value = double.Parse(this.InputValue.Text);
                    thing.Remark = this.InputRemark.Text;
                }
                //例外信息
                for (int i = thing.ExcludeThing.Count - 1; i >= 0; i--)
                {
                    RealThing t = thing.ExcludeThing[i];
                    if (!ExcludeList.Items.Contains(t.Name))
                    {
                        thing.ExcludeThing.Remove(t);
                    }
                }
                foreach (string item in ExcludeList.Items)
                {
                    foreach (RealThing t in excList)
                    {
                        if (t.Name == item && !thing.ExcludeThing.Contains(t) && !thing.ExcludeThingContainName(t))
                        {
                            thing.ExcludeThing.Add(t);
                        }
                    }
                }
                rst = true;
            }

            return rst;
        }

        private void DelExclude_Click(object sender, RoutedEventArgs e)
        {
            if (ExcludeList.SelectedIndex >= 0)
            {
                ExcludeList.Items.RemoveAt(ExcludeList.SelectedIndex);
            }
        }
    }
}
