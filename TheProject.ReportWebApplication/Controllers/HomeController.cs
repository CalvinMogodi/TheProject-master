using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TheProject.ReportWebApplication.Models;
using TheProject.ReportWebApplication.Services;
using System.Data.Entity;
using TheProject.Data;
using Newtonsoft.Json;
using System.Data;

namespace TheProject.ReportWebApplication.Controllers
{
    public class HomeController : BaseController
    {
        #region Properties
        private FacilityService facilityService;
        int _defaultPageSize = 20;

        #endregion
        public ActionResult Index()
        {           
            List<Facility> facilities = GetFacilities();
            List<Facility> SubmittedFacilities = facilities.Where(ss => ss.Status == "Submitted").ToList();

            List<string> regions = SubmittedFacilities.Select(d => d.Region).Distinct().ToList();

            ViewData["PropertiesCount"] = SubmittedFacilities.Count;
            int value1 = SubmittedFacilities.Count;
            int value2 = facilities.Count;
            decimal div = decimal.Divide(value1, value2);
            string propertiesPercentage = String.Format("{0:.##}", (div * 100));
            propertiesPercentage = propertiesPercentage.Replace(",", ".");
            ViewData["NoOfImprovements"] = SubmittedFacilities.Sum(f => f.NoOfImprovements);
            ViewData["ImprovementsSize"] = SubmittedFacilities.Sum(f => f.ImprovementsSize);
            ViewData["OccupationStatus"] = String.Format("{0:.##}", (SubmittedFacilities.Sum(f => f.OccupationStatus)));

            ViewData["PropertiesPercentage"] = propertiesPercentage;
            ViewBag.Regions = new SelectList(regions);
            return View();
        }

        [HttpPost]
        public JsonResult NewChart(string selectedRegion)
        {
            List<object> iData = new List<object>();
            //Creating sample data  
            DataTable dt = new DataTable();
            dt.Columns.Add("Employee", System.Type.GetType("System.String"));
            dt.Columns.Add("Credit", System.Type.GetType("System.Int32"));

            List<Facility> facilities = GetFacilities();
            if (!string.IsNullOrEmpty(selectedRegion))
            {
                List<Facility> sortedFacilities = new List<Facility>();
                foreach (var item in facilities)
                {
                    if (!string.IsNullOrEmpty(item.Region))
                    {
                        if(item.Region.ToLower().Trim() == selectedRegion.ToLower().Trim())
                        {
                            sortedFacilities.Add(item);
                        }       
                       
                    }
                  
                }
                
                facilities = sortedFacilities;
            }

            List<Facility> SubmittedFacilities = facilities.Where(ss => ss.Status == "Submitted").ToList();
            var PropertiesCount = SubmittedFacilities.Count;
            int value1 = SubmittedFacilities.Count;
            int value2 = facilities.Count;
            decimal div = decimal.Divide(value1, value2);
            string propertiesPercentage = String.Format("{0:.##}", (div * 100));
            propertiesPercentage = propertiesPercentage.Replace(",", ".");
            var NoOfImprovements = SubmittedFacilities.Sum(f => f.NoOfImprovements);
            var ImprovementsSize = SubmittedFacilities.Sum(f => f.ImprovementsSize);
            var OccupationStatus = String.Format("{0:.##}", (SubmittedFacilities.Sum(f => f.OccupationStatus)));
            var PropertiesPercentage = propertiesPercentage;
            
            List<DataPoint> dataPoints = GetZoning(SubmittedFacilities);
            List<string> colors = new List<string>();
            var random = new Random();
            foreach (var item in dataPoints)
            {
                DataRow dr = dt.NewRow();
                dr["Employee"] = item.Label;
                dr["Credit"] = item.Y;
                dt.Rows.Add(dr);

                
                var color = string.Format("#{0:X6}", random.Next(0x1000000)); // = "#A197B9"
                colors.Add(color);
            }

           
            //Looping and extracting each DataColumn to List<Object>  
            foreach (DataColumn dc in dt.Columns)
            {
                List<object> x = new List<object>();
                x = (from DataRow drr in dt.Rows select drr[dc.ColumnName]).ToList();
                iData.Add(x);
            }

            iData.Add(colors);
            iData.Add(NoOfImprovements);
            iData.Add(PropertiesCount);
            iData.Add(ImprovementsSize);
            iData.Add(OccupationStatus);
            iData.Add(PropertiesPercentage);
            //Source data returned as JSON  
            return Json(iData, JsonRequestBehavior.AllowGet);
        }

