﻿using System;
using Dapper.Database.Extensions;
using Xunit;

using FactAttribute = Dapper.Tests.Database.SkippableFactAttribute;

namespace Dapper.Tests.Database
{
    public abstract partial class TestSuite
    {
        [Fact]
        [Trait("Category", "Delete")]
        public void DeleteIdentityEntity()
        {
            using (var db = GetSqlDatabase())
            {
                var p = new PersonIdentity { FirstName = "Alice", LastName = "Jones" };
                Assert.True(db.Insert(p));
                Assert.True(p.IdentityId > 0);

                Assert.True(db.Delete(p));

                var gp = db.Get<PersonIdentity>(p.IdentityId);

                Assert.Null(gp);
            }
        }

        [Fact]
        [Trait("Category", "Delete")]
        public void DeleteUniqueIdentifierEntity()
        {
            using (var db = GetSqlDatabase())
            {
                var p = new PersonUniqueIdentifier { GuidId = Guid.NewGuid(), FirstName = "Alice", LastName = "Jones" };
                Assert.True(db.Insert(p));
                Assert.True(db.Delete(p));

                var gp = db.Get(p);
                Assert.Null(gp);
            }
        }

        [Fact]
        [Trait("Category", "Delete")]
        public void DeleteUniqueIdentifierWithAliases()
        {
            using (var db = GetSqlDatabase())
            {
                var p = new PersonUniqueIdentifierWithAliases { GuidId = Guid.NewGuid(), First = "Alice", Last = "Jones" };
                Assert.True(db.Insert(p));
                Assert.True(db.Delete(p));

                var gp = db.Get(p);
                Assert.Null(gp);
            }
        }

        [Fact]
        [Trait("Category", "Delete")]
        public void DeletePersonCompositeKeyWithAliases_DoesNotDeleteOtherPersons()
        {
            using (var db = GetSqlDatabase())
            {
                var sharedGuidId = Guid.NewGuid();
                var pOther = new PersonCompositeKeyWithAliases { GuidId = sharedGuidId, StringId = "Other P", First = "Other Alice", Last = "Other Jones" };
                var p = new PersonCompositeKeyWithAliases { GuidId = sharedGuidId, StringId = "P", First = "Alice", Last = "Jones" };
                Assert.True(db.Insert(pOther));
                Assert.True(db.Insert(p));
                Assert.True(db.Delete(p));

                var gp = db.Get(p);
                var gpOther = db.Get(pOther);
                Assert.Null(gp);
                Assert.NotNull(gpOther);
            }
        }

        [Fact]
        [Trait("Category", "Delete")]
        public void DeleteIdentity()
        {
            using (var db = GetSqlDatabase())
            {
                var p = new PersonIdentity { FirstName = "Alice", LastName = "Jones" };
                var pOther = new PersonIdentity { FirstName = "Other Alice", LastName = "Other Jones" };
                Assert.True(db.Insert(p));
                Assert.True(db.Insert(pOther));
                Assert.True(p.IdentityId > 0);

                Assert.True(db.DeleteByPrimaryKey<PersonIdentity>(p.IdentityId));

                var gp = db.Get(p);
                var gpOther = db.Get(pOther);
                Assert.Null(gp);
                Assert.NotNull(gpOther);
            }
        }

        [Fact]
        [Trait("Category", "Delete")]
        public void DeleteUniqueIdentifier()
        {
            using (var db = GetSqlDatabase())
            {
                var pOther = new PersonUniqueIdentifier { GuidId = Guid.NewGuid(), FirstName = "Other Alice", LastName = "Other Jones" };
                var p = new PersonUniqueIdentifier { GuidId = Guid.NewGuid(), FirstName = "Alice", LastName = "Jones" };
                Assert.True(db.Insert(p));
                Assert.True(db.Insert(pOther));
                Assert.True(db.DeleteByPrimaryKey<PersonUniqueIdentifier>(p.GuidId));

                var gp = db.Get(p);
                var gpOther = db.Get(pOther);
                Assert.Null(gp);
                Assert.NotNull(gpOther);
            }
        }

        [Fact]
        [Trait("Category", "Delete")]
        public void DeletePersonCompositeKey()
        {
            using (var db = GetSqlDatabase())
            {
                var p = new PersonCompositeKey { GuidId = Guid.NewGuid(), StringId = "test", FirstName = "Alice", LastName = "Jones" };
                Assert.True(db.Insert(p));

                Assert.True(db.DeleteByWhereClause<PersonCompositeKey>("where GuidId = @GuidId and StringId = @StringId", p));

                var gp = db.Get(p);
                Assert.Null(gp);
            }
        }


        [Fact]
        [Trait("Category", "Delete")]
        public void DeleteAll()
        {
            using (var db = GetSqlDatabase())
            {
                var p = new PersonCompositeKey { GuidId = Guid.NewGuid(), StringId = "test", FirstName = "Alice", LastName = "Jones" };
                var pOther = new PersonCompositeKey { GuidId = Guid.NewGuid(), StringId = "test2", FirstName = "Alice", LastName = "Jones" };
                Assert.True(db.Insert(p));
                Assert.True(db.Insert(pOther));

                Assert.True(db.DeleteAll<PersonCompositeKey>());

                Assert.Equal(0, db.Count<PersonCompositeKey>());
            }
        }

        [Fact]
        [Trait("Category", "Delete")]
        public void DeleteWhereClause()
        {
            using (var db = GetSqlDatabase())
            {
                var p = new PersonIdentity { FirstName = "Delete", LastName = "Me" };

                for (var i = 0; i < 10; i++)
                {
                    Assert.True(db.Insert(p));
                }

                Assert.Equal(10, db.Count<PersonIdentity>("where FirstName = 'Delete'"));

                Assert.True(db.DeleteByWhereClause<PersonIdentity>("where FirstName = 'Delete'"));

                Assert.Equal(0, db.Count<PersonIdentity>("where FirstName = 'Delete'"));
            }
        }

    }
}
