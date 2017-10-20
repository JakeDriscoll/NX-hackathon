
using Android.Content;
using Android.Database;
using Android.Provider;
using Java.Util;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Data.SqlClient;

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
            // Normally this would be a data connection call to grab the possible list of providers but since this is a minimal set
            // we only have Dr. Sample so going to save the dev time and cheat here. 
            providerList.ItemsSource = new string[]
            {
                "Dr. Sample"                
            };

            var requestAppointment = this.FindByName<Button>("requestAppointment");
            requestAppointment.IsEnabled = false;

            providerList.SelectedIndexChanged += ProviderListChange;
            requestAppointment.Clicked += RequestAppointmentClick;

            void RequestAppointmentClick(object sender, EventArgs e)
            {
                DisplayAlert("Request Submitted", "Your request has been submitted to the office. Please allow up to 1 business days for a response.", "Ok");

                GenerateCalendarAppointment(date, time);
                GeneratePracticeAppointment(date, time);
            };

            void ProviderListChange(object sender, EventArgs e)
            {
                if (providerList.SelectedItem != null)
                {
                    requestAppointment.IsEnabled = true;
                }
            }
        }

        private void GeneratePracticeAppointment(DatePicker date, TimePicker time)
        {
            // The amount of Dev work to update MPV to have a new endpoint and then all the backend work to create the new
            // methods is not difficult, just very time consuming. To save time we're just going to make a direct connection 
            // to Practice to create the appointment
            SqlConnectionStringBuilder connection = new SqlConnectionStringBuilder();
            connection["Data Source"] = "10.0.2.2\\SQL2016";
            connection["Database"] = "PracData_Garbage";
            connection["User ID"] = "sa";
            connection["Password"] = "nextechsql187";
            connection["Integrated Security"] = false;

            try
            {
                using (SqlConnection conn = new SqlConnection(connection.ConnectionString))
                {
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
SET XACT_ABORT ON;
BEGIN TRAN;
SET NOCOUNT ON;

DECLARE @tNewApiKeyID TABLE (NewID INT);

INSERT INTO AppointmentsT (PatientID, LocationID, CreatedDate, CreatedLogin, [Date], StartTime, EndTime, Status, ArrivalTime) 
    OUTPUT inserted.ID INTO @tNewApiKeyID (NewID)
VALUES (89, 1, GETDATE(), 'Administrator', @apptDate, @startTime, @endTime, 1, @arrivalTime)

SELECT NewID FROM @tNewApiKeyID;

INSERT INTO AppointmentResourceT (AppointmentID, ResourceID)
VALUES ((SELECT NewID FROM @tNewApiKeyID), 13)

COMMIT TRAN

";
                        cmd.Parameters.AddWithValue("@apptDate", date.Date);
                        cmd.Parameters.AddWithValue("@startTime", GetStartTime(date.Date, time.Time));
                        cmd.Parameters.AddWithValue("@endTime", GetEndTime(date.Date, time.Time));
                        cmd.Parameters.AddWithValue("@arrivalTime", GetArrivalTime(date.Date, time.Time));

                        conn.OpenAsync();

                        using (SqlDataReader reader = cmd.ExecuteReader(System.Data.CommandBehavior.SingleResult | System.Data.CommandBehavior.SingleRow))
                        {
                            if (reader.Read())
                            {
                                // Give oneself a high five. Or use the appointment ID if you need it.
                            }
                            else
                            {
                                // What the heck happened?!
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                DisplayAlert("Exception (Why did you break it?)", "Apparently Xamrin doesn't allow string interpolation? Good luck finding the exception", "I agree, I'm a bad programmer");
            }
        }

        /// <summary>
        /// Get the arrival time for an appointment
        /// </summary>
        /// <param name="date">Date of the appointment</param>
        /// <param name="time">Time of the appointment</param>
        /// <param name="minutesBeforeArrival">Minutes to arrive before appointment time</param>
        /// <returns>Arrival time datetime object</returns>
        private DateTime GetArrivalTime(DateTime date, TimeSpan time, int minutesBeforeArrival = 30)
        {
            DateTime result = new DateTime(date.Year, date.Month, date.Day, time.Hours, time.Minutes, 0);
            return result.AddMinutes(-minutesBeforeArrival);
        }

        /// <summary>
        /// Get the end time for an appointment
        /// </summary>
        /// <param name="date">Date of the appointment</param>
        /// <param name="time">Time of the appointment</param>
        /// <param name="duration">Duration of the appointment</param>
        /// <returns>End time datetime object</returns>
        private DateTime GetEndTime(DateTime date, TimeSpan time, int duration = 30)
        {
            DateTime result = new DateTime(date.Year, date.Month, date.Day, time.Hours, time.Minutes, 0);
            return result.AddMinutes(duration);
        }

        /// <summary>
        /// Get the start time for an appointment
        /// </summary>
        /// <param name="date">Date of the appointment</param>        
        /// <returns>Appointment Date datetime object</returns>
        private DateTime GetStartTime(DateTime date, TimeSpan time)
        {
            return new DateTime(date.Year, date.Month, date.Day, time.Hours, time.Minutes, 0);
        }

        /// <summary>
        /// Get the appointment arrive time
        /// </summary>
        /// <param name="time">Start time of the appointment</param>
        /// <param name="timePeriod">Time to arrive before appointment, default 30 minutes</param>
        /// <returns>Arrival time</returns>
        private DateTime GetArrivalTime(TimePicker time, int timePeriod = 30)
        {
            return new DateTime(0, 0, 0, 0, time.Time.Hours, time.Time.Minutes - timePeriod, 0);
        }

        /// <summary>
        /// Get the appointment end time
        /// </summary>
        /// <param name="time">Starting time of the appointment</param>
        /// <param name="duration">Duration of the appointment, default 30 minutes</param>
        /// <returns>TimePicker</returns>
        private DateTime GetEndTime(TimePicker time, int duration = 30)
        {
            return new DateTime(0, 0, 0, 0, time.Time.Hours, time.Time.Minutes + duration, 0);
        }

        /// <summary>
        /// Create an appointment event along with a reminder alert for the event
        /// </summary>
        /// <param name="date">Date of the appointment</param>
        /// <param name="time">Time of the appointment</param>
        private void GenerateCalendarAppointment(DatePicker date, TimePicker time)
        {
            try
            {
                // We need the IDs of the local calendars to reference later
                string[] calendarColumns = {
                    CalendarContract.Calendars.InterfaceConsts.Id
                };

                // Grab the local context
                Context context = Android.App.Application.Context;

                //  Grab the local calender IDs
                var loader = new CursorLoader(context, CalendarContract.Calendars.ContentUri, calendarColumns, null, null, null);
                var cursor = (ICursor)loader.LoadInBackground();

                // If we found a calendar, grab the first record, it's the default
                if (cursor.MoveToFirst())
                {
                    var calendarID = cursor.GetInt(cursor.GetColumnIndex(calendarColumns[0]));

                    // Create the appointment event
                    ContentValues appt = new ContentValues();
                    {
                        appt.Put(CalendarContract.Events.InterfaceConsts.CalendarId, calendarID);
                        appt.Put(CalendarContract.Events.InterfaceConsts.Title, "Doctor's Appointment");
                        appt.Put(CalendarContract.Events.InterfaceConsts.Description, "Appointment with " + providerList.SelectedItem.ToString());
                        appt.Put(CalendarContract.Events.InterfaceConsts.Dtstart, GetStartDateAndTime(date, time));
                        appt.Put(CalendarContract.Events.InterfaceConsts.Dtend, GetEndDateAndTime(date, time));
                        appt.Put(CalendarContract.Events.InterfaceConsts.EventTimezone, "EST");
                        appt.Put(CalendarContract.Events.InterfaceConsts.EventEndTimezone, "EST");
                        appt.Put(CalendarContract.Events.InterfaceConsts.EventLocation, @"5550 West Executive Drive, Suite 350 Tampa, FL 33609");
                    }

                    // Insert the calendar event 
                    var uri = Forms.Context.ContentResolver.Insert(CalendarContract.Events.ContentUri, appt);

                    // Grab the newly created event to create a reminder alert
                    long eventID = long.Parse(uri.LastPathSegment);
                    ContentValues remindervalues = new ContentValues();
                    {
                        remindervalues.Put(CalendarContract.Reminders.InterfaceConsts.Minutes, 1440);
                        remindervalues.Put(CalendarContract.Reminders.InterfaceConsts.EventId, eventID);
                        remindervalues.Put(CalendarContract.Reminders.InterfaceConsts.Method, (int)RemindersMethod.Alert);
                    }

                    // Insert the reminder alert
                    var reminderURI = Forms.Context.ContentResolver.Insert(CalendarContract.Reminders.ContentUri, remindervalues);
                }

                // We found no local calendars
                else
                {
                    DisplayAlert("Error", "Please set up a default calender on your phone before using this feature", "Ok");
                }

            }
            catch (Exception e)
            {
                throw;
            }
        }

        /// <summary>
        /// Generate appointment end time in milliseconds
        /// </summary>
        /// <param name="date">DateTime object</param>
        /// <param name="time">Time obect</param>
        /// <param name="duration">Appointment duration, default is 30 minutes</param>
        /// <returns></returns>
        private long GetEndDateAndTime(DatePicker date, TimePicker time, int duration = 30)
        {
            Calendar cal = Calendar.GetInstance(Java.Util.TimeZone.Default);

            cal.Set(CalendarField.DayOfMonth, date.Date.Day);
            cal.Set(CalendarField.HourOfDay, time.Time.Hours);
            cal.Set(CalendarField.Minute, time.Time.Minutes + duration);
            // Java utils is stupid and thinks January is the 0th month
            cal.Set(CalendarField.Month, date.Date.Month - 1);
            cal.Set(CalendarField.Year, date.Date.Year);

            return cal.TimeInMillis;
        }

        /// <summary>
        /// Generate appointment start time in milliseconds
        /// </summary>
        /// <param name="date">DateTime object</param>
        /// <param name="time">Time object</param>
        /// <returns>Appointment start time in milliseconds</returns>
        private long GetStartDateAndTime(DatePicker date, TimePicker time)
        {
            Calendar cal = Calendar.GetInstance(Java.Util.TimeZone.Default);

            cal.Set(CalendarField.DayOfMonth, date.Date.Day);
            cal.Set(CalendarField.HourOfDay, time.Time.Hours);
            cal.Set(CalendarField.Minute, time.Time.Minutes);
            // Java utils is stupid and thinks January is the 0th month
            cal.Set(CalendarField.Month, date.Date.Month - 1);
            cal.Set(CalendarField.Year, date.Date.Year);

            return cal.TimeInMillis;
        }
    }
}