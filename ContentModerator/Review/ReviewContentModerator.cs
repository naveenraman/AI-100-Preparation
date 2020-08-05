using ContentModerator.Interfaces;
using ContentModerator.Review;
using Microsoft.Azure.CognitiveServices.ContentModerator;
using Microsoft.Azure.CognitiveServices.ContentModerator.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace ContentModerator.Review
{
    public class ReviewContentModerator : IContentModerator
    {
        //<Credentials>
        // Your Content Moderator subscription key is found in your Azure portal resource on the 'Keys' page. Add to your environment variables.
        private static readonly string SubscriptionKey = Environment.GetEnvironmentVariable("CONTENT_MODERATOR_SUBSCRIPTION_KEY", EnvironmentVariableTarget.User);
        // Base endpoint URL. Add this to your environment variables. 
        private static readonly string Endpoint = Environment.GetEnvironmentVariable("CONTENT_MODERATOR_ENDPOINT", EnvironmentVariableTarget.User);
        //</Credentials>
        // The list of URLs of the images to create review jobs for.
        private static readonly string[] IMAGE_URLS_FOR_REVIEW = new string[] { "https://moderatorsampleimages.blob.core.windows.net/samples/sample5.png" };
        // The name of the team to assign the review to. Must be the team name used to create your Content Moderator website account. 
        // If you do not yet have an account, follow this: https://docs.microsoft.com/en-us/azure/cognitive-services/content-moderator/quick-start
        // Select the gear symbol (settings)-->Credentials to retrieve it. Your team name is the Id associated with your subscription.
        private static readonly string TEAM_NAME = Environment.GetEnvironmentVariable("CONTENT_MODERATOR_TEAM_NAME", EnvironmentVariableTarget.User);
        // The callback endpoint for completed human reviews. Add to your environment variables. 
        // For example: https://westus.api.cognitive.microsoft.com/contentmoderator/review/v1.0
        // As reviewers complete reviews, results are sent using an HTTP POST request.
        private static readonly string ReviewsEndpoint = Environment.GetEnvironmentVariable("CONTENT_MODERATOR_REVIEWS_ENDPOINT", EnvironmentVariableTarget.User);

        private ContentModeratorClient _reviewClient;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private ContentModeratorClient Authenticate()
        {
            _reviewClient = new ContentModeratorClient(new ApiKeyServiceClientCredentials(SubscriptionKey))
            {
                Endpoint = Endpoint
            };
            return _reviewClient;
        }

        /// <summary>
        /// Create the reviews using the fixed list of images.
        /// </summary>
        public void Moderate()
        {
            //Create a human reviews client
            Authenticate();

            Console.WriteLine("--------------------------------------------------------------");
            Console.WriteLine();
            Console.WriteLine("CREATE HUMAN IMAGE REVIEWS");

            // The minimum amount of time, in milliseconds, to wait between calls to the Image List API.
            const int throttleRate = 2000;
            // The number of seconds to delay after a review has finished before getting the review results from the server.
            const int latencyDelay = 45;

            // The name of the log file to create. Relative paths are relative to the execution directory.
            const string OutputFile = "OutputLog.txt";

            // The optional name of the subteam to assign the review to. Not used for this example.
            //const string Subteam = null;

            // The media type for the item to review. Valid values are "image", "text", and "video".
            const string MediaType = "image";

            // The metadata key to initially add to each review item. This is short for 'score'.
            // It will enable the keys to be 'a' (adult) and 'r' (racy) in the response,
            // with a value of true or false if the human reviewer marked them as adult and/or racy.
            const string MetadataKey = "sc";
            // The metadata value to initially add to each review item.
            const string MetadataValue = "true";

            // A static reference to the text writer to use for logging.
            TextWriter writer;

            // The cached review information, associating a local content ID to the created review ID for each item.
            List<ReviewItem> reviewItems = new List<ReviewItem>();

            using (TextWriter outputWriter = new StreamWriter(OutputFile, false))
            {
                writer = outputWriter;
                WriteLine(writer, null, true);
                WriteLine(writer, "Creating reviews for the following images:", true);

                // Create the structure to hold the request body information.
                List<CreateReviewBodyItem> requestInfo = new List<CreateReviewBodyItem>();

                // Create some standard metadata to add to each item.
                List<CreateReviewBodyItemMetadataItem> metadata =
                    new List<CreateReviewBodyItemMetadataItem>(new CreateReviewBodyItemMetadataItem[]
                    { new CreateReviewBodyItemMetadataItem(MetadataKey, MetadataValue) });

                // Populate the request body information and the initial cached review information.
                for (int i = 0; i < IMAGE_URLS_FOR_REVIEW.Length; i++)
                {
                    // Cache the local information with which to create the review.
                    var itemInfo = new ReviewItem()
                    {
                        Type = MediaType,
                        ContentId = i.ToString(),
                        Url = IMAGE_URLS_FOR_REVIEW[i],
                        ReviewId = null
                    };

                    WriteLine(writer, $" {Path.GetFileName(itemInfo.Url)} with id = {itemInfo.ContentId}.", true);

                    // Add the item informaton to the request information.
                    requestInfo.Add(new CreateReviewBodyItem(itemInfo.Type, itemInfo.Url, itemInfo.ContentId, ReviewsEndpoint, metadata));

                    // Cache the review creation information.
                    reviewItems.Add(itemInfo);
                }

                var reviewResponse = _reviewClient.Reviews.CreateReviewsWithHttpMessagesAsync("application/json", TEAM_NAME, requestInfo);

                // Update the local cache to associate the created review IDs with the associated content.
                var reviewIds = reviewResponse.Result.Body;
                for (int i = 0; i < reviewIds.Count; i++) { reviewItems[i].ReviewId = reviewIds[i]; }

                WriteLine(outputWriter, JsonConvert.SerializeObject(reviewIds, Formatting.Indented));
                Thread.Sleep(throttleRate);

                // Get details of the reviews created that were sent to the Content Moderator website.
                WriteLine(outputWriter, null, true);
                WriteLine(outputWriter, "Getting review details:", true);
                foreach (var item in reviewItems)
                {
                    var reviewDetail = _reviewClient.Reviews.GetReviewWithHttpMessagesAsync(TEAM_NAME, item.ReviewId);
                    WriteLine(outputWriter, $"Review {item.ReviewId} for item ID {item.ContentId} is " +
                        $"{reviewDetail.Result.Body.Status}.", true);
                    WriteLine(outputWriter, JsonConvert.SerializeObject(reviewDetail.Result.Body, Formatting.Indented));
                    Thread.Sleep(throttleRate);
                }

                Console.WriteLine();
                Console.WriteLine("Perform manual reviews on the Content Moderator site.");
                Console.WriteLine("Then, press any key to continue.");
                Console.ReadKey();

                // After the human reviews, the results are confirmed.
                Console.WriteLine();
                Console.WriteLine($"Waiting {latencyDelay} seconds for results to propagate.");
                Thread.Sleep(latencyDelay * 1000);

                // Get details from the human review.
                WriteLine(writer, null, true);
                WriteLine(writer, "Getting review details:", true);
                foreach (var item in reviewItems)
                {
                    var reviewDetail = _reviewClient.Reviews.GetReviewWithHttpMessagesAsync(TEAM_NAME, item.ReviewId);
                    WriteLine(writer, $"Review {item.ReviewId} for item ID {item.ContentId} is " + $"{reviewDetail.Result.Body.Status}.", true);
                    WriteLine(outputWriter, JsonConvert.SerializeObject(reviewDetail.Result.Body, Formatting.Indented));

                    Thread.Sleep(throttleRate);
                }

                Console.WriteLine();
                Console.WriteLine("Check the OutputLog.txt file for results of the review.");

                writer = null;
                outputWriter.Flush();
                outputWriter.Close();
            }
            Console.WriteLine("--------------------------------------------------------------");
        }

        /// <summary>
        /// Helper function that writes a message to the log file, and optionally to the console.
        /// If echo is set to true, details will be written to the console.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="message"></param>
        /// <param name="echo"></param>
        private static void WriteLine(TextWriter writer, string message = null, bool echo = true)
        {
            writer.WriteLine(message ?? String.Empty);
            if (echo) { Console.WriteLine(message ?? String.Empty); }
        }
    }
}
