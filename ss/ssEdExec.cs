using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Globalization;

namespace ss {
    public partial class ssEd {
        int one;
        int zero;
        int alphaUpper;
        int alphaLower;
        void PostEdDot() {
            txt.SyncFormToText();
            }


        public string ToAlphaString(int n, char strt) {
            string s = "";
            do {
                int dig = n % 26;
                s = (char)(dig + strt) + s;
                n /= 26;
                }
            while (n > 0);
            return s;
            }

        string[] SListToArray(SList ss) {
            int cnt = 0;
            for (SList s = ss; s != null; s = s.nxt) cnt++;
            string[] a = new string[cnt];
            int i = 0;
            for (SList s = ss; s != null; s = s.nxt) a[i++] = s.s;
            return a;
            }

        void xCmd(CTree t) {
            ssRange levdot = new ssRange();
            if (t == null) return;
            ssAddress ad = xAddr(t.ad);
            ssAddress a = xAddr(t.a);
            if (txt != null) levdot = txt.dot;
            if (a != null) {
                if (a.txt == null) a.txt = txt;
                else {
                    prvtxt = txt;
                    txt = a.txt;
                    }
                txt.dot = a.rng;
                }
            switch (t.cmd) {
                case 'p':
                    Print();
                    break;
                case '=':
                    MsgLn(AddressStr());
                    break;
                case 'n':
                    //MsgLn("working directory '" + Environment.CurrentDirectory + "'");
                    for (ssText txt = txts; txt != null; txt = txt.Nxt)
                        MsgLn(txt.MenuLine());
                    break;
                case 'a':
                    if (swallowing) return;
                    txt.dot = txt.dot.To(txt.dot.r);
                    if (t.subs != null) Insert(DoSubs("", t.subs, t.s));
                    else Insert(t.s);
                    break;
                case 'i':
                    if (swallowing) return;
                    txt.dot = txt.dot.To(txt.dot.l);
                    if (t.subs != null) Insert(DoSubs("", t.subs, t.s));
                    else Insert(t.s);
                    break;
                case 'c':
                    if (swallowing) return;
                    if (t.subs != null) Change(DoSubs(txt.ToString(), t.subs, t.s));
                    else Change(t.s);
                    break;
                case 't':
                case 'm':
                    MoveCopy(t.cmd, ad);
                    break;
                case 'x':
                    if (t.sub.nxt.cmd == noCmd) t.sub.nxt.cmd = 'p';
                    RegexOptions opts = RegexOptions.Multiline;
                    if (!defs.senseCase) opts |= RegexOptions.IgnoreCase;
                    MatchCollection ms = Regex.Matches(txt.ToString(), t.s, opts);
                    ssRange strt = txt.dot;
                    int cnt = 0;
                    foreach (Match m in ms) {
                        cnt++;
                        if (t.n == cnt || t.n == -1) {
                            txt.dot.l = strt.l + m.Index;
                            txt.dot.len = m.Length;
                            xCmd(t.sub);
                            }
                        }
                    break;
                case 'y':
                    if (t.sub.nxt.cmd == noCmd) t.sub.nxt.cmd = 'p';
                    opts = RegexOptions.Multiline;
                    if (!defs.senseCase) opts |= RegexOptions.IgnoreCase;
                    ms = Regex.Matches(txt.ToString(), t.s, opts);
                    strt = txt.dot;
                    int l = strt.l;
                    cnt = 0;
                    foreach (Match m in ms) {
                        cnt++;
                        if (t.n == cnt || t.n == -1) {
                            txt.dot.l = l;
                            txt.dot.r = m.Index + strt.l;
                            xCmd(t.sub);
                            }
                        l = strt.l + m.Index + m.Length;
                        }
                    cnt++;
                    if (t.n == cnt || t.n == -1) {
                        txt.dot.l = l;
                        txt.dot.r = strt.r;
                        xCmd(t.sub);
                        }
                    break;
                case 'X':
                case 'Y':
                    t.txts = FindText(true, t.s, t.cmd == 'X', false);
                    if (t.txts == null) throw new ssException("file search");
                    for (TList tl = t.txts; tl != null; tl = tl.nxt) {
                        txt = tl.t;
                        xCmd(t.sub);
                        }
                    break;
                case 'g':
                    opts = RegexOptions.Multiline;
                    if (!defs.senseCase) opts |= RegexOptions.IgnoreCase;
                    if (Regex.IsMatch(txt.ToString(), t.s, opts)) xCmd(t.sub);
                    break;
                case 'v':
                    opts = RegexOptions.Multiline;
                    if (!defs.senseCase) opts |= RegexOptions.IgnoreCase;
                    if (!Regex.IsMatch(txt.ToString(), t.s, opts)) xCmd(t.sub);
                    break;
                case 's':
                    opts = RegexOptions.Multiline;
                    if (!defs.senseCase) opts |= RegexOptions.IgnoreCase;
                    ms = Regex.Matches(txt.ToString(), t.s, opts);
                    strt = txt.dot;
                    cnt = 0;
                    foreach (Match m in ms) {
                        cnt++;
                        txt.dot.l = strt.l + m.Index;
                        txt.dot.len = m.Length;
                        if (t.opt == 'g' || cnt == t.n) {
                            if (t.subs != null) Change(DoSubs(m.ToString(), t.subs, t.rep));
                            else Change(t.rep);
                            }
                        }
                    break;
                case 'D':
                    TList ts = null;
                    if (t.fs == null) {
                        if (txt == null) throw new ssException("no current file");
                        else DeleteText(txt, true);
                        }
                    else {
                        for (SList f = t.fs; f != null; f = f.nxt) {
                            ts = FindText(true, f.s, true, false);
                            if (ts == null) throw new ssException("warning: no such file '" + f.s + "'");
                            else {
                                while (ts != null) {
                                    DeleteText(ts.t, true);
                                    ts = ts.nxt;
                                    }
                                }
                            }
                        }
                    break;
                case 'B':
                    if (t.fs != null) AddTexts(SListToArray(t.fs), false);
                    else NewText();
                    break;
                case 'b':
                    bool fnd = false;
                    for (SList f = t.fs; f != null; f = f.nxt) {
                        ts = FindText(false, f.s, true, true);
                        if (ts != null) { fnd = true; txt = ts.t; break; }
                        }
                    if (!fnd) throw new ssException("not in menu: \"" + SListJoin(t.fs) + "\"");
                    MsgLn(txt.MenuLine());
                    WakeUpText(txt);
                    break;
                case 'f':
                    if (t.fs == null) Msg(txt.MenuLine());
                    else Rename(t.fs.s);
                    break;
                case 'e':
                    if (txt == null) throw new ssException("no current file");
                    if (t.fs == null) t.s = txt.Nm;
                    else t.s = t.fs.s;
                    Encoding enc = defs.encoding;
                    string fdta = WinRead(t.s, ref enc);
                    if (fdta != null) {
                        txt.encoding = enc;
                        Rename(t.s);
                        txt.dot.l = 0;
                        txt.dot.r = txt.Length;
                        Change(fdta);
                        txt.dot.To(0);
                        PostEdDot();
                        }
                    break;
                case 'r':
                    if (t.fs == null) t.s = txt.Nm;
                    else t.s = t.fs.s;
                    enc = defs.encoding;
                    fdta = WinRead(t.s, ref enc);
                    if (fdta != null) {
                        txt.encoding = enc;
                        Change(fdta);
                        PostEdDot();
                        }
                    break;
                case 'w':
                    if (txt == null) throw new ssException("no current file");
                    string s;
                    if (t.fs == null) { t.s = txt.Nm; s = txt.FileName(); }
                    else { t.s = t.fs.s; s = t.fs.s; }
                    ssRange r = txt.dot;
                    if (t.a == null) { r.l = 0; r.r = txt.Length; }
                    string dta = txt.ToString(r.l, r.len);
                    string lbl = File.Exists(t.s) ? "" : " (new file) ";
                    if (WinWrite(t.s, dta, txt.encoding)) {
                        if (dta.Length == txt.Length) {
                            txt.TLog.RecordSave();
                            }
                        MsgLn(s + ":" + lbl + "#" + dta.Length.ToString());
                        }
                    PostEdDot();
                    break;
                case 'd':
                    Delete();
                    break;
                case 'k':
                    txt.mark = txt.dot;
                    if (a == null) PostEdDot();
                    break;
                case 'h':
                    ShowHex(txt.ToString());
                    break;
                case '!':
                    Msg(ShellCmd(t.s, null));
                    break;
                case '<':
                    Change(ShellCmd(t.s, null));
                    break;
                case '>':
                    Msg(ShellCmd(t.s, txt.ToString()));
                    break;
                case '|':
                    Change(ShellCmd(t.s, txt.ToString()));
                    break;
                case 'q':
                    //win Remove for non-windowed version
                    log.Frm.UpdateDefs();
                    // */
                    defs.SaveDefs(false);
                    if (DeleteAllTexts()) Environment.Exit(0);
                    break;
                case 'Q':
                    //win Remove for non-windowed version
                    log.Frm.UpdateDefs();
                    // */
                    defs.SaveDefs(true);
                    if (DeleteAllTexts()) Environment.Exit(0);
                    break;
                case 'H':
                    ShowHelp();
                    break;
                case 'u':
                    Undo(t.n);
                    break;
                case '\0':  // end of text
                    break;
                case listHead:   // head of sub lists
                    break;
                case '{':
                    xCmd(t.sub.nxt);  // The head of the sub tree is just another '{', so skip it.
                    break;
                case noCmd:
                    if (a == null) xNoCmd();
                    break;
                case 'T':
                    //win  remove for non-windowed version 
                    txt.Frm.ChangeTab(t.n);
                    //  remove for non-windowed version */
                    break;
                case 'L':
                    //win remove for non-windowed version 
                    txt.ChangeEoln(t.s);
                    //  remove for non-windowed version */
                    break;
                case 'F':
                    //win remove for non-windowed version 
                    txt.FixLineLen(t.n);
                    //  remove for non-windowed version */
                    break;
                case 'C':
                    switch (t.opt) {
                        case 's':
                            defs.senseCase = true;
                            break;
                        case 'i':
                            defs.senseCase = false;
                            break;
                        default:
                            CultureInfo ci = Thread.CurrentThread.CurrentCulture;
                            TextInfo ti = ci.TextInfo;
                            s = txt.ToString();
                            switch (t.opt) {
                                case 'u':
                                    Change(ti.ToUpper(s));
                                    break;
                                case 't':
                                    Change(ti.ToTitleCase(s));
                                    break;
                                case 'l':
                                    Change(ti.ToLower(s));
                                    break;
                                }
                            break;
                        }
                    break;
                case 'E':
                    if (txt == null) {
                        defs.encoding = decodeEncoding(t.opt);
                        //win remove for non-windowing version
                        log.encoding = defs.encoding;
                        //   remove for non-windowing version */
                        }
                    else txt.encoding = decodeEncoding(t.opt);
                    break;
                default:
                    throw new ssException("unknown command");
                }
            if (a != null) PostEdDot();
            if (txt != null && t.nxt != null) txt.dot = levdot;
            xCmd(t.nxt);
            }

