using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ss {
    public struct ssCursor {
        int anch;
        int chn;
        int bt;
        ssRange range;

        public ssCursor(int i) {
            range.r = range.l = anch = chn = bt = i;
            }

        public ssCursor(int a, int b) {
            anch = chn = a;
            bt = b;
            range.l = range.r = 0;  // Compiler tells me I have to set values for left and right before I can use SetRange. Go figure...
            SetRange();
            }

        public ssCursor(ssRange r) {
            anch = chn = r.l;
            bt = r.r;
            range.l = range.r = 0;
            SetRange();
            }

        public ssCursor To(int i) {
            anch = chn = bt = i;
            range.To(i);
            return this;
            }

        public ssCursor To(ssRange r) {
            anch = chn = r.l;
            bt = r.r;
            SetRange();
            return this;
            }

        public ssCursor ExtendTo(int i) {
            chn = bt;
            bt = i;
            SetRange();
            return this;
            }

        public ssCursor Adjust(int loc, int siz, bool insert) {
            range.Adjust(loc, siz, insert);
            anch = chn = range.l;
            bt = range.r;
            return this;
            }

        public ssRange rng {
            get { return range; }
            }

        public ssRange extRng {
            get { return new ssRange(chn, bt).Normalize(); }
            }

        public bool Empty {
            get { return range.Empty; }
            }

        public int l {
            get { return range.l; }
            }

        public int r {
            get { return range.r; }
            }

        public int boat {
            get { return bt; }
            }

        public int anchor {
            get { return anch; }
            }

        public int chain {
            get { return chn; }
            }

        void SetRange() {
            if (anch < bt) {
                range.l = anch;
                range.r = bt;
                }
            else {
                range.l = bt;
                range.r = anch;
                }
            }

        }
    }
