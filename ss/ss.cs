using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using System.IO;

namespace ss {
    static class ss {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            ssEd ed = new ssEd(Environment.GetCommandLineArgs(), 1);

            ed.Log = new ssText(ed, "Type 'H' for help\r\n", null, "~~ss~~", ed.defs.encoding);
            ed.Log.AddForm(new ssForm(ed, ed.Log));

            Application.Run(ed.Log.Frm);
            }
        }
    }