        class TList {
            public TList(ssText tt, ssRange r, TList nn) {
                t = tt; nxt = nn; dot = r;
                }
            public ssText t;
            public ssRange dot;
            public TList nxt;
            }

        TList FindText(bool rgx, string pat, bool matching, bool unique) {
            TList l = new TList(null, new ssRange(), null);
            TList lt = l;
            if (pat == null) return new TList(txt, txt.dot, null);
            for (ssText t = txts; t != null; t = t.Nxt) {
                bool m = rgx ? Regex.IsMatch(t.MenuLine(), pat, defs.senseCase ? RegexOptions.None : RegexOptions.IgnoreCase) : pat == t.FileName();
                if (matching && m || !matching && !m) {
                    lt.nxt = new TList(t, t.dot, null);
                    lt = lt.nxt;
                    }
                }
            if (unique && l.nxt != null && l.nxt.nxt != null) throw new ssException("non-unique match for \"" + pat + "\"");
            return l.nxt;
            }

        void CheckRange(ssText t, int i) {
            if (!t.Contains(i)) throw new ssException("address range");
            }

        void CheckRange(ssText t, ssRange r) {
            if (!t.Contains(r)) throw new ssException("address range");
            }

        void CheckTxt(ssText t) {
            if (t == null) throw new ssException("no current file");
            }

