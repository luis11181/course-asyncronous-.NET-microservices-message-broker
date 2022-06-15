using Play.Catalog.Service.Entities;



namespace Play.Catalog.Service;

public static class Extensions
{
  public static Dtos.ItemDto AsDto(this Item item)//! extension methos, this en esta funcion permite que se pueda usar el objeto item, sobre el cual se esta trabajando, item.AsDto()
  {
    return new Dtos.ItemDto(item.Id,
     item.Name,
      item.Description,
      item.Price,
      item.CreatedDate
     );
  }
}