using ContentModerator.Interfaces;
using Microsoft.Azure.CognitiveServices.ContentModerator;
using Microsoft.Azure.CognitiveServices.ContentModerator.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace ContentModerator.Image
{
    public class ImageContentModerator : IContentModerator
    {
        //<Credentials>
        // Your Content Moderator subscription key is found in your Azure portal resource on the 'Keys' page. Add to your environment variables.
        private static readonly string SubscriptionKey = Environment.GetEnvironmentVariable("CONTENT_MODERATOR_SUBSCRIPTION_KEY", EnvironmentVariableTarget.User);
        // Base endpoint URL. Add this to your environment variables. 
        private static readonly string Endpoint = Environment.GetEnvironmentVariable("CONTENT_MODERATOR_ENDPOINT", EnvironmentVariableTarget.User);
        //</Credentials>
        //<ImageModeration>
        //The name of the file that contains the image URLs to evaluate.
        private static readonly string ImageUrlFile = "ImageFiles.txt";
        // The name of the file to contain the output from the evaluation.
        private static string outputImageFile = "ImageModerationOutput.json";
        //</ImageModeration>
        private ContentModeratorClient _imageClient;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private ContentModeratorClient Authenticate()
        {
            _imageClient = new ContentModeratorClient(new ApiKeyServiceClientCredentials(SubscriptionKey))
            {
                Endpoint = Endpoint
            };
            return _imageClient;
        }

        /// <summary>
        /// Moderate images from list of image URLs
        /// </summary>
        public void Moderate()
        {
            //Create a text review client
            Authenticate();

            Console.WriteLine("--------------------------------------------------------------");
            Console.WriteLine();
            Console.WriteLine("IMAGE MODERATION");
            Console.WriteLine();
            // Create an object to store the image moderation results.
            List<EvaluationData> evaluationData = new List<EvaluationData>();

            using (_imageClient)
            {
                // Read image URLs from the input file and evaluate each one.
                using (StreamReader inputReader = new StreamReader(ImageUrlFile))
                {
                    while (!inputReader.EndOfStream)
                    {
                        string line = inputReader.ReadLine().Trim();
                        if (line != String.Empty)
                        {
                            Console.WriteLine("Evaluating {0}...", Path.GetFileName(line));
                            var imageUrl = new BodyModel("URL", line.Trim());
                            var imageData = new EvaluationData
                            {
                                ImageUrl = imageUrl.Value,

                                // Evaluate for adult and racy content.
                                ImageModeration = _imageClient.ImageModeration.EvaluateUrlInput("application/json", imageUrl, true)
                            };
                            Thread.Sleep(1000);

                            // Detect and extract text.
                            imageData.TextDetection = _imageClient.ImageModeration.OCRUrlInput("eng", "application/json", imageUrl, true);
                            Thread.Sleep(1000);

                            // Detect faces.
                            imageData.FaceDetection = _imageClient.ImageModeration.FindFacesUrlInput("application/json", imageUrl, true);
                            Thread.Sleep(1000);

                            // Add results to Evaluation object
                            evaluationData.Add(imageData);
                        }
                    }
                }

                // Save the moderation results to a file.
                using (StreamWriter outputWriter = new StreamWriter(outputImageFile, false))
                {
                    outputWriter.WriteLine(JsonConvert.SerializeObject(evaluationData, Formatting.Indented));
                    outputWriter.Flush();
                    outputWriter.Close();
                }
                Console.WriteLine();
                Console.WriteLine("Image moderation results written to output file: " + outputImageFile);
                Console.WriteLine();
            }
        }
    }
}
