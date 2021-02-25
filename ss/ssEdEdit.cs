using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace ss {
    public partial class ssEd {
        ssTrans seqRoot;
        //ssTrans seqTail;

        public void SetDot(ssText t, ssRange r) {
            edDot.rng = r;
            edDot.txt = t;
            }

        public void InitAllSeqs() {
            seqRoot.nxt = null;
            //seqTail = seqRoot;
            for (ssText tt = txts; tt != null; tt = tt.Nxt) tt.InitSeq();
            }

        public void Commit() {
            ssTrans t = seqRoot.nxt;
            while (t != null) {
                if (t.a != null) {  // could be null if a D command is part of a compound command
                    if (t.typ != ssTrans.Type.rename) t.a.txt.CheckSeq(ref t.a.rng, t.s != null);
                    t.a.txt.dot = t.a.rng;
                    switch (t.typ) {
                        case ssTrans.Type.rename:
                            string n = t.a.txt.Nm;
                            t.a.txt.Rename(t.s);
                            t.s = n;
                            break;
                        case ssTrans.Type.delete:
                            t.s = t.a.txt.ToString();
                            t.a.rng = t.a.txt.Delete();
                            t.typ = ssTrans.Type.insert;
                            break;
                        case ssTrans.Type.insert:
                            t.a.rng = t.a.txt.Insert(t.s);
                            t.s = null;
                            t.typ = ssTrans.Type.delete;
                            break;
                        }

                    }
                ssTrans tt = t.nxt; // Grab t.nxt before LogTrans changes it.
                if (t.a != null) {
                    if (tt == null) {
                        if (t.typ != ssTrans.Type.rename) t.a.txt.dot = t.a.rng;
                        t.a.txt.SyncFormToText();
                        }
                    TLog.LogTrans(t);  // Form keeps from logging ed.log transactions. We don't check it here.
                    }
                t = tt;
                }
            }

        void PushTrans(ssTrans t) {
            t.nxt = seqRoot.nxt;
            seqRoot.nxt = t;
            }

        public void Rename(string s) {
            ssTrans t = new ssTrans(ssTrans.Type.rename, 0, edDot.Copy(), s, null);
            PushTrans(t);
            }

        public void Delete() {
            ssTrans t = new ssTrans(ssTrans.Type.delete, 0, edDot.Copy(), null, null);
            PushTrans(t);
            }

        public void Insert(string s) {
            ssTrans t = new ssTrans(ssTrans.Type.insert, 0, edDot.Copy(), s, null);
            PushTrans(t);
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
            PushTrans(t1);
            if (t2 != null) PushTrans(t2);
            }

        public void Change(string s) {
            ssAddress ai = edDot.Copy();
            PushTrans(new ssTrans(ssTrans.Type.insert, 0, ai, s, null));
            seqRoot.nxt.a.rng.len = s.Length;
            ssAddress ad = edDot.Copy();
            PushTrans(new ssTrans(ssTrans.Type.delete, 0, ad, null, null));
            }
        }
    }