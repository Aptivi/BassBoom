using Avalonia.Controls;
using BassBoom.Basolia;
using BassBoom.Basolia.File;
using BassBoom.Basolia.Format;

namespace BassBoom.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        if (!Design.IsDesignMode)
            InitBasolia.Init();
        InitializeComponent();
    }
}