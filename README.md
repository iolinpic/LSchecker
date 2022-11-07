# LSchecker
Application for testing internet service availability. Fails detected based on HttpStatusCode other then OK. 
starting and report timers are managed by OS included methods (for example cron in linux)
## Configuring
app has 2 config files: 
- appsettings.json - all settings required, _tg_token is your bot token, _tg_chanel_name is name in public link of chanel after "t.me/"
```
{
  "token": "_tg_token",
  "channel": "@_tg_chanel_name",
  "links": "links.json",
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=ls_res.db"
  }
}
```
- links.json - config containing list of links you would like to monitor formated as:
```
[
  {
    "Link": "http://example.com"
  },
  {
    "Link": "https://example.com"
  }
]
```
app reads it's configs at each run so you can freely change them without reloading
## Database
uses sqlite database for storing results of requests
## Notification
notify user to telegram chanel via telegram bot api (public chanel with bot set as admin)
## Reports
```
 dontet ./LSchecker -report
```
using db stored data for preparing simple xlsx report, containing:
- start - DateTime of detected failure
- end - DateTime of changed state
- reason - HttpStatusCode - supplied
- link - URL from checked list
result file uploaded to telegram channel
after making report all stored in db records till current moment are cleared