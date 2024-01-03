using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

namespace Boot_Report
{
    public partial class Form1 : Form
    {
        public List<string> result = new List<string>();
        public int count = 0;
        public DateTime expiryDate;
        public Form1()
        {
            InitializeComponent();

            //expiry date
            expiryDate = new DateTime(2024,03,31);


            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.WorkerSupportsCancellation = true;

            List<int> years = new List<int>();
            int currentYear = DateTime.Now.Year;
            for(int i = currentYear-2; i < currentYear+3; i++)
            {
                years.Add(i);
            }

            cbYear.DataSource = years;
            cbYear.SelectedItem = currentYear;
            txtMonth.Text = "In 'mm' format";
            txtMonth.ForeColor = SystemColors.GrayText;

            this.AcceptButton = btnGenerate;
        }

        struct DataParameter
        {
            public string month;
            public string year;
        }

        private DataParameter _inputParameter;

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string month = _inputParameter.month;
            string year = _inputParameter.year;
            string day;
            int noOfDays = GetDaysInMonth(month);

            string filePath;

            for (int i = 1; i <= noOfDays; i++)
            {
                day = i.ToString("D2");
                filePath = $"C:\\V_Services\\data\\{year}{month}{day}\\Logfile.log";


                // Perform some work here...
                result.Add(GetFirstAndLastLineChars(filePath));

                Thread.Sleep(15);
                // Report the progress to the UI thread
                int progressPercentage = (int)((double)i / noOfDays * 100);
                backgroundWorker.ReportProgress(progressPercentage);
            }
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
            lblGenerating.Text = "Generating....." + e.ProgressPercentage.ToString()+"%";
            txtOutput.AppendText(result[count] + Environment.NewLine);
            count++;
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            lblGenerating.Text = "Generated.....100%";
            btnGenerate.Enabled = true;
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            //check input
            try
            {
                if (txtMonth.Text == "" || cbYear.Text == "" || int.Parse(txtMonth.Text) > 12 || int.Parse(txtMonth.Text) <= 0 || int.Parse(cbYear.Text) > DateTime.Now.Year)
                {
                    MessageBox.Show("Enter valid input", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Enter valid input", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            


            btnGenerate.Enabled = false;

            if (!backgroundWorker.IsBusy)
            {
                _inputParameter.month = txtMonth.Text;
                _inputParameter.year = cbYear.Text;


                backgroundWorker.RunWorkerAsync(_inputParameter);
            }
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            string txtToCopy = txtOutput.Text;
            if(txtToCopy == "")
            {
                MessageBox.Show("Nothing to copy here","Empty Clipboard",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
            else
            {
                try
                {
                    Clipboard.SetText(txtToCopy);
                    MessageBox.Show("Copied succesfuly", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Copied succesfuly", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }



        //methods
        //get days in month
        public static int GetDaysInMonth(string month)
        {
            int daysInMonth = 0;

            if (int.TryParse(month, out int monthNumber))
            {
                if (monthNumber >= 1 && monthNumber <= 12)
                {
                    switch (monthNumber)
                    {
                        case 1: // January
                        case 3: // March
                        case 5: // May
                        case 7: // July
                        case 8: // August
                        case 10: // October
                        case 12: // December
                            daysInMonth = 31;
                            break;
                        case 4: // April
                        case 6: // June
                        case 9: // September
                        case 11: // November
                            daysInMonth = 30;
                            break;
                        case 2: // February
                            int year = DateTime.Now.Year; // Use the current year for simplicity
                            daysInMonth = DateTime.IsLeapYear(year) ? 29 : 28;
                            break;
                    }
                }
            }

            return daysInMonth;
        }


        //geting times
        public static string GetFirstAndLastLineChars(string filePath)
        {
            if (!File.Exists(filePath))
                return "not found\tnot found";

            string firstChars = "";
            string lastChars = "";

            try
            {
                // Read all lines from the file
                string[] lines = File.ReadAllLines(filePath);

                // Get the first line and its first 8 characters
                if (lines.Length > 0)
                {
                    for(int i = 0; i < lines.Length; i++)
                    {
                        string firstLine = lines[i];
                        firstChars = firstLine.Length >= 8 ? firstLine.Substring(0, 8) : firstLine;
                        if (firstChars.Contains(":") && firstChars.Length == 8 && !firstChars.Contains(" ") && !firstChars.Contains("\t"))
                            break;
                    }
                }

                // Get the last line and its first 8 characters
                if (lines.Length > 1)
                {
                    for (int i = lines.Length - 1; i >= 0; i--)
                    {
                        string lastLine = lines[i];
                        lastChars = lastLine.Length >= 8 ? lastLine.Substring(0, 8) : lastLine;
                        if (lastChars.Contains(":") && lastChars.Length == 8 && !lastChars.Contains(" ") && !lastChars.Contains("\t"))
                            break;
                    }
                    
                }
            }
            catch (Exception ex)
            {
                // Handle file reading errors, if any
                Console.WriteLine($"Error reading the file: {ex.Message}");
            }


            if (!(firstChars.Contains(":") && firstChars.Length == 8 && !firstChars.Contains(" ") && !firstChars.Contains("\t")) || !(lastChars.Contains(":") && lastChars.Length == 8 && !lastChars.Contains(" ") && !lastChars.Contains("\t")))
            {
                return "not found\tnot found";
            }

            
            try
            {
                TimeSpan duration = DateTime.Parse(lastChars) - DateTime.Parse(firstChars);
                return firstChars +"\t"+ lastChars + "\t" + duration.ToString();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
            // Concatenate the first 8 characters of the first and last lines
            return firstChars +"\t"+ lastChars + "\t" +"";
        }

        private void txtMonth_Enter(object sender, EventArgs e)
        {
            if (txtMonth.Text == "In 'mm' format")
            {
                txtMonth.Text = string.Empty;
                txtMonth.ForeColor = SystemColors.MenuText;
            }
        }

        private void txtMonth_Leave(object sender, EventArgs e)
        {
            if (txtMonth.Text == string.Empty)
            {
                txtMonth.Text = "In 'mm' format";
                txtMonth.ForeColor = SystemColors.GrayText;
            }
        }

        private void Form1_HelpButtonClicked(object sender, CancelEventArgs e)
        {
            TimeSpan expiringIn = expiryDate - DateTime.Now;
            MessageBox.Show($"Lisence Expiring in {expiringIn.Days}d {expiringIn.Hours}h {expiringIn.Minutes}m {expiringIn.Seconds}s\nFor any help contact me at bablushaikh0000@gmail.com","About",MessageBoxButtons.OK,MessageBoxIcon.Information);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (expiryDate <  DateTime.Now)
            {
                MessageBox.Show("Your Application is Expired.\nPlease Contact Administrator","Hello",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                this.Close();
            }

        }
    }
}
