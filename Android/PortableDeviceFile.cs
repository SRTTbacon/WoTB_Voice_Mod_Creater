// Portable Device File

namespace PortableDevices
{
    public class PortableDeviceFile : PortableDeviceObject
    {
        public long size = 0;

        public PortableDeviceFile (string id, string name, long objSiz) : base(id, name)
        {
            size = objSiz;
        }

        public string Path { get; set; }
    }
}