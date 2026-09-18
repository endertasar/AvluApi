namespace AvluApi.Models.DTOs;

public class GenerateChargesResultDto
{
    public int Period    { get; init; }
    public int Generated { get; init; }
    public int Skipped   { get; init; }
    public int Missing   { get; init; }
}
