using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using Simcorp.IMS.Phone;
using Simcorp.IMS.Phone.Output;

namespace SMSPhone {

    public delegate string FormatDelegate(SMSMessage message);
    public delegate void AddMessage(SMSMessage message);
    public delegate void UpdateProgBar();

    public partial class SMSPhone : Form {
        private SimCorpMobile simCorp;
        private FormatDelegate Formatter;
        private AddMessage MsgAdder;
        private UpdateProgBar UpdProgBar;
        private string Sender;
        private MsgStorage MsgData=new MsgStorage();
        private string allSenderItem = "All";
        private bool SMSButtonClicked = false;
        private bool bCharge = false;
        private bool bDischarge = true;


        public SMSPhone() {
            simCorp = new SimCorpMobile(new ListViewOutput(MessageListView));
            MsgAdder += AddMsgToTextBox;
            simCorp.MsgStor.MsgAdded += OnMsgAdded;
            simCorp.MsgStor.MsgRemoved += OnMsgRemoved;

            InitializeComponent();
            FormatItem defaultItem = new FormatItem("None", StringFormatter.FormatNone);
            FormatComboBox.Items.Add(defaultItem);
            FormatComboBox.Items.Add(new FormatItem("Start with DateTime", StringFormatter.FormatStartDateTime));
            FormatComboBox.Items.Add(new FormatItem("End with DateTime", StringFormatter.FormatEndDateTime));
            FormatComboBox.Items.Add(new FormatItem("Lowercase", StringFormatter.FormatLower));
            FormatComboBox.Items.Add(new FormatItem("Uppercase", StringFormatter.FormatUpper));
            FormatComboBox.SelectedItem = defaultItem;

            SenderComboBox.Items.Add(allSenderItem);
            SenderComboBox.SelectedItem = allSenderItem;
            OrAndCheckBox.Checked = true;

            ChargeProgressBar.Value = simCorp.Battery.GetCurrentCharge();
            UpdProgBar += OnChargeChanged; 
            new Thread(BatteryDischarge).Start();
            simCorp.Battery.ChargeChanged += OnChargeLevelChanged;
        }

        /// <summary>
        /// Thread methods
        /// </summary>
        public void SMSGenerator() {
            while (SMSButtonClicked) {
                simCorp.GenerateSMS();
                Thread.Sleep(1000);
            }
        }

        public void BatteryDischarge() {
            while (bDischarge) {
                simCorp.Discharge(10);
                Thread.Sleep(1000);
            }
        }

        public void BatteryCharge() {
            while (bCharge) {
                simCorp.Charge(30);
                Thread.Sleep(1000);
            }
        }

        private void OnChargeLevelChanged() {
            try {
                Invoke(UpdProgBar, new object[] { });
            } catch { bCharge = bDischarge = false; }
        }

        private void OnChargeChanged() {
            ChargeProgressBar.Value = (int)simCorp.Battery.GetCurrentCharge();
        }

        private void OnMsgAdded(SMSMessage message) {
            try {
                Invoke(MsgAdder, new object[] { message });
            } catch { SMSButtonClicked = false; }    
        }

        public void AddMsgToTextBox(SMSMessage message) {
            if (!SenderComboBox.Items.Contains(message.User)) {
                SenderComboBox.Items.Add(message.User);
            }
            DisplayMessages(new List<SMSMessage> { message });
        }

        private void OnMsgRemoved(SMSMessage message) {
            DisplayAll(simCorp.MsgStor.MsgList);
        }

        private void FormatComboBox_SelectedIndexChanged(object sender, EventArgs e) {
            FormatItem itm = (FormatItem)FormatComboBox.SelectedItem;
            Formatter = itm.FormatDel;
            DisplayAll(simCorp.MsgStor.MsgList);
            ChargeProgressBar.Value =  simCorp.Battery.GetCurrentCharge();
        }

        private void SenderComboBox_SelectedIndexChanged(object sender, EventArgs e) {
            Sender = SenderComboBox.SelectedItem.ToString();
            DisplayAll(simCorp.MsgStor.MsgList);
        }

        private void SearchTextBox_TextChanged(object sender, EventArgs e) {
            DisplayAll(simCorp.MsgStor.MsgList);
        }

        private void FromDatePicker_ValueChanged(object sender, EventArgs e) {
            if (FromDatePicker.Value.Date > ToDatePicker.Value.Date) {
                ToDatePicker.Value = FromDatePicker.Value;
            }
            DisplayAll(simCorp.MsgStor.MsgList);
        }

        private void ToDatePicker_ValueChanged(object sender, EventArgs e) {
            if (FromDatePicker.Value.Date > ToDatePicker.Value.Date) {
                    FromDatePicker.Value = ToDatePicker.Value;
            }
            DisplayAll(simCorp.MsgStor.MsgList);
        }

        private void OrAndCheckBox_CheckedChanged(object sender, EventArgs e) {
            DisplayAll(simCorp.MsgStor.MsgList);
        }

        private void DisplayAll(List<SMSMessage> msgList) {
            MessageListView.Items.Clear();
            DisplayMessages(msgList);
        }

        private void DisplayMessages(List<SMSMessage> msgList) {
            string searchText = SearchTextBox.Text.ToString();
            DateTime fromDate = FromDatePicker.Value.Date;
            DateTime toDate = ToDatePicker.Value.Date;
            bool andcond = OrAndCheckBox.Checked;
            string fltrSender = (Sender == allSenderItem) ? String.Empty : Sender;

            IEnumerable<SMSMessage> fltrdList = MsgStorage.RetrieveMessages(msgList, fltrSender, searchText, fromDate, toDate, andcond);

            foreach (var message in fltrdList) {
                ShowMessage(message);
            }
        }

        private void ShowMessage(SMSMessage message) {
            string formattedMessage = Formatter(message);
            MessageListView.Items.Add(new ListViewItem(new[] { message.User, formattedMessage }));
        }

        private class FormatItem {
            public string Name;
            public FormatDelegate FormatDel;
            public FormatItem(string name, FormatDelegate formatDel) {
                Name = name;
                FormatDel = formatDel;
            }
            public override string ToString() {
                return Name;
            }
        }

        private void SMSButton_Click(object sender, EventArgs e) {
            SMSButtonClicked = !SMSButtonClicked;
            if ( SMSButtonClicked ) {
                SMSButton.Text = "Stop send SMS";
                new Thread(SMSGenerator).Start();
            } else {
                SMSButton.Text = "Send SMS";
            }
        }


        private void ChargeButton_Click(object sender, EventArgs e) {
            bCharge = !bCharge;
            bDischarge = !bDischarge;
            if (bCharge) {
                ChargeButton.Text = "Stop charging";
                new Thread(BatteryCharge).Start();
            } else {
                ChargeButton.Text = "Charge";
                new Thread(BatteryDischarge).Start();
            }
        }
    }
}
