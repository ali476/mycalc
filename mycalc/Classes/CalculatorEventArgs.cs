using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mycalc.Classes
{
    class CalculatorEventArgs : EventArgs
    {
        public String DisplayValue { get; set; }
        public String History { get; set; }
        public String Memory { get; set; }
    }
}
