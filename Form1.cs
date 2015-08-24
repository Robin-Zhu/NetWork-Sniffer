using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpPcap;
using PacketDotNet;
using System.Threading;

namespace MySniffer
{
	public partial class MyForm : Form
	{
		private CaptureDeviceList devices;           //网卡列表
		public ICaptureDevice device;                //选中网卡

        //每个包的七元组属性
        private string No;
		private string srcIP; 
		private string dstIP;
		private string protocal;
		private string Length;
		private string Time_arrival;
		private string Info = "";

        private string Payload_data;                //数据包内容（保存printHex()函数输出的内容）
		private int selectedDeviceIndex;            //选中网卡的序号
		private int packet_counter = 0;             //抓到的包数
		

		private bool is_started = false;            //是否开始抓包
        private bool is_loadFlie = false;           //是否打开了pcap文件
        private bool is_reassembled = false;        //是否点击了报文重组按钮

		private List<RawCapture> packet_save = new List<RawCapture>();            //保存抓到的每个包
        private List<RawCapture> filtered_packets = new List<RawCapture>();       //保存符合过滤条件的包
		private List<List<string>> attributes_save = new List<List<string>>();    //保存抓到的每个包的七元组属性

        List<string> data_hex_list = new List<string>();                          //保存抓到的每个包的16进制数据（用于数据包查询）                  
        List<string> filtered_data_hex_list = new List<string>();                 //保存符合过滤条件的每个包的16进制数据（用于数据包查询）    

        private List<string> filtered_payload_data = new List<string>();          //保存符合过滤条件的每个包的内容(printHex()函数输出的内容)
		
        private Dictionary<string, List<IPv4Packet>> reassembled_save = new Dictionary<string, List<IPv4Packet>>();     //字典，索引为分片包的ID，数据为同一ID下的各个分片包
        private Dictionary<string, List<string>> reassembled_no = new Dictionary<string, List<string>>();               //字典，索引为分片包的ID，数据为同一ID下的各个分片包序号

        private FilterForm FilterForm = new FilterForm();       //新建过滤窗口的实例



		public MyForm()
		{
			InitializeComponent();
		}


        //保存抓到的每个包以及每个包的七元组属性
		public void savePackets(RawCapture p)
		{
			packet_save.Add(p);
			List<string> packet_attribute = new List<string>();
			packet_attribute.Add(this.No);
			packet_attribute.Add(this.Time_arrival);
			packet_attribute.Add(this.srcIP);
			packet_attribute.Add(this.dstIP);
			packet_attribute.Add(this.protocal);
			packet_attribute.Add(this.Length);
			packet_attribute.Add(this.Info);
			packet_attribute.Add(this.Payload_data);
			attributes_save.Add(packet_attribute);
		}


        //重新开始抓包或者打开pcap文件前重置各变量
		public void resetView()
		{
			packet_save.Clear();
			attributes_save.Clear();
			data_hex_list.Clear();
			filtered_data_hex_list.Clear();
            reassembled_no.Clear();
            reassembled_save.Clear();

			this.packet_counter = 0;
		}


        //处理每个到达的包
		public void device_OnPacketArrival(Object sender, CaptureEventArgs e)
		{
			this.packet_counter++;
			var p = e.Packet;
			var handling_packet = PacketDotNet.Packet.ParsePacket(p.LinkLayerType, p.Data);
			this.No = this.packet_counter.ToString();
			this.Length = p.Data.Length.ToString();
			DateTime time = p.Timeval.Date;
			this.Time_arrival = time.Hour + ":" + time.Minute + ":" + time.Second + ":" + time.Millisecond;
			this.Payload_data = handling_packet.PrintHex();

			this.protocal = "Unknown";      //若不属于任何已知协议，则认为该包是Unknown
            this.Info = "";

			var ARP_packet = (ARPPacket)handling_packet.Extract(typeof(ARPPacket));
			if (ARP_packet != null)
			{
				this.protocal = "ARP";
				this.srcIP = ARP_packet.SenderHardwareAddress.ToString();
				this.dstIP = ARP_packet.TargetHardwareAddress.ToString();
                this.Info = setArpInfo(ARP_packet);
			}


			var IP_packet = (IpPacket)handling_packet.Extract(typeof(IpPacket));
			if (IP_packet != null)
			{
				this.srcIP = IP_packet.SourceAddress.ToString();
				this.dstIP = IP_packet.DestinationAddress.ToString();

                var TCP_packet = (TcpPacket)handling_packet.Extract(typeof(TcpPacket));
                if (TCP_packet != null)
                {
                    this.protocal = "TCP";
                    this.Info = setTcpInfo(TCP_packet);
                }

                var UDP_packet = (UdpPacket)handling_packet.Extract(typeof(UdpPacket));
                if (UDP_packet != null)
                {
                    this.protocal = "UDP";
                    this.Info = setUdpInfo(UDP_packet);
                }

                var ICMP_packet = (ICMPv4Packet)handling_packet.Extract(typeof(ICMPv4Packet));
                if (ICMP_packet != null)
                {
                    this.protocal = "ICMP";

                    var IPv4 = (IPv4Packet)handling_packet.Extract(typeof(IPv4Packet));
                    string MF = Convert.ToString(IPv4.FragmentFlags, 2).PadLeft(3, '0').Substring(2, 1);
                    int OFF = IPv4.FragmentOffset;

                    this.Info = setIcmpInfo(ICMP_packet, MF, OFF);
                }
                var IGMP_packet = (IGMPv2Packet)handling_packet.Extract(typeof(IGMPv2Packet));
                if (IGMP_packet != null)
                {
                    this.protocal = "IGMP";
                    this.Info = setIgmpInfo(IGMP_packet);
                }

			}

            //save_data_hex(FilterForm.filter_chosen, handling_packet);
			savePackets(p);
			

            //没有选择过滤
			if (!FilterForm.filter_chosen)
			{
				save_data_hex(false, handling_packet);
				addRow(this.No, this.Time_arrival, this.srcIP, this.dstIP, this.protocal, this.Length, this.Info);
			}

            //如果选择了过滤，显示已抓到的符合条件的包（startFilter函数）后，对每个新抓到的包判断是否符合过滤条件，符合则输出
            else      
			{
				if (FilterForm.protocal_chosen.Contains(this.protocal))
				{
					if (FilterForm.src_ip == "" || FilterForm.src_ip == this.srcIP)
					{
						if (FilterForm.dst_ip == "" || FilterForm.dst_ip == this.dstIP)
						{
							filtered_payload_data.Add(this.Payload_data);
                            filtered_packets.Add(p);
							save_data_hex(true, handling_packet);

							addRow(this.No, this.Time_arrival, this.srcIP, this.dstIP, this.protocal, this.Length, this.Info);
						}
					}
				}
			}
		}


