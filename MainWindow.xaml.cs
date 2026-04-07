using System.Windows;
using ProductionSimulation.ViewModels;

namespace ProductionSimulation
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}