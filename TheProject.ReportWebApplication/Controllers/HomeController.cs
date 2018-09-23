using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TheProject.ReportWebApplication.Models;
using TheProject.ReportWebApplication.Services;
using System.Data.Entity;
using TheProject.Data;

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
            List<Facility> facilities = GetSubmittedFacilities();

            List<string> regions = facilities.Select(d => d.Region).Distinct().ToList();

            ViewBag.Regions = new SelectList(regions);
            return View();
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
                    facilities.Add(new Facility
                    {
                        ClientCode = item.ClientCode,
                        SettlementType = item.SettlementType,
                        Zoning = item.Zoning,
                        Region = item.Location.Region
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