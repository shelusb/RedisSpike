using System;
using System.Diagnostics;
using System.Reflection.Metadata;
using EasyCaching.Core;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RedisTest.Models;

namespace RedisTest.Controllers
{
   [ApiController]
   [Route("api/[controller]")]
   public class PersonController : ControllerBase
   {
      private IEasyCachingProvider _cachingProvider;
      private IEasyCachingProviderFactory _cachingProviderFactory;

      public PersonController(IEasyCachingProviderFactory cachingProviderFactory)
      {
         _cachingProviderFactory = cachingProviderFactory;
         _cachingProvider = _cachingProviderFactory.GetCachingProvider("redis1");
      }

      [HttpGet("{personId}")]
      public IActionResult GetPerson(int personId)
      {
         if (!_cachingProvider.Exists(personId.ToString()))
            return BadRequest($"the person with id {personId} does not exist ...");

         var personString = _cachingProvider.Get<string>(personId.ToString());
         var person = JsonConvert.DeserializeObject<Person>(personString.ToString());
         
         Debug.WriteLine(person);

         return Ok(person);
      }

      [HttpPost]
      public IActionResult AddPerson(Person person)
      {
         if (_cachingProvider.Exists(person.Id.ToString()))
            return BadRequest("person with the same key already exists ...");

         var personJson = JsonConvert.SerializeObject(person);
         _cachingProvider.Set(person.Id.ToString(), personJson, TimeSpan.FromDays(1));

         Debug.WriteLine($"person added: {person}");

         return Ok(person);
      }

      [HttpDelete("{id}")]
      public IActionResult DeletePerson(int id)
      {
         if (!_cachingProvider.Exists(id.ToString()))
            return BadRequest($"the person with id {id} does not exist ...");

         _cachingProvider.Remove(id.ToString());

         Debug.WriteLine($"person with id {id} deleted");

         return Ok();
      }

      [HttpPut]
      public IActionResult UpdatePerson(Person person)
      {
         if (!_cachingProvider.Exists(person.Id.ToString()))
            return BadRequest($"the person with id {person.Id} does not exist ...");

         var personString = JsonConvert.SerializeObject(person);
         _cachingProvider.Set(person.Id.ToString(), personString, TimeSpan.FromDays(1));

         Debug.WriteLine($"person with id {person.Id} updated: {person}");

         return Ok(person);
      }
   }
}
