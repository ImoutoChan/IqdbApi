namespace IqdbApi.Models
{
    public class YourImage
    {
        public string Name { get; internal set; }

        public Resolution? Resolution { get; internal set; }

        public string PreviewUrl { get; internal set; }

        public string Size { get; internal set; }
    }
}