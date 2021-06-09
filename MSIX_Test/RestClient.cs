using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace MSIX_Test
{
    public class RestClient
    {
        protected HttpClient client { get; set; }

        public RestClient()
        {
            client = new HttpClient();
        }
        public async Task<Stream> GetStream(string api, object param = null)
        {
            var res = await client.GetAsync(GetParamApi(api, param), HttpCompletionOption.ResponseHeadersRead);
            return await res.Content.ReadAsStreamAsync();
        }

        protected string GetParamApi(string api, object param = null)
        {
            if (param != null)
            {
                var query = HttpUtility.ParseQueryString(string.Empty);
                var json = JsonConvert.SerializeObject(param);
                var jobj = JObject.Parse(json);
                foreach (var keyValue in jobj)
                {
                    query[keyValue.Key] = keyValue.Value.ToString();
                }
                api += "?" + query.ToString();
            }
            return api;
        }
    }
}
