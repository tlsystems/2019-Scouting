using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Timers;
using System.Diagnostics;
using System.Globalization;

namespace DataCollector
{
    public class DataCollector
    {
		private const int kReadTimeout = 10;

		private SortedList<uint, SerialPort> _PortList;
		private Timer _PollingTimer;
		private int _PortCount;

		private byte[] _Buttons;
		private bool[] _Active;
		private string[] _PortInfo;

		public byte[] Buttons { get { return _Buttons; } set { } }
		public bool[] Active  { get { return _Active; } set { } }
		public string[] PortInfo { get { return _PortInfo; } set { } }

		private enum ePoll
		{
			pollSwitches,
			pollButtons
		};

		public DataCollector(int controllerCount)
		{
			if (controllerCount < 0) throw new ArgumentException("Port Count must be >= 0");

			_PortCount	= controllerCount;
			_PortList	= new SortedList<uint, SerialPort>(_PortCount);
			_Buttons	= new byte[_PortCount];
			_Active		= new bool[_PortCount];
			_PortInfo	= new string[_PortCount];

			foreach (string portName in System.IO.Ports.SerialPort.GetPortNames())
			{
				if (Convert.ToInt16(portName.Substring(3)) <= 3)
					continue;

				var port = new SerialPort(portName, 115200, Parity.None, 8, StopBits.One);

				port.ReadTimeout = 100;
				port.Open();

				string version;
				if (!QueryPort(port, "v", out version))
					continue;

				int key = PollController(port, ePoll.pollSwitches);
				if (key < 0 || key >= _PortCount)
					continue;

				_PortList.Add((uint)key, port);
				_Active[key] = true;
				_PortInfo[key] = $"{portName} {version}";
			}

			_PollingTimer = new Timer();
			_PollingTimer.Elapsed += OnTimerEvent;
			_PollingTimer.AutoReset = false;
		}

		public void StartPolling(uint pollingInterval)
		{
			_PollingTimer.Interval = pollingInterval;
			_PollingTimer.Enabled = true;
		}

		public void StopPolling()
		{
			_PollingTimer.Enabled = false;
		}

		private void OnTimerEvent(Object source, ElapsedEventArgs e)
		{
			foreach (var kvp in _PortList)
			{
				_Buttons[(int)kvp.Key] = (byte)PollController(kvp.Value, ePoll.pollButtons);
			}
			_PollingTimer.Enabled = true;
		}

		private int PollController(SerialPort port, ePoll pollID)
		{
			string queryRsp = "";
			bool queryOK = false;
			if (pollID == ePoll.pollSwitches)
				queryOK = QueryPort(port, "ps", out queryRsp);
			else if (pollID == ePoll.pollButtons)
				queryOK = QueryPort(port, "pb", out queryRsp);
			
			if (!queryOK)
				return -1;

			return int.Parse(queryRsp.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
		}

		bool QueryPort(SerialPort port, string query, out string response )
		{
			try
			{
				port.Write(query);
				response = port.ReadLine();
			}
			catch (TimeoutException)
			{
				response = "!";
			}

			return (response != "!");
		}

	}
}
