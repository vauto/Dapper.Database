using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Dapper.Database.Attributes;
using Dapper.Database.Extensions;
using static Dapper.Database.Extensions.SqlMapperExtensions;

#if NETSTANDARD1_3
using DataException = System.InvalidOperationException;
#endif

namespace Dapper.Database
{

    /// <summary>
    /// 
    /// </summary>
    public class TableInfo
    {

        private readonly Lazy<IEnumerable<ColumnInfo>> _insertColumns;
        private readonly Lazy<VersionColumnInfo> _versionColumn;
        private readonly Lazy<IEnumerable<ColumnInfo>> _updateColumns;
        private readonly Lazy<IEnumerable<ColumnInfo>> _selectColumns;
        private readonly Lazy<IEnumerable<ColumnInfo>> _keyColumns;
        private readonly Lazy<IEnumerable<ColumnInfo>> _generatedColumns;
        private readonly Lazy<IEnumerable<PropertyInfo>> _propertyList;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="tablenameMapper"></param>
        public TableInfo(Type type, TableNameMapperDelegate tablenameMapper)
        {
            ClassType = type;

            if (tablenameMapper != null)
            {
                TableName = TableNameMapper(type);
            }
            else
            {
                var tableAttr = type
#if NETSTANDARD1_3
                .GetTypeInfo()
#endif
                .GetCustomAttributes(false).SingleOrDefault(attr => attr.GetType().Name == "TableAttribute") as dynamic;

                if (tableAttr != null)
                {
                    TableName = tableAttr.Name;
                    if (tableAttr.Schema != null)
                    {
                        SchemaName = tableAttr.Schema;
                    }
                }
                else
                {
                    TableName = type.Name + "s";
                    if (type.IsInterface() && TableName.StartsWith("I"))
                        TableName = TableName.Substring(1);
                }
            }

            ColumnInfos = type.GetProperties()
                .Where(pInfo => !pInfo.GetCustomAttributes(typeof(IgnoreAttribute), false).Any())
                .Select(pInfo => BuildColumnInfoFromAttributes(pInfo, type))
                .ToArray();

            if (!ColumnInfos.Any(k => k.IsKey))
            {
                var idProp = ColumnInfos.FirstOrDefault(p => string.Equals(p.PropertyName, "id", StringComparison.CurrentCultureIgnoreCase));

                if (idProp != null)
                {
                    idProp.IsKey = idProp.IsGenerated = idProp.IsIdentity = idProp.ExcludeOnInsert = idProp.ExcludeOnUpdate = true;
                }
            }

            _insertColumns = new Lazy<IEnumerable<ColumnInfo>>(() => ColumnInfos.Where(ci => !ci.ExcludeOnInsert), true);
            _updateColumns = new Lazy<IEnumerable<ColumnInfo>>(() => ColumnInfos.Where(ci => !ci.ExcludeOnUpdate && !ci.IsVersion), true);
            _selectColumns = new Lazy<IEnumerable<ColumnInfo>>(() => ColumnInfos.Where(ci => !ci.ExcludeOnSelect), true);
            _keyColumns = new Lazy<IEnumerable<ColumnInfo>>(() => ColumnInfos.Where(ci => ci.IsKey), true);
            _generatedColumns = new Lazy<IEnumerable<ColumnInfo>>(() => ColumnInfos.Where(ci => ci.IsGenerated), true);
            _propertyList = new Lazy<IEnumerable<PropertyInfo>>(() => ColumnInfos.Select(ci => ci.Property), true);
            _versionColumn = new Lazy<VersionColumnInfo>(() => (VersionColumnInfo)ColumnInfos.SingleOrDefault(ci => ci is VersionColumnInfo && ci.IsVersion));
        }

