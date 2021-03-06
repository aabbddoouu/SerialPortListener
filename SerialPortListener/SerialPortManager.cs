﻿using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
using System.IO.Ports;
using System.Reflection;
//using System.ComponentModel;
//using System.Threading;
using System.IO;
//using static System.Net.WebRequestMethods;

namespace SerialPortListener.Serial
{
    
    /// <summary>
    /// Manager for serial port data
    /// </summary>
    public class SerialPortManager : IDisposable
    {   
        private MainForm form;
        public string line;
        public SerialPortManager(MainForm form)
        {
            this.form = form;
            //this.file = new StreamWriter("C:/Users/HP/Desktop/NXP/tab.csv");
            // Finding installed serial ports on hardware
            _currentSerialSettings.PortNameCollection = SerialPort.GetPortNames();
            _currentSerialSettings.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_currentSerialSettings_PropertyChanged);

            // If serial ports is found, we select the first found
            if (_currentSerialSettings.PortNameCollection.Length > 0)
                _currentSerialSettings.PortName = _currentSerialSettings.PortNameCollection[0];
        }
        public StreamWriter file;
        
        ~SerialPortManager()
        {
            Dispose(false);
        }


        #region Fields
        public SerialPort _serialPort;
        private SerialSettings _currentSerialSettings = new SerialSettings();
        private string _latestRecieved = String.Empty;
        public event EventHandler<SerialDataEventArgs> NewSerialDataRecieved; 

        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the current serial port settings
        /// </summary>
        public SerialSettings CurrentSerialSettings
        {
            get { return _currentSerialSettings; }
            set { _currentSerialSettings = value; }
        }

        #endregion

        #region Event handlers

        void _currentSerialSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // if serial port is changed, a new baud query is issued
            if (e.PropertyName.Equals("PortName"))
                UpdateBaudRateCollection();
        }




        #endregion


        void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {

            //int dataLength = _serialPort.BytesToRead;
            //byte[] data = new byte[dataLength];

            line = _serialPort.ReadLine();// (data, 0, dataLength);
            if (line.Length == 0)
                return;
            line = line.Replace('\r', '\n');
            file.WriteLine(line);
            if (!form.backgroundWorker1.IsBusy)
            {
                form.backgroundWorker1.RunWorkerAsync(line);
            }
            if (!form.backgroundWorker3.IsBusy)
            {
                form.backgroundWorker3.RunWorkerAsync(line);
            }
            //MainForm mainForm = new MainForm();



            // Send data to whom ever interested
            if (NewSerialDataRecieved != null)
            {
                ;
            }
            //  NewSerialDataRecieved(this, new SerialDataEventArgs(data));
        }



        #region Methods

        /// <summary>
        /// Connects to a serial port defined through the current settings
        /// </summary>
        public void StartListening()
        {
            // Closing serial port if it is open
            if (_serialPort != null && _serialPort.IsOpen)
                    _serialPort.Close();

            // Setting serial port settings
            _serialPort = new SerialPort(
                _currentSerialSettings.PortName,
                _currentSerialSettings.BaudRate,
                _currentSerialSettings.Parity,
                _currentSerialSettings.DataBits,
                _currentSerialSettings.StopBits);

            // Subscribe to event and open serial port for data
            _serialPort.DataReceived += new SerialDataReceivedEventHandler(_serialPort_DataReceived);
            _serialPort.Open();
        }

        /// <summary>
        /// Closes the serial port
        /// </summary>
        public void StopListening()
        {
            _serialPort.Close();
        }


        /// <summary>
        /// Retrieves the current selected device's COMMPROP structure, and extracts the dwSettableBaud property
        /// </summary>
        private void UpdateBaudRateCollection()
        {
            _serialPort = new SerialPort(_currentSerialSettings.PortName);
            _serialPort.Open();
            object p = _serialPort.BaseStream.GetType().GetField("commProp", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(_serialPort.BaseStream);
            Int32 dwSettableBaud = (Int32)p.GetType().GetField("dwSettableBaud", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetValue(p);

            _serialPort.Close();
            _currentSerialSettings.UpdateBaudRateCollection(dwSettableBaud);
        }

        // Call to release serial port
        public void Dispose()
        {
            Dispose(true);
        }

        // Part of basic design pattern for implementing Dispose
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _serialPort.DataReceived -= new SerialDataReceivedEventHandler(_serialPort_DataReceived);
            }
            // Releasing serial port (and other unmanaged objects)
            if (_serialPort != null)
            {
                if (_serialPort.IsOpen)
                    _serialPort.Close();

                _serialPort.Dispose();
            }
        }


        #endregion

    }

    /// <summary>
    /// EventArgs used to send bytes recieved on serial port
    /// </summary>
    public class SerialDataEventArgs : EventArgs
    {
        public SerialDataEventArgs(byte[] dataInByteArray)
        {
            Data = dataInByteArray;
        }

        /// <summary>
        /// Byte array containing data from serial port
        /// </summary>
        public byte[] Data;
    }
}
