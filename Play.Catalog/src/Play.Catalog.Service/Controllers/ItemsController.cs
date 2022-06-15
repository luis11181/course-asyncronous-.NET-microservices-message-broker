using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using Play.Catalog.Service.Dtos;
using Play.Catalog.Service.Entities;
using Play.Common;
using MassTransit;
using Play.Catalog.Contracts;

namespace Play.Catalog.Service.Controllers;


[ApiController]
//HTT://localhost:5001/items
[Route("items")]// sera la ruta root para las operaciones en ese controlador
public class ItemsController : ControllerBase
{

  // intanciamos el repositorio de ongo db, con este objeto de la clase
  private readonly IRepository<Item> itemsRepository;

  private readonly IPublishEndpoint publishEndpoint;

  public ItemsController(IRepository<Item> itemsRepository, IPublishEndpoint publishEndpoint)
  {
    this.itemsRepository = itemsRepository;
    this.publishEndpoint = publishEndpoint;
  }

  //IEnumerable<T> is the base interface for collections in the System.Collections.Generic namespace such as List<T>, Dictionary<TKey,TValue>, and Stack<T> and other generic collections such as ObservableCollection<T> and ConcurrentStack<T>. Collections that implement IEnumerable<T> can be enumerated by using the foreach statement.
  [HttpGet(Name = "NameForGetValueEndpoint")]
  public async Task<IEnumerable<ItemDto>> GetAsync()
  {
    var items = (await itemsRepository.GetAllAsync()).Select(item => Extensions.AsDto(item)); // other option is to use item.AsDto()

    return items;
  }

  [HttpGet("{id}")]//HTTP://localhost:5001/items/{id}
  public async Task<ActionResult<ItemDto>> GetByIdAsync(Guid id)
  {
    var item = await itemsRepository.GetAsync(id);// obtiene el ,el item de la coleccion de mongodb

    if (item == null)
    {
      return NotFound();// * funcion que retornanot found el cliente
    }

    return item.AsDto();// toca convertir el tipo de dato a el que queremos en Dto
  }

  [HttpPost]
  public async Task<ActionResult<ItemDto>> PostAsync(CreateItemDto createItemdto)
  {
    var item = new Item()
    {
      Name = createItemdto.Name,
      Description = createItemdto.Description,
      Price = createItemdto.Price,
      CreatedDate = DateTimeOffset.UtcNow
    };

    await itemsRepository.CreateAsync(item);

    //* action that will publish the mesage to the mesage broker for async processing
    await publishEndpoint.Publish(new CatalogItemCreated(item.Id, item.Name, item.Description));

    //* An ActionResult that returns a Created (201) response with a Location header.
    return CreatedAtRoute("NameForGetValueEndpoint", new { id = item.Id }, item);

  }

  [HttpPut("{id}")]
  public async Task<IActionResult> PutAsync(Guid id, UpdateItemDto updateItemDto)
  {
    var existingItem = await itemsRepository.GetAsync(id);

    if (existingItem == null)
    {
      return NotFound();
    }

    existingItem.Name = updateItemDto.Name;
    existingItem.Description = updateItemDto.Description;
    existingItem.Price = updateItemDto.Price;

    await itemsRepository.UpdateAsync(existingItem);


    //* action that will publish the mesage to the mesage broker for async processing
    await publishEndpoint.Publish(new CatalogItemUpdated(existingItem.Id, existingItem.Name, existingItem.Description));

    return NoContent();
  }


  [HttpDelete("{id}")]
  public async Task<IActionResult> DeleteAsync(Guid id)
  {
    var existingItem = await itemsRepository.GetAsync(id);

    if (existingItem == null)
    {
      return NotFound();
    }

    await itemsRepository.RemoveAsync(existingItem.Id);


    //* action that will publish the mesage to the mesage broker for async processing
    await publishEndpoint.Publish(new CatalogItemDeleted(existingItem.Id));

    return NoContent();
  }


}


