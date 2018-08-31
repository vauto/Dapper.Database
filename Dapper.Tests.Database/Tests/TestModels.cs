﻿using System;
using Dapper.Database.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dapper.Tests.Database
{

    [Table("Person")]
    public class PersonIdentity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdentityId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    [Table("Person")]
    public class PersonUniqueIdentifier
    {
        [Key]
        public Guid GuidId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    [Table("Person")]
    public class PersonUniqueIdentifierWithAliases
    {
        [Key]
        public Guid GuidId { get; set; }
        [Column("FirstName")]
        public string First { get; set; }
        [Column("LastName")]
        public string Last { get; set; }
    }
    
    [Table("Person")]
    public class PersonCompositeKeyWithAliases
    {
        [Key]
        public Guid GuidId { get; set; }
        [Key]
        public string StringId { get; set; }
        [Column("FirstName")]
        public string First { get; set; }
        [Column("LastName")]
        public string Last { get; set; }
    }


    [Table("Person")]
    public class PersonCompositeKey
    {
        [Key]
        public Guid GuidId { get; set; }
        [Key]
        public string StringId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    [Table("Person")]
    public class PersonExcludedColumns
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdentityId { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string FullName { get; set; }
        [IgnoreSelect]
        public string Notes { get; set; }
        [IgnoreInsert]
        public DateTime? UpdatedOn { get; set; }
        [IgnoreUpdate]
        public DateTime? CreatedOn { get; set; }
        [Ignore]
        public string NoDbColumn { get; set; }

    }

}
