using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class TestEventArgs : System.EventArgs
    {
        public TestEventArgs() { }

        /// <summary>
        /// Event Trigger
        /// </summary>
        public object Sender { get; set; }
        /// <summary>
        /// Passing data as string
        /// </summary>
        public string Text { get; set; }
    }
}
