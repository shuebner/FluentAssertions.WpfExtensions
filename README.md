# FluentAssertionsWpfExtensions
WPF-specific extensions for FluentAssertions

Makes testing the correct implementation of INotifyPropertyChanged (trivial) and INotifyDataErrorInfo (not so trivial) easier.

For testing INotifyPropertyChanged behavior you can just do:
```
viewModel.Should().ExposeNotifyPropertyChangedBehaviorFor(vm => vm.SomeProperty, "some_different_value_to_set");
```

For testing INotifyDataErrorInfo behavior you can just do:
```
instance.Should().ExposeNotifyDataErrorInfoBehaviorFor(
    i => i.SomeValidatedProperty,
    new NotifyDataErrorInfoSamples<string>(
      "valid",
      new InvalidSample<string>("invalid1"),
      new InvalidSample<string>("invalid2")));
```
or for additional assertions on the errors
```
instance.Should().ExposeNotifyDataErrorInfoBehaviorFor(
    i => i.SomeValidatedProperty,
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
```

For more information check out my blog post for testing INotifyPropertyChanged at https://svenhuebner-it.com/simplifying-unit-tests-for-wpf-viewmodels/
