﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

class Program
{
    private static DateTime lastUpdateTime = DateTime.MinValue;
    private const string lineNotifyToken = "JAxPFavKxgi07CLsN2eE29Li09eXa6Ab7ncHikkFAtQ";

    private static async Task Main(string[] args)
    {

        // Get Current Date
        DateTime currentDate = DateTime.Now;
        string Date = currentDate.ToString("yyyy-MM-dd");

        string localDirectory = "@/data/MotionDetect/" + Date + "";

        if (!Directory.Exists(localDirectory))
        {
            Directory.CreateDirectory(localDirectory);
        }

        while (true)
        {
            try
            {
                Console.WriteLine(localDirectory);
                //Console.WriteLine("Date");

                string latestImage = GetLatestImage(localDirectory);
                //Console.WriteLine(latestImage);

                if (!string.IsNullOrEmpty(latestImage) && File.GetLastWriteTime(latestImage) > lastUpdateTime)
                {
                    Console.WriteLine("test");
                    // Send the latest image to Line
                    await SendPictureToLineNotifyAsync(latestImage, "Motion Detected");
                    lastUpdateTime = File.GetLastWriteTime(latestImage);
                }

                // Sleep for a period before checking again (e.g., every 5 seconds)
                await Task.Delay(TimeSpan.FromSeconds(5));

                /////////// send pic to line noti
                //SendPictureToLineNotifyAsync(latestImage, "Motion Detect").Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        // line noti sender task
        async Task SendPictureToLineNotifyAsync(string imagePath, string message)
        {

            using (var httpClient = new HttpClient())
            {
                var content = new MultipartFormDataContent();
                content.Add(new StreamContent(File.OpenRead(imagePath)), "imageFile", "image.jpg");
                content.Add(new StringContent(message), "message");

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("https://notify-api.line.me/api/notify"),
                    Content = content
                };

                request.Headers.Add("Authorization", "Bearer " + lineNotifyToken);

                var response = await httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Line Notify request failed with status code {response.StatusCode}");
                    Console.WriteLine($"Response content: {responseContent}");
                }
                else
                {
                    Console.WriteLine("Picture sent to Line Notify successfully!");
                }
            }
        }

        static string GetLatestImage(string folderPath)
        {
            string[] imageFiles = Directory.GetFiles(folderPath, "*.jpg");

            if (imageFiles.Length == 0)
            {
                return null;
            }

            Array.Sort(imageFiles, (a, b) => File.GetLastWriteTime(b).CompareTo(File.GetLastWriteTime(a)));

            return imageFiles[0];
        }
    }
}