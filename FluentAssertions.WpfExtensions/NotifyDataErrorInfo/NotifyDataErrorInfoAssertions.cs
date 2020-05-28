using FluentAssertions.Events;
using FluentAssertions.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static FluentAssertions.PropertyInfoHelper;

namespace FluentAssertions
{
    public class NotifyDataErrorInfoAssertions<T> : ReferenceTypeAssertions<T, NotifyDataErrorInfoAssertions<T>>
        where T : INotifyDataErrorInfo
    {
        public NotifyDataErrorInfoAssertions(T bindable)
        {
            this.Subject = bindable;
        }

        public void ExposeNotifyDataErrorInfoBehaviorFor<TProperty>(Expression<Func<T, TProperty>> propertyExpression, NotifyDataErrorInfoSamples<TProperty> samples)
        {
            if (propertyExpression is null)
            {
                throw new ArgumentNullException(nameof(propertyExpression));
            }

            if (samples is null)
            {
                throw new ArgumentNullException(nameof(samples));
            }

            var propertyInfo = GetPropertyInfoOrThrow(propertyExpression, nameof(propertyExpression));
            var propertyName = propertyInfo.Name;

            if (this.Subject.HasErrors || this.Subject.GetErrors(propertyName).GetEnumerator().MoveNext())
            {
                throw new ArgumentException("the instance under test must be valid initially");
            }

            if (object.Equals(samples.ValidValue, propertyInfo.GetValue(this.Subject)))
            {
                throw new ArgumentException($"the valid sample value must not be equal to the current value on the instance under test:\n{samples.ValidValue}");
            }

            // set other valid value
            AssertAllAspects(propertyInfo, samples.ValidValue,
                events => events.Should().BeEmpty("setting a valid value that before was valid too, should not trigger an ErrorsChanged event"),
                false,
                e => e.Should().BeEmpty("there should be no errors for a valid property"));

            // set invalid value
            AssertAllAspects(propertyInfo, samples.InvalidSample1.InvalidValue,
                events => AssertExactlyOneEventForProperty(events, propertyInfo.Name),
                true,
                e =>
                {
                    e.Should().NotBeEmpty("there should be at least one error for an invalid property");
                    samples.InvalidSample1.AssertOnErrors?.Invoke(e);
                });

            // set same invalid value
            AssertAllAspects(propertyInfo, samples.InvalidSample1.InvalidValue,
                events => events.Should().BeEmpty("setting an invalid value again should not change the errors and thus does not warrant an event"),
                true,
                e =>
                {
                    e.Should().NotBeEmpty("there should be at least one error for an invalid property");
                    samples.InvalidSample1.AssertOnErrors?.Invoke(e);
                });

            // set other invalid value
            AssertAllAspects(propertyInfo, samples.InvalidSample2.InvalidValue,
                events => AssertExactlyOneEventForProperty(events, propertyInfo.Name),
                true,
                e =>
                {
                    e.Should().NotBeEmpty("there should be at least one error for an invalid property");
                    samples.InvalidSample2.AssertOnErrors?.Invoke(e);
                });

            // set valid value again
            AssertAllAspects(propertyInfo, samples.ValidValue,
                events => AssertExactlyOneEventForProperty(events, propertyInfo.Name),
                false,
                e => e.Should().BeEmpty("there should be no errors for a valid property"));
        }

        private void AssertExactlyOneEventForProperty(IEnumerable<OccurredEvent> events, string propertyName)
        {
            var @event = events.Where(e => ((DataErrorsChangedEventArgs)e.Parameters[1]).PropertyName == propertyName)
                .Should().ContainSingle("exactly one event must be raised for the property").Subject;
            @event.Parameters[0].Should().BeSameAs(this.Subject, "source of the event must be the given instance");
        }

        protected override string Identifier => "INotifyDataErrorInfo";

        private void AssertAllAspects<TProperty>(PropertyInfo propInfo, TProperty value, Action<IEnumerable<OccurredEvent>> assertOnErrorsChangedEvents, bool expectedHasErrors,
            Action<IEnumerable> assertOnErrors)
        {
            using (var monitor = this.Subject.Monitor<INotifyDataErrorInfo>())
            {
                propInfo.SetValue(this.Subject, value);

                propInfo.GetValue(this.Subject).Should().BeEquivalentTo(value, "property must be updated regardless of validity");

                var events = monitor.OccurredEvents
                    .Where(e => e.EventName == nameof(INotifyDataErrorInfo.ErrorsChanged));
                assertOnErrorsChangedEvents(events);

                this.Subject.HasErrors.Should().Be(expectedHasErrors);

                var errors = this.Subject.GetErrors(propInfo.Name);
                assertOnErrors(errors);
            }
        }
    }

    public class NotifyDataErrorInfoSamples<T>
    {
        public NotifyDataErrorInfoSamples(T validValue, InvalidSample<T> invalidSample1, InvalidSample<T> invalidSample2)
        {
            ValidValue = validValue;
            InvalidSample1 = invalidSample1;
            InvalidSample2 = invalidSample2;
        }

        public T ValidValue { get; }

        public InvalidSample<T> InvalidSample1 { get; }

        public InvalidSample<T> InvalidSample2 { get; }
    }

    public class InvalidSample<T>
    {
        public InvalidSample(T invalidValue)
        {
            InvalidValue = invalidValue;
        }

        public T InvalidValue { get; }

        public Action<IEnumerable> AssertOnErrors { get; set; }
    }
}
