using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Timers;

namespace MySniffer
{
    public delegate void DelegateStartFilter();
    
    //过滤窗口
    public partial class FilterForm : Form
    {
        public string[] protocal_chosen = { "ARP", "TCP", "UDP", "ICMP", "IGMP" };  //选择的协议
        public string[] protocal_default = { "ARP", "TCP", "UDP", "ICMP", "IGMP"};  //默认选择的协议      
        public bool[] saved_status = { true, true, true, true, true };              //保存各协议勾选状态
        public string src_ip;           //填写的源地址
        public string dst_ip;           //填写的目的地址
        private string src_save = "";   //保存源地址内容
        private string dst_save = "";   //保存目的地址内容
        public bool filter_chosen;      //是否点击了确定

        private string regexText = "^\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}$";  //正则表达式，判断源IP、目的IP是否输入正确

        public FilterForm()
        {
            filter_chosen = false;
            InitializeComponent();
        }

        //利用委托机制将主程序的startFilter函数绑定到过滤窗口
        public event DelegateStartFilter startFilter;


        //重新选择网卡后的初始化
        public void init_changed_device()
        {
            this.protocal_chosen = protocal_default;
            this.src_ip = "";
            this.dst_ip = "";
            this.src_save = "";
            this.dst_save = "";
            for (int i = 0; i < saved_status.Length - 1; i++)
                saved_status[i] = true;
            this.filter_chosen = false;
            ArpCheckBox.Checked = true;
            TcpCheckBox.Checked = true;
            UdpCheckBox.Checked = true;
            IcmpCheckBox.Checked = true;
            IgmpCheckBox.Checked = true;
            SrcIpTextBox.Text = "";
            DstIpTextBox.Text = "";
        }


        //点击了确定按钮
        private void ConfirmButton_Click(object sender, EventArgs e)
        {
            this.protocal_chosen[0] = ArpCheckBox.Checked ? "ARP" : "";
            this.protocal_chosen[1] = TcpCheckBox.Checked ? "TCP" : "";
            this.protocal_chosen[2] = UdpCheckBox.Checked ? "UDP" : "";
            this.protocal_chosen[3] = IcmpCheckBox.Checked ? "ICMP" : "";
            this.protocal_chosen[4] = IgmpCheckBox.Checked ? "IGMP" : "";
            this.src_ip = SrcIpTextBox.Text;
            this.dst_ip = DstIpTextBox.Text;
            this.filter_chosen = true;
            saveStatus();
            this.Hide();

            startFilter();
        }


        //保存各个过滤选项的状态，下次显示过滤窗口时加载状态
        //如果改变了过滤内容但点了取消，下一次显示窗口时显示的应该还是改变前的
        private void saveStatus()
        {
            saved_status[0] = ArpCheckBox.Checked;
            saved_status[1] = TcpCheckBox.Checked;
            saved_status[2] = UdpCheckBox.Checked;
            saved_status[3] = IcmpCheckBox.Checked;
            saved_status[4] = IgmpCheckBox.Checked;

            src_save = SrcIpTextBox.Text;
            dst_save = DstIpTextBox.Text;
        }


        //显示窗口时加载保存的状态
        public void loadStatus()
        {
            ArpCheckBox.Checked = saved_status[0];
            TcpCheckBox.Checked = saved_status[1];
            UdpCheckBox.Checked = saved_status[2];
            IcmpCheckBox.Checked = saved_status[3];
            IgmpCheckBox.Checked = saved_status[4];

            SrcIpTextBox.Text = src_save;
            DstIpTextBox.Text = dst_save;
        }


        //点击右上角关闭按钮，
        private void FilterForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;    //如果不加这一句,点击关闭后就释放了窗口对象，无法再正常弹出窗口
            this.Hide();
        }


        //点击取消按钮
        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.Hide();
        }


        //输入源地址时实时判断输入的地址是否符合格式要求
        private void SrcIpTextBox_TextChanged(object sender, EventArgs e)
        {
            if (SrcIpTextBox.Text == "" || Regex.IsMatch(SrcIpTextBox.Text, this.regexText))
                SrcIpTextBox.BackColor = Color.White;
            else
                SrcIpTextBox.BackColor = Color.LightPink;

            if (SrcIpTextBox.BackColor == Color.LightPink || DstIpTextBox.BackColor == Color.LightPink)
                ConfirmButton.Enabled = false;
            else
                ConfirmButton.Enabled = true;
        }


        //输入源地址时实时判断输入的地址是否符格式要求
        private void DstIpTextBox_TextChanged(object sender, EventArgs e)
        {
            if (DstIpTextBox.Text == "" || Regex.IsMatch(DstIpTextBox.Text, this.regexText))
                DstIpTextBox.BackColor = Color.White;
            else
                DstIpTextBox.BackColor = Color.LightPink;

            if (SrcIpTextBox.BackColor == Color.LightPink || DstIpTextBox.BackColor == Color.LightPink)
                ConfirmButton.Enabled = false;
            else
                ConfirmButton.Enabled = true;
        }
        
    }
}
