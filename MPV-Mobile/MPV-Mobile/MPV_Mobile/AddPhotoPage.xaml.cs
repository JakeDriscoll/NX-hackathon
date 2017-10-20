using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Blob; // Namespace for Blob storage types

namespace MPV_Mobile
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddPhotoPage : ContentPage
    {
        public AddPhotoPage()
        {
            InitializeComponent();
            var TakePhoto = this.FindByName<Button>("TakePhoto");
            var togglebutton = this.FindByName<Xamarin.Forms.Switch>("doUpload");
            togglebutton.Toggled += Togglebutton_Toggled;
            TakePhoto.Clicked += TakePhotoClicked;
        }

        private void Togglebutton_Toggled(object sender, ToggledEventArgs e)
        {
            if (this.FindByName<Xamarin.Forms.Switch>("doUpload").IsToggled)
                Console.WriteLine("Toggled");
            else
                Console.WriteLine("Not Toggled");
            Console.WriteLine("We got an event!!");
        }

        //static CloudBlobContainer GetContainer(ContainerType containerType)
        //{
        //    var account = CloudStorageAccount.Parse(Constants.StorageConnection);
        //    var client = account.CreateCloudBlobClient();
        //    return client.GetContainerReference(containerType.ToString().ToLower());
        //}

        public static async Task<string> UploadFileAsync(Plugin.Media.Abstractions.MediaFile photo)
        {
            //if (this.FindByName<Xamarin.Forms.Switch>("doUpload").IsToggled)
            //    return String.Empty;

            try
            {
                Uri SasURL = new Uri("https://functionc9fcb2bfb1f1.blob.core.windows.net/team3-demo?st=2017-10-20T11%3A01%3A00Z&se=2018-10-21T11%3A01%3A00Z&sp=rwl&sv=2017-04-17&sr=c&sig=9zm%2FqQJ8j6fVVINopxfr5PPFZ%2F1NE7j8xVau%2FXVFSl0%3D");
                CloudBlobContainer container = new CloudBlobContainer(SasURL);
                var name = System.IO.Path.GetFileName(photo.Path);
                var fileBlob = container.GetBlockBlobReference(name);
                await fileBlob.UploadFromStreamAsync(photo.GetStream());

                return name;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return String.Empty;
            }
            //return name;
        }
        async void TakePhotoClicked(object sender, EventArgs e)
        {
            //if (Plugin.Media.CrossMedia.Current.IsCameraAvailable)
            Plugin.Media.Abstractions.MediaFile photo = null;
            //{
            var action = await DisplayActionSheet("", "Cancel", null, "Take a Photo", "Choose a Photo");
            Debug.WriteLine("Action: " + action);
            if (action.Equals("Choose a Photo", StringComparison.OrdinalIgnoreCase))
            {
                photo = await Plugin.Media.CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions() { });
            }
            else if (action.Equals("Take a Photo", StringComparison.OrdinalIgnoreCase))
            {
                photo = await Plugin.Media.CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions() { });
            }

            TakePhoto.BackgroundColor = Color.DarkOrange;
            if (photo != null)
            {
                PhotoImage.Source = ImageSource.FromStream(() => { return photo.GetStream(); });
                Header.Text = photo.Path;
                String upoadedFileName = String.Empty;
                if (this.FindByName<Xamarin.Forms.Switch>("doUpload").IsToggled)
                    upoadedFileName = await UploadFileAsync(photo);

                if (upoadedFileName != String.Empty)
                {

                    // The amount of Dev work to update MPV to have a new endpoint and then all the backend work to create the new
                    // methods is not difficult, just very time consuming. To save time we're just going to make a direct connection 
                    // to Practice to create the appointment
                    SqlConnectionStringBuilder connection = new SqlConnectionStringBuilder();
                    connection["Data Source"] = "10.0.2.2\\NEXTECH";
                    connection["Database"] = "PracData";
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
INSERT INTO MailSent (PersonID, Selection, [PathName],[Subject],Sender,[Date],[Location],IsPhoto,ServiceDate,MailBatchID) 
    OUTPUT inserted.MailID INTO @tNewApiKeyID (NewID)
VALUES (73, 'BITMAP:FILE', @FileName, '', 'MPV APP', GETDATE(), 1, 1, GETDATE(),2)
SELECT NewID FROM @tNewApiKeyID;
COMMIT TRAN
";
                                cmd.Parameters.AddWithValue("@FileName", upoadedFileName);
                                conn.Open();
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
                                conn.Close();
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine("Something went wrong" + exception.ToString());
                    }
                }
                else
                {
                    await DisplayAlert("Upload Error", "There was an issue uploading your photo, please try again later", "OK");
                }
            }
        }

        bool AddPhotoToPractice(Plugin.Media.Abstractions.MediaFile photo)
        {
            bool success = false;



            return success;

        }
    }
}