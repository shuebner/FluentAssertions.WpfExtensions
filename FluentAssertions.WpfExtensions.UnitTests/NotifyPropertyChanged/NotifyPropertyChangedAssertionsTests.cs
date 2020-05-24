using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FluentAssertions
{
    [TestClass]
    public class NotifyPropertyChangedAssertionsTests
    {
        private TestBindable bindable;

        [TestInitialize]
        public void Initialize()
        {
            bindable = new TestBindable();
        }

        [TestMethod]
        public void ExposeNotifyPropertyChangedBehaviorFor_WhenExpressionIsNull_ShouldThrowArgumentNullException()
        {
            Action act = () => bindable.Should().ExposeNotifyPropertyChangedBehaviorFor(null, "foo");

            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void ExposeNotifyPropertyChangedBehaviorFor_WhenExpressionIsNotAPropertyExpression_ShouldThrowArgumentException()
        {
            Action act = () => bindable.Should().ExposeNotifyPropertyChangedBehaviorFor(b => b, bindable);

            act.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void ExposeNotifyPropertyChangedBehaviorFor_WhenOtherValueIsCurrentValue_ShouldThrowArgumentException()
        {
            bindable.CorrectlyImplementedValue = "foo";

            Action act = () => bindable.Should().ExposeNotifyPropertyChangedBehaviorFor(b => b.CorrectlyImplementedValue, "foo");

            act.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void ExposeNotifyPropertyChangedBehaviorFor_WhenDifferentValueIsSet_And_BindableDoesNotUpdateTheValue_FailsAssertion()
        {
            Action act = () => bindable.Should().ExposeNotifyPropertyChangedBehaviorFor(b => b.ValueThatDoesNotUpdate, "foo");

            act.Should().Throw<AssertFailedException>();
        }

        [TestMethod]
        public void ExposeNotifyPropertyChangedBehaviorFor_WhenDifferentValueIsSet_WhenBindableDoesNotRaiseThePropertyChangedEvent_FailsAssertion()
        {
            Action act = () => bindable.Should().ExposeNotifyPropertyChangedBehaviorFor(b => b.ValueThatDoesNotRaise, "foo");

            act.Should().Throw<AssertFailedException>();
        }

        [TestMethod]
        public void ExposeNotifyPropertyChangedBehaviorFor_WhenDifferentValueIsSet_WhenBindableRaisesThePropertyChangedEventWithTheWrongSource_FailsAssertion()
        {
            Action act = () => bindable.Should().ExposeNotifyPropertyChangedBehaviorFor(b => b.ValueThatRaisesWithWrongSource, "foo");

            act.Should().Throw<AssertFailedException>();
        }

        [TestMethod]
        public void ExposeNotifyPropertyChangedBehaviorFor_WhenDifferentValueIsSet_WhenBindableRaisesThePropertyChangedEventMoreThanOnce_FailsAssertion()
        {
            Action act = () => bindable.Should().ExposeNotifyPropertyChangedBehaviorFor(b => b.ValueThatRaisesMoreThanOnce, "foo");

            act.Should().Throw<AssertFailedException>();
        }

        [TestMethod]
        public void ExposeNotifyPropertyChangedBehaviorFor_WhenDifferentValueIsSet_WhenBindableRaisesTheCorrectPropertyChangedEventExactlyOnce_PassesAssertion()
        {
            Action act = () => bindable.Should().ExposeNotifyPropertyChangedBehaviorFor(b => b.CorrectlyImplementedValue, "foo");

            act.Should().NotThrow();
        }

        [TestMethod]
        public void ExposeNotifyPropertyChangedBehaviorFor_WhenSameValueIsSet_WhenBindableRaisesAnyPropertyChangedEvent_FailsAssertion()
        {
            Action act = () => bindable.Should().ExposeNotifyPropertyChangedBehaviorFor(b => b.ValueThatAlwaysRaises, "foo");

            act.Should().Throw<AssertFailedException>();
        }

        [TestMethod]
        public void ExposeNotifyPropertyChangedBehaviorFor_WhenSameValueIsSet_WhenBindableDoesNotRaiseAnyPropertyChangedEvent_PassesAssertion()
        {
            Action act = () => bindable.Should().ExposeNotifyPropertyChangedBehaviorFor(b => b.CorrectlyImplementedValue, "foo");

            act.Should().NotThrow();
        }

        private class TestBindable : INotifyPropertyChanged
        {
            private string value;
            private string valueThatDoesNotUpdate;
            private string valueThatDoesNotRaise;
            private string valueThatRaisesMoreThanOnce;
            private string valueThatRaisesWithWrongSource;
            private string valueThatAlwaysRaises;

            public string CorrectlyImplementedValue
            {
                get => this.value;
                set
                {
                    if (!object.Equals(this.value, value))
                    {
                        this.value = value;
                        this.RaisePropertyChanged();
                    }
                }
            }

            public string ValueThatDoesNotUpdate
            {
                get => this.valueThatDoesNotUpdate;
                set
                {
                    this.RaisePropertyChanged();
                }
            }

            public string ValueThatDoesNotRaise
            {
                get => this.valueThatDoesNotRaise;
                set
                {
                    this.valueThatDoesNotRaise = value;
                }
            }

            public string ValueThatRaisesWithWrongSource
            {
                get => this.valueThatRaisesWithWrongSource;
                set
                {
                    if (!object.Equals(this.valueThatDoesNotRaise, value))
                    {
                        this.valueThatRaisesWithWrongSource = value;
                        this.PropertyChanged?.Invoke(new object(), new PropertyChangedEventArgs(nameof(this.ValueThatRaisesWithWrongSource)));
                    }
                }
            }

            public string ValueThatRaisesMoreThanOnce
            {
                get => this.valueThatRaisesMoreThanOnce;
                set
                {
                    if (!object.Equals(this.valueThatRaisesMoreThanOnce, value))
                    {
                        this.valueThatRaisesMoreThanOnce = value;
                        this.RaisePropertyChanged();
                        this.RaisePropertyChanged();
                    }
                }
            }

            public string ValueThatAlwaysRaises
            {
                get => this.valueThatAlwaysRaises;
                set
                {
                    this.valueThatAlwaysRaises = value;
                    this.RaisePropertyChanged();
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            private void RaisePropertyChanged([CallerMemberName] string propertyName = null) =>
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
