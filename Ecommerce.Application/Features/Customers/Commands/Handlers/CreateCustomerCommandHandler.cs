﻿using Ecommerce.Application.Features.Customers.Commands;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using MediatR;
using System.Security.Cryptography;

namespace Ecommerce.Application.Features.Customers.Handlers
{
    public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Guid>
    {
        private readonly ICustomerRepository _customerRepository;

        public CreateCustomerCommandHandler(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<Guid> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            // 1. Verificar se o cliente já existe
            var existingCustomer = await _customerRepository.GetByEmailAsync(request.Email);
            if (existingCustomer != null)
            {
                throw new InvalidOperationException("Um cliente com este e-mail já existe.");
            }

            // 2. Hashear a senha (usando a biblioteca nativa do .NET)
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            var pbkdf2 = new Rfc2898DeriveBytes(request.Password, salt, 10000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(20);

            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);

            // Salva o hash combinado (salt + hash) como uma string Base64
            var passwordHash = Convert.ToBase64String(hashBytes);

            // 3. Criar a nova entidade de cliente
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = passwordHash
            };

            // 4. Salvar no banco
            await _customerRepository.AddAsync(customer, cancellationToken);

            return customer.Id;
        }
    }
}