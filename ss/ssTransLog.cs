﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ss {
    public class ssTransLog {
        public ssTransLog(ssEd e, ssText t) {
            ts = null;
            rs = null;
            log = true;
            ed = e;
            txt = t;
            seqRoot = new ssTrans(ssTrans.Type.delete, 0, t.dot, null, null);
            adjEdge = new ssRange(0, 0);
            getnewtrans = true;
            canconsolidate = true;
            savepoint = 0;
            olddot = new ssRange();
            rex = new Regex(@"[\w\s]");
            }


        public void BeginTrans() {
            if (log && getnewtrans) {
                ed.NewTransId();
                getnewtrans = false;
                }
            }

        public void FormLogTrans(ssTrans.Type typ, ssRange r, string s) {
            if (log) {
                curChangeId++;
                if (canconsolidate
                    && ts != null
                    && ts.typ == ssTrans.Type.insert
                    && typ == ssTrans.Type.insert
                    && ts.rng.r == r.l
                    && (
                                (ts.s != null && rex.IsMatch(ts.s))
                            && 
                                (s != null && rex.IsMatch(s))
                        || 
                            s == txt.Eoln
                        )
                    ) {
                    ts.rng.r = r.r;
                    ts.s += s;
                    ts.chgid = curChangeId;
                    }
                else {
                    ts = new ssTrans(typ, ed.CurTransId, r, s, ts);
                    ts.chgid = curChangeId;
                    }
                canconsolidate = s != txt.Eoln;
                rs = null;
                }
            }

        public void EdLogTrans(ssTrans t) {
            if (log) {
                t.id = ed.CurTransId;
                t.chgid = curChangeId;
                t.nxt = ts;
                ts = t;
                curChangeId++;
                rs = null;
                }
            }

        public void Commit() {
            FormLogTrans(ssTrans.Type.dot, OldDot, "");
            ssTrans t = seqRoot.nxt;
            while (t != null) {
                txt.dot = t.rng;
                switch (t.typ) {
                    case ssTrans.Type.rename:
                        string n = txt.Nm;
                        txt.Rename(t.s);
                        t.s = n;
                        break;
                    case ssTrans.Type.delete:
                        t.s = txt.ToString();
                        t.rng = txt.Delete();
                        //t.typ = ssTrans.Type.insert;
                        break;
                    case ssTrans.Type.insert:
                        t.rng = txt.Insert(t.s);
                        //t.s = null;
                        //t.typ = ssTrans.Type.delete;
                        break;
                    }

                ssTrans tt = t.nxt; // Grab t.nxt before LogTrans changes it.
                if (tt == null) {
                    if (t.typ != ssTrans.Type.rename) txt.dot = t.rng;
                    txt.SyncFormToText();
                    }
                EdLogTrans(t);  // Form keeps from logging ed.log transactions. We don't check it here.
                t = tt;
                }
            }

        public void PushTrans(ssTrans t) {
            switch (t.typ) {
                case ssTrans.Type.insert:
                    CheckSeq(ref t.rng, true);
                    break;
                case ssTrans.Type.delete:
                    CheckSeq(ref t.rng, false);
                    break;
                }
            BeginTrans();
            t.nxt = seqRoot.nxt;
            seqRoot.nxt = t;
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
                    case ssTrans.Type.insert:
                        ts.rng = txt.Delete();
                        break;
                    case ssTrans.Type.delete:
                        ts.rng = txt.Insert(ts.s);
                        break;
                    case ssTrans.Type.dot:
                        txt.dot = ts.rng;
                        break;
                    }
                ssTrans x = rs;
                rs = ts;
                ts = ts.nxt;
                rs.nxt = x;
                }
            log = true;
            txt.InvalidateMarks();
            }

        public void Redo(long id) {
            if (rs == null) return;
            log = false;
            while (rs != null && rs.id == id) {
                txt.dot = rs.rng;
                switch (rs.typ) {
                    case ssTrans.Type.rename:
                        txt.Rename(rs.s);
                        break;
                    case ssTrans.Type.delete:
                        rs.rng = txt.Delete();
                        break;
                    case ssTrans.Type.insert:
                        rs.rng = txt.Insert(rs.s);
                        break;
                    case ssTrans.Type.dot:
                        txt.dot = rs.rng;
                        break;
                    }
                ssTrans x = ts;
                ts = rs;
                rs = rs.nxt;
                ts.nxt = x;
                }
            log = true;
            txt.InvalidateMarks();
            }

        public void CheckSeq(ref ssRange r, bool insert) {
            if (insert) r.r = r.l;
            if (adjEdge.r > r.l) {
                txt.dot = olddot;
                throw new ssException("changes not in sequence");
                }
            adjEdge = r;
            }

        public bool Log {
            get { return log; }
            set { log = value; }
            }

        public ssTrans Ts {
            get { return ts; }
            }

        public ssTrans Rs {
            get { return rs; }
            }

        public void InitTrans() {
            getnewtrans = true;
            olddot = txt.dot;
            ed.ResetConsolidation(txt);
            }

        public ssRange OldDot {
            get { return olddot; }
            }

        public void InitSeq() {
            adjEdge.l = 0;
            adjEdge.r = 0;
            seqRoot.nxt = null;
            }


        public void DisableConsolidation() {
            canconsolidate = false;
            }

        public bool Changed {
            get { return ts == null && savepoint != 0 ||
                    ts != null && ts.chgid != savepoint; }
            }


        public void RecordSave() {
            if (ts != null) savepoint = ts.chgid;
            else savepoint = 0;
            canconsolidate = false;
            txt.InvalidateMarks();
            }

        ssEd ed;
        ssText txt;
        ssTrans ts;
        ssTrans rs;
        ssRange adjEdge;
        public long curChangeId;
        long savepoint;
        public ssTrans seqRoot;
        ssRange olddot;
        public bool getnewtrans;
        public bool canconsolidate;
        bool log;
        Regex rex;
        }
    }