        //ARP协议的Info
        private string setArpInfo(ARPPacket ARP)
        {
            string senderIP = ARP.SenderProtocolAddress.ToString();
            string targetIP = ARP.TargetProtocolAddress.ToString();
            string targetMac = ARP.TargetHardwareAddress.ToString();
            if (senderIP == targetIP)
                return "Gratuitous ARP for " + senderIP;
            else if (ARP.Operation.ToString() == "Request")
                return "Who has " + targetIP + "?  Tell " + senderIP;
            else if (ARP.Operation.ToString() == "Response")
                return targetIP + " is at " + targetMac;
            else
                return "Unknown state";
        }

        
        //TCP协议的Info
        private string setTcpInfo(TcpPacket TCP)
        {
            string info = "";
            string flagsList = "[";
            flagsList += TCP.Fin ? "FIN, " : "";
            flagsList += TCP.Syn ? "SYN, " : "";
            flagsList += TCP.Rst ? "RST, " : "";
            flagsList += TCP.Psh ? "PSH, " : "";
            flagsList += TCP.Ack ? "ACK, " : "";
            flagsList += TCP.Urg ? "URG, " : "";
            flagsList += TCP.ECN ? "ECN, " : "";
            flagsList += TCP.CWR ? "CWR]" : "";
            if (flagsList.EndsWith(", "))
            {
                flagsList = flagsList.Remove(flagsList.Length - 2);
                flagsList += "]";
            }
            info = TCP.SourcePort + "->" + TCP.DestinationPort + " " + flagsList + " Seq=" + TCP.SequenceNumber + " Ack=" + TCP.AcknowledgmentNumber + " win=" + TCP.WindowSize;

            return info;
        }


        //UDP协议的Info
        private string setUdpInfo(UdpPacket UDP)
        {
            return  "Source port: " + UDP.SourcePort + " Destination port: " + UDP.DestinationPort;
        }


        //ICMP协议的Info
        private string setIcmpInfo(ICMPv4Packet ICMP, string MF, int OFF)
        {
            //string type = ICMP.Header[0].ToString("D");
            if (isFragment(MF, OFF) && OFF != 0)
                return "Fragment packet";
            else if (isFragment(MF, OFF) && OFF == 0)
            {
                switch (ICMP.Header[0].ToString("D"))
                {
                    case "8":
                        return "Fragment packet. Echo (ping) request  id=0x" + ICMP.ID.ToString("X4") + ",  seq=" + ICMP.Sequence;
                    case "0":
                        return "Fragment packet. Echo (ping) reply  id=0x" + ICMP.ID.ToString("X4") + ",  seq=" + ICMP.Sequence;
                    case "3":
                        return "Fragment packet. Destination unreachable";
                    default:
                        return "Fragment packet. Unknown type/code";
                }
            }
            else
            {
                switch (ICMP.Header[0].ToString("D"))
                {
                    case "8":
                        return "Echo (ping) request  id=0x" + ICMP.ID.ToString("X4") + ",  seq=" + ICMP.Sequence;
                    case "0":
                        return "Echo (ping) reply  id=0x" + ICMP.ID.ToString("X4") + ",  seq=" + ICMP.Sequence;
                    case "3":
                        return "Destination unreachable";
                    default:
                        return "Unknown type/code";
                }
            }
            
        }


        //IGMP协议的Info
        private string setIgmpInfo(IGMPv2Packet IGMP)
        {
            string type = IGMP.Type.ToString();
            string groupAddress = IGMP.GroupAddress.ToString();

            if (type == "MembershipQuery" && IGMP.GroupAddress.ToString() == "0.0.0.0")
                return "Membership Query, general";
            else if (type == "MembershipQuery")
                return "Membership Query, specific for group " + groupAddress;
            else if (type == "MembershipReportIGMPv2")
                return "Membership Report group " + groupAddress;
            else if (type == "LeaveGroup")
                return "Leave Group " + groupAddress;
            else
                return "Unknown state";
        }


        //保存包的16进制数据（用于数据包查询）
		private void save_data_hex(bool is_filtered, Packet packet)
		{
			string data_hex_str = "";           //16进制字节组成的字符串
			foreach (byte b in packet.Bytes)
			{
				data_hex_str += b.ToString("X");
			}
			if (is_filtered)
				filtered_data_hex_list.Add(data_hex_str);
			else
				data_hex_list.Add(data_hex_str);
		}


