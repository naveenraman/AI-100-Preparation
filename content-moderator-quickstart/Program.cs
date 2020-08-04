using Microsoft.Azure.CognitiveServices.ContentModerator;
using Microsoft.Azure.CognitiveServices.ContentModerator.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace content_moderator_quickstart
{
    class Program
    {
        //<Credentials>
        // Your Content Moderator subscription key is found in your Azure portal resource on the 'Keys' page. Add to your environment variables.
        private static readonly string SubscriptionKey = Environment.GetEnvironmentVariable("CONTENT_MODERATOR_SUBSCRIPTION_KEY", EnvironmentVariableTarget.User);
        // Base endpoint URL. Add this to your environment variables. 
        private static readonly string Endpoint = Environment.GetEnvironmentVariable("CONTENT_MODERATOR_ENDPOINT", EnvironmentVariableTarget.User);
        //</Credentials>

        //<TextModeration>
        // Name of the file that contains text
        private static readonly string TextFile = "TextFile.txt";
        // The name of the file to contain the output from the evaluation.
        private static string TextOutputFile = "TextModerationOutput.txt";
        //</TextModeration>

        //<ImageModeration>
        //The name of the file that contains the image URLs to evaluate.
        private static readonly string ImageUrlFile = "ImageFiles.txt";
        // The name of the file to contain the output from the evaluation.
        private static string ImageOutputFile = "ImageModerationOutput.json";
        //</ImageModeration>

        static void Main(string[] args)
        {
            //<Client>
            // Create an image review client
            ContentModeratorClient clientImage = Authenticate(SubscriptionKey, Endpoint);
            // Create a text review client
            ContentModeratorClient clientText = Authenticate(SubscriptionKey, Endpoint);
            // Create a human reviews client
            ContentModeratorClient clientReviews = Authenticate(SubscriptionKey, Endpoint);
            //</Client>

            //<TextModerateCall>
            // Moderate text from text in a file
            ModerateText(clientText, TextFile, TextOutputFile);
            //</TextModerateCall>

            //<ImageModerateCall>
            // Moderate images from list of image URLs
            ModerateImages(clientImage, ImageUrlFile, ImageOutputFile);
            //</ImageModerateCall>

        }

        private static void ModerateImages(ContentModeratorClient clientImage, string imageUrlFile, string imageOutputFile)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// TEXT MODERATION -  moderates text from file
        /// </summary>
        /// <param name="clientText"></param>
        /// <param name="textFile"></param>
        /// <param name="textOutputFile"></param>
        private static void ModerateText(ContentModeratorClient client, string inputFile, string outputFile)
        {
            Console.WriteLine("--------------------------------------------------------------");
            Console.WriteLine();
            Console.WriteLine("TEXT MODERATION");
            Console.WriteLine();

            // Load the input text.
            string text = File.ReadAllText(inputFile);

            // Remove carriage returns
            text = text.Replace(Environment.NewLine, " ");

            // Convert string to a byte[], then into a stream (for parameter in ScreenText()).
            byte[] textBytes = Encoding.UTF8.GetBytes(text);
            MemoryStream stream = new MemoryStream(textBytes);

            Console.WriteLine("Screening {0}...", inputFile);

            // Save the moderation results to a file.
            using (StreamWriter outputWriter = new StreamWriter(outputFile, false))
            {
                using (client)
                {
                    // Screen the input text: check for profanity, classify the text into three categories,
                    // do autocorrect text, and check for personally identifying information (PII)
                    outputWriter.WriteLine("Autocorrect typos, check for matching terms, PII, and classify.");

                    // Moderate the text
                    var screenResult = client.TextModeration.ScreenText("text/plain", stream, "eng", true, true, null, true);
                    outputWriter.WriteLine(JsonConvert.SerializeObject(screenResult, Formatting.Indented));
                }

                outputWriter.Flush();
                outputWriter.Close();
            }

            Console.WriteLine("Results written to {0}", outputFile);
            Console.WriteLine();
        }

        /// <summary>
        /// Creates a new client with a validated subscription key and endpoint.
        /// </summary>
        /// <param name="subscriptionKey"></param>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        private static ContentModeratorClient Authenticate(string subscriptionKey, string endpoint)
        {
            ContentModeratorClient client = new ContentModeratorClient(new ApiKeyServiceClientCredentials(subscriptionKey));
            client.Endpoint = endpoint;
            return client;
        }
    }

    // Contains the image moderation results for an image, 
    // including text and face detection results.
    public class EvaluationData
    {
        // The URL of the evaluated image.
        public string ImageUrl;

        // The image moderation results.
        public Evaluate ImageModeration;

        // The text detection results.
        public OCR TextDetection;

        // The face detection results;
        public FoundFaces FaceDetection;
    }
}
