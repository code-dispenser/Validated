using FluentAssertions;
using FluentAssertions.Execution;
using Validated.Core.Common.Constants;
using Validated.Core.Extensions;
using Validated.Core.Tests.SharedDataFixtures.Common.Models;
using Validated.Core.Tests.SharedDataFixtures.Common.Validators;
using Validated.Core.Types;

namespace Validated.Core.Tests.Unit.Extensions;

public class ValidatedExtensions_Tests
{
    public class AndThen
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task And_then_should_compose_the_validators_producing_a_new_validator(bool happyPath)
        {
            var validatorOne = happyPath ? StubbedValidators.CreatePassingMemberValidator<int>() : StubbedValidators.CreateFailingMemberValidator<int>("PropertyNameOne", "DisplayNameOne", "FailureMessageOne");
            var validatorTwo = happyPath ? StubbedValidators.CreatePassingMemberValidator<int>() : StubbedValidators.CreateFailingMemberValidator<int>("PropertyNameTwo", "DisplayNameTwo", "FailureMessageTwo");

            var validator = ValidatedExtensions.AndThen(validatorOne, validatorTwo);
            var validated = await validator(42, "Path");

            if (true == happyPath)
            {
                validated.Should().Match<Validated<int>>(v => v.IsValid == true && v.Failures.Count == 0);
                return;
            }

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<int>>(v => v.IsValid == false && v.Failures.Count == 2);

                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == "Path" && i.PropertyName=="PropertyNameOne" && i.DisplayName == "DisplayNameOne"
                                                              && i.FailureMessage=="FailureMessageOne" && i.Cause == CauseType.Validation);