        //将包的七元组属性显示出来
		private void addRow(string No, string Time, string Source, string Destination, string Protocal, string Length, string Info)
		{
			string[] set = {No, Time, Source, Destination, Protocal, Length, Info};
			ListViewItem row = new ListViewItem(set);
			this.listView.Items.Add(row);
            //this.listView.Items[listView.Items.Count - 1].EnsureVisible();    //设置滚动条始终在最下面

            //设置不同协议的包的背景色
			switch (Protocal)
			{
				case "ARP":
					row.BackColor = Color.LightSalmon;
					break;
				case "TCP":
					row.BackColor = Color.PaleGreen;
					break;
				case "UDP":
					row.BackColor = Color.LightYellow;
					break;
				case "ICMP":
					row.BackColor = Color.LightPink;
					break;
				case "IGMP":
					row.BackColor = Color.LightBlue;
					break;
				default:
					row.BackColor = Color.LightGray;
					break;
			}
		}


        //按照不同协议的包调用不同的树形控件（显示各标志位）
        private void processDetails(RawCapture p)
        {
            var processing_packet = PacketDotNet.Packet.ParsePacket(p.LinkLayerType, p.Data);

            var ARP_packet = (ARPPacket)processing_packet.Extract(typeof(ARPPacket));
            if (ARP_packet != null)
            {
                arpTreeView(ARP_packet);
            }

            var IP_packet = (IpPacket)processing_packet.Extract(typeof(IpPacket));

            if (IP_packet != null)
            {
                var TCP_packet = (TcpPacket)processing_packet.Extract(typeof(TcpPacket));
                if (TCP_packet != null)
                {
                    tcpTreeView(IP_packet);
                }

                var UDP_packet = (UdpPacket)processing_packet.Extract(typeof(UdpPacket));
                if (UDP_packet != null)
                {
                    udpTreeView(IP_packet);
                }

                var ICMP_packet = (ICMPv4Packet)processing_packet.Extract(typeof(ICMPv4Packet));
                if (ICMP_packet != null)
                {
                    icmpTreeView(IP_packet);
                }

                var IGMP_packet = (IGMPv2Packet)processing_packet.Extract(typeof(IGMPv2Packet));
                if (IGMP_packet != null)
                {
                    igmpTreeView(IP_packet);
                }
               
            }
        }


        //在树形控件中加入ipv4节点（由于TCP、UDP、ICMP、IGMP都有IP头，所以写成一个函数）
        private void addIpv4Node(IPv4Packet IPv4)
        {

            TreeNode ipNode = new TreeNode();
            ipNode.Text = "Internet Protocal Version 4, Src: " + IPv4.SourceAddress + ", Dst: " + IPv4.DestinationAddress;
            treeView.Nodes.Add(ipNode);
            ipNode.Nodes.Add(new TreeNode("version: " + IPv4.Version));
            ipNode.Nodes.Add(new TreeNode("Header Length: " + (((int)IPv4.HeaderLength) * 4).ToString() + " bytes"));
            ipNode.Nodes.Add(new TreeNode("Differentiated Services Field: 0x" + IPv4.DifferentiatedServices.ToString("X2")));
            ipNode.Nodes.Add(new TreeNode("Total Length: " + IPv4.TotalLength.ToString()));
            ipNode.Nodes.Add(new TreeNode("Identification: 0x" + IPv4.Id.ToString("X4") + " (" + IPv4.Id.ToString() + ")"));

            TreeNode flagsNode = new TreeNode("Flags: 0x" + IPv4.FragmentFlags.ToString("X2"));
            ipNode.Nodes.Add(flagsNode);
            string reserved = Convert.ToString(IPv4.FragmentFlags, 2).PadLeft(3, '0').Substring(0, 1);
            string dfrag = Convert.ToString(IPv4.FragmentFlags, 2).PadLeft(3, '0').Substring(1, 1);
            string mfrag = Convert.ToString(IPv4.FragmentFlags, 2).PadLeft(3, '0').Substring(2, 1);
            flagsNode.Nodes.Add(new TreeNode(reserved + "... .... = Reserved bit: " + ((reserved == "0") ? "Not set" : "Set")));
            flagsNode.Nodes.Add(new TreeNode("." + dfrag + ".. .... = Don't fragment: " + ((dfrag == "0") ? "Not set" : "Set")));
            flagsNode.Nodes.Add(new TreeNode(".." + mfrag + ". .... = More fragments: " + ((mfrag == "0") ? "Not set" : "Set")));

            ipNode.Nodes.Add(new TreeNode("Fragment offset: " + IPv4.FragmentOffset));
            ipNode.Nodes.Add(new TreeNode("Time to live: " + IPv4.TimeToLive));
            ipNode.Nodes.Add(new TreeNode("protocal: " + IPv4.Protocol));
            ipNode.Nodes.Add(new TreeNode("Header checksum: 0x" + IPv4.Checksum.ToString("X4")));
            ipNode.Nodes.Add(new TreeNode("Source: " + IPv4.SourceAddress));
            ipNode.Nodes.Add(new TreeNode("Destination: " + IPv4.DestinationAddress));
        }


        //在树形控件中加入ipv4节点（由于TCP、UDP、ICMP、IGMP都有IP头，所以写成一个函数）
        private void addIpv6Node(IPv6Packet IPv6)
        {
            TreeNode ipNode = new TreeNode();
            ipNode.Text = "Internet Protocal Version 6, Src: " + IPv6.SourceAddress + ", Dst: " + IPv6.DestinationAddress;
            treeView.Nodes.Add(ipNode);

            ipNode.Nodes.Add(new TreeNode("Version: " + IPv6.Version));
            ipNode.Nodes.Add(new TreeNode("Traffic class: 0x" + IPv6.TrafficClass.ToString("X8")));
            ipNode.Nodes.Add(new TreeNode("Flowlabel: 0x" + IPv6.FlowLabel.ToString("X8")));
            ipNode.Nodes.Add(new TreeNode("Payload length: " + IPv6.PayloadLength));
            ipNode.Nodes.Add(new TreeNode("Next header: " + IPv6.NextHeader.ToString()));
            ipNode.Nodes.Add(new TreeNode("Hop Limit: " + IPv6.HopLimit));
            ipNode.Nodes.Add(new TreeNode("Source: " + IPv6.SourceAddress));
            ipNode.Nodes.Add(new TreeNode("Destination" + IPv6.DestinationAddress));
        }