        private List<DataPoint> GetZoning(List<Facility> facilities) {

            List<DataPoint> dataPoints = new List<DataPoint>();
            List<string> zonings = facilities.Select(d => d.Zoning).Distinct().ToList();
            List<Facility> newfacilities = new List<Facility>();
            foreach (var item in facilities)
            {
                if (!string.IsNullOrEmpty(item.Zoning))
                {
                    newfacilities.Add(item);
                }
                }
            foreach (var zoning in zonings)
            {
                if (!string.IsNullOrEmpty(zoning))
                {

                    var zoningCount = newfacilities.Where(d => d.Zoning.ToLower().Trim() == zoning.ToLower().Trim()).ToList();
                    dataPoints.Add(new DataPoint(zoning, zoningCount.Count));
                }                
            }
            
            return dataPoints;
        }

        private List<Facility> GetSubmittedFacilities()
        {
            using (ApplicationUnit unit = new ApplicationUnit())
            {
                var dbfacilities = unit.Facilities.GetAll()
                                      .Include(b => b.Buildings)
                                      .Include(d => d.DeedsInfo)
                                      .Include(p => p.ResposiblePerson)
                                      .Include("Location.GPSCoordinates")
                                      .Include("Location.BoundryPolygon")
                                      .Where(ss => ss.Status == "Submitted")
                                      .ToList();
                List<Facility> facilities = new List<Facility>();
                foreach (var item in dbfacilities)
                {
                    double utiliatonStatusTotal = item.Buildings.Sum(b => Convert.ToDouble(b.Status));
                    facilities.Add(new Facility
                    {
                        ClientCode = item.ClientCode,
                        SettlementType = item.SettlementType,
                        Zoning = item.Zoning,
                        Region = item.Location.Region,
                        NoOfImprovements = item.Buildings.Count,
                        ImprovementsSize = item.Buildings.Sum(b => b.ImprovedArea),
                        OccupationStatus = item.Buildings.Count != 0 ? utiliatonStatusTotal / item.Buildings.Count : utiliatonStatusTotal,
                        Status = item.Status
                    });
                }
                return facilities;
            }
        }

        private List<Facility> GetFacilities()
        {
            using (ApplicationUnit unit = new ApplicationUnit())
            {
                var dbfacilities = unit.Facilities.GetAll()
                                      .Include(b => b.Buildings)
                                      .Include(d => d.DeedsInfo)
                                      .Include(p => p.ResposiblePerson)
                                      .Include("Location.GPSCoordinates")
                                      .Include("Location.BoundryPolygon")
                                      .ToList();
                List<Facility> facilities = new List<Facility>();
                foreach (var item in dbfacilities)
                {
                    double utiliatonStatusTotal = item.Buildings.Sum(b => Convert.ToDouble(b.Status));

                    facilities.Add(new Facility
                    {
                        ClientCode = item.ClientCode,
                        SettlementType = item.SettlementType,
                        Zoning = item.Zoning,
                        Region = item.Location.Region,
                        NoOfImprovements = item.Buildings.Count,
                        ImprovementsSize = item.Buildings.Sum(b => b.ImprovedArea),
                        OccupationStatus = item.Buildings.Count != 0 ? utiliatonStatusTotal / item.Buildings.Count : utiliatonStatusTotal,
                        Status = item.Status
                    });

                }
                return facilities;
            }
        }

        public ActionResult Logout()
        {
            return RedirectToAction("Login", "Account");
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}