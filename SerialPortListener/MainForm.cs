using System;
//using System.Collections.Generic;
using System.ComponentModel;
//using System.Data;
using System.Drawing;
using System.Linq;
//using System.Text;
using System.Windows.Forms;
using SerialPortListener.Serial;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms.DataVisualization.Charting;

namespace SerialPortListener
{

    public partial class MainForm : Form
    {
        Point? prevPosition = null;
        string path= "C:\\Users\\Public";
        String old_path = "C:\\Users\\Public";
        Stopwatch sw = new Stopwatch();
        double erreur;
        bool err = true;
        UInt64 count;
        SerialPortManager _spManager, port1;
        public MainForm()
        {
            InitializeComponent();

            UserInitialization();
        }


        private void UserInitialization()
        {
            //StreamWriter file = new StreamWriter("C:/Users/HP/Desktop/NXP/tab.csv");
            _spManager = new SerialPortManager(this);
            port1 = new SerialPortManager(this);

            count = 0;
            SerialSettings mySerialSettings = _spManager.CurrentSerialSettings;
            serialSettingsBindingSource.DataSource = mySerialSettings;
            portNameComboBox.DataSource = mySerialSettings.PortNameCollection;
            baudRateComboBox.DataSource = mySerialSettings.BaudRateCollection;
            dataBitsComboBox.DataSource = mySerialSettings.DataBitsCollection;
            parityComboBox.DataSource = Enum.GetValues(typeof(System.IO.Ports.Parity));
            stopBitsComboBox.DataSource = Enum.GetValues(typeof(System.IO.Ports.StopBits));

            folderBrowserDialog1.Disposed += new EventHandler(folderBrowserDialog1_disposed);
            _spManager.NewSerialDataRecieved += new EventHandler<SerialDataEventArgs>(_spManager_NewSerialDataRecieved);
            this.FormClosing += new FormClosingEventHandler(MainForm_FormClosing);
            /*
            //gyro
            port1._serialPort.BaudRate = 115200;
            port1._serialPort.PortName = "COM33";
            port1._serialPort.Parity = System.IO.Ports.Parity.None;
            port1._serialPort.DataBits = 8;
            port1._serialPort.StopBits = System.IO.Ports.StopBits.One;
            

            port1._serialPort.Open();
        */
            }

        private void FolderBrowserDialog1_Disposed(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            
            _spManager.Dispose();
        }
        public void _spPrint(string line)
        {

        }
        void _spManager_NewSerialDataRecieved(object sender, SerialDataEventArgs e)
        {
            if (this.InvokeRequired)
            {
                // Using this.Invoke causes deadlock when closing serial port, and BeginInvoke is good practice anyway.
                this.BeginInvoke(new EventHandler<SerialDataEventArgs>(_spManager_NewSerialDataRecieved), new object[] { sender, e });
                return;
            }
            /*
                        int maxTextLength = 1000; // maximum text length in text box
                        if (tbData.TextLength > maxTextLength)
                            tbData.Text = tbData.Text.Remove(0, tbData.TextLength - maxTextLength);

                        // This application is connected to a GPS sending ASCCI characters, so data is converted to text
                        string str = Encoding.ASCII.GetString(e.Data);
                        tbData.AppendText(str);
                        tbData.ScrollToCaret();
                        */
        }

        // Handles the "Start Listening"-buttom click event
        private void btnStart_Click(object sender, EventArgs e)
        {
            if (path.Length > 1)
            {
                try
                {
                    _spManager.file = new StreamWriter(path+"/TAB.csv");

                    progressBar1.PerformStep();
                    _spManager.StartListening();
                    progressBar1.PerformStep();
                }
                catch (Exception except)
                {
                    MessageBox.Show(except.Message+"\nPlease retart the App...", "ERROR !", MessageBoxButtons.OK);

                }
                
            }
            else
            {
                MessageBox.Show("Use the save buttion to select a path first !", "Path not found!", MessageBoxButtons.OK);
            }




        }

        // Handles the "Stop Listening"-buttom click event
        private void btnStop_Click(object sender, EventArgs e)
        {
            try
            {
                _spManager.file.Close();
                _spManager.StopListening();
                progressBar1.Value = 0;
            }
            catch (Exception)
            {

            }
            
        }

