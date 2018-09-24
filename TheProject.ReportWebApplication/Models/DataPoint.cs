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
        public DataPoint(string label, string color, string icon, double y)
        {
            this.Label = label;
            this.Y = y;
            this.Color = color;
            this.Icon = icon;
        }

        //Explicitly setting the name to be used while serializing to JSON.
        [DataMember(Name = "label")]
        public string Label = "";

        //Explicitly setting the name to be used while serializing to JSON.
        [DataMember(Name = "color")]
        public string Color = "";

        //Explicitly setting the name to be used while serializing to JSON.
        [DataMember(Name = "icon")]
        public string Icon = "";

        //Explicitly setting the name to be used while serializing to JSON.
        [DataMember(Name = "y")]
        public Nullable<double> Y = null;
    }
}