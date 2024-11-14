namespace Common.Services.Interfaces;

public interface ICodeGeneratorService
{
    Task<List<string>> GetCodesAsync(byte length, ushort count);
}
