using Pyrocorelib;

namespace S2_IM_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            Properties.Settings.Default.ConnectionString = MySqlHelper.ReturnConnectionString("instantmessenger");
            Properties.Settings.Default.Save();
        }
    }
}
