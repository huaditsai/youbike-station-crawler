using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using YouBikeStationCrawler;

namespace YouBikeStationCrawlerService
{
    public class Crawler
    {
        SaveData saveData;
        SaveData save1 = new SaveData(@"C:\inetpub\wwwroot\Download\");
        //Transform trans;

        string infoText = "";

        public void Crawl(string URL)
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

            infoText = "共 " + stations.Count + " 個租賃站" + Environment.NewLine +
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

                    saveData.Save(MyFile.fileName,
                        "items.add(new OverlayItem(new GeoPoint(" +
                        //trans.TWD67_To_lonlat(Double.Parse(lng), Double.Parse(lat), 2) +
                        "(int)( " + lat + " * 1E6), (int)( " + lng + " * 1E6)" +
                        "), \"" + i.ToString("D4") + "\", \" " + sna + " \" ));"
                        );

                    //new Format                    
                    save1.Save(MyFile.fileName,
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
                    infoText += i.ToString() + " "; //跳號的
                }
            }

            if (MyFile.stationsCount != stations.Count) //若這次撈到的數量與原來不同
            {
                Send_Gmail(infoText, "FreeBike Update", "your.account@example.com"); //寄信
                MyFile.stationsCount = stations.Count;
                File.WriteAllText(@".\stationsCount.txt", MyFile.stationsCount.ToString()); //寫下撈到幾筆
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
            if (File.Exists(MyFile.filePath))
            {
                file = new Attachment(MyFile.filePath);
                mmsg.Attachments.Add(file);
            }

            SmtpClient MySmtp = new SmtpClient("smtp.gmail.com", 587); //允許程式使用smtp來發mail，並設定smtp server & port
            MySmtp.Credentials = new NetworkCredential("your.account@example.com", "YOUR_GMAIL_PASSWORD"); //設定帳號與密碼 需要using system.net;
            MySmtp.EnableSsl = true; //開啟SSL連線 (gmail體系須使用SSL連線)
            MySmtp.Send(mmsg);

            MySmtp = null; //將MySmtp清空
            mmsg.Dispose(); //釋放資源
        }
    }
}
