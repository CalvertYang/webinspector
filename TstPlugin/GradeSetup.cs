﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.Serialization;
using PxP.Config;

namespace PxP
{
    public partial class GradeSetup : Form
    {
        #region Local Variables

        private List<PointSubPiece> TmpPointSubPieces = GradeVariable.PointSubPieces;
        private List<MarkSubPiece> TmpMarkSubPieces = GradeVariable.MarkGradeSubPieces;
        private List<RoiGrade> TmpColumnsGrid = GradeVariable.RoiColumnsGrid;
        private List<RoiGrade> TmpRowsGrid = GradeVariable.RoiRowsGrid;

        #endregion

        #region Constructor

        public GradeSetup()
        {
            InitializeComponent();
            InitialzeAllCustomObject();
        }

        ~GradeSetup()
        { 
            
        }

        #endregion

        #region Reflactoring

        // 初始化所有物件
        void InitialzeAllCustomObject()
        {
            // Reload grade config
            SystemVariable.LoadGradeConfig();

            // Radio button get default value
            switch (GradeVariable.RoiMode)
            { 
                case 0:
                    rbNoRoi.Checked = true;
                    break;
                case 1:
                    rbSymmetrical.Checked = true;
                    break;
                default:
                    rbNoRoi.Checked = true;
                    break;
            }

            // Textbox get default value
            if (GradeVariable.RoiGradeColumns > 0)
                tboxColumns.Text = GradeVariable.RoiGradeColumns.ToString();
            if (GradeVariable.RoiGradeRows > 0)
                tboxRows.Text = GradeVariable.RoiGradeRows.ToString();

            // Set cobobox get xml list in config folder
            bsGradConfigList.DataSource = GetGradeConfList();
            cboxGradeConfigFile.DataSource = bsGradConfigList.DataSource;
            cboxGradeConfigFile.SelectedItem = SystemVariable.GradeConfigFileName.ToString().Substring(0, SystemVariable.GradeConfigFileName.ToString().LastIndexOf("."));

            // Set columns gridview get xml default value
            bsColumns.DataSource = TmpColumnsGrid;
            gvColumns.DataSource = bsColumns.DataSource;

            // Set rows gridview get xml default vale
            bsRows.DataSource = TmpRowsGrid;
            gvRows.DataSource = bsRows.DataSource;

            // Subpiece comobox get list
            bsRoiList.DataSource = GetSubPieceList();
            cboxSubPieceOfGrade.DataSource = bsRoiList.DataSource;
            cboxSubPieceOfPoint.DataSource = bsRoiList.DataSource;

            // Grade setting > Point get grid value for ROI-11, ROI-12 etc...
            foreach (PointSubPiece p in TmpPointSubPieces)
            { 
                if(p.Name == cboxSubPieceOfPoint.Text)
                    bsPointSubPiece.DataSource = p.Grades;
            }
            
            gvPoint.DataSource = bsPointSubPiece.DataSource;
            gvPoint.Columns["ClassId"].Visible = false;
            gvPoint.Columns["ClassName"].ReadOnly = true;
            // Grade setting > Point get value of cobobox for enable 
            cboxEnablePTS.Checked = GradeVariable.IsPointEnable;

            // Grade setting > MarkGrade get value of cobobox for enable
            cboxEnableGrade.Checked = GradeVariable.IsMarkGradeEnable;

            // Grade setting > MarkGrade get grid value for ROI-11, ROI-12 etc...
            foreach (MarkSubPiece m in TmpMarkSubPieces)
            {
                if (m.Name == cboxSubPieceOfPoint.Text)
                    bsMarkSubPiece.DataSource = m.Grades;
            }
            gvGrade.DataSource = bsMarkSubPiece.DataSource;
            gvGrade.Columns["GradeName"].ReadOnly = true;

            // Grade setting > pass or fail of cobobox for enable
            cboxEnablePFS.Checked = GradeVariable.IsPassOrFailScoreEnable;

            // Grade setting > pass or fail of textbox for get filter score
            tboxFilterScore.Text = GradeVariable.PassOrFileScore.ToString();

            // Set panel visiable
            panelCreateGrid.Visible = !rbNoRoi.Checked;
            gbPointSetting.Enabled = cboxEnablePTS.Checked;
            gbGradeSetting.Enabled = cboxEnableGrade.Checked;
            tboxFilterScore.Enabled = cboxEnablePFS.Checked;
        }

