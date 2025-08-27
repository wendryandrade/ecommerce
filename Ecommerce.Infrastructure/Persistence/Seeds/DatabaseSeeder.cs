using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace Ecommerce.Infrastructure.Persistence.Seeds
{
    public class DatabaseSeeder
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DatabaseSeeder> _logger;

        public DatabaseSeeder(AppDbContext context, ILogger<DatabaseSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando seed do banco de dados...");

                // Seed por tabela para evitar pular etapas quando já existir parte dos dados
                if (!await _context.Categories.AnyAsync())
                {
                    await SeedCategoriesAsync();
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Categorias populadas.");
                }
                else
                {
                    _logger.LogInformation("Categorias já existem. Pulando criação.");
                }

                if (!await _context.Products.AnyAsync())
                {
                    await SeedProductsAsync();
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Produtos populados.");
                }
                else
                {
                    _logger.LogInformation("Produtos já existem. Pulando criação.");
                }

                if (!await _context.Users.AnyAsync())
                {
                    await SeedUsersAsync();
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Usuários populados.");
                }
                else
                {
                    _logger.LogInformation("Usuários já existem. Pulando criação.");
                }

                if (!await _context.Addresses.AnyAsync())
                {
                    await SeedAddressesAsync();
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Endereços populados.");
                }
                else
                {
                    _logger.LogInformation("Endereços já existem. Pulando criação.");
                }

                if (!await _context.Carts.AnyAsync())
                {
                    _logger.LogInformation("Iniciando seed de carrinhos...");
                    await SeedCartsAsync();
                    await _context.SaveChangesAsync();
                }
                else
                {
                    _logger.LogInformation("Carrinhos já existem. Pulando criação.");
                }

                if (!await _context.Orders.AnyAsync())
                {
                    _logger.LogInformation("Iniciando seed de pedidos...");
                    await SeedOrdersAsync();
                    await _context.SaveChangesAsync();
                }
                else
                {
                    _logger.LogInformation("Pedidos já existem. Pulando criação.");
                }

                _logger.LogInformation("Seed do banco de dados concluído com sucesso!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante o seed do banco de dados");
                throw;
            }
        }

        private async Task SeedCategoriesAsync()
        {
            var categories = new List<Category>
            {
                new Category
                {
                    Id = new Guid("11111111-1111-1111-1111-111111111111"),
                    Name = "Eletrônicos",
                    Description = "Produtos eletrônicos e gadgets"
                },
                new Category
                {
                    Id = new Guid("22222222-2222-2222-2222-222222222222"),
                    Name = "Roupas",
                    Description = "Vestuário e acessórios"
                },
                new Category
                {
                    Id = new Guid("33333333-3333-3333-3333-333333333333"),
                    Name = "Livros",
                    Description = "Livros de diversos gêneros"
                },
                new Category
                {
                    Id = new Guid("44444444-4444-4444-4444-444444444444"),
                    Name = "Casa e Jardim",
                    Description = "Produtos para casa e jardim"
                },
                new Category
                {
                    Id = new Guid("55555555-5555-5555-5555-555555555555"),
                    Name = "Esportes",
                    Description = "Equipamentos e roupas esportivas"
                }
            };

            await _context.Categories.AddRangeAsync(categories);
            _logger.LogInformation("Categorias seedadas: {Count}", categories.Count);
        }

        private async Task SeedProductsAsync()
        {
            var categories = await _context.Categories.ToListAsync();
            
            var products = new List<Product>();

            // Eletrônicos
            var eletronicos = categories.First(c => c.Name == "Eletrônicos");
            products.AddRange(new[]
            {
                new Product
                {
                    Id = new Guid("11111111-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
                    CategoryId = eletronicos.Id,
                    Name = "Smartphone Galaxy S23",
                    Description = "Smartphone Samsung Galaxy S23 com 128GB",
                    Price = 3999.99m,
                    StockQuantity = 50
                },
                new Product
                {
                    Id = new Guid("22222222-BBBB-BBBB-BBBB-BBBBBBBBBBBB"),
                    CategoryId = eletronicos.Id,
                    Name = "Notebook Dell Inspiron",
                    Description = "Notebook Dell Inspiron 15 polegadas, Intel i5, 8GB RAM",
                    Price = 3499.99m,
                    StockQuantity = 25
                },
                new Product
                {
                    Id = new Guid("33333333-CCCC-CCCC-CCCC-CCCCCCCCCCCC"),
                    CategoryId = eletronicos.Id,
                    Name = "Fones de Ouvido Bluetooth",
                    Description = "Fones de ouvido sem fio com cancelamento de ruído",
                    Price = 299.99m,
                    StockQuantity = 100
                }
            });

            // Roupas
            var roupas = categories.First(c => c.Name == "Roupas");
            products.AddRange(new[]
            {
                new Product
                {
                    Id = new Guid("44444444-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
                    CategoryId = roupas.Id,
                    Name = "Camiseta Básica",
                    Description = "Camiseta 100% algodão, disponível em várias cores",
                    Price = 49.99m,
                    StockQuantity = 200
                },
                new Product
                {
                    Id = new Guid("55555555-BBBB-BBBB-BBBB-BBBBBBBBBBBB"),
                    CategoryId = roupas.Id,
                    Name = "Calça Jeans",
                    Description = "Calça jeans masculina, diversos tamanhos",
                    Price = 129.99m,
                    StockQuantity = 75
                },
                new Product
                {
                    Id = new Guid("66666666-CCCC-CCCC-CCCC-CCCCCCCCCCCC"),
                    CategoryId = roupas.Id,
                    Name = "Tênis Esportivo",
                    Description = "Tênis para corrida, confortável e durável",
                    Price = 199.99m,
                    StockQuantity = 60
                }
            });

            // Livros
            var livros = categories.First(c => c.Name == "Livros");
            products.AddRange(new[]
            {
                new Product
                {
                    Id = new Guid("77777777-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
                    CategoryId = livros.Id,
                    Name = "O Senhor dos Anéis",
                    Description = "Trilogia completa de J.R.R. Tolkien",
                    Price = 89.99m,
                    StockQuantity = 30
                },
                new Product
                {
                    Id = new Guid("88888888-BBBB-BBBB-BBBB-BBBBBBBBBBBB"),
                    CategoryId = livros.Id,
                    Name = "Clean Code",
                    Description = "Guia para desenvolvimento de software limpo",
                    Price = 79.99m,
                    StockQuantity = 45
                },
                new Product
                {
                    Id = new Guid("99999999-CCCC-CCCC-CCCC-CCCCCCCCCCCC"),
                    CategoryId = livros.Id,
                    Name = "1984",
                    Description = "Romance distópico de George Orwell",
                    Price = 39.99m,
                    StockQuantity = 80
                }
            });

            // Casa e Jardim
            var casaJardim = categories.First(c => c.Name == "Casa e Jardim");
            products.AddRange(new[]
            {
                new Product
                {
                    Id = new Guid("AAAAAAAA-1111-1111-1111-111111111111"),
                    CategoryId = casaJardim.Id,
                    Name = "Vaso Decorativo",
                    Description = "Vaso de cerâmica para plantas, 30cm",
                    Price = 69.99m,
                    StockQuantity = 40
                },
                new Product
                {
                    Id = new Guid("BBBBBBBB-2222-2222-2222-222222222222"),
                    CategoryId = casaJardim.Id,
                    Name = "Jogo de Panelas",
                    Description = "Conjunto de 5 panelas antiaderentes",
                    Price = 299.99m,
                    StockQuantity = 20
                },
                new Product
                {
                    Id = new Guid("CCCCCCCC-3333-3333-3333-333333333333"),
                    CategoryId = casaJardim.Id,
                    Name = "Luminária de Mesa",
                    Description = "Luminária LED com regulagem de intensidade",
                    Price = 159.99m,
                    StockQuantity = 35
                }
            });

            // Esportes
            var esportes = categories.First(c => c.Name == "Esportes");
            products.AddRange(new[]
            {
                new Product
                {
                    Id = new Guid("DDDDDDDD-4444-4444-4444-444444444444"),
                    CategoryId = esportes.Id,
                    Name = "Bola de Futebol",
                    Description = "Bola oficial tamanho 5, material sintético",
                    Price = 89.99m,
                    StockQuantity = 60
                },
                new Product
                {
                    Id = new Guid("EEEEEEEE-5555-5555-5555-555555555555"),
                    CategoryId = esportes.Id,
                    Name = "Esteira Elétrica",
                    Description = "Esteira para exercícios em casa, 12km/h",
                    Price = 1999.99m,
                    StockQuantity = 10
                },
                new Product
                {
                    Id = new Guid("FFFFFFFF-6666-6666-6666-666666666666"),
                    CategoryId = esportes.Id,
                    Name = "Bicicleta Ergométrica",
                    Description = "Bicicleta para exercícios indoor",
                    Price = 899.99m,
                    StockQuantity = 15
                }
            });

            await _context.Products.AddRangeAsync(products);
            _logger.LogInformation("Produtos seedados: {Count}", products.Count);
        }

        private async Task SeedUsersAsync()
        {
            var users = new List<User>
            {
                new User
                {
                    Id = new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
                    FirstName = "Admin",
                    LastName = "Sistema",
                    Email = "admin@ecommerce.com",
                    PasswordHash = HashPassword("admin123"),
                    Role = "Admin"
                },
                new User
                {
                    Id = new Guid("BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB"),
                    FirstName = "João",
                    LastName = "Silva",
                    Email = "joao.silva@email.com",
                    PasswordHash = HashPassword("123456"),
                    Role = "Customer"
                },
                new User
                {
                    Id = new Guid("CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC"),
                    FirstName = "Ana",
                    LastName = "Santos",
                    Email = "ana.santos@email.com",
                    PasswordHash = HashPassword("123456"),
                    Role = "Customer"
                },
                new User
                {
                    Id = new Guid("DDDDDDDD-DDDD-DDDD-DDDD-DDDDDDDDDDDD"),
                    FirstName = "Pedro",
                    LastName = "Oliveira",
                    Email = "pedro.oliveira@email.com",
                    PasswordHash = HashPassword("123456"),
                    Role = "Customer"
                }
            };

            await _context.Users.AddRangeAsync(users);
            _logger.LogInformation("Usuários seedados: {Count}", users.Count);
        }

        private async Task SeedAddressesAsync()
        {
            var users = await _context.Users.Where(u => u.Role == "Customer").ToListAsync();
            
            var addresses = new List<Address>();

            // Criar endereços para cada usuário customer
            var userIds = users.Select(u => u.Id).ToList();
            foreach (var userId in userIds)
            {
                // Endereço principal do usuário
                addresses.Add(new Address
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Street = $"Rua das Flores, {GetSecureRandomInt(100, 999)} - Apto {GetSecureRandomInt(1, 50)}, Centro",
                    City = "São Paulo",
                    State = "SP",
                    PostalCode = $"01234-{GetSecureRandomInt(100, 999)}"
                });

                // Endereço secundário do usuário (opcional)
                if (GetSecureRandomInt(0, 2) == 1) // 50% de chance
                {
                    addresses.Add(new Address
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        Street = $"Avenida Paulista, {GetSecureRandomInt(1000, 2000)} - Sala {GetSecureRandomInt(100, 300)}, Bela Vista",
                        City = "São Paulo",
                        State = "SP",
                        PostalCode = $"01310-{GetSecureRandomInt(100, 999)}"
                    });
                }
            }

            await _context.Addresses.AddRangeAsync(addresses);
            _logger.LogInformation("Endereços seedados: {Count} para {UserCount} usuários", addresses.Count, users.Count);
        }

        private async Task SeedCartsAsync()
        {
            // Incluir admin e clientes
            var users = await _context.Users.ToListAsync();
            var products = await _context.Products.Take(3).ToListAsync(); // Pegar 3 produtos para teste
            
            _logger.LogInformation("Criando carrinhos para {UserCount} usuários (inclui Admin) com {ProductCount} produtos", users.Count, products.Count);
            
            var carts = new List<Cart>();
            var cartItems = new List<CartItem>();
            foreach (var user in users)
            {
                var cart = new Cart
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id
                };
                carts.Add(cart);

                // Adicionar 1-3 itens aleatórios ao carrinho (garante pelo menos 1)
                var itemCount = GetSecureRandomInt(1, 4);
                
                for (int i = 0; i < itemCount; i++)
                {
                    var product = products[i % products.Count];
                    cartItems.Add(new CartItem
                    {
                        Id = Guid.NewGuid(),
                        CartId = cart.Id,
                        ProductId = product.Id,
                        Quantity = GetSecureRandomInt(1, 4),
                        UnitPrice = product.Price
                    });
                }
            }

            await _context.Carts.AddRangeAsync(carts);
            await _context.CartItems.AddRangeAsync(cartItems);
            _logger.LogInformation("Carrinhos seedados: {CartsCount}, Itens: {ItemsCount}", carts.Count, cartItems.Count);
        }

        private async Task SeedOrdersAsync()
        {
            var users = await _context.Users.Where(u => u.Role == "Customer").ToListAsync();
            var products = await _context.Products.Take(2).ToListAsync();
            
            _logger.LogInformation("Criando pedidos para {UserCount} usuários com {ProductCount} produtos", 
                users.Count, products.Count);
            
            // Evitar N+1 consultas buscando endereços por usuário dentro do loop
            var userIds = users.Select(u => u.Id).ToList();
            var addressesByUser = await _context.Addresses
                .Where(a => a.UserId.HasValue && userIds.Contains(a.UserId.Value))
                .GroupBy(a => a.UserId!.Value)
                .ToDictionaryAsync(g => g.Key, g => g.ToList());
            
            var orders = new List<Order>();
            var orderItems = new List<OrderItem>();
            var payments = new List<Payment>();
            var shippings = new List<Shipping>();
            var orderIndex = 0;
            foreach (var userId in userIds)
            {
                // Buscar endereços do usuário do dicionário pré-carregado
                if (!addressesByUser.TryGetValue(userId, out var userAddresses) || userAddresses.Count == 0)
                {
                    _logger.LogWarning("Usuário {UserId} não tem endereços cadastrados, pulando criação de pedido", userId);
                    continue;
                }
                
                var userAddress = userAddresses[GetSecureRandomInt(0, userAddresses.Count)]; // Escolher endereço do usuário
                
                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ShippingAddressId = userAddress.Id,
                    OrderDate = DateTime.UtcNow.AddDays(-GetSecureRandomInt(1, 30)), // Pedido de 1-30 dias atrás
                    TotalAmount = 0, // Será calculado
                    Status = OrderStatus.Pending
                };
                orders.Add(order);

                // Adicionar 1-2 itens ao pedido
                var itemCount = GetSecureRandomInt(1, 3);
                decimal totalAmount = 0;
                
                for (int i = 0; i < itemCount; i++)
                {
                    var product = products[i % products.Count];
                    var quantity = GetSecureRandomInt(1, 3);
                    var unitPrice = product.Price;
                    totalAmount += unitPrice * quantity;

                    orderItems.Add(new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        OrderId = order.Id,
                        ProductId = product.Id,
                        Quantity = quantity,
                        UnitPrice = unitPrice
                    });
                }

                // Atualizar total do pedido
                order.TotalAmount = totalAmount;

                // Adicionar pagamento
                payments.Add(new Payment
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    PaymentMethod = PaymentMethod.CreditCard,
                    TransactionId = $"TXN_FIXED_{orderIndex:D08}",
                    Amount = totalAmount,
                    Status = PaymentStatus.Paid
                });

                // Adicionar frete
                shippings.Add(new Shipping
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    Carrier = "Correios",
                    TrackingCode = $"BR{DateTime.Now:yyyyMMdd}{GetSecureRandomInt(100000, 999999)}",
                    ShippingCost = 15.99m,
                    EstimatedDeliveryDate = DateTime.UtcNow.AddDays(5)
                });
                
                orderIndex++;
            }

            await _context.Orders.AddRangeAsync(orders);
            await _context.OrderItems.AddRangeAsync(orderItems);
            await _context.Payments.AddRangeAsync(payments);
            await _context.Shippings.AddRangeAsync(shippings);
            
            _logger.LogInformation("Pedidos seedados: {OrdersCount}, Itens: {ItemsCount}, Pagamentos: {PaymentsCount}, Fretes: {ShippingsCount}", 
                orders.Count, orderItems.Count, payments.Count, shippings.Count);
        }

        private static string HashPassword(string password)
        {
            // Usar exatamente o mesmo método de hash que o LoginCommandHandler
            byte[] salt = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(20);

            byte[] hashBytes = new byte[52];
            Array.Copy(salt, 0, hashBytes, 0, 32);
            Array.Copy(hash, 0, hashBytes, 32, 20);

            return Convert.ToBase64String(hashBytes);
        }

        // Método auxiliar para gerar números aleatórios criptograficamente seguros
        private static int GetSecureRandomInt(int minValue, int maxValue)
        {
            var bytes = new byte[4];
            RandomNumberGenerator.Fill(bytes);
            var value = BitConverter.ToUInt32(bytes, 0);
            return (int)(value % (maxValue - minValue)) + minValue;
        }
    }
}