        //在树形控件中加入arp节点
        private void arpTreeView(ARPPacket ARP)
        {
            TreeNode arpNode = new TreeNode();
            arpNode.Text = "Address Resolution Protocal (" + ARP.Operation.ToString() + ")";
            treeView.Nodes.Add(arpNode);

            arpNode.Nodes.Add(new TreeNode("Hardware type: " + ARP.HardwareAddressType + " (" + ARP.Header[0].ToString("D") + ARP.Header[1].ToString("D") + ")"));
            arpNode.Nodes.Add(new TreeNode("Protocal type: " + ARP.ProtocolAddressType + " (0x" + ARP.Header[2].ToString("X2") + ARP.Header[3].ToString("X2") + ")"));
            arpNode.Nodes.Add(new TreeNode("Hardware size: " + ARP.HardwareAddressLength));
            arpNode.Nodes.Add(new TreeNode("Protocal size: " + ARP.ProtocolAddressLength));
            arpNode.Nodes.Add(new TreeNode("Opcode: " + ARP.Operation + " (" + ARP.Header[6].ToString("D") + ARP.Header[7].ToString("D") + ")"));
            arpNode.Nodes.Add(new TreeNode("Sender Mac address: " + ARP.SenderHardwareAddress));
            arpNode.Nodes.Add(new TreeNode("Sender IP address: " + ARP.SenderProtocolAddress));
            arpNode.Nodes.Add(new TreeNode("Target Mac address: " + ARP.TargetHardwareAddress));
            arpNode.Nodes.Add(new TreeNode("Target IP address: " + ARP.TargetProtocolAddress));
            this.treeView.ExpandAll();
        }


        //在树形控件中加入tcp节点
        private void tcpTreeView(IpPacket p)
        {
            var IPv4 = (IPv4Packet)p.Extract(typeof(IPv4Packet));
            if (IPv4 != null)
            {
                addIpv4Node(IPv4);
            }
            else
            {
                var IPv6 = (IPv6Packet)p.Extract(typeof(IPv6Packet));
                addIpv6Node(IPv6);
            }

            var TCP = (TcpPacket)p.Extract(typeof(TcpPacket));

            TreeNode tcpNode = new TreeNode();
            tcpNode.Text = "Transmission Control Protocal, Src Port: " + TCP.SourcePort + ", Dst Port: " + TCP.DestinationPort + ", Seq: " + TCP.SequenceNumber.ToString() + ", Ack: " + TCP.Ack.CompareTo(false);
            treeView.Nodes.Add(tcpNode);
            tcpNode.Nodes.Add(new TreeNode("Source Port: " + TCP.SourcePort));
            tcpNode.Nodes.Add(new TreeNode("Destination Port: " + TCP.DestinationPort));
            tcpNode.Nodes.Add(new TreeNode("Sequence number: " + TCP.SequenceNumber));
            tcpNode.Nodes.Add(new TreeNode("Acknowledgement: " + TCP.AcknowledgmentNumber));
            tcpNode.Nodes.Add(new TreeNode("Header Length: " + (TCP.DataOffset * 4).ToString() + " bytes"));

            TreeNode allFlags = new TreeNode();
            allFlags.Text = System.Convert.ToString(TCP.AllFlags, 2).PadLeft(12, '0') + " = Flags: 0x" + TCP.Header[12].ToString("X").Substring(1, 1).PadLeft(1, '0') + TCP.Header[13].ToString("X").PadLeft(2, '0');
            tcpNode.Nodes.Add(allFlags);
            allFlags.Nodes.Add(new TreeNode("0000 00.. .... = Reserved: Not set"));
            allFlags.Nodes.Add(new TreeNode(".... .." + TCP.Urg.CompareTo(false) + ". .... = Urgent: " + ((TCP.Urg) ? "Set" : "Not set")));
            allFlags.Nodes.Add(new TreeNode(".... ..." + TCP.Ack.CompareTo(false) + " .... = Acknowledgement: " + ((TCP.Ack) ? "Set" : "Not set")));
            allFlags.Nodes.Add(new TreeNode(".... .... " + TCP.Psh.CompareTo(false) + "... = Push: " + ((TCP.Psh) ? "Set" : "Not set")));
            allFlags.Nodes.Add(new TreeNode(".... .... ." + TCP.Rst.CompareTo(false) + ".. = Reset: " + ((TCP.Rst) ? "Set" : "Not set")));
            allFlags.Nodes.Add(new TreeNode(".... .... .." + TCP.Syn.CompareTo(false) + ". = Syn: " + ((TCP.Syn) ? "Set" : "Not set")));
            allFlags.Nodes.Add(new TreeNode(".... .... ..." + TCP.Fin.CompareTo(false) + " = Fin: " + ((TCP.Fin) ? "Set" : "Not set")));

            tcpNode.Nodes.Add(new TreeNode("Window size value: " + TCP.WindowSize));
            tcpNode.Nodes.Add(new TreeNode("Checksum: 0x" + TCP.Checksum.ToString("X4")));
            tcpNode.Nodes.Add(new TreeNode("Urgent Pointer: " + TCP.UrgentPointer));
            this.treeView.ExpandAll();
        }

        
        //在树形控件中加入udp节点
        private void udpTreeView(IpPacket p)
        {
            var IPv4 = (IPv4Packet)p.Extract(typeof(IPv4Packet));
            if (IPv4 != null)
            {
                addIpv4Node(IPv4);
            }
            else
            {
                var IPv6 = (IPv6Packet)p.Extract(typeof(IPv6Packet));
                addIpv6Node(IPv6);
            }
            
            var UDP = (UdpPacket)p.Extract(typeof(UdpPacket));

            TreeNode udpNode = new TreeNode();
            udpNode.Text = "User Datagram Protocal, Src Port: " + UDP.SourcePort + ", Dst Port: " + UDP.DestinationPort;
            treeView.Nodes.Add(udpNode);

            udpNode.Nodes.Add(new TreeNode("Source Port: " + UDP.SourcePort));
            udpNode.Nodes.Add(new TreeNode("Destination Port: " + UDP.DestinationPort));
            udpNode.Nodes.Add(new TreeNode("Length: " + UDP.Length));
            udpNode.Nodes.Add(new TreeNode("Checksum: 0x" + UDP.Checksum.ToString("X4")));
            this.treeView.ExpandAll();
        }


