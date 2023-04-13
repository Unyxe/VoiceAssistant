using System;
using System.Collections.Generic;
using System.Media;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Media;

namespace BARDApp
{
    internal class Program
    {
        public static string[] history_ids = new string[3] { "", "", "" };
        public static string[] api_keys = new string[2] { "VAhUKMq12cSDPx6RO9j01XNTMIarVNr0S6ErgUVHVeaLiwDVu3btSAI_pLcuqlHqYdjrAA.", "UwjYyFFeY6G06k5aKhIumFRaEaG2GHXewYakzlAQEryGwAsElsSlkQjJUDVsPvsCLAsRXQ." };
        public static string[] at_vars = new string[2] { "ABi_lZiMCLkX08DeqRzeeujgNLqp%3A1681341296079", "ABi_lZh4X1Gg449HStgiwwXbp4bo%3A1680948998927" };
        public static string[] elevenlabs_api_keys = new string[2] { "0ff4c5ad57d753f16e9f6c6d1bb844e0", "" }; 
        public static int api_key_num = 0;
        static void Main(string[] args)
        {
            //TextToSpeech("Hi");
            string last_msg = "Hi, what do you think about AI?";
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n");
                last_msg = Console.ReadLine();
                Console.WriteLine("\n");
                Console.ForegroundColor = ConsoleColor.Red;
                last_msg = ParseResponse(GetRawResponse(last_msg.Replace("\n", string.Empty) + " Respond in a chatty shortened form."));
                Console.WriteLine(last_msg);
                TextToSpeech(last_msg);
            }
        }

        public static string GetRawResponse(string input)
        {
            var handler = new HttpClientHandler()
            {
                AllowAutoRedirect = false
            };

            var client = new HttpClient(handler);

            var request = new HttpRequestMessage(HttpMethod.Post, "https://bard.google.com/_/BardChatUi/data/assistant.lamda.BardFrontendService/StreamGenerate?bl=boq_assistant-bard-web-server_20230404.15_p0&_reqid=1440589");

            var cookie = new Cookie("__Secure-1PSID", api_keys[api_key_num])
            {
                Secure = true,
                HttpOnly = true,
                Domain = "bard.google.com"
            };

            handler.CookieContainer.Add(cookie);
            request.Headers.TryAddWithoutValidation("Origin", "https://bard.google.com");

            var content = new StringContent("f.req=%5Bnull%2C%22%5B%5B%5C%22"+input+ "%5C%22%5D%2Cnull%2C%5B%5C%22" + history_ids[0] +"%5C%22%2C%5C%22" + history_ids[1] + "%5C%22%2C%5C%22" + history_ids[2] + "%5C%22%5D%5D%22%5D&at=" + at_vars[api_key_num] +"&", Encoding.UTF8, "application/x-www-form-urlencoded");

            request.Content = content;

            var response = client.SendAsync(request).GetAwaiter().GetResult();

            var responseContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            return responseContent;
        }
        public static string ParseResponse(string input)
        {
            int offset = 6;
            string offsetted  = input.Substring(offset, input.Length - offset);
            string json = offsetted; 

            var substr = JsonNode.Parse(json);

            //Console.WriteLine(substr[0][2].ToString());
            if(substr[0][2] == null)
            {
                return "Rate limit is reached!";
            }
            json = substr[0][2].ToString();
            substr = JsonNode.Parse(json);
            history_ids[0] = substr[1][0].ToString();
            history_ids[1] = substr[1][1].ToString();
            history_ids[2] = substr[4][0][0].ToString();
            return substr[0][0].ToString();
        }

        public static void TextToSpeech(string input)
        {
            WebProxy proxy = new WebProxy
            {
                Address = new Uri("http://127.0.0.1:8080"),
            };
            var handler = new HttpClientHandler()
            {
                AllowAutoRedirect = false,
                //Proxy = proxy
            };

            var client = new HttpClient(handler);

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.elevenlabs.io/v1/text-to-speech/" + "21m00Tcm4TlvDq8ikWAM");

            request.Headers.TryAddWithoutValidation("xi-api-key", elevenlabs_api_keys[0]);

            var content = new StringContent("{\r\n  \"text\": \""+input.Replace("\n", String.Empty)+"\",\r\n  \"voice_settings\": {\r\n    \"stability\": 0,\r\n    \"similarity_boost\": 0\r\n  }\r\n}", Encoding.UTF8, "application/json");

            request.Content = content;

            var response = client.SendAsync(request).GetAwaiter().GetResult();

            var responseContent = response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();

            Random rn = new Random();
            string path = System.IO.Path.GetTempPath() +rn.Next() +@".mp3";
            File.WriteAllBytes(path,responseContent);

            MediaPlayer pl = new MediaPlayer();

            pl.Open(new Uri(path));
            pl.Volume = 3.0f;
            pl.Play();
            //pl = null;
            //File.Delete(path);
        }

    }
}