        private void tbData_TextChanged(object sender, EventArgs e)
        {

        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        { //simply epiC 
            
                if (err)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    //port1._serialPort.WriteLine(_spManager.line);
                    tbData.AppendText(_spManager.line + "\n");
                    tbData.ScrollToCaret();

                    /*
                    if (_spManager.line.Contains('-'))
                    {

                        if (_spManager.line.StartsWith("K"))
                        {
                            string[] ok = _spManager.line.Split('-');
                        }
                        else       //startWith("v")
                        {
                            
                        }


                        return;
                    }
                    */
                    
                    if (_spManager.line.Contains(';'))
                    {

                        string[] ok = _spManager.line.Substring(0, _spManager.line.LastIndexOf(';')).Split(';');
                        Serial_plot.Series[0].Points.AddXY(count, ok[0]);

                        
                        Serial_plot.ChartAreas[0].AxisY.Maximum = Serial_plot.Series[0].Points.FindMaxByValue().YValues[0] + 0.002;
                        Serial_plot.ChartAreas[0].AxisY.Minimum = Serial_plot.Series[0].Points.FindMinByValue().YValues[0] - 0.002;
                        Serial_plot.ChartAreas[0].RecalculateAxesScale();
                        

                        Serial_plot_2.Series[0].Points.AddXY(count, ok[1]);
                        

                        
                        Serial_plot_2.ChartAreas[0].AxisY.Maximum = Serial_plot_2.Series[0].Points.FindMaxByValue().YValues[0] + 0.002;
                        Serial_plot_2.ChartAreas[0].RecalculateAxesScale();
                        Serial_plot_2.ChartAreas[0].AxisY.Minimum = Serial_plot_2.Series[0].Points.FindMinByValue().YValues[0] - 0.002;
                    

                        //Serial_plot.Series[1].Points.AddXY(count, ok[1]);
                        count++;
                        /*if (count==128)
                        {
                            Serial_plot.Series[0].Points.Clear();
                            count = 0;
                        }*/
                        return;
                    }
                    if (_spManager.line.Contains(','))
                    {

                        if (line_scan.Checked)
                        {
                            camera_chart.Series[0].Points.Clear();
                            string[] ok = _spManager.line.Split(',');
                            camera_chart.Series[0].Points.AddY(ok);

                        }
                        

                    }
                });
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (!backgroundWorker2.IsBusy && !backgroundWorker1.IsBusy)
            {
                backgroundWorker2.RunWorkerAsync();
            }
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            BeginInvoke((MethodInvoker)delegate
            {
                Serial_plot.Series[0].Points.Clear();
                Serial_plot_2.Series[0].Points.Clear();
            });
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Serial_plot.Series[0].Points.Clear();
            Serial_plot_2.Series[0].Points.Clear();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            
            {
                _spManager._serialPort.WriteLine(textBox1.Text+"\n");

            }

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            sw.Restart();
            label1.Text = DateTime.Now.ToString("HH:mm:ss.ffffff");

            sw.Stop();
            long nanoseconds = (1000000000 * sw.ElapsedTicks) / Stopwatch.Frequency;
            label2.Text = nanoseconds.ToString();
        }

