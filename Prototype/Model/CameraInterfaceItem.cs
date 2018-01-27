namespace Prototype.Model
{
    public class CameraInterfaceItem
    {
        public CameraInterfaceItem()
        {

        }

        public CameraInterfaceItem(string name)
        {
            Id = name;
            Name = name;
        }

        public string Id { get; set; }
        public string Name { get; set; }
    }
}