        //在树形控件中加入icmp节点
        private void icmpTreeView(IpPacket p)
        {
            var IPv4 = (IPv4Packet)p.Extract(typeof(IPv4Packet));
            if (IPv4 != null)
            {
                addIpv4Node(IPv4);
            }

            string MF = Convert.ToString(IPv4.FragmentFlags, 2).PadLeft(3, '0').Substring(2, 1);
            int OFF = IPv4.FragmentOffset;

            var ICMP = (ICMPv4Packet)p.Extract(typeof(ICMPv4Packet));

            TreeNode icmpNode = new TreeNode();
            icmpNode.Text = "Internet Control Meaasge Protocal";
            treeView.Nodes.Add(icmpNode);

            string type = ICMP.Header[0].ToString("D");
            string code = ICMP.Header[1].ToString("D");
            string description = "";

            //如果是分片包且不是第一个，输出See it's first fragment
            if (isFragment(MF, OFF) && OFF != 0)
            {
                type = "See it's first fragment";
                code = "See it's first fragment";
            }
            else
            {
                if (type == "0")
                    description = " (Echo (ping) reply)";
                else if (type == "8")
                    description = " (Echo (ping) requst)";
                else
                    description = "";
            }
            
            icmpNode.Nodes.Add(new TreeNode("Type: " + type + description));
            icmpNode.Nodes.Add(new TreeNode("Code: " + code));
            icmpNode.Nodes.Add(new TreeNode("Checksum: 0x" + ICMP.Checksum.ToString("X4")));
            this.treeView.ExpandAll();
        }


        //在树形控件中加入igmp节点
        private void igmpTreeView(IpPacket p)
        {
            var IPv4 = (IPv4Packet)p.Extract(typeof(IPv4Packet));
            if (IPv4 != null)
            {
                addIpv4Node(IPv4);
            }

            var IGMP = (IGMPv2Packet)p.Extract(typeof(IGMPv2Packet));

            TreeNode igmpNode = new TreeNode();
            igmpNode.Text = "Internet Group Management Protocal";
            treeView.Nodes.Add(igmpNode);

            igmpNode.Nodes.Add(new TreeNode("Type: " + IGMP.Type + " (0x" + IGMP.Header[0].ToString("X2") + ")"));
            igmpNode.Nodes.Add(new TreeNode("Max Resp Time: " + (Convert.ToDouble(IGMP.MaxResponseTime) / 10.0).ToString("0.0") + " sec" + " (0x" + IGMP.Header[1].ToString("X2") + ")"));
            igmpNode.Nodes.Add(new TreeNode("Header checksum: 0x" + IGMP.Checksum.ToString("X4")));
            igmpNode.Nodes.Add(new TreeNode("Multicast Asddress: " + IGMP.GroupAddress));
            this.treeView.ExpandAll();
        }


        //主窗口加载时的一些初始化
		private void MyForm_Load(object sender, EventArgs e)
		{
			stopButton.Enabled = false;
            restartButton.Enabled = false;
            cancelRecombineButton.Enabled = false;
            showDataBox.BackColor = Color.White;
			Control.CheckForIllegalCrossThreadCalls = false;
			this.devices = CaptureDeviceList.Instance;
			deviceComboBox.Items.AddRange(this.devices.Select(g=>g.Description).ToArray());

            //使用c#的委托机制，过滤窗口点击确定后调用主窗口的startFilter函数
			this.FilterForm.startFilter += new DelegateStartFilter(startFilter);
		}


        //该函数利用委托机制绑定到过滤窗口上
        //过滤窗口中点击确定后调用该函数开始过滤
		public void startFilter()
		{
			int index = 0;
			if (this.device != null)
			{
				this.device.StopCapture();
				this.device.Close();
			}
			listView.Items.Clear();
            treeView.Nodes.Clear();
			showDataBox.Clear();
			filtered_payload_data.Clear();
            filtered_packets.Clear();
			filtered_data_hex_list.Clear();

            this.listView.BeginUpdate();   //与listView.EndUpdate()合用，将所有信息一次性显示到listView中,防止屏幕不断闪烁，同时加快显示速度

            //遍历保存列表，选出符合过滤条件的包
			foreach (var a_packet in attributes_save)
			{
				if (FilterForm.protocal_chosen.Contains(a_packet[4]))
				{
					if (FilterForm.src_ip == "" || FilterForm.src_ip == a_packet[2])
					{
						if (FilterForm.dst_ip == "" || FilterForm.dst_ip == a_packet[3])
						{
							filtered_payload_data.Add(a_packet[7]);
                            filtered_packets.Add(packet_save[index]);

							var packet = PacketDotNet.Packet.ParsePacket(packet_save[index].LinkLayerType, packet_save[index].Data);
							save_data_hex(true,packet);
                            
							addRow(a_packet[0], a_packet[1], a_packet[2], a_packet[3], a_packet[4], a_packet[5], a_packet[6]);
						}
					}
				}
				index++;
			}

            this.listView.EndUpdate();

            //过滤完之前抓到的包后如果没有点暂停则继续抓包，显示符合过滤条件的包
			if (!startButton.Enabled)
			{
				this.device.OnPacketArrival += new SharpPcap.PacketArrivalEventHandler(device_OnPacketArrival);
				int readTimeOut = 1000;
				this.device.Open(DeviceMode.Promiscuous, readTimeOut);
				this.device.StartCapture();
			}
		}


