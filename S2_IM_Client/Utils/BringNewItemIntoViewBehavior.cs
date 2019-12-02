using System.Collections.Specialized;
using System.Windows.Interactivity;
using System.Windows.Controls;
using System.Windows;

namespace S2_IM_Client.Utils
{
    public class BringNewItemIntoViewBehavior : Behavior<ItemsControl>
    {
        private INotifyCollectionChanged _notifier;

        protected override void OnAttached()
        {
            base.OnAttached();
            _notifier = AssociatedObject.Items;
            _notifier.CollectionChanged += ItemsControl_CollectionChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            _notifier.CollectionChanged -= ItemsControl_CollectionChanged;
        }

        private void ItemsControl_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Add)
            {
                return;
            }

            var newIndex = e.NewStartingIndex;
            var newElement = AssociatedObject.ItemContainerGenerator.ContainerFromIndex(newIndex);
            var item = (FrameworkElement)newElement;
            item?.BringIntoView();
        }
    }
}