using NHibernate;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace HW4_BirkanTuncer.Data
{
    public class ContainerMap : ClassMapping<Container>
    {
        public ContainerMap()
        {
            Id(x => x.Id, x =>
            {
                x.Type(NHibernateUtil.Int32);
                x.Column("Id");
                x.UnsavedValue(0);
                x.Generator(Generators.Increment);
            });
            Property(b => b.ContainerName, x =>
            {
                x.Length(50);
                x.Column("containername");
                x.NotNullable(false);
            });
            Property(x => x.Latitude, x =>
            {                
                x.Column("latitude");
                x.NotNullable(false);
            });
            Property(x => x.Longitude, x =>
            {
                x.Column("longitude");
                x.NotNullable(false);
            });
            Property(x => x.VehicleId, x =>
            {
                x.Column("vehicleid");
                x.NotNullable(true);
            });

            Table("container");
        }
    }
}
