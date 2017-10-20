using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace MPV_Mobile.Droid.Models
{
    public enum NotificationType
    {
        AppointmentRequest = 0,
        ImageUploaded,
    }

    public class MpvNotification
    {
        public NotificationType Type { get; set; }
        public string Token { get; set; }
    }
}