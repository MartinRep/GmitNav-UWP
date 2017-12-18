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
            //Creating basic authentication header.
            String authInfo = Util.Neo4j.username + ":" + Util.Neo4j.password;
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Authorization", "Basic " + authInfo);
            // Db Query
            string postData = @"{""statements"":[{""statement"": """ + cypher + "\"" + @"}}]}";
            // Actuall call to server
            HttpResponseMessage respond = await Request(Util.Neo4j.uri, postData, headers);
            if (!respond.IsSuccessStatusCode) return null;  // Error handling 
            return await respond.Content.ReadAsStringAsync();
        }

        // Simle HttpClient method. General
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
