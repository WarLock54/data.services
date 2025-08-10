using Business.IRepository;
using Business.IServices;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Service
{
    public partial class ProductService
    : IProductService
    {
        private readonly IProductRepository _productRepository; // Repository instance for database operations

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository; // Injecting the repository via constructor
        }

        // Retrieves all products, converts them to DTOs, and returns the list
        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetAllAsync(); // Fetch all products from repository

            // Convert each product entity into a Product and return the list
            return products.Select(p => new Product
            {
                Id = p.Id,
                Header = p.Header,
                Price = p.Price
            });
        }

        // Retrieves a product by ID and converts it to a DTO
        public async Task<Product> GetProductByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id); // Fetch product by ID

            // If the product is not found, throw an exception
            if (product == null)
                throw new KeyNotFoundException("Product not found");

            // Convert entity to DTO and return it
            return new Product
            {
                Id = product.Id,
                Header = product.Header,
                Price = product.Price
            };
        }

        // Adds a new product using a request DTO
        public async Task AddProductAsync(Product productDto)
        {
            // Convert DTO to entity
            var product = new Product
            {
                Header = productDto.Header,
                Price = productDto.Price
            };

            // Add the new product to the database
            await _productRepository.AddAsync(product);
        }

        // Updates an existing product with new data
        public async Task UpdateProductAsync(int id, Product productDto)
        {
            var product = await _productRepository.GetByIdAsync(id); // Fetch the product by ID

            // If the product does not exist, throw an exception
            if (product == null)
                throw new KeyNotFoundException("Product not found");

            // Update product fields with new values from DTO
            product.Header = productDto.Header;
            product.Price = productDto.Price;

            // Save the updated product in the database
            await _productRepository.UpdateAsync(product);
        }

        // Deletes a product by ID
        public async Task DeleteProductAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id); // Fetch the product by ID

            // If the product does not exist, throw an exception
            if (product == null)
                throw new KeyNotFoundException("Product not found");

            // Delete the product from the database
            await _productRepository.DeleteAsync(id);
        }
    }
}
