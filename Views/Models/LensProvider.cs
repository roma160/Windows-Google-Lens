using System;
using System.Windows.Media;

namespace Windows_Google_Lens.Views.Models
{
    public class LensProvider
    {
        public Lens.Provider Provider { get; set; }
        public DrawingBrush Icon { get; set; }

        public String Name => Provider.ProviderName;

        public LensProvider()
        {
            Provider = null;
            Icon = null;
        }
    }
}
