using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RFID_CONTROLLER
{
    public partial class StatusPanel : UserControl
    {
        public StatusPanel()
        {
            InitializeComponent();
        }

        [Category("UserDefine")]
        [Description("Caption")]
        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(typeof(string), "title")]
        public string titleLabel
        {
            get => titleLbl.Text; set => titleLbl.Text = value;
        }

        public string errSttLabel
        {
            get => errSttLbl.Text; set => errSttLbl.Text = value;
        }

        public string norSttLabel 
        {
            get => norSttLbl.Text; set => norSttLbl.Text = value;
        }

        public string outSttLabel
        {
            get => outSttLbl.Text; set => outSttLbl.Text = value;
        }

        public string inSttLabel
        {
            get => inSttLbl.Text; set => inSttLbl.Text = value;
        }

        /*
        public string totalEquipLabel
        {
            get => totalEquipLbl.Text; set => totalEquipLbl.Text = value;
        }

        public string operEquipLabel
        {
            get => operEquipLbl.Text; set => operEquipLbl.Text = value;
        }
        */
    }
}