        ssText NullIfTxt(ssText t) { return t == txt ? null : t; }

        ssText TxtIfNull(ssText t) { return t == null ? txt : t; }

        ssAddress xAddr(ATree a) {
            if (a == null) return null;
            ssText atxt = txt;
            if (a.fnm != null) {
                TList fs = FindText(true, a.fnm, true, true);
                if (fs == null) throw new ssException("file search");
                atxt = fs.t;
                }
            switch (a.op) {
                case '#':
                    CheckTxt(atxt);
                    CheckRange(atxt, a.n);
                    return new ssAddress(a.n, a.n, atxt);
                case '0':
                    CheckTxt(atxt);
                    ssRange r = atxt.FindLine(0, a.n, 1);
                    return new ssAddress(r, atxt);
                case '.':
                    CheckTxt(atxt);
                    return new ssAddress(atxt.dot, atxt);
                case '\'':
                    CheckTxt(atxt);
                    return new ssAddress(atxt.mark, atxt);
                case '$':
                    CheckTxt(atxt);
                    return new ssAddress(atxt.Length, atxt.Length, atxt);
                case '/':
                    CheckTxt(atxt);
                    return Search(new ssAddress(atxt.dot, atxt), a.s, true);
                case '?':
                    CheckTxt(atxt);
                    return Search(new ssAddress(atxt.dot, atxt), a.s, false);
                case ',':
                case ';':
                    ssAddress al = xAddr(a.l);
                    if (a.op == ';') {
                        txt = al.txt;
                        txt.dot = al.rng;
                        }
                    ssAddress ar = xAddr(a.r);
                    if (al.txt != ar.txt || ar.rng.l < al.rng.l)
                        throw new ssException("addresses out of order");
                    al.rng.r = ar.rng.r;
                    return al;
                case '+':
                case '-':
                    al = xAddr(a.l);
                    al = xRelAddr(al, a.op, a.r);
                    return al;
                default:
                    throw new ssException("address");
                }
            }