        private void backgroundWorker3_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!err)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    tbData.AppendText(_spManager.line + "\n");
                    tbData.ScrollToCaret();
                    if (_spManager.line.Contains(' '))
                    {

                        erreur = Convert.ToDouble(_spManager.line.Split(' ')[1]);
                        err_chart.Series[0].Points.AddXY(count, erreur);
                        err_chart.ChartAreas[0].AxisY.Maximum = err_chart.Series[0].Points.FindMaxByValue().YValues[0] + 0.0001;
                        err_chart.ChartAreas[0].AxisY.Minimum = err_chart.Series[0].Points.FindMinByValue().YValues[0] - 0.0001;
                        err_chart.ChartAreas[0].RecalculateAxesScale();

                    }
                });
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            err = !err;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            tbData.Clear();
        }

        private void GroupBox4_Enter(object sender, EventArgs e)
        {

        }



        private void Line_scan_bgw_DoWork(object sender, DoWorkEventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        private void folderBrowserDialog1_HelpRequest(object sender, EventArgs e)
        {

        }



        private void backgroundWorker4_DoWork_1(object sender, DoWorkEventArgs e)
        {
            //path = "";
            DialogResult dialogResult;
            dialogResult = DialogResult.OK;
            BeginInvoke((MethodInvoker)delegate
            {
                dialogResult=folderBrowserDialog1.ShowDialog();
            });

            DateTime init_time = DateTime.Now;
            //DateTime curr_time;
            folderBrowserDialog1.Reset();
            while (path== old_path)
            {
                
                if (folderBrowserDialog1.SelectedPath != "")
                {
                    path = folderBrowserDialog1.SelectedPath;
                    old_path = path;
                    break;
                }
                
                /*
                if (GetSecondsDifference(curr_time, init_time) >= 30)
                {
                    break;
                }
                */
                Thread.Sleep(100);
                if (dialogResult == DialogResult.Cancel)
                {
                    break;
                }

            }

            BeginInvoke((MethodInvoker)delegate
            {
                label5.Text = path + "\\TAB.csv";
            });

            folderBrowserDialog1.Dispose();
            /*
            BeginInvoke((MethodInvoker)delegate
            {
                label5.Text = path + "\\TAB.csv";
            });
            */

        }

        private void folderBrowserDialog1_disposed(object sender, EventArgs e)
        {
            BeginInvoke((MethodInvoker)delegate
            {
                backgroundWorker4.Dispose();
            });
        }

        private void button6_Click_2(object sender, EventArgs e)
        {
            if (!backgroundWorker4.IsBusy)
            {
                backgroundWorker4.RunWorkerAsync();
            }
        }

        private void Serial_plot_Click(object sender, EventArgs e)
        {

        }

        private void Serial_plot_MouseMove(object sender, MouseEventArgs e)
        {/////////////////// TO READ ////////////////////////////////////
            var pos = e.Location;
            if (prevPosition.HasValue && pos == prevPosition.Value)
                return;
            Hover_chart.RemoveAll();
            prevPosition = pos;
            var results = Serial_plot.HitTest(pos.X, pos.Y, false,
                                            ChartElementType.DataPoint);
            foreach (var result in results)
            {
                if (result.ChartElementType == ChartElementType.DataPoint)
                {
                    var prop = result.Object as DataPoint;
                    if (prop != null)
                    {
                        var pointXPixel = result.ChartArea.AxisX.ValueToPixelPosition(prop.XValue);
                        var pointYPixel = result.ChartArea.AxisY.ValueToPixelPosition(prop.YValues[0]);

                        // check if the cursor is really close to the point (2 pixels around the point)
                        if (Math.Abs(pos.X - pointXPixel) < 5 ||
                            Math.Abs(pos.Y - pointYPixel) < 20)
                        {
                            Hover_chart.Show("X=" + prop.XValue + ", Y=" + prop.YValues[0], this.Serial_plot,
                                            pos.X, pos.Y - 15);

                        }
                    }
                }
            }
        }

        private void Serial_plot_2_Click(object sender, EventArgs e)
        {

        }

        private void Serial_plot_2_MouseMove(object sender, MouseEventArgs e)
        {/////////////////// TO READ ////////////////////////////////////
            var pos = e.Location;
            if (prevPosition.HasValue && pos == prevPosition.Value)
                return;
            Hover_chart.RemoveAll();
            prevPosition = pos;
            var results = Serial_plot_2.HitTest(pos.X, pos.Y, false,
                                            ChartElementType.DataPoint);
            foreach (var result in results)
            {
                if (result.ChartElementType == ChartElementType.DataPoint)
                {
                    var prop = result.Object as DataPoint;
                    if (prop != null)
                    {
                        var pointXPixel = result.ChartArea.AxisX.ValueToPixelPosition(prop.XValue);
                        var pointYPixel = result.ChartArea.AxisY.ValueToPixelPosition(prop.YValues[0]);

                        // check if the cursor is really close to the point (2 pixels around the point)
                        if (Math.Abs(pos.X - pointXPixel) < 5 ||
                            Math.Abs(pos.Y - pointYPixel) < 20)
                        {
                            Hover_chart.Show("X=" + prop.XValue + ", Y=" + prop.YValues[0], this.Serial_plot_2,
                                            pos.X, pos.Y - 15);

                        }
                    }
                }
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {

            Process.Start(path + "/TAB.csv");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            err_chart.Series[0].Points.Clear();

        }

    }
}
