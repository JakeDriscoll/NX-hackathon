using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MPV_Mobile
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddPhotoPage : ContentPage
    {
        public AddPhotoPage()
        {
            InitializeComponent();
            var TakePhoto = this.FindByName<Button>("TakePhoto");

            TakePhoto.Clicked += TakePhotoClicked;
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
            else if(action.Equals("Take a Photo",StringComparison.OrdinalIgnoreCase))
            {
                photo = await Plugin.Media.CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions() { });
            }

            TakePhoto.BackgroundColor = Color.DarkOrange;
            if (photo != null)
            {
                PhotoImage.Source = ImageSource.FromStream(() => { return photo.GetStream(); });
                Header.Text = photo.Path;
            }
            //}
        }
    }
}