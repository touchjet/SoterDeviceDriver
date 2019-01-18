
using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace SoterDeviceBleTest
{
    public partial class PinMatrixPage : ContentPage
    {
        public static string pin;

        public PinMatrixPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            pin = "";
        }

        async void ButtonConfirm_Clicked(object sender, System.EventArgs e)
        {
            await Navigation.PopModalAsync();
        }

        void ButtonClear_Clicked(object sender, System.EventArgs e)
        {
            pin = "";
            LabelPin.Text = pin;
        }

        void ButtonPIN_Clicked(object sender, System.EventArgs e)
        {
            pin = pin + ((Button)sender).ClassId;
            LabelPin.Text = pin;
        }
    }
}
