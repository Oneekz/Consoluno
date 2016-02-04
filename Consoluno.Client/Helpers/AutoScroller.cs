using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using GalaSoft.MvvmLight.Threading;

namespace Consoluno.Client.Helpers
{
    public class AutoScroller : Behavior<ScrollViewer>
{
    public object AutoScrollTrigger 
    {
        get { return (object)GetValue( AutoScrollTriggerProperty ); }
        set { SetValue( AutoScrollTriggerProperty, value ); }
    }

    public static readonly DependencyProperty AutoScrollTriggerProperty = 
        DependencyProperty.Register(  
            "AutoScrollTrigger", 
            typeof( object ), 
            typeof( AutoScroller ), 
            new PropertyMetadata( null, ASTPropertyChanged ) );

    private static void ASTPropertyChanged( DependencyObject d, DependencyPropertyChangedEventArgs args )
    {

        var ts = d as AutoScroller;            
        if( ts == null )
            return;

        // must be attached to a ScrollViewer
        var sv = ts.AssociatedObject as ScrollViewer;

        // check if we are attached to an ObservableCollection, in which case we
        // will subscribe to CollectionChanged so that we scroll when stuff is added/removed
        var ncol = args.NewValue as INotifyCollectionChanged;
        // new event handler
        if( ncol != null )
            ncol.CollectionChanged += ts.ncol_CollectionChanged;

        // remove old eventhandler
        var ocol = args.OldValue as INotifyCollectionChanged;
        if( ocol != null )
            ocol.CollectionChanged -= ts.ncol_CollectionChanged;


        // also scroll to bottom when the bound object itself changes
        if( sv != null )
            sv.ScrollToBottom();
    }

    void ncol_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        DispatcherHelper.CheckBeginInvokeOnUI(() =>
        {
            (this.AssociatedObject as ScrollViewer).ScrollToBottom();
        });
    }

}
}
