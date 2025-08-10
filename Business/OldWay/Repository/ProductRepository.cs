using Business.IRepository;
using Microsoft.EntityFrameworkCore;
using Model;
using Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Repository
{
    public class ProductRepository :IProductRepository
    {
        private readonly Persistence.ApplicationDbContext _context; // Database context for interacting with the database

        public ProductRepository(Persistence.ApplicationDbContext context)
        {
            _context = context; // Injecting database context via constructor
        }

        // Retrieves all products from the database
        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            // Converts the Products table into a list and returns it asynchronously
            return await _context.Products.ToListAsync();
        }

        // Retrieves a product by its ID
        public async Task<Product> GetByIdAsync(int id)
        {
            // Uses FindAsync to search for a product by its primary key (ID)
            return await _context.Products.FindAsync(id);
        }

        // Adds a new product to the database
        public async Task AddAsync(Product product)
        {
            // Adds the product entity to the database context
            await _context.Products.AddAsync(product);

            // Saves the changes to the database asynchronously
            await _context.SaveChangesAsync();
        }

        // Updates an existing product in the database
        public async Task UpdateAsync(Product product)
        {
            // Marks the product entity as updated in the database context
            _context.Products.Update(product);

            // Saves the updated product data to the database asynchronously
            await _context.SaveChangesAsync();
        }

        // Deletes a product by its ID
        public async Task DeleteAsync(int id)
        {
            // Finds the product in the database using the provided ID
            var product = await _context.Products.FindAsync(id);

            // If the product exists, remove it from the database
            if (product != null)
            {
                _context.Products.Remove(product);

                // Saves the changes to the database asynchronously
                await _context.SaveChangesAsync();
            }
        }
    }
}
