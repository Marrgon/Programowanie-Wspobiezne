﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Input;
using TPW.Presentation.Model;

namespace TPW.Presentation.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private ModelAPI model;
        private string inputText;
        public ObservableCollection<IBall> Circles { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public int BallsCount
        {
            get { return model.GetBallsCount(); }
            set
            {
                if (value >= 0)
                {
                    model.SetBallNumber(value);
                    RaisePropertyChanged();
                }
            }
        }
        public string numberOfBalls
        {
            get
            {
                return inputText;
            }
            set
            {
                inputText = value;
                RaisePropertyChanged(nameof(numberOfBalls));
            }
        }
        public ICommand StartSimulationButton { get; }
        public ICommand StopSimulationButton { get; }
        public MainViewModel(ModelAPI baseModel)
        {
            this.model = baseModel;
            Circles = new ObservableCollection<IBall>();
            IDisposable observer = model.Subscribe(x => Circles.Add(x));
            BallsCount = 5;

            StartSimulationButton = new RelayCommand(() =>
            {
                model.SetBallNumber(readFromTextBox());
                model.StartSimulation();
            });

        }
        public int readFromTextBox()
        {
            int number;
            if (Int32.TryParse(numberOfBalls, out number) && numberOfBalls != "0")
            {
                number = Int32.Parse(numberOfBalls);

                if (number > 1001)
                {
                    return 1001;
                }
                return number;
            }
            return 5;
        }
        public MainViewModel() : this(ModelAPI.CreateApi())
        {

        }


        

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}


public class RelayCommand : ICommand
{
    private readonly Action handler;
    private bool isEnabled;

    public RelayCommand(Action handler)
    {
        this.handler = handler;
        IsEnabled = true;
    }

    public bool IsEnabled
    {
        get { return isEnabled; }
        set
        {
            if (value != isEnabled)
            {
                isEnabled = value;
                if (CanExecuteChanged != null)
                {
                    CanExecuteChanged(this, EventArgs.Empty);
                }
            }
        }
    }

    public bool CanExecute(object parameter)
    {
        return IsEnabled;
    }

    public event EventHandler CanExecuteChanged;

    public void Execute(object parameter)
    {
        handler();
    }
}

public class AsyncObservableCollection<T> : ObservableCollection<T>
{
    private SynchronizationContext _synchronizationContext = SynchronizationContext.Current;

    public AsyncObservableCollection()
    {
    }

    public AsyncObservableCollection(IEnumerable<T> list)
        : base(list)
    {
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (SynchronizationContext.Current == _synchronizationContext)
        {
            
            RaiseCollectionChanged(e);
        }
        else
        {
            
            _synchronizationContext.Send(RaiseCollectionChanged, e);
        }
    }

    private void RaiseCollectionChanged(object param)
    {
        
        base.OnCollectionChanged((NotifyCollectionChangedEventArgs)param);
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (SynchronizationContext.Current == _synchronizationContext)
        {
            
            RaisePropertyChanged(e);
        }
        else
        {
            
            _synchronizationContext.Send(RaisePropertyChanged, e);
        }
    }

    private void RaisePropertyChanged(object param)
    {
        
        base.OnPropertyChanged((PropertyChangedEventArgs)param);
    }
}