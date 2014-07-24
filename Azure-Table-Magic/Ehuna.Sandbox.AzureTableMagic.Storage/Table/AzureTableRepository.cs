using System.Web;
using Ehuna.Sandbox.AzureTableMagic.Storage.Common.Collections;
using Ehuna.Sandbox.AzureTableMagic.Storage.Common.Data;
using Ehuna.Sandbox.AzureTableMagic.Storage.Common.Extensions;
using Ehuna.Sandbox.AzureTableMagic.Storage.Common.Functional;
using Ehuna.Sandbox.AzureTableMagic.Storage.Interfaces;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ehuna.Sandbox.AzureTableMagic.Storage.Table
{
    public class AzureTableRepository<TEntity> : IPartitionedRepository<TEntity>
        where TEntity : class, new()
    {
        const string TokenDelimiter = "__";
        const int ConflictCode = 409;
        const int NotFoundCode = 404;

        private CloudStorageAccount _cloudStorageAccount;
        private CloudTable _table;

        private readonly Func<TEntity, object>[] _partitionKeyGetters;
        private readonly Func<TEntity, object>[] _rowKeyGetters;
        private readonly PropertyInfo[] _allProperties;

        private readonly string[] _partitionKeyEntityPropertyNames;
        private readonly string[] _rowKeyEntityPropertyNames;
        private readonly IEnumerable<PropertyInfo> _nonKeyProperties;

        public
        AzureTableRepository(
             CloudStorageAccount cloudStorageAccount,
             Expression<Func<TEntity, object>>[] partitionKeyGetters,
             Expression<Func<TEntity, object>>[] rowKeyGetters,
             string[] partitionKeyEntityPropertyNames = null,
             string[] rowKeyEntityPropertyNames = null)
        {
            _cloudStorageAccount = cloudStorageAccount;

            // create the table
            var tableClient = cloudStorageAccount.CreateCloudTableClient();
            var tableName = typeof(TEntity).Name;
            var table = tableClient.GetTableReference(tableName);

            table.CreateIfNotExists();

            _table = table;
            
            // Save the partition and row key property names.
            _partitionKeyEntityPropertyNames =
                partitionKeyEntityPropertyNames ??
                    partitionKeyGetters.Select(partitionKeyGetter =>
                                                        partitionKeyGetter.GetMemberName()
                                                      ).ToArray();

            _rowKeyEntityPropertyNames =
                rowKeyEntityPropertyNames ??
                    rowKeyGetters.Select(rowKeyGetter =>
                                                    rowKeyGetter.GetMemberName()
                                                  ).ToArray();

            // Save the functions for retrieving the partition and row keys.
            _partitionKeyGetters = partitionKeyGetters
                                    .CompileAll()
                                    .ToArray();

            _rowKeyGetters = rowKeyGetters
                                    .CompileAll()
                                    .ToArray();

            // Save all the property references.
            _allProperties = typeof(TEntity).GetProperties();

            // Save the non-key properties for easy reference as well.
            _nonKeyProperties = _allProperties.Where(prop =>
                !_partitionKeyEntityPropertyNames.Contains(prop.Name) &&
                !_rowKeyEntityPropertyNames.Contains(prop.Name)
            );
        }

        public
        void
        InsertOrReplace(
            TEntity entity)
        {
            _table.Execute(
                TableOperation.InsertOrReplace(
                    ConvertToDynamicTableEntity(entity)));
        }

        public void Delete(TEntity entity)
        {
            var dynamicTableEntity = ConvertToDynamicTableEntity(entity);
            dynamicTableEntity.ETag = "*";

            try
            {
                _table.Execute(
                    TableOperation.Delete(
                        dynamicTableEntity));
            }
            catch (StorageException ex)
            {
                var code = ex.RequestInformation.HttpStatusCode;

                if (code != NotFoundCode)
                    throw;
            }
        }

        public
        void
        Insert(
            TEntity entity)
        {
            try
            {
                _table.Execute(
                    TableOperation.Insert(
                        ConvertToDynamicTableEntity(entity)));
            }
            catch (StorageException ex)
            {
                var code = ex.RequestInformation.HttpStatusCode;

                if (code != ConflictCode)
                    throw;
            }
        }

        public
        static
        string
        AssembleFullKey(
            TEntity entity,
            params Func<TEntity, object>[] keyParts)
        {
            var fullKey = new StringBuilder();

            foreach (var keyPart in keyParts)
            {
                if (fullKey.Length > 0)
                    fullKey.Append(TokenDelimiter);

                var keyValue = keyPart(entity);
                var keyValueString = ConvertValueToString(keyValue);

                fullKey.Append(keyValueString);
            }

            return EncodeKey(
                        fullKey.ToString());
        }

        public
        static
        string
        ConvertValueToString(
            object objectValue)
        {
            // TODO: Better handle the type conversions so they are more configurable.

            if (objectValue is DateTime)
                return ((DateTime)objectValue).ToString("u");
            if (objectValue is DateTimeOffset)
                return ((DateTimeOffset)objectValue).ToString("u");

            return objectValue.ToString();
        }

        public
        async Task<PagedResult<TEntity>>
         GetPageAsync(
            object partitionKeyFrom,
            object partitionKeyTo,
            int maxPerPage = 1000,
            string nextPageToken = null)
        {
            TableContinuationToken continuationToken = null;

            var min =
                 TableQuery.GenerateFilterCondition(
                     "PartitionKey",
                     QueryComparisons.GreaterThanOrEqual,
                     EncodeKey(
                         ConvertValueToString(
                             partitionKeyFrom))
                     );

            var max =
                TableQuery.GenerateFilterCondition(
                    "PartitionKey",
                    QueryComparisons.LessThan,
                    EncodeKey(
                        ConvertValueToString(
                            partitionKeyTo))
                    );

            var query = new TableQuery().Where(TableQuery.CombineFilters(min, TableOperators.And, max))
                                        .Take(maxPerPage);

            if (!string.IsNullOrWhiteSpace(nextPageToken))
            {
                var parts = nextPageToken.Split(new[] { TokenDelimiter }, StringSplitOptions.None);

                continuationToken = new TableContinuationToken
                {
                    NextPartitionKey = parts.First(),
                    NextRowKey = parts.Second()
                };
            }

            var response = await ExecuteTableQueryAsync(query, continuationToken);

            var pagedResults = new PagedResult<TEntity>
            {
                Items = response.Results.ConvertAll(ConvertToEntity),
                NextPageUrl = response.ContinuationToken == null ?
                                null : response.ContinuationToken.NextPartitionKey + TokenDelimiter + response.ContinuationToken.NextRowKey
            };

            return pagedResults;
        }

        public
        async Task<PagedResult<TEntity>>
        GetPageAsync(
            string partitionKey,
            string rowGreaterThanOrEqual,
            string rowLessThan,
            int maxPerPage = 1000,
            string nextPageToken = null)
        {
            TableContinuationToken continuationToken = null;

            var pkFilter =
                TableQuery.GenerateFilterCondition(
                    "PartitionKey",
                    QueryComparisons.Equal,
                    EncodeKey(
                        partitionKey));

            var rkFilterLower =
                TableQuery.GenerateFilterCondition(
                    "RowKey",
                    QueryComparisons.GreaterThanOrEqual,
                    EncodeKey(
                        rowGreaterThanOrEqual));

            var rkFilterUpper =
                TableQuery.GenerateFilterCondition(
                    "RowKey",
                    QueryComparisons.LessThan,
                    EncodeKey(
                        rowLessThan));

            var combinedRowFilter =
                TableQuery.CombineFilters(
                    rkFilterLower,
                    TableOperators.And,
                    rkFilterUpper);

            var combinedFilter =
                TableQuery.CombineFilters(
                    pkFilter,
                    TableOperators.And,
                    combinedRowFilter);

            var query = new TableQuery().Where(
                combinedFilter
            ).Take(maxPerPage);

            // TODO: refactor to avoid this code duplication

            if (!string.IsNullOrWhiteSpace(nextPageToken))
            {
                var parts = nextPageToken.Split(new[] { TokenDelimiter }, StringSplitOptions.None);

                continuationToken = new TableContinuationToken
                {
                    NextPartitionKey = parts.First(),
                    NextRowKey = parts.Second()
                };
            }

            var response = await ExecuteTableQueryAsync(query, continuationToken);

            var pagedResults = new PagedResult<TEntity>
            {
                Items = response.Results.ConvertAll(ConvertToEntity),
                NextPageUrl = response.ContinuationToken == null ?
                                null : response.ContinuationToken.NextPartitionKey + TokenDelimiter + response.ContinuationToken.NextRowKey
            };

            return pagedResults;
        }

        public
        TEntity
        Get(
            string partitionKey,
            string rowKey)
        {
            var response = _table.Execute(
                TableOperation
                .Retrieve<DynamicTableEntity>(
                    EncodeKey(
                        partitionKey),
                    EncodeKey(
                        rowKey)));

            return
                response.HttpStatusCode != 404
                    ? ConvertToEntity((DynamicTableEntity)response.Result)
                    : null;   // return null if not found
        }

        public
        TEntity
        Get(
            TEntity entity)
        {
            return
                Get(
                    AssembleFullKey(entity, _partitionKeyGetters),
                    AssembleFullKey(entity, _rowKeyGetters));
        }

        public
        async
        Task<TEntity>
        GetAsync(
            string partitionKey,
            string rowKey)
        {
            var pkFilter =
                TableQuery.GenerateFilterCondition(
                    "PartitionKey",
                    QueryComparisons.Equal,
                    EncodeKey(
                        partitionKey));

            var rkFilter =
                TableQuery.GenerateFilterCondition(
                    "RowKey",
                    QueryComparisons.Equal,
                    EncodeKey(
                        rowKey));

            var query =
               new TableQuery()
               .Where(TableQuery.CombineFilters(
                                           pkFilter,
                                           TableOperators.And,
                                           rkFilter));

            var response = await ExecuteTableQueryAsync(query);

            return
                response.Results
                .FirstOrDefault()
                .DoNotNull(
                    result => ConvertToEntity((DynamicTableEntity)result));
        }

        public
        async
        Task<TEntity>
        GetAsync(
            TEntity entity)
        {
            return
                await
                GetAsync(
                        AssembleFullKey(entity, _partitionKeyGetters),
                        AssembleFullKey(entity, _rowKeyGetters));
        }

        private
        Task<TableQuerySegment<DynamicTableEntity>>
        ExecuteTableQueryAsync(
            TableQuery query,
            TableContinuationToken token = null)
        {
            return Task.Factory.FromAsync(
                _table.BeginExecuteQuerySegmented,
                ar => _table.EndExecuteQuerySegmented(ar),
                query,
                token,
                null
            );
        }

        public
        DynamicTableEntity
        ConvertToDynamicTableEntity(
            TEntity entity)
        {
            var partitionKeyString = AssembleFullKey(entity, _partitionKeyGetters);
            var rowKeyString = AssembleFullKey(entity, _rowKeyGetters);

            var nonKeyPropertiesAndValues = _nonKeyProperties.ToDictionary(
                property => property.Name,
                property => ConvertToEntityProperty(property.GetValue(entity))
            );

            return new DynamicTableEntity(
                partitionKeyString,
                rowKeyString,
                null,
                nonKeyPropertiesAndValues
            );
        }

        public
        TEntity
        ConvertToEntity(
            DynamicTableEntity dynamicTableEntity)
        {
            var entity = new TEntity();

            // Set the partition and row key properties
            var partitionKeyProperties = _allProperties.Where(prop =>
                _partitionKeyEntityPropertyNames.Contains(prop.Name)
            ).ToArray();

            var partitionKeyParts = dynamicTableEntity.PartitionKey.Split(
                new[] { TokenDelimiter },
                StringSplitOptions.RemoveEmptyEntries
            );

            for (var i = 0; i < partitionKeyProperties.Count(); i++)
            {
                var partitionKeyProperty = partitionKeyProperties[i];
                var partitionKeyStringValue = partitionKeyParts[i];

                partitionKeyProperty.SetValue(
                    obj: entity,
                    value: ConvertStringToType(
                                stringValue: DecodeKey(partitionKeyStringValue),
                                type: partitionKeyProperty.PropertyType)
                );
            }

            var rowKeyProperties = _allProperties.Where(prop =>
                _rowKeyEntityPropertyNames.Contains(prop.Name)
            ).ToArray();

            var rowKeyParts = dynamicTableEntity.RowKey.Split(
                new[] { TokenDelimiter },
                StringSplitOptions.RemoveEmptyEntries
            );

            for (var i = 0; i < rowKeyProperties.Count(); i++)
            {
                var rowKeyProperty = rowKeyProperties[i];
                var rowKeyStringValue = rowKeyParts[i];

                rowKeyProperty.SetValue(
                    obj: entity,
                    value: ConvertStringToType(
                                stringValue: DecodeKey(rowKeyStringValue),
                                type: rowKeyProperty.PropertyType)
                );
            }

            // Set all the remaining non-key properties
            foreach (var property in _nonKeyProperties)
                property.SetValue(
                    obj: entity,
                    value: ConvertEntityPropertyToType(
                                dynamicTableEntity.Properties[property.Name],
                                property.PropertyType)
                );

            return entity;
        }

        private
        static
        object
        ConvertStringToType(
            string stringValue,
            Type type)
        {
            var tc = TypeDescriptor.GetConverter(type);

            var parsedPropertyValue = tc.ConvertFromString(
                null,
                CultureInfo.InvariantCulture,
                stringValue
            );

            return parsedPropertyValue;
        }

        private
        static
        EntityProperty
        ConvertToEntityProperty(
            object value)
        {
            var typeName = value.GetType().Name;

            switch (typeName)
            {
                case "Boolean": return EntityProperty.GeneratePropertyForBool((bool)value);
                case "Byte[]": return EntityProperty.GeneratePropertyForByteArray((byte[])value);
                case "DateTime": return EntityProperty.GeneratePropertyForDateTimeOffset(new DateTimeOffset((DateTime)value));
                case "DateTimeOffset": return EntityProperty.GeneratePropertyForDateTimeOffset((DateTimeOffset)value);
                case "Double": return EntityProperty.GeneratePropertyForDouble((double)value);
                case "Guid": return EntityProperty.GeneratePropertyForGuid((Guid)value);
                case "Int32": return EntityProperty.GeneratePropertyForInt((int)value);
                case "Int64": return EntityProperty.GeneratePropertyForLong((long)value);
                case "String": return EntityProperty.GeneratePropertyForString((string)value);

                default:
                    throw new NotSupportedException(
                        "The specified type '{0}' is not supported.".Fmt(typeName)
                    );
            }
        }

        private
        static
        object
        ConvertEntityPropertyToType(
            EntityProperty entityProperty,
            Type propertyType)
        {
            var typeName = propertyType.Name;

            switch (typeName)
            {
                case "Boolean": return entityProperty.BooleanValue.Value;
                case "Byte[]": return entityProperty.BinaryValue;
                case "DateTime": return entityProperty.DateTimeOffsetValue.Value.DateTime;
                case "DateTimeOffset": return entityProperty.DateTimeOffsetValue.Value;
                case "Double": return entityProperty.DoubleValue.Value;
                case "Guid": return entityProperty.GuidValue.Value;
                case "Int32": return entityProperty.Int32Value.Value;
                case "Int64": return entityProperty.Int64Value.Value;
                case "String": return entityProperty.StringValue;

                default:
                    throw new NotSupportedException(
                        "The specified type '{0}' is not supported.".Fmt(typeName)
                    );
            }
        }

        private
        static
        string
        EncodeKey(
            string key)
        {
            return HttpUtility.UrlEncode(
                key);
        }

        private
        static
        string
        DecodeKey(
            string key)
        {
            return HttpUtility.UrlDecode(
                key);
        }

    }
}
