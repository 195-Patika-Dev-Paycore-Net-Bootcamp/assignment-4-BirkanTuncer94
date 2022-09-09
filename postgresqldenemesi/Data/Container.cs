using System;

namespace HW4_BirkanTuncer.Data
{
    public class Container
    {
        public Container() { }
        public virtual int Id { get; set; }
        public virtual string ContainerName { get; set; }
        public virtual double Latitude { get; set; }
        public virtual double Longitude { get; set; }
        public virtual int VehicleId { get; set; }

        
    }
}
