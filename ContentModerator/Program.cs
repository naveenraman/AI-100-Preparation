using ContentModerator.Image;
using ContentModerator.Review;
using ContentModerator.Text;

namespace ContentModerator
{
    class Program
    {
        static void Main(string[] args)
        {
            //<Moderator>
            // Create an image review moderator 
            TextContentModerator textContentModerator = new TextContentModerator();
            // Create a text review moderator
            ImageContentModerator imageContentModerator = new ImageContentModerator();
            // Create a human reviews moderator
            ReviewContentModerator reviewContentModerator = new ReviewContentModerator();
            //</Moderator>

            //<TextModerateCall>
            // Moderate text from text in a file
            textContentModerator.Moderate();
            //</TextModerateCall>

            //<ImageModerateCall>
            // Moderate images from list of image URLs
            imageContentModerator.Moderate();
            //</ImageModerateCall>

            //<ReviewModerateCall>
            // Create image reviews for human reviewers
            reviewContentModerator.Moderate();
            //</ReviewModerateCall>
        }
    }
}
