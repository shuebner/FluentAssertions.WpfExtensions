using FluentAssertions.WpfExtensions.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace FluentAssertions
{
    [TestClass]
    public class NotifyDataErrorInfoAssertionsTests
    {
        private NotifyDataErrorInfo instance;

        [TestInitialize]
        public void Initialize()
        {
            instance = new NotifyDataErrorInfo();
        }

        [TestMethod]
        public void WhenExpressionIsNull_ShouldThrowArgumentNullException()
        {
            Action act = () => instance.Should().ExposeNotifyDataErrorInfoBehaviorFor(null, new NotifyDataErrorInfoSamples<string>("foo", new InvalidSample<string>("bar"), new InvalidSample<string>("baz")));

            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void WhenSamplesIsNull_ShouldThrowArgumentNullException()
        {
            Action act = () => instance.Should().ExposeNotifyDataErrorInfoBehaviorFor(i => i.CorrectlyImplementedValue, null);

            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void WhenInstanceInitiallyHasErrors_ShouldThrowArgumentException()
        {
            instance.HasErrors = true;

            Action act = () => instance.Should().ExposeNotifyDataErrorInfoBehaviorFor(
                i => i.CorrectlyImplementedValue,
                new NotifyDataErrorInfoSamples<string>("valid", new InvalidSample<string>("invalid1"), new InvalidSample<string>("invalid2")));

            act.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void WhenInstanceInitiallyContainsErrorForProperty_ShouldThrowArgumentException()
        {
            instance.ErrorsByPropertyName.Add(nameof(NotifyDataErrorInfo.CorrectlyImplementedValue), new[] { "error" });

            Action act = () => instance.Should().ExposeNotifyDataErrorInfoBehaviorFor(
                i => i.CorrectlyImplementedValue,
                new NotifyDataErrorInfoSamples<string>("valid", new InvalidSample<string>("invalid1"), new InvalidSample<string>("invalid2")));

            act.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void WhenValidSampleValueIsCurrentValue_ShouldThrowArgumentException()
        {
            instance.CorrectlyImplementedValue = "initialvalue";

            Action act = () => instance.Should().ExposeNotifyDataErrorInfoBehaviorFor(
                i => i.CorrectlyImplementedValue,
                new NotifyDataErrorInfoSamples<string>("initialvalue", new InvalidSample<string>("invalid1"), new InvalidSample<string>("invalid2")));

            act.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void WhenSettingDifferentValidValue_AndPropertyDidNotUpdate_FailsAssertion()
        {
            Action act = () => instance.Should().ExposeNotifyDataErrorInfoBehaviorFor(
                   i => i.ValueThatDoesNotUpdate,
                   new NotifyDataErrorInfoSamples<string>("valid", new InvalidSample<string>("invalid1"), new InvalidSample<string>("invalid2")));

            act.Should().Throw<AssertFailedException>().WithMessage("*update*");
        }

        [TestMethod]
        public void WhenSettingDifferentValidValue_AndHasErrors_FailsAssertion()
        {
            Action act = () => instance.Should().ExposeNotifyDataErrorInfoBehaviorFor(
                   i => i.ValueThatSetsHasErrorsToTrue,
                   new NotifyDataErrorInfoSamples<string>("valid", new InvalidSample<string>("invalid1"), new InvalidSample<string>("invalid2")));

            act.Should().Throw<AssertFailedException>("*errors*");
        }

        [TestMethod]
        public void WhenSettingDifferentValidValue_AndContainsErrorsForProperty_FailsAssertion()
        {
            Action act = () => instance.Should().ExposeNotifyDataErrorInfoBehaviorFor(
                   i => i.ValueThatSetsError,
                   new NotifyDataErrorInfoSamples<string>("valid", new InvalidSample<string>("invalid1"), new InvalidSample<string>("invalid2")));

            act.Should().Throw<AssertFailedException>();
        }

        [TestMethod]
        public void WhenSettingDifferentValidValue_AndRaisesErrorsChanged_FailsAssertion()
        {
            Action act = () => instance.Should().ExposeNotifyDataErrorInfoBehaviorFor(
                   i => i.ValueThatAlwaysRaisesErrorsChanged,
                   new NotifyDataErrorInfoSamples<string>("valid", new InvalidSample<string>("invalid1"), new InvalidSample<string>("invalid2")));

            act.Should().Throw<AssertFailedException>().WithMessage("*trigger*");
        }

        [TestMethod]
        public void WhenValueIsCorrectlyImplemented_PassesAssertion()
        {
            Action act = () => instance.Should().ExposeNotifyDataErrorInfoBehaviorFor(
                i => i.CorrectlyImplementedValue,
                new NotifyDataErrorInfoSamples<string>(
                    "valid",
                    new InvalidSample<string>("invalid1")
                    {
                        AssertOnErrors = errors => errors.Cast<string>().Should().ContainSingle().Which.Should().Contain("invalid1")
                    },
                    new InvalidSample<string>("invalid2")
                    {
                        AssertOnErrors = errors => errors.Cast<string>().Should().ContainSingle().Which.Should().Contain("invalid2")
                    }));

            act.Should().NotThrow();
        }

        private class NotifyDataErrorInfo : INotifyDataErrorInfo
        {
            private string valueThatSetsHasErrorsToTrue;
            private string valueThatDoesNotUpdate;
            private string valueThatSetsError;
            private string correctlyImplementedValue;
            private string valueThatAlwaysRaisesErrorsChanged;

            public IDictionary<string, IEnumerable> ErrorsByPropertyName { get; } = new Dictionary<string, IEnumerable>();

            public string CorrectlyImplementedValue
            {
                get => correctlyImplementedValue;
                set
                {
                    if (!object.Equals(correctlyImplementedValue, value))
                    {
                        correctlyImplementedValue = value;
                        this.Validate(correctlyImplementedValue);
                    }
                }
            }

            public string ValueThatDoesNotUpdate
            {
                get => valueThatDoesNotUpdate;
                set
                {
                    if (!object.Equals(correctlyImplementedValue, value))
                    {
                        this.Validate(value);
                    }
                }
            }

            public string ValueThatSetsHasErrorsToTrue
            {
                get => valueThatSetsHasErrorsToTrue;
                set
                {
                    valueThatSetsHasErrorsToTrue = value;
                    this.HasErrors = true;
                }
            }

            public string ValueThatSetsError
            {
                get => valueThatSetsError;
                set
                {
                    valueThatSetsError = value;
                    this.ErrorsByPropertyName[nameof(ValueThatSetsError)] = new[] { "some error" };
                }
            }

            public string ValueThatAlwaysRaisesErrorsChanged
            {
                get => valueThatAlwaysRaisesErrorsChanged;
                set
                {
                    valueThatAlwaysRaisesErrorsChanged = value;
                    this.Validate(valueThatAlwaysRaisesErrorsChanged);
                    this.RaiseErrorsChanged(nameof(ValueThatAlwaysRaisesErrorsChanged));
                }
            }

            public bool HasErrors { get; set; }

            public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

            private void Validate(string value, IEnumerable errorsWhenInvalid = null, [CallerMemberName] string propertyName = null)
            {
                var isValid = !value.Contains("invalid");

                if (!isValid)
                {
                    this.HasErrors = true;
                    var oldErrors = this.GetErrors(propertyName);
                    this.ErrorsByPropertyName[propertyName] = errorsWhenInvalid ?? new[] { $"error in {propertyName} with value {value}" };
                    if (!oldErrors.SequenceEqual(this.ErrorsByPropertyName[propertyName]))
                    {
                        this.RaiseErrorsChanged(propertyName);
                    }
                }
                else
                {
                    var hadOldErrors = this.GetErrors(propertyName).GetEnumerator().MoveNext();
                    this.ErrorsByPropertyName[propertyName] = Enumerable.Empty<string>();
                    this.HasErrors = this.ErrorsByPropertyName.Any(e => e.Value.GetEnumerator().MoveNext());
                    if (hadOldErrors)
                    {
                        this.RaiseErrorsChanged(propertyName);
                    }
                }
            }

            public IEnumerable GetErrors(string propertyName) =>
                this.ErrorsByPropertyName.TryGetValue(propertyName, out var errors)
                    ? errors
                    : Enumerable.Empty<object>();

            private void RaiseErrorsChanged(string propertyName)
            {
                this.ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }
    }
}
