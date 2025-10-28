using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RFID_CONTROLLER
{
    public partial class UnauthOutPanel : UserControl
    {
        public UnauthOutPanel(Color color, Dictionary<string, string> dicParams)
        {
            InitializeComponent();

            //locationLbl.Text = locationText;
            labelDate = dicParams["dtTm"];
            labelMgmtId = dicParams["mgmtNo"];
            labelInOut = dicParams["inOut"];
            setFloorGateLabel(dicParams["floor"], dicParams["gateId"]);

            roundBorderPanel.fillColor = color;
            roundBorderPanel.borderColor = color;
        }

        public string labelDate
        {
            get => lblDate.Text; set=> lblDate.Text = value;
        }

        public string labelMgmtId
        {
            get => lblMgmtId.Text; set => lblMgmtId.Text = value;
        }

        public string labelInOut
        {
            get => lblInOut.Text; set => lblInOut.Text = value;
        }

        public void setFloorGateLabel(string floor, string gate)
        {
            lblFloorGate.Text = floor + " / " + gate;
        }

        /*
        public string locationLabel
        {
            get => locationLbl.Text; set => locationLbl.Text = value;
        }
        */
        public void shrinkTableLayout()
        {
            //tableLayoutPanel1.RowStyles.Insert(0, new RowStyle(SizeType.Percent, 70));
            //tableLayoutPanel1.RowStyles.Insert(1, new RowStyle(SizeType.Percent, 30));
        }

        public void setOriginTableLayout()
        {
            //tableLayoutPanel1.RowStyles.Insert(0, new RowStyle(SizeType.Percent, 60));
            //tableLayoutPanel1.RowStyles.Insert(1, new RowStyle(SizeType.Percent, 40));
        }
    }
}
