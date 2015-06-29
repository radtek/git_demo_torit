﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RDN.League.Models.Enum;
using RDN.League.Models.Filters;
using RDN.League.Models.Utilities;
using RDN.Library.Classes.Error;
using RDN.Library.Classes.Location;
using RDN.Library.Util;
using RDN.Library.Util.Enum;
using RDN.Library.Classes.Location;
using RDN.Library.Classes.Store;
using RDN.Library.Cache;

namespace RDN.League.Controllers
{
#if !DEBUG
[RequireHttps] //apply to all actions in controller
#endif
    public class LocationController : BaseController
    {

        [Authorize]
        public ActionResult EditLocation(Guid id)
        {
            try
            {
                var location = RDN.Library.Classes.Location.LocationFactory.GetLocation(id);
                if (location == null)
                    return Redirect(Url.Content("~/?u=" + SiteMessagesEnum.na));

                RDN.League.Models.Location.NewLocation loc = new Models.Location.NewLocation();
                loc.LocationId = location.LocationId;
                loc.LocationName = location.LocationName;

                var add = location.Contact.Addresses.FirstOrDefault();
                if (add != null)
                {
                    loc.Address1 = add.Address1;
                    loc.Address2 = add.Address2;
                    loc.CityRaw = add.CityRaw;
                    loc.StateRaw = add.StateRaw;
                    loc.Zip = add.Zip;
                    loc.Country = add.CountryId;
                }
                loc.Website = location.Website;

                var countries = RDN.Library.Classes.Location.LocationFactory.GetCountries();
                loc.Countries = new SelectList(countries, "CountryId", "Name");

                return View(loc);
            }
            catch (Exception exception)
            {
                ErrorDatabaseManager.AddException(exception, exception.GetType());
            }
            return Redirect(Url.Content("~/?u=" + SiteMessagesEnum.sww));
        }

        [HttpPost]
        [Authorize]
        [LeagueAuthorize(EmailVerification = true, IsInLeague = true)]
        public ActionResult EditLocation(Models.Location.NewLocation location)
        {
            try
            {
                var id = RDN.Library.Classes.Location.LocationFactory.UpdateLocation(location.LocationId, location.LocationName, location.Address1, location.Address2, location.CityRaw, location.Country, location.StateRaw, location.Zip, location.Website, location.OwnerId);
                var countries = RDN.Library.Classes.Location.LocationFactory.GetCountries();
                location.Countries = new SelectList(countries, "CountryId", "Name");
                return Redirect(Url.Content("~/location/all?u=" + SiteMessagesEnum.su));
            }
            catch (Exception exception)
            {
                ErrorDatabaseManager.AddException(exception, exception.GetType());
            }
            return Redirect("~/?u=" + SiteMessagesEnum.sww);
        }


        //[HttpPost]
        //[Authorize]
        //public ActionResult EditPaywall(Paywall pw)
        //{
        //    try
        //    {
        //        string priceDaily = HttpContext.Request.Form["Price"];
        //        string priceFull = HttpContext.Request.Form["FullPrice"];
        //        if (!String.IsNullOrEmpty(priceDaily))
        //            pw.DailyPrice = Convert.ToDecimal(priceDaily);
        //        if (!String.IsNullOrEmpty(priceFull))
        //            pw.TimespanPrice = Convert.ToDecimal(priceFull);
        //        pw.OwnerId = RDN.Library.Classes.Account.User.GetMemberId();
        //        DateTime sd = new DateTime();

        //        bool successsd = DateTime.TryParse(pw.StartDateDisplay, out sd);
        //        if (successsd)
        //            pw.StartDate = sd;
        //        else
        //            pw.StartDate = null;

        //        DateTime ed = new DateTime();

        //        bool successed = DateTime.TryParse(pw.EndDateDisplay, out ed);
        //        if (successed)
        //            pw.EndDate = ed;
        //        else
        //            pw.EndDate = null;



        //        var wall = pw.UpdatePaywall(pw);

        //        if (wall == null)
        //            return Redirect(Url.Content("~/?u=" + SiteMessagesEnum.na));
        //        return Redirect(Url.Content("~/paywall/all?u=" + SiteMessagesEnum.su));
        //    }
        //    catch (Exception exception)
        //    {
        //        ErrorDatabaseManager.AddException(exception, exception.GetType());
        //    }
        //    return Redirect(Url.Content("~/?u=" + SiteMessagesEnum.sww));
        //}


