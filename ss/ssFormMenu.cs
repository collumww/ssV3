using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ss {
    public partial class ssForm : Form {

        public void CmdShowMenu() {
            ContextMenu m = BuildMenu();
            m.Show(this, new Point(mouX, mouY), LeftRightAlignment.Right);
            }



        ContextMenu BuildMenu() {
            MenuItem mich = null;
            ContextMenu m = new ContextMenu();

            MenuItem mi = new MenuItem("All");
            mi.Click += new System.EventHandler(MenuSelectAll);
            m.MenuItems.Add(mi);
            m.MenuItems.Add("Cut", new System.EventHandler(MenuCut));
            m.MenuItems.Add("Snarf", new System.EventHandler(MenuSnarf));
            m.MenuItems.Add("Paste", new System.EventHandler(MenuPaste));
            m.MenuItems.Add("Undo", new System.EventHandler(MenuUndo));
            if (txt == ed.Log)
                m.MenuItems.Add("Exec", new System.EventHandler(MenuExec));
            m.MenuItems.Add("Case", new System.EventHandler(MenuToggleIgnoreCase)).Checked = ed.defs.senseCase;

            if (evd.Shift) m.MenuItems.Add("Look", new System.EventHandler(MenuLookBackward));
            else m.MenuItems.Add("Look", new System.EventHandler(MenuLookForward));

            m.MenuItems.Add("/" + Regex.Escape(ed.LastPat), new System.EventHandler(MenuSearch));

            mi = new MenuItem("New");
            mi.BarBreak = true;
            mi.Click += new System.EventHandler(MenuNew);
            m.MenuItems.Add(mi);
            m.MenuItems.Add("Open", new System.EventHandler(MenuOpen));
            m.MenuItems.Add("Close", new System.EventHandler(MenuClose));
            m.MenuItems.Add("Xerox", new System.EventHandler(MenuXerox));
            m.MenuItems.Add("Write", new System.EventHandler(MenuWrite));
            m.MenuItems.Add("Write As...", new System.EventHandler(MenuWriteAs));
            m.MenuItems.Add(ed.Log.FileName(), new System.EventHandler(ed.Log.MenuClick));
            ssText t = ed.Txts;
            while (t != null) {
                m.MenuItems.Add(t.MenuLine(), new System.EventHandler(t.MenuClick));
                t = t.Nxt;
                }

            mi = new MenuItem("Fonts");
            mi.BarBreak = true;
            for (int i = 0; i < layout.fontCnt; i++) {
                mich = new MenuItem(layout.fontNm[i]);
                if (i == layout.fontNum) {
                    mich.Checked = true;
                    mich.Click += new System.EventHandler(MenuSetFont);
                    }
                else {
                    mich.Click += new System.EventHandler(MenuChooseFont);
                    }
                mi.MenuItems.Add(mich);
                }
            m.MenuItems.Add(mi);

            m.MenuItems.Add("Wrap", new System.EventHandler(MenuToggleWrap)).Checked = wrap;
            m.MenuItems.Add("Indent", new System.EventHandler(MenuToggleAutoIndent)).Checked = layout.autoIndent;
            m.MenuItems.Add("Prog", new System.EventHandler(MenuToggleProgramming)).Checked = layout.programming;
            m.MenuItems.Add("Tabs", new System.EventHandler(MenuToggleExpTabs)).Checked = !layout.expTabs;

            mi = new MenuItem("Events");
            evd.InitEventSetEnum();
            while (true) {
                string es = evd.NextEventSet();
                if (es == null) break;
                mich = new MenuItem(es);
                mich.Click += new System.EventHandler(MenuSetEventSet);
                mich.Checked = es == layout.eventset;
                mi.MenuItems.Add(mich);
                }
            m.MenuItems.Add(mi);

            mi = new MenuItem("Encoding");
            mich = new MenuItem(Encoding.ASCII.EncodingName);
            mich.Checked = txt.encoding.EncodingName == mich.Text;
            mich.Click += new System.EventHandler(MenuSetEncoding);
            mi.MenuItems.Add(mich);
            mich = new MenuItem(Encoding.Unicode.EncodingName);
            mich.Checked = txt.encoding.EncodingName == mich.Text;
            mich.Click += new System.EventHandler(MenuSetEncoding);
            mi.MenuItems.Add(mich);
            mich = new MenuItem(Encoding.UTF32.EncodingName);
            mich.Checked = txt.encoding.EncodingName == mich.Text;
            mich.Click += new System.EventHandler(MenuSetEncoding);
            mi.MenuItems.Add(mich);
            mich = new MenuItem(Encoding.UTF8.EncodingName);
            mich.Checked = txt.encoding.EncodingName == mich.Text;
            mich.Click += new System.EventHandler(MenuSetEncoding);
            mi.MenuItems.Add(mich);
            mich = new MenuItem(Encoding.UTF7.EncodingName);
            mich.Checked = txt.encoding.EncodingName == mich.Text;
            mich.Click += new System.EventHandler(MenuSetEncoding);
            mi.MenuItems.Add(mich);

            m.MenuItems.Add(mi);

            return m;
            }


        void MenuSelectAll(Object sender, EventArgs e) {
            CmdSelectAll();
            }

        void MenuCut(Object sender, EventArgs e) {
            CmdCut();
            }

        void MenuSnarf(Object sender, EventArgs e) {
            CmdSnarf();
            }

        void MenuPaste(Object sender, EventArgs e) {
            CmdPaste();
            }

        void MenuUndo(Object sender, EventArgs e) {
            CmdUndo();
            }

        void MenuLookForward(Object sender, EventArgs e) {
            InvalidateCursor();
            ed.FindDotNoRegEx(true);
            txt.SyncFormToText();
            InvalidateCursor();
            if (e != null) evd.Reset();
            }

        void MenuLookBackward(Object sender, EventArgs e) {
            InvalidateCursor();
            ed.FindDotNoRegEx(false);
            txt.SyncFormToText();
            InvalidateCursor();
            if (e != null) evd.Reset();
            }

        void MenuExec(Object sender, EventArgs e) {
            if (cursor.Empty) {
                InvalidateMarks();
                ssRange r = cursor.rng;
                cursor.To(txt.AlignRange(ref r));
                txt.dot = cursor.rng;
                }
            string[] cmds = txt.ToString().Split(new string[] { txt.Eoln }, StringSplitOptions.None);
            foreach (string cmd in cmds) {
                if (cmd != "") {
                    ed.MsgLn(cmd);
                    ed.Do(cmd);
                    }
                }
            }

        void MenuSearch(Object sender, EventArgs e) {
            ed.Do("/");
            }



        void MenuNew(Object sender, EventArgs e) {
            CmdNew();
            }

        void MenuOpen(Object sender, EventArgs e) {
            CmdOpen();
            }

        void MenuClose(Object sender, EventArgs e) {
            Close();
            }


        void MenuXerox(Object sender, EventArgs e) {
            CmdXerox();
            }

        void MenuWrite(Object sender, EventArgs e) {
            CmdSave();
            }

        void MenuWriteAs(Object sender, EventArgs e) {
            CmdSaveAs();
            }

        void MenuSetFont(Object sender, EventArgs e) {
            CmdSetFont();
            }

        void MenuChooseFont(Object sender, EventArgs e) {
            ChooseFont(((MenuItem)sender).Index);
            }

        void MenuToggleWrap(Object sender, EventArgs e) {
            CmdToggleWrap();
            }

        void MenuToggleAutoIndent(Object sender, EventArgs e) {
            CmdToggleAutoIndent();
            }

        void MenuToggleProgramming(Object sender, EventArgs e) {
            CmdToggleProgramming();
            }

        void MenuToggleExpTabs(Object sender, EventArgs e) {
            CmdToggleExpTabs();
            }

        void MenuToggleIgnoreCase(Object sender, EventArgs e) {
            CmdToggleIgnoreCase();
            }


        void MenuSetEventSet(Object sender, EventArgs e) {
            string s = ((MenuItem)sender).Text;
            layout.eventset = s;
            evd.SetEventSet(s);
            }

        void MenuSetEncoding(Object sender, EventArgs e) {
            int i = ((MenuItem)sender).Index;
            Encoding enc = null;
            switch(i) {
                case 0: enc = Encoding.ASCII; break;
                case 1: enc = Encoding.Unicode; break;
                case 2: enc = Encoding.UTF32; break;
                case 3: enc = Encoding.UTF8; break;
                case 4: enc = Encoding.UTF7; break;
                }
            if (txt == ed.Log) {
                ed.defs.encoding = enc;
                ed.Log.encoding = enc;
                }
            else {
                txt.encoding = enc;
                }
            }


        }
    }