using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using YouBikeStationCrawler;

namespace YouBikeStationCrawlerService
{
    public partial class Service1 : ServiceBase
    {
        Crawler crawler = new Crawler();
                
        string lastUpdateTime = "00-00-00";

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            MyFile.filePath = MyFile.fileDir + MyFile.fileName;
            if (File.Exists(@".\stationsCount.txt"))
                MyFile.stationsCount = Int32.Parse(File.ReadAllText(@".\stationsCount.txt"));

            System.Timers.Timer timer = new Timer();
            timer.Elapsed += new ElapsedEventHandler(timer_Tick);

            timer.Interval = 60 * 1000; // 設定觸發時間
            timer.Start(); // 啟動 Timer
        }

        protected override void OnStop()
        {
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            File.Delete(MyFile.filePath);
            File.Delete(@"C:\inetpub\wwwroot\Download\" + MyFile.fileName);
            crawler.Crawl("http://its.taipei.gov.tw/aspx/Youbike.aspx?Mode=1");
            lastUpdateTime = "上次更新時間 " + DateTime.Now.ToString("HH-mm-ss");

        }


    }
}
