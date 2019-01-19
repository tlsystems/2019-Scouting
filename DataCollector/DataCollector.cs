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
				if (!QueryPort(port, 'v', out version))
					continue;

				string switchVal;
				if (!QueryPort(port, 's', out switchVal))
					continue;

				uint key = uint.Parse(switchVal.Substring(0, 2), NumberStyles.HexNumber);
				if (key >= _PortCount)
					continue;

				_PortList.Add(key, port);
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
			foreach (var port in _PortList.Values)
			{
				port.Write("b");
			}
		}

		private void OnTimerEvent(Object source, ElapsedEventArgs e)
		{
			foreach (var kvp in _PortList)
			{
				Buttons[(int)kvp.Key] = (byte)PollController(kvp.Value);
			}
			_PollingTimer.Enabled = true;
		}

		private int PollController(SerialPort port)
		{
			if (!QueryPort(port, 'b', out string buttonStr))
				return -1;

			return int.Parse(buttonStr.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
		}

		bool QueryPort(SerialPort port, char chQuery, out string response )
		{
			char[] chBuf = { chQuery };
			try
			{
				port.Write(chBuf, 0, 1);
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
