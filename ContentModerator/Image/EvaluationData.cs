using Microsoft.Azure.CognitiveServices.ContentModerator.Models;

namespace ContentModerator.Image
{
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
