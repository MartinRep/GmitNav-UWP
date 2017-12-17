using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;


namespace GmitNavUWP.Service
{
    class Neo4jDb
    {
        public async Task<String> CypherAsync(String cypher, String parameteres)
        {
            String authInfo = Util.Neo4j.username + ":" + Util.Neo4j.password;
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Authorization", "Basic " + authInfo);
            string postData = @"{""statements"":[{""statement"": """ + cypher + "\"" + @"}}]}";
            HttpResponseMessage respond = await Request(Util.Neo4j.uri, postData, headers);
            if (!respond.IsSuccessStatusCode) return null;
            return await respond.Content.ReadAsStringAsync();
        }

        private async Task<HttpResponseMessage> Request( string pUrl, string pJsonContent, Dictionary<string, string> pHeaders)
        {
            HttpClient client = new HttpClient();
            var httpRequestMessage = new HttpRequestMessage();
            httpRequestMessage.Method = HttpMethod.Post;
            httpRequestMessage.RequestUri = new Uri(pUrl);
            foreach (var head in pHeaders)
            {
                httpRequestMessage.Headers.Add(head.Key, head.Value);
            }
            HttpContent httpContent = new StringContent(pJsonContent, Encoding.UTF8, "application/json");
            httpRequestMessage.Content = httpContent;
            return await client.SendAsync(httpRequestMessage);
        }
    }
}
