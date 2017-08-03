using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EmailFinderLib
{
    public class Place
    {
        [JsonProperty("name")]
        public string Name { get; set; }        // Name
        [JsonProperty("vicinity")]
        public string Address { get; set; }     // Address
        [JsonProperty("place_id")]
        public string PlaceId { get; set; }     //Place Id
        [JsonProperty("geometry")]
        public Geometry Geo { get; set; }       // Co-ordinates

        async public Task<Detail> GetDetails()
        {
            return await Api.GetDetails(this.PlaceId);
        }
    }

    public class Detail
    {
        [JsonProperty("name")]
        public string Name { get; set; }        // Name
        [JsonProperty("formatted_address")]
        public string Address { get; set; }     // Address
        [JsonProperty("formatted_phone_number")]
        public string Phone { get; set; }       // Phone Number
        [JsonProperty("website")]
        public string WebSite { get; set; }  //Website
        [JsonProperty("geometry")]
        public Geometry Geo { get; set; }       // Co-ordinates
    }
    public class Geometry
    {
        [JsonProperty("location")]
        public Location Location { get; set; }
    }

    public class Location
    {
        [JsonProperty("lat")]
        public double Latitude { get; set; }
        [JsonProperty("lng")]
        public double Longitude { get; set; }
    }

    public class Response
    {
        [JsonProperty("result")]
        public Detail Detail { get; set; }
        [JsonProperty("results")]
        public List<Place> Places { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
    }

    public class EmailList
    {
        public string Name { get; set; }
        public string Domain { get; set; }
        public string Email { get; set; }
        public Geometry Geo { get; set; }
    }
}

