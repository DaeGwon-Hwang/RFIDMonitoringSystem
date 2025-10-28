using Newtonsoft.Json.Linq;
using RFID_CONTROLLER.Collections;
using RFID_CONTROLLER.Controls.Grid;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Windows.Forms;

namespace RFID_CONTROLLER.Frm
{
    public partial class EquipmentManager : Form
    {
        public EquipmentManager()
        {
            InitializeComponent();
            gridSetting();
            tabReader_Enter();
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            MainController.isOtherWindowOpen = false;
            this.Close();
        }
        #region 고정형리더 관리
        DataList<Data<string>> gateDatalist = new DataList<Data<string>>();
        DataList<Data<string>> antDatalist = new DataList<Data<string>>()
            {
                new Data<string>(){{"cmm_cd", "1"}, {"cmm_nm", "1안테나" }},
                new Data<string>(){{"cmm_cd", "2"}, {"cmm_nm", "2안테나" }},
                new Data<string>(){{"cmm_cd", "3"}, {"cmm_nm", "3안테나" }},
                new Data<string>(){{"cmm_cd", "4"}, {"cmm_nm", "4안테나" }}
            };
        DataList<Data<string>> notiDatalist = new DataList<Data<string>>()
            {
                new Data<string>(){{"cmm_cd", "AC" }, {"cmm_nm", "AC COMMNAD" }},
                new Data<string>(){{"cmm_cd", "SC" }, {"cmm_nm", "NET NOTFY" }}
            };
        private void tabReader_Enter()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                Dictionary<string, object> aData = new Dictionary<string, object>();
                aData.Add("WORKDIV", "118REQ");
                aData.Add("CRUD_TP", "R");

                JObject response = TCPClient.request(aData);
                JArray rows = (JArray)response["rows"];
                if (rows != null)
                {
                    for (int i = 0; i < rows.Count; i++)
                    {
                        JObject row = (JObject)rows[i];
                        string gateId = row["GATE_ID"].ToString();
                        string gateNm = row["GATE_NM"].ToString();
                        gateDatalist.Add(new Data<string>() { { "cmm_cd", gateId }, { "cmm_nm", gateNm } });
                    }
                }
                cbReaderGateID.SetItems(gateDatalist, "cmm_cd", "cmm_nm");
                cbReaderAntCnt.SetItems(antDatalist, "cmm_cd", "cmm_nm");
                cbReaderNotifyType.SetItems(notiDatalist, "cmm_cd", "cmm_nm");

                btnReaderAdd_Click(null, null);

                gridView1.ClearData();

                aData = new Dictionary<string, object>();
                aData.Add("WORKDIV", "119REQ");
                aData.Add("CRUD_TP", "R");
                aData.Add("EQ_TYPE", "GAT");

                response = TCPClient.request(aData);
                rows = (JArray)response["rows"];

