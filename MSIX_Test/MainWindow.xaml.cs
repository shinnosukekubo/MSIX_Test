using System.Threading.Tasks;
using System.Windows;

namespace MSIX_Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = new MainWindowViewModel();
           
        }
    }

    public class MainWindowViewModel
    {
        public string Version { get; set; }
        public MainWindowViewModel()
        {
            var packageService = new PackageService("994c6583-42dd-49b5-9e3a-bf85f3f8b568");
            Version = packageService.GetPackageVersionText();
            MessageBox.Show("更新します", "", MessageBoxButton.OK);
            Task.Run(async () => await packageService.UpdatePackageAsync());
        }
    }

}
