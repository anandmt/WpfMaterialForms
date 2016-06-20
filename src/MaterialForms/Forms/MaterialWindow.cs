﻿using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using MaterialDesignThemes.Wpf;
using MaterialForms.Annotations;

namespace MaterialForms
{
    public class MaterialWindow : INotifyPropertyChanged
    {
        private static int staticDialogId;
        private static DispatcherOption defaultDispatcher;
        private static Dispatcher customDispatcher;

        static MaterialWindow()
        {
            defaultDispatcher = DispatcherOption.CurrentThread;
            var materialFormsApplication = Application.Current ??
                                           new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            LoadResources(materialFormsApplication);
        }

        public static void LoadResources(Application application)
        {
            application.Resources.MergedDictionaries.Add(
                Application.LoadComponent(
                    new Uri("MaterialForms;component/Resources/Material.xaml",
                    UriKind.Relative)) as ResourceDictionary);
        }

        public static void ShutDownCustomDispatcher()
        {
            if (customDispatcher == null)
            {
                return;
            }

            customDispatcher.InvokeShutdown();
            customDispatcher = null;
        }

        public static void ShutDownApplication()
        {
            Application.Current.Shutdown();
        }

        public static void SetDefaultDispatcher(DispatcherOption dispatcherOption)
        {
            defaultDispatcher = dispatcherOption;
        }

        public bool CheckDispatcherAccess(DispatcherOption dispatcherOption)
        {
            var dispatcher = GetDispatcher(dispatcherOption);
            return dispatcher.CheckAccess();
        }

        private static Dispatcher GetDispatcher(DispatcherOption dispatcherOption)
        {
            Dispatcher dispatcher;
            switch (dispatcherOption)
            {
                case DispatcherOption.CurrentThread:
                    dispatcher = Dispatcher.CurrentDispatcher;
                    break;
                case DispatcherOption.CurrentApplication:
                    dispatcher = Application.Current.Dispatcher;
                    break;
                case DispatcherOption.Custom:
                    dispatcher = GetCustomDispatcher();
                    break;
                default:
                    throw new InvalidOperationException();
            }
            return dispatcher;
        }

        private static Dispatcher GetCustomDispatcher()
        {
            if (customDispatcher != null)
            {
                return customDispatcher;
            }

            var waitHandle = new ManualResetEventSlim();
            var thread = new Thread(() =>
            {
                customDispatcher = Dispatcher.CurrentDispatcher;
                waitHandle.Set();
                Dispatcher.Run();
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            waitHandle.Wait();
            return customDispatcher;
        }

        private int currentDialogId;

        private string title = "Dialog";
        private double width = 400d;
        private double height = double.NaN;
        private bool showMinButton;
        private bool showMaxRestoreButton = true;
        private bool showCloseButton;
        private bool canResize;
        private MaterialDialog dialog;

        public MaterialWindow()
        {
        }

        public MaterialWindow(MaterialDialog dialog)
        {
            Dialog = dialog;
        }

        public string Title
        {
            get { return title; }
            set
            {
                if (value == title) return;
                title = value;
                OnPropertyChanged();
            }
        }

        public double Width
        {
            get { return width; }
            set
            {
                if (value.Equals(width)) return;
                width = value;
                OnPropertyChanged();
            }
        }

        public double Height
        {
            get { return height; }
            set
            {
                if (value.Equals(height)) return;
                height = value;
                OnPropertyChanged();
            }
        }

        public bool ShowMinButton
        {
            get { return showMinButton; }
            set
            {
                if (value == showMinButton) return;
                showMinButton = value;
                OnPropertyChanged();
            }
        }

        public bool ShowMaxRestoreButton
        {
            get { return showMaxRestoreButton; }
            set
            {
                if (value == showMaxRestoreButton) return;
                showMaxRestoreButton = value;
                OnPropertyChanged();
            }
        }

        public bool ShowCloseButton
        {
            get { return showCloseButton; }
            set
            {
                if (value == showCloseButton) return;
                showCloseButton = value;
                OnPropertyChanged();
            }
        }

        public bool CanResize
        {
            get { return canResize; }
            set
            {
                if (value == canResize) return;
                canResize = value;
                OnPropertyChanged();
            }
        }

        public MaterialDialog Dialog
        {
            get { return dialog; }
            set
            {
                if (Equals(value, dialog)) return;
                dialog = value;
                OnPropertyChanged();
            }
        }

        public bool? ShowSync()
        {
            currentDialogId = Interlocked.Increment(ref staticDialogId);
            var window = new MaterialFormsWindow(this, currentDialogId);
            return window.ShowDialog();
        }

        public Task<bool?> Show() => Show(defaultDispatcher);

        public Task<bool?> Show(DispatcherOption dispatcherOption)
        {
            var dispatcher = GetDispatcher(dispatcherOption);
            return Show(dispatcher);
        }

        private Task<bool?> Show(Dispatcher dispatcher)
        {
            if (dispatcher.CheckAccess())
            {
                return Task.FromResult(ShowSync());
            }

            var completion = new TaskCompletionSource<bool?>();
            dispatcher.InvokeAsync(() =>
            {
                try
                {
                    completion.SetResult(ShowSync());
                }
                catch (Exception ex)
                {
                    completion.SetException(ex);
                }
            });

            return completion.Task;
        }

        public async Task ShowDialog(MaterialDialog modalDialog, double dialogWidth = double.NaN)
        {
            var view = modalDialog.View;
            view.Width = dialogWidth;
            await DialogHost.Show(view, "DialogHost" + currentDialogId);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}