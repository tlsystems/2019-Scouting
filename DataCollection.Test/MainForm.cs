using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO.Ports;



namespace _2019_Scouting
{
	public partial class MainForm : Form
	{
		SortedList<int, SerialPort> _PortList = new SortedList<int, SerialPort>(8);

		Button[,] _Buttons = new Button[8, 8];
		GroupBox[] _Controllers = new GroupBox[8];

		Timer _PollingTimer;

		public MainForm()
		{
			InitializeComponent();

			FillControllArrays();

			foreach (string portName in System.IO.Ports.SerialPort.GetPortNames())
			{
				Debug.WriteLine(portName);
				if (Convert.ToInt16(portName.Substring(3)) <= 3) continue;

				var port = new SerialPort(portName, 115200, Parity.None, 8, StopBits.One);
				port.Open();
				port.Write("s");
				var csSwitches = port.ReadLine();
				int key = (Convert.ToInt32(csSwitches[3])) - 0x30;

				_PortList.Add(key, port);

				_Controllers[key].Enabled = true;
				_Controllers[key].Text = portName;
				switch (key)
				{
					case 0:
					case 7:
						_Controllers[key].BackColor = Color.GreenYellow;
						break;
					case 1:
					case 2:
					case 3:
						_Controllers[key].BackColor = Color.Red;
						break;
					case 4:
					case 5:
					case 6:
						_Controllers[key].BackColor = Color.RoyalBlue;
						break;
					default:
						break;
				}
			}

			_PollingTimer = new Timer();
			_PollingTimer.Interval = Convert.ToInt16(updnPolling.Value);
			_PollingTimer.Tick += new EventHandler(PollingEvent);
			_PollingTimer.Start();
		}

		private void cbUnoList_SelectedIndexChanged(object sender, EventArgs e)
		{

		}

		private void updnPollingChanged(object sender, EventArgs e)
		{
			if (updnPolling.Value == 11) updnPolling.Value = 10;
			_PollingTimer.Interval = Convert.ToInt16(updnPolling.Value);
		}

		private void FillControllArrays()
		{
			_Buttons[0, 0] = btn01;
			_Buttons[0, 1] = btn02;
			_Buttons[0, 2] = btn03;
			_Buttons[0, 3] = btn04;
			_Buttons[0, 4] = btn05;
			_Buttons[0, 5] = btn06;
			_Buttons[0, 6] = btn07;
			_Buttons[0, 7] = btn08;

			_Buttons[1, 0] = btn11;
			_Buttons[1, 1] = btn12;
			_Buttons[1, 2] = btn13;
			_Buttons[1, 3] = btn14;
			_Buttons[1, 4] = btn15;
			_Buttons[1, 5] = btn16;
			_Buttons[1, 6] = btn17;
			_Buttons[1, 7] = btn18;

			_Buttons[2, 0] = btn21;
			_Buttons[2, 1] = btn22;
			_Buttons[2, 2] = btn23;
			_Buttons[2, 3] = btn24;
			_Buttons[2, 4] = btn25;
			_Buttons[2, 5] = btn26;
			_Buttons[2, 6] = btn27;
			_Buttons[2, 7] = btn28;

			_Buttons[3, 0] = btn31;
			_Buttons[3, 1] = btn32;
			_Buttons[3, 2] = btn33;
			_Buttons[3, 3] = btn34;
			_Buttons[3, 4] = btn35;
			_Buttons[3, 5] = btn36;
			_Buttons[3, 6] = btn37;
			_Buttons[3, 7] = btn38;

			_Buttons[4, 0] = btn41;
			_Buttons[4, 1] = btn42;
			_Buttons[4, 2] = btn43;
			_Buttons[4, 3] = btn44;
			_Buttons[4, 4] = btn45;
			_Buttons[4, 5] = btn46;
			_Buttons[4, 6] = btn47;
			_Buttons[4, 7] = btn48;

			_Buttons[5, 0] = btn51;
			_Buttons[5, 1] = btn52;
			_Buttons[5, 2] = btn53;
			_Buttons[5, 3] = btn54;
			_Buttons[5, 4] = btn55;
			_Buttons[5, 5] = btn56;
			_Buttons[5, 6] = btn57;
			_Buttons[5, 7] = btn58;

			_Buttons[6, 0] = btn61;
			_Buttons[6, 1] = btn62;
			_Buttons[6, 2] = btn63;
			_Buttons[6, 3] = btn64;
			_Buttons[6, 4] = btn65;
			_Buttons[6, 5] = btn66;
			_Buttons[6, 6] = btn67;
			_Buttons[6, 7] = btn68;

			_Buttons[7, 0] = btn71;
			_Buttons[7, 1] = btn72;
			_Buttons[7, 2] = btn73;
			_Buttons[7, 3] = btn74;
			_Buttons[7, 4] = btn75;
			_Buttons[7, 5] = btn76;
			_Buttons[7, 6] = btn77;
			_Buttons[7, 7] = btn78;

			_Controllers[0] = groupBox0;
			_Controllers[1] = groupBox1;
			_Controllers[2] = groupBox2;
			_Controllers[3] = groupBox3;
			_Controllers[4] = groupBox4;
			_Controllers[5] = groupBox5;
			_Controllers[6] = groupBox6;
			_Controllers[7] = groupBox7;
		}

		private void PollingEvent(Object myObject, EventArgs myEventArgs)
		{
			foreach (var kvp in _PortList)
			{
				byte btButtons = PollController(kvp.Key);
				for (int i=0; i<8; i++)
				{
					_Buttons[kvp.Key, i].BackColor = ((btButtons & 0x01) == 1) 
														? Color.Red 
														: Color.FromArgb(255, 240, 240, 240);
					btButtons >>= 1;
				}
			}
		}

		private byte PollController(int portKey)
		{
			var port = _PortList[portKey];
			port.Write("b");
			var csButtons = port.ReadLine().Substring(2);
			byte btButtons  = byte.Parse(csButtons, System.Globalization.NumberStyles.HexNumber);
			return btButtons;
		}

		private void btnExit_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}
	}
}
