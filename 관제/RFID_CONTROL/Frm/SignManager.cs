using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RFID_CONTROLLER.Frm
{
    public partial class SignManager : Form
    {
        public SignManager()
        {
            InitializeComponent();
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            MainController.isOtherWindowOpen = false;
            this.Close();
        }

        private void SignManager_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Dispose();
        }

        private string m_strUserName = "";
        private string m_strUserID = "";
        public string GetSignID()
        {
            return m_strUserID;
        }
        public string GetSignName()
        {
            return m_strUserName;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                if (textBoxID.Editor.Text != "" && textBoxPW.Editor.Text != "")
                {
                    Dictionary<string, object> aData = new Dictionary<string, object>();
                    aData.Add("WORKDIV", "100REQ");
                    aData.Add("USER_ID", textBoxID.Editor.Text);
                    aData.Add("PWD", textBoxPW.Editor.Text);

                    JObject response = TCPClient.request(aData);

                    if ((string)response["RESULT"] == "0")
                    {
                        DialogResult = DialogResult.OK;
                    }
                    else
                    {
                        MessageBox.Show("로그인 정보를 확인하세요.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception 발생!");
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void SignManager_Load(object sender, EventArgs e)
        {
            MainController.isOtherWindowOpen = true;
        }

        private void textBoxPW_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                if (e.KeyCode == Keys.Enter)
                {
                    if (textBoxID.Editor.Text != "" && textBoxPW.Editor.Text != "")
                    {
                        Dictionary<string, object> aData = new Dictionary<string, object>();
                        aData.Add("WORKDIV", "100REQ");
                        aData.Add("USER_ID", textBoxID.Editor.Text);
                        aData.Add("PWD", textBoxPW.Editor.Text);

                        JObject response = TCPClient.request(aData);

                        if ((string)response["RESULT"] == "0")
                        {
                            DialogResult = DialogResult.OK;
                        }
                        else
                        {
                            MessageBox.Show("로그인 정보를 확인하세요.");
                        }
                    }
                }
            } catch(Exception ex)
            {
                Console.WriteLine("Exception 발생!");
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }
    }
}
