using Android.Content;
using Android.OS;
using System;
using System.Collections.Generic;
using System.Text;
using TeaClient.Services.VibrationService;
using Xamarin.Forms;

[assembly: Dependency(typeof(VibrationService))]

namespace TeaClient.Services.VibrationService
{
   public class VibrationService: IVibrationService
    {
        public void Vibrate()
        {
            var vibrator = (Vibrator)Android.App.Application.Context.GetSystemService(Context.VibratorService);
            if (vibrator.HasVibrator)
            {
                vibrator.Vibrate(100); // Vibrate for 100 milliseconds
            }
        }
    }
}