                if (rows != null)
                {
                    DataList<Data<string>> datalist = new DataList<Data<string>>();
                    for (int i = 0; i < rows.Count; i++)
                    {
                        JObject row = (JObject)rows[i];

                        var data = new Data<string>();
                        data["SEQ"] = (i + 1).ToString();
                        data["EQ_ID"] = (string)row["EQ_ID"];
                        data["EQ_NM"] = (string)row["EQ_NM"];
                        data["GATE_ID_NM"] = (string)row["GATE_ID_NM"];
                        data["IP_ADDR"] = Utils.parseIPAddress((string)row["IP_ADDR"]);
                        data["NETMASK"] = Utils.parseIPAddress((string)row["NETMASK"]);
                        data["GATEWAY"] = Utils.parseIPAddress((string)row["GATEWAY"]);
                        data["ATTENU"] = (string)row["ATTENU"];
                        data["MAC_ADDR"] = (string)row["MAC_ADDR"];
                        data["ANT_CNT"] = (string)row["ANT_CNT"];
                        data["ANT_SEQ"] = (string)row["ANT_SEQ"];
                        data["NOTFY_TYPE_NM"] = (string)row["NOTFY_TYPE_NM"];
                        data["GATE_ID"] = (string)row["GATE_ID"];
                        data["NOTFY_TYPE"] = (string)row["NOTFY_TYPE"];
                        datalist.Add(data);
                    }
                    gridView1.SetDataList(datalist);
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

        private void btnReaderAdd_Click(object sender, EventArgs e)
        {
            txtReaderEQID.Editor.Text = txtReaderEQNM.Editor.Text = txtReaderIPAddr.Editor.Text = txtReaderNetMask.Editor.Text = txtReaderGateWay.Editor.Text = "";
            txtReaderAttenu.Editor.Text = txtReaderMacAddr.Editor.Text = txtReaderAntSeq.Editor.Text = "";
            cbReaderGateID.Editor.SelectedIndex = cbReaderAntCnt.Editor.SelectedIndex = cbReaderNotifyType.Editor.SelectedIndex = 0;

            txtReaderEQID.Enabled = true;
            txtReaderEQID.Focus();
        }

        private void btnReaderSave_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                if (txtReaderEQID.Editor.Text == "" || txtReaderEQNM.Editor.Text == "")
                {
                    MessageBox.Show("필수 항목이 누락되었습니다.");
                    return;
                }

                if (txtReaderEQID.Enabled)
                {
                    bool flag = false;

                    for (int i = 0; i < gridView3.RowCount; i++)
                    {
                        if (gridView1.GetRowCellValue(i, "EQ_ID").ToString() == txtReaderEQID.Text)
                        {
                            flag = true;
                            break;
                        }
                    }

                    if (flag)
                    {
                        MessageBox.Show("장비ID가 중복됩니다.");
                        return;
                    }
                }

                Dictionary<string, object> aData = new Dictionary<string, object>();

                aData.Add("WORKDIV", "119REQ");
                aData.Add("CRUD_TP", txtReaderEQID.Enabled ? "C" : "U");
                aData.Add("EQ_ID", txtReaderEQID.Editor.Text);
                aData.Add("EQ_NM", txtReaderEQNM.Editor.Text);
                aData.Add("EQ_TYPE", "GAT");
                aData.Add("GATE_ID", cbReaderGateID.GetValue());
                aData.Add("IP_ADDR", txtReaderIPAddr.Editor.Text);
                aData.Add("NETMASK", txtReaderNetMask.Editor.Text);
                aData.Add("GATEWAY", txtReaderGateWay.Editor.Text);
                aData.Add("MAC_ADDR", txtReaderMacAddr.Editor.Text);
                aData.Add("ATTENU", txtReaderAttenu.Editor.Text);
                aData.Add("ANT_CNT", cbReaderAntCnt.GetValue());
                aData.Add("ANT_SEQ", txtReaderAntSeq.Editor.Text);
                aData.Add("NOTFY_TYPE", cbReaderNotifyType.GetValue());
                aData.Add("NOTFY_STT", "ATV");

                JObject response = TCPClient.request(aData);

                if ((int)response["RESULT"] == 0)
                {
                    MessageBox.Show("저장되었습니다.");

                    tabReader_Enter();
                }
                else
                {
                    String msg = response["ERROR"].ToString();
                    if (msg == "dup")
                    {
                        msg = "키 값이 중복입니다.";
                    }
                    MessageBox.Show(msg == null ? "처리도중 에러가 발생했습니다." : msg);
                }
            } catch(Exception ex)
            {
                Console.WriteLine("Exception 발생!");
            }
            finally { Cursor = Cursors.Default; }
            
        }

        private void gridView1_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            int selectedIndex = e.FocusedRowHandle;
            if (selectedIndex < 0) return;

            txtReaderEQID.SetValue(gridView1.GetRowCellValue(selectedIndex, "EQ_ID").ToString());
            txtReaderEQNM.SetValue(gridView1.GetRowCellValue(selectedIndex, "EQ_NM").ToString());

