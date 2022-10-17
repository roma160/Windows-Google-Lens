using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Windows_Google_Lens.Views.Custom
{
    public class ComboBoxItemTemplateSelector : DataTemplateSelector
    {
        // Can set both templates from XAML
        public DataTemplate SelectedItemTemplate { get; set; }
        public DataTemplate ItemTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            bool selected = false;
            
            var fe = container as FrameworkElement;
            var parent = fe?.TemplatedParent;
            var cbo = parent as ComboBox;
            if (cbo != null)
                selected = true;

            return selected ? SelectedItemTemplate : ItemTemplate;
        }
    }
}
