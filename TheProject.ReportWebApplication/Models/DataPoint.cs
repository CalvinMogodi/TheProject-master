using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace TheProject.ReportWebApplication.Models
{
    //DataContract for Serializing Data - required to serve in JSON format
    [DataContract]
    public class DataPoint
    {
        public DataPoint(string label, string color, int total, double y, double percentage, string id)
        {
            this.Label = label;
            this.Y = y;
            this.Color = color;
            this.Total = total;
            this.Percentage = percentage;
            this.Id = id;
        }

        //Explicitly setting the name to be used while serializing to JSON.
        [DataMember(Name = "label")]
        public string Label = "";

        //Explicitly setting the name to be used while serializing to JSON.
        [DataMember(Name = "color")]
        public string Color = "";

        //Explicitly setting the name to be used while serializing to JSON.
        [DataMember(Name = "total")]
        public Nullable<int> Total = null;

        //Explicitly setting the name to be used while serializing to JSON.
        [DataMember(Name = "y")]
        public Nullable<double> Y = null;

        //Explicitly setting the name to be used while serializing to JSON.
        [DataMember(Name = "percentage")]
        public Nullable<double> Percentage = null;        

        //Explicitly setting the name to be used while serializing to JSON.
        [DataMember(Name = "id")]
        public string Id = null;        
    }
}