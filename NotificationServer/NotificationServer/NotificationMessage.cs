using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationServer
{
    public enum NotificationType
    {
        AppointmentRequest = 0,
        ImageUploaded,
    }

    public class NotificationMessage
    {
        public NotificationContent notification { get; set; }
        public string to { get; set; }
        //public NotificationRecipient Recipient { get; set; }
    }

    public class NotificationContent
    {
        public string title { get; set; }
        public string body { get; set; }
    }

    public class NotificationRecipient
    {
        public NotificationType Type { get; set; }
        public string To { get; set; }
    }
}
