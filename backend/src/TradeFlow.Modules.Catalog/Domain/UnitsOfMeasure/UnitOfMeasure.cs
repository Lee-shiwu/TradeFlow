using System;
using System.Collections.Generic;
using System.Text;
using TradeFlow.BuildingBlocks.Exceptions;

namespace TradeFlow.Modules.Catalog.Domain.UnitsOfMeasure;

public sealed class UnitOfMeasure
{
    public Guid Id { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;
    public UnitOfMeasureStatus Status { get; private set; } = UnitOfMeasureStatus.Active;
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();
    internal const int MaxCodeLength = 10;
    internal const int MaxNameLength = 50;

    public static UnitOfMeasure Create(string? code)
    {
        string normalizedCode = NormalizeCode(code);
        string name = ResolveName(normalizedCode);
        return new UnitOfMeasure
        {
            Id = Guid.NewGuid(),
            Code = normalizedCode,
            Name = name,
            Status = UnitOfMeasureStatus.Active,
            RowVersion = Array.Empty<byte>()
        };


    }

    private static string NormalizeCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new BusinessRuleException(UnitOfMeasureErrors.CodeRequiredCode, UnitOfMeasureErrors.CodeRequiredMessage);
        }
        string normalizedCode = code.Trim().ToUpperInvariant();

        if (normalizedCode.Length > MaxCodeLength)
        {
            throw new BusinessRuleException(UnitOfMeasureErrors.CodeInvalidCode, UnitOfMeasureErrors.CodeTooLongMessage);
        }
        bool containsInvalidCode = normalizedCode.Any(character => character < 'A' || character > 'Z');
        if (containsInvalidCode)
        {
            throw new BusinessRuleException(UnitOfMeasureErrors.CodeInvalidCode, UnitOfMeasureErrors.CodeInvalidCharactersMessage);
        }
        return normalizedCode;
    }

    private static string ResolveName(string normalizedCode)
    {
        return normalizedCode switch
        {
            "EA" => "Each",
            "KG" => "Kilogram",
            "L" => "Litre",
            "M" => "Metre",

            _ => throw new BusinessRuleException(UnitOfMeasureErrors.CodeUnsupportedCode, UnitOfMeasureErrors.CodeUnsupportedMessage)
        };

    }

    private UnitOfMeasure()
    { }
}
