using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ss {
    public struct ssRange {
        public ssRange(int left, int right) {
            l = left;
            r = right;
            }
        public int l;
        public int r;

        public int len {
            get { return r - l; }
            set { r = l + value; }
            }

        public ssRange Move(int inc) {
            l += inc;
            r += inc;
            return this;
            }

        public ssRange To(int loc) {
            l = loc;
            r = loc;
            return this;
            }

        public bool Clip(ssRange dom) {
            l = Math.Max(l, dom.l);
            r = Math.Min(r, dom.r);
            return l <= r;
            }

        public ssRange Normalize() {
            if (l > r) { int x = l;  l = r; r = x; }
            return this;
            }

        public bool Overlaps(ssRange dom) {
            return r > dom.l && l < dom.r;
            }

        public bool Contains(int i) {
            return i >= l && i < r;
            }

        public bool Empty {
            get { return l == r; }
            }

        public ssRange Adjust(int loc, int siz, bool insert) {
            if (siz == 0 || loc >= r) return this;
            if (insert) {
                if (loc < l) { this.Move(siz); return this; }
                else         { r += siz; return this; }
                }
            else {
                int tr = loc + siz;
                if (tr <= l) { this.Move(-siz); return this; }
                else {
                    if (tr < r) {
                        if (loc <= l) { l = loc; r -= (tr - l); return this; }
                        else if (loc < r) { r -= siz; return this; }
                        }
                    else {
                        if (loc < l) { l = r = loc; }
                        else { r = loc; return this; } // guards above guarantee loc < r && tr >= r
                        }
                    }
                }
            return this;
            }

        public static bool operator == (ssRange l, ssRange r) {
            return l.l == r.l && l.r == r.r;
            }

        public static bool operator != (ssRange l, ssRange r) {
            return l.l != r.l || l.r != r.r;
            }

        }
    }
