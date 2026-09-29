# YouBike Station Crawler

Two small C# tools from 2013 and 2014 that collected Taipei YouBike station data for my FreeBike and UBike+ Android apps: a desktop WinForms version, and a Windows Service version that ran unattended on a server.

**Status: archived historical project, no longer maintained.** The Taipei ITS page it reads now returns 404, and the code has not been built or run since 2014.

## Background

In 2013 Taipei's YouBike network was expanding quickly, and a bike-sharing app needed an up-to-date station list. The Taipei ITS website (`its.taipei.gov.tw`) published the station data for its own map page as one long `|`-separated string. These tools downloaded that string, sorted the stations by number, reported gaps in the numbering, and wrote the list in the formats the apps used. YouBike data is now published as official open data, so a scraper like this is no longer needed.

## Contents

| Folder | Built | What it does |
|---|---|---|
| `YouBikeStationCrawler/` | July to September 2013 | WinForms app. At 01:01:01, 06:06:06, 12:12:12 and 18:18:18 every day it downloads the station string and writes `_YouBikeStation.txt` in two formats: Java lines (`items.add(new OverlayItem(new GeoPoint(...)))`) ready to paste into FreeBike's Google Maps overlay, and a comma-separated list. It sends an email when the number of stations changes. |
| `YouBikeStationCrawlerService/` | January to February 2014 | The same crawler as a Windows Service that refreshes every minute. It writes the comma-separated list into the IIS folder `C:\inetpub\wwwroot\Download\` so the list could be downloaded over HTTP. |

`Transform.cs` converts between WGS84 longitude/latitude and TWD97 (TM2) coordinates. It follows the C# code in the blog post [TWD97轉經緯度WGS84 經緯度WGS84轉TWD97](http://wangshifuola.blogspot.com/2010/08/twd97wgs84-wgs84twd97.html) (2010). The final version uses the feed's longitude and latitude directly, so the calls to it are commented out.

Related repositories: [freebike-android](https://github.com/huaditsai/freebike-android), [freebike-android-2012](https://github.com/huaditsai/freebike-android-2012), [ubike-plus-android](https://github.com/huaditsai/ubike-plus-android).

## Cleaned before publication

- The Gmail address and password used to send the email are replaced with `your.account@example.com` and `YOUR_GMAIL_PASSWORD`.
- Build output (`bin/`, `obj/`) and Visual Studio user files (`*.suo`, `*.user`) are removed.

## License

No open-source license applies to my own code. Do not assume the repository grants any license. `Transform.cs` is based on the blog post credited above.
