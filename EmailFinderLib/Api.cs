using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;

namespace EmailFinderLib
{
    public static class Api
    {
        private static HttpClient Client { get; set; }
        static string apiKey = "AIzaSyB15CUlTjBgqJU3o2GJ35XK0OyHhgpPLyU";

        static Api()
        {
            Client = new HttpClient();
            Client.BaseAddress = new Uri("https://maps.googleapis.com/maps/api/place/");
        }

        public static void SetApiKey(string key)
        {
            apiKey = key;
        }
        /// <summary>
        /// Gets stores of a certain category near the specified co-ordinates. Can be show only
        /// open stores or all stores.
        /// </summary>
        /// <param name="lat">Latitude of user.</param>
        /// <param name="lng">Longitude of user.</param>
        /// <param name="type">Category of stores to find.</param>
        /// <param name="radius">Zone value</param>
        async public static Task<Response> GetPlaces(string lat, string lng, string radius, string type)
        {
            string url = String.Format("nearbysearch/json?key={0}&location={1},{2}&radius={4}&keyword={3}", apiKey, lat, lng, type, radius);
            try
            {
                var resp = await Client.GetAsync(url);
                if (resp.IsSuccessStatusCode)
                {
                    var res = await resp.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<Response>(res);
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get full details of specified place ID.
        /// </summary>
        /// <param name="placeId">ID of place.</param>
        async public static Task<Detail> GetDetails(string placeId)
        {
            try
            {
                var resp = await Client.GetAsync(String.Format("details/json?key={0}&placeid={1}&sensor=true", apiKey, placeId));
                if (resp.IsSuccessStatusCode)
                {
                    return (JsonConvert.DeserializeObject(await resp.Content.ReadAsStringAsync(), typeof(Response)) as Response).Detail;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