        ssAddress xRelAddr(ssAddress rt, char dir, ATree ar) {
            switch (ar.op) {
                case '#':
                    if (dir == '+') rt.rng.To(rt.rng.r + ar.n);
                    else rt.rng.To(rt.rng.l - ar.n);
                    if (!rt.txt.Contains(rt.rng)) throw new ssException("address range");
                    return rt;
                case '/':
                    return Search(rt, ar.s, dir == '+');
                case '?':
                    return Search(rt, ar.s, dir == '-');
                case '0':
                    if (dir == '+')
                        return new ssAddress
                            (rt.txt.FindLine(rt.rng.r, ar.n, 1), rt.txt);
                    else
                        return new ssAddress
                            (rt.txt.FindLine(rt.rng.l, ar.n, -1), rt.txt);
                default:
                    throw new ssException("address");
                }
            }


            private bool compWithCase(char a, char b) {
            if (defs.senseCase) return a == b;
            else return Char.ToLower(a) == Char.ToLower(b);
            }


            public void FindDotNoRegEx(bool forward) {
            int len = txt.dot.len;
            if (forward) {
                int i = txt.dot.r;
                int lim = txt.Length - len;
                for (; ; ) {
                    int j = txt.dot.l;
                    int k = i;
                    while (j < txt.dot.r && k < txt.Length) {
                        if (!compWithCase(txt[j], txt[k])) break;
                        j++;
                        k++;
                        }
                    if (j == txt.dot.r) {
                        txt.dot.l = i;
                        txt.dot.len = len;
                        return;
                        }
                    if (i >= lim)
                        i = 0;
                    else
                        i++;
                    }
                }
            else {
                int i = txt.dot.l - 1;
                int lim = len - 1;
                for (; ; ) {
                    int j = txt.dot.r - 1;
                    int k = i;
                    while (j >= txt.dot.l && k >= 0) {
                        if (!compWithCase(txt[j], txt[k])) break;
                        j--;
                        k--;
                        }
                    if (j < txt.dot.l) {
                        txt.dot.l = i + 1 - len;
                        txt.dot.len = len;
                        return;
                        }
                    if (i <= lim)
                        i = txt.Length - 1;
                    else
                        i--;
                    }
                }
            }

        ssAddress Search(ssAddress rt, string pat, bool forward) {
            Match m;
            Regex rex;
            int start;

            string s = rt.txt.ToString(0, rt.txt.Length);
            bool found = true;
            RegexOptions opts = RegexOptions.Multiline;
            if (!defs.senseCase) opts |= RegexOptions.IgnoreCase;
            if (forward) {
                rex = new Regex(pat, opts);
                start = rt.rng.r;
                m = rex.Match(s, start);
                if (!m.Success) {
                    m = rex.Match(s, 0, rt.txt.Length);
                    if (!m.Success || m.Index > start) found = false;
                    }
                }
            else {
                opts |= RegexOptions.RightToLeft;
                rex = new Regex(pat, opts);
                start = rt.rng.l;
                m = rex.Match(s, 0, start);
                if (!m.Success) {
                    m = rex.Match(s, 0, rt.txt.Length);
                    if (!m.Success || m.Index + m.Length <= start) found = false;
                    }
                }
            if (found) {
                return new ssAddress(m.Index, m.Index + m.Length, rt.txt);
                }
            else throw new ssException("search");
            }


        int IndexToTextLine(int i) {
            int ln = 0;
            for (int j = 0; j <= i; j++)
                if (txt.AtBOLN(j)) ln++;
            return ln;
            }



