using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using SQLite;
using System;
using System.IO;

namespace DigWex
{ 
  [Table("Stats")]
  public class StatsEntity
  {
    [PrimaryKey]
    public long Id { get; set; }
    [NotNull]
    public string Data { get; set; }
  } 

  [Table("Data")]
  public class DataEntity
  {
    [PrimaryKey]
    public int Id { get; set; }
    public string Data { get; set; }
  }

  public class DataContext : SQLiteAsyncConnection
  {
    public const string DB_NAME = "base.db";

    private static readonly Lazy<DataContext> _instance =
        new Lazy<DataContext>(() => new DataContext());

    private DataContext() : base(Path.GetFullPath(Config.AppData + "/databases/base.db"))
    {
    }

    public static DataContext Instance
    {
      get { return _instance.Value; }
    }

    public async Task Init()
    {
      await Initialize().ConfigureAwait(false);
    }

    private async Task Initialize()
    {
      await CreateTableAsync<DataEntity>().ConfigureAwait(false);
      await CreateTableAsync<StatsEntity>().ConfigureAwait(false);
    }

    public async Task SaveItems<T>(IEnumerable<T> items) where T : new()
    {
      await InsertAllAsync(items).ConfigureAwait(false);
    }

    public async Task SaveItem<T>(T item) where T : new()
    {
      await InsertAsync(item).ConfigureAwait(false);
    }

    public Task<List<T>> GetItems<T>() where T : new()
    {
      return Table<T>().ToListAsync();
    }

    public Task<List<T>> GetItems<T>(int n) where T : new()
    {
      return Table<T>().Take(n).ToListAsync();
    }

    public Task<DataEntity> GetClientData(int id)
    {
      return Table<DataEntity>().Where(t => t.Id == id).FirstOrDefaultAsync();
    }

    public async Task InsertOrUpdateAsync<T>(T item) where T : new()
    {
      int count = await UpdateAsync(item);
      if (count == 0)
        await InsertAsync(item);
    }

    public async Task UpdateItems(IEnumerable items)
    {
      await UpdateAllAsync(items);
    }

    public async Task UpdateItem<T>(T item) where T : new()
    {
      await UpdateAsync(item);
    }

    public async Task DeleteItems(IEnumerable items)
    {
      foreach (var item in items)
      {
        await DeleteAsync(item);
      }
    }

    public Task<int> DeleteItem(object item)
    {
      return DeleteAsync(item);
    }
  }
}