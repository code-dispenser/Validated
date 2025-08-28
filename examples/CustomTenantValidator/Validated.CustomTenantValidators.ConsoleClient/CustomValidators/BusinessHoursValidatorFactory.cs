using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Validated.Core.Common.Constants;
using Validated.Core.Factories;
using Validated.Core.Types;

namespace Validated.CustomTenantValidators.ConsoleClient.CustomValidators;

public class BusinessHoursValidatorFactory : IValidatorFactory
{
    private readonly ILogger _logger;

    public const string RuleType_BusinessHours = "RuleType_BusinessHours";
    public BusinessHoursValidatorFactory(ILoggerFactory? loggerFactory = null)//Made optional for demo but you should require a logger

      => _logger =  loggerFactory?.CreateLogger<BusinessHoursValidatorFactory>() ?? NullLogger<BusinessHoursValidatorFactory>.Instance;
        
    
    /*
        * You could call make an async call to a database or web service, just add async => (valueToValidate . . . ) => and awaits
        * 
        * The MemberValidator delegate has one required param (the value to be validated) and three optional params
        * The path will here will be populated and should be used in any InvalidEntries..
        * The compareTo here will be empty and can be discarded. Its only used in multi-tenant value object comparisons
        * The cancellationToken will be None unless you provide it so it could be discarded.
    */ 
    public MemberValidator<T> CreateFromConfiguration<T>(ValidationRuleConfig ruleConfig) where T : notnull

        => (valueToValidate, path, compareTo, cancellationToken) =>
        {
            try
            {
                if (valueToValidate is not DateTime appointmentDateTime)     return Task.FromResult(LogAndReturn<T>(_logger,path, CauseType.SystemError,ruleConfig,null));

                if (ruleConfig is null || ruleConfig.AdditionalInfo is null) return Task.FromResult(LogAndReturn<T>(_logger, path,CauseType.RuleConfigError, ruleConfig, null));

                var ruleData = ruleConfig.AdditionalInfo;

                if (false == ruleData.ContainsKey("WorkingDays") || false == ruleData.ContainsKey("Holidays")) 
                    return Task.FromResult(LogAndReturn<T>(_logger, path,CauseType.RuleConfigError, ruleConfig, null));

                if (false == TimeOnly.TryParse(ruleData!["OpeningTime"], out var starTime) || false == TimeOnly.TryParse(ruleData["ClosingTime"], out var endTime)) 
                    return Task.FromResult(LogAndReturn<T>(_logger, path, CauseType.RuleConfigError, ruleConfig, null));

                var workingDays = new HashSet<DayOfWeek>(ruleData["WorkingDays"].Split(',', StringSplitOptions.TrimEntries).Select(day => Enum.Parse<DayOfWeek>(day, true)));

                if (workingDays.Count == 0) 
                    return Task.FromResult(LogAndReturn<T>(_logger, path, CauseType.RuleConfigError, ruleConfig, null));

                var holidays = new HashSet<DateOnly>(ruleData["Holidays"].Split(",", StringSplitOptions.TrimEntries).Select(date => DateOnly.ParseExact(date, "yyyy-MM-dd")));

                var dateValue = DateOnly.FromDateTime(appointmentDateTime);
                var timeValue = TimeOnly.FromDateTime(appointmentDateTime);

                if (true == holidays.Contains(dateValue)) 
                    return Task.FromResult(Validated<T>.Invalid(new InvalidEntry(ruleConfig.FailureMessage.Replace("{Reason}", "Bank holiday"), path, ruleConfig.PropertyName, ruleConfig.DisplayName, CauseType.Validation)));

                if (false == workingDays.Contains(appointmentDateTime.DayOfWeek))
                    return Task.FromResult(Validated<T>.Invalid(new InvalidEntry(ruleConfig.FailureMessage.Replace("{Reason}", "Closed"), path, ruleConfig.PropertyName, ruleConfig.DisplayName, CauseType.Validation)));


                if (timeValue < starTime || timeValue > endTime)
                    return Task.FromResult(Validated<T>.Invalid(new InvalidEntry(ruleConfig.FailureMessage.Replace("{Reason}", "Closed"), path, ruleConfig.PropertyName, ruleConfig.DisplayName, CauseType.Validation)));

                return Task.FromResult(Validated<T>.Valid(valueToValidate));
            }
            catch(Exception ex)//Prefer not to throw, just log the exception for inspection and return an invalid result
            {
                return Task.FromResult(LogAndReturn<T>(_logger, path, CauseType.SystemError, ruleConfig, ex));
            }
           
        };

    private Validated<T> LogAndReturn<T>(ILogger logger, string path, CauseType causeType, ValidationRuleConfig? ruleConfig = null, Exception? exception = null) where T : notnull
    {
        logger.LogError(exception, "Configuration error causing the validation failure for Tenant:{TenantId} - {TypeFullName}.{PropertyName}",
           ruleConfig?.TenantID        ?? "[Null]",
           ruleConfig?.TypeFullName    ?? "[Null]",
           ruleConfig?.PropertyName    ?? "[Null]"
       );

        var failureMessage = ruleConfig?.FailureMessage?.Replace("{Reason}", "") ?? "Unable to verity appointment details, please contact support";

        return Validated<T>.Invalid(new InvalidEntry(failureMessage, path, ruleConfig?.PropertyName ?? "Unknown", ruleConfig?.DisplayName ?? "Unknown", causeType));
    }
}

