﻿using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RDN.Library.Cache;
using RDN.Library.Classes.Error;
using RDN.Utilities.Config;
using RDN.Utilities.Distance;
using RDN.Portable.Config;
using RDN.Portable.Models.Json.Calendar;
using RDN.Utilities.Dates;
using RDN.Library.Classes.Calendar;
using RDN.Portable.Classes.Controls.Calendar;
using System.IO;
using RDN.Portable.Network;
using RDN.Portable.Classes.Controls.Calendar.Enums;
using RDN.Library.Classes.Controls.Calendar;
using RDN.Library.Classes.Calendar.Enums;
using ScheduleWidget.Enums;
using RDN.Library.Classes.Colors;
using RDN.Portable.Classes.Colors;
using RDN.Portable.Classes.Account.Classes;

namespace RDN.Api.Controllers
{
    public class CalendarController : Controller
    {
        public static int PULL_COUNT = 5;
        public JsonResult CalendarEvents(string lId)
        {
            var now = DateTime.UtcNow;

            EventsJson leagues = new EventsJson();
            try
            {
                Guid leagueId = new Guid(lId);
                var events = SiteCache.GetCalendarEvents(leagueId, PULL_COUNT, now);

                leagues.Count = events.Events.Count;
                foreach (var e in events.Events)
                {
                    EventJson j = new EventJson();
                    j.Address = e.Address;
                    j.LeagueId = lId;
                    j.CalendarItemId = e.CalendarItemId.ToString().Replace("-", "");
                    j.EndDate = e.EndDate;
                    j.Name = e.Name;
                    j.NameUrl = e.NameUrl;
                    j.StartDate = e.StartDate;
                    j.Description = e.Notes;
                    j.EventUrl = e.Link;
                    j.TicketUrl = e.TicketUrl;
                    j.RDNUrl = ServerConfig.WEBSITE_DEFAULT_LOCATION + "/roller-derby-event/" + RDN.Utilities.Strings.StringExt.ToSearchEngineFriendly(e.Name) + "/" + j.CalendarItemId;
                    if (e.Location != null)
                        j.Location = e.Location.LocationName;
                    j.LeagueId = e.OrganizersId.ToString().Replace("-", "");
                    j.LogoUrl = e.ImageUrl;
                    j.OrganizersId = e.OrganizersId.ToString().Replace("-", "");
                    j.OrganizersName = e.OrganizersName;
                    leagues.Events.Add(j);

                }

                return Json(leagues, JsonRequestBehavior.AllowGet);

            }
            catch (Exception exception)
            {
                ErrorDatabaseManager.AddException(exception, exception.GetType());
            }
            return Json(leagues, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CalendarEventsAll(string p, string c)
        {
            var now = DateTime.UtcNow;

            EventsJson evs = new EventsJson();
            try
            {
                var events = SiteCache.GetCalendarEvents(now, Convert.ToInt32(p), Convert.ToInt32(c));

                evs.Count = events.Count;
                foreach (var e in events)
                {
                    EventJson j = new EventJson();
                    j.Address = e.Address;
                    j.LeagueId = e.OrganizersId.ToString().Replace("-", "");
                    j.CalendarItemId = e.CalendarItemId.ToString().Replace("-", "");
                    j.EndDate = e.EndDate;
                    j.Name = e.Name;
                    j.NameUrl = e.NameUrl;
                    j.StartDate = e.StartDate;
                    j.LogoUrl = e.ImageUrl;
                    j.OrganizersId = e.OrganizersId.ToString().Replace("-", "");
                    j.OrganizersName = e.OrganizersName;
                    j.Description = e.NotesHtml;
                    j.EventUrl = e.Link;
                    j.TicketUrl = e.TicketUrl;
                    j.RDNUrl = ServerConfig.WEBSITE_DEFAULT_LOCATION + "/roller-derby-event/" + RDN.Utilities.Strings.StringExt.ToSearchEngineFriendly(e.Name) + "/" + j.CalendarItemId;
                    if (e.Location != null)
                        j.Location = e.Location.LocationName;
                    evs.Events.Add(j);
                }

                return Json(evs, JsonRequestBehavior.AllowGet);

            }
            catch (Exception exception)
            {
                ErrorDatabaseManager.AddException(exception, exception.GetType());
            }
            return Json(evs, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SearchEventsAll(string p, string c, string s)
        {
            var now = DateTime.UtcNow;

            EventsJson evs = new EventsJson();
            try
            {
                var events = SiteCache.SearchCalendarEvents(now, s, Convert.ToInt32(p), Convert.ToInt32(c));

                evs.Count = events.Count;
                foreach (var e in events)
                {
                    EventJson j = new EventJson();
                    j.Address = e.Address;
                    j.LeagueId = e.OrganizersId.ToString().Replace("-", "");
                    j.CalendarItemId = e.CalendarItemId.ToString().Replace("-", "");
                    j.EndDate = e.EndDate;
                    j.Name = e.Name;
                    j.NameUrl = e.NameUrl;
                    j.StartDate = e.StartDate;
                    j.LogoUrl = e.ImageUrl;
                    j.OrganizersId = e.OrganizersId.ToString().Replace("-", "");
                    j.OrganizersName = e.OrganizersName;
                    j.Description = e.Notes;
                    j.EventUrl = e.Link;
                    j.TicketUrl = e.TicketUrl;
                    j.RDNUrl = ServerConfig.WEBSITE_DEFAULT_LOCATION + "/roller-derby-event/" + RDN.Utilities.Strings.StringExt.ToSearchEngineFriendly(e.Name) + "/" + j.CalendarItemId;
                    if (e.Location != null)
                        j.Location = e.Location.LocationName;
                    evs.Events.Add(j);
                }

                return Json(evs, JsonRequestBehavior.AllowGet);

            }
            catch (Exception exception)
            {
                ErrorDatabaseManager.AddException(exception, exception.GetType());
            }
            return Json(evs, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SearchEventsAllByLL(string p, string c, string lat, string lon)
        {
            var now = DateTime.UtcNow;

            EventsJson evs = new EventsJson();
            try
            {
                lat = lat.Replace(",", ".");
                lon = lon.Replace(",", ".");
                var latitude = Convert.ToDouble(lat);
                var longitude = Convert.ToDouble(lon);
                GeoCoordinate geo = new GeoCoordinate(latitude, longitude);
                var events = SiteCache.SearchCalendarEvents(now, longitude, latitude, Convert.ToInt32(p), Convert.ToInt32(c));

                evs.Count = events.Count;
                foreach (var e in events)
                {
                    EventJson j = new EventJson();
                    j.Address = e.Address;
                    j.LeagueId = e.OrganizersId.ToString().Replace("-", "");
                    j.CalendarItemId = e.CalendarItemId.ToString().Replace("-", "");
                    j.EndDate = e.EndDate;
                    j.Name = e.Name;
                    j.NameUrl = e.NameUrl;
                    j.StartDate = e.StartDate;
                    j.LogoUrl = e.ImageUrl;
                    j.OrganizersId = e.OrganizersId.ToString().Replace("-", "");
                    j.OrganizersName = e.OrganizersName;
                    j.Description = e.Notes;
                    j.EventUrl = e.Link;
                    j.TicketUrl = e.TicketUrl;
                    j.Latitude = e.Location.Contact.Addresses.FirstOrDefault().Coords.Latitude;
                    j.Longitude = e.Location.Contact.Addresses.FirstOrDefault().Coords.Longitude;
                    j.Miles = Conversion.ConvertMetersToMiles(geo.GetDistanceTo(new GeoCoordinate(j.Latitude, j.Longitude)));
                    j.RDNUrl = ServerConfig.WEBSITE_DEFAULT_LOCATION + "/roller-derby-event/" + RDN.Utilities.Strings.StringExt.ToSearchEngineFriendly(e.Name) + "/" + j.CalendarItemId;
                    if (e.Location != null)
                        j.Location = e.Location.LocationName;
                    evs.Events.Add(j);
                }

                return Json(evs, JsonRequestBehavior.AllowGet);

            }
            catch (Exception exception)
            {
                ErrorDatabaseManager.AddException(exception, exception.GetType());
            }
            return Json(evs, JsonRequestBehavior.AllowGet);
        }

        //#region Events

        public ActionResult EditEvent()
        {
            try
            {
                if (HttpContext.Request.InputStream != null)
                {
                    Stream stream = HttpContext.Request.InputStream;
                    var ob = Network.LoadObject<CalendarEventPortable>(ref stream);
                    var mem = MemberCache.GetMemberDisplay(ob.MemberId);
                    if (ob.UserId == mem.UserId)
                    {
                        var calEvent = CalendarEventFactory.UpdateEvent(ob.CalendarId, ob.CalendarItemId, ob.StartDate, ob.EndDate, ob.Location.LocationId, ob.Name, ob.Link, ob.Notes, ob.AllowSelfCheckIn, ob.EventType.CalendarEventTypeId, ob.TicketUrl, ob.ColorTempSelected, ob.IsPublicEvent, ob.GroupsForEvent.Select(x => x.Id).ToList());
                        ob.IsSuccessful = true;
                        return Json(ob, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception exception)
            {
                ErrorDatabaseManager.AddException(exception, exception.GetType());
            }
            return Json(new CalendarEventPortable(), JsonRequestBehavior.AllowGet);
        }
        //public ActionResult EditEvent(string type, string calId, string eventId)
        //{
        //    NewCalendarEvent cal = new NewCalendarEvent();
        //    try
        //    {
        //        var calEvent = CalendarEvent.GetEvent(new Guid(eventId), new Guid(calId));
        //        if (calEvent == null)
        //            return Redirect(Url.Content("~/calendar/" + type + "/" + calId));

        //        cal = new NewCalendarEvent(calEvent);
        //        cal.CalendarId = new Guid(calId);
        //        cal.CalendarType = type;
        //        cal.LeagueId = MemberCache.GetLeagueIdOfMember(RDN.Library.Classes.Account.User.GetMemberId());
        //        var colors = ColorDisplay.GetLeagueColors(cal.LeagueId);
        //        cal.ColorList = new SelectList(colors, "HexColor", "NameOfColor");

        //        var locs = RDN.Library.Classes.Calendar.Calendar.GetLocationsOfCalendar(new Guid(calId));
        //        var eventTypes = RDN.Library.Classes.Calendar.Calendar.GetEventTypesOfCalendar(new Guid(calId));
        //        var AllowSelfCheckin = Calendar.GetCalendar(new Guid(calId), (CalendarOwnerEntityEnum)Enum.Parse(typeof(CalendarOwnerEntityEnum), type));

        //        if (cal.Location != null)
        //            cal.Locations = new SelectList(locs, "LocationId", "LocationName", (object)cal.Location.LocationId);
        //        else
        //            cal.Locations = new SelectList(locs, "LocationId", "LocationName");

        //        if (cal.EventType != null)
        //            cal.EventTypes = new SelectList(eventTypes, "CalendarEventTypeId", "EventTypeName", (object)cal.EventType.CalendarEventTypeId);
        //        else
        //            cal.EventTypes = new SelectList(eventTypes, "CalendarEventTypeId", "EventTypeName");

        //        cal.AllowSelfCheckIn = AllowSelfCheckin.AllowSelfCheckIn;

        //        var repeatsFrequency = (from ScheduleWidget.Enums.FrequencyTypeEnum d in Enum.GetValues(typeof(ScheduleWidget.Enums.FrequencyTypeEnum))
        //                                where d.ToString() != "None"
        //                                select new SelectListItem { Value = ((int)d).ToString(), Text = d.ToString(), Selected = FrequencyTypeEnum.Weekly == d });

        //        if (String.IsNullOrEmpty(cal.RepeatsFrequencySelectedId))
        //            cal.RepeatsFrequencyDropDown = new SelectList(repeatsFrequency, "Value", "Text", ((object)2));
        //        else
        //            cal.RepeatsFrequencyDropDown = new SelectList(repeatsFrequency, "Value", "Text", ((object)cal.RepeatsFrequencySelectedId));

        //        cal.EndsOccurences = "0";
        //        List<string> repeatCount = new List<string>();
        //        for (int i = 1; i < 50; i++)
        //        {
        //            repeatCount.Add(i.ToString());
        //        }

        //    }
        //    catch (Exception exception)
        //    {
        //        ErrorDatabaseManager.AddException(exception, exception.GetType());
        //    }
        //    return View(cal);
        //}
        public ActionResult EditReoccurringEvent()
        {

            try
            {
                if (HttpContext.Request.InputStream != null)
                {
                    Stream stream = HttpContext.Request.InputStream;
                    var ob = Network.LoadObject<CalendarEventPortable>(ref stream);
                    var mem = MemberCache.GetMemberDisplay(ob.MemberId);
                    if (ob.UserId == mem.UserId)
                    {
                        FrequencyTypeEnum frequency = FrequencyTypeEnum.None;

                        switch (ob.Frequency)
                        {
                            case FrequencyTypeCalendarEnum.Daily:
                                frequency = FrequencyTypeEnum.Daily;
                                break;
                            case FrequencyTypeCalendarEnum.Monthly:
                                frequency = FrequencyTypeEnum.Daily;
                                break;
                            case FrequencyTypeCalendarEnum.Weekly:
                                frequency = FrequencyTypeEnum.Weekly;
                                break;
                        }

                        var calEvent = CalendarEventFactory.UpdateEventReOcurring(ob.CalendarId, ob.CalendarItemId, ob.StartDate, ob.EndDate, ob.Location.LocationId, ob.Name, ob.Link, ob.Notes, ob.AllowSelfCheckIn, frequency, ob.IsSunday, ob.IsMonday, ob.IsTuesday, ob.IsWednesday, ob.IsThursday, ob.IsFriday, ob.IsSaturday, ob.EndsWhenReoccuringEnum, ob.EndsWhenReoccuringEnum == EndsWhenReoccuringEnum.On ? Convert.ToDateTime(ob.EndDateReoccurring) : new DateTime(), ob.EventType.CalendarEventTypeId, 0, ob.ColorTempSelected, ob.IsPublicEvent, ob.GroupsForEvent.Select(x => x.Id).ToList(), mem.MemberId);
                        ob.IsSuccessful = true;
                        return Json(ob, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception exception)
            {
                ErrorDatabaseManager.AddException(exception, exception.GetType());
            }
            return Json(new CalendarEventPortable(), JsonRequestBehavior.AllowGet);
        }
        //public ActionResult EditReoccurringEvent(string type, string calId, string reoccuringEventId)
        //{
        //    NewCalendarEvent cal = new NewCalendarEvent();
        //    try
        //    {
        //        var calEvent = CalendarEvent.GetEventReocurring(new Guid(reoccuringEventId), new Guid(calId));
        //        if (calEvent == null)
        //            return Redirect(Url.Content("~/calendar/" + type + "/" + calId));

        //        cal = new NewCalendarEvent(calEvent);
        //        cal.CalendarId = new Guid(calId);
        //        cal.CalendarType = type;
        //        cal.LeagueId = MemberCache.GetLeagueIdOfMember(RDN.Library.Classes.Account.User.GetMemberId());
        //        var colors = ColorDisplay.GetLeagueColors(cal.LeagueId);
        //        cal.ColorList = new SelectList(colors, "HexColor", "NameOfColor");

        //        var locs = RDN.Library.Classes.Calendar.Calendar.GetLocationsOfCalendar(new Guid(calId));
        //        var eventTypes = RDN.Library.Classes.Calendar.Calendar.GetEventTypesOfCalendar(new Guid(calId));
        //        var AllowSelfCheckin = Calendar.GetCalendar(new Guid(calId), (CalendarOwnerEntityEnum)Enum.Parse(typeof(CalendarOwnerEntityEnum), type));

        //        if (cal.Location != null)
        //            cal.Locations = new SelectList(locs, "LocationId", "LocationName", (object)cal.Location.LocationId);
        //        else
        //            cal.Locations = new SelectList(locs, "LocationId", "LocationName");

        //        if (cal.EventType != null)
        //            cal.EventTypes = new SelectList(eventTypes, "CalendarEventTypeId", "EventTypeName", (object)cal.EventType.CalendarEventTypeId);
        //        else
        //            cal.EventTypes = new SelectList(eventTypes, "CalendarEventTypeId", "EventTypeName");

        //        cal.AllowSelfCheckIn = AllowSelfCheckin.AllowSelfCheckIn;

        //        var repeatsFrequency = (from ScheduleWidget.Enums.FrequencyTypeEnum d in Enum.GetValues(typeof(ScheduleWidget.Enums.FrequencyTypeEnum))
        //                                where d.ToString() != "None"
        //                                select new SelectListItem { Value = ((int)d).ToString(), Text = d.ToString(), Selected = FrequencyTypeEnum.Weekly == d });
        //        if (String.IsNullOrEmpty(cal.RepeatsFrequencySelectedId))
        //            cal.RepeatsFrequencyDropDown = new SelectList(repeatsFrequency, "Value", "Text", ((object)2));
        //        else
        //            cal.RepeatsFrequencyDropDown = new SelectList(repeatsFrequency, "Value", "Text", ((object)cal.RepeatsFrequencySelectedId));

        //        var montlhyInterval = (from ScheduleWidget.Enums.MonthlyIntervalEnum d in Enum.GetValues(typeof(ScheduleWidget.Enums.MonthlyIntervalEnum))
        //                               select new SelectListItem { Value = ((int)d).ToString(), Text = d.ToString(), Selected = MonthlyIntervalEnum.First == d });
        //        if (String.IsNullOrEmpty(cal.MonthlyIntervalId))
        //            cal.MonthlyInterval = new SelectList(montlhyInterval, "Value", "Text", ((object)1));
        //        else
        //            cal.MonthlyInterval = new SelectList(montlhyInterval, "Value", "Text", ((object)cal.MonthlyIntervalId));

        //        cal.EndsOccurences = "0";
        //        List<string> repeatCount = new List<string>();
        //        for (int i = 1; i < 50; i++)
        //        {
        //            repeatCount.Add(i.ToString());
        //        }

        //    }
        //    catch (Exception exception)
        //    {
        //        ErrorDatabaseManager.AddException(exception, exception.GetType());
        //    }
        //    return View(cal);
        //}

        public ActionResult CalendarEditEventType()
        {
            try
            {
                if (HttpContext.Request.InputStream != null)
                {
                    Stream stream = HttpContext.Request.InputStream;
                    var ob = Network.LoadObject<CalendarEventType>(ref stream);
                    var mem = MemberCache.GetMemberDisplay(ob.CurrentMemberId);
                    if (ob.UserId == mem.UserId)
                    {
                        long eventTypeid = CalendarFactory.UpdateCalendarEventType(ob);
                        ob.IsSuccessful = true;
                        return Json(ob, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception exception)
            {
                ErrorDatabaseManager.AddException(exception, exception.GetType());
            }
            return Json(new CalendarEventType(), JsonRequestBehavior.AllowGet);
        }
        ///// <summary>
        ///// gets the event type values so that users know what points they are using when creating an event.
        ///// </summary>
        ///// <param name="eventTypeId"></param>
        ///// <returns></returns>
        public ActionResult GetEventTypeValues()
        {
            try
            {
                if (HttpContext.Request.InputStream != null)
                {
                    Stream stream = HttpContext.Request.InputStream;
                    var ob = Network.LoadObject<CalendarSendParams>(ref stream);
                    var mem = MemberCache.GetMemberDisplay(ob.CurrentMemberId);
                    if (ob.UserId == mem.UserId)
                    {
                        var type = CalendarFactory.GetEventType(ob.EventTypeId);
                        type.IsSuccessful = true;
                        return Json(type, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception exception)
            {
                ErrorDatabaseManager.AddException(exception, exception.GetType());
            }
            return Json(new CalendarEventType(), JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetEventTypes()
        {
            try
            {
                if (HttpContext.Request.InputStream != null)
                {
                    Stream stream = HttpContext.Request.InputStream;
                    var ob = Network.LoadObject<CalendarSendParams>(ref stream);
                    var mem = MemberCache.GetMemberDisplay(ob.CurrentMemberId);
                    if (ob.UserId == mem.UserId)
                    {
                        var type = CalendarFactory.GetEventTypesOfCalendar(ob.CalendarId);

                        return Json(type, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception exception)
            {
                ErrorDatabaseManager.AddException(exception, exception.GetType());
            }
            return Json(new List<CalendarEventType>(), JsonRequestBehavior.AllowGet);
        }
        public ActionResult LeagueColors()
        {
            try
            {
                if (HttpContext.Request.InputStream != null)
                {
                    Stream stream = HttpContext.Request.InputStream;
                    var ob = Network.LoadObject<MemberPassParams>(ref stream);
                    var mem = MemberCache.GetMemberDisplay(ob.Mid);
                    if (ob.Uid == mem.UserId)
                    {
                        var colors = ColorDisplayFactory.GetLeagueColors(mem.CurrentLeagueId);
                        return Json(colors, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception exception)
            {
                ErrorDatabaseManager.AddException(exception, exception.GetType());
            }
            return Json(new List<ColorDisplay>(), JsonRequestBehavior.AllowGet);
        }
        public ActionResult CalendarNewEventType()
        {
            try
            {
                if (HttpContext.Request.InputStream != null)
                {
                    Stream stream = HttpContext.Request.InputStream;
                    var ob = Network.LoadObject<CalendarEventType>(ref stream);
                    var mem = MemberCache.GetMemberDisplay(ob.CurrentMemberId);
                    if (ob.UserId == mem.UserId)
                    {
                        ob.CalendarEventTypeId = CalendarFactory.AddCalendarEventType(ob);
                        ob.IsSuccessful = true;
                        return Json(ob, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception exception)
            {
                ErrorDatabaseManager.AddException(exception, exception.GetType());
            }
            return Json(new CalendarEventType(), JsonRequestBehavior.AllowGet);
        }

        ///// <summary>
        ///// allows the user to check them self into the event by just clickin on the checkin link on calendar list
        ///// </summary>
        ///// <param name="calendarId"></param>
        ///// <param name="eventId"></param>
        ///// <param name="note"></param>
        ///// <param name="pointType"></param>
        ///// <returns></returns>
        public JsonResult CheckSelfIntoEvent()
        {
            try
            {
                if (HttpContext.Request.InputStream != null)
                {
                    Stream stream = HttpContext.Request.InputStream;
                    var ob = Network.LoadObject<CalendarSendParams>(ref stream);
                    var mem = MemberCache.GetMemberDisplay(ob.CurrentMemberId);
                    if (ob.UserId == mem.UserId)
                    {
                        ob.IsSuccessful = CalendarFactory.CheckSelfIn(ob.CalendarId, ob.EventId, ob.CurrentMemberId, ob.Note, ob.PointType, ob.IsTardy, 0);
                        return Json(ob, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception exception)
            {
                ErrorDatabaseManager.AddException(exception, exception.GetType());
            }
            return Json(new CalendarSendParams(), JsonRequestBehavior.AllowGet);
        }
        public JsonResult SetAvailabilityForEvent()
        {
            try
            {
                if (HttpContext.Request.InputStream != null)
                {
                    Stream stream = HttpContext.Request.InputStream;
                    var ob = Network.LoadObject<CalendarSendParams>(ref stream);
                    var mem = MemberCache.GetMemberDisplay(ob.CurrentMemberId);
                    if (ob.UserId == mem.UserId)
                    {
                        ob.IsSuccessful = CalendarFactory.SetAvailabilityForEvent(ob.CalendarId, ob.EventId, ob.CurrentMemberId, ob.Note, ob.Availability);
                        return Json(ob, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception exception)
            {
                ErrorDatabaseManager.AddException(exception, exception.GetType());
            }
            return Json(new CalendarSendParams(), JsonRequestBehavior.AllowGet);
        }
        public JsonResult CheckMemberIntoEvent()
        {
            try
            {
                if (HttpContext.Request.InputStream != null)
                {
                    Stream stream = HttpContext.Request.InputStream;
                    var ob = Network.LoadObject<CalendarSendParams>(ref stream);
                    var mem = MemberCache.GetMemberDisplay(ob.CurrentMemberId);
                    if (ob.UserId == mem.UserId)
                    {
                        ob.IsSuccessful = CalendarFactory.CheckSelfIn(ob.CalendarId, ob.EventId, ob.MemberId, ob.Note, ob.PointType, ob.IsTardy, ob.AddPoints);
                        return Json(ob, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception exception)
            {
                ErrorDatabaseManager.AddException(exception, exception.GetType());
            }
            return Json(new CalendarSendParams(), JsonRequestBehavior.AllowGet);
        }
        public JsonResult CheckInMemberRemove()
        {
            try
            {
                if (HttpContext.Request.InputStream != null)
                {
                    Stream stream = HttpContext.Request.InputStream;
                    var ob = Network.LoadObject<CalendarSendParams>(ref stream);
                    var mem = MemberCache.GetMemberDisplay(ob.CurrentMemberId);
                    if (ob.UserId == mem.UserId)
                    {
                        ob.IsSuccessful = CalendarFactory.CheckInRemove(ob.CalendarId, ob.EventId, ob.MemberId);
                        return Json(ob, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception exception)
            {
                ErrorDatabaseManager.AddException(exception, exception.GetType());
            }
            return Json(new CalendarSendParams(), JsonRequestBehavior.AllowGet);
        }
        public ActionResult ViewEvent()
        {
            try
            {
                if (HttpContext.Request.InputStream != null)
                {
                    Stream stream = HttpContext.Request.InputStream;
                    var ob = Network.LoadObject<CalendarSendParams>(ref stream);
                    var mem = MemberCache.GetMemberDisplay(ob.CurrentMemberId);
                    if (ob.UserId == mem.UserId)
                    {

                        var calEvent = CalendarEventFactory.GetEvent(ob.EventId, ob.CurrentMemberId, ob.CalendarId);
                        calEvent.IsSuccessful = true;
                        CalendarEventPortable ev = (CalendarEventPortable)calEvent;
                        return Json(ev, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception exception)
            {
                ErrorDatabaseManager.AddException(exception, exception.GetType());
            }
            return Json(new CalendarEventPortable(), JsonRequestBehavior.AllowGet);
        }


        public ActionResult DeleteEvent()
        {
            try
            {
                if (HttpContext.Request.InputStream != null)
                {
                    Stream stream = HttpContext.Request.InputStream;
                    var ob = Network.LoadObject<CalendarSendParams>(ref stream);
                    var mem = MemberCache.GetMemberDisplay(ob.CurrentMemberId);
                    if (ob.UserId == mem.UserId)
                    {
                        var calEvent = CalendarEventFactory.DeleteEvent(ob.CalendarId, ob.EventId);
                        ob.IsSuccessful = true;
                        return Json(ob, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception exception)
            {
                ErrorDatabaseManager.AddException(exception, exception.GetType());
            }
            return Json(new CalendarSendParams(), JsonRequestBehavior.AllowGet);
        }
        public ActionResult DeleteEventReoccurring()
        {
            try
            {
                if (HttpContext.Request.InputStream != null)
                {
                    Stream stream = HttpContext.Request.InputStream;
                    var ob = Network.LoadObject<CalendarSendParams>(ref stream);
                    var mem = MemberCache.GetMemberDisplay(ob.CurrentMemberId);
                    if (ob.UserId == mem.UserId)
                    {
                        var calEvent = CalendarEventFactory.DeleteEventReccurring(ob.CalendarId, ob.EventId);
                        ob.IsSuccessful = true;
                        return Json(ob, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception exception)
            {
                ErrorDatabaseManager.AddException(exception, exception.GetType());
            }
            return Json(new CalendarSendParams(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult NewEvent()
        {
            try
            {
                if (HttpContext.Request.InputStream != null)
                {
                    Stream stream = HttpContext.Request.InputStream;
                    var ob = Network.LoadObject<CalendarEventPortable>(ref stream);
                    var mem = MemberCache.GetMemberDisplay(ob.MemberId);
                    if (ob.UserId == mem.UserId)
                    {
                        FrequencyTypeEnum frequency = FrequencyTypeEnum.None;

                        switch (ob.Frequency)
                        {
                            case FrequencyTypeCalendarEnum.Daily:
                                frequency = FrequencyTypeEnum.Daily;
                                break;
                            case FrequencyTypeCalendarEnum.Monthly:
                                frequency = FrequencyTypeEnum.Daily;
                                break;
                            case FrequencyTypeCalendarEnum.Weekly:
                                frequency = FrequencyTypeEnum.Weekly;
                                break;
                        }

                        if (!ob.IsReoccurring)
                            ob.CalendarItemId = CalendarEventFactory.CreateNewEvent(ob.CalendarId, ob.StartDate, ob.EndDate, ob.Location.LocationId, ob.Name, ob.Link, ob.Notes, ob.AllowSelfCheckIn, ob.IsPublicEvent, false, new Guid(), ob.EventType.CalendarEventTypeId, ob.BroadcastEvent, ob.TicketUrl, ob.ColorTempSelected, ob.GroupsForEvent.Select(x => x.Id).ToList(), ob.MemberId);
                        else
                            ob.CalendarItemId = CalendarEventFactory.CreateNewEventReOcurring(ob.CalendarId, ob.StartDate, ob.EndDate, ob.Location.LocationId, ob.Name, ob.Link, ob.Notes, ob.AllowSelfCheckIn, frequency, ob.IsSunday, ob.IsMonday, ob.IsTuesday, ob.IsWednesday, ob.IsThursday, ob.IsFriday, ob.IsSaturday, ob.EndsWhenReoccuringEnum, ob.OccurrencesTillEnd, ob.EndsWhenReoccuringEnum == EndsWhenReoccuringEnum.On ? Convert.ToDateTime(ob.EndDateReoccurring) : new DateTime(), ob.EventType.CalendarEventTypeId, ob.BroadcastEvent, ob.IsPublicEvent, 0, ob.TicketUrl, ob.ColorTempSelected, ob.GroupsForEvent.Select(x => x.Id).ToList(), ob.MemberId);
                        ob.IsSuccessful = true;
                        return Json(ob, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception exception)
            {
                ErrorDatabaseManager.AddException(exception, exception.GetType());
            }
            return Json(new CalendarEventPortable(), JsonRequestBehavior.AllowGet);
        }

        //#endregion

        #region Calendar

        public ActionResult CalendarSettings()
        {
            try
            {
                if (HttpContext.Request.InputStream != null)
                {
                    Stream stream = HttpContext.Request.InputStream;
                    var ob = Network.LoadObject<Calendar>(ref stream);
                    var mem = MemberCache.GetMemberDisplay(ob.MemberId);
                    if (ob.UserId == mem.UserId)
                    {
                        CalendarFactory.UpdateCalendarSettings(ob);
                        var calen = CalendarFactory.GetCalendar(ob.CalendarId, ob.OwnerEntity);
                        ViewBag.IsSuccessful = true;
                        return Json(ob, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception exception)
            {
                ErrorDatabaseManager.AddException(exception, exception.GetType());
            }
            return Json(new Calendar(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult CreateCalendar()
        {
            try
            {
                if (HttpContext.Request.InputStream != null)
                {
                    Stream stream = HttpContext.Request.InputStream;
                    var ob = Network.LoadObject<CalendarSendParams>(ref stream);
                    var mem = MemberCache.GetMemberDisplay(ob.CurrentMemberId);
                    if (ob.UserId == mem.UserId)
                    {
                        if (ob.CalendarType == CalendarOwnerEntityEnum.league)
                        {
                            var cal = CalendarFactory.CreateCalendar(mem.CurrentLeagueId, CalendarOwnerEntityEnum.league);
                            ob.CalendarId = cal.CalendarId;
                        }
                        MemberCache.Clear(ob.CurrentMemberId);
                        MemberCache.ClearApiCache(ob.UserId);
                        ob.IsSuccessful = true;
                        return Json(ob, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception exception)
            {
                ErrorDatabaseManager.AddException(exception, exception.GetType());
            }
            return Json(new CalendarSendParams() { IsSuccessful = false }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult CalendarList()
        {

            Calendar cal = new Calendar();
            try
            {
                if (HttpContext.Request.InputStream != null)
                {
                    Stream stream = HttpContext.Request.InputStream;
                    var ob = Network.LoadObject<CalendarSendParams>(ref stream);
                    var mem = MemberCache.GetMemberDisplay(ob.CurrentMemberId);
                    if (ob.UserId == mem.UserId)
                    {

                        var league = MemberCache.GetLeagueOfMember(ob.CurrentMemberId);
                        var isAttendanceManagerOrBetter = MemberCache.IsAttendanceManagerOrBetterOfLeague(ob.CurrentMemberId);
                        cal.CurrentDateSelected = new DateTime(ob.Year, ob.Month, 15);

                        cal = CalendarFactory.GetCalendarSchedule(ob.CalendarId, ob.CalendarType, DateTimeExt.FirstDayOfMonthFromDateTime(cal.CurrentDateSelected).AddDays(-1), DateTimeExt.LastDayOfMonthFromDateTime(cal.CurrentDateSelected).AddDays(1), ob.CurrentMemberId, isAttendanceManagerOrBetter);

                        cal.CurrentDateSelected = new DateTime(ob.Year, ob.Month, 15);
                        if (!cal.DisableBirthdays)
                            cal.Events.AddRange(MemberCache.GetMemberBirthdays(ob.CurrentMemberId, DateTimeExt.FirstDayOfMonthFromDateTime(cal.CurrentDateSelected).AddDays(-1), DateTimeExt.LastDayOfMonthFromDateTime(cal.CurrentDateSelected).AddDays(1)));
                        if (!cal.DisableSkatingStartDates)
                            cal.Events.AddRange(MemberCache.GetMemberStartDates(ob.CurrentMemberId, DateTimeExt.FirstDayOfMonthFromDateTime(cal.CurrentDateSelected).AddDays(-1), DateTimeExt.LastDayOfMonthFromDateTime(cal.CurrentDateSelected).AddDays(1)));
                        cal.Events = cal.Events.OrderBy(x => x.StartDate).ToList();
                        cal.IsSuccessful = true;
                        return Json(cal, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception exception)
            {
                ErrorDatabaseManager.AddException(exception, exception.GetType());
            }
            return Json(cal, JsonRequestBehavior.AllowGet);
        }
        #endregion
        ///// <summary>
        ///// opends a new event window
        ///// </summary>
        ///// <param name="type"></param>
        ///// <param name="id"></param>
        ///// <param name="check"></param>
        ///// <returns></returns>
        //public ActionResult NewEvent(string type, string id, string check)
        //{
        //    Models.Calendar.NewCalendarEvent post = new Models.Calendar.NewCalendarEvent();
        //    try
        //    {
        //        bool didLocationGetAdded = false;
        //        post.CalendarId = new Guid(id);
        //        post.CalendarType = type;
        //        var locs = RDN.Library.Classes.Calendar.Calendar.GetLocationsOfCalendar(new Guid(id));
        //        var eventTypes = RDN.Library.Classes.Calendar.Calendar.GetEventTypesOfCalendar(new Guid(id));
        //        var AllowSelfCheckin = Calendar.GetCalendar(new Guid(id), (CalendarOwnerEntityEnum)Enum.Parse(typeof(CalendarOwnerEntityEnum), type));

        //        if (!String.IsNullOrEmpty(check))
        //        {
        //            //if the check is true, it means this page came from "Create Event and Add Another"
        //            if (check == "true")
        //            {
        //                SiteMessage message = new SiteMessage();
        //                message.MessageType = SiteMessageType.Success;
        //                message.Message = "Event Was Created.  You can now add another.";
        //                this.AddMessage(message);
        //            }
        //            else // check is actually a location id.
        //                didLocationGetAdded = true;
        //        }

        //        post.EventTypes = new SelectList(eventTypes, "CalendarEventTypeId", "EventTypeName");

        //        if (didLocationGetAdded)
        //        {
        //            //an id will exist if they just created a new location.
        //            var location = Location.GetLocation(new Guid(check));
        //            locs.Add(location);
        //            post.Locations = new SelectList(locs, "LocationId", "LocationName", (object)location.LocationId);
        //        }
        //        else
        //        {
        //            post.Locations = new SelectList(locs, "LocationId", "LocationName");
        //        }
        //        post.AllowSelfCheckIn = AllowSelfCheckin.AllowSelfCheckIn;
        //        post.StartDate = DateTime.Now;
        //        post.EndDate = DateTime.Now;
        //        var repeatsFrequency = (from ScheduleWidget.Enums.FrequencyTypeEnum d in Enum.GetValues(typeof(ScheduleWidget.Enums.FrequencyTypeEnum))
        //                                where d.ToString() != "None"
        //                                select new SelectListItem { Value = ((int)d).ToString(), Text = d.ToString(), Selected = FrequencyTypeEnum.Weekly == d });
        //        post.RepeatsFrequencyDropDown = new SelectList(repeatsFrequency, "Value", "Text", ((object)2));
        //        var montlhyInterval = (from ScheduleWidget.Enums.MonthlyIntervalEnum d in Enum.GetValues(typeof(ScheduleWidget.Enums.MonthlyIntervalEnum))
        //                               where d.ToString() != "None"
        //                               select new SelectListItem { Value = ((int)d).ToString(), Text = d.ToString(), Selected = MonthlyIntervalEnum.First == d });
        //        post.MonthlyInterval = new SelectList(montlhyInterval, "Value", "Text", ((object)1));

        //        post.EndsOccurences = "0";
        //        List<string> repeatCount = new List<string>();
        //        for (int i = 1; i < 50; i++)
        //        {
        //            repeatCount.Add(i.ToString());
        //        }

        //        post.LeagueId = MemberCache.GetLeagueIdOfMember(RDN.Library.Classes.Account.User.GetMemberId());
        //        var colors = ColorDisplay.GetLeagueColors(post.LeagueId);
        //        post.ColorList = new SelectList(colors, "HexColor", "NameOfColor");
        //    }
        //    catch (Exception exception)
        //    {
        //        ErrorDatabaseManager.AddException(exception, exception.GetType());
        //    }
        //    return View(post);
        //}



    }
}
