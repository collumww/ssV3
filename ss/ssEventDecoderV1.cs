using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using System.IO;

namespace ss {
    public class ssEventDecoderV1 {
        public ssEventDecoderV1(ssForm f) {
            frm = f;
            evFile = "ssEvents.ini";
            keyconv = new KeysConverter();
            methods = Type.GetType("ss.ssForm").GetMethods(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            actiontyp = Type.GetType("ss.ssAction");

            events = LoadEvents(f, evFile);
            ematch = events;
            ematchcnt = 0;

            mods = new ssEvent();
            modstail = mods;

            elog = new ssEvent();
            elogtail = elog;
            elogcnt = 0;
            logging = false;

            prvmod = Keys.None;

            defInsert = pAction("InsertChar");

            enabled = true;
            }


        public void SetKeyModifier(Keys k, bool val) {
            switch (k) {
                case Keys.ShiftKey: mShift = val; break;
                case Keys.ControlKey: mCtl = val; break;
                case Keys.Menu: mAlt = val; break;
                }
            }

        public void SetMouModifier(MouseButtons k, bool val) {
            switch (k) {
                case MouseButtons.Left: mMouLeft = val; break;
                case MouseButtons.Middle: mMouMiddle = val; break;
                case MouseButtons.Right: mMouRight = val; break;
                case MouseButtons.XButton1: mMouX1 = val; break;
                case MouseButtons.XButton2: mMouX2 = val; break;
                }
            }

        public bool ShouldIgnoreKeyPress() {
            return mCtl ||
                   mMouLeft ||
                   mMouMiddle ||
                   mMouRight ||
                   mMouX1 ||
                   mMouX2;
            }

        //public bool ShouldIgnoreKeyDown(Keys k) {
        //    return k == Keys.ShiftKey && mShift ||
        //           k == Keys.ControlKey && mCtl ||
        //           k == Keys.Alt && mAlt;
        //    }

        public bool MouseModifierDown() {
            return mMouLeft ||
                   mMouMiddle ||
                   mMouRight ||
                   mMouX1 ||
                   mMouX2;
            }


        public bool Enabled() {
            return enabled;
            }


        public void Eat(Keys k, char c, ssEventType t) {
            if (!enabled && !MouseModifierDown()) {
                enabled = true;
                }
            if (logging) {
                if (k == Keys.Home && t == ssEventType.down) {
                    ShowEvents("--- Event log ------------------------", elog.nxt);
                    elogcnt = 0;
                    elog.nxt = null;
                    elogtail = elog;
                    ematchcnt = 0;
                    }
                else {
                    elogcnt++;
                    elogtail.nxt = new ssEvent(k, c, t, null, false);
                    elogtail = elogtail.nxt;
                    }
                }
            EatModifiers(k, t);
            if (!Matching() && HaveModifiers() && !IsModifier(k) && t == ssEventType.down)
                MatchModifiers();      // When you hold down a modifier and press a key again, like ctrl-v, v, v, etc.
            Match(k, c, t);
            }


        public void Reset() {
            ematch = events;
            modstail = mods;
            prvmod = Keys.None;
            ClearModifiers();
            }




        public void Pause() {
            }



        //---- Private things below ---------------------------------------------------------

        ssEvent events;        // Defined event chains
        ssEvent ematch;        // The last event matched in the current chain.
        int ematchcnt;
        ssForm frm;

        ssEvent elog;
        ssEvent elogtail;
        int elogcnt;
        bool logging;

        ssEvent mods;            // Current modifiers
        ssEvent modstail;         // Last modifier keyed

        bool mShift, mCtl, mAlt, mMouLeft, mMouMiddle, mMouRight, mMouX1, mMouX2;
        bool enabled;

        Keys prvmod;

        MethodInfo defInsert;

        string evFile;
        KeysConverter keyconv;
        MethodInfo[] methods;
        Type actiontyp;


        private void ShowEvents(string msg, ssEvent e) {
            frm.Ed.MsgLn(msg);
            for (ssEvent el = e; el != null; el = el.nxt) {
                frm.Ed.MsgLn(el.ToString());
                }

            }


        private void ClearModifiers() {
            mShift = mCtl = mAlt = mMouLeft = mMouMiddle = mMouRight = mMouX1 = mMouX2 = false;
            }


        private void EatModifiers(Keys k, ssEventType t) {
            if (IsModifier(k)) {
                if (t == ssEventType.down) {
                    if (k == prvmod) return;
                    modstail.nxt = new ssEvent(k, '\0', t, null, false);
                    modstail = modstail.nxt;
                    prvmod = k;
                    }
                else if (t == ssEventType.up) {
                    ssEvent x = mods;
                    while (x.nxt != null) {
                        if (x.nxt.k == k) x.nxt = x.nxt.nxt;
                        else x = x.nxt;
                        }
                    modstail = x;
                    }
                }
            }


        private bool IsModifier(Keys k) {
            return k == Keys.ShiftKey
                || k == Keys.ControlKey
                || k == Keys.Menu
                || k == Keys.LButton
                || k == Keys.MButton
                || k == Keys.RButton
                || k == Keys.XButton1
                || k == Keys.XButton2;
            }


        private bool HaveModifiers() {
            return modstail != mods;
            }


        private void MatchModifiers() {
            for (ssEvent e = mods.nxt; e != null; e = e.nxt) {
                Match(e.k, e.c, e.t);
                }
            }


        private void ResetMatch() {
            ematch = events;
            prvmod = Keys.None;
            }


        private bool Matching() {
            return ematch != events;     // True if I'm in the middle of trying to match something.
            }


        private bool SingleMatch(ssEvent e, Keys k, char c, ssEventType t) {
            return e.t == t && ((t != ssEventType.press && e.k == k) || (t == ssEventType.press && e.c == c));
            }

        private void Match(Keys k, char c, ssEventType t) {
            ssEvent e = ematch.nxt;
            if (ematch.t == t && t == ssEventType.move) e = ematch;

            while (e != null) {
                if (SingleMatch(e, k, c, t)) {
                    break;
                    }
                e = e.alt;
                }

            if (e == null) {
                if (t == ssEventType.press) defInsert.Invoke(frm, null);
                ResetMatch();
                }
            else {
                ematch = e;
                if (e.a != null) {
                    ematchcnt++;
                    if (enabled) e.a.Invoke(frm, null);
                    if (logging) {
                        ShowEvents("--- Modifiers ------------------------", mods.nxt);
                        }
                    if (!e.cont) {
                        enabled = false;
                        ResetMatch();
                        }
                    }
                }
            }



        char pChar(string s) {
            if (s.Length == 0) return '\0';
            if (char.IsDigit(s[0])) {
                int x = Int32.Parse(s);
                return (char)x;
                }
            if (s.Length == 1) return char.Parse(s);
            throw new ssException("error parsing event field 'char' in events file");
            }

        

        ssEventType pType(string s) {
            switch (s) {
                case "down":
                    return ssEventType.down;
                case "up":
                    return ssEventType.up;
                case "press":
                    return ssEventType.press;
                case "move":
                    return ssEventType.move;
                case "turn":
                    return ssEventType.turn;
                default:
                    throw new ssException("error parsing event field 'type' in events file");
                }
            }


        MethodInfo pAction(string s) {
            if (s == "null") return null;
            s = "Cmd" + s;
            foreach (MethodInfo m in methods) {
                if (m.Name == s) return m;  //(ssAction)m.CreateDelegate(actiontyp, f);
                }
            throw new ssException("error parsing event field 'action' in events file");
            }



        ssEvent pEvent(ssScanner scnr, ssForm f) {
            ssEvent e = new ssEvent();
            try {
                scnr.GetStrSpDelim(); e.k = (Keys)keyconv.ConvertFromString(scnr.S);
                scnr.GetStrSpDelim(); e.t = pType(scnr.S);
                if (e.t == ssEventType.press) {
                    scnr.GetStrSpDelim(); e.c = pChar(scnr.S);
                    }
                if (scnr.C != '|' && scnr.C != '+' && scnr.C != ';') {
                    scnr.GetStrSpDelim(); e.a = pAction(scnr.S);
                    }
                if (scnr.C != '|' && scnr.C != '+' && scnr.C != ';') {
                    scnr.GetStrSpDelim(); e.cont = bool.Parse(scnr.S);
                    }
                }
            catch (Exception ee) {
                throw new ssException("error parsing event in events file" + "\r\n?" + ee.Message);
                }
            return e;
            }



        ssEvent pEventTree(ssScanner scnr, ssForm f) {
            ssEvent rt = pEvent(scnr, f);
            ssEvent e = rt;

            for (;;) {
                switch (scnr.C) {
                    case '|':
                        scnr.GetChar();
                        e.alt = pEvent(scnr, f);
                        e = e.alt;
                        break;
                    case '+':
                        scnr.GetChar();
                        e.nxt = pEventTree(scnr, f);
                        break;
                    case ';':
                        scnr.GetChar();
                        return rt;
                    case '\0':
                        return rt;
                    default:
                        if (char.IsWhiteSpace(scnr.C)) {
                            scnr.GetChar();
                            }
                        else throw new ssException("error parsing event tree in events file");
                        break;
                    }
                }
            }




        private ssEvent LoadEvents(ssForm f, string fnm) {
            ssEvent es = new ssEvent();
            fnm = System.IO.Path.GetDirectoryName(Application.ExecutablePath) +
                System.IO.Path.DirectorySeparatorChar + fnm;

            int cnt = 0;
            ssScanner scnr = null;
            while (cnt < 2 && scnr == null){
                try {
                    cnt++;
                    scnr = new ssScanner(File.ReadAllText(fnm), true);
                    scnr.GetChar();
                    }
                catch {
                    try {
                        if (cnt < 2) {
                            f.Ed.MsgLn("creating default events file");
                            File.WriteAllText(fnm, defEvents);
                            }
                        else {
                            throw new ssException("no events file");
                            }
                        }
                    catch (Exception ee) {
                        f.Ed.Err("error writing default events file" + "\r\n" + ee.Message);
                        }
                    }
                }

            try {
                es.nxt = pEventTree(scnr, f);
                }
            catch (Exception e) {
                if (f == f.Ed.Log.Frm) f.Ed.Err(e.Message + ": " + scnr.S); // Guard this so you don't get messages for every form created.
                }
            return es;
            }

        // Defaults for event handling

        private const string defEvents =
@"# Key Character type (up down press move turn) action continue to match
  Menu down
    + ShiftKey down
        + Left down MoveCharLeft
        | Right down MoveCharRight
        ;
    | Home down CursorBOLine
    | End down CursorEOLine
    | Left down MoveWordLeft
    | Right down MoveWordRight
    | Up down MoveLinesUp
    | Down down MoveLinesDown
    ;

| Back down Back
| Tab down Tab
| Enter down Enter
| Delete down Delete
| Left down CursorNextWordLeft 
| Right down CursorNextWordRight 
| Up down CursorUp
| Down down CursorDown 
| PageUp down TextDownPage
| PageDown down TextUpPage
| Home down CursorBOParagraph
| End down CursorEOParagraph
| Escape down ToggleToCommand

| ShiftKey down SelExtOn true
    + ShiftKey up SelExtOff
    | Up down CursorUp
    | Down down CursorDown
    | Left down CursorNextWordLeft  
    | Right down CursorNextWordRight 
    | Home down CursorBOParagraph
    | End down CursorEOParagraph
    | Tab down ShiftTab
    | Menu down
        + Home down CursorBOLine
        | End down CursorEOLine
        ;
    | ControlKey down
        + Up down CursorUp
        | Down down CursorDown
        | Left down CursorLeft  
        | Right down CursorRight 
        | Home down FileBeg
        | End down FileEnd
        | S down SaveAs
        ;
    ;

| ShiftKey up SelExtOff

| ControlKey down  
    + S down Save
    | O down Open
    | N down New
    | A down SelectAll
    | T down SetFont
    | X down Cut
    | C down Snarf
    | V down Paste
    | Z down Undo
    | G down Xerox
    | W down ToggleWrap
	| Up down TextDown1 
	| Down down TextUp1 
    | Left down CursorLeft
    | Right down CursorRight
    | Home down FileBeg
    | End down FileEnd
    | LButton down CursorWordLeft
    | RButton down CursorWordRight
    | I down ShowInternals
    | Delete down DeleteWord
    | Back down BackDeleteWord
	;

| LButton down
	+ E down MarkFromCursor   
    | U down CopyMarkToCursor 
    | O down MoveMarkToCursor 
    | A down MatchBraces true
    | OemPeriod down CopyCursorBeforeMark 
    | P down CopyCursorAfterMark 
    | J down ToMarkLeft 
    | K down ToMarkRight 
    | I down ShowMark 
    | Y down ToggleCrossHairs 
    | RButton down Cut
    ;

| RButton down
    + RButton up ShowMenu
    | LButton down Paste
    ;
;
";
        }
    }
