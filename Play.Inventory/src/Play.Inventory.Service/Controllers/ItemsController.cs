using Microsoft.AspNetCore.Mvc;
using Play.Common;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Dtos;
using Play.Inventory.Service.Entities;

namespace Play.Inventory.Service.Controllers
{

  [ApiController]
  [Route("items")]
  public class ItemsController : ControllerBase
  {
    private readonly IRepository<InventoryItem> itemsRepository;

    private readonly CatalogClient catalogClient;

    public ItemsController(IRepository<InventoryItem> itemsRepository, CatalogClient catalogClient)

    {
      this.itemsRepository = itemsRepository;
      this.catalogClient = catalogClient;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync(Guid userId)
    {
      if (userId == Guid.Empty)
      {
        return BadRequest();
      }


      var catalogItems = await catalogClient.GetCatalogItemsAsync();

      var inventoryItemEntities = await itemsRepository.GetAllAsync(item => item.UserId == userId);

      var inventoryItemsDtos = inventoryItemEntities.Select(inventoryItem =>
      {
        var catalogItem = catalogItems.Single(catalogItem => catalogItem.Id == inventoryItem.CatalogItemId);
        return inventoryItem.AsDto(catalogItem.Name, catalogItem.Description);
      });

      return Ok(inventoryItemsDtos);
    }


    [HttpPost]
    public async Task<ActionResult> PostAsync(GrandItemsDto grandItemsDto)
    {

      var inventoryItem = await itemsRepository.GetAsync(item => item.UserId == grandItemsDto.UserId && item.CatalogItemId == grandItemsDto.CatalogItemId);


      if (inventoryItem == null)
      {
        inventoryItem = new InventoryItem
        {
          UserId = grandItemsDto.UserId,
          CatalogItemId = grandItemsDto.CatalogItemId,
          Quantity = grandItemsDto.Quantity,
          AcquiredDate = DateTimeOffset.Now
        };
        await itemsRepository.CreateAsync(inventoryItem);
      }
      else
      {
        inventoryItem.Quantity += grandItemsDto.Quantity;
        await itemsRepository.UpdateAsync(inventoryItem);
      }

      return Ok();



    }


  }


}