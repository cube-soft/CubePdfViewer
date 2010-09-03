﻿/* ------------------------------------------------------------------------- */
/*
 *  MainForm.cs
 *
 *  Copyright (c) 2010 CubeSoft Inc. All rights reserved.
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see < http://www.gnu.org/licenses/ >.
 *
 *  Last-modified: Wed 01 Sep 2010 00:10:00 JST
 */
/* ------------------------------------------------------------------------- */
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Cube {
    /* --------------------------------------------------------------------- */
    ///
    /// MainForm
    /// 
    /// <summary>
    /// NOTE: PDFViewer ではファイルをロードしている間，「リサイズ」，
    /// 「フォームを閉じる」，「各種マウスイベント」を無効化している．
    /// ただ，PDFViewer はこの処理が原因で異常終了するケースが散見される
    /// ため，CubePDF Viewer ではこの処理は保留する．
    /// 
    /// また，現在は使用していないが，PDFLoadBegin, PDFLoadCompleted
    /// イベントが用意されてある．
    /// ファイルのロード時間がやや長いので，この辺りのイベントに適切な
    /// ハンドラを指定する必要があるか．
    /// 追記: PDFLoad() よりは，その後の RenderPage() メソッドの方に
    /// 大きく時間を食われている模様．そのため，これらのイベントは
    /// あまり気にしなくて良い．
    /// 
    /// RenderFinished の他に RenderNotifyFinished と言うイベントも存在
    /// する．現状では，どのような条件でこのイベントが発生するのかは不明．
    /// </summary>
    /// 
    /* --------------------------------------------------------------------- */
    public partial class MainForm : Form {
        /* ----------------------------------------------------------------- */
        /// Constructor
        /* ----------------------------------------------------------------- */
        public MainForm() {
            InitializeComponent();

            int x = Screen.PrimaryScreen.Bounds.Height - 100;
            this.Size = new Size(System.Math.Max(x, 800), x);
            this.NavigationSplitContainer.Panel1Collapsed = true;
            this.MenuSplitContainer.SplitterDistance = this.MenuToolStrip.Height;
            this.SubMenuSplitContainer.SplitterDistance = this.SubMenuToolStrip.Width;
            this.DefaultTabPage.VerticalScroll.SmallChange = 3;
            this.DefaultTabPage.HorizontalScroll.SmallChange = 3;
            this.FitToHeightButton.Checked = true;
            CreateTabContextMenu(this.PageViewerTabControl);

            this.MouseEnter += new EventHandler(this.MainForm_MouseEnter);
            this.MouseWheel += new MouseEventHandler(this.MainForm_MouseWheel);
        }

        /* ----------------------------------------------------------------- */
        /// UpdateFitCondtion
        /* ----------------------------------------------------------------- */
        private void UpdateFitCondition(FitCondition which) {
            fit_ = which;
            this.FitToWidthButton.Checked = ((fit_ & FitCondition.Width) != 0);
            this.FitToHeightButton.Checked = ((fit_ & FitCondition.Height) != 0);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Refresh
        /// 
        /// <summary>
        /// システムの Refresh() を呼ぶ前に，必要な情報を全て更新する．
        /// MEMO: サムネイル画面を更新するとちらつきがひどいので，
        /// 最小限の更新になるようにしている．
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private void Refresh(PictureBox canvas, string message = "") {
            if (canvas == null || canvas.Tag == null) {
                CurrentPageTextBox.Text = "0";
                TotalPageLabel.Text = "/ 0";
                ZoomDropDownButton.Text = "100%";
            }
            else {
                var core = (PDFLibNet.PDFWrapper)canvas.Tag;
                CurrentPageTextBox.Text = core.CurrentPage.ToString();
                TotalPageLabel.Text = "/ " + core.PageCount.ToString();
                ZoomDropDownButton.Text = ((int)core.Zoom).ToString() + "%";
                if (this.PageViewerTabControl != null) this.PageViewerTabControl.Refresh();
            }

            if (this.MainMenuStrip != null) this.MainMenuStrip.Refresh();
            if (this.FooterStatusStrip != null) this.FooterStatusStrip.Refresh();

            // scrollbarのsmallchangeの更新
            var vsb = this.PageViewerTabControl.SelectedTab.VerticalScroll;
            var hsb = this.PageViewerTabControl.SelectedTab.HorizontalScroll;
            // Minimumは0と仮定
            vsb.SmallChange = (vsb.Maximum - vsb.LargeChange) / 20;
            hsb.SmallChange = (hsb.Maximum - hsb.LargeChange) / 20;
        }

        /* ----------------------------------------------------------------- */
        /// Open
        /* ----------------------------------------------------------------- */
        private void Open(TabPage tab, string path, string password = "") {
            var canvas = CanvasPolicy.Create(tab);

            try {
                CanvasPolicy.Open(canvas, path, password, fit_);
                this.CreateThumbnail(canvas);
            }
            catch (System.Security.SecurityException /* err */) {
                PasswordDialog dialog = new PasswordDialog(path);
                dialog.ShowDialog();
                if (dialog.Password.Length > 0) this.Open(tab, path, dialog.Password);
            }
            catch (Exception /* err */) { }
            finally {
                this.Refresh(canvas);
            }
        }

        /* ----------------------------------------------------------------- */
        /// Search
        /* ----------------------------------------------------------------- */
        private void Search(TabPage tab, string text, bool next) {
            var canvas = CanvasPolicy.Get(tab);

            try {
                var args = new SearchArgs(text);
                args.FromBegin = begin_;
                args.IgnoreCase = true;
                args.WholeDocument = true;
                args.WholeWord = false;
                args.FindNext = next;

                var result = CanvasPolicy.Search(canvas, args);
                begin_ = !result; // 最後まで検索したら始めに戻る
            }
            catch (Exception /* err */) { }
            finally {
                this.Refresh(canvas);
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// ResetSearch
        /// 
        /// <summary>
        /// MEMO: ライブラリが，検索結果を描画する状態を解除する方法を
        /// 持っていないため，空の文字列で検索してリセットする．
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private void ResetSearch(TabPage tab) {
            var canvas = CanvasPolicy.Get(tab);

            try {
                var dummy = new SearchArgs();
                CanvasPolicy.Search(canvas, dummy);
            }
            catch (Exception /* err */) { }
            finally {
                begin_ = true;
                this.Refresh(canvas);
            }
        }

        /* ----------------------------------------------------------------- */
        /// Adjust
        /* ----------------------------------------------------------------- */
        private void Adjust(TabPage tab) {
            var canvas = CanvasPolicy.Get(tab);

            try {
                if (this.FitToWidthButton.Checked) CanvasPolicy.FitToWidth(canvas);
                else if (this.FitToHeightButton.Checked) CanvasPolicy.FitToHeight(canvas);
                else CanvasPolicy.Adjust(canvas);
            }
            catch (Exception /* err */) { }
            finally {
                this.Refresh(canvas);
            }
        }

        /* ----------------------------------------------------------------- */
        /// CreateTab
        /* ----------------------------------------------------------------- */
        public TabPage CreateTab(TabControl parent) {
            var tab = new TabPage();

            // TabPage の設定
            tab.AutoScroll = true;
            tab.VerticalScroll.SmallChange = (tab.VerticalScroll.Maximum - tab.VerticalScroll.Minimum) / 20;
            tab.HorizontalScroll.SmallChange = (tab.HorizontalScroll.Maximum - tab.HorizontalScroll.Minimum) / 20;
            tab.BackColor = Color.DimGray;
            tab.BorderStyle = BorderStyle.Fixed3D;
            tab.ContextMenuStrip = new ContextMenuStrip();
            tab.Text = "(無題)";

            parent.Controls.Add(tab);
            parent.SelectedIndex = parent.TabCount - 1;

            return tab;
        }

        /* ----------------------------------------------------------------- */
        /// DestroyTab
        /* ----------------------------------------------------------------- */
        public void DestroyTab(TabPage tab) {
            var parent = (TabControl)tab.Parent;
            var canvas = CanvasPolicy.Get(tab);
            var thumb = CanvasPolicy.GetThumbnail(this.NavigationSplitContainer.Panel1);
            if (thumb != null) CanvasPolicy.DestroyThumbnail(thumb);
            CanvasPolicy.Destroy(canvas);
            if (this.PageViewerTabControl.TabCount > 1) parent.TabPages.Remove(tab);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// CreateTabContextMenu
        ///
        /// <summary>
        /// コンテキストメニューを設定する．
        /// TODO: コンテキストメニューから登録元である TabControl の
        /// オブジェクトへ辿る方法の調査．現状では，暫定的にコンテキスト
        /// メニューの Tag に TabControl のオブジェクトを設定しておく
        /// 事で対処している．
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public void CreateTabContextMenu(TabControl parent) {
            var menu = new ContextMenuStrip();
            var elem = new ToolStripMenuItem();
            elem.Text = "閉じる";
            elem.Click += new EventHandler(TabClosed);
            menu.Items.Add(elem);
            parent.MouseDown += new MouseEventHandler(ContextMenu_MouseDown);
            parent.ContextMenuStrip = menu;

            foreach (TabPage child in parent.TabPages) {
                child.ContextMenuStrip = new ContextMenuStrip();
            }
        }

        /* ----------------------------------------------------------------- */
        /// CreateThumbnail
        /* ----------------------------------------------------------------- */
        private void CreateThumbnail(PictureBox canvas) {
            var old = CanvasPolicy.GetThumbnail(this.NavigationSplitContainer.Panel1);
            if (old != null) CanvasPolicy.DestroyThumbnail(old);
            ListView thumb = CanvasPolicy.CreateThumbnail(canvas, this.NavigationSplitContainer.Panel1, RenderThumbnailFinished);
            thumb.SelectedIndexChanged += new EventHandler(PageChanged);
        }

        /* ----------------------------------------------------------------- */
        //  キーボード・ショートカット一覧
        /* ----------------------------------------------------------------- */
        #region Keybord shortcuts

        /* ----------------------------------------------------------------- */
        ///
        /// MainForm_KeyDown
        ///
        /// <summary>
        /// キーボード・ショートカット一覧．
        /// KeyPreview を有効にして，全てのキーボードイベントを一括で
        /// 処理している．
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private void MainForm_KeyDown(object sender, KeyEventArgs e) {
            switch (e.KeyCode) {
            case Keys.Enter:
                if (this.SearchTextBox.Focused && this.SearchTextBox.Text.Length > 0) {
                    this.SearchButton_Click(this.SearchButton, e);
                }
                break;
            case Keys.Escape:
                this.ResetSearch(this.PageViewerTabControl.SelectedTab);
                break;
            case Keys.Right:
            case Keys.Down:
                if (e.Control) this.ZoomInButton_Click(this.ZoomInButton, e);
                else this.NextPageButton_Click(this.NextPageButton, e);
                break;
            case Keys.Left:
            case Keys.Up:
                if (e.Control) this.ZoomOutButton_Click(this.ZoomOutButton, e);
                else this.PreviousPageButton_Click(this.PreviousPageButton, e);
                break;
            case Keys.F3: // 検索
                if (this.SearchTextBox.Text.Length > 0) this.Search(this.PageViewerTabControl.SelectedTab, this.SearchTextBox.Text, !e.Shift);
                break;
            case Keys.F4: // 閉じる
                if (e.Control) this.DestroyTab(this.PageViewerTabControl.SelectedTab);
                break;
            case Keys.F:  // 検索ボックスにフォーカス
                if (e.Control) this.SearchTextBox.Focus();
                break;
            case Keys.M:  // メニューの表示/非表示
                if (e.Control) this.MenuModeButton_Click(this.MenuModeButton, e);
                break;
            case Keys.N:  // 新規タブ
                if (e.Control) this.CreateTab(this.PageViewerTabControl);
                break;
            case Keys.O:  // ファイルを開く
                if (e.Control) this.OpenButton_Click(this.PageViewerTabControl.SelectedTab, e);
                break;
            case Keys.W:  // ファイルを閉じる
                if (e.Control) this.CloseButton_Click(this.PageViewerTabControl.SelectedTab, e);
                break;
            default:
                break;
            }
        }

        #endregion

        /* ----------------------------------------------------------------- */
        //  メインフォームに関する各種イベント・ハンドラ
        /* ----------------------------------------------------------------- */
        #region MainForm Event handlers

        /* ----------------------------------------------------------------- */
        /// MainForm_SizeChanged
        /* ----------------------------------------------------------------- */
        private void MainForm_SizeChanged(object sender, EventArgs e) {
            this.Adjust(this.PageViewerTabControl.SelectedTab);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// MainForm_MouseWheel
        /// 
        /// <summary>
        /// マウスホイールによるスクロールの処理．
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private int mouseWheelPrevCount = 0;
        private int mouseWheelNextCount = 0;
        private void MainForm_MouseWheel(object sender, MouseEventArgs e) {
            var tab = this.PageViewerTabControl.SelectedTab;
            var scroll = tab.VerticalScroll;
            if (!scroll.Visible) return;
            
            var realMaximum = 1 + scroll.Maximum - scroll.LargeChange; // ユーザのコントロールで取れるscroll.Valueの最大値
            int delta = -(e.Delta / 120) * scroll.SmallChange;
            if (scroll.Value == scroll.Minimum && delta < 0)
            {
                if (mouseWheelPrevCount > 3) {
                    if (PreviousPage())
                    {
                        tab.AutoScrollPosition = new Point(0, 0);
                    }
                    mouseWheelPrevCount = 0;
                }
                else mouseWheelPrevCount++;
            }
            else if (scroll.Value == realMaximum && delta > 0)
            {
                if (mouseWheelNextCount > 3) {
                    if (NextPage())
                    {
                        tab.AutoScrollPosition = new Point(0, 0);
                    }
                    mouseWheelNextCount = 0; 
                } else mouseWheelNextCount++;
            } 
            else  if (scroll.Value >= scroll.Minimum &&
                scroll.Value <= realMaximum) 
            {
                scroll.Value = Between(scroll.Minimum, scroll.Value + delta, realMaximum);
                mouseWheelNextCount = 0; mouseWheelPrevCount = 0;
            }
            
        }

        private int Between(int min, int value, int max)
        {
            if (value < min) return min;
            else if (value > max) return max;
            else return value;
        }
        /* ----------------------------------------------------------------- */
        /// MainForm_MouseEnter
        /* ----------------------------------------------------------------- */
        private void MainForm_MouseEnter(object sender, EventArgs e) {
            this.Focus();
        }

        #endregion

        /* ----------------------------------------------------------------- */
        //  メインフォームに登録している各種コントロールのイベントハンドラ
        /* ----------------------------------------------------------------- */
        #region Other controls event handlers

        /* ----------------------------------------------------------------- */
        /// FileButton_DropDownItemClicked
        /* ----------------------------------------------------------------- */
        private void FileButton_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            var control = (ToolStripSplitButton)sender;
            control.HideDropDown();
            if (e.ClickedItem.Name == "CloseMenuItem") {
                CloseButton_Click(sender, e);
                return;
            }

            if (e.ClickedItem.Name == "OpenNewTabMenuItem") {
                TabPage selected = null;
                foreach (TabPage child in this.PageViewerTabControl.TabPages) {
                    if (child.Controls["Canvas"] == null) { // 未使用タブ
                        selected = child;
                        child.Select();
                        break;
                    }
                }
                if (selected == null) CreateTab(this.PageViewerTabControl);
            }
            this.OpenButton_Click(sender, e);
        }

        /* ----------------------------------------------------------------- */
        /// OpenButton_Click
        /* ----------------------------------------------------------------- */
        private void OpenButton_Click(object sender, EventArgs e) {
            var dialog = new OpenFileDialog();
            dialog.Filter = "PDF ファイル(*.pdf)|*.pdf";
            if (dialog.ShowDialog() == DialogResult.OK) {
                var tab = this.PageViewerTabControl.SelectedTab;
                this.Open(tab, dialog.FileName);
            }
        }

        /* ----------------------------------------------------------------- */
        /// CloseButton_Click
        /* ----------------------------------------------------------------- */
        private void CloseButton_Click(object sender, EventArgs e) {
            var tab = this.PageViewerTabControl.SelectedTab;
            this.DestroyTab(tab);
        }

        /* ----------------------------------------------------------------- */
        /// PageViewerTabControl_SelectedIndexChanged
        /* ----------------------------------------------------------------- */
        private void PageViewerTabControl_SelectedIndexChanged(object sender, EventArgs e) {
            var control = (TabControl)sender;
            var canvas = CanvasPolicy.Get(control.SelectedTab);
            if (canvas == null) return;

            this.CreateThumbnail(canvas);
            CanvasPolicy.Adjust(canvas);
            this.Refresh(canvas);
        }

        /* ----------------------------------------------------------------- */
        /// ZoomInButton_Click
        /* ----------------------------------------------------------------- */
        private void ZoomInButton_Click(object sender, EventArgs e) {
            this.UpdateFitCondition(FitCondition.None);
            var canvas = CanvasPolicy.Get(this.PageViewerTabControl.SelectedTab);
            if (canvas == null) return;

            try {
                CanvasPolicy.ZoomIn(canvas);
            }
            catch (Exception /* err */) { }
            finally {
                this.Refresh(canvas);
            }
        }

        /* ----------------------------------------------------------------- */
        /// ZoomOutButton_Click
        /* ----------------------------------------------------------------- */
        private void ZoomOutButton_Click(object sender, EventArgs e) {
            this.UpdateFitCondition(FitCondition.None);
            var canvas = CanvasPolicy.Get(this.PageViewerTabControl.SelectedTab);
            if (canvas == null) return;

            try {
                CanvasPolicy.ZoomOut(canvas);
            }
            catch (Exception /* err */) { }
            finally {
                this.Refresh(canvas);
            }
        }

        /* ----------------------------------------------------------------- */
        /// ZoomDropDownButton_DropDownItemClicked
        /* ----------------------------------------------------------------- */
        private void ZoomDropDownButton_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e) {
            this.UpdateFitCondition(FitCondition.None);
            var canvas = CanvasPolicy.Get(this.PageViewerTabControl.SelectedTab);
            if (canvas == null) return;

            try {
                var zoom = e.ClickedItem.Text.Replace("%", "");
                CanvasPolicy.Zoom(canvas, int.Parse(zoom));
            }
            catch (Exception /* err */) { }
            finally {
                this.Refresh(canvas);
            }
        }

        /* ----------------------------------------------------------------- */
        /// FitToWidthButton_Click
        /* ----------------------------------------------------------------- */
        private void FitToWidthButton_Click(object sender, EventArgs e) {
            this.UpdateFitCondition(this.FitToWidthButton.Checked ? FitCondition.Width : FitCondition.None);
            var canvas = CanvasPolicy.Get(this.PageViewerTabControl.SelectedTab);
            if (canvas == null) return;

            try {
                if (this.FitToWidthButton.Checked) CanvasPolicy.FitToWidth(canvas);
            }
            catch (Exception /* err */) { }
            finally {
                this.Refresh(canvas);
            }
        }

        /* ----------------------------------------------------------------- */
        /// FitToHeightButton_Click
        /* ----------------------------------------------------------------- */
        private void FitToHeightButton_Click(object sender, EventArgs e) {
            this.UpdateFitCondition(this.FitToHeightButton.Checked ? FitCondition.Height : FitCondition.None);
            var canvas = CanvasPolicy.Get(this.PageViewerTabControl.SelectedTab);
            if (canvas == null) return;

            try {
                if (this.FitToHeightButton.Checked) CanvasPolicy.FitToHeight(canvas);
            }
            catch (Exception /* err */) { }
            finally {
                this.Refresh(canvas);
            }
        }

        /* ----------------------------------------------------------------- */
        /// NOTE: 2010/09/03 NextPageとPreviousPageはMouseScrollでも利用したいので、本体を分離する
        /* ----------------------------------------------------------------- */
        private bool NextPage()
        {
            bool ret = false;
            var canvas = CanvasPolicy.Get(this.PageViewerTabControl.SelectedTab);
            if (canvas == null) return ret;
            
            try
            {
                CanvasPolicy.NextPage(canvas);
                ret = true;
            }
            catch (Exception /* err */) { ret = false; }
            finally
            {
                this.Refresh(canvas);
            }
            return ret;
        }

        private bool PreviousPage()
        {
            var ret = false;
            var canvas = CanvasPolicy.Get(this.PageViewerTabControl.SelectedTab);
            if (canvas == null) return ret;

            try
            {
                CanvasPolicy.PreviousPage(canvas);
                ret = true;
            }
            catch (Exception /* err */) { ret = false; }
            finally
            {
                this.Refresh(canvas);
            }
            return ret;
        }


        /* ----------------------------------------------------------------- */
        /// NextPageButton_Click
        /* ----------------------------------------------------------------- */
        private void NextPageButton_Click(object sender, EventArgs e) {
            NextPage();
        }

        /* ----------------------------------------------------------------- */
        /// PreviousPageButton_Click
        /* ----------------------------------------------------------------- */
        private void PreviousPageButton_Click(object sender, EventArgs e) {
            PreviousPage();
        }

        /* ----------------------------------------------------------------- */
        /// FirstPageButton_Click
        /* ----------------------------------------------------------------- */
        private void FirstPageButton_Click(object sender, EventArgs e) {
            var canvas = CanvasPolicy.Get(this.PageViewerTabControl.SelectedTab);
            if (canvas == null) return;

            try {
                CanvasPolicy.FirstPage(canvas);
            }
            catch (Exception /* err */) { }
            finally {
                this.Refresh(canvas);
            }
        }

        /* ----------------------------------------------------------------- */
        /// LastPageButton_Click
        /* ----------------------------------------------------------------- */
        private void LastPageButton_Click(object sender, EventArgs e) {
            var canvas = CanvasPolicy.Get(this.PageViewerTabControl.SelectedTab);
            if (canvas == null) return;

            try {
                CanvasPolicy.LastPage(canvas);
            }
            catch (Exception /* err */) { }
            finally {
                this.Refresh(canvas);
            }
        }

        /* ----------------------------------------------------------------- */
        /// CurrentPageTextBox_KeyDown
        /* ----------------------------------------------------------------- */
        private void CurrentPageTextBox_KeyDown(object sender, KeyEventArgs e) {
            var canvas = CanvasPolicy.Get(this.PageViewerTabControl.SelectedTab);
            if (canvas == null) return;

            if (e.KeyCode == Keys.Enter) {
                try {
                    var control = (ToolStripTextBox)sender;
                    int page = int.Parse(control.Text);
                    CanvasPolicy.MovePage(canvas, page);
                }
                catch (Exception /* err */) { }
                finally {
                    this.Refresh(canvas);
                }
            }
        }

        /* ----------------------------------------------------------------- */
        /// SearchTextBox_TextChanged
        /* ----------------------------------------------------------------- */
        private void SearchTextBox_TextChanged(object sender, EventArgs e) {
            begin_ = true;
        }

        /* ----------------------------------------------------------------- */
        /// SearchButton_Click
        /* ----------------------------------------------------------------- */
        private void SearchButton_Click(object sender, EventArgs e) {
            this.Search(this.PageViewerTabControl.SelectedTab, this.SearchTextBox.Text, true);
        }

        /* ----------------------------------------------------------------- */
        /// MenuModeButton_Click
        /* ----------------------------------------------------------------- */
        private void MenuModeButton_Click(object sender, EventArgs e) {
            this.MenuSplitContainer.Panel1Collapsed = !this.MenuSplitContainer.Panel1Collapsed;
            this.MenuModeButton.Image = this.MenuSplitContainer.Panel1Collapsed ?
                global::Cube.Properties.Resources.showmenu :
                global::Cube.Properties.Resources.hidemenu;
            this.Adjust(this.PageViewerTabControl.SelectedTab);
        }

        /* ----------------------------------------------------------------- */
        /// ThumbButton_Click
        /* ----------------------------------------------------------------- */
        private void ThumbButton_Click(object sender, EventArgs e) {
            this.NavigationSplitContainer.Panel1Collapsed = !this.NavigationSplitContainer.Panel1Collapsed;
            this.Adjust(this.PageViewerTabControl.SelectedTab);
        }

        /* ----------------------------------------------------------------- */
        /// PageViewerTabControl_DragEnter
        /* ----------------------------------------------------------------- */
        private void PageViewerTabControl_DragEnter(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.All;
            else e.Effect = DragDropEffects.None;
        }

        /* ----------------------------------------------------------------- */
        /// PageViewerTabControl_DragDrop
        /* ----------------------------------------------------------------- */
        private void PageViewerTabControl_DragDrop(object sender, DragEventArgs e) {
            var control = (TabControl)sender;

            bool current = true;
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (var path in files) {
                    if (System.IO.Path.GetExtension(path).ToLower() != ".pdf") continue;
                    var tab = current ? control.SelectedTab : this.CreateTab(control);
                    current = false;
                    this.Open(tab, path);
                }
            }
        }

        /* ----------------------------------------------------------------- */
        /// RenderThumbnailFinished
        /* ----------------------------------------------------------------- */
        private void RenderThumbnailFinished(int page, bool successs) {
            Invoke(new PDFLibNet.RenderNotifyFinishedHandler(RenderThumbnailFinishedInvoke), page, successs);
        }

        /* ----------------------------------------------------------------- */
        /// RenderThumbnailFinishedInvoke
        /* ----------------------------------------------------------------- */
        private void RenderThumbnailFinishedInvoke(int page, bool successs) {
            var thumb = CanvasPolicy.GetThumbnail(this.NavigationSplitContainer.Panel1);
            if (thumb == null) return;
            if (successs) thumb.Invalidate(thumb.Items[page - 1].Bounds);
        }

        /* ----------------------------------------------------------------- */
        /// PageChanged
        /* ----------------------------------------------------------------- */
        private void PageChanged(object sender, EventArgs e) {
            var thumb = (ListView)sender;
            if (thumb.SelectedItems.Count == 0) return;
            var page = thumb.SelectedItems[0].Index + 1;

            var tab = this.PageViewerTabControl.SelectedTab;
            var canvas = CanvasPolicy.Get(tab);
            CanvasPolicy.MovePage(canvas, page);
            this.Refresh(canvas);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// TabClosed
        /// 
        /// <summary>
        /// コンテキストメニューの「閉じる」が押された時のイベントハンドラ．
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        private void TabClosed(object sender, EventArgs e) {
            var control = this.PageViewerTabControl;
            for (int i = 0; i < control.TabCount; i++) {
                var rect = control.GetTabRect(i);
                if (pos_.X > rect.Left && pos_.X < rect.Right && pos_.Y > rect.Top && pos_.Y < rect.Bottom) {
                    TabPage tab = control.TabPages[i];
                    this.DestroyTab(tab);
                    break;
                }
            }
        }

        /* ----------------------------------------------------------------- */
        /// ContextMenu_MouseDown
        /* ----------------------------------------------------------------- */
        private void ContextMenu_MouseDown(object sender, MouseEventArgs e) {
            pos_ = e.Location;
        }

        #endregion

        /* ----------------------------------------------------------------- */
        //  メンバ変数の定義
        /* ----------------------------------------------------------------- */
        #region Member variables
        private bool begin_ = true;
        private FitCondition fit_ = FitCondition.Height;
        private Point pos_;
        #endregion

    }
}
