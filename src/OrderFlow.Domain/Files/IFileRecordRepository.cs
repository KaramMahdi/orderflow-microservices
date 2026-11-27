using Domain.Shared.Layer.Repositories;

namespace OrderFlow.Domain.Files;

/// <summary>The repository of <see cref="FileRecord"/>. Implemented in EntityFrameworkCore.</summary>
public interface IFileRecordRepository : IRepository<FileRecord>;
