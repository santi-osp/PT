using DataGo.Domain;
using Microsoft.EntityFrameworkCore;

namespace DataGo.Infrastructure;

internal static class SeedData
{
    private static Guid Id(int value) => Guid.Parse($"00000000-0000-0000-0000-{value:D12}");

    public static void Add(ModelBuilder modelBuilder)
    {
        var countryId = Id(1);
        var departmentId = Id(2);
        var municipalityId = Id(3);
        modelBuilder.Entity<Country>().HasData(new Country(countryId, "Colombia"));
        modelBuilder.Entity<Department>().HasData(new Department(departmentId, countryId, "Antioquia"));
        modelBuilder.Entity<Municipality>().HasData(new Municipality(municipalityId, departmentId, "Medellín"));

        var zoneCodes = new[] { "MDE-NORTE", "MDE-NORORIENTE", "MDE-CENTRO", "MDE-CENTRO-OCCIDENTE", "MDE-OCCIDENTE", "MDE-SUROCCIDENTE", "MDE-SUR", "MDE-SURORIENTE" };
        var zones = zoneCodes.Select((code, i) => new TransportZone(Id(10 + i), code)).ToArray();
        modelBuilder.Entity<TransportZone>().HasData(zones);

        var neighborhoodNames = new[]
        {
            "Laureles", "Conquistadores", "Florida Nueva", "Estadio", "Los Colores", "Carlos E. Restrepo",
            "El Poblado", "Provenza", "Manila", "Astorga", "Castropol", "Santa María de los Ángeles",
            "Belén", "Belén Rosales", "Belén La Palma", "Belén Fátima", "Belén Granada", "Los Alpes",
            "La América", "Santa Lucía", "Calasanz", "La Floresta", "Santa Mónica", "Robledo", "Pilarica",
            "Córdoba", "López de Mesa", "Boston", "Bomboná", "Prado", "Villa Nueva", "Buenos Aires",
            "Miraflores", "La Milagrosa", "Aranjuez", "Manrique Central", "Campo Valdés", "Moravia",
            "Castilla", "Pedregal", "Boyacá Las Brisas", "Guayabal", "Cristo Rey", "Trinidad"
        };
        modelBuilder.Entity<Neighborhood>().HasData(neighborhoodNames.Select((name, i) =>
            new Neighborhood(Id(100 + i), municipalityId, zones[i % zones.Length].Id, name)));

        // Fictitious names created solely as demonstration data for the technical exercise.
        var centerNames = new[]
        {
            "Centro Medellín Norte", "Centro Medellín Sur", "Centro Medellín Oriente", "Centro Medellín Occidente",
            "Centro Medellín Centro", "Centro Laureles", "Centro El Poblado", "Centro Belén", "Centro Guayabal",
            "Centro La América", "Centro Robledo", "Centro Castilla", "Centro Aranjuez", "Centro Buenos Aires",
            "Centro Manrique", "Centro San Javier", "Centro Doce de Octubre", "Centro Villa Hermosa",
            "Centro Santa Cruz", "Centro Popular"
        };
        modelBuilder.Entity<Center>().HasData(centerNames.Select((name, i) =>
            new Center(Id(200 + i), $"MDE-C{i + 1:D3}", name)));

        modelBuilder.Entity<ModernChannelCustomer>().HasData(
            new ModernChannelCustomer(Id(301), DocumentType.CC, "1010000001"),
            new ModernChannelCustomer(Id(302), DocumentType.CC, "1010000002"),
            new ModernChannelCustomer(Id(303), DocumentType.CE, "2010000001"),
            new ModernChannelCustomer(Id(304), DocumentType.NIT, "900123456"),
            new ModernChannelCustomer(Id(305), DocumentType.NIT, "901234567"));
    }
}
