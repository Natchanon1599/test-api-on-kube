using TestKubeLibrary.Interfaces;
using TestKubeLibrary.Models;

namespace TestKubeApi.Repositories;

// Data access for testkube_test: raw SQL through IDataAccess (Dapper).
// Kept thin on purpose - no try/catch/log here; the business logic layer owns that.
public class TestKubeRepository : ITestKubeRepository
{
    private readonly IDataAccess _db;

    private const string SelectColumns = "SELECT id, title, is_done, created_at FROM testkube_test";

    public TestKubeRepository(IDataAccess db)
    {
        _db = db;
    }

    public async Task<IEnumerable<TestKubeTest>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.LoadData<TestKubeTest>($"{SelectColumns} ORDER BY created_at DESC", ct: ct);
    }

    public async Task<TestKubeTest?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var rows = await _db.LoadData<TestKubeTest>($"{SelectColumns} WHERE id = @Id", new { Id = id }, ct);
        return rows.FirstOrDefault();
    }

    public async Task<TestKubeTest> CreateAsync(TestKubeTest item, CancellationToken ct = default)
    {
        const string sql = @"INSERT INTO testkube_test (title, is_done)
                             VALUES (@Title, @IsDone)
                             RETURNING id, title, is_done, created_at";
        var rows = await _db.LoadData<TestKubeTest>(sql, new { item.Title, item.IsDone }, ct);
        return rows.First();
    }

    public async Task<bool> UpdateAsync(int id, TestKubeTest input, CancellationToken ct = default)
    {
        const string sql = "UPDATE testkube_test SET title = @Title, is_done = @IsDone WHERE id = @Id";
        var affected = await _db.SaveData(sql, new { Id = id, input.Title, input.IsDone }, ct);
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var affected = await _db.SaveData("DELETE FROM testkube_test WHERE id = @Id", new { Id = id }, ct);
        return affected > 0;
    }
}
