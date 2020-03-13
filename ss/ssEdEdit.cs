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
        ssTrans seqTail;

        public void SetDot(ssText t, ssRange r) {
            edDot.rng = r;
            edDot.txt = t;
            }

        public void InitAllSeqs() {
            seqRoot.nxt = null;
            seqTail = seqRoot;
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


        public void Rename(string s) {
            seqTail.nxt = new ssTrans(ssTrans.Type.rename, 0, edDot.Copy(), s, null);
            seqTail = seqTail.nxt;
            }

        public void Delete() {
            seqTail.nxt = new ssTrans(ssTrans.Type.delete, 0, edDot.Copy(), null, null);
            seqTail = seqTail.nxt;
            }

        public void Insert(string s) {
            seqTail.nxt = new ssTrans(ssTrans.Type.insert, 0, edDot.Copy(), s, null);
            seqTail = seqTail.nxt;
            seqTail.a.rng.len = s.Length;
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
            seqTail.nxt = t1;
            seqTail = seqTail.nxt;
            if (t2 != null) { 
                seqTail.nxt = t2;
                seqTail = seqTail.nxt;
                }
            }

        public void Change(string s) {
            string ss = txt.ToString();
            ssAddress ad = edDot.Copy();
            seqTail.nxt = new ssTrans(ssTrans.Type.delete, 0, ad, null, null);
            seqTail = seqTail.nxt;
            ssAddress ai = edDot.Copy();
            ai.rng.Move(ai.rng.len);
            seqTail.nxt = new ssTrans(ssTrans.Type.insert, 0, ai, s, null);
            seqTail = seqTail.nxt;
            seqTail.a.rng.len = s.Length;
            }
        }
    }