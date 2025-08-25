-- Script de Seed para o Banco de Dados do Ecommerce
-- Este script pode ser executado diretamente no SQL Server para popular o banco com dados iniciais

-- Inserir Categorias
INSERT INTO Categories (Id, Name, Description) VALUES
(NEWID(), 'Eletrônicos', 'Produtos eletrônicos e gadgets'),
(NEWID(), 'Roupas', 'Vestuário e acessórios'),
(NEWID(), 'Livros', 'Livros de diversos gêneros'),
(NEWID(), 'Casa e Jardim', 'Produtos para casa e jardim'),
(NEWID(), 'Esportes', 'Equipamentos e roupas esportivas');

-- Inserir Usuários (senhas hash: admin123, 123456)
INSERT INTO Users (Id, FirstName, LastName, Email, PasswordHash, Role) VALUES
(NEWID(), 'Admin', 'Sistema', 'admin@ecommerce.com', 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=', 'Admin'),
(NEWID(), 'João', 'Silva', 'joao.silva@email.com', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMQrcbDL8=', 'Customer'),
(NEWID(), 'Maria', 'Santos', 'maria.santos@email.com', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMQrcbDL8=', 'Customer'),
(NEWID(), 'Pedro', 'Oliveira', 'pedro.oliveira@email.com', 'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMQrcbDL8=', 'Customer');

-- Inserir Produtos (assumindo que as categorias foram criadas primeiro)
DECLARE @EletronicosId UNIQUEIDENTIFIER = (SELECT Id FROM Categories WHERE Name = 'Eletrônicos');
DECLARE @RoupasId UNIQUEIDENTIFIER = (SELECT Id FROM Categories WHERE Name = 'Roupas');
DECLARE @LivrosId UNIQUEIDENTIFIER = (SELECT Id FROM Categories WHERE Name = 'Livros');
DECLARE @CasaJardimId UNIQUEIDENTIFIER = (SELECT Id FROM Categories WHERE Name = 'Casa e Jardim');
DECLARE @EsportesId UNIQUEIDENTIFIER = (SELECT Id FROM Categories WHERE Name = 'Esportes');

INSERT INTO Products (Id, CategoryId, Name, Description, Price, StockQuantity) VALUES
-- Eletrônicos
(NEWID(), @EletronicosId, 'Smartphone Galaxy S23', 'Smartphone Samsung Galaxy S23 com 128GB', 3999.99, 50),
(NEWID(), @EletronicosId, 'Notebook Dell Inspiron', 'Notebook Dell Inspiron 15 polegadas, Intel i5, 8GB RAM', 3499.99, 25),
(NEWID(), @EletronicosId, 'Fones de Ouvido Bluetooth', 'Fones de ouvido sem fio com cancelamento de ruído', 299.99, 100),
(NEWID(), @EletronicosId, 'Smart TV 55"', 'Smart TV Samsung 55 polegadas 4K', 2499.99, 30),
(NEWID(), @EletronicosId, 'Tablet iPad', 'iPad Apple 10.2 polegadas 64GB', 2999.99, 20),

-- Roupas
(NEWID(), @RoupasId, 'Camiseta Básica', 'Camiseta 100% algodão, disponível em várias cores', 49.99, 200),
(NEWID(), @RoupasId, 'Calça Jeans', 'Calça jeans masculina, diversos tamanhos', 129.99, 75),
(NEWID(), @RoupasId, 'Tênis Esportivo', 'Tênis para corrida, confortável e durável', 199.99, 60),
(NEWID(), @RoupasId, 'Vestido Casual', 'Vestido feminino casual, algodão', 89.99, 45),
(NEWID(), @RoupasId, 'Jaqueta de Couro', 'Jaqueta de couro sintético, diversos tamanhos', 299.99, 25),

-- Livros
(NEWID(), @LivrosId, 'O Senhor dos Anéis', 'Trilogia completa de J.R.R. Tolkien', 89.99, 30),
(NEWID(), @LivrosId, 'Clean Code', 'Guia para desenvolvimento de software limpo', 79.99, 45),
(NEWID(), @LivrosId, '1984', 'Romance distópico de George Orwell', 39.99, 80),
(NEWID(), @LivrosId, 'Dom Casmurro', 'Clássico da literatura brasileira', 29.99, 60),
(NEWID(), @LivrosId, 'Harry Potter e a Pedra Filosofal', 'Primeiro livro da série Harry Potter', 49.99, 100),

-- Casa e Jardim
(NEWID(), @CasaJardimId, 'Vaso Decorativo', 'Vaso de cerâmica para plantas, 30cm', 69.99, 40),
(NEWID(), @CasaJardimId, 'Jogo de Panelas', 'Conjunto de 5 panelas antiaderentes', 299.99, 20),
(NEWID(), @CasaJardimId, 'Luminária de Mesa', 'Luminária LED com regulagem de intensidade', 159.99, 35),
(NEWID(), @CasaJardimId, 'Sofá 3 Lugares', 'Sofá confortável para sala de estar', 1299.99, 10),
(NEWID(), @CasaJardimId, 'Mesa de Jantar', 'Mesa de jantar para 6 pessoas', 899.99, 15),

-- Esportes
(NEWID(), @EsportesId, 'Bola de Futebol', 'Bola oficial tamanho 5, material sintético', 89.99, 60),
(NEWID(), @EsportesId, 'Esteira Elétrica', 'Esteira para exercícios em casa, 12km/h', 1999.99, 10),
(NEWID(), @EsportesId, 'Bicicleta Ergométrica', 'Bicicleta para exercícios indoor', 899.99, 15),
(NEWID(), @EsportesId, 'Prancha de Surf', 'Prancha de surf profissional', 599.99, 8),
(NEWID(), @EsportesId, 'Raquete de Tênis', 'Raquete de tênis profissional', 299.99, 25);

-- Inserir Endereços para usuários clientes
DECLARE @JoaoId UNIQUEIDENTIFIER = (SELECT Id FROM Users WHERE Email = 'joao.silva@email.com');
DECLARE @MariaId UNIQUEIDENTIFIER = (SELECT Id FROM Users WHERE Email = 'maria.santos@email.com');
DECLARE @PedroId UNIQUEIDENTIFIER = (SELECT Id FROM Users WHERE Email = 'pedro.oliveira@email.com');

INSERT INTO Addresses (Id, UserId, Street, Number, Complement, Neighborhood, City, State, ZipCode, IsDefault) VALUES
-- Endereços do João
(NEWID(), @JoaoId, 'Rua das Flores', '123', 'Apto 101', 'Centro', 'São Paulo', 'SP', '01234-567', 1),
(NEWID(), @JoaoId, 'Avenida Paulista', '1000', 'Sala 200', 'Bela Vista', 'São Paulo', 'SP', '01310-100', 0),

-- Endereços da Maria
(NEWID(), @MariaId, 'Rua Augusta', '500', 'Casa 15', 'Consolação', 'São Paulo', 'SP', '01205-000', 1),
(NEWID(), @MariaId, 'Rua Oscar Freire', '200', 'Loja 10', 'Jardins', 'São Paulo', 'SP', '01426-000', 0),

-- Endereços do Pedro
(NEWID(), @PedroId, 'Rua 25 de Março', '100', 'Galeria Central', 'Sé', 'São Paulo', 'SP', '01021-000', 1),
(NEWID(), @PedroId, 'Rua Vergueiro', '300', 'Edifício Comercial', 'Liberdade', 'São Paulo', 'SP', '01504-000', 0);

PRINT 'Seed do banco de dados concluído com sucesso!';
PRINT 'Dados inseridos:';
PRINT '- 5 Categorias';
PRINT '- 4 Usuários (1 Admin, 3 Customers)';
PRINT '- 25 Produtos (5 por categoria)';
PRINT '- 6 Endereços (2 por usuário customer)';




