using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tabber
{
    public class DragItemArgs : EventArgs
    {
        public TabberItem Tab { get; set; }
        public int MyProperty { get; set; }
    }
}
