using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monitoring_net9.Models
{
    public class AppSettings
    {
        public string SelectedScreen
        {
            get;
            set;
        } = "DISPLAY1";

        public bool Fullscreen
        {
            get;
            set;
        } = true;
    }
}
