using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MPV_Mobile
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
   
   
    public partial class Login : ContentPage
    {
        public ObservableCollection<string> Items { get; set; }

        public Login()
        {
            //  InitializeComponent();

            Items = new ObservableCollection<string>
            {
                "Item 1",
                "Item 2",
                "Item 3",
                "Item 4",
                "Item 5"
            };

            Content = new StackLayout
            {
                Spacing = 20,
                Padding = 50,
                VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    new Entry {Placeholder = "Username"},
                    new Entry {Placeholder = "Password", IsPassword = true},
                    new Button
                    {
                        Text = "Register", TextColor = Color.White, BackgroundColor = Color.FromHex("77D065")
                    }
                }

            };
            
            var settingsPage = new ContentPage
            {
                Title = "Settings",
                Icon = "Setting.png",

            };
            
            var mainPage = new TabbedPage { Children = { settingsPage } };

          async void OnActionSheetSimpleClicked (object sender, EventArgs e)
            {
                var action = await DisplayActionSheet("ActionSheet: Send to?", "Cancel", null, "Email", "Twitter", "Facebook");
               
            }


        }

        async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null)
                return;

            await DisplayAlert("Item Tapped", "An item was tapped.", "OK");

            //Deselect Item
            ((ListView)sender).SelectedItem = null;
        }
    }
}
