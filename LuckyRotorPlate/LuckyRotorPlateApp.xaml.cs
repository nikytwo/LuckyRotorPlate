using System.Collections.ObjectModel;
using System.Configuration;
using System.Reflection;
using System.Windows;
using LuckyRotorPlate.DataService;
using LuckyRotorPlate.Model;

namespace LuckyRotorPlate
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class LuckyRotorPlateApp : Application
    {
        private ObservableCollection<RealThing> employees;
        private ObservableCollection<RealThing> gifts;
        private ObservableCollection<Solution> solutions;
        private AppSettingsSection appSettings;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            loadConfig();
            loadData();
        }

        private void loadConfig()
        {
            //读取程序集的配置文件
            string assemblyConfigFile = Assembly.GetEntryAssembly().Location;

            Configuration config = ConfigurationManager.OpenExeConfiguration(assemblyConfigFile);
            //获取appSettings节点
            appSettings = (AppSettingsSection)config.GetSection("appSettings");    
        }

        private void loadData()
        {
            employees = XmlHelper.getRealThings(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase
                + appSettings.Settings["EmployeesData"].Value, null);
            gifts = XmlHelper.getRealThings(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase
                + appSettings.Settings["GiftsData"].Value, employees);
            solutions = XmlHelper.getSolutions(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase
                + appSettings.Settings["SolutionsData"].Value);
            //StepInfo s = solutions[0].StepList[0];
            //备份
            XmlHelper.SaveRealThings(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase
                + appSettings.Settings["EmployeesData"].Value + ".bak", employees);
            XmlHelper.SaveRealThings(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase
                + appSettings.Settings["GiftsData"].Value + ".bak", gifts);
            XmlHelper.SaveSolutions(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase
                + appSettings.Settings["SolutionsData"].Value + ".bak", solutions);
        }

        public AppSettingsSection AppSettings
        {
            get { return appSettings; }
            set { appSettings = value; }
        }

        public ObservableCollection<RealThing> Employees
        {
            get { return employees; }
            set { employees = value; }
        }

        public ObservableCollection<Solution> Solutions
        {
            get { return solutions; }
            set { solutions = value; }
        }

        public ObservableCollection<RealThing> Gifts
        {
            get { return gifts; }
            set { gifts = value; }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            //
        }
    }
}
