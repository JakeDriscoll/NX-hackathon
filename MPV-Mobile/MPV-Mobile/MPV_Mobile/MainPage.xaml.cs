using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Windows;

namespace MPV_Mobile
{
	public partial class MainPage : ContentPage
	{       

		public MainPage()
		{
            InitializeComponent();            
            
            // Create local variables of our app elements
            var Schedule = this.FindByName<Button>("Schedule");
            var Import = this.FindByName<Button>("Import");
            var Message = this.FindByName<Button>("Message");
            var Login = this.FindByName<Button>("Login");
            var HeaderLogo = this.FindByName<Image>("HeaderLogo");

            // Data manipulation
            HeaderLogo.Source = ImageSource.FromFile("mpv_logo.png");
            HeaderLogo.Aspect = Aspect.AspectFit;

            /// OnClick event handlers
            #region

            Schedule.Clicked += ScheduleClicked;

            Import.Clicked += ImportClicked;

            Message.Clicked += MessageClicked;

            Login.Clicked += LoginClicked;            

            void LoginClicked(object sender, EventArgs e)
            {
                Navigation.PushModalAsync(new Login());
            }

            void ScheduleClicked(object sender, EventArgs e)
            {
                Navigation.PushModalAsync(new ScheduleAppointment());                
            }

            void ImportClicked(object sender, EventArgs e)
            {
                Import.BackgroundColor = Color.Teal;
                Navigation.PushModalAsync(new AddPhotoPage());
            }

            void MessageClicked(object sender, EventArgs e)
            {
                Message.BackgroundColor = Color.DarkOrange;                
            }

#endregion

        }
	}
}
