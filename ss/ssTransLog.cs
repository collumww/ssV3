using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ss {
    public class ssTransLog {
        public ssTransLog(ssEd e, ssText t) {
            ts = null;
            log = true;
            ed = e;
            txt = t;
            getnewtrans = true;
            olddot = new ssRange();
            }

        //public void NewTrans() {
        //    curid++;
        //    }

        public void BeginTrans() {
            if (log && getnewtrans) {
                ed.NewTransId();
                getnewtrans = false;
                }
            }

        public void LogTrans(ssTrans.Type typ, ssRange r, ssText t, string s) {
            if (log) {
                ts = new ssTrans(typ, ed.CurTransId, r, s, ts);
                }
            }

        public void LogTrans(ssTrans t) {
            if (log) {
                t.id = ed.CurTransId;
                t.nxt = ts;
                ts = t;
                }
            }

        public void Undo(long id) {
            if (ts == null) return;
            log = false;
            while (ts != null && ts.id == id) {
                txt.dot = ts.rng;
                switch (ts.typ) {
                    case ssTrans.Type.rename:
                        txt.Rename(ts.s);
                        break;
                    case ssTrans.Type.delete:
                        txt.Delete();
                        break;
                    case ssTrans.Type.insert:
                        txt.Insert(ts.s);
                        break;
                    case ssTrans.Type.dot:
                        txt.dot = ts.rng;
                        break;
                    }
                ts = ts.nxt;
                }
            log = true;
            txt.changeCnt--;
            }

        public bool Log {
            get { return log; }
            set { log = value; }
            }

        public ssTrans Ts {
            get { return ts; }
                }

        public void InitTrans() {
            getnewtrans = true;
            olddot = txt.dot;
            }

        public ssRange OldDot {
            get { return olddot; }
            }

        ssEd ed;
        ssText txt;
        ssTrans ts;
        ssRange olddot;
        public bool getnewtrans;
        bool log;
        }
    }
