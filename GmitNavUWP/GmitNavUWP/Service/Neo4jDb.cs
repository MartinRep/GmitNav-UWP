using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace GmitNavUWP.Service
{
    class Neo4jDb
    {
        public async Task<JsonObject> CypherAsync(String cypher, JsonObject parameters)
        {
            Chilkat.Http http = new Chilkat.Http();
            
            //bool success;

            ////  Any string unlocks the component for the 1st 30-days.
            //success = http.UnlockComponent("Anything for 30-day trial");
            //if (success != true)
            //{
            //    Debug.WriteLine(http.LastErrorText);
            //    return;
            //}

            //  Set the Login and Password properties for authentication.
            http.Login = Util.Neo4j.username;
            http.Password = Util.Neo4j.password;
            JsonObject cypherJson = JsonObject.Parse(cypher);
            cypherJson.Add("statement", cypherJson);
            JsonArray request = new JsonArray();
            request.Add(cypherJson);
            request.Add(parameters);

            Chilkat.HttpResponse response = await http.PostJsonAsync(Util.Neo4j.uri, request.ToString());
            JsonObject resultJsonData = JsonObject.Parse(response.BodyStr);
            // string html = await http.QuickGetStrAsync(Util.Neo4j.uri);
            //  Note:
            //if (http.LastMethodSuccess != true)
            //{
            //    Debug.WriteLine(http.LastErrorText);
            //    return null;
            //}

            //  Examine the HTTP status code returned.
            //  A status code of 401 is typically returned for "access denied";
            //  if no login/password is provided, or if the credentials (login/password)
            //  are incorrect.
            Debug.WriteLine("HTTP status code for Basic authentication: " + Convert.ToString(http.LastStatus));

            //  Examine the HTML returned for the URL:
            //Debug.WriteLine(html);

            


            return resultJsonData;
        }
    }
}
