using System;
using System.ComponentModel;
using CyanStars.ChartEditor.Model;
using CyanStars.ChartEditor.View;

namespace CyanStars.Assets.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private MainModel mainModel;

        public MainViewModel(MainModel mainModel)
        {
            this.mainModel = mainModel;
        }


        public void ChangeEditTool(EditTools editTool)
        {
            // TODO
            throw new NotSupportedException();
        }

        // protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        // {
        //     PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        // }
        //
        // protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        // {
        //     if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        //     field = value;
        //     OnPropertyChanged(propertyName);
        //     return true;
        // }
    }
}
