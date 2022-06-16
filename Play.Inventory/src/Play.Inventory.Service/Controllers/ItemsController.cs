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
    private readonly IRepository<InventoryItem> inventoryItemsRepository;

    // private readonly CatalogClient catalogClient;

    private readonly IRepository<CatalogItem> catalogItemsRepository;

    public ItemsController(IRepository<InventoryItem> itemsRepository, IRepository<CatalogItem> catalogItemsRepository)

    {
      this.inventoryItemsRepository = itemsRepository;
      this.catalogItemsRepository = catalogItemsRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync(Guid userId)
    {
      if (userId == Guid.Empty)
      {
        return BadRequest();
      }


      var inventoryItemEntities = await inventoryItemsRepository.GetAllAsync(item => item.UserId == userId);

      var itemIds = inventoryItemEntities.Select(item => item.CatalogItemId);

      var catalogItemsEntities = await catalogItemsRepository.GetAllAsync(item => itemIds.Contains(item.Id));


      var inventoryItemsDtos = inventoryItemEntities.Select(inventoryItem =>
      {
        var catalogItem = catalogItemsEntities.Single(catalogItem => catalogItem.Id == inventoryItem.CatalogItemId);
        return inventoryItem.AsDto(catalogItem.Name, catalogItem.Description);
      });

      return Ok(inventoryItemsDtos);
    }


    [HttpPost]
    public async Task<ActionResult> PostAsync(GrandItemsDto grandItemsDto)
    {

      var inventoryItem = await inventoryItemsRepository.GetAsync(item => item.UserId == grandItemsDto.UserId && item.CatalogItemId == grandItemsDto.CatalogItemId);


      if (inventoryItem == null)
      {
        inventoryItem = new InventoryItem
        {
          UserId = grandItemsDto.UserId,
          CatalogItemId = grandItemsDto.CatalogItemId,
          Quantity = grandItemsDto.Quantity,
          AcquiredDate = DateTimeOffset.Now
        };
        await inventoryItemsRepository.CreateAsync(inventoryItem);
      }
      else
      {
        inventoryItem.Quantity += grandItemsDto.Quantity;
        await inventoryItemsRepository.UpdateAsync(inventoryItem);
      }

      return Ok();



    }


  }


}