        protected virtual ColumnInfo BuildColumnInfoFromAttributes(PropertyInfo propertyInfo, Type classType)
        {
            var columnAtt = propertyInfo.GetCustomAttributes(false).SingleOrDefault(attr => attr.GetType().Name == "ColumnAttribute") as dynamic;
            var seqAtt = propertyInfo.GetCustomAttributes(false).SingleOrDefault(a => a is SequenceAttribute) as dynamic;

            var isVersionSpecified = propertyInfo.GetCustomAttributes(false).Any(a => a is ConcurrencyCheckAttribute);
            var ci = (isVersionSpecified) ? new VersionColumnInfo(propertyInfo) : new ColumnInfo(propertyInfo);

            ci.IsVersion = isVersionSpecified;
            ci.Property = propertyInfo;
            ci.ColumnName = columnAtt?.Name ?? propertyInfo.Name;
            ci.PropertyName = propertyInfo.Name;
            ci.IsKey = propertyInfo.GetCustomAttributes(false).Any(a => a is KeyAttribute);
            ci.IsIdentity = (propertyInfo.GetCustomAttributes(false).Any(a => a is DatabaseGeneratedAttribute g
                                                                   && g.DatabaseGeneratedOption ==
                                                                   DatabaseGeneratedOption.Identity))
                            || seqAtt != null;
            ci.IsGenerated = (propertyInfo.GetCustomAttributes(false).Any(a => a is DatabaseGeneratedAttribute g
                                                                    && g.DatabaseGeneratedOption !=
                                                                    DatabaseGeneratedOption.None))
                             || seqAtt != null;
            ci.ExcludeOnSelect = propertyInfo.GetCustomAttributes(false).Any(a => a is IgnoreSelectAttribute);
            ci.SequenceName = seqAtt?.Name;

            ci.ExcludeOnInsert = (ci.IsGenerated && seqAtt == null)
                || propertyInfo.GetCustomAttributes(false).Any(a => a is IgnoreInsertAttribute)
                || propertyInfo.GetCustomAttributes(false).Any(a => a is ReadOnlyAttribute);

            ci.ExcludeOnUpdate = ci.IsGenerated
                || propertyInfo.GetCustomAttributes(false).Any(a => a is IgnoreUpdateAttribute)
                || propertyInfo.GetCustomAttributes(false).Any(a => a is ReadOnlyAttribute);
            if (ci.IsGenerated)
            {
                var parameter = Expression.Parameter(classType);
                var property = Expression.Property(parameter, ci.Property);
                var conversion = Expression.Convert(property, typeof(object));
                var lambda = Expression.Lambda(conversion, parameter);
                ci.Output = lambda;
            }

            ThrowIfColumnInfoIsInvalid(classType, ci);

            return ci;
        }

        private static void ThrowIfColumnInfoIsInvalid(Type type, ColumnInfo ci)
        {
            if (ci.IsVersion) return;

            var isVersionValid = !ci.IsKey && !ci.IsGenerated && !ci.IsIdentity && !ci.ExcludeOnUpdate &&
                                 !ci.ExcludeOnInsert && !ci.ExcludeOnSelect;
            if (!isVersionValid)
            {
                throw new ValidationException(
                    $"Property: {ci.PropertyName} on {type.Name} cannot have a ConcurrencyCheckAttribute specified along with any of the following notations: " +
                    $"Key, Identity, Generated, IgnoreUpdate, or ReadOnly.");
            }

            //Additional column validation checks here, if needed.
        }

        /// <summary>
        /// 
        /// </summary>
        public Type ClassType { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string TableName { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string SchemaName { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        private IEnumerable<ColumnInfo> ColumnInfos { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ColumnInfo GetSingleKey()
        {
            var keys = _keyColumns.Value;
            if (keys.Count() != 1)
                throw new DataException("<T> only supports an entity with a single [Key]");

            return keys.SingleOrDefault();
        }

        /// <summary>
        /// Gets a list of all key columns defined on the table
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ColumnInfo> GetCompositeKeys()
        {
            var keys = _keyColumns.Value;
            if (keys.Count() == 0)
                throw new DataException("<T> does not have a [Key]");
            return keys;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ColumnInfo> InsertColumns => _insertColumns.Value;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public VersionColumnInfo VersionColumn => _versionColumn.Value;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ColumnInfo> UpdateColumns => _updateColumns.Value;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ColumnInfo> SelectColumns => _selectColumns.Value;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ColumnInfo> KeyColumns => _keyColumns.Value;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ColumnInfo> GeneratedColumns => _generatedColumns.Value;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PropertyInfo> PropertyList => _propertyList.Value;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool HasSequenceName => ColumnInfos.Any(ci => !string.IsNullOrWhiteSpace(ci.SequenceName));

    }

    /// <summary>
    /// The database specific description of a property on a type.
    /// </summary>
    public class ColumnInfo
    {
        public ColumnInfo(PropertyInfo propertyInfo)
        {
            Property = propertyInfo;

        }

        /// <summary>
        /// 
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsKey { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsGenerated { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsIdentity { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsVersion { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool ExcludeOnInsert { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool ExcludeOnUpdate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool ExcludeOnSelect { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public PropertyInfo Property { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string SequenceName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public LambdaExpression Output { get; set; }

        /// <summary>
        /// Gets the value of the specified column for a given instance of the object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <returns></returns>
        public virtual object GetValue<T>(T instance)
        {
            return Property.GetValue(instance);
        }
    }

    /// <summary>
    /// The Version column has specific behavior and additional data that needs to be consumed like a ColumnInfo.
    /// Specifically, we need to specify a different value to update the Version column to, from what is used as part of the where clause
    /// This property is the "Version" for Optimistic Concurrency on the table. Uses <see cref="ConcurrencyCheckAttribute"/>
    /// NOTE: Can only have one Version column specified on the table, and it cannot also be a Key, Identity, etc.
    /// </summary>
    public class VersionColumnInfo : ColumnInfo
    {
        public VersionColumnInfo(PropertyInfo propertyInfo)
        : base(propertyInfo)
        {

        }


        /// <summary>
        /// This is assumed to be the value for an update
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <returns></returns>
        public override object GetValue<T>(T instance)
        {
            return base.GetValue(instance);
        }
    }
}
