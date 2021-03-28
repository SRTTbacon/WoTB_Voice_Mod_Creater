// Portable Device Object
using System;

namespace PortableDevices
{
    public abstract class PortableDeviceObject
    {
        protected PortableDeviceObject (string id, string name)
        {
            this.Id = id;
            this.Name = name;
        }

        public string Id { get; private set; }

        public string Name { get; private set; }

        public void DisplayObject ()
        {
            try
            {
                Console.WriteLine (Name);                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}