        //选择/改变抓包的网卡
		private void deviceComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (this.is_started && !this.is_loadFlie)
			{
				this.device.StopCapture();
				this.device.Close();
				startButton.Enabled = true;
				stopButton.Enabled = false;
				MessageBox.Show("变更网卡");
				listView.Items.Clear();
                treeView.Nodes.Clear();
				showDataBox.Clear();
				this.packet_counter = 0;
				this.packet_save.Clear();
				this.attributes_save.Clear();

                //改变抓包的网卡后过滤窗口回复初始状态
				FilterForm.init_changed_device();
			}

			this.selectedDeviceIndex = deviceComboBox.SelectedIndex;
			this.device = this.devices[this.selectedDeviceIndex];
		}


        //点击开始抓包按钮
		private void startButton_Click(object sender, EventArgs e)
		{
			if (deviceComboBox.Text == "")
				MessageBox.Show("请选择网卡");
			else
			{
				if (is_loadFlie)   //读pcap文件时点击开始抓包可以开始正常抓包
				{
					resetView();
					listView.Items.Clear();
                    treeView.Nodes.Clear();
					showDataBox.Clear();
					FilterForm.init_changed_device();
				}
				startButton.Enabled = false;
				stopButton.Enabled = true;
				saveButton.Enabled = false;
                restartButton.Enabled = false;
				this.is_started = true;
                deviceComboBox.Enabled = false;
				this.device.OnPacketArrival += new SharpPcap.PacketArrivalEventHandler(device_OnPacketArrival);
				int readTimeOut = 1000;
				this.device.Open(DeviceMode.Promiscuous, readTimeOut);
				this.device.StartCapture();
			}
		}


        //点击停止抓包按钮
		private void stopButton_Click(object sender, EventArgs e)
		{
			startButton.Enabled = true;
			stopButton.Enabled = false;
			saveButton.Enabled = true;
            restartButton.Enabled = true;
            deviceComboBox.Enabled = true;
			this.device.StopCapture();
			this.device.Close();
		}


        //点击重新开始抓包按钮
        private void restartButton_Click(object sender, EventArgs e)
        {
            resetView();
            listView.Items.Clear();
            treeView.Nodes.Clear();
            showDataBox.Clear();
            FilterForm.init_changed_device();
            startButton.Enabled = false;
            stopButton.Enabled = true;
            saveButton.Enabled = false;
            this.is_started = true;
            this.device.OnPacketArrival += new SharpPcap.PacketArrivalEventHandler(device_OnPacketArrival);
            int readTimeOut = 1000;
            this.device.Open(DeviceMode.Promiscuous, readTimeOut);
            this.device.StartCapture();
        }