        [Authorize]
        [LeagueAuthorize(EmailVerification = true, IsInLeague = true, IsManager = false)]
        public ActionResult DeleteLocation(string lId)
        {
            try
            {
                var locations = RDN.Library.Classes.Location.LocationFactory.RemoveLocation(new Guid(lId));
                return Json(new { isSuccess = locations }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception)
            {
                ErrorDatabaseManager.AddException(exception, exception.GetType());
            }
            return Json(new { isSuccess = false }, JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        [LeagueAuthorize(EmailVerification = true, IsInLeague = true, IsManager = false)]
        public ActionResult AllLocations()
        {

            try
            {
                NameValueCollection nameValueCollection = HttpUtility.ParseQueryString(Request.Url.Query);
                string u = nameValueCollection["u"];
                if (u == SiteMessagesEnum.s.ToString())
                {
                    SiteMessage message = new SiteMessage();
                    message.MessageType = SiteMessageType.Success;
                    message.Message = "Successfully Added New Location.";
                    this.AddMessage(message);
                }
                else if (u == SiteMessagesEnum.su.ToString())
                {
                    SiteMessage message = new SiteMessage();
                    message.MessageType = SiteMessageType.Success;
                    message.Message = "Successfully Edited Location.";
                    this.AddMessage(message);
                }

                var id = MemberCache.GetCalendarIdForMemberLeague(RDN.Library.Classes.Account.User.GetMemberId());
                var locations = RDN.Library.Classes.Calendar.CalendarFactory.GetLocationsOnlyOfCalendar(id);
                return View(locations);
            }
            catch (Exception exception)
            {
                ErrorDatabaseManager.AddException(exception, exception.GetType());
            }
            return Redirect(Url.Content("~/?u=" + SiteMessagesEnum.sww));
        }
        [Authorize]
        [LeagueAuthorize(EmailVerification = true, IsInLeague = true, IsManager = false)]
        public ActionResult NewLocation(string ownerType, string redirectto, string id)
        {
            Models.Location.NewLocation location = new Models.Location.NewLocation();
            try
            {
                NameValueCollection nameValueCollection = HttpUtility.ParseQueryString(Request.Url.Query);
                string updated = nameValueCollection["u"];

                if (!String.IsNullOrEmpty(updated) && updated == SiteMessagesEnum.sal.ToString())
                {
                    SiteMessage message = new SiteMessage();
                    message.MessageType = SiteMessageType.Success;
                    message.Message = "Successfully Added New Location.";
                    this.AddMessage(message);
                }

                location.OwnerId = new Guid(id);
                location.OwnerType = ownerType;
                location.RedirectTo = redirectto;
                var countries = RDN.Library.Classes.Location.LocationFactory.GetCountries();
                location.Countries = new SelectList(countries, "CountryId", "Name");
            }
            catch (Exception exception)
            {
                ErrorDatabaseManager.AddException(exception, exception.GetType());
            }
            return View(location);
        }

        [HttpPost]
        [Authorize]
        [LeagueAuthorize(EmailVerification = true, IsInLeague = true)]
        public ActionResult NewLocation(Models.Location.NewLocation location)
        {
            try
            {
                // if a create another was clicked instead of just submitting the event.
                bool createAnother = false;
                if (Request.Form["createAnother"] != null)
                    createAnother = true;

                var locType = (LocationOwnerType)Enum.Parse(typeof(LocationOwnerType), location.RedirectTo);
                var id = RDN.Library.Classes.Location.LocationFactory.CreateNewLocation(locType, location.LocationName, location.Address1, location.Address2, location.CityRaw, location.Country, location.StateRaw, location.Zip, location.Website, location.OwnerId);

                if (!createAnother)
                {
                    if (locType == LocationOwnerType.calendar)
                        return Redirect(Url.Content("~/calendar/new/" + location.OwnerType + "/" + location.OwnerId.ToString().Replace("-", "") + "/" + id.LocationId.ToString().Replace("-", "")));
                    else if (locType == LocationOwnerType.shop)
                    {
                        var sg = new StoreGateway();
                        var realStore = sg.GetStoreForManager(location.OwnerId);
                        return Redirect(Url.Content("~/store/settings/" + realStore.PrivateManagerId.ToString().Replace("-", "") + "/" + realStore.MerchantId.ToString().Replace("-", "")));
                    }
                }
                else
                {
                    return Redirect(HttpContext.Request.Url.AbsoluteUri + "?u=" + SiteMessagesEnum.sal);
                }
            }
            catch (Exception exception)
            {
                ErrorDatabaseManager.AddException(exception, exception.GetType());
            }
            return View(location);
        }

     
    }



}
