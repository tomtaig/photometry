using System.Collections.Generic;

namespace Tests
{
    public class FakePhoto
    {
        public FakePhoto()
        {
            BadPixels = new List<BadPixel>();
            Stars = new List<StarInfo>();
        }

        public List<BadPixel> BadPixels { get; set; }
        public List<StarInfo> Stars { get; set; }
    }
}
