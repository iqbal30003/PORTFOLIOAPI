using Microsoft.AspNetCore.Mvc;
using PortfolioAPI.Models;
using System;
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
            new Product { Id = 1, Name = "Laptop", Price = 1200, Category = ProductCategory.Electronics },
            new Product { Id = 2, Name = "Phone", Price = 800, Category = ProductCategory.Electronics },
            new Product { Id = 3, Name = "Headphones", Price = 150, Category = ProductCategory.Audio }
        };

        [HttpGet]
        public ActionResult Get(
            string? search = null,
            string? sortBy = null,
            ProductCategory? category = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            int skip = 0,
            int take = 50)
        {
            var query = products
                .Where(p => !p.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.Name.ToLower().Contains(search.ToLower()));

            if (category.HasValue)
                query = query.Where(p => p.Category == category.Value);

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            query = sortBy switch
            {
                "price" => query.OrderBy(p => p.Price),
                "name" => query.OrderBy(p => p.Name),
                _ => query
            };

            var totalCount = query.Count();

            var data = query
                .Skip(skip)
                .Take(take)
                .ToList();

            // ✅ FIXED: safe header assignment (no ASP0019 warning)
            Response.Headers["X-Total-Count"] = totalCount.ToString();
            Response.Headers["X-Skip"] = skip.ToString();
            Response.Headers["X-Take"] = take.ToString();

            return Ok(new
            {
                totalCount,
                skip,
                take,
                data
            });
        }

        [HttpGet("{id}")]
        public ActionResult<Product> GetById(int id)
        {
            var product = products.FirstOrDefault(p => p.Id == id && !p.IsDeleted);
            if (product == null)
                return NotFound(new { message = "Product not found" });

            return Ok(product);
        }

        [HttpPost]
        public ActionResult<Product> Create(Product product)
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
        public ActionResult Update(int id, Product updated)
        {
            var product = products.FirstOrDefault(p => p.Id == id && !p.IsDeleted);
            if (product == null)
                return NotFound(new { message = "Product not found" });

            if (string.IsNullOrWhiteSpace(updated.Name))
                return BadRequest(new { message = "Name is required" });

            if (updated.Price <= 0)
                return BadRequest(new { message = "Price must be greater than zero" });

            product.Name = updated.Name;
            product.Price = updated.Price;
            product.Category = updated.Category;
            product.UpdatedAt = DateTime.UtcNow;

            return NoContent();
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var product = products.FirstOrDefault(p => p.Id == id && !p.IsDeleted);
            if (product == null)
                return NotFound(new { message = "Product not found" });

            product.IsDeleted = true;
            product.UpdatedAt = DateTime.UtcNow;

            return NoContent();
        }

        [HttpGet("stats")]
        public ActionResult GetStats()
        {
            var activeProducts = products.Where(p => !p.IsDeleted).ToList();

            return Ok(new
            {
                totalProducts = products.Count,
                activeProducts = activeProducts.Count,
                deletedProducts = products.Count(p => p.IsDeleted),
                minPrice = activeProducts.Any() ? activeProducts.Min(p => p.Price) : 0,
                maxPrice = activeProducts.Any() ? activeProducts.Max(p => p.Price) : 0,
                averagePrice = activeProducts.Any()
                    ? Math.Round(activeProducts.Average(p => p.Price), 2)
                    : 0
            });
        }
    }
}
