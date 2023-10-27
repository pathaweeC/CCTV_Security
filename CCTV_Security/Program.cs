using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
    private const string LineAccessToken = "JAxPFavKxgi07CLsN2eE29Li09eXa6Ab7ncHikkFAtQ";
    private static DateTime lastUpdateTime = DateTime.MinValue;

    static async Task Main(string[] args)
    {
        // Get Current Date
        DateTime currentDate = DateTime.Now;
        string Date = currentDate.ToString("yyyy-MM-dd");
        string localDirectory = "@/data/MotionDetect/" + Date + "";

        while (true)
        {
            Console.WriteLine("Waiting for new images in the folder...");
            string latestImage = GetLatestImage(localDirectory);

            //if (!string.IsNullOrEmpty(latestImage) && File.GetLastWriteTime(latestImage) > lastUpdateTime)
            //{
                // Send the latest image to Line Notify with a message
                await SendPictureToLineNotifyAsync(latestImage, "Motion Detected");
                lastUpdateTime = File.GetLastWriteTime(latestImage);
            //}

            // Sleep for a period before checking again (e.g., every 5 seconds)
            await Task.Delay(TimeSpan.FromSeconds(5));
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

    static async Task SendPictureToLineNotifyAsync(string imagePath, string message)
    {
        using (var httpClient = new HttpClient())
        {
            var content = new MultipartFormDataContent();
            content.Add(new StreamContent(File.OpenRead(imagePath)), "imageFile", Path.GetFileName(imagePath));
            content.Add(new StringContent(message), "message");

            var response = await httpClient.PostAsync($"https://notify-api.line.me/api/notify?token={LineAccessToken}", content);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Failed to send Line Notify message.");
            }
            else
            {
                Console.WriteLine("Line Notify message sent successfully.");
            }
        }
    }
}
