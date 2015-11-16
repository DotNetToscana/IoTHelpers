using LedManager.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace LedManager
{
    public sealed partial class MainPage : Page
    {
        //public const string ServiceUrl = "http://localhost:37309/api/";
        public const string SERVICE_URL = "http://iotserviceweb.azurewebsites.net/api/";

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is VoiceCommandActivatedEventArgs)
            {
                var args = e.Parameter as VoiceCommandActivatedEventArgs;
                var speechRecognitionResult = args.Result;

                var command = speechRecognitionResult.Text;
                statusMessage.Text = string.Format("Esecuzione del comando {0}...", command);

                var action = speechRecognitionResult.RulePath.FirstOrDefault();
                var turnOn = (action == "turnOn");

                var semanticProperties = speechRecognitionResult.SemanticInterpretation.Properties;

                // Figure out the color.
                var color = string.Empty;
                if (semanticProperties.ContainsKey("colors"))
                    color = semanticProperties["colors"][0];

                var rgb = this.GetRgb(color, turnOn);

                using (var client = new RestClient(SERVICE_URL))
                {
                    try
                    {
                        await client.PostAsync("led/set", rgb);
                        statusMessage.Text = "Comando inviato con successo.";
                    }
                    catch (Exception ex)
                    {
                        statusMessage.Text = "Errore durante l'invio del comando: " + ex.Message;
                    }
                }
            }
        }

        private Rgb GetRgb(string color, bool turnOn)
        {
            var rgb = new Rgb();

            if (turnOn)
            {
                switch (color.ToLower())
                {
                    case "rosso":
                        rgb.Red = turnOn;
                        break;

                    case "verde":
                        rgb.Green = turnOn;
                        break;

                    case "blu":
                        rgb.Blue = turnOn;
                        break;

                    case "bianco":
                        rgb.Red = turnOn;
                        rgb.Green = turnOn;
                        rgb.Blue = turnOn;
                        break;
                }
            }

            return rgb;
        }
    }
}
