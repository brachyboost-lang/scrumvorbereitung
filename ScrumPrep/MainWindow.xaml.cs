using System.Windows;
using ScrumPrep.ViewModels;

namespace ScrumPrep;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
