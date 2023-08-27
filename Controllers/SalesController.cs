using Labiba.Actions.Logger.Core.Filters;
using Labiba.Actions.Logger.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using static Labiba.Sales.WebAPI.Models.Req_Models;
using static Labiba.Sales.WebAPI.Models.LabibaResponses;
using static Labiba.Sales.WebAPI.Models.LabibaResponses.HeroCardsModel;
using Labiba.Actions.Logger.Core.Repositories.Interfaces;
using System.Net;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Globalization;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using System.IO;
using System.Threading;

namespace Labiba.Sales.WebAPI.Controllers
{

    [ApiController]
    public class SalesController : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly IActionLogger _actionLogger;
        public SalesController(
           IActionLogger ActionLogger,
           IHttpContextAccessor httpContextAccessor,
           IConfiguration configuration)
        {
            _actionLogger = ActionLogger;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        public class calendarService
        {
            private readonly string _keyFilePath = "Key.json";
            private readonly CalendarService _service;

            public calendarService()
            {
                GoogleCredential credential;
                using (var stream = new FileStream(_keyFilePath, FileMode.Open, FileAccess.Read))
                { credential = GoogleCredential.FromStream(stream).CreateScoped(CalendarService.Scope.Calendar); }

                _service = new CalendarService (new BaseClientService.Initializer() { HttpClientInitializer = credential, ApplicationName = "GoogleCalendarLabiba", });

            }

            public CalendarService GetService()
            {
                return _service;
            }
        }

        [HttpPost]
        [Route("api/SalesController/GetWeeksAvailableDates")]
        [LogAction(ActionId = 10623454, ClientId = 12105)]
        [Obsolete]
        public async Task<IActionResult> GetWeeksAvailableDates (WeeksAvailableDates parametersModel)
        {
            StateModel stateModel = new StateModel();
            LogDetails logDetails = new LogDetails();
            RootObject cardsRootObject = new RootObject();
            List<hero_cards> cards = new List<hero_cards>();
            logDetails.Method = "Post";

            try
            {
                DateTime currentDateTime = DateTime.Now;
                var TodayDate = currentDateTime.ToString("yyyy-MM-dd");
                var NowTime = currentDateTime.ToString("HH:mm");

                calendarService calendarServiceInstance = new calendarService();
                CalendarService service = calendarServiceInstance.GetService();


                if (parametersModel.DatesorTimes.Trim().ToLower() == "dates")
                {
                  
                    List<string> nextFiveDays = new List<string>();
                    int daysAdded;

                    if (currentDateTime.TimeOfDay < new TimeSpan(15, 30, 0))
                    {
                        daysAdded = 0;
                    }
                    else
                    {
                        daysAdded = 1;
                    }

                    while (nextFiveDays.Count < 5)
                    {
                        DateTime nextDate = currentDateTime.AddDays(daysAdded);
                        if (nextDate.DayOfWeek != DayOfWeek.Friday && nextDate.DayOfWeek != DayOfWeek.Saturday)
                        {
                            if (parametersModel.Language.ToLower().Trim() == "ar")
                            {
                                string dayNameArabic = nextDate.ToString("dddd", new CultureInfo("ar-SA"));
                                string dateEnglish = nextDate.ToString("yyyy-MM-dd");
                                string formattedDate = $"{dayNameArabic}, {dateEnglish}";
                                nextFiveDays.Add(formattedDate);
                            }
                            else
                            {
                                nextFiveDays.Add(nextDate.ToString("dddd, yyyy-MM-dd"));
                            }

                        }
                        daysAdded++;
                    }

                    if (parametersModel.Language.ToLower().Trim() == "ar")
                    {
                        foreach (string date in nextFiveDays)
                        {
                            cards.Add(new hero_cards()
                            {
                                Title = $"<list>{date}",
                                Buttons = new List<Button>() { new Button() { Title = "اختار", Type = "postback", Value = date } }
                            });
                        }
                    }
                    if (parametersModel.Language.ToLower().Trim() == "en")
                    {
                        foreach (string date in nextFiveDays)
                        {
                            cards.Add(new hero_cards()
                            {
                                Title = $"<list>{date}",
                                Buttons = new List<Button>() { new Button() { Title = "Select", Type = "postback", Value = date } }
                            });
                        }
                    }
                }


                if (parametersModel.DatesorTimes.Trim().ToLower() == "times")
                {
                    string[] parts = parametersModel.Date_ForTheTimes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    string day = parts[0].Trim();
                    string date = parts[1].Trim();
                    DateTime dateToCheck = DateTime.Parse(date);

                    EventsResource.ListRequest request = service.Events.List(parametersModel.Calendar_ID);
                    request.TimeMin = dateToCheck;
                    request.TimeMax = dateToCheck.AddDays(1);
                    Events events = request.Execute();
                    var sortedEvents = events.Items.OrderBy(e => e.Start.DateTime);

                    List<string> timeSlots = GenerateTimeSlots("9:00 am", "5:00 pm", TimeSpan.FromHours(1));
                    List<string> timeSlotsNew = new List<string>();


                    foreach (var eventItem in sortedEvents)
                    {
                        int i = 0;
                        foreach (var time in timeSlots)
                        {
                            string[] TimeParts = time.Split('-');
                            string StartDateString = TimeParts[0].Trim();
                            string EndDateString = TimeParts[1].Trim();
                            DateTime StartDate = DateTime.Parse(StartDateString);
                            DateTime EndDate = DateTime.Parse(EndDateString);
                            DateTime dateTime1 = eventItem.Start.DateTime ?? StartDate;
                            DateTime dateTime2 = eventItem.End.DateTime ?? EndDate;

                            if (date == TodayDate)
                            {
                                if (currentDateTime.TimeOfDay > StartDate.TimeOfDay)
                                {
                                    if (!timeSlotsNew.Contains(time))
                                    {
                                        timeSlotsNew.Add(time);
                                    }
                                    i++;
                                    continue;
                                }
                            }
                            if (dateTime1.TimeOfDay >= StartDate.TimeOfDay && dateTime1.TimeOfDay < EndDate.TimeOfDay)
                            {
                                if (!timeSlotsNew.Contains(time))
                                {
                                    timeSlotsNew.Add(time);
                                }
                                i++;
                            }
                            if (dateTime2.TimeOfDay > StartDate.TimeOfDay && dateTime2.TimeOfDay < EndDate.TimeOfDay)
                            {
                                if (!timeSlotsNew.Contains(time))
                                {
                                    timeSlotsNew.Add(time);
                                }
                                i++;
                            }

                        }
                    }

                    timeSlots.RemoveAll(timeslot => timeSlotsNew.Contains(timeslot));

                    if (timeSlots.Count == 0)
                    {
                        stateModel.state = "Not_Found";
                        logDetails.ResponseToLabiba = JsonConvert.SerializeObject(stateModel);
                        await _actionLogger.LogDetails(logDetails);
                        return Ok(stateModel);
                    }

                    if (parametersModel.Language.ToLower().Trim() == "ar")
                    {
                        foreach (var freeHour in timeSlots)
                        {
                            cards.Add(new hero_cards()
                            {
                                Title = $"<list>{freeHour}",
                                Buttons = new List<Button>() { new Button() { Title = "اختار", Type = "postback", Value = freeHour } }
                            });
                        }
                    }
                    if (parametersModel.Language.ToLower().Trim() == "en")
                    {
                        foreach (var freeHour in timeSlots)
                        {
                            cards.Add(new hero_cards()
                            {
                                Title = $"<list>{freeHour}",
                                Buttons = new List<Button>() { new Button() { Title = "Select", Type = "postback", Value = freeHour } }
                            });
                        }
                    }

                }

                cardsRootObject.response = "Success";
                cardsRootObject.success_message = "Here are some results that I found";
                cardsRootObject.failure_message = "Oops. I couldn't find anything";
                cardsRootObject.hero_cards = cards;

                logDetails.ResponseToLabiba = JsonConvert.SerializeObject(cardsRootObject);
                await _actionLogger.LogDetails(logDetails);
                return Ok(cardsRootObject);
            }
            catch (Exception ex)
            {
                stateModel.state = "Failure";
                await _actionLogger.LogExecption(logDetails, ex);
                logDetails.ResponseToLabiba = JsonConvert.SerializeObject(stateModel);
                await _actionLogger.LogDetails(logDetails);
                return Ok(stateModel);
            }
        }

        static List<string> GenerateTimeSlots(string startTime, string endTime, TimeSpan interval)
        {
            DateTime start = DateTime.Parse(startTime);
            DateTime end = DateTime.Parse(endTime);

            List<string> timeSlots = new List<string>();

            while (start < end)
            {
                DateTime slotStart = start;
                DateTime slotEnd = start.Add(interval);

                if (slotEnd > end)
                {
                    slotEnd = end;
                }

                timeSlots.Add($"{slotStart:h:mm tt}-{slotEnd:h:mm tt}");

                start = slotEnd;
            }

            return timeSlots;
        }

    }
}

