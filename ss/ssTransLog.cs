using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ss {
    public class ssTransLog {
        public ssTransLog(ssEd e) {
            ts = null;
            log = true;
            ed = e;
            }

        //public void NewTrans() {
        //    curid++;
        //    }

        public void LogTrans(ssTrans.Type typ, ssRange r, ssText t, string s) {
            if (log) ts = new ssTrans(typ, ed.CurTransId, new ssAddress(r, t), s, ts);
            }

        public void LogTrans(ssTrans.Type typ, ssAddress aa, string ss) {
            if (log) ts = new ssTrans(typ, ed.CurTransId, aa, ss, ts);
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
            //long id = ts.id;
            while (ts != null && ts.id == id) {
                if (ts.a != null) {
                    ts.a.txt.dot = ts.a.rng;
                    switch (ts.typ) {
                        case ssTrans.Type.rename:
                            ts.a.txt.Rename(ts.s);
                            break;
                        case ssTrans.Type.delete:
                            ts.a.txt.Delete();
                            break;
                        case ssTrans.Type.insert:
                            ts.a.txt.Insert(ts.s);
                            break;
                        }
                    }
                ts = ts.nxt;
                }
            log = true;
            }

        public bool Log {
            get { return log; }
            set { log = value; }
            }

        public ssTrans Ts {
            get { return ts; }
                }

        //long curid;
        ssEd ed;
        ssTrans ts;
        bool log;
        }
    }
