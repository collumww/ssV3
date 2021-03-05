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
            for (ssText tt = txts; tt != null; tt = tt.Nxt) tt.TLog.InitSeq();
            }

        public void Rename(string s) {
            ssTrans t = new ssTrans(ssTrans.Type.rename, 0, txt.dot, s, null);
            txt.TLog.PushTrans(t);
            }

        public void Delete() {
            ssTrans t = new ssTrans(ssTrans.Type.delete, 0, txt.dot, null, null);
            txt.TLog.PushTrans(t);
            }

        public void Insert(string s) {
            ssTrans t = new ssTrans(ssTrans.Type.insert, 0, txt.dot, s, null);
            txt.TLog.PushTrans(t);
            txt.dot.len = s.Length;
            }

        public void Insert(char c) {
            Insert(c.ToString());
            }

        public void MoveCopy(char cmd, ssAddress dst) {
            dst.rng.l = dst.rng.r;
            if (txt == dst.txt && txt.dot.Overlaps(dst.rng)) throw new ssException("addresses overlap");
            string s = txt.ToString();
            ssTrans t1 = new ssTrans(ssTrans.Type.insert, 0, dst.rng, s, null);
            ssText txt1 = dst.txt;
            ssTrans t2 = null;
            ssText txt2 = null;
            if (cmd == 'm') {
                t2 = new ssTrans(ssTrans.Type.delete, 0, txt.dot, null, null);
                txt2 = txt;
                t1.rng.len = s.Length;
                if (t2.rng.l < t1.rng.l) {
                    ssTrans x = t1;
                    t1 = t2;
                    t2 = x;
                    ssText txtx = txt1;
                    txt1 = txt2;
                    txt2 = txtx;
                    }
                }
            txt1.TLog.PushTrans(t1);
            if (t2 != null) txt2.TLog.PushTrans(t2);
            }

        public void Change(string s) {
            ssAddress ai = edDot.Copy();
            txt.TLog.PushTrans(new ssTrans(ssTrans.Type.insert, 0, txt.dot, s, null));
            txt.TLog.seqRoot.nxt.rng.len = s.Length;
            ssAddress ad = edDot.Copy();
            txt.TLog.PushTrans(new ssTrans(ssTrans.Type.delete, 0, txt.dot, null, null));
            }
        }
    }