using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Tabs.Model;
using Xamarin.Forms;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Tabs
{
    public partial class CustomVision : ContentPage
    {
        public CustomVision()
        {
            InitializeComponent();
        }

        private async void loadCamera(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await DisplayAlert("No Camera", ":( No camera available.", "OK");
                return;
            }

            MediaFile file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
            {
                PhotoSize = PhotoSize.Medium,
                Directory = "Sample",
                Name = $"{DateTime.UtcNow}.jpg"
            });

            if (file == null)
                return;

            image.Source = ImageSource.FromStream(() =>
            {
                return file.GetStream();
            });


            await MakePredictionRequest(file);
        }

        static byte[] GetImageAsByteArray(MediaFile file)
        {
            var stream = file.GetStream();
            BinaryReader binaryReader = new BinaryReader(stream);
            return binaryReader.ReadBytes((int)stream.Length);
        }

        async Task MakePredictionRequest(MediaFile file)
        {
            var client = new HttpClient();

            client.DefaultRequestHeaders.Add("Prediction-Key", "103d18ea607f45878ebc064ab7104661");

            string url = "https://southcentralus.api.cognitive.microsoft.com/customvision/v1.0/Prediction/54aa2f87-9414-44b9-81ca-0e8a8d0c9564/image?iterationId=beba495e-e48b-4592-9cd6-a684b82b1d8e";

            HttpResponseMessage response;

            byte[] byteData = GetImageAsByteArray(file);

            using (var content = new ByteArrayContent(byteData))
            {
                TagLabel.Text = "";
                PredictionLabel.Text = "";
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response = await client.PostAsync(url, content);
               

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    JObject rss = JObject.Parse(responseString);
                    //Querying with LINQ
                    //Get all Prediction Values
                    var Probability = from p in rss["Predictions"] select (int)p["Probability"];
                    var Tag = from p in rss["Predictions"] select (string)p["Tag"];
                    List<string> list = new List<string>();

                    //Truncate values to labels in XAML
                    foreach (var item in Tag)
                    {
                        TagLabel.Text += item + ": \n";
                    }
                    int index = 0;
                    foreach (var item in Probability)
                    {
                        PredictionLabel.Text += item + "\n";
                    }

                }
                NotHotDogModel Model = new NotHotDogModel()
                {
                    Tag = "balls"
                };
                await AzureManager.AzureManagerInstance.PostHotDogInformation(Model);
                //Get rid of file once we have finished using it
                file.Dispose();
            }
        }
    }
}