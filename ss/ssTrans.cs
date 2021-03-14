using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ss {
    public class ssTrans {
        public ssTrans(Type t, long i, ssRange r, string ss, ssTrans nnxt) {
            typ = t;
            id = i;
            rng = r;
            s = ss;
            cnt = 1;
            nxt = nnxt;
            }

        public enum Type { insert, delete, rename, dot };

        public Type typ;
        public long id;         // A large edit command will consist of many of these with the same id number
        public ssRange rng;
        public string s;
        public long cnt;
        public ssTrans nxt;
        }
    }
