using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace YouBikeStationCrawler
{
    public partial class Form1 : Form
    {
        SaveData saveData;
        SaveData save1 = new SaveData(@"C:\Users\huadi\Desktop\");
        Transform trans;

        int stationsCount = 0;

        string fileDir = @".\";
        string fileName = "_YouBikeStation.txt";
        string filePath = "";

        string lastUpdateTime = "00-00-00";

        public Form1()
        {
            InitializeComponent();
            saveData = new SaveData(fileDir); //存文字檔
            trans = new Transform(); //轉換座標

            RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            //rkApp.SetValue("YouBike Station Crawler", Application.ExecutablePath.ToString());
            rkApp.DeleteValue("YouBike Station Crawler", false);

            filePath = fileDir + fileName;
            if (File.Exists(@".\stationsCount.txt"))
                stationsCount = Int32.Parse(File.ReadAllText(@".\stationsCount.txt"));

            timer1.Interval = 1000; // 設定每秒觸發一次
            timer1.Enabled = true; // 啟動 Timer

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            lab_lastUpdateTime.Text = lastUpdateTime + Environment.NewLine +
                DateTime.Now.ToString("HH-mm-ss");

            if (DateTime.Now.ToString("HH-mm-ss") == "01-01-01" ||
                DateTime.Now.ToString("HH-mm-ss") == "06-06-06" ||
                DateTime.Now.ToString("HH-mm-ss") == "12-12-12" ||
                DateTime.Now.ToString("HH-mm-ss") == "18-18-18")
            {
                File.Delete(filePath);
                File.Delete(@"C:\Users\huadi\Desktop\" + fileName);
                Crawler("http://its.taipei.gov.tw/aspx/Youbike.aspx?Mode=1");
                lastUpdateTime = "上次更新時間 " + DateTime.Now.ToString("HH-mm-ss");
            }
        }

        public void Crawler(string URL)
        {
            HttpWebRequest all_request = (HttpWebRequest)WebRequest.Create(URL);
            WebResponse all_response = all_request.GetResponse();

            string all_code = "";
            //得到網頁內容
            StreamReader the_Reader = new StreamReader(all_response.GetResponseStream(), Encoding.UTF8);
            all_code = the_Reader.ReadToEnd();
            the_Reader.Close();

            List<string> stations = all_code.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            stations.Sort((x, y) => { return x.ToString().Split('_')[0].CompareTo(y.ToString().Split('_')[0]); }); //以編號排序

            label1.Text = "共 " + stations.Count + " 個租賃站" + Environment.NewLine +
                          "最高編號為 " + stations[stations.Count - 1].Split('_')[0] + Environment.NewLine +
                          "跳號的有 ";

            // 代號       名稱         總車位  目前車輛數       地區                                (位)地址      英文區域        英文名稱
            // sno + "_" + sna + "_" + tot + "_" + sbi + "_" + sarea +  "_" + lng + "_" + lat + "_" + ar + "_" + sareaen + "_" + snaen + "_|";
            int fix = 0; //YouBike 編號有跳號的修正
            for (int i = 0; i <= Int32.Parse(stations[stations.Count - 1].Split('_')[0]); i++)
            {
                if (i == Int32.Parse(stations[i - fix].Split('_')[0]))
                {
                    string sno = stations[i - fix].Split('_')[0]; //代號
                    string sna = stations[i - fix].Split('_')[1]; //名稱
                    string tot = stations[i - fix].Split('_')[2]; //總車位
                    string sbi = stations[i - fix].Split('_')[3]; //目前車輛數
                    string sarea = stations[i - fix].Split('_')[4]; //地區
                    string lat = stations[i - fix].Split('_')[5]; //tm2_tw67
                    string lng = stations[i - fix].Split('_')[6];
                    string ar = stations[i - fix].Split('_')[7]; //地址
                    string sareaen = stations[i - fix].Split('_')[8]; //英文區域
                    string snaen = stations[i - fix].Split('_')[9]; //英文名稱

                    saveData.Save(fileName,
                        "items.add(new OverlayItem(new GeoPoint(" +
                        //trans.TWD67_To_lonlat(Double.Parse(lng), Double.Parse(lat), 2) +
                        "(int)( " + lat + " * 1E6), (int)( " + lng + " * 1E6)" +
                        "), \"" + i.ToString("D4") + "\", \" " + sna + " \" ));"
                        );

                    //new Format                    
                    save1.Save(fileName,
                        sno + ","
                        + lat + ","
                        + lng + ","
                        //trans.TWD67_To_lonlat(Double.Parse(lat), Double.Parse(lng), 2).Replace("(int)( ", "").Replace(" * 1E6)", "") + ","
                        + sna + ","
                        + ar + ","
                        + sarea + ","
                        + sareaen + ","
                        + snaen);

                }
                else //跳號了則將此點放到經緯度(0,0)的位置
                {
                    fix++;
                    //saveData.Save(fileName,
                    //    "items.add(new OverlayItem(new GeoPoint(0,0), " + i.ToString("D4") + ", null));"
                    //    );
                    label1.Text += i.ToString() + " "; //跳號的
                }
            }

            if (stationsCount != stations.Count) //若這次撈到的數量與原來不同
            {
                Send_Gmail(label1.Text, "FreeBike Update", "your.account@example.com"); //寄信
                stationsCount = stations.Count;
                File.WriteAllText(@".\stationsCount.txt", stationsCount.ToString()); //寫下撈到幾筆
            }
        }

        public void Send_Gmail(string msg, string mysubject, string address)
        {
            MailMessage mmsg = new MailMessage("your.account@example.com", address);
            mmsg.IsBodyHtml = true; //設定是否採用HTML格式
            mmsg.BodyEncoding = Encoding.UTF8; //設定mail內容的編碼
            mmsg.SubjectEncoding = Encoding.UTF8; //設定mail主旨的編碼
            mmsg.Priority = MailPriority.Normal; //設定優先權 1.High 2.Normail 3.Low
            mmsg.Subject = mysubject; // mail主旨
            mmsg.Body = msg; //mail內容

            Attachment file = null;
            if (File.Exists(filePath))
            {
                file = new Attachment(filePath);
                mmsg.Attachments.Add(file);
            }

            SmtpClient MySmtp = new SmtpClient("smtp.gmail.com", 587); //允許程式使用smtp來發mail，並設定smtp server & port
            MySmtp.Credentials = new NetworkCredential("your.account@example.com", "YOUR_GMAIL_PASSWORD"); //設定帳號與密碼 需要using system.net;
            MySmtp.EnableSsl = true; //開啟SSL連線 (gmail體系須使用SSL連線)
            MySmtp.Send(mmsg);

            MySmtp = null; //將MySmtp清空
            mmsg.Dispose(); //釋放資源
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                //this.Hide();
                this.notifyIcon1.Visible = true;
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //讓Form再度顯示，並寫狀態設為Normal
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            File.Delete(filePath);
            File.Delete(@"C:\Users\huadi\Desktop\" + fileName);
            Crawler("http://its.taipei.gov.tw/aspx/Youbike.aspx?Mode=1");
            lastUpdateTime = "上次更新時間 " + DateTime.Now.ToString("HH-mm-ss");
        }

    }
}
