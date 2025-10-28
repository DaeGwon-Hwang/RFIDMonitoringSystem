using Newtonsoft.Json.Linq;
using RFID_CONTROLLER.Collections;
using RFID_CONTROLLER.Controls.Grid;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace RFID_CONTROLLER.Frm
{
    public partial class SWManager : Form
    {
        public SWManager()
        {
            InitializeComponent();
            SWManager_Enter();
        }

        #region 프로그램 관리
        DataList<Data<string>> eqTypeDatalist = new DataList<Data<string>>()
            {
                new Data<string>(){{"cmm_cd", "GAT" } ,{"cmm_nm", "고정형" } },
                new Data<string>(){{"cmm_cd", "PDA" } ,{"cmm_nm", "PDA" } },
                new Data<string>(){{"cmm_cd", "CNT" } ,{"cmm_nm", "정수점검기" } },
                new Data<string>(){{"cmm_cd", "PRT" } ,{"cmm_nm", "태그발행기" } }
            };
        DataList<Data<string>> swTypeDatalist = new DataList<Data<string>>()
            {
                new Data<string>(){{"cmm_cd", "FW" } ,{"cmm_nm", "펌웨어" } },
                new Data<string>(){{"cmm_cd", "SW" } ,{"cmm_nm", "프로그램" } }
            };
        private void SWManager_Enter()
        {
            
            cbEqType.SetItems(eqTypeDatalist, "cmm_cd", "cmm_nm");
            cbSwType.SetItems(swTypeDatalist, "cmm_cd", "cmm_nm");

            //그리드 읽기전용
            gridView1.OptionsBehavior.Editable = false;
            //그리드 행 변경 이벤트
            gridView1.FocusedRowChanged += gridView1_FocusedRowChanged;

            btnSwAdd_Click(null, null);
            btnSWSearch_Click(null, null);
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            MainController.isOtherWindowOpen = false;
            this.Close();
        }

        private void btnSwAdd_Click(object sender, EventArgs e)
        {
            txtSwSeq.Editor.Text = "";
            txtSwNm.Editor.Text = "";
            txtDescp.Editor.Text = "";
            txtFileName.Editor.Text = "";
            cbEqType.Editor.SelectedIndex = 0;
            cbSwType.Editor.SelectedIndex = 0;
            txtSwNm.Focus();
        }

        private void btnSWSearch_Click(object sender, EventArgs e)
        {
            try
            {
                gridView1.ClearData();

                Dictionary<string, object> aData = new Dictionary<string, object>();

                aData.Add("WORKDIV", "120REQ");
                aData.Add("CRUD_TP", "R");
                aData.Add("EQ_TYPE", cbEqType.GetValue());

                JObject response = TCPClient.request(aData);
                JArray rows = (JArray)response["rows"];

                if (rows != null)
                {
                    DataList<Data<string>> datalist = new DataList<Data<string>>();

                    for (int i = 0; i < rows.Count; i++)
                    {
                        JObject row = (JObject)rows[i];

                        var data = new Data<string>();
                        data["SEQ"] = (string)row["SEQ"];
                        data["EQ_TYPE"] = (string)row["EQ_TYPE"];
                        data["EQ_TYPE_NM"] = (string)row["EQ_TYPE_NM"];
                        data["SW_TYPE"] = (string)row["SW_TYPE"];
                        data["SW_TYPE_NM"] = (string)row["SW_TYPE_NM"];
                        data["SW_NM"] = (string)row["SW_NM"];
                        data["SW_FILENAME"] = (string)row["SW_FILENAME"];
                        data["DESCP"] = (string)row["DESCP"];
                        datalist.Add(data);
                    }
                    gridView1.SetDataList(datalist);
                    gridView1.ValidationAll();
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

        private void btnSwSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtSwNm.Editor.Text == "")
                {
                    MessageBox.Show("필수 항목이 누락되었습니다.");
                    return;
                }

                Dictionary<string, object> aData = new Dictionary<string, object>();

                aData.Add("WORKDIV", "120REQ");
                aData.Add("CRUD_TP", txtSwSeq.Editor.Text == "" ? "C" : "U");
                aData.Add("SEQ", txtSwSeq.Editor.Text);
                aData.Add("EQ_TYPE", cbEqType.GetValue());
                aData.Add("SW_TYPE", cbSwType.GetValue());
                aData.Add("SW_NM", txtSwNm.Editor.Text);
                aData.Add("DESCP", txtDescp.Editor.Text);

                if (txtFileName.Editor.Text != "")
                {
                    String result = Utils.fileToBase64(txtFileName.Editor.Text);

                    aData.Add("SW_FILENAME", Path.GetFileName(txtFileName.Editor.Text));
                    aData.Add("SW_FILE", result);
                }

                JObject response = TCPClient.request(aData);

                if ((int)response["RESULT"] == 0)
                {
                    MessageBox.Show("저장되었습니다.");

                    btnSWSearch_Click(null, null);
                    btnSwAdd_Click(null, null);
                }
                else
                {
                    String msg = response["ERROR"].ToString();
                    MessageBox.Show(msg == null ? "처리도중 에러가 발생했습니다." : msg);
                }
            } catch(Exception ex)
            {
                Console.WriteLine("Exception 발생!");
            }
            finally { Cursor = Cursors.Default; }
            
        }

        private void btnFile_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtFileName.SetValue(openFileDialog1.FileName);
            }
        }

        private void gridView1_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            int selectedIndex = e.FocusedRowHandle;
            if (selectedIndex < 0) return;

            for(int i=0; i<eqTypeDatalist.Count; i++)
            {
                var data = eqTypeDatalist[i];
                if (data["cmm_cd"] == gridView1.GetRowCellValue(selectedIndex, "EQ_TYPE_NM").ToString())
                {
                    cbEqType.Editor.SelectedIndex = i + 1;
                }
            }

            for (int i=0; i<swTypeDatalist.Count; i++)
            {
                var data = swTypeDatalist[i];
                if (data["cmm_cd"] == gridView1.GetRowCellValue(selectedIndex, "SW_TYPE_NM").ToString())
                {
                    cbSwType.Editor.SelectedIndex = i + 1;
                }
            }
            txtSwSeq.SetValue(gridView1.GetRowCellValue(selectedIndex, "SEQ").ToString());
            txtSwNm.SetValue(gridView1.GetRowCellValue(selectedIndex, "SW_NM").ToString());
            txtDescp.SetValue(gridView1.GetRowCellValue(selectedIndex, "DESCP").ToString());
        }

        private void btnSwDel_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtSwSeq.Editor.Text == "")
                {
                    MessageBox.Show("삭제할 항목을 선택하세요.");
                    return;
                }

                if (MessageBox.Show("선택 항목을 삭제하시겠습니까?", "삭제 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No)
                {
                    return;
                }

                Dictionary<string, object> aData = new Dictionary<string, object>();

                aData.Add("WORKDIV", "120REQ");
                aData.Add("CRUD_TP", "D");
                aData.Add("SEQ", txtSwSeq.Editor.Text);

                JObject response = TCPClient.request(aData);

                if ((int)response["RESULT"] == 0)
                {
                    MessageBox.Show("삭제되었습니다.");

                    btnSWSearch_Click(null, null);
                    btnSwAdd_Click(null, null);
                }
                else
                {
                    MessageBox.Show(response["ERROR"] == null ? "처리도중 에러가 발생했습니다." : response["ERROR"].ToString());
                }
            } catch(Exception ex)
            {
                Console.WriteLine("Exception 발생!");
            }
            finally { Cursor = Cursors.Default; }
            
        }

        #endregion

        private void SWManager_Load(object sender, EventArgs e)
        {
            MainController.isOtherWindowOpen = true;
        }
    }
}
