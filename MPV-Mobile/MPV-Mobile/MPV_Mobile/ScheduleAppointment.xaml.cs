using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Database;
using Android.Provider;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;


namespace MPV_Mobile
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ScheduleAppointment : ContentPage
    {
        public ScheduleAppointment()
        {
            InitializeComponent();

            var date = this.FindByName<DatePicker>("scheduleAppointmentDatePicker");
            date.MinimumDate = DateTime.Today.AddDays(1);

            var time = this.FindByName<TimePicker>("scheduleAppointmentTimePicker");
            time.Time = new TimeSpan(12, 0, 0);

            var providerList = this.FindByName<Picker>("providerList");
            providerList.ItemsSource = new string[]
            {
                "Dr. Smet",
                "Dr. Maida",
                "Dr. Driskol",
                "Dr. Shah"
            };

            var requestAppointment = this.FindByName<Button>("requestAppointment");
            requestAppointment.IsEnabled = false;

            providerList.SelectedIndexChanged += ProviderListChange;
            requestAppointment.Clicked += RequestAppointmentClick;

            void RequestAppointmentClick(object sender, EventArgs e)
            {
                DisplayAlert("Request Submitted", "Your request has been submitted to the office. Please allow up to 1 business days for a response.", "Ok");

                GenerateCalendarAppointment(date, time);

            };

            void ProviderListChange(object sender, EventArgs e)
            {
                if (providerList.SelectedItem != null)
                {
                    requestAppointment.IsEnabled = true;
                }
            }
        }

        private void GenerateCalendarAppointment(DatePicker date, TimePicker time)
        {
            try
            {
                var calendarsUri = CalendarContract.Calendars.ContentUri;

                string[] projections = {
                    CalendarContract.Calendars.InterfaceConsts.Id,
                    CalendarContract.Calendars.InterfaceConsts.CalendarDisplayName,
                    CalendarContract.Calendars.InterfaceConsts.AccountName
                };

                Context context = Android.App.Application.Context;

                var loader = new CursorLoader(context, calendarsUri, projections, null, null, null);
                var cursor = (ICursor)loader.LoadInBackground();

                var fdas = cursor.GetColumnNames();

                if (cursor.MoveToFirst())
                {

                    var calendarID = cursor.GetInt(cursor.GetColumnIndex(projections[0]));

                    ContentValues appt = new ContentValues();
                    {
                        appt.Put(CalendarContract.Events.InterfaceConsts.CalendarId, calendarID);
                        appt.Put(CalendarContract.Events.InterfaceConsts.Title, "Doctor's Appointment");
                        appt.Put(CalendarContract.Events.InterfaceConsts.Description, "Appointment with " + providerList.SelectedItem.ToString());
                        appt.Put(CalendarContract.Events.InterfaceConsts.Dtstart, GetStartDateAndTime(date, time));                        
                        appt.Put(CalendarContract.Events.InterfaceConsts.Dtend, GetEndDateAndTime(date, time));
                        appt.Put(CalendarContract.Events.InterfaceConsts.EventTimezone, "EST");
                        appt.Put(CalendarContract.Events.InterfaceConsts.EventEndTimezone, "EST");
                        appt.Put(CalendarContract.Events.InterfaceConsts.EventLocation, @"5550 West Executive Drive, Suite 350 
                                                                                            Tampa, FL 33609");                        
                    }

                    var uri = Forms.Context.ContentResolver.Insert(CalendarContract.Events.ContentUri, appt);

                    long eventID = long.Parse(uri.LastPathSegment);
                    ContentValues remindervalues = new ContentValues();
                    {
                        remindervalues.Put(CalendarContract.Reminders.InterfaceConsts.Minutes, 1440);
                        remindervalues.Put(CalendarContract.Reminders.InterfaceConsts.EventId, eventID);
                        remindervalues.Put(CalendarContract.Reminders.InterfaceConsts.Method, (int)RemindersMethod.Alert);
                    }
                    var reminderURI = Forms.Context.ContentResolver.Insert(CalendarContract.Reminders.ContentUri, remindervalues);
                }
                else
                {
                    DisplayAlert("Error", "Please set up a default calender on your phone before using this feature", "Ok");
                }

            }
            catch (Exception e)
            {
                string asdf = e.ToString();
            }
        }

        private long GetEndDateAndTime(DatePicker date, TimePicker time)
        {
            Calendar c = Calendar.GetInstance(Java.Util.TimeZone.Default);

            c.Set(CalendarField.DayOfMonth, date.Date.Day);
            c.Set(CalendarField.HourOfDay, time.Time.Hours);            
            c.Set(CalendarField.Minute, time.Time.Minutes + 30);
            // Java utils is stupid and thinks January is the 0th month
            c.Set(CalendarField.Month, date.Date.Month - 1);
            c.Set(CalendarField.Year, date.Date.Year);

            return c.TimeInMillis;
        }

        private long GetStartDateAndTime(DatePicker date, TimePicker time)
        {
            Calendar c = Calendar.GetInstance(Java.Util.TimeZone.Default);

            c.Set(CalendarField.DayOfMonth, date.Date.Day);
            c.Set(CalendarField.HourOfDay, time.Time.Hours);
            c.Set(CalendarField.Minute, time.Time.Minutes);
            // Java utils is stupid and thinks January is the 0th month
            c.Set(CalendarField.Month, date.Date.Month - 1);
            c.Set(CalendarField.Year, date.Date.Year);

            return c.TimeInMillis;
        }
    }
}