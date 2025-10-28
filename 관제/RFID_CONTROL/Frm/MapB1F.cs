using Newtonsoft.Json.Linq;
using RFID_CONTROLLER.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RFID_CONTROLLER.Frm
{
    public partial class MapB1F : Form
    {
        private Dictionary<String, GateInfo> icons;
        public String gateID;
        private Thread thread;
        private delegate void DisplayHandler();
        private sttPanelCnt B1SttPanelBig = new sttPanelCnt(11);

        public MapB1F()
        {
            InitializeComponent();

            thread = new Thread(delegate ()
            {
                run();
            });

            thread.Start();
        }

        private void MapB1F_Load(object sender, EventArgs e)
        {
            //게이트 아이콘 표시
            icons = new Dictionary<String, GateInfo>();

            icons.Add("RFB1G01", new GateInfo(
                pictureBox1,
                FitWidthGap(Convert.ToInt32(pictureBox1.Size.Width * 23.5 / 100)),
                Convert.ToInt32(pictureBox1.Size.Height * 42 / 100)
                )
            );

            icons.Add("RFB1G02", new GateInfo(
                pictureBox1,
                FitWidthGap(Convert.ToInt32(pictureBox1.Size.Width * 63 / 100)),
                Convert.ToInt32(pictureBox1.Size.Height * 83 / 100)
                )
            );

            icons.Add("RFB1G03", new GateInfo(
                pictureBox1,
                FitWidthGap(Convert.ToInt32(pictureBox1.Size.Width * 47.5 / 100)),
                Convert.ToInt32(pictureBox1.Size.Height * 66 / 100)
                )
            );

            icons.Add("RFB1G04", new GateInfo(
                pictureBox1,
                FitWidthGap(Convert.ToInt32(pictureBox1.Size.Width * 59 / 100)),
                Convert.ToInt32(pictureBox1.Size.Height * 32 / 100)
                )
            );

            icons.Add("RFB1G05", new GateInfo(
                pictureBox1,
                FitWidthGap(Convert.ToInt32(pictureBox1.Size.Width * 51 / 100)),
                Convert.ToInt32(pictureBox1.Size.Height * 43 / 100)
                )
            );

            icons.Add("RFB1G06", new GateInfo(
                pictureBox1,
                FitWidthGap(Convert.ToInt32(pictureBox1.Size.Width * 56.5 / 100)),
                Convert.ToInt32(pictureBox1.Size.Height * 32.5 / 100)
                )
            );

            icons.Add("RFB1G07", new GateInfo(
                pictureBox1,
                FitWidthGap(Convert.ToInt32(pictureBox1.Size.Width * 26 / 100)),
                Convert.ToInt32(pictureBox1.Size.Height * 24.5 / 100)
                )
            );

            icons.Add("RFB1G08", new GateInfo(
                pictureBox1,
                FitWidthGap(Convert.ToInt32(pictureBox1.Size.Width * 47 / 100)),
                Convert.ToInt32(pictureBox1.Size.Height * 37.5 / 100)
                )
            );

            icons.Add("RFB1G09", new GateInfo(
                pictureBox1,
                FitWidthGap(Convert.ToInt32(pictureBox1.Size.Width * 66.5 / 100)),
                Convert.ToInt32(pictureBox1.Size.Height * 11.5 / 100)
                )
            );

            icons.Add("RFB1G10", new GateInfo(
                pictureBox1,
                FitWidthGap(Convert.ToInt32(pictureBox1.Size.Width * 83.5 / 100)),
                Convert.ToInt32(pictureBox1.Size.Height * 35 / 100)
                )
            );

            icons.Add("RFB1G11", new GateInfo(
                pictureBox1,
                FitWidthGap(Convert.ToInt32(pictureBox1.Size.Width * 82 / 100)),
                Convert.ToInt32(pictureBox1.Size.Height * 38 / 100)
                )
            );

            ContextMenu menu = new ContextMenu();
            menu.MenuItems.Add(new MenuItem("시작", (sndr, args) =>
                menuItemStartClick(sender, args)
            ));
            menu.MenuItems.Add(new MenuItem("중지", (sndr, args) =>
                menuItemStopClick(sender, args)
            ));
            menu.MenuItems.Add(new MenuItem("재시작", (sndr, args) =>
                menuItemRestartClick(sender, args)
            ));
            menu.MenuItems.Add(new MenuItem("경광등제어", (sndr, args) =>
                menuItemControlClick(sender, args)
            ));

            foreach (KeyValuePair<string, GateInfo> icon in icons)
            {
                icon.Value.setGateID(icon.Key);
                icon.Value.setContextMenu(menu);
                icon.Value.setParent(this);
                icon.Value.showGateId();
            }
        }

        internal void setTooltip(Control c, string p)
        {
            toolTip1.SetToolTip(c, p);
        }

        private int FitWidthGap(int locationX)
        {
            int shrinkRate = 35;
            if (locationX > pictureBox1.Size.Width / 2)
            {
                locationX += (locationX - (pictureBox1.Size.Width / 2)) * 1 / shrinkRate;
            }
            else
            {
                locationX -= ((pictureBox1.Size.Width / 2) - locationX) * 1 / shrinkRate;
            }
            return locationX;
        }

        private void ShowToolTip(Control control, string text)
        {
            System.Windows.Forms.ToolTip toolTip = new System.Windows.Forms.ToolTip();
            toolTip.InitialDelay = 0;
            toolTip.ShowAlways = true;
            toolTip.SetToolTip(control, text);
            toolTip.Show(text, control, 0, control.Height);
        }

        private void run()
        {
            while (true)
            {
#if !DEBUG
                Thread.Sleep(1000 * 5);
                this.Invoke(new DisplayHandler(display));
#endif
            }
        }

        private void display()
        {
            Dictionary<string, object> aData = new Dictionary<string, object>();
            aData = new Dictionary<string, object>();
            aData.Add("WORKDIV", "118REQ");
            aData.Add("CRUD_TP", "R");

            JObject response = TCPClient.request(aData);
            JArray rows = (JArray)response["rows"];

            if (rows != null)
            {
                for (int i = 0; i < rows.Count; i++)
                {
                    JObject row = (JObject)rows[i];

                    String key = (string)row["GATE_ID"];

                    if (icons.ContainsKey(key))
                    {
                        GateInfo gate = icons[key];
                        if (gate != null)
                        {
                            gate.updateUI((string)row["STATUS"]);

                            switch (key.Substring(0, 4))
                            {
                                case "RFB1":
                                    if (row["STATUS"].ToString().Equals("NOR")) B1SttPanelBig.addNorCnt();
                                    else if (row["STATUS"].ToString().Equals("ERR")) B1SttPanelBig.addErrCnt();
                                    else if (row["STATUS"].ToString().Equals("IN")) B1SttPanelBig.addInCnt();
                                    else if (row["STATUS"].ToString().Equals("OUT")) B1SttPanelBig.addOutCnt();
                                    else B1SttPanelBig.addErrCnt();
                                    break;
                            }
                        }
                    }
                }

                B1SttPanelBig.cntCheck();

                statusPanel1.norSttLabel = B1SttPanelBig.getNorCnt().ToString();
                statusPanel1.errSttLabel = B1SttPanelBig.getErrCnt().ToString();
                statusPanel1.inSttLabel = B1SttPanelBig.getInCnt().ToString();
                statusPanel1.outSttLabel = B1SttPanelBig.getOutCnt().ToString();

                B1SttPanelBig.resetAllCnt();

            }
        }

        private void closeBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void MapB1F_FormClosing(object sender, FormClosingEventArgs e)
        {
            thread.Abort();
            this.Dispose();
        }

        private void menuItemStartClick(object sender, EventArgs args)
        {
            Form sign = new SignManager();
            sign.StartPosition = FormStartPosition.Manual;
            sign.Left = (this.Left + (this.Width - sign.Width) / 2) - 40;
            sign.Top = 150;
            DialogResult result = sign.ShowDialog();

            if (result == DialogResult.OK)
            {
                Dictionary<string, object> aData = new Dictionary<string, object>();
                aData.Add("WORKDIV", "004REQ");
                aData.Add("WK_TYPE", "STT");
                aData.Add("GATE_ID", this.gateID);

                JObject response = TCPClient.request(aData);

                if ((string)response["RESULT"] == "OK")
                {
                    MessageBox.Show("처리되었습니다.");
                }
                else
                {
                    MessageBox.Show((string)response["ERROR"]);
                }
            }
        }

        private void menuItemStopClick(object sender, EventArgs args)
        {
            Form sign = new SignManager();
            sign.StartPosition = FormStartPosition.Manual;
            sign.Left = (this.Left + (this.Width - sign.Width) / 2) - 40;
            sign.Top = 150;
            DialogResult result = sign.ShowDialog();

            if (result == DialogResult.OK)
            {
                Dictionary<string, object> aData = new Dictionary<string, object>();
                aData.Add("WORKDIV", "004REQ");
                aData.Add("WK_TYPE", "STP");
                aData.Add("GATE_ID", this.gateID);

                JObject response = TCPClient.request(aData);

                if ((string)response["RESULT"] == "OK")
                {
                    MessageBox.Show("처리되었습니다.");
                }
                else
                {
                    MessageBox.Show((string)response["ERROR"]);
                }
            }

        }

        private void menuItemRestartClick(object sender, EventArgs args)
        {
            Form sign = new SignManager();
            sign.StartPosition = FormStartPosition.Manual;
            sign.Left = (this.Left + (this.Width - sign.Width) / 2) - 40;
            sign.Top = 150;
            DialogResult result = sign.ShowDialog();

            if (result == DialogResult.OK)
            {
                Dictionary<string, object> aData = new Dictionary<string, object>();
                aData.Add("WORKDIV", "004REQ");
                aData.Add("WK_TYPE", "RBT");
                aData.Add("GATE_ID", this.gateID);

                JObject response = TCPClient.request(aData);

                if ((string)response["RESULT"] == "OK")
                {
                    MessageBox.Show("처리되었습니다.");
                }
                else
                {
                    MessageBox.Show((string)response["ERROR"]);
                }
            }
        }

        private void menuItemControlClick(object sender, EventArgs args)
        {
            Form sign = new SignManager();
            sign.StartPosition = FormStartPosition.Manual;
            sign.Left = (this.Left + (this.Width - sign.Width) / 2) - 40;
            sign.Top = 150;
            DialogResult result = sign.ShowDialog();

            if (result == DialogResult.OK)
            {
                Dictionary<string, object> aData = new Dictionary<string, object>();
                aData.Add("WORKDIV", "004REQ");
                aData.Add("WK_TYPE", "OBZ");
                aData.Add("GATE_ID", this.gateID);

                JObject response = TCPClient.request(aData);

                if ((string)response["RESULT"] == "OK")
                {
                    MessageBox.Show("처리되었습니다.");
                }
                else
                {
                    MessageBox.Show((string)response["ERROR"]);
                }
            }
        }

        class GateInfo
        {
            private MapB1F parent;
            private string mapNm = string.Empty;
            private string gateID;
            private static Image iconNOR = Resources.img_circle_g_big_32;
            private static Image iconERR = Resources.img_circle_o_big_32;
            private static Image iconOut = Resources.img_circle_r_big_32;
            private static Image iconIn = Resources.img_circle_b_big_32;

            private Control pictureBox;
            private int x;
            private int y;

            public PictureBox icon
            {
                get; set;
            }

            public GateInfo(Control c, int x, int y)
            {
                icon = new PictureBox();
                icon.BackgroundImageLayout = ImageLayout.None;
                icon.SizeMode = PictureBoxSizeMode.AutoSize;

                icon.Location = new Point(x, y);

                c.Controls.Add(icon);

                icon.Click += new EventHandler(icon_Click);
                icon.MouseEnter += new EventHandler(icon_Enter);
                updateUI(iconERR);

                pictureBox = c;
                this.x = x;
                this.y = y;
            }

            public void showGateId()
            {
                System.Windows.Forms.Label dynamicLabel = new System.Windows.Forms.Label();
                dynamicLabel.Text = this.gateID.Substring(this.gateID.Length - 5);
                dynamicLabel.Font = new Font("Arial", 13, FontStyle.Bold);
                dynamicLabel.ForeColor = Color.Black;
                dynamicLabel.BackColor = Color.White;
                dynamicLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                dynamicLabel.AutoSize = true;
                dynamicLabel.Location = new System.Drawing.Point(x - dynamicLabel.Size.Width / 6, y + iconNOR.Size.Height);
                this.pictureBox.Controls.Add(dynamicLabel);
            }

            public void setGateID(String gateID)
            {
                this.gateID = gateID;
            }

            public void icon_Click(object sender, EventArgs e)
            {
                parent.gateID = this.gateID;
                icon.ContextMenu.Show(icon, new Point(5, 5));
            }

            public void icon_Enter(object sender, EventArgs e)
            {
                parent.setTooltip(icon, this.gateID.Substring(this.gateID.Length - 5) + "\n" + Utils.gateIdNameDic[this.gateID]);
            }

            public void setContextMenu(ContextMenu cs)
            {
                icon.ContextMenu = cs;
            }

            public void updateUI(Image status)
            {
                icon.Image = status;
                icon.BackColor = Color.Transparent;
            }

            public void updateUI(String status)
            {
                if (status == "ERR") updateUI(iconERR);
                else if (status == "NOR") updateUI(iconNOR);
                else if (status == "OUT") updateUI(iconOut);
                else if (status == "IN") updateUI(iconIn);
                else updateUI(iconERR);
            }
            public void setParent(MapB1F c)
            {
                this.parent = c;
            }
        }

        private void toolTip1_Draw(object sender, DrawToolTipEventArgs e)
        {
            using (StringFormat sf = new StringFormat())
            {
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                sf.HotkeyPrefix = System.Drawing.Text.HotkeyPrefix.None;
                sf.FormatFlags = StringFormatFlags.NoWrap;

                e.DrawBackground();
                e.DrawBorder();
                using (Font f = new Font("Tahoma", 12))
                {
                    e.Graphics.DrawString(e.ToolTipText, f,
                    SystemBrushes.ActiveCaptionText, e.Bounds, sf);
                }
            }

        }

        private void toolTip1_Popup(object sender, PopupEventArgs e)
        {
            Font customFont = new Font("Tahoma", 12);
            var textSize = TextRenderer.MeasureText(toolTip1.GetToolTip((Control)e.AssociatedControl), customFont);
            e.ToolTipSize = new Size(Convert.ToInt32(textSize.Width * 1.3), textSize.Height + 10);
        }
    }
}
