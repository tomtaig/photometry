namespace Prototype.Model
{
    public class CameraItem
    {
        public CameraItem()
        {

        }

        public CameraItem(string name)
        {
            Id = name;
            Name = name;
        }

        public string Id { get; set; }
        public string Name { get; set; }
    }
}
