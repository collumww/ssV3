using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ss {
    public class ssAddress {
        public ssAddress(ssRange r, ssText t) {
            rng = r;
            txt = t;
            }
        public ssAddress(int l, int r, ssText t) {
            rng = new ssRange(l, r);
            txt = t;
            }
        public ssAddress Copy() {
            return new ssAddress(rng, txt);
            }
        public ssRange rng;
        public ssText txt;
        }
    }