        // 取得 Folder 底下所有 XML 清單
        List<string> GetGradeConfList()
        {
            List<string> ConfList = new List<string>();
            string ConfPath = Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName) + "/../Parameter Files/CPxP/grade/";
            DirectoryInfo di = new DirectoryInfo(ConfPath);
            FileInfo[] rgFiles = di.GetFiles("*.xml");

            foreach (FileInfo fi in rgFiles)
            {
                ConfList.Add(fi.Name.ToString().Substring(0, fi.Name.ToString().LastIndexOf(".")));
            }
            return ConfList;
        }

        // Get subpiece list
        List<string> GetSubPieceList()
        {
            List<string> result = new List<string>();
            result.Add("ALL");
            foreach (RoiGrade r in TmpRowsGrid)
            {
                foreach (RoiGrade c in TmpColumnsGrid)
                {
                    string tmp =  "ROI-" + r.Name + c.Name;
                    result.Add(tmp);
                }
            }
            return result;
        }

        // 轉換 ASCII Number
        public static char Chr(int Num)
        {
            char C = Convert.ToChar(Num);
            return C;
        }

        // 轉換 Char to ASCnumber
        public static int ASC(string S)
        {
            int N = Convert.ToInt32(S[0]);
            return N;
        }

        // Switch radiobutton to get ROI Model 
        int GetROIMode(Control container)
        {
            int RoiMode = 0;
            RadioButton rb = GetCheckedRadio(container);
            switch (rb.Name)
            { 
                case "rbNoRoi":
                    RoiMode = 0;
                    break;
                case "rbSymmetrical":
                    RoiMode = 1;
                    break;
            }
            return RoiMode;
        }

        RadioButton GetCheckedRadio(Control container)
        {
            foreach (Control control in container.Controls)
            {
                RadioButton radio = control as RadioButton;

                if (radio != null && radio.Checked)
                {
                    return radio;
                }
            }
            return null;
        }

        #endregion

        #region Action Events
        
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // Save XML
        private void btnSaveGradeConfigFile_Click(object sender, EventArgs e)
        {
            string FolderPath = Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName) + "\\..\\Parameter Files\\CPxP\\grade\\";
            string FullSystemPath = FolderPath + cboxGradeConfigFile.Text + ".xml";
            GradeConfig gc = new GradeConfig();
            gc.Roi = new Roi();
            gc.Roi.RoiMode = GetROIMode(gbROIItem).ToString();
            gc.Roi.RoiRows = gvRows.Rows.Count.ToString();
            gc.Roi.RoiColumns = gvColumns.Rows.Count.ToString();
            gc.Roi.Column = new Column[gvColumns.Rows.Count];
            for (var i = 0; i < gvColumns.Rows.Count; i++)
            {
                gc.Roi.Column[i] = new Column();
                gc.Roi.Column[i].Name = gvColumns.Rows[i].Cells["Name"].Value.ToString();
                gc.Roi.Column[i].Start = gvColumns.Rows[i].Cells["Start"].Value.ToString();
                gc.Roi.Column[i].End = gvColumns.Rows[i].Cells["End"].Value.ToString();
            }
            gc.Roi.Row = new Row[gvRows.Rows.Count];
            for (var i = 0; i < gvRows.Rows.Count; i++)
            {
                gc.Roi.Row[i] = new Row();
                gc.Roi.Row[i].Name = gvRows.Rows[i].Cells["Name"].Value.ToString();
                gc.Roi.Row[i].Start = gvRows.Rows[i].Cells["Start"].Value.ToString();
                gc.Roi.Row[i].End = gvRows.Rows[i].Cells["End"].Value.ToString();
            }
            gc.Grade = new PxP.Config.Grade();
          
            // Write PxP.Config.PointGrade data
            gc.Grade.PointGrade = new PxP.Config.PointGrade();
            gc.Grade.PointGrade.Enable = cboxEnablePTS.Checked ? "1" : "0";
            gc.Grade.PointGrade.SubPiece = new SubPiece[TmpPointSubPieces.Count];

