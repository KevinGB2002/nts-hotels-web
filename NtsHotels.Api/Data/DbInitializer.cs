using Microsoft.EntityFrameworkCore;
using NtsHotels.Api.Models;

namespace NtsHotels.Api.Data;

public static class DbInitializer
{
    private const string AdminRoleName = "Admin";
    private const string AdminEmail = "admin@ntshotels.com";
    private const string AdminDefaultPassword = "Admin123*";

    public static async Task SeedAsync(ApplicationDbContext db)
    {
        await SeedAdminAsync(db);
        await SeedHotelsAsync(db);
    }

    private static async Task SeedAdminAsync(ApplicationDbContext db)
    {
        var adminRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == AdminRoleName);
        if (adminRole is null)
        {
            adminRole = new Role { Name = AdminRoleName };
            db.Roles.Add(adminRole);
            await db.SaveChangesAsync();
        }

        if (await db.Users.AnyAsync(u => u.RoleId == adminRole.Id))
            return;

        var admin = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Administrador NTS",
            Email = AdminEmail,
            RoleId = adminRole.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(AdminDefaultPassword);

        db.Users.Add(admin);
        await db.SaveChangesAsync();
    }

    private static async Task SeedHotelsAsync(ApplicationDbContext db)
    {
        if (await db.Hotels.AnyAsync())
            return;

        var now = DateTime.UtcNow;
        var hotels = new List<Hotel>
        {
            new() { Id = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef0123456789"), Name = "Hotel Gran Plaza Bogota", Description = "Hotel de lujo ubicado en el centro de la ciudad con excelente vista y comodidades ejecutivas.", Address = "Calle 72 # 10-34", City = "Bogotá", Rating = 5, ImageUrl = "https://images.unsplash.com/photo-1566073771259-6a8506099945", IsActive = true, CreatedAt = now },
            new() { Id = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f0123456789a"), Name = "Hotel Campestre El Descanso", Description = "Un espacio ideal para conectar con la naturaleza, zonas verdes y descanso familiar.", Address = "Km 5 Vía Principal", City = "Cajicá", Rating = 4, ImageUrl = "https://images.unsplash.com/photo-1582719478250-c89cae4dc85b", IsActive = true, CreatedAt = now },
            new() { Id = Guid.Parse("c3d4e5f6-a7b8-9012-cdef-0123456789ab"), Name = "Business Suite Express", Description = "Pensado para viajeros de negocios, con conectividad de alta velocidad y salas de juntas.", Address = "Av. El Dorado # 68-12", City = "Bogotá", Rating = 3, ImageUrl = "https://images.unsplash.com/photo-1590490360182-c33d57733427", IsActive = true, CreatedAt = now },
            new() { Id = Guid.Parse("d4e5f6a7-b8c9-0123-def0-123456789abc"), Name = "Hotel Colonial Suesca", Description = "Encanto histórico y arquitectura colonial rodeado de montañas y naturaleza para desconectarse.", Address = "Carrera 4 # 5-20", City = "Suesca", Rating = 4, ImageUrl = "https://images.unsplash.com/photo-1551882547-ff40c63fe5fa", IsActive = true, CreatedAt = now },
            new() { Id = Guid.Parse("e5f6a7b8-c9d0-1234-ef01-23456789abcd"), Name = "Hotel Mirador del Caribe", Description = "Ubicado frente al mar Caribe con arquitectura moderna, piscinas infinitas y acceso directo a la playa.", Address = "Bocagrande Cra. 2 # 5-10", City = "Cartagena", Rating = 5, ImageUrl = "https://images.unsplash.com/photo-1571896349842-33c89424de2d", IsActive = true, CreatedAt = now },
            new() { Id = Guid.Parse("f6a7b8c9-d0e1-2345-f012-3456789abcde"), Name = "Hotel Boutique Eje Cafetero", Description = "Disfruta del paisaje cultural cafetero, rodeado de cafetales, tranquilidad y café de origen artesanal.", Address = "Km 3 Vía Armenia", City = "Pereira", Rating = 4, ImageUrl = "https://images.unsplash.com/photo-1540541338287-41700207dee6", IsActive = true, CreatedAt = now },
            new() { Id = Guid.Parse("1a2b3c4d-5e6f-7890-a1b2-c3d4e5f67890"), Name = "Medellín Urban Living", Description = "Moderno hotel en el corazón de El Poblado, cerca de la mejor oferta gastronómica y de entretenimiento.", Address = "Calle 10 # 35-20", City = "Medellín", Rating = 4, ImageUrl = "https://images.unsplash.com/photo-1561501900-3701fa6a0864", IsActive = true, CreatedAt = now },
            new() { Id = Guid.Parse("2b3c4d5e-6f7a-8901-b2c3-d4e5f678901a"), Name = "Hotel Villa de Leyva Colonial", Description = "A pocos pasos de la Plaza Mayor, con hermosos patios interiores empedrados, flores y chimeneas.", Address = "Carrera 9 # 12-45", City = "Villa de Leyva", Rating = 5, ImageUrl = "https://images.unsplash.com/photo-1566665797739-1674de7a421a", IsActive = true, CreatedAt = now },
            new() { Id = Guid.Parse("3c4d5e6f-7a8b-9012-c3d4-e5f678901ab2"), Name = "Hostería Campestre Andina Chía", Description = "Un refugio muy acogedor cerca de la sabana, excelente para fines de semana en familia y comida típica.", Address = "Vía Chía-Cajicá Km 2", City = "Chía", Rating = 3, ImageUrl = "https://images.unsplash.com/photo-1520250497591-112f2f40a3f4", IsActive = true, CreatedAt = now }
        };

        db.Hotels.AddRange(hotels);
        await db.SaveChangesAsync();
    }
}
