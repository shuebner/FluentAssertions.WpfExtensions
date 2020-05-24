using System.ComponentModel;

namespace FluentAssertions
{
    public static class NotifyPropertyChangedExtensions
    {
        public static NotifyPropertyChangedAssertions<T> Should<T>(this T bindable)
            where T : INotifyPropertyChanged
        {
            return new NotifyPropertyChangedAssertions<T>(bindable);
        }
    }
}
