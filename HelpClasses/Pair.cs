
using System.ComponentModel;
using System.Runtime.CompilerServices;
using HelpClasses.Annotations;


namespace HelpClasses
{
    public class Pair<T,TE> : INotifyPropertyChanged
    {
        private T _key = default(T);
        public T Key
        {
            get { return _key; }
            set
            {
                
                    _key = value;
                    OnPropertyChanged();
                
            }
        }


        private TE _value = default(TE);
        public TE Value
        {
            get { return _value; }
            set
            {
                
                    this._value = value;
                    OnPropertyChanged();
                
            }
        }

        public Pair() { }
        public Pair(T key, TE value)
            : this()
        {
            Key = key;
            Value = value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
