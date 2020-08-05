using ContentModerator.Interfaces;
using Microsoft.Azure.CognitiveServices.ContentModerator;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace ContentModerator.Text
{
    public class TextContentModerator : IContentModerator
    {
        //<Credentials>
        // Your Content Moderator subscription key is found in your Azure portal resource on the 'Keys' page. Add to your environment variables.
        private static readonly string SubscriptionKey = Environment.GetEnvironmentVariable("CONTENT_MODERATOR_SUBSCRIPTION_KEY", EnvironmentVariableTarget.User);
        // Base endpoint URL. Add this to your environment variables. 
        private static readonly string Endpoint = Environment.GetEnvironmentVariable("CONTENT_MODERATOR_ENDPOINT", EnvironmentVariableTarget.User);
        //</Credentials>
        //<TextModeration>
        // Name of the file that contains text
        private static readonly string inputTextFile = "TextFile.txt";
        // The name of the file to contain the output from the evaluation.
        private static string outputTextFile = "TextModerationOutput.txt";
        //</TextModeration>

        private ContentModeratorClient _textClient;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private ContentModeratorClient Authenticate()
        {
            _textClient = new ContentModeratorClient(new ApiKeyServiceClientCredentials(SubscriptionKey))
            {
                Endpoint = Endpoint
            };
            return _textClient;
        }

        /// <summary>
        /// Moderate text from text in a file
        /// </summary>
        public void Moderate()
        {
            //Create an image review client
            Authenticate();

            Console.WriteLine("--------------------------------------------------------------");
            Console.WriteLine();
            Console.WriteLine("TEXT MODERATION");
            Console.WriteLine();

            // Load the input text.
            string text = File.ReadAllText(inputTextFile);

            // Remove carriage returns
            text = text.Replace(Environment.NewLine, " ");

            // Convert string to a byte[], then into a stream (for parameter in ScreenText()).
            byte[] textBytes = Encoding.UTF8.GetBytes(text);
            MemoryStream stream = new MemoryStream(textBytes);

            Console.WriteLine("Screening {0}...", inputTextFile);

            // Save the moderation results to a file.
            using (StreamWriter outputWriter = new StreamWriter(outputTextFile, false))
            {
                using (_textClient)
                {
                    // Screen the input text: check for profanity, classify the text into three categories,
                    // do autocorrect text, and check for personally identifying information (PII)
                    outputWriter.WriteLine("Autocorrect typos, check for matching terms, PII, and classify.");

                    // Moderate the text
                    var screenResult = _textClient.TextModeration.ScreenText("text/plain", stream, "eng", true, true, null, true);
                    outputWriter.WriteLine(JsonConvert.SerializeObject(screenResult, Formatting.Indented));
                }

                outputWriter.Flush();
                outputWriter.Close();
            }

            Console.WriteLine("Results written to {0}", outputTextFile);
            Console.WriteLine();
        }
    }
}
