using FollowerParser.MVVM.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace FollowerParser.MVVM.ViewModel
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        private InstagramBot _instagramBot;
        private ObservableCollection<Follower> _followerData;


        public ObservableCollection<Follower> FollowerData
        {
            get { return _followerData; }
            set
            {
                _followerData = value;
                OnPropertyChanged(nameof(FollowerData));
            }
        }

        private string _username;
        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        private string _targetUsername;
        public string TargetUsername
        {
            get { return _targetUsername; }
            set
            {
                _targetUsername = value;
                OnPropertyChanged(nameof(TargetUsername));
            }
        }

        private int _timeoutMin;
        public int TimeoutMin
        {
            get { return _timeoutMin; }
            set
            {
                _timeoutMin = value;
                OnPropertyChanged(nameof(TimeoutMin));
            }
        }

        private int _timeoutMax;
        public int TimeoutMax
        {
            get { return _timeoutMax; }
            set
            {
                _timeoutMax = value;
                OnPropertyChanged(nameof(TimeoutMax));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public MainViewModel()
        {
            FollowerData = new ObservableCollection<Follower>();
            _instagramBot = new InstagramBot();
        }

        public void Parse()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(TargetUsername) || TimeoutMin <= 0 || TimeoutMax <= 0)
            {
                MessageBox.Show("Please fill in all fields and ensure timeout is a positive integer.");
                return;
            }
            if ( TimeoutMin > TimeoutMax)
            {
                MessageBox.Show("Timeout minimum can`t be greater than timeout maximum.");
                return;
            }
            if (TimeoutMin < 1500)
            {
                MessageBox.Show("Timeout minimum can`t be less than 1500.");
                return;
            }

            FollowerData.Clear(); // Clear previous data
            _instagramBot.SetTimeoutRange(TimeoutMin, TimeoutMax);
            _instagramBot.Login(Username, Password);

            Thread parsingThread = new Thread(() =>
            {
                List<Follower> followers = _instagramBot.FollowingUser(TargetUsername);
                if (followers.Count() == 0)
                {
                    MessageBox.Show("This profile is private or doesn`t have followers");
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        foreach (var follower in followers)
                        {
                            FollowerData.Add(follower);
                        }
                    });
                }
            });

            parsingThread.Start();
        }

        public void Closing()
        {
            _instagramBot.Quitting();
        }
    }
}