            for (var i = 0; i < TmpPointSubPieces.Count; i++)
            {
                gc.Grade.PointGrade.SubPiece[i] = new SubPiece();
                gc.Grade.PointGrade.SubPiece[i].Name = ((PointSubPiece)TmpPointSubPieces[i]).Name.ToString();
                gc.Grade.PointGrade.SubPiece[i].Items = new object[((PointSubPiece)TmpPointSubPieces[i]).Grades.Count];
                for (var j = 0; j < ((PointSubPiece)TmpPointSubPieces[i]).Grades.Count; j++)
                {
                    gc.Grade.PointGrade.SubPiece[i].Items[j] = new PxP.Config.FlawType();
                    if (cboxAllSameOfPoint.Checked)
                    {
                        ((PointSubPiece)TmpPointSubPieces[i]).Grades[j].Score = ((PointSubPiece)TmpPointSubPieces[0]).Grades[j].Score;
                    }

                    ((PxP.Config.FlawType)gc.Grade.PointGrade.SubPiece[i].Items[j]).Id = ((PointSubPiece)TmpPointSubPieces[i]).Grades[j].ClassId.ToString();
                    ((PxP.Config.FlawType)gc.Grade.PointGrade.SubPiece[i].Items[j]).Value = ((PointSubPiece)TmpPointSubPieces[i]).Grades[j].Score.ToString();
                }
            }
           
            // Write PxP.Config.MarkGrade data
            gc.Grade.MarkGrade = new PxP.Config.MarkGrade();
            gc.Grade.MarkGrade.Enable = cboxEnableGrade.Checked ? "1" : "0";
            gc.Grade.MarkGrade.SubPiece = new SubPiece[TmpMarkSubPieces.Count];
            for (var i = 0; i < TmpMarkSubPieces.Count; i++)
            {
                gc.Grade.MarkGrade.SubPiece[i] = new SubPiece();
                gc.Grade.MarkGrade.SubPiece[i].Name = ((MarkSubPiece)TmpMarkSubPieces[i]).Name.ToString();
                
                if (cboxAllSameOfGrade.Checked)
                {
                        gc.Grade.MarkGrade.SubPiece[i].Items = new object[((MarkSubPiece)TmpMarkSubPieces[0]).Grades.Count];
                }
                else
                {
                    gc.Grade.MarkGrade.SubPiece[i].Items = new object[((MarkSubPiece)TmpMarkSubPieces[i]).Grades.Count];
                }

                for (var j = 0; j < gc.Grade.MarkGrade.SubPiece[i].Items.Count(); j++)
                {
                    gc.Grade.MarkGrade.SubPiece[i].Items[j] = new GradeRow();
                    if (cboxAllSameOfGrade.Checked)
                    {
                        if (TmpMarkSubPieces[i].Grades.Count != ((MarkSubPiece)TmpMarkSubPieces[0]).Grades.Count)
                        {
                            ((MarkSubPiece)TmpMarkSubPieces[i]).Grades = new List<MarkGrade>(((MarkSubPiece)TmpMarkSubPieces[0]).Grades.Count);
                            ((MarkSubPiece)TmpMarkSubPieces[i]).Grades = ((MarkSubPiece)TmpMarkSubPieces[0]).Grades;
                        }
                    }
                    ((GradeRow)gc.Grade.MarkGrade.SubPiece[i].Items[j]).Id = ((MarkSubPiece)TmpMarkSubPieces[i]).Grades[j].GradeName.ToString();
                    ((GradeRow)gc.Grade.MarkGrade.SubPiece[i].Items[j]).Value = ((MarkSubPiece)TmpMarkSubPieces[i]).Grades[j].Score.ToString();
                }
            }

            // Write PxP.Config.PassOrFail data
            gc.Grade.PassFail = new PassFail();
            gc.Grade.PassFail.Enable = cboxEnablePFS.Checked ? "1" : "0";
            gc.Grade.PassFail.Score = tboxFilterScore.Text;
            
            XDocument XDoc = SerializationUtil.Serialize(gc);

