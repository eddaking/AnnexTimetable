using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace timetableRequest
{    
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Username: ");
            string username = Console.ReadLine();
            Console.WriteLine("Password: ");
            string password = Console.ReadLine();
            Console.WriteLine("Room: '000202065'");
            string room = Console.ReadLine();
            Task t = new Task(() => HTTPStuff(username, password, room));
            t.Start();
            Console.ReadLine();
        }
        static async void HTTPStuff(string username, string password, string room)
        {

            var cookieContainer = new CookieContainer();
            var values = new Dictionary<string, string>
                {
                   { "UserName", "Soton\\" + username },
                   { "Password", password },
                   { "AuthMethod", "FormsAuthentication" }
                };
            var handler = new HttpClientHandler()
            {
                CookieContainer = cookieContainer,
            };

            var baseAddress = new Uri("https://logon.soton.ac.uk");
            cookieContainer.Add(baseAddress, new Cookie("MSISIPSelectionPersistent", "QUQgQVVUSE9SSVRZ"));
            //2017-09-14:13:32:21Z\1
            var dtStr = DateTime.Now.ToString("yyyy-MM-dd:HH:mm:ss'\\Z1'");
            var dt64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(dtStr));
            cookieContainer.Add(baseAddress, new Cookie("MSISLoopDetectionCookie", dt64));

            // ... Use HttpClient.            
            HttpClient client = new HttpClient(handler);

            var content = new FormUrlEncodedContent(values);

            HttpResponseMessage response = await client.PostAsync("https://logon.soton.ac.uk/adfs/ls/?wa=wsignin1.0&wtrealm=https%3a%2f%2ftimetable.soton.ac.uk", content);
            HttpContent pageContent = response.Content;

            // ... Read the string.
            string result = await pageContent.ReadAsStringAsync();

            var body = result.Split(new string[] { "<body>" }, StringSplitOptions.None)[1].Split(new string[] { "</body>" }, StringSplitOptions.None);
            var inputs = body[0].Split(new string[] { "<input" }, StringSplitOptions.None);

            var action = inputs[0].Split(new string[] { "action=\"" }, StringSplitOptions.None)[1].Split(new string[] { "\"" }, StringSplitOptions.None)[0];

            var nameVal = new Dictionary<string, string> { };

            var name = "";
            var val = "";

            for (int i = 1; i < inputs.Length - 1; i++)
            {
                name = inputs[i].Split(new string[] { "name=\"" }, StringSplitOptions.None)[1].Split(new string[] { "\"" }, StringSplitOptions.None)[0];
                val = inputs[i].Split(new string[] { "value=\"" }, StringSplitOptions.None)[1].Split(new string[] { "\"" }, StringSplitOptions.None)[0];

                val = WebUtility.HtmlDecode(val);

                nameVal.Add(name, val);
            }

            var content2 = new FormUrlEncodedContent(nameVal);
            
            HttpResponseMessage response2 = await client.PostAsync(action , content2);

            //sept 1 17 00:00:00 - Aug 31 18 23:59:59
            var start = "1504224000";
            var end = "1535759999";
            //var room = "000202065";
            var timetableDataUrl = "https://timetable.soton.ac.uk/api/Timetable/Location/" + room + "?start=" + start + "&end=" + end + "&isDraft=false";
            HttpResponseMessage response3 = await client.GetAsync(timetableDataUrl);
            HttpContent pageContent3 = response3.Content;

            string result3 = await pageContent3.ReadAsStringAsync();

            Console.WriteLine(result3);
        }
    }
}
