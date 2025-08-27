using Validated.Core.Extensions;
using Validated.Core.Types;

namespace Validated.ValueObject.Domain.ValueObjects;

public sealed record DateRange 
{ 
    public DateOnly StartDate { get; }
    public DateOnly EndDate   { get; }

    private DateRange(DateOnly startDate, DateOnly endDate)

        => (StartDate, EndDate) = (startDate, endDate);

    internal static Validated<DateRange> Create(Validated<DateOnly> validatedStartDate, Validated<DateOnly> validatedEndDate)

        => (validatedStartDate, validatedEndDate).Combine((startDate, endDate) => new DateRange(startDate, endDate));
}