            XDoc.Save(FullSystemPath);
            GradeVariable.MarkGradeSubPieces = TmpMarkSubPieces;
            GradeVariable.PointSubPieces = TmpPointSubPieces;
            GradeVariable.RoiColumnsGrid = TmpColumnsGrid;
            GradeVariable.RoiRowsGrid = TmpRowsGrid;
            MessageBox.Show("Success");
        }

        // Select subpiece like ROI-11, ROI-12
        private void cboxSubPieceOfPoint_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (PointSubPiece p in TmpPointSubPieces)
            {
                if (p.Name == cboxSubPieceOfPoint.Text)
                {
                    bsPointSubPiece.DataSource = p.Grades;
                    bsPointSubPiece.ResetBindings(false);
                    bsPointSubPiece.ResumeBinding();
                }
            }
            gvPoint.DataSource = bsPointSubPiece.DataSource;
        }

        private void cboxSubPieceOfGrade_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (MarkSubPiece m in TmpMarkSubPieces)
            {
                if (m.Name == cboxSubPieceOfGrade.Text)
                {
                    bsMarkSubPiece.DataSource = m.Grades;
                    bsMarkSubPiece.ResetBindings(false);
                    bsMarkSubPiece.ResumeBinding();
                }
            }
            gvGrade.DataSource = bsMarkSubPiece.DataSource;
        }

        private void gvGrade_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            // Get all rows entered on each press of Enter.
            if (e.RowIndex == gvGrade.Rows.Count - 1 && !String.IsNullOrEmpty(gvGrade.Rows[e.RowIndex].Cells[1].Value.ToString()))
            {
                foreach (MarkSubPiece m in TmpMarkSubPieces)
                {
                    if (m.Name == cboxSubPieceOfGrade.Text)
                    {
                        MarkGrade tmp = new MarkGrade();
                        if (e.RowIndex < 4)
                        {
                            tmp.GradeName = Chr(e.RowIndex + 66).ToString();
                            tmp.Score = Convert.ToInt32(gvGrade.Rows[e.RowIndex].Cells[1].Value) + 1;
                            m.Grades.Add(tmp);
                        }
                        bsMarkSubPiece.DataSource = m.Grades;
                    }
                }
                gvGrade.DataSource = typeof(MarkSubPiece);
                gvGrade.DataSource = bsMarkSubPiece.DataSource;
                gvGrade.Columns["GradeName"].ReadOnly = true;
            }
        }

        private void gvGrade_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete )
            {
                if (gvGrade.Rows.Count > 1)
                {
                    bsMarkSubPiece.RemoveAt(gvGrade.CurrentRow.Index);
                    bsMarkSubPiece.ResetBindings(false);
                    bsMarkSubPiece.ResumeBinding();
                    for (var i = 0; i < bsMarkSubPiece.Count; i++)
                    {
                        ((MarkGrade)bsMarkSubPiece[i]).GradeName = Chr(i + 65).ToString();
                    }
                }
                else
                {
                    ((MarkGrade)bsMarkSubPiece[0]).Score = 0;
                }
                gvGrade.DataSource = typeof(MarkSubPiece);
                gvGrade.DataSource = bsMarkSubPiece.DataSource;
                gvGrade.Columns[0].ReadOnly = true;
            }
        }

        private void gvGrade_MouseDown(object sender, MouseEventArgs e)
        {
            gvGrade.EndEdit();
        }

        private void gvGrade_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;
            e.Cancel = false;
        }

        private void btnCreateGrid_Click(object sender, EventArgs e)
        {
            int x = int.TryParse(tboxColumns.Text, out x) ? x : 1;
            int y = int.TryParse(tboxRows.Text, out y) ? y : 1;
            TmpColumnsGrid.Clear();
            for (var i = 0; i < x; i++)
            {
                RoiGrade tmp = new RoiGrade();
                tmp.Name = (i + 1).ToString();
                tmp.Start = 0;
                tmp.End = 0;
                TmpColumnsGrid.Add(tmp);
            }
            TmpRowsGrid.Clear();
            for (var i = 0; i < y; i++)
            {
                RoiGrade tmp = new RoiGrade();
                tmp.Name = (i + 1).ToString();
                tmp.Start = 0;
                tmp.End = 0;
                TmpRowsGrid.Add(tmp);
            }

            gvColumns.DataSource = typeof(List<RoiGrade>);
            gvColumns.DataSource = bsColumns.DataSource;
            gvRows.DataSource = typeof(List<RoiGrade>);
            gvRows.DataSource = bsRows.DataSource;

            // Clear and create list of ROI-Pieces like ROI-11, ROI-12 etc...
            bsRoiList.DataSource = GetSubPieceList();
            cboxSubPieceOfPoint.DataSource = bsRoiList.DataSource;
            cboxSubPieceOfGrade.DataSource = bsRoiList.DataSource;

            // Create new TmpPointSubPieces
            // 1. 先加入一筆 ALL 的 SubPiece 指全部使用相同設定
            TmpPointSubPieces.Clear();
            PointSubPiece allsame = new PointSubPiece();
            allsame.Name = "ALL";
            allsame.Grades = new List<PointGrade>();
            foreach (FlawTypeNameExtend i in PxPVariable.FlawTypeName)
            {
                PointGrade tmpPG = new PointGrade();
                tmpPG.ClassId = i.FlawType;
                tmpPG.ClassName = i.Name;
                tmpPG.Score = 0;

                allsame.Grades.Add(tmpPG);
            }
            TmpPointSubPieces.Add(allsame);
            
            // 2. 用 Rows 和 Columns 造出所有欄位
            foreach (RoiGrade r in TmpRowsGrid)
            {
                foreach (RoiGrade c in TmpColumnsGrid)
                {
                    PointSubPiece tmpPointSubPiece = new PointSubPiece();
                    tmpPointSubPiece.Name = "ROI-" + r.Name + c.Name;
                    tmpPointSubPiece.Grades = new List<PointGrade>();
                    foreach (FlawTypeNameExtend i in PxPVariable.FlawTypeName)
                    {
                        PointGrade tmpPG = new PointGrade();
                        tmpPG.ClassId = i.FlawType;
                        tmpPG.ClassName = i.Name;
                        tmpPG.Score = 0;
                        tmpPointSubPiece.Grades.Add(tmpPG);
                    }
                    TmpPointSubPieces.Add(tmpPointSubPiece);
                }
            }


            // Create new TmpMarkSubPieces
            // 1. 先加入一筆 ALL 的 SubPiece 指全部使用相同設定
            TmpMarkSubPieces.Clear();
            MarkSubPiece msp = new MarkSubPiece();
            msp.Name = "ALL";
            msp.Grades = new List<MarkGrade>();

            MarkGrade tmpMG = new MarkGrade();
            tmpMG.GradeName = Chr(65).ToString();
            tmpMG.Score = 0;
            msp.Grades.Add(tmpMG);
            TmpMarkSubPieces.Add(msp);
            
            // 2. 用 Rows 和 Columns 造出所有欄位
            foreach (RoiGrade r in TmpRowsGrid)
            {
                foreach (RoiGrade c in TmpColumnsGrid)
                {
                    MarkSubPiece tmpMarkSubPiece = new MarkSubPiece();
                    tmpMarkSubPiece.Name = "ROI-" + r.Name + c.Name;
                    tmpMarkSubPiece.Grades = new List<MarkGrade>();

                    MarkGrade tmpMG2 = new MarkGrade();
                    tmpMG2.GradeName = Chr(65).ToString();
                    tmpMG2.Score = 0;
                    tmpMarkSubPiece.Grades.Add(tmpMG2);
                    TmpMarkSubPieces.Add(tmpMarkSubPiece);
                }
            }
            cboxSubPieceOfPoint_SelectedIndexChanged(cboxSubPieceOfPoint,e);
            cboxSubPieceOfGrade_SelectedIndexChanged(cboxSubPieceOfGrade,e);
        }

        private void rbNoRoi_CheckedChanged(object sender, EventArgs e)
        {
            panelCreateGrid.Visible = !rbNoRoi.Checked;
        }

        private void cboxEnablePTS_CheckedChanged(object sender, EventArgs e)
        {
            gbPointSetting.Enabled = cboxEnablePTS.Checked;
        }

        private void cboxEnableGrade_CheckedChanged(object sender, EventArgs e)
        {
            gbGradeSetting.Enabled = cboxEnableGrade.Checked;
            cboxEnablePTS.Enabled = !gbGradeSetting.Enabled;
            if (gbGradeSetting.Enabled)
                cboxEnablePTS.Checked = true;
        }

        private void cboxEnablePFS_CheckedChanged(object sender, EventArgs e)
        {
            tboxFilterScore.Enabled = cboxEnablePFS.Checked;
        }

        private void gvPoint_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;
            e.Cancel = false;
        }

        private void gvGrade_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (gvGrade.Rows.Count > 1 && gvGrade.Columns[e.ColumnIndex].Name == "Score" && e.RowIndex > 0)
            {
                if (Convert.ToInt32(e.FormattedValue) <= Convert.ToInt32(gvGrade.Rows[e.RowIndex - 1].Cells[1].Value))
                {
                    MessageBox.Show("Test");
                    e.Cancel = true;
                }
            }
        }
        
        #endregion
    }
}
