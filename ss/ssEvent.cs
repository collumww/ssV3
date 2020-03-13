using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;

namespace ss {
    public enum ssEventType { down, press, up, move, turn };
    public enum ssEventCmdOption { none, exec, execPreserveDot, begin };

    public class ssEvent {
        public ssEvent() {
            c = '\0';
            a = null;
            cont = false;
            cmdopt = ssEventCmdOption.none;
            cmd = "";
            }

        public ssEvent(Keys kk, char cc, ssEventType tt, MethodInfo aa, bool con) {
            k = kk;
            c = cc;
            t = tt;
            a = aa;
            cont = con;
            cmdopt = ssEventCmdOption.none;
            }

        public string ToString() {
            string s = String.Format("{0} {1} {2} {3}", k, c, t, a);
            return s;
            }



        public ssEvent copy() {
            ssEvent e = new ssEvent(k, c, t, a, cont);
            e.cmd = cmd;
            e.cmdopt = cmdopt;
            if (nxt != null) e.nxt = nxt.copy();
            if (alt != null) e.alt = alt.copy();
            return e;
            }
    

        public Keys k;
        public char c;
        public ssEventType t;
        public MethodInfo a;
        public bool cont;
        public string cmd;
        public ssEventCmdOption cmdopt;
        public ssEvent alt;
        public ssEvent nxt;
        }
    }
