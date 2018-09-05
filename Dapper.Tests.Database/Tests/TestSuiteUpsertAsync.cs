﻿using System;
using System.Threading.Tasks;
using Dapper.Database.Extensions;
using Xunit;
using FactAttribute = Xunit.SkippableFactAttribute;


namespace Dapper.Tests.Database
{
    public abstract partial class TestSuite
    {

        [Fact]
        [Trait("Category", "UpsertAsync")]
        public async Task UpsertIdentityAsync()
        {
            using (var connection = GetSqlDatabase())
            {
                var p = new PersonIdentity { FirstName = "Alice", LastName = "Jones" };
                Assert.True(await connection.UpsertAsync(p));
                Assert.True(p.IdentityId > 0);

                p.FirstName = "Greg";
                p.LastName = "Smith";
                Assert.True(await connection.UpsertAsync(p));

                var gp = await connection.GetAsync<PersonIdentity>(p.IdentityId);

                Assert.Equal(p.IdentityId, gp.IdentityId);
                Assert.Equal(p.FirstName, gp.FirstName);
                Assert.Equal(p.LastName, gp.LastName);
            }
        }

        [Fact]
        [Trait("Category", "UpsertAsync")]
        public async Task UpsertUniqueIdentifierAsync()
        {
            using (var connection = GetSqlDatabase())
            {
                var p = new PersonUniqueIdentifier { GuidId = Guid.NewGuid(), FirstName = "Alice", LastName = "Jones" };
                Assert.True(await connection.UpsertAsync(p));

                p.FirstName = "Greg";
                p.LastName = "Smith";
                Assert.True(await connection.UpsertAsync(p));

                var gp = await connection.GetAsync<PersonUniqueIdentifier>(p.GuidId);

                Assert.Equal(p.FirstName, gp.FirstName);
                Assert.Equal(p.LastName, gp.LastName);
            }
        }

        [Fact]
        [Trait("Category", "UpsertAsync")]
        public async Task UpsertUniqueIdentifierWithAliasesAsync()
        {
            using (var connection = GetSqlDatabase())
            {
                var p = new PersonUniqueIdentifierWithAliases { GuidId = Guid.NewGuid(), First = "Alice", Last = "Jones" };
                Assert.True(await connection.UpsertAsync(p));

                p.First = "Greg";
                p.Last = "Smith";
                Assert.True(await connection.UpsertAsync(p));

                var gp = await connection.GetAsync<PersonUniqueIdentifierWithAliases>(p.GuidId);

                Assert.Equal(p.First, gp.First);
                Assert.Equal(p.Last, gp.Last);
            }
        }

        [Fact]
        [Trait("Category", "UpsertAsync")]
        public async Task UpsertPersonCompositeKeyAsync()
        {
            using (var connection = GetSqlDatabase())
            {
                var p = new PersonCompositeKey { GuidId = Guid.NewGuid(), StringId = "test", FirstName = "Alice", LastName = "Jones" };
                Assert.True(await connection.UpsertAsync(p));

                p.FirstName = "Greg";
                p.LastName = "Smith";
                Assert.True(await connection.UpsertAsync(p));

                var gp = await connection.GetAsync<PersonCompositeKey>($"where GuidId = {P}GuidId and StringId = {P}StringId", p);

                Assert.Equal(p.StringId, gp.StringId);
                Assert.Equal(p.FirstName, gp.FirstName);
                Assert.Equal(p.LastName, gp.LastName);
            }
        }

        [Fact]
        [Trait("Category", "UpsertAsync")]
        public async Task UpsertComputedAsync()
        {

            var dnow = DateTime.UtcNow;
            using (var connection = GetSqlDatabase())
            {
                var p = new PersonExcludedColumns { FirstName = "Alice", LastName = "Jones", Notes = "Hello", CreatedOn = dnow, UpdatedOn = dnow };
                Assert.True(await connection.UpsertAsync(p));

                if (p.FullName != null)
                {
                    Assert.Equal("Alice Jones", p.FullName);
                }

                p.FirstName = "Greg";
                p.LastName = "Smith";
                p.CreatedOn = DateTime.UtcNow;
                Assert.True(await connection.UpsertAsync(p));
                if (p.FullName != null)
                {
                    Assert.Equal("Greg Smith", p.FullName);
                }

                var gp = await connection.GetAsync<PersonExcludedColumns>(p.IdentityId);

                Assert.Equal(p.IdentityId, gp.IdentityId);
                Assert.Null(gp.Notes);
                Assert.InRange(gp.CreatedOn.Value, dnow.AddSeconds(-1), dnow.AddSeconds(1)); // to cover fractional seconds rounded up/down (amounts supported between databases vary, but should all be ±1 second at most. )
                Assert.InRange(gp.UpdatedOn.Value, dnow.AddMinutes(-1), dnow.AddMinutes(1)); // to cover clock skew, delay in DML, etc.
                Assert.Equal(p.FirstName, gp.FirstName);
                Assert.Equal(p.LastName, gp.LastName);
            }
        }


        [Fact]
        [Trait("Category", "UpsertAsync")]
        public async Task UpsertPartialAsync()
        {
            using (var connection = GetSqlDatabase())
            {
                var p = new PersonIdentity { FirstName = "Alice", LastName = "Jones" };
                Assert.True(await connection.UpsertAsync(p));
                Assert.True(p.IdentityId > 0);

                p.FirstName = "Greg";
                p.LastName = "Smith";
                Assert.True(await connection.UpsertAsync(p, new string[] { "LastName" }));

                var gp = await connection.GetAsync<PersonIdentity>(p.IdentityId);

                Assert.Equal(p.IdentityId, gp.IdentityId);
                Assert.Equal("Alice", gp.FirstName);
                Assert.Equal("Smith", gp.LastName);
            }
        }

        [Fact]
        [Trait("Category", "UpsertAsync")]
        public async Task UpsertPartialCallbacksAsync()
        {
            var dnow = DateTime.UtcNow;
            using (var connection = GetSqlDatabase())
            {
                var p = new PersonExcludedColumns { FirstName = "Alice", LastName = "Jones" };
                Assert.True(await connection.UpsertAsync(p, (i) => i.CreatedOn = dnow, (u) => u.UpdatedOn = dnow));
                Assert.True(p.IdentityId > 0);

                p.FirstName = "Greg";
                p.LastName = "Smith";
                p.CreatedOn = DateTime.UtcNow;
                Assert.True(await connection.UpsertAsync(p, new[] { "LastName", "CreatedOn", "UpdatedOn" }, (i) => i.CreatedOn = dnow, (u) => u.UpdatedOn = dnow));

                var gp = await connection.GetAsync<PersonExcludedColumns>(p.IdentityId);

                Assert.Equal(p.IdentityId, gp.IdentityId);
                Assert.Equal("Alice", gp.FirstName);
                Assert.Equal("Smith", gp.LastName);
                Assert.InRange(gp.CreatedOn.Value, dnow.AddSeconds(-1), dnow.AddSeconds(1)); // to cover fractional seconds rounded up/down (amounts supported between databases vary, but should all be ±1 second at most. )
                Assert.InRange(gp.UpdatedOn.Value, dnow.AddMinutes(-1), dnow.AddMinutes(1)); // to cover clock skew, delay in DML, etc.
            }
        }

    }
}
