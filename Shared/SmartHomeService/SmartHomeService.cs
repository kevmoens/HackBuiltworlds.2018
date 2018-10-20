using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Shared.SmartHomeService
{
    public class SmartHomeService
    {
        public void SetURI(string uri)
        {
            Session.URL = uri;
        }


        public async Task<string> GetData(string value)
        {
            HttpClient client;
            client = new HttpClient();

            Uri uri = new Uri(string.Format(Session.URL + "/GetData/{0}", value));
            var request = new HttpRequestMessage()
            {
                RequestUri = uri
                ,Method = HttpMethod.Get
            };
            //request.Headers.Add("Authorization", Session.SessionID);
            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            return "Unable to connect to service";

        }



        public async Task<OpenSessionResult> OpenSession(string value)
        {
            HttpClient client;
            client = new HttpClient();

            Uri uri = new Uri(string.Format(Session.URL + "/OpenSession/{0}", value));
            var request = new HttpRequestMessage()
            {
                RequestUri = uri
                ,
                Method = HttpMethod.Get
            };
            //request.Headers.Add("Authorization", Session.SessionID);
            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                string result = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<OpenSessionResult>(result);
            }
            return new OpenSessionResult() { Error = "Hololens request error" };

        }


        public async Task<List<BulbAddedDto>> GetNotes()
        {
            HttpClient client;
            client = new HttpClient();

            Uri uri = new Uri(string.Format(Session.URL + "/GetNotes"));
            var request = new HttpRequestMessage()
            {
                RequestUri = uri
                ,
                Method = HttpMethod.Get
            };
            //request.Headers.Add("Authorization", Session.SessionID);
            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                string result = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<BulbAddedDto>>(result);
            }
            return new List<BulbAddedDto>();

        }

        public async Task Ping()
        {
            HttpClient client;
            client = new HttpClient();

            Uri uri = new Uri(string.Format(Session.URL + "/Ping"));
            var request = new HttpRequestMessage()
            {
                RequestUri = uri
    ,
                Method = HttpMethod.Get
            };
            var response = await client.SendAsync(request);


        }

        public async Task Reset()
        {
            HttpClient client;
            client = new HttpClient();

            Uri uri = new Uri(string.Format(Session.URL + "/Reset"));
            var request = new HttpRequestMessage()
            {
                RequestUri = uri
    ,
                Method = HttpMethod.Get
            };
            var response = await client.SendAsync(request);


        }

        public async Task AddNote(BulbAddedDto Note)
        {
            HttpClient client;
            client = new HttpClient();
            Uri uri = new Uri(Session.URL + "/AddNote"); //" + query);
            var request = new HttpRequestMessage()
            {
                RequestUri = uri
                ,
                Method = HttpMethod.Post
            };
            request.Content = new StringContent(JsonConvert.SerializeObject(Note));
            //request.Headers.Add("Authorization", Session.SessionID);
            var response = await client.SendAsync(request);
            //if (response.IsSuccessStatusCode)
            //{
            //    return await response.Content.ReadAsStringAsync();
            //}
            //return "";

        }
    }
}