            for (int i=0; i<cbReaderGateID.Editor.Properties.Items.Count; i++)
            {
                object item = cbReaderGateID.Editor.Properties.Items[i];
                if (item.ToString() == gridView1.GetRowCellValue(selectedIndex, "GATE_ID_NM").ToString())
                {
                    cbReaderGateID.Editor.SelectedIndex = i;
                }
            }
            //cbReaderGateID.Editor.EditValue = gridView1.GetRowCellValue(selectedIndex, "GATE_ID_NM").ToString();

            txtReaderIPAddr.SetValue(gridView1.GetRowCellValue(selectedIndex, "IP_ADDR").ToString());
            txtReaderNetMask.SetValue(gridView1.GetRowCellValue(selectedIndex, "NETMASK").ToString());
            txtReaderGateWay.SetValue(gridView1.GetRowCellValue(selectedIndex, "GATEWAY").ToString());
            txtReaderAttenu.SetValue(gridView1.GetRowCellValue(selectedIndex, "ATTENU").ToString());
            txtReaderMacAddr.SetValue(gridView1.GetRowCellValue(selectedIndex, "MAC_ADDR").ToString());

            for(int i=0; i<antDatalist.Count; i++)
            {
                var data = antDatalist[i];
                if (data["cmm_cd"] == gridView1.GetRowCellValue(selectedIndex, "ANT_CNT").ToString())
                {
                    cbReaderAntCnt.Editor.SelectedIndex = i + 1;
                }
            }

            //cbReaderAntCnt.Editor.EditValue = gridView1.GetRowCellValue(selectedIndex, "ANT_CNT").ToString();
            txtReaderAntSeq.SetValue(gridView1.GetRowCellValue(selectedIndex, "ANT_SEQ").ToString());
            
