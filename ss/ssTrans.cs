using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ss {
    public class ssTrans {
        public ssTrans(Type t, long i, ssAddress aa, string ss, ssTrans nnxt) {
            typ = t;
            id = i;
            a = aa;
            s = ss;
            nxt = nnxt;
            }

        public enum Type { insert, delete, rename };

        public Type typ;
        public long id;         // A large edit command will consist of many of these with the same id number
        public ssAddress a;
        public string s;       // null here means it was an insert. Presence of a string means a delete.
        public ssTrans nxt;
        }
    }
