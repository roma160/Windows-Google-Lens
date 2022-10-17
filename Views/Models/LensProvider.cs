using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Windows_Google_Lens.Views.Models
{
    public class LensProvider
    {
        public Lens.Provider Provider { get; set; }
        public ImageSource Icon { get; set; }

        public String Name => Provider.ProviderName;

        public LensProvider()
        {
            Provider = null;
            Icon = null;
        }
    }
}