            for(int i=0; i<notiDatalist.Count; i++)
            {
                var data = notiDatalist[i];
                if (data["cmm_cd"] == gridView1.GetRowCellValue(selectedIndex, "NOTFY_TYPE").ToString())
                {
                    cbReaderNotifyType.Editor.SelectedIndex = i + 1;
                }
            }
            
            
            //cbReaderNotifyType.Editor.EditValue = gridView1.GetRowCellValue(selectedIndex, "NOTFY_TYPE").ToString();
            txtReaderEQID.Enabled = false;
        }

        private void btnReaderDel_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                if (txtReaderEQID.Editor.Text == "")
                {
                    MessageBox.Show("삭제할 항목을 선택하세요.");
                    return;
                }

                if (MessageBox.Show("선택 항목을 삭제하시겠습니까?", "삭제 확인", MessageBoxButtons.YesNo
                    , MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No)
                {
                    return;
                }


                Dictionary<string, object> aData = new Dictionary<string, object>();

                aData.Add("WORKDIV", "119REQ");
                aData.Add("CRUD_TP", "D");
                aData.Add("EQ_ID", txtReaderEQID.Editor.Text);

                JObject response = TCPClient.request(aData);

                if ((int)response["RESULT"] == 0)
                {
                    MessageBox.Show("삭제되었습니다.");

                    tabReader_Enter();
                }
                else
                {
                    MessageBox.Show(response["ERROR"] == null ? "처리도중 에러가 발생했습니다." : response["ERROR"].ToString());
                }
            } catch(Exception ex)
            {
                Console.WriteLine("Exception 발생!");
            } finally { Cursor = Cursors.Default; }
            
        }
        #endregion

        #region RFID장비관리
        private void tabRFID_Enter()
        {
            DataList<Data<string>>rfidEqDatalist = new DataList<Data<string>>()
            {
                new Data<string>(){{"cmm_cd", "PDA" }, {"cmm_nm", "PDA" } },
                new Data<string>(){{"cmm_cd", "CNT" }, {"cmm_nm", "정수점검기" } },
                new Data<string>(){{"cmm_cd", "PRT" }, {"cmm_nm", "태그발행기" } }
            };
            rbRFIDEQType.SetItems(rfidEqDatalist, "cmm_cd", "cmm_nm");

            btnRFIDSearch_Click(null, null);
        }

        private void btnRFIDSearch_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                gridView2.ClearData();
                Dictionary<string, object> aData = new Dictionary<string, object>();
                aData.Add("WORKDIV", "119REQ");
                aData.Add("CRUD_TP", "R");
                aData.Add("EQ_TYPE", rbRFIDEQType.GetValue());

                JObject response = TCPClient.request(aData);
                JArray rows = (JArray)response["rows"];

                if (rows != null)
                {
                    DataList<Data<string>> datalist = new DataList<Data<string>>();
                    for (int i = 0; i < rows.Count; i++)
                    {
                        JObject row = (JObject)rows[i];
                        var data = new Data<string>();
                        data["SEQ"] = (i + 1).ToString();
                        data["EQ_ID"] = (string)row["EQ_ID"];
                        data["EQ_NM"] = (string)row["EQ_NM"];
                        data["MAC_ADDR"] = Utils.parseIPAddress((string)row["MAC_ADDR"]);
                        data["IP_ADDR"] = Utils.parseIPAddress((string)row["IP_ADDR"]);
                        data["NETMASK"] = Utils.parseIPAddress((string)row["NETMASK"]);
                        data["GATEWAY"] = Utils.parseIPAddress((string)row["GATEWAY"]);
                        data["EQ_TYPE"] = (string)row["EQ_TYPE"];
                        datalist.Add(data);
                    }
                    gridView2.SetDataList(datalist);
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

        private void btnRFIDEQAdd_Click(object sender, EventArgs e)
        {
            txtRFIDEQID.Editor.Text = txtRFIDEQNM.Editor.Text = txtRFIDMacAddr.Editor.Text = txtRFIDEQIPAddr.Editor.Text = txtRFIDEQNetMask.Editor.Text = txtRFIDEQGateWay.Editor.Text = "";
            rbRFIDEQType.Editor.SelectedIndex = 0;
            txtRFIDEQID.Enabled = true;

            txtRFIDEQID.Focus();
        }

        private void btnRFIDEQSave_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                if (txtRFIDEQID.Editor.Text == "" || txtRFIDEQNM.Editor.Text == "")
                {
                    MessageBox.Show("필수 항목이 누락되었습니다.");
                    return;
                }

                if (txtRFIDEQID.Enabled)
                {
                    bool flag = false;

                    for (int i = 0; i < gridView2.RowCount; i++)
                    {
                        if (gridView2.GetRowCellValue(i, "EQ_ID").ToString() == txtRFIDEQID.Text)
                        {
                            flag = true;
                            break;
                        }
                    }

                    if (flag)
                    {
                        MessageBox.Show("장비ID가 중복됩니다.");
                        return;
                    }
                }

                Dictionary<string, object> aData = new Dictionary<string, object>();

                aData.Add("WORKDIV", "119REQ");
                aData.Add("CRUD_TP", txtRFIDEQID.Enabled ? "C" : "U");
                aData.Add("EQ_ID", txtRFIDEQID.Editor.Text);
                aData.Add("EQ_NM", txtRFIDEQNM.Editor.Text);
                aData.Add("EQ_TYPE", rbRFIDEQType.GetValue());
                aData.Add("IP_ADDR", txtRFIDEQIPAddr.Editor.Text);
                aData.Add("NETMASK", txtRFIDEQNetMask.Editor.Text);
                aData.Add("GATEWAY", txtRFIDEQGateWay.Editor.Text);
                aData.Add("MAC_ADDR", txtRFIDMacAddr.Editor.Text);

                JObject response = TCPClient.request(aData);

                if ((int)response["RESULT"] == 0)
                {
                    MessageBox.Show("저장되었습니다.");

                    btnRFIDSearch_Click(null, null);
                    tabRFID_Enter();
                }
                else
                {
                    String msg = response["ERROR"].ToString();

                    if (msg == "dup")
                    {
                        msg = "키 값이 중복입니다.";
                    }

                    MessageBox.Show(msg == null ? "처리도중 에러가 발생했습니다." : msg);
                }
            } catch(Exception ex)
            {
                Console.WriteLine("Exception 발생!");
            }
            finally { Cursor = Cursors.Default; }
        }

        private void gridView2_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            int selectedIndex = e.FocusedRowHandle;
            if (selectedIndex < 0) return;

            txtRFIDEQID.SetValue(gridView2.GetRowCellValue(selectedIndex, "EQ_ID").ToString());
            txtRFIDEQNM.SetValue(gridView2.GetRowCellValue(selectedIndex, "EQ_NM").ToString());
            txtRFIDMacAddr.SetValue(gridView2.GetRowCellValue(selectedIndex, "MAC_ADDR").ToString());
            txtRFIDEQIPAddr.SetValue(gridView2.GetRowCellValue(selectedIndex, "IP_ADDR").ToString());
            txtRFIDEQNetMask.SetValue(gridView2.GetRowCellValue(selectedIndex, "NETMASK").ToString());
            txtRFIDEQGateWay.SetValue(gridView2.GetRowCellValue(selectedIndex, "GATEWAY").ToString());
            txtRFIDEQID.Enabled = false;
        }

        private void btnRFIDEQDel_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                if (txtRFIDEQID.Editor.Text == "")
                {
                    MessageBox.Show("삭제할 항목을 선택하세요.");
                    return;
                }

                if (MessageBox.Show("선택 항목을 삭제하시겠습니까?", "삭제 확인", MessageBoxButtons.YesNo
                    , MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No)
                {
                    return;
                }


                Dictionary<string, object> aData = new Dictionary<string, object>();

                aData.Add("WORKDIV", "119REQ");
                aData.Add("CRUD_TP", "D");
                aData.Add("EQ_ID", txtRFIDEQID.Editor.Text);

                JObject response = TCPClient.request(aData);

                if ((int)response["RESULT"] == 0)
                {
                    MessageBox.Show("삭제되었습니다.");

                    btnRFIDSearch_Click(null, null);
                    tabRFID_Enter();
                }
                else
                {
                    MessageBox.Show(response["ERROR"] == null ? "처리도중 에러가 발생했습니다." : response["ERROR"].ToString());
                }
            } catch(Exception ex)
            { 
                Console.WriteLine("Exception 발생!");
            } finally { Cursor = Cursors.Default; }
        }

        #endregion

        #region 존관리
        private void tabZone_Enter()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                btnZoneAdd_Click(null, null);

                gridView3.ClearData();

                Dictionary<string, object> aData = new Dictionary<string, object>();

                aData.Add("WORKDIV", "117REQ");
                aData.Add("CRUD_TP", "R");

                JObject response = TCPClient.request(aData);
                JArray rows = (JArray)response["rows"];

                if (rows != null)
                {
                    DataList<Data<string>> datalist = new DataList<Data<string>>();
                    for (int i = 0; i < rows.Count; i++)
                    {
                        JObject row = (JObject)rows[i];
                        var data = new Data<string>();
                        data["SEQ"] = (i + 1).ToString();
                        data["ZONE_ID"] = (string)row["ZONE_ID"];
                        data["ZONE_NM"] = (string)row["ZONE_NM"];
                        data["GATE_NM"] = (string)row["GATE_NM"];
                        datalist.Add(data);
                    }
                    gridView3.SetDataList(datalist);
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

        private void btnZoneAdd_Click(object sender, EventArgs e)
        {
            txtZoneID.Editor.Text = txtZoneNM.Editor.Text = "";
            txtZoneID.Enabled = true;

            txtZoneID.Focus();
        }

        private void btnZoneSave_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                if (txtZoneID.Editor.Text == "" || txtZoneNM.Editor.Text == "")
                {
                    MessageBox.Show("필수 항목이 누락되었습니다.");
                    return;
                }

                if (txtZoneID.Enabled)
                {
                    bool flag = false;

                    for (int i = 0; i < gridView3.RowCount; i++)
                    {
                        if (gridView3.GetRowCellValue(i, "ZONE_ID").ToString() == txtZoneID.Editor.Text)
                        {
                            flag = true;
                            break;
                        }
                    }

                    if (flag)
                    {
                        MessageBox.Show("존ID가 중복됩니다.");
                        return;
                    }
                }

                Dictionary<string, object> aData = new Dictionary<string, object>();
                aData.Add("WORKDIV", "117REQ");
                aData.Add("CRUD_TP", txtZoneID.Enabled ? "C" : "U");
                aData.Add("ZONE_ID", txtZoneID.Editor.Text);
                aData.Add("ZONE_NM", txtZoneNM.Editor.Text);

                JObject response = TCPClient.request(aData);

                if ((int)response["RESULT"] == 0)
                {
                    MessageBox.Show("저장되었습니다.");

                    tabZone_Enter();
                }
                else
                {
                    String msg = response["ERROR"].ToString();

                    if (msg == "dup")
                    {
                        msg = "키 값이 중복입니다.";
                    }

                    MessageBox.Show(msg == null ? "처리도중 에러가 발생했습니다." : msg);
                }
            } catch(Exception ex)
            {
                Console.WriteLine("Exception 발생!");
            } finally { Cursor = Cursors.Default; }
        }

        private void gridView3_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            int selectedIndex = e.FocusedRowHandle;
            if (selectedIndex < 0) return;

            txtZoneID.SetValue(gridView3.GetRowCellValue(selectedIndex, "ZONE_ID").ToString());
            txtZoneNM.SetValue(gridView3.GetRowCellValue(selectedIndex, "ZONE_NM").ToString());

            txtZoneID.Enabled = false;
        }

        private void btnZoneDel_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                if (txtZoneID.Editor.Text == "")
                {
                    MessageBox.Show("삭제할 항목을 선택하세요.");
                    return;
                }

                if (MessageBox.Show("선택 항목을 삭제하시겠습니까?", "삭제 확인", MessageBoxButtons.YesNo
                    , MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No)
                {
                    return;
                }


                Dictionary<string, object> aData = new Dictionary<string, object>();
                aData.Add("WORKDIV", "117REQ");
                aData.Add("CRUD_TP", "D");
                aData.Add("ZONE_ID", txtZoneID.Editor.Text);

                JObject response = TCPClient.request(aData);

                if ((int)response["RESULT"] == 0)
                {
                    MessageBox.Show("삭제되었습니다.");

                    tabZone_Enter();
                }
                else
                {
                    MessageBox.Show(response["ERROR"] == null ? "처리도중 에러가 발생했습니다." : response["ERROR"].ToString());
                }
            } catch(Exception ex)
            {
                Console.WriteLine("Exception 발생!");
            } finally { Cursor = Cursors.Default; }
            
        }

        #endregion

        #region GATE관리
        DataList<Data<string>> zoneDatalist = new DataList<Data<string>>();
        private void tabGate_Enter()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                DataList<Data<string>> gateTypeDatalist = new DataList<Data<string>>()
            {
                new Data<string>(){{"cmm_cd", "NOR" } ,{"cmm_nm", "일반" } },
                new Data<string>(){{"cmm_cd", "CHK" } ,{"cmm_nm", "목지점" } },
                new Data<string>(){{"cmm_cd", "WAT" } ,{"cmm_nm", "감시지점" } }
            };
                cbGateType.SetItems(gateTypeDatalist, "cmm_cd", "cmm_nm");


                Dictionary<string, object> aData = new Dictionary<string, object>();
                aData.Add("WORKDIV", "117REQ");
                aData.Add("CRUD_TP", "R");

                JObject response = TCPClient.request(aData);
                JArray rows = (JArray)response["rows"];

                if (rows != null)
                {
                    for (int i = 0; i < rows.Count; i++)
                    {
                        JObject row = (JObject)rows[i];
                        string zoneId = row["ZONE_ID"].ToString();
                        string zoneNm = row["ZONE_NM"].ToString();
                        zoneDatalist.Add(new Data<string>() { { "cmm_cd", zoneId }, { "cmm_nm", zoneNm } });
                    }
                }
                cbInZnID.SetItems(zoneDatalist, "cmm_cd", "cmm_nm");
                cbOutZnID.SetItems(zoneDatalist, "cmm_cd", "cmm_nm");

                btnGateAdd_Click(null, null);

                gridView4.ClearData();

                aData = new Dictionary<string, object>();
                aData.Add("WORKDIV", "118REQ");
                aData.Add("CRUD_TP", "R");

                response = TCPClient.request(aData);
                rows = (JArray)response["rows"];

                if (rows != null)
                {
                    DataList<Data<string>> datalist = new DataList<Data<string>>();
                    for (int i = 0; i < rows.Count; i++)
                    {
                        JObject row = (JObject)rows[i];
                        var data = new Data<string>();
                        data["SEQ"] = (i + 1).ToString();
                        data["GATE_ID"] = (string)row["GATE_ID"];
                        data["GATE_NM"] = (string)row["GATE_NM"];
                        data["GATE_TYPE_NM"] = (string)row["GATE_TYPE_NM"];
                        data["GATE_TYPE"] = (string)row["GATE_TYPE"];
                        data["USE_SENSOR"] = (string)row["USE_SENSOR"];
                        data["IN_ZN_ID"] = (string)row["IN_ZN_ID"];
                        data["OUT_ZN_ID"] = (string)row["OUT_ZN_ID"];
                        datalist.Add(data);
                    }
                    gridView4.SetDataList(datalist);
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

        private void btnGateSave_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                if (txtGateNM.Editor.Text == "" || txtGateID.Editor.Text == "")
                {
                    MessageBox.Show("필수 항목이 누락되었습니다.");
                    return;
                }

                if (txtGateID.Enabled)
                {
                    bool flag = false;

                    for (int i = 0; i < gridView3.RowCount; i++)
                    {
                        if (gridView3.GetRowCellValue(i, "GATE_ID").ToString() == txtGateID.Editor.Text)
                        {
                            flag = true;
                            break;
                        }
                    }

                    if (flag)
                    {
                        MessageBox.Show("게이트 ID가 중복됩니다.");
                        return;
                    }
                }

                Dictionary<string, object> aData = new Dictionary<string, object>();

                aData.Add("WORKDIV", "118REQ");
                aData.Add("CRUD_TP", txtGateID.Enabled ? "C" : "U");
                aData.Add("GATE_ID", txtGateID.Editor.Text);
                aData.Add("GATE_NM", txtGateNM.Editor.Text);
                aData.Add("GATE_TYPE", cbGateType.GetValue());
                aData.Add("USE_SENSOR", rbUseSensor1.Checked ? "Y" : "N");
                aData.Add("IN_ZN_ID", cbInZnID.GetValue());
                aData.Add("OUT_ZN_ID", cbOutZnID.GetValue());
                JObject response = TCPClient.request(aData);
                if ((int)response["RESULT"] == 0)
                {
                    MessageBox.Show("저장되었습니다.");
                    tabGate_Enter();
                }
                else
                {
                    String msg = response["ERROR"].ToString();
                    if (msg == "dup")
                    {
                        msg = "키 값이 중복입니다.";
                    }
                    MessageBox.Show(msg == null ? "처리도중 에러가 발생했습니다." : msg);
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

        private void btnGateAdd_Click(object sender, EventArgs e)
        {
            txtGateID.Editor.Text = txtGateNM.Editor.Text = "";
            cbGateType.Editor.SelectedIndex = cbInZnID.Editor.SelectedIndex = cbOutZnID.Editor.SelectedIndex = 0;
            rbUseSensor1.Checked = true;
            txtGateID.Enabled = true;

            txtGateID.Focus();
        }

        private void btnGateDel_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                if (txtGateID.Editor.Text == "")
                {
                    MessageBox.Show("삭제할 항목을 선택하세요.");
                    return;
                }

                if (MessageBox.Show("선택 항목을 삭제하시겠습니까?", "삭제 확인", MessageBoxButtons.YesNo
                    , MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.No)
                {
                    return;
                }

                Dictionary<string, object> aData = new Dictionary<string, object>();
                aData.Add("WORKDIV", "118REQ");
                aData.Add("CRUD_TP", "D");
                aData.Add("GATE_ID", txtGateID.Editor.Text);
                JObject response = TCPClient.request(aData);
                if ((int)response["RESULT"] == 0)
                {
                    MessageBox.Show("삭제되었습니다.");
                    tabGate_Enter();
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

        private void gridView4_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            int selectedIndex = e.FocusedRowHandle;
            if (selectedIndex < 0) return;

            txtGateID.SetValue(gridView4.GetRowCellValue(selectedIndex, "GATE_ID").ToString());
            txtGateNM.SetValue(gridView4.GetRowCellValue(selectedIndex, "GATE_NM").ToString());
            //cbGateType.Editor.EditValue = gridView4.GetRowCellValue(selectedIndex, "GATE_TYPE_NM").ToString();

            for(int i=0; i<cbGateType.Editor.Properties.Items.Count; i++)
            {
                object item = cbGateType.Editor.Properties.Items[i];
                if(item.ToString() == gridView4.GetRowCellValue(selectedIndex, "GATE_TYPE_NM").ToString())
                {
                    cbGateType.Editor.SelectedIndex = i;
                }
            }

            if (gridView4.GetRowCellValue(selectedIndex, "USE_SENSOR").ToString() == "Y")
            {
                rbUseSensor1.Checked = true;
            }
            else
            {
                rbUseSensor2.Checked = true;
            }

            for (int i=0; i<zoneDatalist.Count; i++)
            {
                var data = zoneDatalist[i];
                if (data["cmm_cd"] == gridView4.GetRowCellValue(selectedIndex, "IN_ZN_ID").ToString())
                {
                    cbInZnID.Editor.SelectedIndex = i+1;
                }
                if (data["cmm_cd"] == gridView4.GetRowCellValue(selectedIndex, "OUT_ZN_ID").ToString())
                {
                    cbOutZnID.Editor.SelectedIndex = i+1;
                }
            }
            txtGateID.Enabled = false;
        }
        #endregion


        private void gridSetting()
        {
            //그리드 읽기전용
            gridView1.OptionsBehavior.Editable = false;
            gridView2.OptionsBehavior.Editable = false;
            gridView3.OptionsBehavior.Editable = false;
            gridView4.OptionsBehavior.Editable = false;
            //그리드 행 변경 이벤트
            gridView1.FocusedRowChanged += gridView1_FocusedRowChanged;
            gridView2.FocusedRowChanged += gridView2_FocusedRowChanged;
            gridView3.FocusedRowChanged += gridView3_FocusedRowChanged;
            gridView4.FocusedRowChanged += gridView4_FocusedRowChanged;
        }

        private void mainTab_SelectedPageChanged(object sender, DevExpress.XtraTab.TabPageChangedEventArgs e)
        {
            int selectedTabIndex = xtraTabControl1.SelectedTabPageIndex;
            if (selectedTabIndex == 0)
            {
                tabReader_Enter();
            }
            else if (selectedTabIndex == 1)
            {
                tabRFID_Enter();
            }
            else if (selectedTabIndex == 2)
            {
                tabZone_Enter();
            }
            else if (selectedTabIndex == 3)
            {
                tabGate_Enter();
            }
        }

        private void EquipmentManager_Load(object sender, EventArgs e)
        {
            MainController.isOtherWindowOpen = true;
        }
    }
}
