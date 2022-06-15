namespace Play.Inventory.Service.Dtos
{


  public record GrandItemsDto(Guid UserId, Guid CatalogItemId, int Quantity);

  public record InventoryItemDto(Guid CatalogItemId, int Quantity, DateTimeOffset AcquiredDate, string Name, string Description);


  public record CatalogItemDto(Guid Id, string Name, string Description);

}