        //点击保存文件按钮
        private void saveButton_Click(object sender, EventArgs e)
        {
            if (!stopButton.Enabled)
            {
                string savePath = "";

                //windows保存文件的对话框
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.InitialDirectory = "c:\\";
                saveFileDialog.Filter = "pcap文件(*.pcap)|*.pcap";
                saveFileDialog.DefaultExt = ".pcap";
                saveFileDialog.RestoreDirectory = true;
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    savePath = saveFileDialog.FileName;
                }
                if (savePath != "")
                {
                    SharpPcap.LibPcap.CaptureFileWriterDevice captureFilerWriter = new SharpPcap.LibPcap.CaptureFileWriterDevice(savePath, System.IO.FileMode.OpenOrCreate);
                    foreach (var packet in packet_save)
                    {
                        captureFilerWriter.Write(packet);
                    }
                    MessageBox.Show("文件已保存在" + savePath);
                }


            }
        }


        //点击打开文件按钮
        private void openButton_Click(object sender, EventArgs e)
        {
            string loadPath = "";

            //windows打开文件的对话框
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = "c:\\";
            openFileDialog.Filter = "pcap文件(*.pcap)|*.pcap";
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                loadPath = openFileDialog.FileName;
                startButton.Enabled = true;
                stopButton.Enabled = false;
                restartButton.Enabled = false;

                if (this.device != null)
                {
                    this.device.StopCapture();
                    this.device.Close();
                }

                listView.Items.Clear();
                treeView.Nodes.Clear();
                showDataBox.Clear();
            }
            if (loadPath != "")
            {
                ICaptureDevice captureReaderDevice = new SharpPcap.LibPcap.CaptureFileReaderDevice(loadPath);
                captureReaderDevice.OnPacketArrival += new PacketArrivalEventHandler(device_OnPacketArrival);
                this.is_loadFlie = true;
                deviceComboBox.Enabled = true;
                resetView();
                FilterForm.init_changed_device();
                captureReaderDevice.Capture();      //开始读取文件
            }
        }


        //选中包列表中的某一行，显示相应包的树形标志位和包内容
		private void listView_SelectedIndexChanged(object sender, EventArgs e)
		{
            int focusedIndex = listView.FocusedItem.Index;
			showDataBox.Clear();
            treeView.Nodes.Clear();

            //如果选中包为分片包则调用处理分片的函数
            if (is_reassembled)
            {
                if (FilterForm.filter_chosen)
                    processReassembled_F(focusedIndex);
                else
                    processReassembled(focusedIndex);
            }
            else
            {
                if (FilterForm.filter_chosen)
                {
                    showDataBox.AppendText(filtered_payload_data[focusedIndex]);
                    showDataBox.SelectionStart = 0;
                    showDataBox.ScrollToCaret();

                    processDetails(filtered_packets[focusedIndex]);
                }
                else
                {
                    showDataBox.AppendText(attributes_save[focusedIndex][7]);
                    showDataBox.SelectionStart = 0;
                    showDataBox.ScrollToCaret();

                    processDetails(packet_save[focusedIndex]);

                }
            }
            
		}


        //将选中的分片包以及与其同一ID的分片包存入字典，该函数用于选择了过滤时，作用与下一个函数相同
        //原理是遍历保存下来的包的列表寻找ID相同的包
        private void processReassembled_F(int focusedIndex)
        {
            var p = filtered_packets[focusedIndex];
            var focused_packet = PacketDotNet.Packet.ParsePacket(p.LinkLayerType, p.Data);
            var ipv4 = (IPv4Packet)focused_packet.Extract(typeof(IPv4Packet));
            int k = 0;

            if (ipv4 != null)
            {
                string ID = ipv4.Id.ToString("X4");
                string MF = Convert.ToString(ipv4.FragmentFlags, 2).PadLeft(3, '0').Substring(2, 1);
                int OFF = ipv4.FragmentOffset;

                if (!isFragment(MF, OFF))
	            {
		            showDataBox.AppendText(filtered_payload_data[focusedIndex]);
                    showDataBox.SelectionStart = 0;
                    showDataBox.ScrollToCaret();

                    processDetails(filtered_packets[focusedIndex]);
	            }
                else
	            {   //如果未保存改选中项，加入字典后调用输出函数，如果已保存，直接调用输出函数
                    processDetails(filtered_packets[focusedIndex]);

                    if (!reassembled_save.Keys.Contains(ID))   //如果没保存过则将与其同一ID的所有包保存
                    {
                        reassembled_save.Add(ID, new List<IPv4Packet>());
                        reassembled_no.Add(ID, new List<string>());
                        foreach (var item in filtered_packets)
                        {
                            var t = PacketDotNet.Packet.ParsePacket(item.LinkLayerType, item.Data);
                            var ip = (IPv4Packet)t.Extract(typeof(IPv4Packet));
                            if (ip != null)
                            {
                                //找到一个包后按偏移量的大小顺序插入字典的key为个ID的列表中
                                if (ip.Id.ToString("X4") == ID)
                                {
                                    int len = reassembled_save[ID].Count;
                                    reassembled_no[ID].Add(listView.Items[k].SubItems[0].Text);
                                    if (len == 0)
                                        reassembled_save[ID].Add(ip);
                                    else
                                    {                                      
                                        if (ip.FragmentOffset > reassembled_save[ID][len - 1].FragmentOffset)
                                            reassembled_save[ID].Add(ip);
                                        else
                                            for (int i = len - 1; i >= 0; i--)
                                            {
                                                if (ip.FragmentOffset < reassembled_save[ID][i].FragmentOffset)
                                                {
                                                    reassembled_save[ID].Insert(i, ip);
                                                    break;
                                                }
                                            }
                                    }
                                }   
                            }

                            k++;
                        }
                    }

                    showReassembled(ID);
	            }
                
            }
            else
            {
                showDataBox.AppendText(filtered_payload_data[focusedIndex]);
                showDataBox.SelectionStart = 0;
                showDataBox.ScrollToCaret();

                processDetails(filtered_packets[focusedIndex]);
            }

        }


        //将选中的分片包以及与其同一ID的分片包存入字典，该函数用于未选择过滤时，作用与上一个函数相同
        //原理是遍历保存下来的包的列表寻找ID相同的包
        private void processReassembled(int focusedIndex)
        {
            var p = packet_save[focusedIndex];
            var focused_packet = PacketDotNet.Packet.ParsePacket(p.LinkLayerType, p.Data);
            var ipv4 = (IPv4Packet)focused_packet.Extract(typeof(IPv4Packet));
            int k = 0;

            if (ipv4 != null)
            {
                string ID = ipv4.Id.ToString("X4");
                string MF = Convert.ToString(ipv4.FragmentFlags, 2).PadLeft(3, '0').Substring(2, 1);
                int OFF = ipv4.FragmentOffset;

                if (!isFragment(MF, OFF))
                {
                    showDataBox.AppendText(attributes_save[focusedIndex][7]);
                    showDataBox.SelectionStart = 0;
                    showDataBox.ScrollToCaret();

                    processDetails(packet_save[focusedIndex]);
                }
                else
                {   //如果未保存改选中项，加入字典后调用输出函数，如果已保存，直接调用输出函数
                    processDetails(packet_save[focusedIndex]);

                    if (!reassembled_save.Keys.Contains(ID))    //如果没保存过则将与其同一ID的所有包保存
                    {
                        reassembled_save.Add(ID, new List<IPv4Packet>());
                        reassembled_no.Add(ID, new List<string>());
                        foreach (var item in packet_save)
                        {
                            var t = PacketDotNet.Packet.ParsePacket(item.LinkLayerType, item.Data);
                            var ip = (IPv4Packet)t.Extract(typeof(IPv4Packet));
                            if (ip != null)
                            {
                                //找到一个包后按偏移量的大小顺序插入字典的key为个ID的列表中
                                if (ip.Id.ToString("X4") == ID)
                                {
                                    int len = reassembled_save[ID].Count;
                                    reassembled_no[ID].Add(listView.Items[k].SubItems[0].Text);
                                    if (len == 0)
                                        reassembled_save[ID].Add(ip);
                                    else
                                    {
                                        if (ip.FragmentOffset > reassembled_save[ID][len - 1].FragmentOffset)
                                            reassembled_save[ID].Add(ip);
                                        else
                                            for (int i = len - 1; i >= 0; i--)
                                            {
                                                if (ip.FragmentOffset < reassembled_save[ID][i].FragmentOffset)
                                                {
                                                    reassembled_save[ID].Insert(i, ip);
                                                    break;
                                                }
                                            }
                                    }
                                }
                            }

                            k++;
                        }
                    }

                    showReassembled(ID);
                }

            }
            else
            {
                showDataBox.AppendText(attributes_save[focusedIndex][7]);
                showDataBox.SelectionStart = 0;
                showDataBox.ScrollToCaret();

                processDetails(packet_save[focusedIndex]);

            }
            
        }


        //根据More fragments和Offset判断是否是分片包
        private bool isFragment(string MF, int OFF)
        {
            if (MF == "1" || OFF != 0)
                return true;
            else
                return false;
        }


        //显示分片包重组后的信息
        private void showReassembled(string ID)
        {
            string data_hex = "";            
            string ascii = "";

            showDataBox.AppendText("该包为分片包。\r\n\r\n分片包序号: ");
            foreach (var no in reassembled_no[ID])
            {
                showDataBox.AppendText(no + " ");
            }

            foreach (var p in reassembled_save[ID])
            {
                string bt = "";
                foreach (var b in p.PayloadPacket.Bytes)
                {
                    data_hex += b.ToString("X2") + " ";
                    int dec = Convert.ToInt32(b.ToString("X2"), 16);                  
                    if (dec < 33 || dec > 126)   //ASCII码小于33或大于126的为控制字符等不可显示字符，这里显示为"."
                        bt += ".";
                    else
                    {
                        Byte[] arr = System.BitConverter.GetBytes(dec);
                        bt += System.Text.Encoding.ASCII.GetString(arr).ToArray()[0];
                    }                   
                }
                ascii += bt;
            }
            showDataBox.AppendText("\r\n\r\n重组后的分片包完整数据共有: " + data_hex.Length / 3 + " 字节\r\n");
            showDataBox.AppendText("\r\n分片包完整数据: \r\n\r\n");
            showDataBox.AppendText(data_hex);

            showDataBox.AppendText("*******************************************************************************************************************************\r\n");

            showDataBox.AppendText(ascii);

            showDataBox.SelectionStart = 0;
            showDataBox.ScrollToCaret();

        }


        //点击过滤窗口的确定按钮
		private void filterButton_Click(object sender, EventArgs e)
		{
            this.FilterForm.loadStatus();   //加载过滤窗口上一次点击确定后状态
			this.FilterForm.Show();
		}


        //点击数据包查询按钮
		private void searchButton_Click(object sender, EventArgs e)
		{
			string search = "";
			foreach (ListViewItem row in listView.Items)
			{
				if (row.BackColor == Color.DarkBlue)
				{
                    //将每一行恢复为原来的颜色
					row.ForeColor = Color.Black;
					switch (row.SubItems[4].Text)
					{
						case "ARP":
							row.BackColor = Color.LightSalmon;
							break;
						case "TCP":
							row.BackColor = Color.PaleGreen;
							break;
						case "UDP":
							row.BackColor = Color.LightYellow;
							break;
						case "ICMP":
							row.BackColor = Color.LightPink;
							break;
						case "IGMP":
							row.BackColor = Color.LightBlue;
							break;
                        case "IPv6":
                            row.BackColor = Color.LightGray;
                            break;
						default:
							row.BackColor = Color.White;
							break;
					}
				}
			}
			if (searchText.Text != "")
			{
				search = searchText.Text;
                //调用真正的搜索函数
				search_in_packets(search);
			}
		}


        //将要查询的字符串转换为16进制字节后进行搜索
		public void search_in_packets(string search)
		{
			int i;
			string search_hex_str = "";

			byte[] hex_array = System.Text.Encoding.ASCII.GetBytes(search);

			foreach (byte b in hex_array)
				search_hex_str += b.ToString("x");

            //查询到以后以深蓝色高亮显示该包所在行
			if (FilterForm.filter_chosen)
			{
				for (i = filtered_data_hex_list.Count - 1; i >= 0; i--)
				{
					if (filtered_data_hex_list[i].Contains(search_hex_str))
					{
						listView.Items[i].ForeColor = Color.White;
						listView.Items[i].BackColor = Color.DarkBlue;
						listView.Items[i].EnsureVisible();      //增进用户体验，跳转到查询到的第一个包所在行
					}
				}
			}
			else
			{
				for (i = data_hex_list.Count - 1; i >= 0; i--)
				{
					if (data_hex_list[i].Contains(search_hex_str))
					{
						listView.Items[i].ForeColor = Color.White;
						listView.Items[i].BackColor = Color.DarkBlue;
                        listView.Items[i].EnsureVisible();      //增进用户体验，跳转到查询到的第一个包所在行
					}
				}
			}
		}


        //点击报文重组按钮
        private void recombineButton_Click(object sender, EventArgs e)
        {
            this.is_reassembled = true;
            recombineButton.Enabled = false;
            cancelRecombineButton.Enabled = true;
        }


        //点击取消重组按钮
        private void cancelRecombineButton_Click(object sender, EventArgs e)
        {
            this.is_reassembled = false;
            recombineButton.Enabled = true;
            cancelRecombineButton.Enabled = false;
        }


        //点击主窗口的关闭按钮时要手动停止抓包，否则进程会驻留在windows后台
        private void MyForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (is_started)
            {
                this.device.StopCapture();
                this.device.Close();
            }
        }


	}
}
