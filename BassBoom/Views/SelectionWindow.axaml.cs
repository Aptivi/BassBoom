using Avalonia.Controls;
using System.Collections.ObjectModel;

namespace BassBoom.Views
{
    public partial class SelectionWindow : Window
    {
        public string SelectionInput { get; set; }

        public SelectionWindow(ObservableCollection<string> selections)
        {
            InitializeComponent();
            DataContext = new SelectionData(this)
            {
                Selections = selections
            };
            ShowInTaskbar = false;
            CanResize = false;
        }
    }

    public class SelectionData
    {
        private readonly Window view;

        public ObservableCollection<string> Selections { get; set; } = new();

        public void Acknowledge()
        {
            var thisView = (SelectionWindow)view;
            thisView.SelectionInput = thisView.selection.SelectedItem?.ToString();
            thisView.Close();
        }

        public SelectionData(Window view)
        {
            this.view = view;
        }
    }
}
