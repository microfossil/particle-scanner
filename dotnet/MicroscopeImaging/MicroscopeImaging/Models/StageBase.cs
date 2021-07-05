using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;
using System.Timers;
using GalaSoft.MvvmLight;
using WPFHelpers.Helpers;

namespace MicroscopeImaging.Models
{
    public abstract class StageBase : ObservableObject
    {
        //Configuration
        public abstract int LargeStepXY { get; set; }
        public abstract int MediumStepXY { get; set; }
        public abstract int SmallStepXY { get; set; }
        public abstract int LargeStepZ { get; set; }
        public abstract int MediumStepZ { get; set; }
        public abstract int SmallStepZ { get; set; }
        public abstract int LimitPosX { get; set; }
        public abstract int LimitPosY { get; set; }
        public abstract int LimitNegX { get; set; }
        public abstract int LimitNegY { get; set; }
        public abstract int MicrometersPerUnit { get; set; }

        //Connection
        public abstract int BaudRate { get; set; }
        private SerialPort serial;
        public ObservableCollection<String> Ports { get; set; } = new ObservableCollection<String>();

        //Update Timer
        private Timer updateTimer;

        //Properties
        private bool isConnected;

        public bool IsConnected
        {
            get => isConnected;
            set => Set(ref isConnected, value);
        }

        private int x = 0;

        public int X
        {
            get => x;
            set => Set(ref x, value);
        }

        private int y = 0;

        public int Y
        {
            get => y;
            set => Set(ref y, value);
        }

        private int z = 0;

        public int Z
        {
            get => z;
            set => Set(ref z, value);
        }

        public int VirtualX { get; set; } = -Int32.MaxValue;
        public int VirtualY { get; set; } = -Int32.MaxValue;
        public int VirtualZ { get; set; } = -Int32.MaxValue;

        public StageBase()
        {
            updateTimer = new Timer(1000);
            updateTimer.Elapsed += UpdateTimer_Elapsed;
        }

        private void UpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            OnPollForUpdates();
        }

        public void RefreshPorts()
        {
            Ports.Clear();
            foreach (String s in SerialPort.GetPortNames())
            {
                Ports.Add(s);
                Console.WriteLine(s);
            }
        }

        public void Connect(String portName)
        {
            serial = new SerialPort(portName, BaudRate, Parity.None, 8, StopBits.One);
            try
            {
                serial.Open();
                if (serial.IsOpen)
                {
                    IsConnected = true;
                    serial.DataReceived += Serial_DataReceived;
                    VirtualX = -Int32.MaxValue;
                    VirtualY = -Int32.MaxValue;
                    VirtualZ = -Int32.MaxValue;
                    OnConnection();
                    updateTimer.Start();
                }
            }
            catch (Exception ex)
            {
                DebugHelper.MethodInfo("Problem opening serial port: " + ex.Message);
            }
        }

        public void Disconnect()
        {
            if (IsConnected)
            {
                serial.DataReceived -= Serial_DataReceived;
                serial.Close();
                IsConnected = false;
            }
        }

        private void Serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            String reply = serial.ReadLine();
            ParseMessageReceived(reply);
        }

        public void Zero()
        {
            VirtualX = X;
            VirtualY = Y;
            VirtualZ = Z;
        }

        public void Move(int x, int y, int z)
        {
            //New positions
            VirtualX = x;
            VirtualY = y;
            VirtualZ = z;

            if (VirtualX > LimitPosX) VirtualX = LimitPosX;
            if (VirtualX < LimitNegX) VirtualX = LimitNegX;
            if (VirtualY > LimitPosY) VirtualY = LimitPosY;
            if (VirtualY < LimitNegY) VirtualY = LimitNegY;

            MoveZ(VirtualZ);
            MoveX(VirtualX);
            MoveY(VirtualY);
        }

        public void MoveRelative(String axis, bool direction, int distance)
        {
            int val = distance;
            if (!direction) val *= -1;

            switch (axis.ToLower())
            {
                case "x":
                    if (VirtualX == -Int32.MaxValue) VirtualX = X;
                    VirtualX += val;
                    if (VirtualX > LimitPosX) VirtualX = LimitPosX;
                    if (VirtualX < LimitNegX) VirtualX = LimitNegX;
                    MoveX(VirtualX);
                    break;
                case "y":
                    if (VirtualY == -Int32.MaxValue) VirtualY = Y;
                    VirtualY += val;
                    if (VirtualY > LimitPosY) VirtualY = LimitPosY;
                    if (VirtualY < LimitNegY) VirtualY = LimitNegY;
                    MoveY(VirtualY);
                    break;
                case "z":
                    if (VirtualZ == -Int32.MaxValue) VirtualZ = Z;
                    VirtualZ += val;
                    MoveZ(VirtualZ);
                    break;
            }
            Debug.WriteLine($"{VirtualX}, {VirtualY}, {VirtualZ}");
        }

        public void SendCommand(params String[] values)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string s in values)
            {
                if (sb.Length != 0) sb.Append(" ");
                sb.Append(s);
            }

            sb.Append("\n");
            // Debug.WriteLine("Command sent: " + sb.ToString());
            if (IsConnected)
            {
                serial.Write(sb.ToString());
            }
        }

        public abstract void OnConnection();
        public abstract void OnPollForUpdates();
        public abstract void ParseMessageReceived(string message);

        public virtual void Home()
        {
            VirtualX = 0;
            VirtualY = 0;
            VirtualZ = 0;
        }
        public abstract void MoveX(int coordinate);
        public abstract void MoveY(int coordinate);
        public abstract void MoveZ(int coordinate);
    }
}