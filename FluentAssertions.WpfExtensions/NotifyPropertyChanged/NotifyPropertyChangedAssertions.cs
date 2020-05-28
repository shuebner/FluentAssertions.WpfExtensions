using FluentAssertions.Events;
using FluentAssertions.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using static FluentAssertions.PropertyInfoHelper;

namespace FluentAssertions
{
    public class NotifyPropertyChangedAssertions<T> : ReferenceTypeAssertions<T, NotifyPropertyChangedAssertions<T>>
        where T : INotifyPropertyChanged
    {
        public NotifyPropertyChangedAssertions(T bindable)
        {
            this.Subject = bindable;
        }

        public void ExposeNotifyPropertyChangedBehaviorFor<TProperty>(Expression<Func<T, TProperty>> propertyExpression, TProperty otherValue)
        {
            if (propertyExpression is null)
            {
                throw new ArgumentNullException(nameof(propertyExpression));
            }

            var propertyInfo = GetPropertyInfoOrThrow(propertyExpression, nameof(propertyExpression));
            var propertyName = propertyInfo.Name;

            if (object.Equals(propertyInfo.GetValue(this.Subject), otherValue))
            {
                throw new ArgumentException($"{nameof(otherValue)} must have a value different from the current value of property {propertyName}, but both where {otherValue}", nameof(otherValue));
            }

            using (var monitor = this.Subject.Monitor<INotifyPropertyChanged>())
            {
                propertyInfo.SetValue(this.Subject, otherValue);

                propertyInfo.GetValue(this.Subject).Should().Be(otherValue, "after setting a property with a value, the same value must be returned by the respective getter");

                var propertyChangedEvent = GetPropertyChangedEvents(monitor)
                    .Should().ContainSingle(e => ((PropertyChangedEventArgs)e.Parameters[1]).PropertyName == propertyName,
                        "exactly one PropertyChangedEvent for the changed property must be raised")
                    .Which.Parameters[0].Should().BeSameAs(this.Subject,
                        "the event source must be the instance to which the property belongs");

                monitor.Clear();
                
                propertyInfo.SetValue(this.Subject, otherValue);

                GetPropertyChangedEvents(monitor)
                    .Should().NotContain(e => ((PropertyChangedEventArgs)e.Parameters[1]).PropertyName == propertyName,
                        "PropertyChangedEvent must not be raised when setting the same value");
            }

            IEnumerable<OccurredEvent> GetPropertyChangedEvents(IMonitor<INotifyPropertyChanged> monitor) =>
                monitor.OccurredEvents
                    .Where(e => e.EventName == nameof(INotifyPropertyChanged.PropertyChanged))
                    .Where(e => e.Parameters[1] is PropertyChangedEventArgs);
        }

        protected override string Identifier => "INotifyPropertyChanged";
    }
}
