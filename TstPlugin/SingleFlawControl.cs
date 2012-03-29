﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WRPlugIn;
using System.Threading;

namespace PxP
{
    public partial class SingleFlawControl : UserControl
    {
        public PictureBox[] pb;
        private Label lbCoordinate;
        private double dRatio = 0;
        private double dCurrentRatio = 0;

        private IFlawInfo flaw;

        public SingleFlawControl(IFlawInfo info, int station)
        {
            InitializeComponent();

            flaw = info;
            lbFlawID.Text += info.FlawID.ToString();
            lbCoordinate = new Label();
            lbCoordinate.BackColor = Color.LightSkyBlue;
            lbCoordinate.AutoSize = true;
            lbCoordinate.Visible = false;

            pb = new PictureBox[station];

            for (int i = 0; i < station; i++)
            {
                tabFlawControl.TabPages.Add("S" + ((i + 1).ToString()));
                pb[i] = new PictureBox();
                pb[i].SizeMode = PictureBoxSizeMode.Zoom;
                pb[i].Location = new Point(0, 0);
                pb[i].DoubleClick += new EventHandler(SingleFlawControl_DoubleClick);
                //pb[i].MouseMove+=new MouseEventHandler(PictureBox_MouseMove);
                //pb[i].MouseHover += new EventHandler(PictureBox_MouseMove);
                //pb[i].MouseLeave+=new EventHandler(PictrueBox_MouseLeave);
                //pb[i].Click +=new EventHandler(PictureBox_Click);
                tabFlawControl.TabPages[i].Controls.Add(pb[i]);
            }

            foreach (IImageInfo image in info.Images)
            {
                pb[image.Station].Image = image.Image;
            }

            this.Tag = info.FlawID;
        }

        private MouseEventArgs e;
        private object sender;
        private void PictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            this.e = e;
            this.sender = sender;
            new Thread(() =>
            {
                MethodInvoker GetThread = new MethodInvoker(GetLabel);
                this.BeginInvoke(GetThread);

            }).Start();

        }
        private void GetLabel()
        {
            TabPage tp = tabFlawControl.SelectedTab;
            Point pbPoint = e.Location;
            int x = pbPoint.X + ((PictureBox)sender).Location.X;
            int y = pbPoint.Y + ((PictureBox)sender).Location.Y;
            int xResult = x + 10;
            int yResult = y + 10;

            lbCoordinate.Text = ((int)(pbPoint.X / dCurrentRatio)).ToString() + "," + ((int)(pbPoint.Y / dCurrentRatio)).ToString();

            if (xResult + lbCoordinate.Width + 5 > tp.ClientSize.Width)
            {
                xResult = tp.ClientSize.Width - lbCoordinate.Width - 10;
            }

            if (yResult + lbCoordinate.Height + 5 > tp.ClientSize.Height)
            {
                yResult = tp.ClientSize.Height - lbCoordinate.Height - 10;
            }

            tabFlawControl.SelectedTab.Controls.Add(lbCoordinate);
            lbCoordinate.Location = new Point(xResult, yResult);
            lbCoordinate.Visible = true;
            lbCoordinate.BringToFront();
        }
        private void SingleFlawControl_DoubleClick(object sender, EventArgs e)
        { 
            
        }

    }
}
