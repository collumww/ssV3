using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace ss {
    public partial class ssEd {
        //ssTrans seqRoot;

        public void InitAllSeqs() {
            for (ssText tt = txts; tt != null; tt = tt.Nxt) tt.InitSeq();
            }

        public void Rename(string s) {
            ssTrans t = new ssTrans(ssTrans.Type.rename, 0, edDot.Copy(), s, null);
            t.a.txt.PushTrans(t);
            }

        public void Delete() {
            ssTrans t = new ssTrans(ssTrans.Type.delete, 0, edDot.Copy(), null, null);
            t.a.txt.PushTrans(t);
            }

        public void Insert(string s) {
            ssTrans t = new ssTrans(ssTrans.Type.insert, 0, edDot.Copy(), s, null);
            t.a.txt.PushTrans(t);
            t.a.rng.len = s.Length;
            }

        public void Insert(char c) {
            Insert(c.ToString());
            }

        public void MoveCopy(char cmd, ssAddress dst) {
            dst.rng.l = dst.rng.r;
            if (txt == dst.txt && txt.dot.Overlaps(dst.rng)) throw new ssException("addresses overlap");
            string s = txt.ToString();
            ssTrans t1 = new ssTrans(ssTrans.Type.insert, 0, dst, s, null);
            ssTrans t2 = null;
            if (cmd == 'm') {
                t2 = new ssTrans(ssTrans.Type.delete, 0, edDot.Copy(), null, null);
                t1.a.rng.len = s.Length;
                if (t2.a.rng.l < t1.a.rng.l) {
                    ssTrans x = t1;
                    t1 = t2;
                    t2 = x;
                    }
                }
            t1.a.txt.PushTrans(t1);
            if (t2 != null) t2.a.txt.PushTrans(t2);
            }

        public void Change(string s) {
            ssAddress ai = edDot.Copy();
            ai.txt.PushTrans(new ssTrans(ssTrans.Type.insert, 0, ai, s, null));
            ai.txt.seqRoot.nxt.a.rng.len = s.Length;
            ssAddress ad = edDot.Copy();
            ad.txt.PushTrans(new ssTrans(ssTrans.Type.delete, 0, ad, null, null));
            }
        }
    }