using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using EcoProIT.DataLayer;
using EcoProIT.UserControles.Annotations;
using EcoProIT.UserControles;
using EcoProIT.UserControles.View;
using Xceed.Wpf.Toolkit;

namespace EcoProIT.UserControles.View
{
    /// <summary>
    ///     Interaction logic for DistributionView.xaml
    /// </summary>
    public partial class DistributionView : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty HasDistributionProperty =
            DependencyProperty.Register("HasDistribution", typeof (IHasDistribution), typeof (DistributionView),
                                        new PropertyMetadata(new SubJob(), PropertyChangedCallback));

        private readonly ObservableCollection<UIElement> _distributionControles = new ObservableCollection<UIElement>();
        private ObservableCollection<IDistribution> _distributionAvailible = new ObservableCollection<IDistribution>();

        /// <exception cref="System.ArgumentNullException">typeName is null. </exception>
        /// <exception cref="System.Reflection.TargetInvocationException">A class initializer is invoked and throws an exception. </exception>
        /// <exception cref="System.ArgumentException">typeName represents a generic type that has a pointer type, a ByRef type, or <see cref="T:System.Void" /> as one of its type arguments.-or-typeName represents a generic type that has an incorrect number of type arguments.-or-typeName represents a generic type, and one of its type arguments does not satisfy the constraints for the corresponding type parameter.</exception>
        /// <exception cref="System.TypeLoadException">typeName represents an array of <see cref="T:System.TypedReference" />. </exception>
        /// <exception cref="System.IO.FileLoadException">The assembly or one of its dependencies was found, but could not be loaded. </exception>
        /// <exception cref="System.BadImageFormatException">The assembly or one of its dependencies is not valid. -or-Version 2.0 or later of the common language runtime is currently loaded, and the assembly was compiled with a later version.</exception>
        public DistributionView()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this))
                return;
            Type type = typeof (IDistribution);
            IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies()
                                               .SelectMany(s => s.GetTypes())
                                               .Where(p => type.IsAssignableFrom(p) && p.IsClass && !p.IsAbstract);

            foreach (Type type1 in types)
            {
                try
                {
                    if (type1.AssemblyQualifiedName != null)
                        try
                        {
                            _distributionAvailible.Add(
// ReSharper disable AssignNullToNotNullAttribute
                                (IDistribution) Activator.CreateInstance(Type.GetType(type1.AssemblyQualifiedName)));
// ReSharper restore AssignNullToNotNullAttribute
                        }
                        catch
                        {
                        }
                }
                catch
                {
                }
            }
        }

        [DataLayer.Annotations.NotNull]
        public IDistribution Distribution
        {
            get { return HasDistribution.Distribution; }
            set
            {
                if(value == null)
                    return;
                HasDistribution.Distribution = value;
                CheckRightObject();
                OnPropertyChanged();
                SetNewPropertyControles();
            }
        }

        [DataLayer.Annotations.NotNull]
        public IHasDistribution HasDistribution
        {
            get { return (IHasDistribution) GetValue(HasDistributionProperty); }
            set { SetValue(HasDistributionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SetDistribution.  This enables animation, styling, binding, etc...

        [DataLayer.Annotations.NotNull]
        public ObservableCollection<IDistribution> DistributionAvailible
        {
            get { return _distributionAvailible; }
            private set { _distributionAvailible = value; }
        }

        [DataLayer.Annotations.NotNull]
        public ObservableCollection<UIElement> DistributionProperties
        {
            get { return _distributionControles; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void CheckRightObject()
        {
            IDistribution dist = _distributionAvailible.First(f => f.Name == HasDistribution.Distribution.Name);
            if (dist == HasDistribution.Distribution)
                return;
            _distributionAvailible.Remove(dist);
            _distributionAvailible.Add(HasDistribution.Distribution);
        }

        private static void PropertyChangedCallback(DependencyObject dependencyObject,
                                                    DependencyPropertyChangedEventArgs
                                                        dependencyPropertyChangedEventArgs)
        {
            var view = dependencyObject as DistributionView;
            var hasdist = (dependencyPropertyChangedEventArgs.NewValue as IHasDistribution);

            if (hasdist != null && hasdist.Distribution != null && view != null)
            {
                view.Distribution = hasdist.Distribution;
            }
        }

        private void SetNewPropertyControles()
        {
            _distributionControles.Clear();
            foreach (string parameterName in Distribution.ParameterNames)
            {
                _distributionControles.Add(new Label {Content = parameterName});
                var bind = new Binding(parameterName)
                    {
                        Source = Distribution,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    };
                var control = new DecimalUpDown {DefaultValue = (decimal)0.0, MinWidth = 60, Increment = (decimal)0.5, Minimum = 0};
                control.SetBinding(DecimalUpDown.ValueProperty, bind);
                _distributionControles.Add(control);
            }
        }

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}