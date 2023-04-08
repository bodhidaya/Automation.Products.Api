using Azure.Data.Tables;
using LanguageExt;
using Microsoft.Extensions.Azure;
using static Storage.Table.Helper.AzureTableStorageWrapper;

namespace Storage.Table.Helper;

public interface ITableService
{
    Task<TableOperation> UpsertAsync<T>(
        string category,
        string table,
        T data,
        bool merge,
        CancellationToken token
    ) where T : ITableEntity;
}

internal class TableService : ITableService
{
    private readonly IAzureClientFactory<TableServiceClient> _factory;

    public TableService(IAzureClientFactory<TableServiceClient> factory) => _factory = factory;

    public async Task<TableOperation> UpsertAsync<T>(
        string category,
        string table,
        T data,
        bool merge,
        CancellationToken token
    ) where T : ITableEntity =>
        (
            await (
                from sc in GetServiceClient(_factory, category)
                from tc in GetTableClient(sc, table)
                from op in Upsert(tc, data, token, merge)
                select op
            ).Run()
        ).Match(
            op => op,
            err =>
                TableOperation.Failure(
                    TableOperationError.New(err.Code, err.Message, err.ToException())
                )
        );
}
