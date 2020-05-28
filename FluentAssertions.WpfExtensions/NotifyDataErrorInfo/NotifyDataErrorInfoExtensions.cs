using System.ComponentModel;

namespace FluentAssertions
{
    public static class NotifyDataErrorInfoExtensions
    {
        public static NotifyDataErrorInfoAssertions<T> Should<T>(this T instance)
            where T : INotifyDataErrorInfo
        {
            return new NotifyDataErrorInfoAssertions<T>(instance);
        }
    }
}
