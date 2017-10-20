using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NexTech;
using System.Data.SqlClient;
using System.Timers;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace NotificationServer
{
    public class NotificationSender
    {
        private static long Counter = 0;
        private static System.Timers.Timer InitializationTimer;
        private static readonly string FCMAddress = "https://fcm.googleapis.com/fcm/send";
        private static readonly string ServerKey = "AAAADtfyRQU:APA91bHoBH3AL0t4zc0MiJ5FJcZR_8ZctqzrLXyIQP8kGfbKguyi7QseeAg7YSVCFtd2cCaot_Hu4tLmKKQhFoMxYKDT3gMftgR2kfvzLIylP-SC3u53lD5otbyT1Cy5uOy6ZIUegvf_";
        private static readonly string TestToken = "eebgf0vNCjA:APA91bHZf8fIpY3eT8Y_TNuZDV_MM4qeLllFyhczdvPm0IiLjT3KxhCmDl7b5cZXptRH8vDN7p03YryBvDFWdV_Jm_jKDDQPyCki_hB6YqJWixOm8vG0fJij3OMWWt4xN6mYV6JxCyXg";
        public NotificationSender()
        {
            InitializationTimer = new System.Timers.Timer();
            InitializationTimer.Elapsed += new ElapsedEventHandler(CheckNotifications);
            InitializationTimer.Interval = 30000; // check every 30 seconds
            InitializationTimer.AutoReset = true;
            InitializationTimer.Start();
        }

        private async void CheckNotifications(object source, ElapsedEventArgs e)
        {
            Counter++;
            /* using (SqlConnection con = new SqlConnection($@"Server=GEONOSIS\SQL2014EXPRESS;Database=PracData;User ID=sa;Password=nextechsql187;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;"))
             {
                 con.Open();
                 using (SqlCommand cmd = new SqlCommand(@"
 SELECT Token, Event FROM MpvNotificationT", con))
                 {
                     using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                     {
                         while (dr.Read())
                         {
                             string token = dr["Token"].ToString();
                             NotificationType eventType = (NotificationType) dr["Event"];
                             NotificationMessage message = new NotificationMessage()
                             {
                                 Recipient = new NotificationRecipient() { Type = eventType, To = token }
                             };
                             {
                                 await Send(message);
                             }
                         }
                     }
                 }
             }*/
            NotificationMessage message = new NotificationMessage()
            {
                notification = new NotificationContent() { title = $"Test Title {Counter}", body = $"Test Body {Counter}"},
                to = TestToken
            };
            await Send(message);
        }

        private async Task Send(NotificationMessage message)
        {
            try
            {
                var values = new Dictionary<string, string>
                {
                    {"to", message.to}
                };
                string messageJson = Newtonsoft.Json.JsonConvert.SerializeObject(message);
                using (var request = new HttpClient())
                {
                    //Setup Headers
                    request.DefaultRequestHeaders.Accept.Clear();
                    request.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    //request.DefaultRequestHeaders.Add("Content-Type", "application/json");
                    request.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"key={ServerKey}");
                    //request.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("key", )

                    var response = await request.PostAsJsonAsync(
                        FCMAddress,
                        messageJson
                    );
                    response.EnsureSuccessStatusCode();

                    string result = await response.Content.ReadAsStringAsync();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }


    }
}
