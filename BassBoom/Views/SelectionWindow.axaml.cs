//
// BassBoom  Copyright (C) 2023  Aptivi
//
// This file is part of Nitrocid KS
//
// BassBoom is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BassBoom is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
//

using Avalonia.Controls;

namespace BassBoom.Views
{
    public partial class SelectionWindow : Window
    {
        public string SelectionInput { get; set; }

        public SelectionWindow()
        {
            InitializeComponent();
            DataContext = new SelectionData(this);
            ShowInTaskbar = false;
            CanResize = false;
        }
    }

    public class SelectionData
    {
        private readonly Window view;

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
