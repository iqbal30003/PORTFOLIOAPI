using Microsoft.AspNetCore.Mvc;
using PortfolioAPI.Models;
using System.Collections.Generic;
using System.Linq;

namespace PortfolioAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private static List<Product> products = new()
        {
            new Product { Id = 1, Name = "Laptop", Price = 1200 },
            new Product { Id = 2, Name = "Phone", Price = 800 }
        };

        [HttpGet]
        public ActionResult<IEnumerable<Product>> Get(
            string? search = null,
            string? sortBy = null,
            int skip = 0,
            int take = 50)
        {
            var query = products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.Name.ToLower().Contains(search.ToLower()));

            query = sortBy switch
            {
                "price" => query.OrderBy(p => p.Price),
                "name" => query.OrderBy(p => p.Name),
                _ => query
            };

            return Ok(query.Skip(skip).Take(take).ToList());
        }

        [HttpGet("{id}")]
        public ActionResult<Product> GetById(int id)
        {
            var product = products.FirstOrDefault(p => p.Id == id);
            if (product == null)
                return NotFound(new { message = "Product not found" });

            return Ok(product);
        }

        [HttpPost]
        public ActionResult<Product> Create([FromBody] Product product)
        {
            if (string.IsNullOrWhiteSpace(product.Name))
                return BadRequest(new { message = "Name is required" });

            if (product.Price <= 0)
                return BadRequest(new { message = "Price must be greater than zero" });

            product.Id = products.Any() ? products.Max(p => p.Id) + 1 : 1;
            product.CreatedAt = DateTime.UtcNow;

            products.Add(product);

            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody] Product updated)
        {
            var product = products.FirstOrDefault(p => p.Id == id);
            if (product == null)
                return NotFound(new { message = "Product not found" });

            if (string.IsNullOrWhiteSpace(updated.Name))
                return BadRequest(new { message = "Name is required" });

            if (updated.Price <= 0)
                return BadRequest(new { message = "Price must be greater than zero" });

            product.Name = updated.Name;
            product.Price = updated.Price;
            product.UpdatedAt = DateTime.UtcNow;

            return NoContent();
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var product = products.FirstOrDefault(p => p.Id == id);
            if (product == null)
                return NotFound(new { message = "Product not found" });

            products.Remove(product);
            return NoContent();
        }
    }
}