        string AddressStr() {
            int l = IndexToTextLine(txt.dot.l);
            int r = IndexToTextLine(txt.dot.r > txt.dot.l ? txt.NxtLeft(txt.dot.r) : txt.dot.r);
            string s = l.ToString();
            if (r != l) s += "," + r.ToString();
            s += "; #" + txt.dot.l.ToString();
            if (txt.dot.l != txt.dot.r) s += ",#" + txt.dot.r.ToString();
            return s;

            }

        public void SyncFormToTextAll() {
            for (ssText t = txts; t != null; t = t.Nxt) t.SyncFormToText();
            }

        public void Undo(int n) {
            ssText t;
            //long id = 0;
            //long tid = 0;
            for (; n > 0; n--) {
                //id = 0;
                //for (t = txts; t != null; t = t.Nxt) {
                //    if (t.TLog.Ts != null) {
                //        tid = t.TLog.Ts.id;
                //        if (tid > id) id = tid;
                //        }
                //    }
                for (t = txts; t != null; t = t.Nxt)
                    t.TLog.Undo(curTransId);
                //t.TLog.Undo(id);
                PrevTransId();
                }
            SyncFormToTextAll();
            }

        void xNoCmd() {
            if (txt == null) return;
            if (txt.RangeAligned(txt.dot)) txt.dot = txt.FindLine(txt.dot.r, 1, 1);
            else txt.AlignRange(ref txt.dot);
            PostEdDot();
            Print();
            }

        void Print() {
            Msg(txt.ToString());
            }



        private string ShellCmd(string cmd, string inp) {
            string s = "";
            string tfnm = null;
            try {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = defs.cmd;
                psi.Arguments = defs.cmdArgs;
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.RedirectStandardInput = true;
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                psi.CreateNoWindow = true;
                if (inp != null) {
                    tfnm = Path.GetTempFileName();
                    WinWrite(tfnm, inp, txt.encoding);
                    cmd = string.Format("gc {0} | {1}", tfnm, cmd);
                    }
                Process p = new Process();
                p.StartInfo = psi;
                p.Start();
                p.StandardInput.WriteLine(cmd);
                p.StandardInput.Close();
                p.WaitForExit(1000);
                s = p.StandardOutput.ReadToEnd();
                s += "\r\n" + p.StandardError.ReadToEnd();
                int ec = p.ExitCode;
                p.Close();
                p.Dispose();
                if (inp != null) { File.Delete(tfnm); }
                if (ec != 0) throw new ssException("non-zero exit code: " + p.ExitCode.ToString());
                }
            catch (Exception e) {
                Err("problem shelling command: \r\n" + e.Message);
                }
            //win remove for non-windowed version
            log.Activate();
            // remave for non-windowed version */
            return s;
            }

        void ShowHex(string s) {
            StringBuilder sbh = new StringBuilder();
            StringBuilder sbc = new StringBuilder();
            for (int i = 0; i < s.Length; i++) {
                char c = s[i];
                if (i % 8 == 0) {
                    if (i > 0) {
                        sbc.Insert(0, '"');
                        sbc.Append('"');
                        sbh.Append(sbc.ToString());
                        MsgLn(sbh.ToString());
                        }
                    sbh.Clear();
                    sbc.Clear();
                    }
                sbh.Append(System.Convert.ToUInt16(c).ToString("X4"));
                sbh.Append(' ');
                sbc.Append(char.IsControl(c) ? '.' : c);
                }
            sbc.Insert(0, '"');
            sbc.Append('"');
            sbh.Append(sbc.ToString());
            MsgLn(sbh.ToString());
            }

        void ShowHelp() {
            string[] ss = new string[2];
            ss[0] = defs.exePath + "\\ssHelp.txt";
            ss[1] = defs.exePath + "\\ssManual.txt";
            AddTexts(ss, true);
            }


        Encoding decodeEncoding(char c) {
            switch (c) {
                case 'a': return System.Text.Encoding.ASCII;
                case 'u': return System.Text.Encoding.Unicode;
                case '3': return System.Text.Encoding.UTF32;
                case '8': return System.Text.Encoding.UTF8;
                case '7': return System.Text.Encoding.UTF7;
                }
            return System.Text.Encoding.UTF8;
            }

        public char encodeEncoding(Encoding enc) {
            switch (enc.EncodingName) {
                case "US-ASCII": return 'a';
                case "Unicode": return 'u';
                case "Unicode (UTF-32)": return '3';
                case "Unicode (UTF-8)": return '8';
                case "Unicode (UTF-7)": return '7';
                default: return '8';
                }
            }
        }
    }