                validated.Failures[1].Should().Match<InvalidEntry>(i => i.Path == "Path" && i.PropertyName=="PropertyNameTwo" && i.DisplayName == "DisplayNameTwo"
                                                   && i.FailureMessage=="FailureMessageTwo" && i.Cause == CauseType.Validation);
            }

        }
    }
    public class Apply
    {

        [Fact]
        public void Apply_should_get_the_func_and_apply_it_to_the_valid_validated_value_when_its_valid()
        {
            static int addTenFunc(int x) => x + 10;

            var validatedFunc = Validated<Func<int, int>>.Valid(addTenFunc);
            var validatedItem = Validated<int>.Valid(5);

            var validated = ValidatedExtensions.Apply(validatedFunc, validatedItem);

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<int>>(v => v.IsValid == true && v.Failures.Count == 0);
                validated.GetValueOr(0).Should().Be(15); // 10 + 5
            }
        }

        [Fact]
        public void Apply_should_return_an_invalid_validated_when_the_func_is_valid_but_the_validated_item_is_invalid()
        {
            static int addTenFunc(int x) => x + 10;

            var validatedFunc = Validated<Func<int, int>>.Valid(addTenFunc);
            var validatedItem = Validated<int>.Invalid(new InvalidEntry("FailureMessage", "Path", "PropertyName", "DisplayName", CauseType.Validation));

            var validated = ValidatedExtensions.Apply(validatedFunc, validatedItem);

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<int>>(v => v.IsValid == false && v.Failures.Count == 1);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == "Path" && i.PropertyName == "PropertyName" && i.DisplayName == "DisplayName"
                                                                && i.FailureMessage == "FailureMessage" && i.Cause == CauseType.Validation);
            }
        }

        [Fact]
        public void Apply_should_return_an_invalid_validated_when_the_func_is_invalid_and_the_validated_item_is_valid()
        {
            var validatedFunc = Validated<Func<int, int>>.Invalid(new InvalidEntry("FailureMessage", "Path", "PropertyName", "DisplayName", CauseType.Validation));
            var validatedItem = Validated<int>.Valid(5);

            var validated = ValidatedExtensions.Apply(validatedFunc, validatedItem);

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<int>>(v => v.IsValid == false && v.Failures.Count == 1);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == "Path" && i.PropertyName == "PropertyName" && i.DisplayName == "DisplayName"
                                                                && i.FailureMessage == "FailureMessage" && i.Cause == CauseType.Validation);
            }
        }

        [Fact]
        public void Apply_should_return_an_invalid_validated_with_all_failures_when_both_the_func_and_validated_item_is_invalid()
        {
            var validatedFunc = Validated<Func<int, int>>.Invalid(new InvalidEntry("FuncFailureMessage", "Path", "PropertyName", "DisplayName", CauseType.Validation));
            var validatedItem = Validated<int>.Invalid(new InvalidEntry("ValidatedFailureMessage", "Path", "PropertyName", "DisplayName", CauseType.Validation));

            var validated = ValidatedExtensions.Apply(validatedFunc, validatedItem);

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<int>>(v => v.IsValid == false && v.Failures.Count == 2);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == "Path" && i.FailureMessage == "FuncFailureMessage" && i.Cause == CauseType.Validation);
                validated.Failures[1].Should().Match<InvalidEntry>(i => i.Path == "Path" && i.FailureMessage == "ValidatedFailureMessage" && i.Cause == CauseType.Validation);
            }
        }


        [Fact]
        public void Apply_show_work_with_curried_functions()
        {
            Func<int, int> addCurriedFunc(int x) => y => x + y;

            var validatedCurriedFunc = Validated<Func<int, Func<int, int>>>.Valid(addCurriedFunc);
            var firstValidated = Validated<int>.Valid(10);
            var secondValidated = Validated<int>.Valid(5);

            var partiallyApplied = ValidatedExtensions.Apply(validatedCurriedFunc, firstValidated);
            var finalResult = ValidatedExtensions.Apply(partiallyApplied, secondValidated);

            using (new AssertionScope())
            {
                partiallyApplied.Should().Match<Validated<Func<int, int>>>(v => v.IsValid == true && v.Failures.Count == 0);
                partiallyApplied.GetValueOr(null!).Should().BeOfType<Func<int, int>>();// x already applied

                finalResult.Should().Match<Validated<int>>(v => v.IsValid == true && v.Failures.Count == 0);
                finalResult.GetValueOr(0).Should().Be(15);
            }
        }

        [Fact]
        public async Task Apply_async_should_get_the_func_and_apply_it_to_the_valid_validated_value_when_its_valid()
        {
            static int addTenFunc(int x) => x + 10;

            var validatedFunc = Task.FromResult(Validated<Func<int, int>>.Valid(addTenFunc));
            var validatedItem = Task.FromResult(Validated<int>.Valid(5));

            var validated = await ValidatedExtensions.Apply(validatedFunc, validatedItem);

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<int>>(v => v.IsValid == true && v.Failures.Count == 0);
                validated.GetValueOr(0).Should().Be(15); // 10 + 5
            }
        }

        [Fact]
        public async Task Apply_async_should_return_an_invalid_validated_when_the_func_is_valid_but_the_validated_item_is_invalid()
        {
            static int addTenFunc(int x) => x + 10;

            var validatedFunc = Task.FromResult(Validated<Func<int, int>>.Valid(addTenFunc));
            var validatedItem = Task.FromResult(Validated<int>.Invalid(new InvalidEntry("FailureMessage", "Path", "PropertyName", "DisplayName",  CauseType.Validation)));

            var validated = await ValidatedExtensions.Apply(validatedFunc, validatedItem);

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<int>>(v => v.IsValid == false && v.Failures.Count == 1);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == "Path" && i.PropertyName == "PropertyName" && i.DisplayName == "DisplayName"
                                                                && i.FailureMessage == "FailureMessage" && i.Cause == CauseType.Validation);
            }
        }

        [Fact]
        public async Task Apply_async_should_return_an_invalid_validated_when_the_func_is_invalid_and_the_validated_item_is_valid()
        {
            var validatedFunc = Task.FromResult(Validated<Func<int, int>>.Invalid(new InvalidEntry("FailureMessage", "Path", "PropertyName", "DisplayName", CauseType.Validation)));
            var validatedItem = Task.FromResult(Validated<int>.Valid(5));

            var validated = await ValidatedExtensions.Apply(validatedFunc, validatedItem);

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<int>>(v => v.IsValid == false && v.Failures.Count == 1);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == "Path" && i.PropertyName == "PropertyName" && i.DisplayName == "DisplayName"
                                                                && i.FailureMessage == "FailureMessage" && i.Cause == CauseType.Validation);
            }
        }

        [Fact]
        public async Task Apply_async_should_return_an_invalid_validated_with_all_failures_when_both_the_func_and_validated_item_is_invalid()
        {
            var validatedFunc = Task.FromResult(Validated<Func<int, int>>.Invalid(new InvalidEntry("FuncFailureMessage", "Path", "PropertyName", "DisplayName",  CauseType.Validation)));
            var validatedItem = Task.FromResult(Validated<int>.Invalid(new InvalidEntry("ValidatedFailureMessage", "Path", "PropertyName", "DisplayName", CauseType.Validation)));

            var validated = await ValidatedExtensions.Apply(validatedFunc, validatedItem);

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<int>>(v => v.IsValid == false && v.Failures.Count == 2);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == "Path" && i.FailureMessage == "FuncFailureMessage" && i.Cause == CauseType.Validation);
                validated.Failures[1].Should().Match<InvalidEntry>(i => i.Path == "Path" && i.FailureMessage == "ValidatedFailureMessage" && i.Cause == CauseType.Validation);
            }
        }

        [Fact]
        public async Task Apply_async_show_work_with_curried_functions()
        {
            Func<int, int> addCurriedFunc(int x) => y => x + y;

            var validatedCurriedFunc = Task.FromResult(Validated<Func<int, Func<int, int>>>.Valid(addCurriedFunc));
            var firstValidated = Task.FromResult(Validated<int>.Valid(10));
            var secondValidated = Task.FromResult(Validated<int>.Valid(5));

            var partiallyApplied = await ValidatedExtensions.Apply(validatedCurriedFunc, firstValidated);
            var finalResult = await ValidatedExtensions.Apply(Task.FromResult(partiallyApplied), secondValidated);

            using (new AssertionScope())
            {
                partiallyApplied.Should().Match<Validated<Func<int, int>>>(v => v.IsValid == true && v.Failures.Count == 0);
                partiallyApplied.GetValueOr(null!).Should().BeOfType<Func<int, int>>();// x already applied

                finalResult.Should().Match<Validated<int>>(v => v.IsValid == true && v.Failures.Count == 0);
                finalResult.GetValueOr(0).Should().Be(15);
            }
        }
    }
    public class Combine
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Entity_validator_Combine_should_combine_entity_validators_to_produce_a_single_validated_result_with_all_failures_if_they_occurred(bool happyPath)
        {
            var validatorOne = StubbedValidators.CreatePassingEntityValidator<ContactDto>();
            var validatorTwo = happyPath ? StubbedValidators.CreatePassingEntityValidator<ContactDto>() : StubbedValidators.CreateFailingEntityValidator<ContactDto>("PropertyNameOne", "DisplayNameOne", "FailureMessageOne");
            var validatorThree = happyPath ? StubbedValidators.CreatePassingEntityValidator<ContactDto>() : StubbedValidators.CreateFailingEntityValidator<ContactDto>("PropertyNameTwo", "DisplayNameTwo", "FailureMessageTwo");

            var validator = ValidatedExtensions.Combine(validatorOne, validatorTwo, validatorThree);
            var validated = await validator(new ContactDto(), nameof(ContactDto));

            if (true == happyPath)
            {
                validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == true && v.Failures.Count == 0);
                return;
            }

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<ContactDto>>(v => v.IsValid == false && v.Failures.Count == 2);
                validated.Failures[0].Should().Match<InvalidEntry>(i => i.Path == "ContactDto" && i.PropertyName=="PropertyNameOne" && i.DisplayName == "DisplayNameOne"
                                                               && i.FailureMessage=="FailureMessageOne" && i.Cause == CauseType.Validation);

                validated.Failures[1].Should().Match<InvalidEntry>(i => i.Path == "ContactDto" && i.PropertyName=="PropertyNameTwo" && i.DisplayName == "DisplayNameTwo"
                                                   && i.FailureMessage=="FailureMessageTwo" && i.Cause == CauseType.Validation);
            }

        }



        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Validated_combine_with_two_params_should_return_either_a_valid_validated_or_an_invalid_validated_with_the_combined_failures(bool happyPath)
        {


            var validatorOne = happyPath ? StubbedValidators.CreatePassingMemberValidator<string>() : StubbedValidators.CreateFailingMemberValidator<string>("PropertyOne", "PropertyOne", "Value one is in valid");
            var validatorTwo = happyPath ? StubbedValidators.CreatePassingMemberValidator<int>() : StubbedValidators.CreateFailingMemberValidator<int>("PropertyTwo", "PropertyTwo", "Value two is in valid");

            var validated = (await validatorOne("ValueOne", "Path"), await validatorTwo(42, "Path")).Combine((name, age) => new CombineWithTwoParam(name, age));

            if (true == happyPath)
            {
                using var scope = new AssertionScope();

                validated.Should().Match<Validated<CombineWithTwoParam>>(v => v.IsValid == true && v.Failures.Count == 0);
                validated.GetValueOr(null!).Age.Should().Be(42);
                return;
            }

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<CombineWithTwoParam>>(v => v.IsValid == false && v.Failures.Count == 2);
                validated.Failures[1].Should().Match<InvalidEntry>(i => i.PropertyName == "PropertyTwo" && i.FailureMessage == "Value two is in valid");
            }


        }
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Validated_combine_with_three_params_should_return_either_a_valid_validated_or_an_invalid_validated_with_the_combined_failures(bool happyPath)
        {
            var validatorOne = happyPath ? StubbedValidators.CreatePassingMemberValidator<string>() : StubbedValidators.CreateFailingMemberValidator<string>("PropertyOne", "PropertyOne", "Value one is in valid");
            var validatorTwo = happyPath ? StubbedValidators.CreatePassingMemberValidator<int>() : StubbedValidators.CreateFailingMemberValidator<int>("PropertyTwo", "PropertyTwo", "Value two is in valid");
            var validatorThree = happyPath ? StubbedValidators.CreatePassingMemberValidator<DateOnly>() : StubbedValidators.CreateFailingMemberValidator<DateOnly>("PropertyThree", "PropertyThree", "Value three is in valid");

            var dob = DateOnly.FromDateTime(new DateTime(2000, 01, 01));

            var validated = (await validatorOne("ValueOne", "Path"), await validatorTwo(42, "Path"), await validatorThree(dob, "Path")).Combine((name, age, dob) => new CombineWithThreeParam(name, age, dob));

            if (true == happyPath)
            {
                using var scope = new AssertionScope();

                validated.Should().Match<Validated<CombineWithThreeParam>>(v => v.IsValid == true && v.Failures.Count == 0);
                validated.GetValueOr(null!).DOB.Should().Be(dob);
                return;
            }

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<CombineWithThreeParam>>(v => v.IsValid == false && v.Failures.Count == 3);
                validated.Failures[2].Should().Match<InvalidEntry>(i => i.PropertyName == "PropertyThree" && i.FailureMessage == "Value three is in valid");
            }

        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Validated_combine_with_four_params_should_return_either_a_valid_validated_or_an_invalid_validated_with_the_combined_failures(bool happyPath)
        {
            var validatorOne = happyPath ? StubbedValidators.CreatePassingMemberValidator<string>() : StubbedValidators.CreateFailingMemberValidator<string>("PropertyOne", "PropertyOne", "Value one is in valid");
            var validatorTwo = happyPath ? StubbedValidators.CreatePassingMemberValidator<int>() : StubbedValidators.CreateFailingMemberValidator<int>("PropertyTwo", "PropertyTwo", "Value two is in valid");
            var validatorThree = happyPath ? StubbedValidators.CreatePassingMemberValidator<DateOnly>() : StubbedValidators.CreateFailingMemberValidator<DateOnly>("PropertyThree", "PropertyThree", "Value three is in valid");
            var validatorFour = happyPath ? StubbedValidators.CreatePassingMemberValidator<decimal>() : StubbedValidators.CreateFailingMemberValidator<decimal>("PropertyFour", "PropertyFour", "Value four is in valid");

            var dob = DateOnly.FromDateTime(new DateTime(2000, 01, 01));

            var validated = (await validatorOne("ValueOne", "Path"), await validatorTwo(42, "Path"), await validatorThree(dob, "Path"), await validatorFour(1.23M, "Path"))
                                .Combine((name, age, dob, total) => new CombineWithFourParam(name, age, dob, total));

            if (true == happyPath)
            {
                using var scope = new AssertionScope();

                validated.Should().Match<Validated<CombineWithFourParam>>(v => v.IsValid == true && v.Failures.Count == 0);
                validated.GetValueOr(null!).Total.Should().Be(1.23M);
                return;
            }

            using (new AssertionScope())
            {
                validated.Should().Match<Validated<CombineWithFourParam>>(v => v.IsValid == false && v.Failures.Count == 4);
                validated.Failures[3].Should().Match<InvalidEntry>(i => i.PropertyName == "PropertyFour" && i.FailureMessage == "Value four is in valid");
            }

        }
    }

}
