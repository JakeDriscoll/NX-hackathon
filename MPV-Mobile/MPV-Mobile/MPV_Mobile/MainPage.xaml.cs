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
                        
            var Schedule = this.FindByName<Button>("Schedule");
            var Import = this.FindByName<Button>("Import");
            var Message = this.FindByName<Button>("Message");

            Schedule.Clicked += ScheduleClicked;

            Import.Clicked += ImportClicked;

            Message.Clicked += MessageClicked;

            void ScheduleClicked(object sender, EventArgs e)
            {
                Schedule.BackgroundColor = Color.Aqua;
            }

            void ImportClicked(object sender, EventArgs e)
            {
                Import.BackgroundColor = Color.Teal;
            }

            void MessageClicked(object sender, EventArgs e)
            {
                Message.BackgroundColor = Color.DarkOrange;
            }
        }
	}
}
