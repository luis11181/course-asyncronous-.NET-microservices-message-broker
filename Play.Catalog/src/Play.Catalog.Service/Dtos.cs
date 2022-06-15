

using System.ComponentModel.DataAnnotations;

namespace Play.Catalog.Service.Dtos;

//* A DTO is an object that defines how the data will be sent over the network

public record ItemDto(Guid Id, string Name, string Description, decimal Price, DateTimeOffset CreatedDate);

public record CreateItemDto([Required] string Name, string Description, [Range(0, 1000)] decimal Price);

public record UpdateItemDto([Required] string Name, string Description, [Range(0, 1000)] decimal Price);

