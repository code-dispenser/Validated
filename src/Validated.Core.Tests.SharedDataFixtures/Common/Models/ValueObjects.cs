using Validated.Core.Types;
using Validated.Core.Extensions;

namespace Validated.Core.Tests.SharedDataFixtures.Common.Models;

public sealed record DateRange 
{ 
    public DateOnly StartDate { get; }
    public DateOnly EndDate   { get; }

    private DateRange(DateOnly startDate, DateOnly endDate)

        => (StartDate, EndDate) = (startDate, endDate);

    public static Validated<DateRange> Create(Validated<DateOnly> startDate, Validated<DateOnly> endDate)
    {
        Func<DateOnly, Func<DateOnly, DateRange>> curriedFunc = startDate => endDate => new DateRange(startDate, endDate);

        return Validated<Func<DateOnly, Func<DateOnly, DateRange>>>.Valid(curriedFunc)
              .Apply(startDate)
                  .Apply(endDate);
    }
}

